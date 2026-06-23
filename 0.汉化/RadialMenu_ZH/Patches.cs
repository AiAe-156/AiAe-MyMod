using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine.UI;

namespace RadialMenuZH
{
    /// <summary>
    /// 在所有 mod 加载完成后（Localization.Initialize 之后）一次性安装汉化：
    ///   1) 覆盖 Radial Menu 的键位名为中文；
    ///   2) 给 RadialMenuController.UpdateUI 挂 Postfix，在显示层翻译菜单文字。
    /// </summary>
    [HarmonyPatch(typeof(Localization), "Initialize")]
    public static class Localization_Initialize_Patch
    {
        private static bool installed = false;

        public static void Postfix()
        {
            // 键位名：原 mod 在自己的 OnLoad 里 Strings.Add 了英文，这里后写覆盖即可生效（后写者赢）。
            // 即使语言切换导致 Initialize 再次触发，这里幂等重写也没问题。
            Strings.Add(new string[] { "STRINGS.INPUT_BINDINGS.RADIALMENU.NAME", "径向菜单" });
            Strings.Add(new string[] { "STRINGS.INPUT_BINDINGS.RADIALMENU.800", "打开径向菜单" });

            if (!installed)
                installed = TryInstallMenuPatch();
        }

        private static bool TryInstallMenuPatch()
        {
            try
            {
                Type t = ResolveType("RadialMenuController");
                if (t == null)
                {
                    UnityEngine.Debug.Log("[RadialMenu_ZH] 未找到 RadialMenuController：可能未启用 Radial Menu 或其版本已更新，跳过汉化（不影响游戏）。");
                    return false;
                }

                MethodInfo updateUI = AccessTools.Method(t, "UpdateUI");
                if (updateUI == null)
                {
                    UnityEngine.Debug.Log("[RadialMenu_ZH] 未找到 UpdateUI 方法：Radial Menu 内部结构可能已更新，跳过汉化（不影响游戏）。");
                    return false;
                }

                if (!RadialMenuTranslatePatch.CacheFields(t))
                {
                    UnityEngine.Debug.Log("[RadialMenu_ZH] 缺少关键字段（centerText/outerTexts）：结构可能已更新，跳过汉化（不影响游戏）。");
                    return false;
                }

                HarmonyMethod postfix = new HarmonyMethod(
                    typeof(RadialMenuTranslatePatch),
                    nameof(RadialMenuTranslatePatch.Postfix));
                new Harmony("com.aiae.radialmenu.zh").Patch(updateUI, postfix: postfix);

                UnityEngine.Debug.Log("[RadialMenu_ZH] 径向菜单汉化补丁已成功挂载。");
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[RadialMenu_ZH] 安装汉化补丁时出错，已跳过（不影响游戏）：" + e);
                return false;
            }
        }

        private static Type ResolveType(string name)
        {
            Type t = AccessTools.TypeByName(name);
            if (t != null) return t;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    t = asm.GetType(name);
                    if (t != null) return t;
                }
                catch { /* 个别程序集反射可能抛错，忽略继续 */ }
            }
            return null;
        }
    }

    /// <summary>
    /// UpdateUI 的 Postfix：原方法跑完后，centerText / outerTexts[] 已是最终（英文）文字，
    /// 我们读取当前轮盘上下文，按上下文字典把英文标签替换成中文。
    /// 只动显示文字，不动 RadialOption.Name（它同时是查图标/导航的 key）。
    /// </summary>
    public static class RadialMenuTranslatePatch
    {
        private static FieldInfo fCurrentState;        // enum WheelState: 0=Categories 1=Buildings
        private static FieldInfo fHoveredCategoryIndex; // 0=工具 1=概览 2=建造
        private static FieldInfo fCenterText;           // UnityEngine.UI.Text
        private static FieldInfo fOuterTexts;           // List<Text>

        public static bool CacheFields(Type t)
        {
            fCurrentState = AccessTools.Field(t, "currentState");
            fHoveredCategoryIndex = AccessTools.Field(t, "hoveredCategoryIndex");
            fCenterText = AccessTools.Field(t, "centerText");
            fOuterTexts = AccessTools.Field(t, "outerTexts");
            // 中心与外圈文字是核心，必须有；状态字段缺失也能退化处理。
            return fCenterText != null && fOuterTexts != null;
        }

        public static void Postfix(object __instance)
        {
            try
            {
                int ctx = GetContext(__instance);

                Text center = fCenterText.GetValue(__instance) as Text;
                if (center != null)
                    center.text = Translations.Translate(center.text, ctx);

                IEnumerable outer = fOuterTexts.GetValue(__instance) as IEnumerable;
                if (outer != null)
                {
                    foreach (object o in outer)
                    {
                        Text txt = o as Text;
                        if (txt != null)
                            txt.text = Translations.Translate(txt.text, ctx);
                    }
                }
            }
            catch
            {
                // 任意异常都不能影响原 mod 的菜单显示
            }
        }

        private static int GetContext(object inst)
        {
            // currentState==Buildings -> 建筑列表(3)；否则用 hoveredCategoryIndex(0/1/2)
            if (fCurrentState != null)
            {
                int state = Convert.ToInt32(fCurrentState.GetValue(inst));
                if (state == 1) return 3;
            }
            if (fHoveredCategoryIndex != null)
                return Convert.ToInt32(fHoveredCategoryIndex.GetValue(inst));
            return 0;
        }
    }
}
