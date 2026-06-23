using System.Collections.Generic;
using System.Runtime.Serialization;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/RationTracker")]
public class RationTracker : WorldResourceAmountTracker<RationTracker>, ISaveLoadable
{
	[Serialize]
	public Dictionary<string, float> caloriesConsumedByFood = new Dictionary<string, float>();

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		itemTag = GameTags.Edible;
	}

	[OnDeserialized]
	private void OnDeserialized()
	{
		if (caloriesConsumedByFood != null && caloriesConsumedByFood.Count > 0)
		{
			foreach (string key in caloriesConsumedByFood.Keys)
			{
				float num = caloriesConsumedByFood[key];
				float value = 0f;
				if (amountsConsumedByID.TryGetValue(key, out value))
				{
					amountsConsumedByID[key] = value + num;
				}
				else
				{
					amountsConsumedByID.Add(key, num);
				}
			}
		}
		caloriesConsumedByFood = null;
	}

	protected override ItemData GetItemData(Pickupable item)
	{
		Edible component = item.GetComponent<Edible>();
		return new ItemData
		{
			ID = component.FoodID,
			amountValue = component.Calories,
			units = component.Units
		};
	}

	public float GetAmountConsumed()
	{
		float num = 0f;
		foreach (KeyValuePair<string, float> item in amountsConsumedByID)
		{
			num += item.Value;
		}
		return num;
	}

	public float GetAmountConsumedForIDs(List<string> itemIDs)
	{
		float num = 0f;
		foreach (string itemID in itemIDs)
		{
			if (amountsConsumedByID.ContainsKey(itemID))
			{
				num += amountsConsumedByID[itemID];
			}
		}
		return num;
	}

	public float CountAmountForItemWithID(string ID, WorldInventory inventory, bool excludeUnreachable = true)
	{
		float num = 0f;
		ICollection<Pickupable> pickupables = inventory.GetPickupables(itemTag);
		if (pickupables != null)
		{
			foreach (Pickupable item in pickupables)
			{
				if (!item.KPrefabID.HasTag(GameTags.StoredPrivate))
				{
					ItemData itemData = GetItemData(item);
					if (itemData.ID == ID)
					{
						num += itemData.amountValue;
					}
				}
			}
		}
		return num;
	}
}
