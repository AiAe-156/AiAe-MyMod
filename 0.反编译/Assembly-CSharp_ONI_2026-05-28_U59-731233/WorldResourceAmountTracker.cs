using System.Collections.Generic;
using KSerialization;

[SerializationConfig(MemberSerialization.OptIn)]
public abstract class WorldResourceAmountTracker<T> : KMonoBehaviour where T : KMonoBehaviour
{
	protected struct ItemData
	{
		public string ID;

		public float amountValue;

		public float units;
	}

	public struct Frame
	{
		public float amountProduced;

		public float amountConsumed;
	}

	private static T instance;

	[Serialize]
	public Frame currentFrame = default(Frame);

	[Serialize]
	public Frame previousFrame = default(Frame);

	[Serialize]
	public Dictionary<string, float> amountsConsumedByID = new Dictionary<string, float>();

	protected Tag itemTag;

	protected Tag[] ignoredTags;

	public static void DestroyInstance()
	{
		instance = null;
	}

	public static T Get()
	{
		return instance;
	}

	protected override void OnPrefabInit()
	{
		Debug.Assert(instance == null, "Error, WorldResourceAmountTracker of type T has already been initialize and another instance is attempting to initialize. this isn't allowed because T is meant to be a singleton, ensure only one instance exist. existing instance GameObject: " + ((instance == null) ? "" : instance.gameObject.name) + ". Error triggered by instance of T in GameObject: " + base.gameObject.name);
		instance = this as T;
		itemTag = GameTags.Edible;
	}

	protected override void OnSpawn()
	{
		Subscribe(631075836, OnNewDay);
	}

	private void OnNewDay(object data)
	{
		previousFrame = currentFrame;
		currentFrame = default(Frame);
	}

	protected abstract ItemData GetItemData(Pickupable item);

	public float CountAmount(Dictionary<string, float> unitCountByID, WorldInventory inventory, bool excludeUnreachable = true)
	{
		float totalUnitsFound;
		return CountAmount(unitCountByID, out totalUnitsFound, inventory, excludeUnreachable);
	}

	public float CountAmount(Dictionary<string, float> unitCountByID, out float totalUnitsFound, WorldInventory inventory, bool excludeUnreachable)
	{
		float num = 0f;
		totalUnitsFound = 0f;
		ICollection<Pickupable> pickupables = inventory.GetPickupables(itemTag);
		if (pickupables != null)
		{
			foreach (Pickupable item in pickupables)
			{
				if (item.KPrefabID.HasTag(GameTags.StoredPrivate))
				{
					continue;
				}
				if (ignoredTags != null)
				{
					bool flag = false;
					Tag[] array = ignoredTags;
					foreach (Tag tag in array)
					{
						if (item.KPrefabID.HasTag(tag))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				ItemData itemData = GetItemData(item);
				num += itemData.amountValue;
				if (unitCountByID != null)
				{
					if (!unitCountByID.ContainsKey(itemData.ID))
					{
						unitCountByID[itemData.ID] = 0f;
					}
					unitCountByID[itemData.ID] += itemData.units;
				}
				totalUnitsFound += itemData.units;
			}
		}
		return num;
	}

	public void RegisterAmountProduced(float val)
	{
		currentFrame.amountProduced += val;
	}

	public void RegisterAmountConsumed(string ID, float valueConsumed)
	{
		currentFrame.amountConsumed += valueConsumed;
		if (!amountsConsumedByID.ContainsKey(ID))
		{
			amountsConsumedByID.Add(ID, valueConsumed);
		}
		else
		{
			amountsConsumedByID[ID] += valueConsumed;
		}
	}
}
