# 智能调节器 Pro（ConstantTemperatureCooler）

> 《缺氧 / Oxygen Not Included》本地模组——增强原版**空调（气体）**与**液体调温机**，让它们从「固定降温 14°C / 满功率运转」变成**可设目标温度、可调降温幅度与功率、并按实际降温比例动态耗电**的智能调温设备。

程序集名 / 命名空间：`ConstantTemperatureCooler`（项目文件夹名为「智能调节器Pro」）。

---

## 功能

给原版 `AirConditioner`（空调）与 `LiquidConditioner`（液体调温机）注入一个 `AirConditionerAdjustable` 组件，在建筑详情侧栏新增「温度调控」面板：

### 🎯 目标温度
复用原版容量控制侧屏（标题改为「目标温度」）。调节器会把流体最低冷却到该温度，可设范围 **0 K ~ 573.15 K（约 0 ~ 300°C）**。流体已低于目标温度时不再降温（避免过冷）。

### 📉 最大降温幅度（滑块，1 ~ 20°C）
单次通过管道可降低的最大温度。原版固定为 20°C，这里可调小——降温越小，**单位流体的耗电越低**。

### ⚡ 最大功率限制（滑块，1 W ~ 建筑额定功率）
直接限制设备允许消耗的最大功率，由它反推等效降温幅度。

### 🔀 调控模式（复选框：温度限制 / 功率限制，二选一）
- **取消勾选**＝以「最大降温幅度」为准（功率随之浮动，功率滑块显示为参考值）；
- **勾选**＝以「最大功率限制」为准（降温幅度随之浮动，降温滑块显示为参考值）。
- 侧栏 tooltip 实时显示「当前生效：最大降温 X°C / 最大功耗 Y W」，并按 10 kg/s 水举例换算「最大可转移 Z kDTU/s 热量」。

### 🔋 动态功耗
实际功耗 = 额定功率 × (实际降温幅度 ÷ 20)。即**部分负载时按比例省电**，而非原版的恒定满载。本 mod 同时把两种设备的额定功耗设为：**气体空调 340 W、液体调温机 1700 W**。

### 📋 其他
- 支持**复制设置**（`CopyBuildingSettings`）：目标温度、降温幅度、功率、调控模式一并复制到同类设备。
- 侧栏面板重排：目标温度在上、调控限制在下。

---

## 工作原理

| 补丁 | 作用 |
|---|---|
| `AirConditionerConfig / LiquidConditionerConfig . ConfigureBuildingTemplate` (postfix) | 给两种调温机 `AddOrGet<AirConditionerAdjustable>` |
| `…Config . CreateBuildingDef` (postfix) | 设额定功耗：空调 340 W、液冷 1700 W |
| `AirConditioner . UpdateState` (**prefix, return false**) | 核心：完整重写降温逻辑，把原版硬编码的 20°C 上限换成动态 `GetEffectiveMaxDelta()` |
| `CapacityControlSideScreen . SetTarget` (postfix) | 仅对本 mod 设备，把侧屏标题改为「目标温度」 |
| `DetailsScreen . OnPrefabInit` (postfix) | 调整侧栏顺序，让目标温度排在调控限制之前 |

`AirConditionerAdjustable` 实现 `IUserControlledCapacity`（目标温度）、`IMultiSliderControl`（双滑块）、`ICheckboxControl`（调控模式），状态用 `[Serialize]` 持久化存档。

---

## 源码结构

| 文件 | 说明 |
|---|---|
| `AdjustableCoolersMod.cs` | 入口 `UserMod2.OnLoad`：注册本地化键、`PatchAll` 由 Harmony 自动完成 |
| `AirConditionerAdjustable.cs` | 核心组件：目标温度 / 双滑块 / 复选框 / 动态功耗 + 两个滑块控制器 |
| `AirConditionerPatches.cs` | 全部 Harmony 补丁（见上表） |
| `Strings.cs` | 「温度调控」面板的中文本地化字符串 |
| `Properties/AssemblyInfo.cs` | 程序集信息 |

---

## 构建与部署

```bash
dotnet build -c Release
```

- 目标框架：**.NET Framework 4.7.1**，C# 12，`AllowUnsafeBlocks`。
- **不依赖 PLib**，仅引用 `Assembly-CSharp`(`-firstpass`)、`0Harmony`、`UnityEngine`(`.CoreModule`)。
- 构建后 `CopyToModDir` 自动把 `ConstantTemperatureCooler.dll` 复制到部署目录。

**本机路径（在 .csproj 中配置）：**
- 游戏 Managed：`F:\Games\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed`
- 部署目录：`C:\Users\AiAe\Documents\Klei\OxygenNotIncluded\mods\Local\智能调节器Pro`

> ⚠️ **本仓库未包含 `mod.yaml` / `mod_info.yaml`**，而 ONI 本地 mod 必须有 `mod_info.yaml` 才会被游戏识别。若部署目录尚无这两个文件，请补上，例如：
>
> `mod_info.yaml`
> ```yaml
> supportedContent: ALL
> minimumSupportedBuild: 737195
> version: 1.0.0
> APIVersion: 2
> ```
> `mod.yaml`
> ```yaml
> title: 智能调节器Pro
> staticID: ConstantTemperatureCooler
> description: 让空调与液体调温机可设目标温度、可调降温幅度与功率，并按实际降温比例动态耗电。
> ```

---

## 备注

- 仅改动原版两种调温机的行为，不新增建筑、不依赖 DLC。
- `UpdateState` 采用 prefix `return false` 完全接管原版方法——若游戏大版本改了 `AirConditioner.UpdateState` 的内部字段名，需对照反编译同步更新（参见 `..\0.反编译\`）。
