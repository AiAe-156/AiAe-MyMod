# 电力扩展（PowerExtension）

> 《缺氧 / Oxygen Not Included》本地模组——新增一批中后期**电力建筑、移动电源与高压导线**，补全原版储能与配电的档位空缺。

程序集名 / 命名空间：`电力扩展` / `PowerExtension`。

> 📖 各内容物的**详细数值、配方与历次平衡改动**见同目录 [`说明.md`](说明.md)，本文件为开发 / 安装向概览。

---

## 内容一览

### 🔋 物品（焊接台制造）
| 名称 | 内部 ID | 说明 |
|---|---|---|
| 空气燃料电源 | `AirBattery` | 一次性高能移动电源，默认 720 kJ；经大型 / 袖珍放电器供电。**损毁会爆炸放热（约 1.2 MJ）** |
| 耗尽的空气燃料电源 | `EmptyAirBattery` | 用尽后的外壳，可在焊接台翻新回收 |

### 🏗️ 建筑
| 名称 | 内部 ID | 说明 |
|---|---|---|
| 先进离子电池列 | `IonBatteryArray` | 3×4，**即建即满电**，可调变压输出（0–20000 W）+ 自动化接口，默认 1800 kJ；高发热、有自放电 |
| 重型智能电池 | `StackBattery` | 3×2，**可落地、可堆叠（≤15 层，下层为上层地基）**、可建于水中；智能电池逻辑；默认 100 kJ。**需眼冒金星 DLC**（复用蓄电池舱贴图与太空电力科技） |
| 中级变压器 | `PowerTransformer2kW` | 2×2，把流过的电力限制为 2 kW |

### 🔌 电力系统
| 名称 | 内部 ID | 承载 | 说明 |
|---|---|---|---|
| 高压导线 | `MediumWire` | **10 kW** | 介于精炼高瓦导线（20 kW）与原版橡胶绝缘导线（4 kW）之间；精炼金属 + 橡胶/塑料建造 |
| 高压导线桥 | `MediumWireBridge` | **10 kW** | 同上承载的导线桥 |

---

## 模组选项（PLib，支持热加载）

实现 `IOptions`，在游戏内「模组」→「选项」修改后**无需重启**，新建建筑立即采用新值。

| 选项 | 默认 |
|---|---|
| 离子电池列容量 (千焦) | 1800 |
| 空气燃料电源容量 (千焦) | 720 |
| 堆叠蓄电池容量 (千焦) | 100（漏电按容量 1.5%/周期、发热按容量 2% 千复制热/秒联动） |

---

## 科技与 DLC

- **科技解锁**：大部分内容在「可再生能源」科技；**重型智能电池**在「太空电力（SpacePower）」（与原版蓄电池仓同节点）。
- **DLC 要求**：重型智能电池需 **眼冒金星 / Spaced Out（DLC3）**；空气燃料电源系统需 DLC3；高压导线 / 桥、离子电池列、中级变压器在基础版即可。

---

## 源码结构

| 文件 | 说明 |
|---|---|
| `PowerExtensionMod.cs` | 入口：`PUtil.InitLibrary` + 注册 PLib 选项 |
| `PowerExtensionPatches.cs` | 主补丁集（配方注入、智能电池 UI、变压输出、堆叠 / 动画 / 染色等，最大文件） |
| `AirBattery.cs` / `AirBatteryConfig.cs` / `EmptyAirBatteryConfig.cs` | 空气燃料电源（运行时逻辑 + 满 / 空两态配置） |
| `IonBatteryArrayConfig.cs` / `IonBatterySmart.cs` / `IonBatteryTransformer.cs` | 先进离子电池列（配置 + 共享智能电池逻辑 + 变压输出） |
| `StackBatteryConfig.cs` | 重型智能电池（堆叠 / 地基校验 / 动画 / 染色） |
| `PowerTransformer2kWConfig.cs` | 中级变压器 |
| `MediumWireConfig.cs` / `MediumWireBridgeConfig.cs` | 高压导线 / 导线桥 |
| `ModOptions.cs` | PLib 容量选项（热加载） |
| `Strings.cs` | 全部中文本地化字符串 |
| `anim/` | 自定义动画资源（变压器 / 大型智能电池 / 导线 / 导线桥） |

---

## 构建与部署

```bash
dotnet build -c Release
```

- 目标框架：**.NET Framework 4.8**。
- 依赖：**PLib 4.19.0**（NuGet `packages\` 内）、Harmony、Newtonsoft.Json。
- 构建后 `CopyToLocalMods` **仅自动复制 `电力扩展.dll`** 到部署目录。

**本机路径（在 .csproj 中配置）：**
- 游戏 Managed：`F:\Games\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed`
- 部署目录：`C:\Users\AiAe\Documents\Klei\OxygenNotIncluded\mods\Local\电力扩展`

> ⚠️ **资源不随 dll 自动部署**：本 mod 含自定义建筑动画（`anim/`），且 ONI 本地 mod 还需 `mod_info.yaml`（仓库内未包含）。完整可运行的 mod 目录除 `电力扩展.dll` 外，还应包含 `anim/` 资源与 `mod_info.yaml` / `mod.yaml`（首次部署需手动放好，之后只热更 dll）。参考 `mod_info.yaml`：
> ```yaml
> supportedContent: ALL
> minimumSupportedBuild: 737195
> version: 1.0.0
> APIVersion: 2
> ```

---

## 兼容性提示

- 高压导线为 10 kW，与 U59 官方新增的 4 kW 橡胶绝缘导线（`WireRubber` / `WireRubberBridge`）定位错开、并存不冲突。
- 历次随游戏版本的适配（如 U59 `MeterScreen_Electrobanks` API 改名、导线瓦数枚举调整）详见 [`说明.md`](说明.md) 的更新日志。
