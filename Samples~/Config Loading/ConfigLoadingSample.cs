using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Yvsim.ConfigLoader.Samples
{
    /// <summary>演示文本、JSON 字符串和本地文件的异步读取；仅创建本实例拥有的临时文件。</summary>
    public sealed class ConfigLoadingSample : MonoBehaviour
    {
        [Tooltip("示例 JSON 文本。未指定时读取示例随附的 Resources/YvsimConfigLoading/level.json。")]
        [SerializeField] private TextAsset exampleJson;

        private readonly JsonConfigLoader<LevelConfig> loader = new JsonConfigLoader<LevelConfig>();
        private readonly TextLoader textLoader = new TextLoader();
        private string temporaryFile;
        private string status = "Ready";
        private bool closing;

        public bool LastLoadSucceeded { get; private set; }
        public int LoadedItemCount => loader.Config?.items?.Length ?? 0;
        public string Status => status;

        private void Start()
        {
            if (exampleJson == null) exampleJson = Resources.Load<TextAsset>("YvsimConfigLoading/level");
            ParseText();
        }

        /// <summary>把相同原文分别交给 TextLoader 和泛型 JSON 加载器。</summary>
        public void ParseText()
        {
            if (!TryGetText(out string json)) return;
            LastLoadSucceeded = textLoader.ReadText(json) && loader.ReadJson(textLoader.Content);
            status = LastLoadSucceeded ? $"Parsed {LoadedItemCount} items from TextAsset." : "Parse failed; see Console.";
            Debug.Log("Config Loading: " + status, this);
        }

        /// <summary>写入独占的临时文件，再经包的异步文件接口读取，展示完成回调。</summary>
        public void LoadFile()
        {
            if (!TryGetText(out string json)) return;
            try
            {
                if (temporaryFile == null)
                    temporaryFile = Path.Combine(Application.temporaryCachePath, "YvsimConfigSample_" + Guid.NewGuid().ToString("N") + ".json");
                File.WriteAllText(temporaryFile, json, new UTF8Encoding(false));
                LastLoadSucceeded = false;
                status = "Loading temporary JSON file...";
                loader.ReadJsonFileAsync(temporaryFile, Complete);
            }
            catch (Exception exception)
            {
                LastLoadSucceeded = false;
                status = "Temporary file failed: " + exception.Message;
                Debug.LogException(exception, this);
            }
        }

        public void ReloadFile()
        {
            if (temporaryFile == null) { LoadFile(); return; }
            LastLoadSucceeded = false;
            status = "Reloading temporary JSON file...";
            loader.ReloadJsonFileAsync(temporaryFile, Complete);
        }

        private void Complete(bool success)
        {
            if (closing) return;
            LastLoadSucceeded = success;
            status = success ? $"File loaded: {loader.Config.title}, {LoadedItemCount} items." : "Load stopped or failed; previous successful data is retained.";
            Debug.Log("Config Loading: " + status, this);
        }

        private bool TryGetText(out string json)
        {
            json = exampleJson == null ? null : exampleJson.text;
            if (json != null) return true;
            LastLoadSucceeded = false;
            status = "Missing level.json. Import the complete Config Loading sample.";
            Debug.LogError(status, this);
            return false;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, Mathf.Min(580, Screen.width - 32), Screen.height - 32), GUI.skin.box);
            GUILayout.Label("Config Loader - text and asynchronous JSON loading");
            GUILayout.Label(status);
            if (GUILayout.Button("Parse bundled TextAsset")) ParseText();
            if (GUILayout.Button("Load temporary file asynchronously")) LoadFile();
            if (GUILayout.Button("Reload file")) ReloadFile();
            if (GUILayout.Button("Stop current load")) loader.StopLoading();
            if (loader.Config != null)
            {
                GUILayout.Label("Title: " + loader.Config.title);
                if (loader.Config.items != null)
                    foreach (LevelItem item in loader.Config.items)
                        if (item != null) GUILayout.Label($"#{item.id}: {item.name}");
            }
            GUILayout.EndArea();
        }

        private void OnDisable() { loader.StopLoading(); }

        private void OnDestroy()
        {
            closing = true;
            loader.StopLoading();
            if (temporaryFile == null) return;
            try { if (File.Exists(temporaryFile)) File.Delete(temporaryFile); }
            catch (Exception exception) { Debug.LogWarning("Could not clean the sample temporary file: " + exception.Message, this); }
        }

        [Serializable]
        public sealed class LevelConfig
        {
            [Tooltip("示例配置标题，与 JSON 中的 title 字段对应。")]
            public string title;
            [Tooltip("示例项目数组，演示嵌套数据的反序列化。")]
            public LevelItem[] items;
        }

        [Serializable]
        public sealed class LevelItem
        {
            [Tooltip("示例项目的整数编号。")]
            public int id;
            [Tooltip("示例项目名称。")]
            public string name;
        }
    }
}
