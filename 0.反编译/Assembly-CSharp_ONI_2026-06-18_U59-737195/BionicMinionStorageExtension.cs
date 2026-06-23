using System.Collections.Generic;
using UnityEngine;

public class BionicMinionStorageExtension : KMonoBehaviour, StoredMinionIdentity.IStoredMinionExtension
{
	private static readonly List<Tag> StoragesTypesToTransfer = new List<Tag>
	{
		GameTags.StoragesIds.BionicBatteryStorage,
		GameTags.StoragesIds.BionicUpgradeStorage,
		GameTags.StoragesIds.BionicOxygenTankStorage
	};

	public void AddStoredMinionGameObjectRequirements(GameObject storedMinionGameObject)
	{
		Storage[] components = storedMinionGameObject.GetComponents<Storage>();
		foreach (Tag inventoryType in StoragesTypesToTransfer)
		{
			if (components == null || !(components.FindFirst((Storage s) => s.storageID == inventoryType) != null))
			{
				Storage storage = storedMinionGameObject.AddComponent<Storage>();
				storage.allowItemRemoval = false;
				storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
				storage.storageID = inventoryType;
			}
		}
	}

	void StoredMinionIdentity.IStoredMinionExtension.PullFrom(StoredMinionIdentity source)
	{
		Storage[] components = source.GetComponents<Storage>();
		Storage[] components2 = GetComponents<Storage>();
		Storage[] array = components;
		foreach (Storage storage in array)
		{
			bool test = false;
			Storage[] array2 = components2;
			foreach (Storage storage2 in array2)
			{
				if (storage2.storageID == storage.storageID)
				{
					storage.Transfer(storage2, block_events: false, hide_popups: true);
					test = true;
					break;
				}
			}
			DebugUtil.DevAssert(test, "Missmatched storages on BionicMinionStorageExtension");
		}
	}

	void StoredMinionIdentity.IStoredMinionExtension.PushTo(StoredMinionIdentity destination)
	{
		GameObject gameObject = destination.gameObject;
		AddStoredMinionGameObjectRequirements(gameObject);
		Storage[] components = GetComponents<Storage>();
		Storage[] components2 = gameObject.GetComponents<Storage>();
		foreach (Tag item in StoragesTypesToTransfer)
		{
			Storage storage = null;
			Storage target = null;
			Storage[] array = components;
			foreach (Storage storage2 in array)
			{
				if (storage2.storageID == item)
				{
					storage = storage2;
					break;
				}
			}
			array = components2;
			foreach (Storage storage3 in array)
			{
				if (storage3.storageID == item)
				{
					target = storage3;
					break;
				}
			}
			storage.Transfer(target, block_events: true, hide_popups: true);
		}
	}
}
