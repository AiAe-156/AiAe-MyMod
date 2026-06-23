using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using KMod;
using UnityEngine;

namespace PeterHan.PLib.Core;

/// <summary>
/// Static utility functions used across mods.
/// </summary>
public static class PUtil
{
	/// <summary>
	/// Whether PLib has been initialized.
	/// </summary>
	private static volatile bool initialized;

	/// <summary>
	/// Serializes attempts to initialize PLib.
	/// </summary>
	private static readonly object initializeLock;

	/// <summary>
	/// The characters which are not allowed in file names.
	/// </summary>
	private static readonly HashSet<char> INVALID_FILE_CHARS;

	/// <summary>
	/// Retrieves the current changelist version of the game. LU-371502 has a version of
	/// 371502u.
	///
	/// If the version cannot be determined, returns 0.
	/// </summary>
	public static uint GameVersion { get; }

	static PUtil()
	{
		initialized = false;
		initializeLock = new object();
		INVALID_FILE_CHARS = new HashSet<char>(Path.GetInvalidFileNameChars());
		GameVersion = GetGameVersion();
	}

	/// <summary>
	/// Generates a mapping of assembly names to Mod instances. Only works after all mods
	/// have been loaded.
	/// </summary>
	/// <returns>A mapping from assemblies to the Mod instance that owns them.</returns>
	public static IDictionary<Assembly, Mod> CreateAssemblyToModTable()
	{
		Global instance = Global.Instance;
		Dictionary<Assembly, Mod> dictionary = new Dictionary<Assembly, Mod>(32);
		if ((Object)(object)instance != (Object)null)
		{
			List<Mod> list = instance.modManager?.mods;
			if (list != null)
			{
				foreach (Mod item in list)
				{
					ICollection<Assembly> collection = item?.loaded_mod_data?.dlls;
					if (collection == null)
					{
						continue;
					}
					foreach (Assembly item2 in collection)
					{
						dictionary[item2] = item;
					}
				}
			}
		}
		return dictionary;
	}

	/// <summary>
	/// Finds the distance between two points.
	/// </summary>
	/// <param name="x1">The first X coordinate.</param>
	/// <param name="y1">The first Y coordinate.</param>
	/// <param name="x2">The second X coordinate.</param>
	/// <param name="y2">The second Y coordinate.</param>
	/// <returns>The non-taxicab (straight line) distance between the points.</returns>
	public static float Distance(float x1, float y1, float x2, float y2)
	{
		float num = x2 - x1;
		float num2 = y2 - y1;
		return Mathf.Sqrt(num * num + num2 * num2);
	}

	/// <summary>
	/// Finds the distance between two points.
	/// </summary>
	/// <param name="x1">The first X coordinate.</param>
	/// <param name="y1">The first Y coordinate.</param>
	/// <param name="x2">The second X coordinate.</param>
	/// <param name="y2">The second Y coordinate.</param>
	/// <returns>The non-taxicab (straight line) distance between the points.</returns>
	public static double Distance(double x1, double y1, double x2, double y2)
	{
		double num = x2 - x1;
		double num2 = y2 - y1;
		return Math.Sqrt(num * num + num2 * num2);
	}

	/// <summary>
	/// Retrieves the current game version from the Klei code.
	/// </summary>
	/// <returns>The change list version of the game, or 0 if it cannot be determined.</returns>
	private static uint GetGameVersion()
	{
		uint result = 0u;
		if (PPatchTools.TryGetFieldValue<uint>(typeof(KleiVersion), "ChangeList", out var value))
		{
			result = value;
		}
		return result;
	}

