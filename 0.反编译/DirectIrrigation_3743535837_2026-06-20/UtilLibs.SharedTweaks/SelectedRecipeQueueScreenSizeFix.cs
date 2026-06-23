using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using UnityEngine;

namespace UtilLibs.SharedTweaks;

public sealed class SelectedRecipeQueueScreenSizeFix : PForwardedComponent
{
	public override Version Version => new Version(1, 0, 0, 0);

	public static void Register()
	{
		new SelectedRecipeQueueScreenSizeFix().RegisterForForwarding();
	}

	public override void Initialize(Harmony plibInstance)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(SelectedRecipeQueueScreen), "RefreshSizeScrollContainerSize", (Type[])null, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(SelectedRecipeQueueScreenSizeFix), "SelectedRecipeQueueScreen_Postfix", (Type[])null, (Type[])null);
			plibInstance.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo2), (HarmonyMethod)null, (HarmonyMethod)null);
			Debug.Log((object)(GetType().ToString() + " successfully patched"));
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)(GetType().ToString() + " patch failed!"));
			Debug.LogWarning((object)ex.Message);
		}
	}

	public static void SelectedRecipeQueueScreen_Postfix(SelectedRecipeQueueScreen __instance)
	{
		float minHeight = __instance.scrollContainer.minHeight;
		float num = KPlayerPrefs.GetFloat(KCanvasScaler.UIScalePrefKey) / 100f;
		if (num > 1f)
		{
			num *= num;
		}
		float num2 = 632f;
		if (__instance.RefreshMinionDisplayAnim())
		{
			num2 -= 128f;
		}
		float num3 = num2 / 1080f;
		float num4 = (float)Screen.height * num3;
		num4 /= num;
		__instance.scrollContainer.minHeight = Mathf.Min(minHeight, num4);
	}
}
