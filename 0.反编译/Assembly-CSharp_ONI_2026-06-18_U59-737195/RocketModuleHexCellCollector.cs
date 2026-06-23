using System.Collections.Generic;
using KSerialization;
using UnityEngine;

public class RocketModuleHexCellCollector : GameStateMachine<RocketModuleHexCellCollector, RocketModuleHexCellCollector.Instance, IStateMachineTarget, RocketModuleHexCellCollector.Def>
{
	public class Def : BaseDef
	{
		public float collectSpeed;

		public bool formatCapacityBarAsUnits;

		public List<Tag> forbiddenTags;
	}

	public class InSpaceStates : State
	{
		public State idle;

		public State collecting;
	}

	private class ItemData
	{
		public Tag ItemID;

		public float Mass;

		public float Proportion;

		public float massPerUnit;

		public bool usesUnits;

		public bool isValid;

		public void Clear()
		{
			ItemID = null;
			Mass = 0f;
			Proportion = 0f;
			isValid = false;
			usesUnits = false;
			massPerUnit = 1f;
		}
	}

	public new class Instance : GameInstance, IHexCellCollector
	{
		[Serialize]
		public int LastCollectedIndex;

		[Serialize]
		public float MassCharge;

		public Storage storage;

		public TreeFilterable treeFilterable;

		private Clustercraft clustercraft;

		public bool IsCollecting => IsInsideState(base.sm.space.collecting);

		public bool IsSpaceshipMoving => clustercraft.IsFlightInProgress();

		public AxialI StarmapLocation => clustercraft.Location;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			storage = GetComponent<Storage>();
			treeFilterable = null;
		}

		public override void StartSM()
		{
			clustercraft = GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<Clustercraft>();
			base.sm.ClusterCraft.Set(clustercraft.gameObject, this);
			base.StartSM();
		}

		public void TriggerHexCellStorageChangeEvent(object o)
		{
			base.sm.HexCellInventoryChangedSignal.Trigger(base.smi);
		}

		public void ToggleCollectingTag(bool collecting)
		{
			if (collecting)
			{
				clustercraft.AddTag(GameTags.RocketCollectingResources);
				return;
			}
			List<Instance> allHexCellCollectorModules = clustercraft.GetAllHexCellCollectorModules();
			bool flag = false;
			foreach (Instance item in allHexCellCollectorModules)
			{
				if (item != this && item != null && item.IsCollecting)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				clustercraft.RemoveTag(GameTags.RocketCollectingResources);
			}
		}

		public bool CheckIsCollecting()
		{
			return IsInsideState(base.sm.space.collecting);
		}

		public string GetProperName()
		{
			return GetComponent<RocketModuleCluster>().GetProperName();
		}

		public Sprite GetUISprite()
		{
			return global::Def.GetUISprite(base.master.gameObject.GetComponent<KPrefabID>().PrefabID()).first;
		}

		public float GetCapacity()
		{
			return storage.Capacity();
		}

		public float GetMassStored()
		{
			return storage.MassStored();
		}

		public float TimeInState()
		{
			return timeinstate;
		}

