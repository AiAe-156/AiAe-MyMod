using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UtilLibs.SharedTweaks;

public sealed class ResearchScreenCollapseEntries : PForwardedComponent
{
	private static Dictionary<ResearchEntry, List<GameObject>> CollectedIcons = new Dictionary<ResearchEntry, List<GameObject>>();

	private static Dictionary<ResearchEntry, GameObject> CollapsedIndicators = new Dictionary<ResearchEntry, GameObject>();

	public override Version Version => new Version(1, 0, 0, 0);

	public static void Register()
	{
		new ResearchScreenCollapseEntries().RegisterForForwarding();
	}

	public override void Initialize(Harmony plibInstance)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(ResearchEntry), "OnHover", (Type[])null, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(ResearchEntry), "SetEverythingOff", (Type[])null, (Type[])null);
			MethodInfo methodInfo3 = AccessTools.Method(typeof(ResearchEntry), "GetFreeIcon", (Type[])null, (Type[])null);
			MethodInfo methodInfo4 = AccessTools.Method(typeof(ResearchEntry), "SetTech", (Type[])null, (Type[])null);
			MethodInfo methodInfo5 = AccessTools.Method(typeof(ResearchScreenCollapseEntries), "TurnOffPostfix", (Type[])null, (Type[])null);
			MethodInfo methodInfo6 = AccessTools.Method(typeof(ResearchScreenCollapseEntries), "TurnOnPrefix", (Type[])null, (Type[])null);
			MethodInfo methodInfo7 = AccessTools.Method(typeof(ResearchScreenCollapseEntries), "CollectEntriesPostfix", (Type[])null, (Type[])null);
			plibInstance.Patch((MethodBase)methodInfo, new HarmonyMethod(methodInfo6, 300, (string[])null, (string[])null, (bool?)null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			plibInstance.Patch((MethodBase)methodInfo2, (HarmonyMethod)null, new HarmonyMethod(methodInfo5, 300, (string[])null, (string[])null, (bool?)null), (HarmonyMethod)null, (HarmonyMethod)null);
			plibInstance.Patch((MethodBase)methodInfo3, (HarmonyMethod)null, new HarmonyMethod(methodInfo7), (HarmonyMethod)null, (HarmonyMethod)null);
			plibInstance.Patch((MethodBase)methodInfo4, (HarmonyMethod)null, new HarmonyMethod(methodInfo5), (HarmonyMethod)null, (HarmonyMethod)null);
			Debug.Log((object)(GetType().ToString() + " successfully patched"));
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)(GetType().ToString() + " patch failed!"));
			Debug.LogWarning((object)ex.Message);
		}
	}

	public static void CollectEntriesPostfix(ResearchEntry __instance, ref GameObject __result)
	{
		Tech targetTech = __instance.targetTech;
		if (targetTech == null || targetTech.unlockedItems.Count() > 8)
		{
			if (!CollectedIcons.ContainsKey(__instance))
			{
				CollectedIcons[__instance] = new List<GameObject>(8);
			}
			CollectedIcons[__instance].Add(__result);
		}
	}

	public static void TurnOffPostfix(ResearchEntry __instance)
	{
		CollapseExcessEntries(__instance, collapseEntries: true);
	}

	public static void TurnOnPrefix(ResearchEntry __instance, bool entered, Tech hoverSource)
	{
		if (entered && hoverSource == __instance.targetTech)
		{
			CollapseExcessEntries(__instance, collapseEntries: false);
		}
	}

	private static void HandleFastTrack(ResearchEntry entry, bool enableOldLayout)
	{
		LayoutElement val = default(LayoutElement);
		LayoutGroup val2 = default(LayoutGroup);
		KChildFitter val3 = default(KChildFitter);
		if (((Component)entry).TryGetComponent<LayoutElement>(ref val) && ((Component)entry).TryGetComponent<LayoutGroup>(ref val2) && ((Component)((KMonoBehaviour)entry).transform.parent).TryGetComponent<KChildFitter>(ref val3) && ((Behaviour)val).enabled != ((Behaviour)val2).enabled)
		{
			if (enableOldLayout)
			{
				SetFreezeState(val, val2, freeze: false);
				val3.FitSize();
			}
			else
			{
				SetFreezeState(val, val2, freeze: true);
				val3.FitSize();
			}
		}
	}

	private static void SetFreezeState(LayoutElement frozenLayout, LayoutGroup realLayout, bool freeze)
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(Util.rectTransform((Component)(object)realLayout));
		if (freeze)
		{
			((Behaviour)realLayout).enabled = false;
			((Behaviour)frozenLayout).enabled = true;
		}
		else
		{
			((Behaviour)frozenLayout).enabled = false;
			((Behaviour)realLayout).enabled = true;
		}
	}

	private static void CollapseExcessEntries(ResearchEntry entry, bool collapseEntries)
	{
		Tech targetTech = entry.targetTech;
		if (targetTech != null && targetTech.unlockedItems.Count() <= 8)
		{
			return;
		}
		if (!CollapsedIndicators.TryGetValue(entry, out var value))
		{
			value = CreateCollapseIcon(entry);
		}
		if (CollectedIcons.TryGetValue(entry, out var value2))
		{
			value.SetActive(collapseEntries);
			for (int num = value2.Count - 1; num >= 7; num--)
			{
				value2[num].SetActive(!collapseEntries);
			}
			HandleFastTrack(entry, !collapseEntries);
		}
	}

	private static GameObject CreateCollapseIcon(ResearchEntry entry)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		((Component)entry.researchName).GetComponent<ToolTip>().SizingSetting = (ToolTipSizeSetting)0;
		GameObject val = Util.KInstantiateUI(entry.iconPrefab, entry.iconPanel, false);
		val.SetActive(true);
		HierarchyReferences component = val.GetComponent<HierarchyReferences>();
		KImage reference = component.GetReference<KImage>("Background");
		((Component)component.GetReference<KImage>("Icon")).gameObject.SetActive(false);
		((Graphic)reference).color = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
		((Component)component.GetReference<KImage>("DLCOverlay")).gameObject.SetActive(false);
		LocText val2 = Util.KInstantiateUI<LocText>(((Component)entry.researchName).gameObject, val, true);
		Tech targetTech = entry.targetTech;
		((TMP_Text)val2).SetText("+" + ((targetTech != null) ? (targetTech.unlockedItems.Count - 7) : (-1)));
		((TMP_Text)val2).enableAutoSizing = false;
		((TMP_Text)val2).fontSize = 32f;
		((TMP_Text)val2).alignment = (TextAlignmentOptions)514;
		((TMP_Text)val2).textWrappingMode = (TextWrappingModes)0;
		((TMP_Text)val2).overflowMode = (TextOverflowModes)0;
		CollapsedIndicators[entry] = val;
		return val;
	}
}
