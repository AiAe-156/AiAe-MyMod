using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using PeterHan.PLib.Core;

namespace UtilLibs.SharedTweaks;

public sealed class ResearchNotificationMessageFix : PForwardedComponent
{
	public override Version Version => new Version(1, 0, 0, 0);

	public static void Register()
	{
		new ResearchNotificationMessageFix().RegisterForForwarding();
	}

	public override void Initialize(Harmony plibInstance)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(ResearchCompleteMessage), "GetMessageBody", (Type[])null, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(ResearchNotificationMessageFix), "LinebreakTranspiler", (Type[])null, (Type[])null);
			plibInstance.Patch((MethodBase)methodInfo, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(methodInfo2), (HarmonyMethod)null);
			Debug.Log((object)(GetType().ToString() + " successfully patched"));
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)(GetType().ToString() + " patch failed!"));
			Debug.LogWarning((object)ex.Message);
		}
	}

	private static IEnumerable<CodeInstruction> LinebreakTranspiler(ILGenerator _, IEnumerable<CodeInstruction> orig)
	{
		List<CodeInstruction> list = orig.ToList();
		int num = list.FindIndex((CodeInstruction ci) => CodeInstructionExtensions.LoadsConstant(ci, ", "));
		if (num == -1)
		{
			Console.WriteLine("TRANSPILER FAILED: ResearchCompleteMessage");
			return list;
		}
		list[num].operand = "\n  • ";
		return list;
	}
}