		public string GetCapacityBarText()
		{
			if (base.def.formatCapacityBarAsUnits)
			{
				return GameUtil.GetFormattedUnits(GetMassStored()) + " / " + GameUtil.GetFormattedUnits(GetCapacity());
			}
			return GameUtil.GetFormattedMass(GetMassStored()) + " / " + GameUtil.GetFormattedMass(GetCapacity());
		}
	}

	public State ground;

	public InSpaceStates space;

	public Signal HexCellInventoryChangedSignal;

	public TargetParameter ClusterCraft;

	public TargetParameter HexCellInventory;

	private static List<ItemData> ItemDataObjects = new List<ItemData>
	{
		new ItemData(),
		new ItemData(),
		new ItemData(),
		new ItemData(),
		new ItemData(),
		new ItemData()
	};

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.Never;
		default_state = ground;
		ground.TagTransition(GameTags.RocketNotOnGround, space).Enter(ClearHexCellInventoryChangeCallbacks);
		space.TagTransition(GameTags.RocketNotOnGround, ground, on_remove: true).Enter(RefreshHexCellInventoryChangeCallbacks).EventHandler(GameHashes.ClusterLocationChanged, RefreshHexCellInventoryChangeCallbacks)
			.EventHandler(GameHashes.ClusterDestinationReached, RefreshHexCellInventoryChangeCallbacks)
			.DefaultState(space.idle);
		space.idle.OnSignal(HexCellInventoryChangedSignal, space.collecting, CanCollect).EventHandlerTransition(GameHashes.ClusterLocationChanged, space.collecting, CanCollect).EventHandlerTransition(GameHashes.ClusterDestinationReached, space.collecting, CanCollect)
			.Target(ClusterCraft)
			.EventHandlerTransition(GameHashes.ClusterDestinationChanged, space.collecting, CanCollect);
		space.collecting.Toggle("ToggleCollectingTag", AddCollectingTag, RemoveCollectingTag).UpdateTransition(space.idle, CollectUpdate, UpdateRate.SIM_1000ms).Exit(ClearMassCharge);
	}

	public static void ClearHexCellInventoryChangeCallbacks(Instance smi)
	{
		GameObject gameObject = smi.sm.HexCellInventory.Get(smi);
		if (gameObject != null)
		{
			gameObject.Unsubscribe(-1697596308, smi.TriggerHexCellStorageChangeEvent);
			smi.sm.HexCellInventory.Set(null, smi);
		}
	}

	public static void RefreshHexCellInventoryChangeCallbacks(Instance smi)
	{
		GameObject gameObject = smi.sm.HexCellInventory.Get(smi);
		if (gameObject != null)
		{
			gameObject.Unsubscribe(-1697596308, smi.TriggerHexCellStorageChangeEvent);
		}
		StarmapHexCellInventory starmapHexCellInventory = ClusterGrid.Instance.AddOrGetHexCellInventory(smi.StarmapLocation);
		smi.sm.HexCellInventory.Set(starmapHexCellInventory.gameObject, smi);
		if (starmapHexCellInventory != null)
		{
			starmapHexCellInventory.gameObject.Subscribe(-1697596308, smi.TriggerHexCellStorageChangeEvent);
		}
	}

	public static bool CanCollect(Instance smi, object o)
	{
		return CanCollect(smi);
	}

	public static bool CanCollect(Instance smi)
	{
		if (smi.storage.RemainingCapacity() <= 0f)
		{
			return false;
		}
		StarmapHexCellInventory starmapHexCellInventory = ClusterGrid.Instance.AddOrGetHexCellInventory(smi.StarmapLocation);
		bool flag = starmapHexCellInventory.TotalMass > 0f;
		if (smi.IsSpaceshipMoving)
		{
			return false;
		}
		if (!flag)
		{
			return false;
		}
		foreach (StarmapHexCellInventory.SerializedItem item in starmapHexCellInventory.Items)
		{
			if (CanHexCellItemBeStored(item, smi, out var _, out var _))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CollectUpdate(Instance smi, float dt)
	{
		if (dt == 0f)
		{
			return false;
		}
		Storage storage = smi.storage;
		float num = storage.RemainingCapacity();
		if (num <= 0f)
		{
			return true;
		}
		StarmapHexCellInventory starmapHexCellInventory = ClusterGrid.Instance.AddOrGetHexCellInventory(smi.StarmapLocation);
		bool flag = starmapHexCellInventory.TotalMass > 0f;
		if (smi.IsSpaceshipMoving)
		{
			return true;
		}
		if (!flag)
		{
			return true;
		}
		float b = smi.MassCharge + dt * smi.def.collectSpeed;
		smi.MassCharge = 0f;
		b = Mathf.Min(num, b);
		int count = starmapHexCellInventory.Items.Count;
		float num2 = b;
		float num3 = 0f;
		bool flag2 = false;
		ClearAllItemData();
		if (ItemDataObjects.Count < count)
		{
			int num4 = count - ItemDataObjects.Count;
			for (int i = 0; i < num4; i++)
			{
				ItemDataObjects.Add(new ItemData());
			}
		}
		float num5 = 0f;
		for (int j = 0; j < count; j++)
		{
			ItemData itemData = ItemDataObjects[j];
			itemData.Clear();
			StarmapHexCellInventory.SerializedItem serializedItem = starmapHexCellInventory.Items[j];
			bool itemUsesUnits = false;
			float massPerUnit = 1f;
			bool flag3 = CanHexCellItemBeStored(serializedItem, smi, out itemUsesUnits, out massPerUnit);
			itemData.ItemID = serializedItem.ID;
			itemData.Mass = serializedItem.Mass;
			itemData.massPerUnit = massPerUnit;
			itemData.usesUnits = itemUsesUnits;
			itemData.isValid = flag3;
			num5 += (flag3 ? serializedItem.Mass : 0f);
		}
		for (int k = 0; k < count; k++)
		{
			ItemData itemData2 = ItemDataObjects[k];
			if (!itemData2.isValid)
			{
				continue;
			}
			itemData2.Proportion = itemData2.Mass / num5;
			float num6 = itemData2.Proportion * b;
			if (!itemData2.usesUnits || num6 >= itemData2.massPerUnit)
			{
				float mass = num6;
				if (itemData2.usesUnits)
				{
					mass = (float)Mathf.FloorToInt(num6 / itemData2.massPerUnit) * itemData2.massPerUnit;
				}
				float num7 = starmapHexCellInventory.ExtractAndStoreItemMass(itemData2.ItemID, mass, storage);
				num2 -= num7;
				num3 += num7;
			}
			else
			{
				flag2 = true;
			}
		}
		if (flag2)
		{
			smi.MassCharge += num2;
		}
		if (!(storage.RemainingCapacity() <= 0f))
		{
			if (!flag2)
			{
				return num3 <= 0f;
			}
			return false;
		}
		return true;
	}

	private static bool CanHexCellItemBeStored(StarmapHexCellInventory.SerializedItem item, Instance smi, out bool itemUsesUnits, out float massPerUnit)
	{
		itemUsesUnits = false;
		massPerUnit = 1f;
		GameObject prefab = Assets.GetPrefab(item.ID);
		if (prefab != null)
		{
			KPrefabID component = prefab.GetComponent<KPrefabID>();
			bool flag = false;
			if (smi.treeFilterable != null)
			{
				foreach (Tag tag in smi.treeFilterable.GetTags())
				{
					if (component.HasTag(tag))
					{
						flag = true;
						break;
					}
				}
			}
			else
			{
				flag = component.HasAnyTags(smi.storage.storageFilters);
			}
			if (flag && smi.def.forbiddenTags != null)
			{
				foreach (Tag forbiddenTag in smi.def.forbiddenTags)
				{
					if (component.HasTag(forbiddenTag))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				Element element = ElementLoader.GetElement(component.PrefabID());
				PrimaryElement component2 = prefab.GetComponent<PrimaryElement>();
				itemUsesUnits = element == null && component2 != null && GameTags.DisplayAsUnits.Contains(item.ID);
				massPerUnit = ((component2 == null) ? 1f : component2.MassPerUnit);
				if (!itemUsesUnits || (item.Mass >= component2.MassPerUnit && smi.storage.RemainingCapacity() >= component2.MassPerUnit))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void RemoveCollectingTag(Instance smi)
	{
		ToggleCollectingTag(smi, v: false);
	}

	public static void AddCollectingTag(Instance smi)
	{
		ToggleCollectingTag(smi, v: true);
	}

	public static void ToggleCollectingTag(Instance smi, bool v)
	{
		smi.ToggleCollectingTag(v);
	}

	public static void ClearMassCharge(Instance smi)
	{
		smi.MassCharge = 0f;
	}

	private static void ClearAllItemData()
	{
		foreach (ItemData itemDataObject in ItemDataObjects)
		{
			itemDataObject.Clear();
		}
	}
}