	/// <summary>
	/// Retrieves the mod directory for the specified assembly. If an archived version is
	/// running, the path to that version is reported.
	/// </summary>
	/// <param name="modDLL">The assembly used for a mod.</param>
	/// <returns>The directory where the mod is currently executing.</returns>
	public static string GetModPath(Assembly modDLL)
	{
		if (modDLL == null)
		{
			throw new ArgumentNullException("modDLL");
		}
		string text = null;
		try
		{
			text = Directory.GetParent(modDLL.Location)?.FullName;
		}
		catch (NotSupportedException thrown)
		{
			LogExcWarn(thrown);
		}
		catch (SecurityException thrown2)
		{
			LogExcWarn(thrown2);
		}
		catch (IOException thrown3)
		{
			LogExcWarn(thrown3);
		}
		return text ?? Path.Combine(Manager.GetDirectory(), modDLL.GetName().Name ?? "");
	}

	/// <summary>
	/// Initializes PLib. While most components are initialized dynamically if used, some
	/// key infrastructure must be initialized first.
	/// </summary>
	/// <param name="logVersion">If true, the mod name and version is emitted to the log.</param>
	public static void InitLibrary(bool logVersion = true)
	{
		Assembly callingAssembly = Assembly.GetCallingAssembly();
		lock (initializeLock)
		{
			if (!initialized)
			{
				initialized = true;
				if (logVersion)
				{
					Debug.LogFormat("[PLib] Mod {0} initialized, version {1}", new object[2]
					{
						callingAssembly.GetNameSafe(),
						callingAssembly.GetFileVersion() ?? "Unknown"
					});
				}
			}
		}
	}

	/// <summary>
	/// Returns true if the file is a valid file name. If the argument contains path
	/// separator characters, this method returns false, since that is not a valid file
	/// name.
	///
	/// Null and empty file names are not valid file names.
	/// </summary>
	/// <param name="file">The file name to check.</param>
	/// <returns>true if the name could be used to name a file, or false otherwise.</returns>
	public static bool IsValidFileName(string file)
	{
		bool flag = file != null;
		if (flag)
		{
			int length = file.Length;
			for (int i = 0; i < length && flag; i++)
			{
				if (INVALID_FILE_CHARS.Contains(file[i]))
				{
					flag = false;
				}
			}
		}
		return flag;
	}

	/// <summary>
	/// Logs a message to the debug log.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void LogDebug(object message)
	{
		Debug.LogFormat("[PLib/{0}] {1}", new object[2]
		{
			Assembly.GetCallingAssembly().GetNameSafe(),
			message
		});
	}

	/// <summary>
	/// Logs an error message to the debug log.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void LogError(object message)
	{
		Debug.LogErrorFormat("[PLib/{0}] {1}", new object[2]
		{
			Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
			message
		});
	}

	/// <summary>
	/// Logs an exception message to the debug log.
	/// </summary>
	/// <param name="thrown">The exception to log.</param>
	public static void LogException(Exception thrown)
	{
		Debug.LogErrorFormat("[PLib/{0}] {1} {2} {3}", new object[4]
		{
			Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
			thrown.GetType(),
			thrown.Message,
			thrown.StackTrace
		});
	}

	/// <summary>
	/// Logs an exception message to the debug log at WARNING level.
	/// </summary>
	/// <param name="thrown">The exception to log.</param>
	public static void LogExcWarn(Exception thrown)
	{
		Debug.LogWarningFormat("[PLib/{0}] {1} {2} {3}", new object[4]
		{
			Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
			thrown.GetType(),
			thrown.Message,
			thrown.StackTrace
		});
	}

	/// <summary>
	/// Logs a warning message to the debug log.
	/// </summary>
	/// <param name="message">The message to log.</param>
	public static void LogWarning(object message)
	{
		Debug.LogWarningFormat("[PLib/{0}] {1}", new object[2]
		{
			Assembly.GetCallingAssembly().GetNameSafe() ?? "?",
			message
		});
	}

	/// <summary>
	/// Measures how long the specified code takes to run. The result is logged to the
	/// debug log in microseconds.
	/// </summary>
	/// <param name="code">The code to execute.</param>
	/// <param name="header">The name used in the log to describe this code.</param>
	public static void Time(Action code, string header = "Code")
	{
		if (code == null)
		{
			throw new ArgumentNullException("code");
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		code();
		stopwatch.Stop();
		LogDebug("{1} took {0:D} us".F(stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency, header));
	}
}
