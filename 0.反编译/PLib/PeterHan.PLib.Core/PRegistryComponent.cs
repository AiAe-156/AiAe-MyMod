using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace PeterHan.PLib.Core;

/// <summary>
/// A custom component added to manage shared data between mods, especially instances of
/// PForwardedComponent used by both PLib and other mods.
/// </summary>
internal sealed class PRegistryComponent : MonoBehaviour, IPLibRegistry
{
	/// <summary>
	/// The Harmony instance name used when patching via PLib.
	/// </summary>
	internal const string PLIB_HARMONY = "PeterHan.PLib";

	/// <summary>
	/// A pointer to the active PLib registry.
	/// </summary>
	private static PRegistryComponent instance;

	/// <summary>
	/// true if the forwarded components have been instantiated, or false otherwise.
	/// </summary>
	private static bool instantiated;

	/// <summary>
	/// The candidate components with versions, from multiple assemblies.
	/// </summary>
	private readonly ConcurrentDictionary<string, PVersionList> forwardedComponents;

	/// <summary>
	/// The components actually instantiated (latest version of each).
	/// </summary>
	private readonly ConcurrentDictionary<string, object> instantiatedComponents;

	/// <summary>
	/// The latest versions of each component.
	/// </summary>
	private readonly ConcurrentDictionary<string, PForwardedComponent> latestComponents;

	/// <summary>
	/// Stores shared mod data which needs single instance existence. Available to all
	/// PLib consumers through PLib API.
	/// </summary>
	public IDictionary<string, object> ModData { get; }

	/// <summary>
	/// The Harmony instance used by PLib patching.
	/// </summary>
	public Harmony PLibInstance { get; }

	/// <summary>
	/// Applies the latest version of all forwarded components.
	/// </summary>
	private static void ApplyLatest()
	{
		bool flag = false;
		if ((Object)(object)instance != (Object)null)
		{
			lock (instance)
			{
				if (!instantiated)
				{
					flag = (instantiated = true);
				}
			}
		}
		if (flag)
		{
			instance.Instantiate();
		}
	}

	internal PRegistryComponent()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		if ((Object)(object)instance == (Object)null)
		{
			instance = this;
		}
		ModData = new ConcurrentDictionary<string, object>(2, 64);
		forwardedComponents = new ConcurrentDictionary<string, PVersionList>(2, 32);
		instantiatedComponents = new ConcurrentDictionary<string, object>(2, 32);
		latestComponents = new ConcurrentDictionary<string, PForwardedComponent>(2, 32);
		PLibInstance = new Harmony("PeterHan.PLib");
	}

	public void AddCandidateVersion(PForwardedComponent instance)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		AddCandidateVersion(instance.ID, instance);
	}

	/// <summary>
	/// Adds a remote or local forwarded component by ID.
	/// </summary>
	/// <param name="id">The real ID of the component.</param>
	/// <param name="instance">The candidate instance to add.</param>
	private void AddCandidateVersion(string id, PForwardedComponent instance)
	{
		PVersionList orAdd = forwardedComponents.GetOrAdd(id, (string _) => new PVersionList());
		if (orAdd == null)
		{
			PRegistry.LogPatchWarning("Missing version info for component type " + id);
			return;
		}
		List<PForwardedComponent> components = orAdd.Components;
		bool flag = components.Count < 1;
		components.Add(instance);
		if (flag)
		{
			instance.Bootstrap(PLibInstance);
		}
	}

	/// <summary>
	/// Applies a bootstrapper patch which will complete forwarded component initialization
	/// before mods are post-loaded.
	/// </summary>
	internal void ApplyBootstrapper()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		try
		{
			PLibInstance.Patch(typeof(Mod), "PostLoad", new HarmonyMethod(typeof(PRegistryComponent), "ApplyLatest", (Type[])null));
		}
		catch (AmbiguousMatchException thrown)
		{
			PUtil.LogException(thrown);
		}
		catch (ArgumentException thrown2)
		{
			PUtil.LogException(thrown2);
		}
		catch (TypeLoadException thrown3)
		{
			PUtil.LogException(thrown3);
		}
	}

	/// <summary>
	/// Called from other mods to add a candidate version of a particular component.
	/// </summary>
	/// <param name="instance">The component to be added.</param>
	internal void DoAddCandidateVersion(object instance)
	{
		AddCandidateVersion(instance.GetType().FullName, new PRemoteComponent(instance));
	}

	/// <summary>
	/// Called from other mods to get a list of all components with the given ID.
	/// </summary>
	/// <param name="id">The component ID to retrieve.</param>
	/// <returns>The instantiated instance of that component, or null if no component by
	/// that name was found or ever registered.</returns>
	internal ICollection DoGetAllComponents(string id)
	{
		if (!forwardedComponents.TryGetValue(id, out var value))
		{
			value = null;
		}
		return value?.Components;
	}

	/// <summary>
	/// Called from other mods to get the instantiated version of a particular component.
	/// </summary>
	/// <param name="id">The component ID to retrieve.</param>
	/// <returns>The instantiated instance of that component, or null if no component by
	/// that name was found or successfully instantiated.</returns>
	internal object DoGetLatestVersion(string id)
	{
		if (!instantiatedComponents.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public IEnumerable<PForwardedComponent> GetAllComponents(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		if (!forwardedComponents.TryGetValue(id, out var value))
		{
			value = null;
		}
		return value?.Components;
	}

	public PForwardedComponent GetLatestVersion(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		if (!latestComponents.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public object GetSharedData(string id)
	{
		if (!forwardedComponents.TryGetValue(id, out var value))
		{
			value = null;
		}
		return value?.SharedData;
	}

	/// <summary>
	/// Goes through the forwarded components, and picks the latest version of each to
	/// instantiate.
	/// </summary>
	public void Instantiate()
	{
		foreach (KeyValuePair<string, PVersionList> forwardedComponent in forwardedComponents)
		{
			List<PForwardedComponent> components = forwardedComponent.Value.Components;
			int count = components.Count;
			if (count > 0)
			{
				string key = forwardedComponent.Key;
				components.Sort();
				PForwardedComponent pForwardedComponent = components[count - 1];
				latestComponents.GetOrAdd(key, pForwardedComponent);
				try
				{
					instantiatedComponents.GetOrAdd(key, pForwardedComponent?.DoInitialize(PLibInstance));
				}
				catch (Exception thrown)
				{
					PRegistry.LogPatchWarning("Error when instantiating component " + key + ":");
					PUtil.LogException(thrown);
				}
			}
		}
		foreach (KeyValuePair<string, PForwardedComponent> latestComponent in latestComponents)
		{
			try
			{
				latestComponent.Value.PostInitialize(PLibInstance);
			}
			catch (Exception thrown2)
			{
				PRegistry.LogPatchWarning("Error when instantiating component " + latestComponent.Key + ":");
				PUtil.LogException(thrown2);
			}
		}
	}

	public void SetSharedData(string id, object data)
	{
		if (forwardedComponents.TryGetValue(id, out var value))
		{
			value.SharedData = data;
		}
	}

	public override string ToString()
	{
		return forwardedComponents.ToString();
	}
}
