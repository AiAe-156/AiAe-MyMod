using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract class ClosestPickupableSensor<T> : Sensor where T : Component
{
	public Action<T> OnItemChanged;

	protected T item;

	protected int itemNavCost = int.MaxValue;

	protected Tag itemSearchTag;

	protected Tag[] requiredTags;

	protected bool isThereAnyItemAvailable;

	protected bool itemInReachButNotPermitted;

	private Navigator navigator;

	protected ConsumableConsumer consumableConsumer;

	private Storage storage;

	public ClosestPickupableSensor(Sensors sensors, Tag itemSearchTag, bool shouldStartActive)
		: base(sensors, shouldStartActive)
	{
		navigator = GetComponent<Navigator>();
		consumableConsumer = GetComponent<ConsumableConsumer>();
		storage = GetComponent<Storage>();
		this.itemSearchTag = itemSearchTag;
	}

	public T GetItem()
	{
		return item;
	}

	public int GetItemNavCost()
	{
		return (item == null) ? int.MaxValue : itemNavCost;
	}

	public virtual HashSet<Tag> GetForbbidenTags()
	{
		return (consumableConsumer == null) ? new HashSet<Tag>(0) : consumableConsumer.forbiddenTagSet;
	}

	public override void Update()
	{
		HashSet<Tag> forbbidenTags = GetForbbidenTags();
		int cost = int.MaxValue;
		Pickupable pickupable = FindClosestPickupable(storage, forbbidenTags, out cost, itemSearchTag, requiredTags);
		bool flag = itemInReachButNotPermitted;
		T val = null;
		bool flag2 = false;
		if (pickupable != null)
		{
			val = pickupable.GetComponent<T>();
			flag2 = true;
			flag = false;
		}
		else
		{
			int cost2;
			Pickupable pickupable2 = FindClosestPickupable(storage, new HashSet<Tag>(), out cost2, itemSearchTag, requiredTags);
			flag = pickupable2 != null;
		}
		if (val != item || isThereAnyItemAvailable != flag2)
		{
			item = val;
			itemNavCost = cost;
			isThereAnyItemAvailable = flag2;
			itemInReachButNotPermitted = flag;
			ItemChanged();
		}
	}

	public Pickupable FindClosestPickupable(Storage destination, HashSet<Tag> exclude_tags, out int cost, Tag categoryTag, Tag[] otherRequiredTags = null)
	{
		WorldContainer myWorld = base.gameObject.GetMyWorld();
		List<Pickupable> pickupables = CollectionPool<List<Pickupable>, Pickupable>.Get();
		myWorld.worldInventory.GetPickupablesFromRelatedWorlds(categoryTag, ref pickupables);
		if (pickupables == null || pickupables.Count == 0)
		{
			cost = int.MaxValue;
			CollectionPool<List<Pickupable>, Pickupable>.Release(pickupables);
			return null;
		}
		if (otherRequiredTags == null)
		{
			otherRequiredTags = new Tag[1] { categoryTag };
		}
		Pickupable result = null;
		int num = int.MaxValue;
		foreach (Pickupable item in pickupables)
		{
			if (FetchManager.IsFetchablePickup_Exclude(item.KPrefabID, item.storage, item.UnreservedFetchAmount, exclude_tags, otherRequiredTags, destination))
			{
				int navigationCost = item.GetNavigationCost(navigator, item.cachedCell);
				if (navigationCost != -1 && navigationCost < num)
				{
					result = item;
					num = navigationCost;
				}
			}
		}
		cost = num;
		CollectionPool<List<Pickupable>, Pickupable>.Release(pickupables);
		return result;
	}

	public virtual void ItemChanged()
	{
		OnItemChanged?.Invoke(item);
	}
}
