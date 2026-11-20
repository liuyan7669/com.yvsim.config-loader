# Config Loader

适用于 Unity 的轻量配置数据工具，提供通用文本读取、常用配置的数据结构、JSON 解析、异步文件加载和查询接口。

## 功能

- `DialogueConfig`：管理分组对话、角色、文本、音频和动画配置。
- `QuestionConfig`：管理题目、选项、答案、得分和答案比较。
- `TipConfig`：管理提示文本及对应的音频资源路径。
- `TextLoader`：独立读取任意文本文件或 URL，不限制文件扩展名。
- `JsonConfigLoader<T>`：读取并解析由调用方自行定义结构的 JSON 配置。
- 全部配置加载器内部组合并复用 `TextLoader`，统一管理读取、重新读取和停止读取。
- 提供 Unity 编辑器菜单，一键生成配置示例文件。

## 安装

当前源码版本为 `1.0.0`。准备本包源码，使用本地 UPM 安装：

1. 在 `Window > Package Manager > + > Add package from disk...` 选择本包根目录的 `package.json`。
2. 等待 Unity 解析清单中声明的 Unity 依赖并完成编译。

也可以将完整包目录放入项目的 `Packages/com.yvsim.config-loader`，作为 Embedded Package 使用。

本说明不代表 Git Tag 或 OpenUPM 的 `1.0.0` 已发布；发布完成前请使用上述本地源码安装方式。

## 创建示例配置

安装后可通过 Unity 菜单 `Yvsim > 配置 > 模板` 创建以下文件：

- `game_config.json`
- `dialogues.json`
- `questions.json`
- `tips.json`

示例文件生成在 `Assets/StreamingAssets` 中。若同名文件已经存在，不会覆盖原文件。
`game_config.json` 与下方的 `GameConfig` 示例结构一致，可直接使用 `JsonConfigLoader<GameConfig>` 加载。

## 使用示例

### 单独读取任意文本

`TextLoader` 只负责读取原始字符串，不关心 `.txt`、`.json`、`.csv`、`.xml` 或自定义扩展名：

```csharp
using Yvsim.ConfigLoader;
using UnityEngine;

TextLoader loader = new TextLoader();
loader.LoadAsync(filePath, success =>
{
    if (success)
    {
        string content = loader.Content;
        Debug.Log(content);
    }
});
```

也可以直接保存调用方已有的原始文本：

```csharp
loader.ReadText("任意文本内容");
```

### 加载自定义 JSON 配置

配置类型和 JSON 字段由调用方自行定义：

```csharp
using System;
using System.Collections.Generic;

[Serializable]
public class GameConfig
{
    public string title;
    public List<GameConfigItem> items;
}

[Serializable]
public class GameConfigItem
{
    public int id;
    public string name;
}
```

通过泛型加载器读取配置文件：

```csharp
using System.IO;
using Yvsim.ConfigLoader;
using UnityEngine;

JsonConfigLoader<GameConfig> loader = new JsonConfigLoader<GameConfig>();
string path = Path.Combine(Application.streamingAssetsPath, "game_config.json");

loader.ReadJsonFileAsync(path, success =>
{
    if (success)
    {
        GameConfig config = loader.Config;
        Debug.Log(config.title);
    }
});
```

### 解析 JSON 字符串

```csharp
using Yvsim.ConfigLoader;

DialogueConfig config = new DialogueConfig();
if (config.ReadJson(jsonContent))
{
    var dialogues = config.GetDialoguesByTableName("Opening");
}
```

### 从 StreamingAssets 异步加载

```csharp
using System.IO;
using Yvsim.ConfigLoader;
using UnityEngine;

QuestionConfig config = new QuestionConfig();
string path = Path.Combine(Application.streamingAssetsPath, "questions.json");

config.ReadJsonFileAsync(path, success =>
{
    if (success && config.TryGetQuestionById("Q001", out QuestionData question))
    {
        Debug.Log(question.question);
    }
});
```

需要中止当前异步加载时，可调用 `StopLoading()`；需要停止当前加载并重新读取时，可调用配置加载器的 `ReloadJsonFileAsync()`，或文本加载器的 `ReloadAsync()`。

## 主要接口

- `DialogueConfig`：`GetTableByName`、`TryGetTableByName`、`GetDialoguesByTableName`
- `QuestionConfig`：`TryGetQuestionById`、`SetCurrentScoreById`、`SetSelectAnswerById`、`IsAnswerCorrectById`
- `TipConfig`：`GetItemById`、`TryGetItemById`、`GetTipTextById`、`GetTipAudioPathById`
- `TextLoader`：`ReadText`、`LoadAsync`、`ReloadAsync`、`StopLoading`
- `JsonConfigLoader<T>`：`ReadJson`、`ReadJsonFileAsync`、`ReloadJsonFileAsync`、`StopLoading`

## 注意事项

- 配置类使用 `JsonUtility` 解析 JSON，配置类型应标记 `[Serializable]`，字段名必须与 JSON 一致。
- `TextLoader` 只返回原始字符串；需要转换为对象时使用 `JsonConfigLoader<T>` 或在调用方自行解析。
- 对话、题目和提示配置使用对象根节点，不支持直接以 JSON 数组作为根节点。
- `JsonConfigLoader<T>` 同样要求使用对象根节点，不直接支持属性、`Dictionary` 或根数组。
- `audioPath` 保存的是 `Resources` 相对路径，不包含 `Resources` 目录名和文件扩展名。
- 异步入口接收绝对本地路径或 URL；WebGL 远程地址需允许 CORS，Android StreamingAssets 可直接使用 Unity 提供的 `jar:file` 地址。
- 新加载会取消同实例的上一请求且不回调上一请求；显式 `StopLoading()` 以失败结果结束当前请求。加载失败时，先前成功数据仍然保留，请只在回调成功后使用新结果。

## 环境要求

- Unity 2021.3 或更高版本

## 许可证

本包使用 MIT License，详情参见仓库中的许可证文件。

## 依赖说明

本包不依赖 Inspector Groups 或其他 Yvsim 包。

## 可导入示例

在 Package Manager 的 Samples 中导入 **Config Loading**。演示 TextAsset 文本、泛型 JSON 解析、本地文件异步加载与重新加载。

在新场景的空物体上添加 `ConfigLoadingSample` 后进入 Play Mode。示例操作面板需要 Unity 内置 IMGUI 模块，不需要 Groups；具体步骤和预期结果见 [示例说明](Samples~/Config%20Loading/README.md)。

## 从 GitHub 安装

仓库：[com.yvsim.config-loader](https://github.com/liuyan7669/com.yvsim.config-loader)。仓库名与 UPM 包名一致。

在 Unity Package Manager 选择 Add package from git URL，输入：

```text
https://github.com/liuyan7669/com.yvsim.config-loader.git#main
```

此地址指向 main 分支，当前包版本为 1.0.0。本次上传不创建版本 tag 或 GitHub Release，也不代表已在 OpenUPM 登记；Unity 官方依赖仍按 package.json 解析。
