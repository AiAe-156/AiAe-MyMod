# ThirdParty HotFix U59

`ThirdPartyHotFix_U59` 是一个用于《Oxygen Not Included》U59 版本的本地第三方模组兼容修复补丁。

## 适用环境

- 游戏：Oxygen Not Included
- 当前验证版本：U59-737195-SCRPAN
- 目标框架：.NET Framework 4.8
- 本地部署目录：`C:\Users\AiAe\Documents\Klei\OxygenNotIncluded\mods\Local\ThirdPartyHotFix_U59`

## 已包含修复

### Stairs

- 在 `Db.Initialize` 后移除 `Stairs.PathProbeTask_Patch.Prefix` 对 `AsyncPathProber.Manager.NextTask` 的补丁。
- 用于规避 U59 下路径探测相关兼容问题。

### Better Info Cards

- 为 `Database.ExportGO` 添加空对象保护前缀。
- 当 `BetterInfoCards.Patches.DatabasePatches.curSelectable` 为 `null` 或 Unity 已销毁对象时跳过导出，避免空引用异常。

### True Tiles

- 在 HotFix `OnLoad` 阶段安装早期 Harmony hook，拦截 `HarmonyLib.PatchClassProcessor.Patch`。
- 将 True Tiles 的 `BlockTileRenderer.AddBlock` transpiler 重定向到 U59 的 6 参数 overload：`AddBlock(int, BuildingDef, bool, SimHashes, int, bool)`。
- 将 True Tiles 的 `RenderInfo` postfix 应用到 U59 的 6 参数构造函数。
- 在 `Db.Initialize` 后补充兜底 patch，处理 True Tiles 已加载但早期 hook 未覆盖的情况。
- 替换并保护 `Assets.OnPrefabInit` 的 True Tiles postfix。
- 增加 finalizer，仅吞掉来自 `TrueTiles.Patches.AssetsPatch+Assets_OnPrefabInit_Patch.Postfix` 的 `NullReferenceException`。

### ElementConsumer（核心方法通用防护）

- 为 `ElementConsumer.AddMass(Sim.ConsumedMassInfo)` 添加优先级最高的前缀。
- 当 sim 回调命中的 `ElementConsumer` 或其 `storage` 已被销毁（stale handle）时，从 handle 映射中移除并跳过本次 AddMass，避免在已销毁对象上触发空引用异常。

### WorldGenSpawner（核心方法通用防护）

- 为 `WorldGenSpawner.Spawnable.TrySpawn` 添加优先级最高的前缀。
- 当 `spawnInfo.id` 为空时跳过生成并调用 `FreeResources()`，避免世界生成阶段因空 prefab id 崩溃。
- 主要在「全新生成世界」阶段生效。

> **与 Fully Submerged 的关系**：上面的 ElementConsumer / WorldGenSpawner 两条是核心方法的通用空值 / 竞态防护，不针对任何具体第三方模组，最初是为兼容 Fully Submerged（`pythooonuser-fully-submerged`，Steam `3742843221`，Aquatic 水世界 worldgen 模组）而加入。
>
> 该模组现已更新至 **0.2.0**：其 yaml 数据中不再存在空的 prefab `id`，唯一一处 `internalMobs`（`PalmyShore.yaml` 的 DewPalm / Slickshell）已整段注释，且近期 0.1.0 / 0.2.0 两次游戏日志中这两条守卫从未实际触发（无 `Skip ...` 记录）。
>
> 因此对 0.2.0 而言这两条补丁已基本不再必要。考虑到它们是对核心方法的通用防护、开销极小、且不与 Fully Submerged 冲突，目前**保留**作为安全网。如需 100% 确认，可在临时禁用这两条守卫后用 0.2.0 重新生成一张 Aquatic 星图，观察是否复现原 NRE。

## 加载顺序要求

True Tiles 的 `AddBlock` 崩溃发生在 True Tiles `OnLoad/PatchAll` 阶段。若要让早期 hook 生效，`ThirdPartyHotFix_U59` 必须启用并排在 `TrueTiles` 前加载。

建议顺序：

1. `ThirdPartyHotFix_U59`
2. `TrueTiles`
3. `AkiTrueTiles_SkinSelectorAddon`

如果 HotFix 排在 True Tiles 后面，`Db.Initialize` 阶段兜底仍可能修复部分运行期问题，但无法保证阻止 True Tiles 加载期的 `AmbiguousMatchException`。

## 构建与部署

在源码目录执行：

```powershell
dotnet build "e:\项目\缺氧\MyMod\ThirdPartyHotFix_U59\ThirdPartyHotFix.csproj" -c Release
```

项目的 post-build 会自动复制：

- `ThirdPartyHotFix.dll`
- `mod.yaml`
- `mod_info.yaml`

到本地模组目录：

```text
C:\Users\AiAe\Documents\Klei\OxygenNotIncluded\mods\Local\ThirdPartyHotFix_U59
```

## 日志验证

游戏日志路径：

```text
C:\Users\AiAe\AppData\LocalLow\Klei\Oxygen Not Included\Player.log
```

关键成功日志：

```text
[ThirdPartyHotFix] Installed TrueTiles early patch hook
[ThirdPartyHotFix] Patched TrueTiles AddBlock transpiler to U59 6-arg overload
[ThirdPartyHotFix] Patched TrueTiles RenderInfo postfix to U59 6-arg constructor
[ThirdPartyHotFix] Removed TrueTiles Assets.OnPrefabInit postfix owner=TrueTiles
[ThirdPartyHotFix] Replaced TrueTiles Assets.OnPrefabInit postfix with guarded wrapper and finalizer
[ThirdPartyHotFix] Unpatched Stairs.PathProbeTask_Patch.Prefix on AsyncPathProber.Manager.NextTask
[ThirdPartyHotFix] Guarded BetterInfoCards.ExportGO with null-check prefix
[ThirdPartyHotFix] Guarded ElementConsumer.AddMass against destroyed storage callbacks
[ThirdPartyHotFix] Guarded WorldGenSpawner.Spawnable.TrySpawn against null prefab ids
```

上面是各补丁的「安装成功」日志，并不代表它们真正拦截到了问题。ElementConsumer / WorldGenSpawner 两条守卫只有在运行期实际命中问题时才会打印下面的 `Skip` 日志（正常情况下不应出现）：

```text
[ThirdPartyHotFix] Skip WorldGenSpawner null prefab id at cell=... type=... element=... pos=(...)
[ThirdPartyHotFix] Skip ElementConsumer.AddMass: storage is destroyed
```

若旧 True Tiles postfix 仍被 Harmony 动态 wrapper 调用，但异常被正确压制，可能出现：

```text
[ThirdPartyHotFix] Suppressed TrueTiles Assets.OnPrefabInit NullReferenceException
```

不应再出现：

```text
Ambiguous match for HarmonyMethod[(class=Rendering.BlockTileRenderer, methodname=AddBlock
TrueTiles.Patches.AssetsPatch+Assets_OnPrefabInit_Patch.Postfix
```

## 反编译参考

本次 True Tiles 修复参考了以下归档：

- `e:\项目\缺氧\MyMod\0.反编译\TrueTiles_2815406414_2026-06-18`
- `e:\项目\缺氧\MyMod\0.反编译\Assembly-CSharp_ONI_2026-06-18_U59-737195`

反编译索引：

- `e:\项目\缺氧\MyMod\0.反编译\索引.md`
