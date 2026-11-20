# Config Loading 示例

## 导入与启动

1. 在 Package Manager 的 Config Loader 页面导入 **Config Loading**。
2. 确保已安装 Unity 内置 **IMGUI** 模块（`com.unity.modules.imgui`，用于示例的运行时操作面板）。无需 Inspector Groups。
3. 新建场景，在空物体上添加 `ConfigLoadingSample`，进入 Play Mode。无需手动绑定 JSON；脚本默认读取随附 Resources 资源。

启动后会显示 `Training room` 和三个项目。也可将同结构的 JSON TextAsset 拖入 `Example Json` 字段。

## 操作与预期结果

- **Parse bundled TextAsset**：通过 `TextLoader.ReadText` 接收原文，再用 `JsonConfigLoader<LevelConfig>.ReadJson` 解析。
- **Load temporary file asynchronously**：将 JSON 写入本实例独占的临时文件，通过 `ReadJsonFileAsync` 读取；回调后显示标题和三个项目。
- **Reload file**：演示 `ReloadJsonFileAsync`。
- **Stop current load**：演示 `StopLoading`，取消时不会把旧数据误当新结果。小文件读取很快，点击时请求可能已完成。

状态同时输出到 Console。示例只使用本包和 Unity 官方模块；不要求网络、场景资源或其他 Yvsim 包。

## 文件与平台边界

示例使用 `Application.temporaryCachePath/YvsimConfigSample_<唯一标识>.json`，销毁组件时仅清理该实例创建的文件，不修改 StreamingAssets 或用户配置。异常中断可能留下可手动清理的临时文件。

本地异步读取示例适用于 Editor/桌面。WebGL 和移动端的文件系统、权限及远程 CORS 需按目标平台另行验证；JSON 字符串解析不依赖磁盘。

`level.json` 和嵌套数据类型展示 JsonUtility 的字段映射：根节点为对象，使用字段而非属性，不使用 Dictionary。
