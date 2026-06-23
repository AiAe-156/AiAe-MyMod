using System;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using STRINGS;
using UnityEngine;

namespace UtilLibs.SharedTweaks;

public class AttachmentPointTagNameFix : PForwardedComponent
{
	private const string missing = "MISSING.STRINGS.";

	public override Version Version => new Version(1, 0, 0, 1);

	public static void Register()
	{
		new AttachmentPointTagNameFix().RegisterForForwarding();
	}

	public override void Initialize(Harmony plibInstance)
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(BuildingDef), "IsAreaClear", new Type[9]
			{
				typeof(GameObject),
				typeof(int),
				typeof(Orientation),
				typeof(ObjectLayer),
				typeof(ObjectLayer),
				typeof(bool),
				typeof(bool),
				typeof(string).MakeByRefType(),
				typeof(bool)
			}, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(BuildingDef), "IsValidBuildLocation", new Type[5]
			{
				typeof(GameObject),
				typeof(int),
				typeof(Orientation),
				typeof(bool),
				typeof(string).MakeByRefType()
			}, (Type[])null);
			MethodInfo methodInfo3 = AccessTools.Method(typeof(AttachmentPointTagNameFix), "Postfix", (Type[])null, (Type[])null);
			plibInstance.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo3, 600, (string[])null, (string[])null, (bool?)null), (HarmonyMethod)null, (HarmonyMethod)null);
			plibInstance.Patch((MethodBase)methodInfo2, (HarmonyMethod)null, new HarmonyMethod(methodInfo3, 600, (string[])null, (string[])null, (bool?)null), (HarmonyMethod)null, (HarmonyMethod)null);
			Debug.Log((object)(GetType().ToString() + " successfully patched"));
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)(GetType().ToString() + " patch failed!"));
			Debug.LogWarning((object)ex.Message);
		}
	}

	public static void Postfix(BuildingDef __instance, ref string fail_reason, ref bool __result)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		if (!__result && (int)__instance.BuildLocationRule == 12)
		{
			string text = StringEntry.op_Implicit(Strings.Get("STRINGS.MISC.TAGS." + ((Tag)(ref __instance.AttachmentSlotTag)).Name.ToUpperInvariant()));
			if (!text.Contains("MISSING.STRINGS."))
			{
				fail_reason = string.Format(LocString.op_Implicit(TOOLTIPS.HELP_BUILDLOCATION_ATTACHPOINT), text);
			}
		}
	}
}
