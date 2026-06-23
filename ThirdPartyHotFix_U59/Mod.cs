// ============================================================
// 模块: Mod.cs
// 描述: ThirdPartyHotFix (U59) 入口
//       临时修复以下两个第三方模组在 ONI U59 (Aquatic DLC Beta) 下
//       的异常风暴，避免 Player.log 被刷爆并导致 OverlayScreen 连锁 NRE：
//       1) Stairs.Patches+PathProbeTask_Patch.Prefix
//          —— 在 worker 线程里通过 Grid.PosToCell(KMonoBehaviour cmp)
//             访问 navigator.transform，触发 Unity
//             "get_transform can only be called from the main thread"
//          —— 处理：直接 Unpatch
//       2) BetterInfoCards.ExportSelectToolData+GetSelectInfo_Patch.ExportGO(string)
//          —— transpiler 注入的 ExportGO 在 curSelectable == null 时 NRE
//          —— 处理：Prefix 加 null-guard，curSelectable 为空时直接 return false
// 创建: 2026-05-28
// ============================================================
using HarmonyLib;
using KMod;

namespace ThirdPartyHotFix
{
    public class Mod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            TrueTilesLoadHotFix.Install(harmony);
            base.OnLoad(harmony);
            harmony.PatchAll();
        }
    }
}
