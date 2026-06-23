// ============================================================
// 模块: HotFixPatches.cs
// 描述: 在 Db.Initialize 完成后扫描已加载程序集，
//       Unpatch / 重新 Patch 两个有问题的第三方 patch。
//       此时机晚于所有 mod 的 UserMod2.OnLoad，且早于
//       Game.OnPrefabInit / AsyncPathProber 启动，安全可靠。
// ============================================================
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ThirdPartyHotFix
{
    [HarmonyPatch(typeof(Db), "Initialize")]
    public static class Db_Initialize_HotFix
    {
        private static bool _applied;

        [HarmonyPriority(Priority.Last)]
        public static void Postfix()
        {
            if (_applied) return;
            _applied = true;

            var harmony = new Harmony("aiae.oni.thirdpartyhotfix");

            try { UnpatchStairsPathProbe(harmony); }
            catch (Exception e) { Debug.LogError("[ThirdPartyHotFix] Stairs unpatch failed: " + e); }

            try { GuardBetterInfoCardsExportGO(harmony); }
            catch (Exception e) { Debug.LogError("[ThirdPartyHotFix] BetterInfoCards guard failed: " + e); }

            try { TrueTilesLoadHotFix.PatchLoadedTrueTiles(harmony); }
            catch (Exception e) { Debug.LogError("[ThirdPartyHotFix] TrueTiles hotfix failed: " + e); }

            try { GuardElementConsumerAddMass(harmony); }
            catch (Exception e) { Debug.LogError("[ThirdPartyHotFix] ElementConsumer guard failed: " + e); }

            try { GuardWorldGenSpawnerSpawnable(harmony); }
            catch (Exception e) { Debug.LogError("[ThirdPartyHotFix] WorldGenSpawner guard failed: " + e); }
        }

        // ----------------- Stairs -----------------
        private static void UnpatchStairsPathProbe(Harmony harmony)
        {
            var stairsAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Stairs");
            if (stairsAsm == null)
            {
                Debug.Log("[ThirdPartyHotFix] Stairs assembly not loaded, skip");
                return;
            }

            // 嵌套类反射名: Outer+Inner
            var patchType = stairsAsm.GetType("Stairs.Patches+PathProbeTask_Patch");
            if (patchType == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] Stairs.Patches+PathProbeTask_Patch type not found");
                return;
            }
            var prefix = AccessTools.Method(patchType, "Prefix");
            if (prefix == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] Stairs PathProbeTask_Patch.Prefix not found");
                return;
            }

            // 目标方法: AsyncPathProber+Manager.NextTask (嵌套类)
            var managerType = typeof(AsyncPathProber).GetNestedType("Manager",
                BindingFlags.Public | BindingFlags.NonPublic);
            if (managerType == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] AsyncPathProber+Manager type not found");
                return;
            }
            var nextTask = AccessTools.Method(managerType, "NextTask");
            if (nextTask == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] AsyncPathProber.Manager.NextTask not found");
                return;
            }

            harmony.Unpatch(nextTask, prefix);
            Debug.Log("[ThirdPartyHotFix] Unpatched Stairs.PathProbeTask_Patch.Prefix on AsyncPathProber.Manager.NextTask");
        }

        private static void GuardElementConsumerAddMass(Harmony harmony)
        {
            var addMass = AccessTools.Method(typeof(ElementConsumer), "AddMass", new[] { typeof(Sim.ConsumedMassInfo) });
            var prefix = AccessTools.Method(typeof(ElementConsumerHotFixHelper), nameof(ElementConsumerHotFixHelper.AddMass_Prefix));
            ElementConsumerHotFixHelper.HandleInstanceMapField = AccessTools.Field(typeof(ElementConsumer), "handleInstanceMap");
            ElementConsumerHotFixHelper.StorageField = AccessTools.Field(typeof(ElementConsumer), "storage");
            if (addMass == null || prefix == null || ElementConsumerHotFixHelper.HandleInstanceMapField == null || ElementConsumerHotFixHelper.StorageField == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] ElementConsumer.AddMass guard target missing");
                return;
            }

            harmony.Patch(addMass, prefix: new HarmonyMethod(prefix) { priority = Priority.First });
            Debug.Log("[ThirdPartyHotFix] Guarded ElementConsumer.AddMass against destroyed storage callbacks");
        }

        private static void GuardWorldGenSpawnerSpawnable(Harmony harmony)
        {
            var trySpawn = AccessTools.Method(typeof(WorldGenSpawner.Spawnable), "TrySpawn");
            var prefix = AccessTools.Method(typeof(WorldGenSpawnerHotFixHelper), nameof(WorldGenSpawnerHotFixHelper.TrySpawn_Prefix));
            if (trySpawn == null || prefix == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] WorldGenSpawner.Spawnable.TrySpawn guard target missing");
                return;
            }

            harmony.Patch(trySpawn, prefix: new HarmonyMethod(prefix) { priority = Priority.First });
            Debug.Log("[ThirdPartyHotFix] Guarded WorldGenSpawner.Spawnable.TrySpawn against null prefab ids");
        }

        internal static FieldInfo BicCurSelectableField;

        private static void GuardBetterInfoCardsExportGO(Harmony harmony)
        {
            var bicAsm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "BetterInfoCards");
            if (bicAsm == null)
            {
                Debug.Log("[ThirdPartyHotFix] BetterInfoCards assembly not loaded, skip");
                return;
            }

            var holderType = bicAsm.GetType("BetterInfoCards.ExportSelectToolData");
            if (holderType == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] BetterInfoCards.ExportSelectToolData not found");
                return;
            }

            var patchType = bicAsm.GetType("BetterInfoCards.ExportSelectToolData+GetSelectInfo_Patch");
            if (patchType == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] BetterInfoCards GetSelectInfo_Patch not found");
                return;
            }

            // ExportGO 是 private static
            var exportGO = patchType.GetMethod("ExportGO",
                BindingFlags.NonPublic | BindingFlags.Static,
                null, new[] { typeof(string) }, null);
            if (exportGO == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] ExportGO(string) not found");
                return;
            }

            BicCurSelectableField = AccessTools.Field(holderType, "curSelectable");
            if (BicCurSelectableField == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] curSelectable field not found");
                return;
            }

            var prefix = AccessTools.Method(typeof(BetterInfoCardsHotFixHelper),
                nameof(BetterInfoCardsHotFixHelper.ExportGO_Prefix));
            harmony.Patch(exportGO, prefix: new HarmonyMethod(prefix) { priority = Priority.First });
            Debug.Log("[ThirdPartyHotFix] Guarded BetterInfoCards.ExportGO with null-check prefix");
        }
    }

    public static class WorldGenSpawnerHotFixHelper
    {
        public static bool TrySpawn_Prefix(WorldGenSpawner.Spawnable __instance)
        {
            try
            {
                var spawnInfo = __instance?.spawnInfo;
                if (spawnInfo == null) return true;
                if (!string.IsNullOrEmpty(spawnInfo.id)) return true;

                Debug.LogWarning("[ThirdPartyHotFix] Skip WorldGenSpawner null prefab id at cell=" + __instance.cell + " type=" + spawnInfo.type + " element=" + spawnInfo.element + " pos=(" + spawnInfo.location_x + "," + spawnInfo.location_y + ")");
                __instance.FreeResources();
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError("[ThirdPartyHotFix] WorldGenSpawner.TrySpawn guard failed open: " + e);
                return true;
            }
        }
    }

    public static class ElementConsumerHotFixHelper
    {
        internal static FieldInfo HandleInstanceMapField;
        internal static FieldInfo StorageField;

        public static bool AddMass_Prefix(Sim.ConsumedMassInfo consumed_info)
        {
            try
            {
                if (!Sim.IsValidHandle(consumed_info.simHandle)) return true;
                var map = HandleInstanceMapField?.GetValue(null) as System.Collections.Generic.Dictionary<int, ElementConsumer>;
                if (map == null || !map.TryGetValue(consumed_info.simHandle, out var consumer)) return true;

                if (ReferenceEquals(consumer, null) || consumer == null)
                {
                    map.Remove(consumed_info.simHandle);
                    Debug.LogWarning("[ThirdPartyHotFix] Skip ElementConsumer.AddMass: consumer is destroyed");
                    return false;
                }

                var storage = StorageField.GetValue(consumer) as Storage;
                if (ReferenceEquals(storage, null) || storage == null)
                {
                    map.Remove(consumed_info.simHandle);
                    Debug.LogWarning("[ThirdPartyHotFix] Skip ElementConsumer.AddMass: storage is destroyed");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[ThirdPartyHotFix] ElementConsumer.AddMass guard failed open: " + e);
                return true;
            }
        }
    }

    // 必须独立非泛型类，HarmonyMethod 反射调用要求 public static
    public static class BetterInfoCardsHotFixHelper
    {
        public static bool ExportGO_Prefix()
        {
            try
            {
                var f = Db_Initialize_HotFix.BicCurSelectableField;
                if (f == null) return true;
                var v = f.GetValue(null);
                // null 或 已销毁 Unity Object 都跳过 body, 避免 NRE
                if (v == null) return false;
                if (v is UnityEngine.Object uo && uo == null) return false;
                return true;
            }
            catch
            {
                // 防御：任何反射异常直接跳过 body 比让 NRE 风暴好
                return false;
            }
        }
    }
}
