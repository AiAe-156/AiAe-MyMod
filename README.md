# 缺氧 Mod 开发库（MyMod）

《缺氧 / Oxygen Not Included》本地 mod 开发的 monorepo，附反编译与汉化参考资料。各子项目均为 BepInEx 之外的 **KMod 本地模组**（`UserMod2` + Harmony）。

## 模组项目

| 项目 | 类型 | 一句话简介 | 文档 |
|---|---|---|---|
| **DirectIrrigationAdjacency** | 功能扩展 | 创意工坊 Direct Irrigation（`3743535837`）的本地扩展：环境四方向取水 + 跨元素相邻共享 + 空容器导管桥 + 灌溉充足取消运送 | [README](DirectIrrigationAdjacency/README.md) |
| **ThirdPartyHotFix_U59** | 兼容修复 | U59 下 Stairs / Better Info Cards / True Tiles 等第三方 mod 的崩溃热修 + `ElementConsumer` / `WorldGenSpawner` 核心方法空值防护 | [README](ThirdPartyHotFix_U59/README.md) |
| **智能调节器Pro** | 行为增强 | 增强原版空调 / 液体调温机：可设目标温度、可调最大降温幅度与功率、按实际降温比例动态耗电（程序集名 `ConstantTemperatureCooler`） | [README](智能调节器Pro/README.md) |
| **电力扩展** | 内容扩展 | 新增中后期电力建筑与物品：空气燃料电源、先进离子电池列、重型智能电池、中级变压器、10kW 高压导线 / 桥 | [README](电力扩展/README.md) |

## 辅助目录

| 目录 | 用途 |
|---|---|
| `0.反编译/` | Assembly-CSharp 按日期版本 + 第三方 mod 反编译归档，**设计补丁前的权威参考**（含 `索引.md`） |
| `0.汉化/` | 汉化相关资料 |

## 通用约定

- **目标框架**：.NET Framework 4.8（`智能调节器Pro` 为 4.7.1）。
- **游戏引用**：各 `.csproj` 通过 `ManagedDir` 变量引用游戏 Managed 目录 `F:\Games\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed`——换机器 / 换安装位置只需改这一处。
- **构建与部署**：`dotnet build -c Release`，PostBuild 自动把产物拷贝到本地 mod 目录 `C:\Users\AiAe\Documents\Klei\OxygenNotIncluded\mods\Local\<项目名>`。
- **PLib**：`DirectIrrigationAdjacency`、`电力扩展` 依赖；`ThirdPartyHotFix_U59`、`智能调节器Pro` 不依赖。
- **⚠️ 部署完整性**：`智能调节器Pro` 与 `电力扩展` 的源码树**未包含 `mod.yaml` / `mod_info.yaml`**，且其 csproj 仅自动拷贝 dll（不含 `anim/` 等资源）。ONI 本地 mod 必须有 `mod_info.yaml` 才会被识别——完整部署需在 mod 目录另行维护 yaml 与资源，详见各自 README。

---

> 本目录是 git 仓库。各子项目的实现细节、选项、构建与版本兼容信息见其各自的 `README.md`。
