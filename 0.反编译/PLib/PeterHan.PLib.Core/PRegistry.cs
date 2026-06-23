using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.PLib.Core;

/// <summary>
/// Provides the user facing API to the PLib Registry.
/// </summary>
public static class PRegistry
{
	/// <summary>
	/// A pointer to the active PLib registry.
	/// </summary>
	private static IPLibRegistry instance = null;

	/// <summary>
	/// Ensures that PLib can only be initialized by one thread at a time.
	/// </summary>
	private static readonly object instanceLock = new object();

	/// <summary>
	/// The singleton instance of this class.
	/// </summary>
	public static IPLibRegistry Instance
	{
		get
		{
			lock (instanceLock)
			{
				if (instance == null)
				{
					Init();
				}
			}
			return instance;
		}
	}

	/// <summary>
	/// Retrieves a value from the single-instance share.
	/// </summary>
	/// <typeparam name="T">The type of the desired data.</typeparam>
	/// <param name="key">The string key to retrieve. <i>Suggested key format: YourMod.
	/// Category.KeyName</i></param>
	/// <returns>The data associated with that key.</returns>
	public static T GetData<T>(string key)
	{
		T result = default(T);
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		IDictionary<string, object> modData = Instance.ModData;
		if (modData != null && modData.TryGetValue(key, out var value) && value is T result2)
		{
			return result2;
		}
		return result;
	}

	/// <summary>
	/// Initializes the patch bootstrapper, creating a PRegistry if not yet present.
	/// </summary>
	private static void Init()
	{
		Global obj = Global.Instance;
		GameObject val = ((obj != null) ? ((Component)obj).gameObject : null);
		if ((Object)(object)val != (Object)null)
		{
			Component component = val.GetComponent("PRegistryComponent");
			if ((Object)(object)component == (Object)null)
			{
				PRegistryComponent pRegistryComponent = val.AddComponent<PRegistryComponent>();
				string name = ((object)pRegistryComponent).GetType().Name;
				if (name != "PRegistryComponent")
				{
					LogPatchWarning("PRegistryComponent has the type name " + name + "; this may be the result of ILMerging PLib more than once!");
				}
				pRegistryComponent.ApplyBootstrapper();
				instance = pRegistryComponent;
			}
			else
			{
				instance = new PRemoteRegistry(component);
			}
		}
		else
		{
			instance = null;
		}
		if (instance != null)
		{
			new PLibCorePatches().Register(instance);
		}
	}

	/// <summary>
	/// Logs a debug message while patching in PLib patches.
	/// </summary>
	/// <param name="message">The debug message.</param>
	internal static void LogPatchDebug(string message)
	{
		Debug.LogFormat("[PLibPatches] {0}", new object[1] { message });
	}

	/// <summary>
	/// Logs a warning encountered while patching in PLib patches.
	/// </summary>
	/// <param name="message">The warning message.</param>
	internal static void LogPatchWarning(string message)
	{
		Debug.LogWarningFormat("[PLibPatches] {0}", new object[1] { message });
	}

	/// <summary>
	/// Saves a value into the single-instance share.
	/// </summary>
	/// <param name="key">The string key to set. <i>Suggested key format: YourMod.
	/// Category.KeyName</i></param>
	/// <param name="value">The data to be associated with that key.</param>
	public static void PutData(string key, object value)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		IDictionary<string, object> modData = Instance.ModData;
		if (modData != null)
		{
			if (modData.ContainsKey(key))
			{
				modData[key] = value;
			}
			else
			{
				modData.Add(key, value);
			}
		}
	}
}
