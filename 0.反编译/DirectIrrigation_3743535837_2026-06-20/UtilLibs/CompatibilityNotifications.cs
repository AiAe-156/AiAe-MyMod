using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using STRINGS;
using UnityEngine;

namespace UtilLibs;

public class CompatibilityNotifications
{
	[HarmonyPatch(typeof(MainMenu), "OnSpawn")]
	public static class MainMenu_OnSpawn_Patch
	{
		internal static void Postfix(MainMenu __instance)
		{
			DumpIncompatibilityMessage(__instance);
		}
	}

	public const string CompatibilityDataKey = "Sgt_Imalas_IncompatibleModList";

	private static string BrokenTimeoutFixed = "CrapManager_BrokenTimeoutFixed";

	private static int ManagerFixVersion = 1;

	private static string BrokenLoggingFixed = "CrapConsole_LoggingPreventionFixed";

	private static int LoggingFixVersion = 2;

	private static readonly string GameName = "OxygenNotIncluded_DebugConsole";

	public static void CheckAndAddIncompatibles(string assemblyName, string modName, string conflictingMod)
	{
		Debug.Log((object)("checking if incompatible mod is installed: " + assemblyName));
		initList();
		if (AppDomain.CurrentDomain.GetAssemblies().ToList().Any((Assembly ass) => ass.FullName.ToLowerInvariant().Contains(assemblyName.ToLowerInvariant())))
		{
			Debug.Log((object)("incompatible mod found: " + assemblyName));
			AddIncompatibleToList(modName, conflictingMod);
		}
		else
		{
			Debug.Log((object)("mod not found: " + assemblyName));
		}
	}

	public static void FixBrokenTimeout(Harmony harmony)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		if (PRegistry.GetData<int>(BrokenTimeoutFixed) >= ManagerFixVersion)
		{
			return;
		}
		PRegistry.PutData(BrokenTimeoutFixed, ManagerFixVersion);
		Type type = Type.GetType("Ony.OxygenNotIncluded.ModManager.Updater, Release_DLC1.Mod.ModManager");
		if (type == null)
		{
			return;
		}
		Type type2 = type.GetNestedTypes(AccessTools.all).FirstOrDefault((Type t) => !t.FullName.Contains("All") && t.FullName.Contains("<Update>"));
		if (!(type2 == null))
		{
			MethodInfo methodInfo = AccessTools.Method(type2, "MoveNext", (Type[])null, (Type[])null);
			if (!(methodInfo == null))
			{
				MethodInfo methodInfo2 = AccessTools.Method(typeof(CompatibilityNotifications), "BrokenTimeoutFixTranspiler", (Type[])null, (Type[])null);
				harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(methodInfo2), (HarmonyMethod)null);
			}
		}
	}

	public static IEnumerable<CodeInstruction> BrokenTimeoutFixTranspiler(ILGenerator _, IEnumerable<CodeInstruction> orig)
	{
		List<CodeInstruction> list = orig.ToList();
		int num = list.FindIndex((CodeInstruction c) => CodeInstructionExtensions.LoadsConstant(c, 5000L));
		if (num == -1)
		{
			return list;
		}
		list[num].operand = 999999999;
		return list;
	}

	public static void RemoveCrashingIncompatibility(Harmony harmony, IReadOnlyList<Mod> mods, string faultyId)
	{
		faultyId = faultyId.ToLowerInvariant();
		Mod val = mods.FirstOrDefault((Mod mod) => mod.staticID.ToLowerInvariant().Contains(faultyId));
		if (val != null)
		{
			val.SetCrashed();
			val.SetEnabledForActiveDlc(false);
			harmony.UnpatchAll(val.staticID);
		}
	}

	public static void FlagLoggingPrevention(IReadOnlyList<Mod> _mods)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Harmony val = new Harmony(default(Guid).ToString());
		val.UnpatchAll("OxygenNotIncluded_v0.1");
		RemoveCrashingIncompatibility(val, _mods, "DEBUGCONSOLE");
	}

	public static void FixLogging(Harmony harmony)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		if (PRegistry.GetData<int>(BrokenLoggingFixed) < LoggingFixVersion)
		{
			PRegistry.PutData(BrokenLoggingFixed, LoggingFixVersion);
			Mod val = Global.Instance.modManager.mods.FirstOrDefault((Mod m) => m.staticID.Contains("DebugConsole"));
			if (val != null)
			{
				val.foundInStackTrace = true;
				MethodInfo methodInfo = AccessTools.Method(typeof(Mod), "IsEnabledForActiveDlc", (Type[])null, (Type[])null);
				MethodInfo methodInfo2 = AccessTools.Method(typeof(CompatibilityNotifications), "Skip", (Type[])null, (Type[])null);
				harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(methodInfo2, 0, (string[])null, (string[])null, (bool?)null), (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
	}

	private static void Skip(Mod __instance, ref bool __result)
	{
		if (__instance.staticID.Contains("DebugConsole"))
		{
			__result = false;
		}
	}

	private static void initList()
	{
		if (PRegistry.GetData<Dictionary<string, string>>("Sgt_Imalas_IncompatibleModList") == null)
		{
			List<Tuple<string, string>> value = new List<Tuple<string, string>>();
			PRegistry.PutData("Sgt_Imalas_IncompatibleModList", value);
		}
	}

	private static void AddIncompatibleToList(string modName, string conflictingModName)
	{
		Dictionary<string, string> dictionary = PRegistry.GetData<Dictionary<string, string>>("Sgt_Imalas_IncompatibleModList");
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, string>();
		}
		if (conflictingModName.Count() > 40)
		{
			conflictingModName = conflictingModName.Remove(40);
			conflictingModName += "...";
		}
		if (modName == GameName)
		{
			if (!dictionary.ContainsKey(modName))
			{
				dictionary.Add(modName, "");
				dictionary[modName] = conflictingModName;
			}
		}
		else
		{
			if (!dictionary.ContainsKey(modName))
			{
				dictionary.Add(modName, "");
			}
			Dictionary<string, string> dictionary2 = dictionary;
			dictionary2[modName] = dictionary2[modName] + "\n• " + conflictingModName;
		}
		PRegistry.PutData("Sgt_Imalas_IncompatibleModList", dictionary);
	}

	public static void DumpIncompatibilityMessage(MainMenu parent)
	{
		Dictionary<string, string> data = PRegistry.GetData<Dictionary<string, string>>("Sgt_Imalas_IncompatibleModList");
		if (data == null || data.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<string, string> item in data)
		{
			if (!(item.Key == GameName))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(item.Key + " has declared the following mods as conflicting:");
				stringBuilder.AppendLine(item.Value);
				Manager.Dialog(((Component)parent).gameObject, "Conflicting Mods found!", stringBuilder.ToString(), LocString.op_Implicit(CONFIRMDIALOG.OK), (Action)null, (string)null, (Action)null, (string)null, (Action)null, (Sprite)null);
			}
		}
		PRegistry.PutData("Sgt_Imalas_IncompatibleModList", null);
	}
}
