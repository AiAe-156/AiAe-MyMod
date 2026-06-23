using System.Collections.Generic;
using System.Runtime.Serialization;
using KSerialization;
using UnityEngine;

public class StarmapHexCellInventory : KMonoBehaviour, ISaveLoadable
{
	[SerializationConfig(MemberSerialization.OptIn)]
	public class SerializedItem
	{
		[Serialize]
		public Tag ID;

		[Serialize]
		public float Mass;

		private Element.State stateMask;

		public Element.State StateMask => stateMask;

		public bool IsSolid => (stateMask & Element.State.Solid) != 0;

		public bool IsLiquid => (stateMask & Element.State.Liquid) != 0;

		public bool IsGas => (stateMask & Element.State.Gas) != 0;

		public bool IsEntity => stateMask == Element.State.Vacuum;

		public Element.State ItemMatterState => stateMask;

		public SerializedItem(Tag id, float mass)
			: this(id, mass, Element.State.Vacuum)
		{
		}

		public SerializedItem(Tag id, float mass, Element.State state)
		{
			ID = id;
			Mass = mass;
			stateMask = state;
		}

		public void RecalculateState()
		{
			Element element = ElementLoader.GetElement(ID);
			if (element == null)
			{
				stateMask = Element.State.Vacuum;
			}
			else
			{
				stateMask = element.state;
			}
		}
	}

	public static Dictionary<AxialI, StarmapHexCellInventory> AllInventories = new Dictionary<AxialI, StarmapHexCellInventory>();

	[Serialize]
	public List<SerializedItem> Items = new List<SerializedItem>();

	public int ItemCount
	{
		get
		{
			if (Items != null)
			{
				return Items.Count;
			}
			return 0;
		}
	}

	public float TotalMass => ReadTotalMass();

	public static void ClearStatics()
	{
		AllInventories.Clear();
	}

	public bool RegisterInventory(AxialI location)
	{
		StarmapHexCellInventory value = null;
		if (!AllInventories.TryGetValue(location, out value) || value == this)
		{
			AllInventories[location] = this;
			return true;
		}
		return false;
	}

	public void TransferAllItemsFromExternalInventory(StarmapHexCellInventory externalInventory)
	{
		bool flag = false;
		foreach (SerializedItem item in externalInventory.Items)
		{
			bool flag2 = TransferItemFromGroup(item.ID, item.Mass, item.StateMask) != null;
			flag = flag || flag2;
		}
		if (flag)
		{
			base.gameObject.Trigger(-1697596308);
		}
		externalInventory.DeleteAll();
	}

	private SerializedItem TransferItemFromGroup(Tag itemID, float mass, Element.State state)
	{
		return AddItem(itemID, mass, state, triggerStorageChangeCb: false);
	}

	public SerializedItem AddItem(Element element, float mass)
	{
		return AddItem(element.id.CreateTag(), mass, element.state);
	}

	public SerializedItem AddItem(Tag itemID, float mass, Element.State state)
	{
		return AddItem(itemID, mass, state, triggerStorageChangeCb: true);
	}

	private SerializedItem AddItem(Tag itemID, float mass, Element.State state, bool triggerStorageChangeCb)
	{
		SerializedItem serializedItem = FindItem(itemID);
		if (serializedItem == null)
		{
			serializedItem = new SerializedItem(itemID, 0f, state);
			Items.Add(serializedItem);
		}
		serializedItem.Mass += mass;
		if (triggerStorageChangeCb)
		{
			base.gameObject.Trigger(-1697596308);
		}
		return serializedItem;
	}

	public PrimaryElement ExtractAndSpawnItem(Tag ID)
	{
		return ExtractAndSpawnItemMass(ID, float.MaxValue);
	}

