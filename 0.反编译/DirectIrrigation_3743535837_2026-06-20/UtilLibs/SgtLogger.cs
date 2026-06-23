using System;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using KMod;
using PeterHan.PLib.AVC;
using UtilLibs.ModVersionCheck;

namespace UtilLibs;

public class SgtLogger
{
	private static Harmony harmony;

	public SgtLogger(string log)
	{
		l(log);
	}

	public static void LogVersionAndInitUpdating(UserMod2 usermod, Harmony _harmony)
	{
		LogVersion(usermod, _harmony);
	}

	public static void LogVersion(UserMod2 usermod, Harmony _harmony, bool VersionChecking = true)
	{
		harmony = _harmony;
		if (VersionChecking)
		{
			VersionChecker.HandleVersionChecking(usermod, harmony);
			PVersionCheck pVersionCheck = new PVersionCheck();
			pVersionCheck.Register(usermod, new SteamVersionChecker());
		}
		CompatibilityNotifications.FixLogging(harmony);
		debuglog(usermod.mod.staticID + " - Mod Version: " + usermod.mod.packagedModInfo.version + " ");
	}

	public static void l(string message, string assemblyOverride = "")
	{
		debuglog(message, assemblyOverride);
	}

	public static void Assert(string name, object arg)
	{
		if (arg == null)
		{
			warning("Assert failed, " + name + " is null");
		}
	}

	public static void Assert(object arg, string name)
	{
		if (arg == null)
		{
			warning("Assert failed, " + name + " is null");
		}
	}

	public static void debuglog(object a, object b = null, object c = null, object d = null)
	{
		string text = ((a.ToString() + b != null) ? (" " + b.ToString()) : ((string.Empty + c != null) ? (" " + c.ToString()) : ((string.Empty + d != null) ? (" " + d.ToString()) : string.Empty)));
		string name = Assembly.GetExecutingAssembly().GetName().Name;
		string value = TimeStamp() + " [INFO] [" + name + "]: " + text;
		Console.WriteLine(value);
	}

	public static void debuglog(string message, string assemblyOverride = "")
	{
		if (assemblyOverride == "")
		{
			assemblyOverride = Assembly.GetExecutingAssembly().GetName().Name;
		}
		string value = TimeStamp() + " [INFO] [" + assemblyOverride + "]: " + message;
		Console.WriteLine(value);
	}

	public static string TimeStamp()
	{
		return string.Concat("[" + TimeZoneInfo.ConvertTimeToUtc(DateTime.Now).ToString("HH:mm:ss.fff"), "] [", Thread.CurrentThread.ManagedThreadId, "]");
	}

	public static void log(string message, string assemblyOverride = "")
	{
		debuglog(message, assemblyOverride);
	}

	public static void warning(string message, string assemblyOverride = "")
	{
		dlogwarn(message, assemblyOverride);
	}

	public static void error(string message, string assemblyOverride = "")
	{
		dlogerror(message, assemblyOverride);
	}

	public static void transpilerfail(string message, string assemblyOverride = "")
	{
		dlogerror("TRANSPILER FAILED: " + message, assemblyOverride);
	}

	public static void logwarning(string message, string assemblyOverride = "")
	{
		dlogwarn(message, assemblyOverride);
	}

	public static void logerror(string message, string assemblyOverride = "")
	{
		dlogerror(message, assemblyOverride);
	}

	public static void dlogwarn(string message, string assemblyOverride = "")
	{
		if (assemblyOverride == "")
		{
			assemblyOverride = Assembly.GetExecutingAssembly().GetName().Name;
		}
		string format = TimeStamp() + " [WARNING] [" + assemblyOverride + "]: " + message;
		Console.WriteLine(format, assemblyOverride);
	}

	public static void dlogerror(string message, string assemblyOverride = "")
	{
		if (assemblyOverride == "")
		{
			assemblyOverride = Assembly.GetExecutingAssembly().GetName().Name;
		}
		string format = TimeStamp() + " [ERROR] [" + assemblyOverride + "]: " + message;
		Console.WriteLine(format, assemblyOverride);
	}

	public static void logError(string v)
	{
		string name = Assembly.GetExecutingAssembly().GetName().Name;
		string format = TimeStamp() + " [ERROR] [" + name + "]: " + v;
		Console.WriteLine(format, name);
	}
}
