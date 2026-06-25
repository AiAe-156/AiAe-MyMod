# DirectIrrigationAdjacency（直接灌溉 · 相邻扩展）

> 《缺氧 / Oxygen Not Included》本地模组——创意工坊 **Direct Irrigation**（`3743535837`）的本地增强扩展。
> 让种植容器在「原地直接灌溉」之外，还能从**四周环境取水**、在**相邻容器之间共享**灌溉液体与固体肥，并用空容器充当**导管桥**打通隔着一格的作物；灌溉充足时还会自动取消卡死的小人运送单。

---

## ⚠️ 前置依赖

**必须配合创意工坊 Direct Irrigation（Steam `3743535837`）使用。** 本扩展不自带灌溉框架，而是复用 DI 的整套安全机制：

- DI 为灌溉作物添加的液体 `PassiveElementConsumer`（环境消费者）；
- DI 的 `IrrigationMonitor.Instance.SetStorage` postfix → `WireLiquidPECs`（把所有液体 PEC 接到灌溉 Storage）；
- DI 的 `LiquidPECStorageLimiter`（ISim4000ms 带滞回的安全限流）。

单独启用本 mod 没有意义。

---

## 功能

### ① 环境四方向取水
在 prefab 注册阶段（`Assets.RegisterOnAddPrefab`），对**任意**带液体 PEC 的作物，克隆出几个采样偏移到邻格的同款消费者：**上 / 左 / 右各 1 格 + 向下连续 N 格**（可选再加 4 个斜角）。于是作物能直接吸取相邻格的环境液体，无需小人运送。

- **不硬编码植物列表**：凡 DI 给了基础液体 PEC 的作物（含 U59 新植物 / 其他 mod 作物）全部自动覆盖。
- **兜底自造**（`兜底覆盖未知作物`）：对 DI 不认识、但确有液体灌溉需求的作物，照 DI 同款自造一个基础消费者，再被偏移克隆。
- **越过实心砖**：土培砖等本体格是实心的，`向下取水深度=2` 即可越过砖本体、够到正下方那格的环境液体。

### ② 相邻跨元素共享
每秒在相邻种植容器之间，按「元素需求」均衡灌溉液体与固体肥。**门槛是逐元素的，不是同类型**：不同种类的种植箱 / 砖也能共享，但只共享双方作物都消耗的那个元素（盐水大家共享、盐只在需盐作物间、泥土只在需泥土作物间）。采用「向中间收敛」的纯均衡——每次最多搬差额的一半，绝不把来源搬到比目标更低，不抖动、不偷料。

### ③ 空容器导管桥
空种植容器本身不消耗元素，但当它夹在**两个以上、对同一元素存在真实落差**的已种植消费者中间时，会把该元素从更富的一侧拉进自己，缺口侧消费者再从桥里抽走——从而打通隔着一个空格的两株作物。少于两个该元素消费者、或它们已均衡时，桥一滴都不抽（**黑洞防护**，不会把单株抽干、也不会无谓稀释）。

### ④ 灌溉充足时取消小人运送
环境取水 / 相邻共享把液体填到 ≥作物的运送补充阈值后，主动取消卡死的「需要供应 XX 克」运送单。原版只在存储不足时新建运送、却不会因被环境填满而取消，导致容器明明已有上百 kg 盐水仍一直要小人搬、甚至因存储已满永远完不成。**只作用于液体**（环境可再生），固体肥料保留运送以免整排断料。

---

## 模组选项（游戏内「模组」→「选项」，PLib）

### 1 · 环境直接取水
| 选项 | 默认 | 范围 | 说明 |
|---|---|---|---|
| 启用环境四方向取水 | 开 | — | 关闭则只保留 DI 自身的「自身格」取水 |
| 向下取水深度 | 2 | 1–3 | 设 2 越过实心砖本体取其下方液体 |
| 环境取水含斜角 | 关 | — | 额外加 4 个斜角消费者（增加 sim 负担） |
| 环境取水速率倍率 | 1.0 | 1–20 | 乘 DI 基准 0.5 kg/s/消费者，高需求作物可调大 |
| 环境蓄水上限 (kg) | 100 | 10–2000 | 每株环境取水蓄入灌溉存储的上限 |
| 兜底覆盖未知作物 | 开 | — | 给 DI 不认识但需液体灌溉的作物自造基础消费者 |