	public PrimaryElement ExtractAndSpawnItemMass(Tag ID, float mass)
	{
		GameObject gameObject = null;
		PrimaryElement primaryElement = null;
		SerializedItem serializedItem = FindItem(ID);
		if (serializedItem != null)
		{
			float num = Mathf.Min(mass, serializedItem.Mass);
			Element element = ElementLoader.GetElement(ID);
			Vector3 position = base.transform.GetPosition();
			if (num <= 0f)
			{
				Debug.LogWarning("StarmapHexCellInventory.ExtractAndSpawn() found an invalid mass to extract from item ID(" + ID.ToString() + "). If the stored item had zero mass, it will be removed now");
				if (serializedItem.Mass <= 0f)
				{
					DeleteItem(serializedItem);
					return null;
				}
			}
			if (element != null)
			{
				if (element.IsGas)
				{
					gameObject = GasSourceManager.Instance.CreateChunk(element, num, element.defaultValues.temperature, byte.MaxValue, 0, position).gameObject;
				}
				else if (element.IsLiquid)
				{
					gameObject = LiquidSourceManager.Instance.CreateChunk(element, num, element.defaultValues.temperature, byte.MaxValue, 0, position).gameObject;
				}
				else if (element.IsSolid)
				{
					gameObject = element.substance.SpawnResource(position, num, element.defaultValues.temperature, byte.MaxValue, 0, prevent_merge: true, forceTemperature: false, manual_activation: true);
					gameObject.GetComponent<Pickupable>().prevent_absorb_until_stored = true;
					element.substance.ActivateSubstanceGameObject(gameObject, byte.MaxValue, 0);
				}
				primaryElement = gameObject.GetComponent<PrimaryElement>();
				primaryElement.KeepZeroMassObject = false;
			}
			else
			{
				GameObject prefab = Assets.GetPrefab(serializedItem.ID);
				if (!(prefab != null))
				{
					Debug.LogWarning("StarmapHexCellInventory.ExtractAndSpawn() found an invalid item ID(" + ID.ToString() + ") stored. Removing from list.");
					DeleteItem(serializedItem);
					return null;
				}
				gameObject = Util.KInstantiate(prefab, base.transform.gameObject);
				gameObject.transform.SetLocalPosition(position);
				primaryElement = gameObject.GetComponent<PrimaryElement>();
				primaryElement.Units = num;
				gameObject.SetActive(value: true);
			}
			if (primaryElement != null)
			{
				DeleteItemMass(serializedItem, num);
			}
		}
		return primaryElement;
	}

	public float ExtractAndStoreItemMass(Tag ID, float mass, Storage storage)
	{
		SerializedItem serializedItem = FindItem(ID);
		if (serializedItem == null)
		{
			return 0f;
		}
		float num = Mathf.Min(mass, serializedItem.Mass);
		if (num <= 0f)
		{
			Debug.LogWarning("StarmapHexCellInventory.ExtractAndSpawn() found an invalid mass to extract from item ID(" + ID.ToString() + "). If the stored item had zero mass, it will be removed now");
			if (serializedItem.Mass <= 0f)
			{
				DeleteItem(serializedItem);
				return 0f;
			}
		}
		Element element = ElementLoader.GetElement(ID);
		if (element != null)
		{
			DeleteItemMass(serializedItem, num);
			storage.AddElement(element.id, num, element.defaultValues.temperature, byte.MaxValue, 0);
			return num;
		}
		GameObject prefab = Assets.GetPrefab(serializedItem.ID);
		if (prefab != null)
		{
			GameObject gameObject = Util.KInstantiate(prefab, base.transform.gameObject);
			gameObject.transform.SetLocalPosition(base.transform.GetPosition());
			gameObject.GetComponent<PrimaryElement>().Units = num;
			gameObject.SetActive(value: true);
			DeleteItemMass(serializedItem, num);
			storage.Store(gameObject, hide_popups: true);
			return num;
		}
		Debug.LogWarning("StarmapHexCellInventory.ExtractAndSpawn() found an invalid item ID(" + ID.ToString() + ") stored. Removing from list.");
		DeleteItem(serializedItem);
		return 0f;
	}

	private void DeleteAll()
	{
		Items.Clear();
		base.gameObject.Trigger(-1697596308);
	}

	private void DeleteItem(SerializedItem item)
	{
		DeleteItemMass(item, item.Mass);
	}

	private void DeleteItemMass(SerializedItem item, float massToDelete)
	{
		if (item != null)
		{
			item.Mass -= massToDelete;
			if (item.Mass <= 0f)
			{
				Items.Remove(item);
			}
			base.gameObject.Trigger(-1697596308);
		}
	}

	private void RefreshStatusItems(object data = null)
	{
		GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().MiscStatusItems.ClusterMapHarvestableResource, Items);
	}

	private SerializedItem FindItem(Tag id)
	{
		if (Items != null)
		{
			return Items.Find((SerializedItem i) => i.ID == id);
		}
		return null;
	}

	private float ReadTotalMass()
	{
		if (Items == null || Items.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		foreach (SerializedItem item in Items)
		{
			num += item.Mass;
		}
		return num;
	}

	[OnDeserialized]
	internal void OnDeserializedMethod()
	{
		if (Items == null)
		{
			return;
		}
		Items.RemoveAll((SerializedItem x) => Assets.TryGetPrefab(x.ID) == null);
		foreach (SerializedItem item in Items)
		{
			item.RecalculateState();
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Subscribe(-1697596308, RefreshStatusItems);
		RefreshStatusItems();
	}
}
