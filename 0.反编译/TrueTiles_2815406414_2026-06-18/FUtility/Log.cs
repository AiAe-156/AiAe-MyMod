using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using KMod;

namespace FUtility;

public class Log
{
	public static string modName = typeof(Log).Assembly.GetName().Name.Replace("Merged", "");

	private static string prefix = "[" + modName + "]: ";

	public static void SetName(string name)
	{
		prefix = "[" + name + "]: ";
	}

	public static void PrintVersion()
	{
		Info("Loaded version " + GetVersion());
	}

	public static void PrintVersion(UserMod2 mod)
	{
		Info("Loaded " + mod.mod.title + ", v" + mod.mod.packagedModInfo.version);
		Debug(mod.mod.title + " loaded in DEBUG MODE.");
	}

	public static string GetVersion()
	{
		return Assembly.GetExecutingAssembly().GetName().Version.ToString();
	}

	public static void Info(params object[] arg)
	{
		Debug.Log((object)(prefix + string.Join(", ", arg)));
	}

	public static void Warning(params object[] arg)
	{
		Debug.LogWarning((object)(prefix + string.Join(", ", arg)));
	}

	public static void Assert(string name, object arg)
	{
		if (arg == null)
		{
			Warning("Assert failed, " + name + " is null");
		}
	}

	[Obsolete]
	public static void Debuglog(params object[] arg)
	{
		Debug(arg);
	}

	public static void Debug(params object[] arg)
	{
	}

	public static void Error(object arg)
	{
		Debug.LogError((object)(prefix + arg.ToString()));
	}

	public static void PrintInstructions(List<CodeInstruction> codes)
	{
		Debug("\n");
		for (int i = 0; i < codes.Count; i++)
		{
			Debug(i + ": " + (object)codes[i]);
		}
	}
}
