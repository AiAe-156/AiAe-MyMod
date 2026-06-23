using System;
using System.Collections;
using System.Collections.Generic;

namespace PeterHan.PLib.Core;

/// <summary>
/// Transparently provides the functionality of PRegistry, while the actual instance is
/// from another mod's bootstrapper.
/// </summary>
internal sealed class PRemoteRegistry : IPLibRegistry
{
	/// <summary>
	/// The prototype used for delegates to remote GetAllComponents.
	/// </summary>
	private delegate ICollection GetAllComponentsDelegate(string id);

	/// <summary>
	/// The prototype used for delegates to remote GetLatestVersion and GetSharedData.
	/// </summary>
	private delegate object GetObjectDelegate(string id);

	/// <summary>
	/// The prototype used for delegates to remote SetSharedData.
	/// </summary>
	private delegate void SetObjectDelegate(string id, object value);

	/// <summary>
	/// Points to the local registry's version of AddCandidateVersion.
	/// </summary>
	private readonly Action<object> addCandidateVersion;

	/// <summary>
	/// Points to the local registry's version of GetAllComponents.
	/// </summary>
	private readonly GetAllComponentsDelegate getAllComponents;

	/// <summary>
	/// Points to the local registry's version of GetLatestVersion.
	/// </summary>
	private readonly GetObjectDelegate getLatestVersion;

	/// <summary>
	/// Points to the local registry's version of GetSharedData.
	/// </summary>
	private readonly GetObjectDelegate getSharedData;

	/// <summary>
	/// Points to the local registry's version of SetSharedData.
	/// </summary>
	private readonly SetObjectDelegate setSharedData;

	/// <summary>
	/// The components actually instantiated (latest version of each).
	/// </summary>
	private readonly IDictionary<string, PForwardedComponent> remoteComponents;

	public IDictionary<string, object> ModData { get; private set; }

	/// <summary>
	/// Creates a remote registry wrapping the target object.
	/// </summary>
	/// <param name="instance">The PRegistryComponent instance to wrap.</param>
	internal PRemoteRegistry(object instance)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		remoteComponents = new Dictionary<string, PForwardedComponent>(32);
		if (!PPatchTools.TryGetPropertyValue<IDictionary<string, object>>(instance, "ModData", out var value))
		{
			throw new ArgumentException("Remote instance missing ModData");
		}
		ModData = value;
		Type type = instance.GetType();
		addCandidateVersion = type.CreateDelegate<Action<object>>("DoAddCandidateVersion", instance, new Type[1] { typeof(object) });
		getAllComponents = type.CreateDelegate<GetAllComponentsDelegate>("DoGetAllComponents", instance, new Type[1] { typeof(string) });
		getLatestVersion = type.CreateDelegate<GetObjectDelegate>("DoGetLatestVersion", instance, new Type[1] { typeof(string) });
		if (addCandidateVersion == null || getLatestVersion == null || getAllComponents == null)
		{
			throw new ArgumentException("Remote instance missing candidate versions");
		}
		getSharedData = type.CreateDelegate<GetObjectDelegate>("GetSharedData", instance, new Type[1] { typeof(string) });
		setSharedData = type.CreateDelegate<SetObjectDelegate>("SetSharedData", instance, new Type[2]
		{
			typeof(string),
			typeof(object)
		});
		if (getSharedData == null || setSharedData == null)
		{
			throw new ArgumentException("Remote instance missing shared data");
		}
	}

	public void AddCandidateVersion(PForwardedComponent instance)
	{
		addCandidateVersion(instance);
	}

	public IEnumerable<PForwardedComponent> GetAllComponents(string id)
	{
		ICollection<PForwardedComponent> collection = null;
		ICollection collection2 = getAllComponents(id);
		if (collection2 != null)
		{
			collection = new List<PForwardedComponent>(collection2.Count);
			foreach (object item2 in collection2)
			{
				if (item2 is PForwardedComponent item)
				{
					collection.Add(item);
				}
				else
				{
					collection.Add(new PRemoteComponent(item2));
				}
			}
		}
		return collection;
	}

	public PForwardedComponent GetLatestVersion(string id)
	{
		if (!remoteComponents.TryGetValue(id, out var value))
		{
			object obj = getLatestVersion(id);
			value = ((obj == null) ? null : ((!(obj is PForwardedComponent pForwardedComponent)) ? new PRemoteComponent(obj) : pForwardedComponent));
			remoteComponents.Add(id, value);
		}
		return value;
	}

	public object GetSharedData(string id)
	{
		return getSharedData(id);
	}

	public void SetSharedData(string id, object data)
	{
		setSharedData(id, data);
	}
}