### 2 · 相邻共享 / 导管桥
| 选项 | 默认 | 范围 | 说明 |
|---|---|---|---|
| 启用相邻共享 | 开 | — | 跨种类、逐元素均衡液体与固体肥 |
| 启用空容器导管桥 | 开 | — | 空容器在两株之间安全中转，带黑洞防护 |
| 排除管道供液容器(液培砖) | 开 | — | 液培砖 / 宽种植砖（管道供液）不参与共享与桥，但仍可环境取水 |
| 允许斜角补给 | 关 | — | 共享 / 桥包含 4 个斜角邻居 |
| 液体共享速率 (kg/秒) | 5 | 0.1–50 | 每个邻居每秒最多搬运的液体质量 |
| 固体共享速率 (kg/秒) | 2 | 0.1–50 | 每个邻居每秒最多搬运的固体质量 |

### 3 · 小人运送（兜底）
| 选项 | 默认 | 说明 |
|---|---|---|
| 灌溉充足时取消小人运送 | 开 | 液体足够时取消卡死运送单；液体不足照常运送；只作用于液体 |

---

## 工作原理与安全范式

- **prefab 阶段挂载**：环境消费者与相邻均衡器都在 `Assets.RegisterOnAddPrefab` 回调里添加。`PassiveElementConsumer` / `AdjacencyBalancer` 都是 SimComponent，只有走正常 spawn 生命周期才会向 Sim 注册——运行时 `AddComponent` 不触发 `OnSpawn`，无效且危险，已弃。
- **周期 Sim 驱动**：相邻共享挂在 `AdjacencyBalancer : ISim1000ms` 上，每秒触发一次。**不挂事件驱动的 `IrrigationMonitor.UpdateIrrigation`**——空仓饿砖不产生存储变化事件、永不触发，是「隔壁新砖永远不共享」的真因（已坐实，已根治）。
- **绝不手搓质量**：搬运一律走 `Storage.Transfer`，绝不 `AddLiquid` + 手改 `PrimaryElement.Mass`。早期版本每帧手改质量配合并行 Sim 会引发 `ntdll` 原生硬崩（详见 `问题与对话要点总结_2026-06-20.md`），已重写为当前安全版。

---

## 源码结构

| 文件 | 说明 |
|---|---|
| `Mod.cs` | 入口 `UserMod2.OnLoad`：注册 PLib 选项、`PatchAll`、`ElementConsumerExtender.Register()` |
| `ElementConsumerExtender.cs` | 功能①环境取水：`RegisterOnAddPrefab` 克隆偏移 PEC + 兜底自造；同时给每个 `PlantablePlot` 容器挂 `AdjacencyBalancer` |
| `AdjacencyBalancer.cs` | `ISim1000ms` 触发器壳：每秒调 `SuppressManualDelivery` + `BalancePlot` |
| `AdjacencySupply.cs` | 功能②③④主逻辑：相邻共享、导管桥、取消运送 |
| `ModOptions.cs` | PLib 选项定义（上表 13 项） |

---

## 构建与部署

```bash
dotnet build -c Release
```

`.csproj` 的 PostBuild 会自动 `xcopy` 部署以下文件到本地 mod 目录：

- `DirectIrrigationAdjacency.dll`
- `DirectIrrigationAdjacency.json`
- `mod.yaml` / `mod_info.yaml`

**本机路径（在 .csproj 中配置）：**
- 游戏 Managed 引用目录：`F:\Games\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\`
- 部署目录：`C:\Users\AiAe\Documents\Klei\OxygenNotIncluded\mods\Local\DirectIrrigationAdjacency\`
- PLib 引用：来自创意工坊订阅 `…\mods\Steam\1839645620\PLib.dll`

> `PLib.dll` 不随本 mod 部署（`Private=false`），运行时由已订阅的 PLib 提供。

---

## 版本与兼容

- 目标框架：**.NET Framework 4.8**
- `mod_info.yaml`：`minimumSupportedBuild: 737195`、`APIVersion: 2`、`version: 1.0.0`
- 依赖：Direct Irrigation（`3743535837`）、PLib

---

## 历史

`问题与对话要点总结_2026-06-20.md` 是 2026-06-20 一次崩溃排查的快照（当时为稳住游戏，曾把环境取水默认关闭、并保留 `Storage.AddLiquid` 桥接）。**该快照已过时**：此后整套逻辑被重写为当前安全版（环境取水默认开、相邻共享改为周期触发 + 逐元素门槛 + 纯 `Storage.Transfer`、新增导管桥与取消运送）。阅读源码以当前文件为准。
