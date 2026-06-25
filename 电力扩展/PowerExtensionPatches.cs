// ============================================================
// 模块: PowerExtensionPatches.cs
// 描述: 电力扩展模组的 Harmony 补丁集合
//       包含高压导线(10kW)、空气燃料电源、离子电池列、
//       电路管理器扩展等核心功能的补丁
// 最后修改: 2026-05-28 (U59-731233 Aquatic DLC Beta 兼容性修订)
// 修改概览: 完整修复 8kW 导线等级扩展导致的 IndexOutOfRangeException
//           - 修复 wireGroups 构造器补丁未正确写回字段的问题
//           - 新增 Reset/UpdateOverloadTime 补丁，消除硬编码循环上限
//           - 新增 Refresh 补丁，确保新 CircuitInfo 的 bridgeGroups 数组大小
//           - [2026-04-04] 新增 GetMaxSafeWattage 补丁，修复 F2 电力覆盖层崩溃
//           - [2026-04-04] UpdateOverloadTime 增加 bridgeGroups 边界检查
//           - [2026-05-28] U59 已官方加入 4kW 橡胶绝缘导线(Max4000=5)，
//             本 mod 改用 Max8kW=6 作为更高一档(8kW)的中压导线，
//             与官方 4kW 错开，且建材改为「精炼金属 + 橡胶」对齐设计语义
//           - [2026-05-28] 重写 MeterScreen_Electrobanks 两个补丁以适配
//             U59 新 API (joulesDictionary 字段已改名为
//             per_electrobankType_UnitCount_Dictionary，且语义改为单位计数)
// ============================================================
using HarmonyLib;
using STRINGS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PowerExtension
{
    public static class PowerExtensionPatches
    {
        [HarmonyPatch(typeof(Localization), "Initialize")]
        public class Localization_Initialize_Patch
        {
            public static void Postfix()
            {
                // 1. 自动注册模组内的所有 Strings
                ModUtil.RegisterForTranslation(typeof(PowerExtension.Strings));

                // 2. [核心修复] 手动补全富勒烯标签
                // 解决 "MISSING.STRINGS.MISC.TAGS.FULLERENE"
                global::Strings.Add("STRINGS.MISC.TAGS.FULLERENE", "富勒烯");

                // 3. 映射建筑名称
                // 因为 ModUtil 生成的 ID 是 "POWEREXTENSION.STRINGS.EXT_BUILDINGS..."
                // 但游戏生成菜单时查找的是 "STRINGS.BUILDINGS.PREFABS.{ID}..."
                // 所以这里需要手动建立映射

                // 中压导线
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.MEDIUMWIRE.NAME", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.MEDIUMWIRE.NAME);
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.MEDIUMWIRE.DESC", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.MEDIUMWIRE.DESC);
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.MEDIUMWIRE.EFFECT", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.MEDIUMWIRE.EFFECT);

                // 中压导线桥
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.MEDIUMWIREBRIDGE.NAME", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.MEDIUMWIREBRIDGE.NAME);
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.MEDIUMWIREBRIDGE.DESC", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.MEDIUMWIREBRIDGE.DESC);
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.MEDIUMWIREBRIDGE.EFFECT", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.MEDIUMWIREBRIDGE.EFFECT);

                // 2kW 变压器
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.POWERTRANSFORMER2KW.NAME", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.POWERTRANSFORMER2KW.NAME);
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.POWERTRANSFORMER2KW.DESC", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.POWERTRANSFORMER2KW.DESC);
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.POWERTRANSFORMER2KW.EFFECT", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.POWERTRANSFORMER2KW.EFFECT);

                // 离子电池列
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.IONBATTERYARRAY.NAME", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.IONBATTERYARRAY.NAME);
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.IONBATTERYARRAY.DESC", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.IONBATTERYARRAY.DESC);
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.IONBATTERYARRAY.EFFECT", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.IONBATTERYARRAY.EFFECT);

                // 堆叠蓄电池
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.STACKBATTERY.NAME", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.STACKBATTERY.NAME);
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.STACKBATTERY.DESC", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.STACKBATTERY.DESC);
                global::Strings.Add("STRINGS.BUILDINGS.PREFABS.STACKBATTERY.EFFECT", PowerExtension.Strings.EXT_BUILDINGS.PREFABS.STACKBATTERY.EFFECT);

                // 物品映射
                global::Strings.Add("STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.AIRBATTERY.NAME", PowerExtension.Strings.EXT_ITEMS.INDUSTRIAL_PRODUCTS.AIRBATTERY.NAME);
                global::Strings.Add("STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.AIRBATTERY.DESC", PowerExtension.Strings.EXT_ITEMS.INDUSTRIAL_PRODUCTS.AIRBATTERY.DESC);
                global::Strings.Add("STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.EMPTYAIRBATTERY.NAME", PowerExtension.Strings.EXT_ITEMS.INDUSTRIAL_PRODUCTS.EMPTYAIRBATTERY.NAME);
                global::Strings.Add("STRINGS.ITEMS.INDUSTRIAL_PRODUCTS.EMPTYAIRBATTERY.DESC", PowerExtension.Strings.EXT_ITEMS.INDUSTRIAL_PRODUCTS.EMPTYAIRBATTERY.DESC);

                // 标签
                global::Strings.Add("STRINGS.MISC.TAGS.POWEREXT_IONMETAL", PowerExtension.Strings.EXT_MISC.TAGS.POWEREXT_IONMETAL);

                // 选项
                global::Strings.Add("STRINGS.UI.FRONTEND.POWEREXTENSION.MODOPTIONS.BATTERYCAPACITY.NAME", PowerExtension.Strings.EXT_UI.FRONTEND.POWEREXTENSION.MODOPTIONS.BATTERYCAPACITY.NAME);
                global::Strings.Add("STRINGS.UI.FRONTEND.POWEREXTENSION.MODOPTIONS.BATTERYCAPACITY.TOOLTIP", PowerExtension.Strings.EXT_UI.FRONTEND.POWEREXTENSION.MODOPTIONS.BATTERYCAPACITY.TOOLTIP);
                global::Strings.Add("STRINGS.UI.FRONTEND.POWEREXTENSION.MODOPTIONS.IONBATTERYCAPACITY.NAME", PowerExtension.Strings.EXT_UI.FRONTEND.POWEREXTENSION.MODOPTIONS.IONBATTERYCAPACITY.NAME);
                global::Strings.Add("STRINGS.UI.FRONTEND.POWEREXTENSION.MODOPTIONS.IONBATTERYCAPACITY.TOOLTIP", PowerExtension.Strings.EXT_UI.FRONTEND.POWEREXTENSION.MODOPTIONS.IONBATTERYCAPACITY.TOOLTIP);
            }
        }

        // ======================== 元素标签注入 ========================
        [HarmonyPatch(typeof(ElementLoader), "Load")]
        public class ElementLoader_Load_Patch
        {
            public static void Postfix()
            {
                Tag ionTag = TagManager.Create("PowerExt_IonMetal");

                // 铝
                Element al = ElementLoader.FindElementByHash(SimHashes.Aluminum);
                if (al != null)
                {
                    var list = al.oreTags.ToList();
                    if (!list.Contains(ionTag)) list.Add(ionTag);
                    al.oreTags = list.ToArray();
                }

                // [新增] 铁 (精炼铁)
                Element iron = ElementLoader.FindElementByHash(SimHashes.Iron);
                if (iron != null)
                {
                    var list = iron.oreTags.ToList();
                    if (!list.Contains(ionTag)) list.Add(ionTag);
                    iron.oreTags = list.ToArray();
                }

                // 固体锌
                Element zn = ElementLoader.FindElementByHash(SimHashes.Zinc);
                if (zn != null)
                {
                    var list = zn.oreTags.ToList();
                    if (!list.Contains(ionTag)) list.Add(ionTag);
                    zn.oreTags = list.ToArray();
                }
            }
        }

        // ======================== 电池面板 UI 修复 ========================
        [HarmonyPatch(typeof(BatteryUI), "Initialize")]
        public class BatteryUI_Initialize_Patch
        {
            public static void Postfix(BatteryUI __instance)
            {
                var sizeMapField = Traverse.Create(__instance).Field("sizeMap");
                var map = sizeMapField.GetValue<Dictionary<float, float>>();
                if (map != null)
                {
                    // 注意：这里读取的是千焦(kJ)，需要乘1000转为焦耳(J)以匹配电池实际容量
                    float ionCap = ModOptions.Instance.IonBatteryCapacity * 1000f;

                    if (!map.ContainsKey(ionCap))
                        map[ionCap] = 30f;
                }
            }
        }

        // ======================== 8kW 导线逻辑 ========================
        // [2026-05-28] U59 已把 Wire.WattageRating 官方扩展到
        //   Max500=0, Max1000=1, Max2000=2, Max20000=3, Max50000=4,
        //   Max4000=5, NumRatings=6
        // 因此原本占用的索引 7 (Max4kW) 改为索引 6 (Max8kW)，
        // wattage 设为 10000W(10kW)，与官方 4kW 错开。
        // 注：枚举名 Max8kW 为历史命名，实际承载已调整为 10kW(见 GetMaxWattageAsFloat)。
        // 注：索引 6 在原版 enum 中是 NumRatings (上限标记)，被我们
        // 实际占用为可用导线槽，需通过 wireGroups/bridgeGroups 数组
        // 扩展 patch 把 6 槽数组扩到 7 槽，避免 AddItem/Refresh 越界。
        public enum ExtendedWattageRating { Max8kW = 6 }

        [HarmonyPatch(typeof(Wire), "GetMaxWattageAsFloat")]
        public class Wire_GetMaxWattageAsFloat_Patch
        {
            public static bool Prefix(Wire.WattageRating rating, ref float __result)
            {
                if ((int)rating == (int)ExtendedWattageRating.Max8kW) { __result = 10000f; return false; }
                return true;
            }
        }

        // [修复 2026-02-16] 原实现通过 ref ___wireGroups 替换数组，
        // 但新数组未正确写回字段且丢失了原有元素，导致 CircuitManager.Rebuild 越界。
        // 改用 Traverse 显式设置字段值，并复制原有元素到新数组中。
        [HarmonyPatch(typeof(ElectricalUtilityNetwork), MethodType.Constructor)]
        public class ElectricalUtilityNetwork_CTor_Patch
        {
            public static void Postfix(ElectricalUtilityNetwork __instance)
            {
                int neededSize = (int)ExtendedWattageRating.Max8kW + 1;
                var field = Traverse.Create(__instance).Field<List<Wire>[]>("wireGroups");
                if (field.Value.Length < neededSize)
                {
                    var newArray = new List<Wire>[neededSize];
                    for (int i = 0; i < field.Value.Length; i++)
                        newArray[i] = field.Value[i];
                    field.Value = newArray;
                }
            }
        }

        // [新增 2026-02-16] 原版 Reset 中硬编码 for (i < 5)，
        // 无法遍历到扩展的 wireGroups[7]，导致中压导线不会被正确重置。
        // 替换为动态数组长度遍历。
        [HarmonyPatch(typeof(ElectricalUtilityNetwork), nameof(ElectricalUtilityNetwork.Reset))]
        public class ElectricalUtilityNetwork_Reset_Patch
        {
            public static bool Prefix(ElectricalUtilityNetwork __instance, List<Wire> ___allWires, UtilityNetworkGridNode[] grid, float ___timeOverloaded, List<Wire>[] ___wireGroups)
            {
                for (int i = 0; i < ___wireGroups.Length; i++)
                {
                    List<Wire> wires = ___wireGroups[i];
                    if (wires == null) continue;
                    for (int j = 0; j < wires.Count; j++)
                    {
                        Wire wire = wires[j];
                        if (wire != null)
                        {
                            wire.circuitOverloadTime = ___timeOverloaded;
                            int cell = Grid.PosToCell(wire.transform.GetPosition());
                            UtilityNetworkGridNode node = grid[cell];
                            node.networkIdx = -1;
                            grid[cell] = node;
                        }
                    }
                    wires.Clear();
                }
                ___allWires.Clear();
                Traverse.Create(__instance).Method("RemoveOverloadedNotification").GetValue();
                return false;
            }
        }

        // [新增 2026-02-16] 原版 UpdateOverloadTime 中硬编码 for (i < 5)，
        // 无法检测到 wireGroups[7] 上的过载。替换为动态数组长度遍历。
        // [修复 2026-04-04] 增加 bridgeGroups 边界检查，防止 bridgeGroups 未扩展时越界。
        [HarmonyPatch(typeof(ElectricalUtilityNetwork), nameof(ElectricalUtilityNetwork.UpdateOverloadTime))]
        public class ElectricalUtilityNetwork_UpdateOverloadTime_Patch
        {
            public static bool Prefix(ElectricalUtilityNetwork __instance, float dt, float watts_used,
                List<WireUtilityNetworkLink>[] bridgeGroups,
                ref float ___timeOverloaded, ref GameObject ___targetOverloadedWire,
                ref Notification ___overloadedNotification, ref float ___timeOverloadNotificationDisplayed,
                List<Wire>[] ___wireGroups)
            {
                bool wattage_rating_exceeded = false;
                List<Wire> overloaded_wires = null;
                List<WireUtilityNetworkLink> overloaded_bridges = null;
                for (int i = 0; i < ___wireGroups.Length; i++)
                {
                    List<Wire> wires = ___wireGroups[i];
                    List<WireUtilityNetworkLink> bridges = (i < bridgeGroups.Length) ? bridgeGroups[i] : null;
                    Wire.WattageRating rating = (Wire.WattageRating)i;
                    float max_wattage = Wire.GetMaxWattageAsFloat(rating);
                    if (watts_used > max_wattage && ((bridges != null && bridges.Count > 0) || (wires != null && wires.Count > 0)))
                    {
                        wattage_rating_exceeded = true;
                        overloaded_wires = wires;
                        overloaded_bridges = bridges;
                        break;
                    }
                }
                overloaded_wires?.RemoveAll((Wire x) => x == null);
                overloaded_bridges?.RemoveAll((WireUtilityNetworkLink x) => x == null);
                if (wattage_rating_exceeded)
                {
                    ___timeOverloaded += dt;
                    if (___timeOverloaded > 6f)
                    {
                        ___timeOverloaded = 0f;
                        if (___targetOverloadedWire == null)
                        {
                            if (overloaded_bridges != null && overloaded_bridges.Count > 0)
                            {
                                int idx = UnityEngine.Random.Range(0, overloaded_bridges.Count);
                                ___targetOverloadedWire = overloaded_bridges[idx].gameObject;
                            }
                            else if (overloaded_wires != null && overloaded_wires.Count > 0)
                            {
                                int idx = UnityEngine.Random.Range(0, overloaded_wires.Count);
                                ___targetOverloadedWire = overloaded_wires[idx].gameObject;
                            }
                        }
                        if (___targetOverloadedWire != null)
                        {
                            ___targetOverloadedWire.BoxingTrigger(-794517298, new BuildingHP.DamageSourceInfo
                            {
                                damage = 1,
                                source = STRINGS.BUILDINGS.DAMAGESOURCES.CIRCUIT_OVERLOADED,
                                popString = STRINGS.UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.CIRCUIT_OVERLOADED,
                                takeDamageEffect = SpawnFXHashes.BuildingSpark,
                                fullDamageEffectName = "spark_damage_kanim",
                                statusItemID = Db.Get().BuildingStatusItems.Overloaded.Id
                            });
                        }
                        if (___overloadedNotification == null)
                        {
                            ___timeOverloadNotificationDisplayed = 0f;
                            ___overloadedNotification = new Notification(STRINGS.MISC.NOTIFICATIONS.CIRCUIT_OVERLOADED.NAME, NotificationType.BadMinor, null, click_focus: ___targetOverloadedWire.transform);
                            GameScheduler.Instance.Schedule("Power Tutorial", 2f, delegate
                            {
                                Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_Power);
                            });
                            Notifier notifier = Game.Instance.FindOrAdd<Notifier>();
                            notifier.Add(___overloadedNotification);
                        }
                    }
                }
                else
                {
                    ___timeOverloaded = Mathf.Max(0f, ___timeOverloaded - (dt * 0.95f));
                    ___timeOverloadNotificationDisplayed += dt;
                    if (___timeOverloadNotificationDisplayed > 5f)
                    {
                        Traverse.Create(__instance).Method("RemoveOverloadedNotification").GetValue();
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CircuitManager), "Rebuild")]
        public class CircuitManager_Rebuild_Patch
        {
            public static bool Prefix(object ___circuitInfo)
            {
                var circuitInfoList = (IList)___circuitInfo;
                int neededSize = (int)ExtendedWattageRating.Max8kW + 1;
                for (int i = 0; i < circuitInfoList.Count; i++)
                {
                    object info = circuitInfoList[i];
                    var bridgeGroups = Traverse.Create(info).Field<List<WireUtilityNetworkLink>[]>("bridgeGroups");
                    if (bridgeGroups.Value.Length < neededSize)
                    {
                        var list = bridgeGroups.Value.ToList();
                        while (list.Count < neededSize) list.Add(new List<WireUtilityNetworkLink>());
                        bridgeGroups.Value = list.ToArray();
                    }
                }
                return true;
            }
        }

        // [新增 2026-02-16] 原版 Refresh 中创建新 CircuitInfo 时
        // bridgeGroups = new List<WireUtilityNetworkLink>[5]，长度不够。
        // 在 Refresh 之后扩展所有 bridgeGroups 到 neededSize。
        [HarmonyPatch(typeof(CircuitManager), "Refresh")]
        public class CircuitManager_Refresh_Patch
        {
            public static void Postfix(object ___circuitInfo)
            {
                var circuitInfoList = (IList)___circuitInfo;
                int neededSize = (int)ExtendedWattageRating.Max8kW + 1;
                for (int i = 0; i < circuitInfoList.Count; i++)
                {
                    object info = circuitInfoList[i];
                    var bridgeGroups = Traverse.Create(info).Field<List<WireUtilityNetworkLink>[]>("bridgeGroups");
                    if (bridgeGroups.Value.Length < neededSize)
                    {
                        var newArray = new List<WireUtilityNetworkLink>[neededSize];
                        for (int j = 0; j < bridgeGroups.Value.Length; j++)
                            newArray[j] = bridgeGroups.Value[j];
                        for (int j = bridgeGroups.Value.Length; j < neededSize; j++)
                            newArray[j] = new List<WireUtilityNetworkLink>();
                        bridgeGroups.Value = newArray;
                        circuitInfoList[i] = info;
                    }
                }
            }
        }

        // [修复 2026-04-04] GetMaxSafeWattage 在 wireGroups 被扩展到8的情况下，
        // 用 wireGroups.Length 作为循环上限，但传入的 bridgeGroups 仍可能只有5个元素
        // (因为 CircuitInfo 是 struct，Rebuild/Refresh 补丁的扩展可能未及时覆盖所有路径)。
        // 这导致按 F2 切换电力覆盖层时，每帧调用 GetMaxSafeWattageForCircuit → GetMaxSafeWattage
        // 访问 bridgeGroups[5..7] 越界爆炸。
        // 解决方案：替换整个方法，用 min(wireGroups.Length, bridgeGroups.Length) 作为循环上限。
        [HarmonyPatch(typeof(ElectricalUtilityNetwork), nameof(ElectricalUtilityNetwork.GetMaxSafeWattage))]
        public class ElectricalUtilityNetwork_GetMaxSafeWattage_Patch
        {
            public static bool Prefix(ElectricalUtilityNetwork __instance,
                List<WireUtilityNetworkLink>[] bridgeGroups,
                List<Wire>[] ___wireGroups,
                ref float __result)
            {
                int limit = Mathf.Min(___wireGroups.Length, bridgeGroups.Length);
                for (int i = 0; i < limit; i++)
                {
                    List<Wire> wires = ___wireGroups[i];
                    bool hasWires = wires != null && wires.Count > 0;
                    List<WireUtilityNetworkLink> bridges = bridgeGroups[i];
                    bool hasBridges = bridges != null && bridges.Count > 0;
                    if (hasWires || hasBridges)
                    {
                        __result = Wire.GetMaxWattageAsFloat((Wire.WattageRating)i);
                        return false;
                    }
                }
                // 继续检查 wireGroups 中超出 bridgeGroups 范围的部分（只看 wires）
                for (int i = limit; i < ___wireGroups.Length; i++)
                {
                    List<Wire> wires = ___wireGroups[i];
                    if (wires != null && wires.Count > 0)
                    {
                        __result = Wire.GetMaxWattageAsFloat((Wire.WattageRating)i);
                        return false;
                    }
                }
                __result = 0f;
                return false;
            }
        }

        // ======================== 重型智能电池：堆叠上限 15 层 + 地基校验 ========================
        // 建筑以顶部 (0,2) 连接点逐层向上堆叠，相邻两层锚点相差 (0,2)。
        // 放置/建造校验时：
        //   1. 从候选锚点向下逐层(步进 -2)统计已存在的同类电池，达到 15 层则拒绝；
        //   2. 如果下方没有同类电池(即为底层)，检查 3 格宽地基是否为实心方块。
        // IsValidPlaceLocation(预览/下单) 与 IsValidBuildLocation 是两个独立入口，
        // 故两者都打 Postfix；任何异常都吞掉以免影响其他建筑放置。
        public static class StackBatteryLimit
        {
            public const int MAX_STACK = 15;

            public static void Apply(BuildingDef def, int cell, ref bool result, ref string fail_reason)
            {
                if (!result) return;
                if (def == null || def.PrefabID != StackBatteryConfig.ID) return;
                try
                {
                    // 统计下方同类电池层数
                    // 注意：Grid.Objects 在建筑占用的每个格子都返回该建筑，
                    // 所以必须用 PosToCell 取锚点坐标与预期位置比对，
                    // 防止空一格也被误判为"下方有电池"。
                    int below = cell;
                    int count = 0;
                    for (int i = 0; i < MAX_STACK + 2; i++)
                    {
                        below = Grid.OffsetCell(below, 0, -2);
                        if (!Grid.IsValidCell(below)) break;
                        GameObject go = Grid.Objects[below, (int)ObjectLayer.Building];
                        if (go == null) break;
                        KPrefabID kp = go.GetComponent<KPrefabID>();
                        if (kp == null || kp.PrefabTag != (Tag)StackBatteryConfig.ID) break;
                        int anchorCell = Grid.PosToCell(go.transform.GetPosition());
                        if (anchorCell != below) break;
                        count++;
                    }
                    if (count >= MAX_STACK)
                    {
                        result = false;
                        fail_reason = PowerExtension.Strings.EXT_BUILDINGS.PREFABS.STACKBATTERY.STACK_LIMIT_MSG;
                        return;
                    }

                    // 底层地基校验：下方无同类电池时，3 格宽底边必须有实心方块
                    if (count == 0)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            int groundCell = Grid.OffsetCell(cell, x, -1);
                            if (!Grid.IsValidCell(groundCell) || !Grid.Solid[groundCell])
                            {
                                result = false;
                                fail_reason = UI.TOOLTIPS.HELP_BUILDLOCATION_FLOOR;
                                return;
                            }
                        }
                    }
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(BuildingDef), nameof(BuildingDef.IsValidPlaceLocation),
            new System.Type[] { typeof(GameObject), typeof(int), typeof(Orientation), typeof(bool), typeof(string), typeof(bool) },
            new HarmonyLib.ArgumentType[] { HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Out, HarmonyLib.ArgumentType.Normal })]
        public class BuildingDef_IsValidPlaceLocation_StackLimit_Patch
        {
            public static void Postfix(BuildingDef __instance, int cell, ref bool __result, ref string fail_reason)
            {
                StackBatteryLimit.Apply(__instance, cell, ref __result, ref fail_reason);
            }
        }

        [HarmonyPatch(typeof(BuildingDef), nameof(BuildingDef.IsValidBuildLocation),
            new System.Type[] { typeof(GameObject), typeof(int), typeof(Orientation), typeof(bool), typeof(string) },
            new HarmonyLib.ArgumentType[] { HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Normal, HarmonyLib.ArgumentType.Out })]
        public class BuildingDef_IsValidBuildLocation_StackLimit_Patch
        {
            public static void Postfix(BuildingDef __instance, int cell, ref bool __result, ref string fail_reason)
            {
                StackBatteryLimit.Apply(__instance, cell, ref __result, ref fail_reason);
            }
        }

        // ======================== 建筑与科技注册 ========================
        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                ModUtil.AddBuildingToPlanScreen("Power", "MediumWire", "wires");
                ModUtil.AddBuildingToPlanScreen("Power", "MediumWireBridge", "wires");
                ModUtil.AddBuildingToPlanScreen("Power", "PowerTransformer2kW", "powercontrol");
                ModUtil.AddBuildingToPlanScreen("Power", "IonBatteryArray", "batteries");
                ModUtil.AddBuildingToPlanScreen("Power", "StackBattery", "batteries");
            }

            public static void Postfix()
            {
#pragma warning disable CS0618
                MoveBuildingAfter("Power", "WireRefinedHighWattage", "MediumWire");
                MoveBuildingAfter("Power", "MediumWire", "MediumWireBridge");
                MoveBuildingAfter("Power", "PowerTransformerHeavy", "PowerTransformer2kW");
                MoveBuildingAfter("Power", "BatterySmart", "IonBatteryArray");
                MoveBuildingAfter("Power", "IonBatteryArray", "StackBattery");
#pragma warning restore CS0618
            }

            private static void MoveBuildingAfter(HashedString category, string targetId, string buildingId)
            {
                int categoryIndex = TUNING.BUILDINGS.PLANORDER.FindIndex(x => x.category == category);
                if (categoryIndex == -1) return;

                var planList = TUNING.BUILDINGS.PLANORDER[categoryIndex].data as IList<string>;
                if (planList == null) return;

                int targetIndex = planList.IndexOf(targetId);
                int myIndex = planList.IndexOf(buildingId);

                if (targetIndex != -1 && myIndex != -1)
                {
                    planList.RemoveAt(myIndex);
                    targetIndex = planList.IndexOf(targetId);
                    planList.Insert(targetIndex + 1, buildingId);
                }
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        public class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                string techGroup = "RenewableEnergy";
                var techList = Db.Get().Techs.Get(techGroup).unlockedItemIDs;

                if (!techList.Contains("MediumWire")) techList.Add("MediumWire");
                if (!techList.Contains("MediumWireBridge")) techList.Add("MediumWireBridge");
                if (!techList.Contains("PowerTransformer2kW")) techList.Add("PowerTransformer2kW");
                if (!techList.Contains("AirBattery")) techList.Add("AirBattery");
                if (!techList.Contains("EmptyAirBattery")) techList.Add("EmptyAirBattery");
                if (!techList.Contains("IonBatteryArray")) techList.Add("IonBatteryArray");

                // 堆叠蓄电池：与原版蓄电池舱同节点(太空电力)解锁
                var spacePowerTech = Db.Get().Techs.Get("SpacePower").unlockedItemIDs;
                if (!spacePowerTech.Contains("StackBattery")) spacePowerTech.Add("StackBattery");

                AddBatteryRecipes();
            }

            private static void AddBatteryRecipes()
            {
                string fabricatorId = "AdvancedCraftingTable";

                Tag batteryTag = "AirBattery".ToTag();
                Tag emptyBatteryTag = "EmptyAirBattery".ToTag();
                Tag graphiteTag = SimHashes.Graphite.CreateTag();

                // 构建安全的金属数组
                List<Tag> metalList = new List<Tag> {
                    SimHashes.Aluminum.CreateTag(),
                    SimHashes.Iron.CreateTag()      // [新增] 增加精炼铁
                    };
                Tag zincTag = SimHashes.Zinc.CreateTag();
                if (ElementLoader.FindElementByHash(SimHashes.Zinc) != null)
                {
                    metalList.Add(zincTag);
                }
                Tag[] validMetals = metalList.ToArray();

                // 构建安全的橡胶/塑料数组（与高压导线一致）
                List<Tag> plasticList = new List<Tag> { SimHashes.Polypropylene.CreateTag() };
                if (ElementLoader.FindElementByHash(SimHashes.Rubber) != null)
                {
                    plasticList.Add(SimHashes.Rubber.CreateTag());
                }
                Tag bioPlasticTag = new Tag("BioPlastic");
                if (ElementLoader.GetElement(bioPlasticTag) != null || Assets.GetPrefab(bioPlasticTag) != null)
                {
                    plasticList.Add(bioPlasticTag);
                }
                Tag[] validPlastics = plasticList.ToArray();

                // 注册"全新电池"配方
                string recipeId1 = ComplexRecipeManager.MakeRecipeID(fabricatorId,
                    new ComplexRecipe.RecipeElement[] {
                        new ComplexRecipe.RecipeElement(validMetals, 600f),
                        new ComplexRecipe.RecipeElement(graphiteTag, 10f),
                        new ComplexRecipe.RecipeElement(validPlastics, 50f)
                    },
                    new ComplexRecipe.RecipeElement[] { new ComplexRecipe.RecipeElement(batteryTag, 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature) });

                if (ComplexRecipeManager.Get().GetRecipe(recipeId1) == null)
                {
                    new ComplexRecipe(recipeId1,
                        new ComplexRecipe.RecipeElement[] {
                            new ComplexRecipe.RecipeElement(validMetals, 600f),
                            new ComplexRecipe.RecipeElement(graphiteTag, 10f),
                            new ComplexRecipe.RecipeElement(validPlastics, 50f)
                        },
                        new ComplexRecipe.RecipeElement[] { new ComplexRecipe.RecipeElement(batteryTag, 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature) })
                    {
                        time = 90f,
                        description = "制造全新的空气燃料电源。",
                        nameDisplay = ComplexRecipe.RecipeNameDisplay.Result,
                        fabricators = new List<Tag> { fabricatorId },
                        sortOrder = -999
                    };
                }

                // 注册"补充电池"配方
                string recipeId2 = ComplexRecipeManager.MakeRecipeID(fabricatorId,
                    new ComplexRecipe.RecipeElement[] {
                        new ComplexRecipe.RecipeElement(emptyBatteryTag, 1f),
                        new ComplexRecipe.RecipeElement(validMetals, 600f)
                    },
                    new ComplexRecipe.RecipeElement[] { new ComplexRecipe.RecipeElement(batteryTag, 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature) });

                if (ComplexRecipeManager.Get().GetRecipe(recipeId2) == null)
                {
                    new ComplexRecipe(recipeId2,
                        new ComplexRecipe.RecipeElement[] {
                            new ComplexRecipe.RecipeElement(emptyBatteryTag, 1f),
                            new ComplexRecipe.RecipeElement(validMetals, 600f)
                        },
                        new ComplexRecipe.RecipeElement[] { new ComplexRecipe.RecipeElement(batteryTag, 1f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature) })
                    {
                        time = 40f,
                        description = "补充耗尽的空气燃料电源。",
                        nameDisplay = ComplexRecipe.RecipeNameDisplay.Custom,
                        customName = "空气燃料电池(补充)",
                        fabricators = new List<Tag> { fabricatorId },
                        sortOrder = -998
                    };
                }
            }
        }

        // ======================== 空气燃料电源逻辑补丁 ========================
        [HarmonyPatch(typeof(LargeElectrobankDischargerConfig), "ConfigureBuildingTemplate")]
        public class LargeElectrobankDischargerConfig_Patch
        {
            public static void Postfix(GameObject go)
            {
                Storage storage = go.GetComponent<Storage>();
                if (storage != null && storage.capacityKg < 250f) storage.capacityKg = 250f;
            }
        }

        [HarmonyPatch(typeof(SmallElectrobankDischargerConfig), "ConfigureBuildingTemplate")]
        public class SmallElectrobankDischargerConfig_Patch
        {
            public static void Postfix(GameObject go)
            {
                Storage storage = go.GetComponent<Storage>();
                if (storage != null && storage.capacityKg < 250f) storage.capacityKg = 250f;
            }
        }

        [HarmonyPatch(typeof(Electrobank), "get_Charge")]
        public class Electrobank_get_Charge_Patch
        {
            public static bool Prefix(Electrobank __instance, ref float __result)
            {
                if (__instance is AirBattery battery)
                {
                    __result = battery.customCharge;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Electrobank), "RemovePower")]
        public class Electrobank_RemovePower_Patch
        {
            public static bool Prefix(Electrobank __instance, float joules, bool dropWhenEmpty, ref float __result)
            {
                if (__instance is AirBattery battery)
                {
                    __result = battery.RemovePowerInternal(joules, dropWhenEmpty);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Electrobank), "AddPower")]
        public class Electrobank_AddPower_Patch
        {
            public static bool Prefix(Electrobank __instance, ref float __result)
            {
                if (__instance is AirBattery) { __result = 0f; return false; }
                return true;
            }
        }

        [HarmonyPatch(typeof(Electrobank), "get_IsFullyCharged")]
        public class Electrobank_get_IsFullyCharged_Patch
        {
            public static bool Prefix(Electrobank __instance, ref bool __result)
            {
                if (__instance is AirBattery battery)
                {
                    __result = battery.IsFullyChargedInternal;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Electrobank), "Sim1000ms")]
        public class Electrobank_Sim1000ms_Patch
        {
            public static bool Prefix(Electrobank __instance)
            {
                if (__instance is AirBattery) return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(Electrobank), "get_Display")]
        public class Electrobank_get_Display_Patch
        {
            public static bool Prefix(Electrobank __instance, ref bool __result)
            {
                if (__instance is AirBattery) { __result = false; return false; }
                return true;
            }
        }

        // [2026-05-28] U59 InternalRefresh 已改用 WorldResourceAmountTracker<ElectrobankTracker>
        // 计算总焦耳；该 tracker 默认按 120kJ × 单位数估算，无法识别 AirBattery
        // 的自定义容量。Postfix 用 Components.Electrobanks 重新精确计算并覆盖。
        [HarmonyPatch(typeof(MeterScreen_Electrobanks), "InternalRefresh")]
        public class MeterScreen_Electrobanks_InternalRefresh_Patch
        {
            public static void Postfix(MeterScreen_Electrobanks __instance)
            {
#pragma warning disable CS0618
                if (!SaveLoader.Instance.IsDLCActiveForCurrentSave("DLC3_ID")) return;
#pragma warning restore CS0618

                long trueJoules = 0;
                var items = Components.Electrobanks.GetItems(ClusterManager.Instance.activeWorldId);
                if (items != null)
                {
                    foreach (var bank in items)
                    {
                        if (bank == null) continue;
                        var primary = bank.GetComponent<PrimaryElement>();
                        float units = primary != null ? primary.Units : 1f;
                        trueJoules += (long)(bank.Charge * units);
                    }
                }

                var cachedJoulesField = Traverse.Create(__instance).Field("cachedJoules");
                long cachedJoules = cachedJoulesField.GetValue<long>();

                if (cachedJoules != trueJoules)
                {
                    __instance.Label.text = GameUtil.GetFormattedJoules(trueJoules);
                    cachedJoulesField.SetValue(trueJoules);
                }

                List<MinionIdentity> minions = Traverse.Create(__instance).Method("GetWorldMinionIdentities").GetValue<List<MinionIdentity>>();
                int minionCount = minions != null ? minions.Count : 0;

                if (__instance.diagnosticGraph != null)
                {
                    var sparkLayer = __instance.diagnosticGraph.GetComponentInChildren<SparkLayer>();
                    if (sparkLayer != null)
                    {
                        sparkLayer.SetColor(
                            ((float)trueJoules > (float)minionCount * 120000f) ? Constants.NEUTRAL_COLOR : Constants.NEGATIVE_COLOR
                        );
                    }
                }
            }
        }

        // [2026-05-28] U59 OnTooltip 改用 per_electrobankType_UnitCount_Dictionary
        // (单位计数语义，× 120kJ 估算)。原 joulesDictionary 字段不再存在。
        // Postfix 完全清空原版已添加 tooltip，按 prefab 重建，焦耳数取真实值。
        [HarmonyPatch(typeof(MeterScreen_Electrobanks), "OnTooltip")]
        public class MeterScreen_Electrobanks_OnTooltip_Patch
        {
            public static void Postfix(MeterScreen_Electrobanks __instance)
            {
#pragma warning disable CS0618
                if (!SaveLoader.Instance.IsDLCActiveForCurrentSave("DLC3_ID")) return;
#pragma warning restore CS0618

                long trueJoules = 0;
                int powerBanksAvailable = 0;
                var perPrefabJoules = new Dictionary<string, float>();

                var items = Components.Electrobanks.GetItems(ClusterManager.Instance.activeWorldId);
                if (items != null)
                {
                    foreach (var bank in items)
                    {
                        if (bank == null) continue;
                        var primary = bank.GetComponent<PrimaryElement>();
                        float units = primary != null ? primary.Units : 1f;
                        float j = bank.Charge * units;
                        trueJoules += (long)j;
                        powerBanksAvailable++;

                        string id = bank.gameObject.PrefabID().ToString();
                        if (!perPrefabJoules.ContainsKey(id)) perPrefabJoules[id] = 0f;
                        perPrefabJoules[id] += j;
                    }
                }

                string formattedJoules = GameUtil.GetFormattedJoules(trueJoules);
                __instance.Label.text = formattedJoules;
                __instance.Tooltip.ClearMultiStringTooltip();

                string tooltipHeader;
                string perCycleMinimum = GameUtil.GetFormattedJoules(120000f);
                try
                {
                    tooltipHeader = string.Format(
                        global::STRINGS.UI.TOOLTIPS.METERSCREEN_ELECTROBANK_JOULES,
                        formattedJoules,
                        perCycleMinimum,
                        GameUtil.GetFormattedUnits(powerBanksAvailable));
                }
                catch (System.FormatException)
                {
                    try
                    {
                        tooltipHeader = string.Format(
                            global::STRINGS.UI.TOOLTIPS.METERSCREEN_ELECTROBANK_JOULES,
                            formattedJoules);
                    }
                    catch (System.FormatException)
                    {
                        tooltipHeader = $"Joules Available: {formattedJoules}";
                    }
                }

                __instance.Tooltip.AddMultiStringTooltip(tooltipHeader, __instance.ToolTipStyle_Header);
                __instance.Tooltip.AddMultiStringTooltip("", __instance.ToolTipStyle_Property);

                foreach (var kv in perPrefabJoules.OrderByDescending(x => x.Value))
                {
                    GameObject prefab = Assets.GetPrefab(kv.Key);
                    if (prefab != null)
                    {
                        __instance.Tooltip.AddMultiStringTooltip(
                            $"{prefab.GetProperName()}: {GameUtil.GetFormattedJoules(kv.Value)}",
                            __instance.ToolTipStyle_Property);
                    }
                    else
                    {
                        string invalidTypeText;
                        try
                        {
                            invalidTypeText = string.Format(global::STRINGS.UI.TOOLTIPS.METERSCREEN_INVALID_ELECTROBANK_TYPE, kv.Key);
                        }
                        catch (System.FormatException)
                        {
                            invalidTypeText = $"Invalid Power Bank Type: {kv.Key}";
                        }
                        __instance.Tooltip.AddMultiStringTooltip(invalidTypeText, __instance.ToolTipStyle_Property);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Database.BuildingStatusItems), "CreateStatusItems")]
        public class BuildingStatusItems_CreateStatusItems_Patch
        {
            public static void Postfix(Database.BuildingStatusItems __instance)
            {
                var originalCallback = __instance.ElectrobankJoulesAvailable.resolveStringCallback;
                __instance.ElectrobankJoulesAvailable.resolveStringCallback = (string str, object data) =>
                {
                    string result = originalCallback(str, data);
                    if (data is ElectrobankDischarger ed)
                    {
                        float maxJoules = 0f;
                        var storage = ed.GetComponent<Storage>();
                        if (storage != null && storage.items != null)
                        {
                            foreach (GameObject go in storage.items)
                            {
                                if (go == null) continue;
                                var bank = go.GetComponent<Electrobank>();
                                if (bank is AirBattery customBattery)
                                    maxJoules += customBattery.MaxCapacity;
                                else if (bank != null)
                                    maxJoules += 120000f;
                            }
                        }
                        if (maxJoules > 0 && maxJoules != 120000f)
                        {
                            string oldMaxStr = GameUtil.GetFormattedJoules(120000f, "F1", GameUtil.TimeSlice.None);
                            string newMaxStr = GameUtil.GetFormattedJoules(maxJoules, "F1", GameUtil.TimeSlice.None);
                            result = result.Replace(oldMaxStr, newMaxStr);
                        }
                    }
                    return result;
                };
            }
        }

        [HarmonyPatch(typeof(ElectrobankDischarger), "UpdateMeter")]
        public class ElectrobankDischarger_UpdateMeter_Patch
        {
            public static bool Prefix(ElectrobankDischarger __instance, ref MeterController ___meterController)
            {
                if (___meterController == null)
                {
                    ___meterController = new MeterController(__instance.GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer, System.Array.Empty<string>());
                }

                float maxJoules = 0f;
                var storage = __instance.GetComponent<Storage>();
                if (storage != null && storage.items != null)
                {
                    foreach (GameObject go in storage.items)
                    {
                        if (go == null) continue;
                        var bank = go.GetComponent<Electrobank>();
                        if (bank is AirBattery customBattery)
                            maxJoules += customBattery.MaxCapacity;
                        else if (bank != null)
                            maxJoules += 120000f;
                    }
                }

                if (maxJoules == 0f) maxJoules = 120000f;

                ___meterController.SetPositionPercent(__instance.ElectrobankJoulesStored / maxJoules);
                return false;
            }
        }
    }
}
