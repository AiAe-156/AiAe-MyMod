using System;
using System.Collections.Generic;

public class DiscoverResources : KMonoBehaviour
{
	[Serializable]
	public struct Resource
	{
		public Tag prefabId;

		public Tag categoryTag;
	}

	public List<Resource> resourcesToDiscover;

	public void Add(Tag prefabId, Tag categoryTag)
	{
		if (resourcesToDiscover == null)
		{
			resourcesToDiscover = new List<Resource>();
		}
		resourcesToDiscover.Add(new Resource
		{
			prefabId = prefabId,
			categoryTag = categoryTag
		});
	}

	protected override void OnPrefabInit()
	{
		if (resourcesToDiscover == null)
		{
			return;
		}
		foreach (Resource item in resourcesToDiscover)
		{
			DiscoveredResources.Instance.Discover(item.prefabId, item.categoryTag);
		}
	}
}
