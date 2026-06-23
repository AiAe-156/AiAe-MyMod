using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using TMPro;
using UnityEngine.UI;

namespace UtilLibs.SharedTweaks;

public sealed class DynamicMaterialSelectorHeaderHeight : PForwardedComponent
{
	public override Version Version => new Version(1, 0, 0, 0);

	public static void Register()
	{
		new DynamicMaterialSelectorHeaderHeight().RegisterForForwarding();
	}

	public override void Initialize(Harmony plibInstance)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(MaterialSelector), "UpdateHeader", (Type[])null, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(DynamicMaterialSelectorHeaderHeight), "DynamicHeightPostfix", (Type[])null, (Type[])null);
			plibInstance.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo2), (HarmonyMethod)null, (HarmonyMethod)null);
			Debug.Log((object)(GetType().ToString() + " successfully patched"));
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)(GetType().ToString() + " patch failed!"));
			Debug.LogWarning((object)ex.Message);
		}
	}

	public static void DynamicHeightPostfix(MaterialSelector __instance)
	{
		LocText componentInChildren = __instance.Headerbar.GetComponentInChildren<LocText>();
		int lineCount = ((TMP_Text)componentInChildren).textInfo.lineCount;
		int num = lineCount * 24;
		__instance.Headerbar.GetComponent<LayoutElement>().minHeight = num;
	}
}
