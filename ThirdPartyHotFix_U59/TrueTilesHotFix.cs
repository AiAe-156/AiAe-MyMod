using HarmonyLib;
using Rendering;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ThirdPartyHotFix
{
    internal static class TrueTilesLoadHotFix
    {
        private static bool _installed;
        private static bool _addBlockPatched;
        private static bool _renderInfoPatched;
        private static bool _assetsPatched;
        private static readonly string[] TargetPatchTypes =
        {
            "TrueTiles.Patches.BlockTileRendererPatch+Rendering_BlockTileRenderer_AddBlock_Patch",
            "TrueTiles.Patches.RenderInfoPatch+RenderInfo_Ctor_Patch",
            "TrueTiles.Patches.AssetsPatch+Assets_OnPrefabInit_Patch"
        };

        public static void Install(Harmony harmony)
        {
            if (_installed) return;
            _installed = true;

            var processorPatch = AccessTools.Method(typeof(Harmony).Assembly.GetType("HarmonyLib.PatchClassProcessor"), "Patch");
            var finalizer = AccessTools.Method(typeof(TrueTilesLoadHotFix), nameof(PatchClassProcessorPatch_Finalizer));
            if (processorPatch == null || finalizer == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] TrueTiles early hook unavailable");
                return;
            }

            harmony.Patch(processorPatch, finalizer: new HarmonyMethod(finalizer) { priority = Priority.First });
            Debug.Log("[ThirdPartyHotFix] Installed TrueTiles early patch hook");
        }

        public static Exception PatchClassProcessorPatch_Finalizer(object __instance, Exception __exception)
        {
            if (__instance == null) return __exception;

            var patchType = GetPatchType(__instance);
            if (patchType == null || !TargetPatchTypes.Contains(patchType.FullName)) return __exception;

            try
            {
                var harmony = GetHarmony(__instance);
                if (patchType.FullName.EndsWith("Rendering_BlockTileRenderer_AddBlock_Patch"))
                {
                    return PatchTrueTilesAddBlock(harmony, patchType) ? null : __exception;
                }

                if (patchType.FullName.EndsWith("RenderInfo_Ctor_Patch"))
                {
                    return PatchTrueTilesRenderInfo(harmony, patchType) ? null : __exception;
                }

                if (patchType.FullName.EndsWith("Assets_OnPrefabInit_Patch"))
                {
                    return PatchTrueTilesAssetsOnPrefabInit(harmony, patchType.Assembly) ? null : __exception;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ThirdPartyHotFix] TrueTiles fallback patch failed: " + e);
            }

            return __exception;
        }

        public static void PatchLoadedTrueTiles(Harmony harmony)
        {
            var trueTiles = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "TrueTiles");
            if (trueTiles == null)
            {
                Debug.Log("[ThirdPartyHotFix] TrueTiles assembly not loaded, skip");
                return;
            }

            try
            {
                var addBlockPatch = trueTiles.GetType("TrueTiles.Patches.BlockTileRendererPatch+Rendering_BlockTileRenderer_AddBlock_Patch");
                if (addBlockPatch != null) PatchTrueTilesAddBlock(harmony, addBlockPatch);
            }
            catch (Exception e) { Debug.LogError("[ThirdPartyHotFix] TrueTiles AddBlock fallback failed: " + e); }

            try
            {
                var renderInfoPatch = trueTiles.GetType("TrueTiles.Patches.RenderInfoPatch+RenderInfo_Ctor_Patch");
                if (renderInfoPatch != null) PatchTrueTilesRenderInfo(harmony, renderInfoPatch);
            }
            catch (Exception e) { Debug.LogError("[ThirdPartyHotFix] TrueTiles RenderInfo fallback failed: " + e); }

            try { PatchTrueTilesAssetsOnPrefabInit(harmony, trueTiles); }
            catch (Exception e) { Debug.LogError("[ThirdPartyHotFix] TrueTiles Assets guard failed: " + e); }
        }

        private static Type GetPatchType(object patchClassProcessor)
        {
            return Traverse.Create(patchClassProcessor).Field<Type>("containerType").Value;
        }

        private static Harmony GetHarmony(object patchClassProcessor)
        {
            return Traverse.Create(patchClassProcessor).Field<Harmony>("instance").Value;
        }

        private static bool PatchTrueTilesAddBlock(Harmony harmony, Type patchType)
        {
            if (_addBlockPatched) return true;
            var transpiler = AccessTools.Method(patchType, "Transpiler");
            var addBlock = AccessTools.Method(typeof(BlockTileRenderer), "AddBlock", new[]
            {
                typeof(int),
                typeof(BuildingDef),
                typeof(bool),
                typeof(SimHashes),
                typeof(int),
                typeof(bool)
            });

            if (harmony == null || transpiler == null || addBlock == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] TrueTiles AddBlock fallback target missing");
                return false;
            }

            harmony.Patch(addBlock, transpiler: new HarmonyMethod(transpiler));
            _addBlockPatched = true;
            Debug.Log("[ThirdPartyHotFix] Patched TrueTiles AddBlock transpiler to U59 6-arg overload");
            return true;
        }

        private static bool PatchTrueTilesRenderInfo(Harmony harmony, Type patchType)
        {
            if (_renderInfoPatched) return true;
            var postfix = AccessTools.Method(patchType, "Postfix");
            var renderInfoType = AccessTools.TypeByName("Rendering.BlockTileRenderer+RenderInfo");
            var constructor = AccessTools.Constructor(renderInfoType, new[]
            {
                typeof(BlockTileRenderer),
                typeof(int),
                typeof(int),
                typeof(BuildingDef),
                typeof(SimHashes),
                typeof(bool)
            }, false);

            if (harmony == null || postfix == null || constructor == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] TrueTiles RenderInfo fallback target missing");
                return false;
            }

            harmony.Patch(constructor, postfix: new HarmonyMethod(postfix));
            _renderInfoPatched = true;
            Debug.Log("[ThirdPartyHotFix] Patched TrueTiles RenderInfo postfix to U59 6-arg constructor");
            return true;
        }

        private static bool PatchTrueTilesAssetsOnPrefabInit(Harmony harmony, Assembly trueTiles)
        {
            if (_assetsPatched) return true;
            var patchType = trueTiles.GetType("TrueTiles.Patches.AssetsPatch+Assets_OnPrefabInit_Patch");
            var postfix = AccessTools.Method(patchType, "Postfix");
            var safePostfix = AccessTools.Method(typeof(TrueTilesAssetsHotFixHelper), nameof(TrueTilesAssetsHotFixHelper.Postfix));
            var finalizer = AccessTools.Method(typeof(TrueTilesAssetsHotFixHelper), nameof(TrueTilesAssetsHotFixHelper.Finalizer));
            var target = AccessTools.Method(typeof(Assets), "OnPrefabInit");
            if (harmony == null || postfix == null || safePostfix == null || finalizer == null || target == null)
            {
                Debug.LogWarning("[ThirdPartyHotFix] TrueTiles Assets postfix not found");
                return false;
            }

            var patches = Harmony.GetPatchInfo(target);
            if (patches != null)
            {
                foreach (var patch in patches.Postfixes.Where(p => p.PatchMethod?.DeclaringType?.Assembly == trueTiles).ToArray())
                {
                    harmony.Unpatch(target, patch.PatchMethod);
                    Debug.Log("[ThirdPartyHotFix] Removed TrueTiles Assets.OnPrefabInit postfix owner=" + patch.owner);
                }
            }

            harmony.Patch(target,
                postfix: new HarmonyMethod(safePostfix) { priority = Priority.Last },
                finalizer: new HarmonyMethod(finalizer) { priority = Priority.First });
            _assetsPatched = true;
            Debug.Log("[ThirdPartyHotFix] Replaced TrueTiles Assets.OnPrefabInit postfix with guarded wrapper and finalizer");
            return true;
        }
    }

    public static class TrueTilesAssetsHotFixHelper
    {
        public static Exception Finalizer(Exception __exception)
        {
            if (__exception == null) return null;
            if (__exception is NullReferenceException && __exception.StackTrace != null && __exception.StackTrace.Contains("TrueTiles.Patches.AssetsPatch+Assets_OnPrefabInit_Patch.Postfix"))
            {
                Debug.LogWarning("[ThirdPartyHotFix] Suppressed TrueTiles Assets.OnPrefabInit NullReferenceException");
                return null;
            }
            return __exception;
        }

        public static void Postfix()
        {
            try
            {
                var loaderType = AccessTools.TypeByName("TrueTiles.Cmps.TileAssetLoader");
                var loader = Traverse.Create(loaderType).Field("Instance").GetValue();
                if (loader == null)
                {
                    Debug.LogWarning("[ThirdPartyHotFix] Skip TrueTiles LoadOverrides: TileAssetLoader.Instance is null");
                    return;
                }

                AccessTools.Method(loaderType, "LoadOverrides")?.Invoke(loader, null);

                var modType = AccessTools.TypeByName("TrueTiles.Mod");
                var harmony = Traverse.Create(modType).Field("harmonyInstance").GetValue<Harmony>();
                var patchType = AccessTools.TypeByName("TrueTiles.Patches.GamePatch+Game_OnSpawn_Patch");
                AccessTools.Method(patchType, "Patch")?.Invoke(null, new object[] { harmony });
            }
            catch (Exception e)
            {
                Debug.LogError("[ThirdPartyHotFix] TrueTiles guarded Assets postfix failed: " + e);
            }
        }
    }
}
