using UnityEngine;

public class FetchDrone : KMonoBehaviour
{
	private static string BOTTOM = "bottom";

	private static string BOTTOM_CARRY = "bottom_carry";

	private KBatchedAnimController animController;

	private Storage pickupableStorage;

	[MyCmpAdd]
	private ChoreConsumer choreConsumer;

	protected override void OnSpawn()
	{
		ChoreGroup[] array = new ChoreGroup[15]
		{
			Db.Get().ChoreGroups.Build,
			Db.Get().ChoreGroups.Basekeeping,
			Db.Get().ChoreGroups.Cook,
			Db.Get().ChoreGroups.Art,
			Db.Get().ChoreGroups.Dig,
			Db.Get().ChoreGroups.Research,
			Db.Get().ChoreGroups.Farming,
			Db.Get().ChoreGroups.Ranching,
			Db.Get().ChoreGroups.MachineOperating,
			Db.Get().ChoreGroups.MedicalAid,
			Db.Get().ChoreGroups.Combat,
			Db.Get().ChoreGroups.LifeSupport,
			Db.Get().ChoreGroups.Recreation,
			Db.Get().ChoreGroups.Toggle,
			Db.Get().ChoreGroups.Rocketry
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				choreConsumer.SetPermittedByUser(array[i], is_allowed: false);
			}
		}
		Storage[] components = GetComponents<Storage>();
		foreach (Storage storage in components)
		{
			if (storage.storageID != GameTags.ChargedPortableBattery)
			{
				pickupableStorage = storage;
				break;
			}
		}
		animController = GetComponent<KBatchedAnimController>();
		pickupableStorage.Subscribe(-1697596308, OnStorageChanged);
		Subscribe(-1582839653, OnTagsChanged);
	}

	protected override void OnCleanUp()
	{
		Unsubscribe(-1697596308);
		Unsubscribe(-1582839653);
		base.OnCleanUp();
	}

	private void OnTagsChanged(object data)
	{
		TagChangedEventData value = ((Boxed<TagChangedEventData>)data).value;
		if (value.added && value.tag == GameTags.Creatures.Die)
		{
			Brain component = GetComponent<Brain>();
			if (component != null && !component.IsRunning())
			{
				component.Resume("death");
			}
		}
	}

	private void OnStorageChanged(object data)
	{
		GameObject gameObject = (GameObject)data;
		RemoveTracker(gameObject);
		ShowPickupSymbol(gameObject);
	}

	private void ShowPickupSymbol(GameObject pickupable)
	{
		bool flag = pickupableStorage.items.Contains(pickupable);
		if (flag)
		{
			AddAnimTracker(pickupable);
		}
		animController.SetSymbolVisiblity(BOTTOM, !flag);
		animController.SetSymbolVisiblity(BOTTOM_CARRY, flag);
	}

	private void AddAnimTracker(GameObject go)
	{
		KAnimControllerBase component = go.GetComponent<KAnimControllerBase>();
		if (!(component == null) && component.AnimFiles != null && component.AnimFiles.Length != 0 && component.AnimFiles[0] != null && component.GetComponent<Pickupable>().trackOnPickup)
		{
			KBatchedAnimTracker component2 = go.GetComponent<KBatchedAnimTracker>();
			if (!(component2 != null) || !(component2.controller == animController))
			{
				component2 = go.AddComponent<KBatchedAnimTracker>();
				component2.useTargetPoint = false;
				component2.fadeOut = false;
				component2.symbol = ((go.GetComponent<Brain>() != null) ? new HashedString("snapTo_pivot") : new HashedString("snapTo_thing"));
				component2.forceAlwaysVisible = true;
			}
		}
	}

	private void RemoveTracker(GameObject go)
	{
		KBatchedAnimTracker kBatchedAnimTracker = ((go != null) ? go.GetComponent<KBatchedAnimTracker>() : null);
		if (kBatchedAnimTracker != null && kBatchedAnimTracker.controller == animController)
		{
			Object.Destroy(kBatchedAnimTracker);
		}
	}
}
