using System.Collections.Generic;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/SolidConduitDispenser")]
public class SolidConduitDispenser : KMonoBehaviour, ISaveLoadable, IConduitDispenser
{
	[SerializeField]
	public SimHashes[] elementFilter;

	[SerializeField]
	public bool invertElementFilter;

	[SerializeField]
	public bool alwaysDispense;

	[SerializeField]
	public bool useSecondaryOutput;

	[SerializeField]
	public bool solidOnly;

	private static readonly Operational.Flag outputConduitFlag = new Operational.Flag("output_conduit", Operational.Flag.Type.Functional);

	[MyCmpReq]
	private Operational operational;

	[MyCmpReq]
	public Storage storage;

	private HandleVector<int>.Handle partitionerEntry;

	private int utilityCell = -1;

	private bool dispensing;

	private int round_robin_index;

	public Storage Storage => storage;

	public ConduitType ConduitType => ConduitType.Solid;

	public SolidConduitFlow.ConduitContents ConduitContents => GetConduitFlow().GetContents(utilityCell);

	public bool IsDispensing => dispensing;

	public bool IsConnected
	{
		get
		{
			GameObject gameObject = Grid.Objects[utilityCell, 20];
			if (gameObject != null)
			{
				return gameObject.GetComponent<BuildingComplete>() != null;
			}
			return false;
		}
	}

	public SolidConduitFlow GetConduitFlow()
	{
		return Game.Instance.solidConduitFlow;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		utilityCell = GetOutputCell();
		ScenePartitionerLayer layer = GameScenePartitioner.Instance.objectLayers[20];
		partitionerEntry = GameScenePartitioner.Instance.Add("SolidConduitConsumer.OnSpawn", base.gameObject, utilityCell, layer, OnConduitConnectionChanged);
		GetConduitFlow().AddConduitUpdater(ConduitUpdate, ConduitFlowPriority.Dispense);
		OnConduitConnectionChanged(null);
	}

	protected override void OnCleanUp()
	{
		GetConduitFlow().RemoveConduitUpdater(ConduitUpdate);
		GameScenePartitioner.Instance.Free(ref partitionerEntry);
		base.OnCleanUp();
	}

	private void OnConduitConnectionChanged(object data)
	{
		dispensing = dispensing && IsConnected;
		Trigger(-2094018600, (object)BoxedBools.Box(IsConnected));
	}

	private void ConduitUpdate(float dt)
	{
		bool flag = false;
		operational.SetFlag(outputConduitFlag, IsConnected);
		if (operational.IsOperational || alwaysDispense)
		{
			SolidConduitFlow conduitFlow = GetConduitFlow();
			if (conduitFlow.HasConduit(utilityCell) && conduitFlow.IsConduitEmpty(utilityCell))
			{
				Pickupable pickupable = FindSuitableItem();
				if ((bool)pickupable)
				{
					if (pickupable.PrimaryElement.Mass > 20f)
					{
						pickupable = pickupable.Take(Mathf.Max(20f, pickupable.PrimaryElement.MassPerUnit));
					}
					conduitFlow.AddPickupable(utilityCell, pickupable);
					flag = true;
				}
			}
		}
		storage.storageNetworkID = GetConnectedNetworkID();
		dispensing = flag;
	}

	private bool isSolid(GameObject o)
	{
		PrimaryElement component = o.GetComponent<PrimaryElement>();
		if (component == null)
		{
			return false;
		}
		if (component.Element.IsSolid)
		{
			return true;
		}
		if ((double)component.MassPerUnit != 1.0)
		{
			return true;
		}
		return false;
	}

	private Pickupable FindSuitableItem()
	{
		List<GameObject> items = storage.items;
		if (items.Count < 1)
		{
			return null;
		}
		round_robin_index %= items.Count;
		GameObject gameObject = items[round_robin_index];
		round_robin_index++;
		if (solidOnly && !isSolid(gameObject))
		{
			bool flag = false;
			int num = 0;
			while (!flag && num < items.Count)
			{
				gameObject = items[(round_robin_index + num) % items.Count];
				if (isSolid(gameObject))
				{
					flag = true;
				}
				num++;
			}
			if (!flag)
			{
				return null;
			}
		}
		if (!gameObject)
		{
			return null;
		}
		return gameObject.GetComponent<Pickupable>();
	}

	private int GetConnectedNetworkID()
	{
		GameObject gameObject = Grid.Objects[utilityCell, 20];
		SolidConduit solidConduit = ((gameObject != null) ? gameObject.GetComponent<SolidConduit>() : null);
		return ((solidConduit != null) ? solidConduit.GetNetwork() : null)?.id ?? (-1);
	}

	private int GetOutputCell()
	{
		Building component = GetComponent<Building>();
		if (useSecondaryOutput)
		{
			ISecondaryOutput[] components = GetComponents<ISecondaryOutput>();
			foreach (ISecondaryOutput secondaryOutput in components)
			{
				if (secondaryOutput.HasSecondaryConduitType(ConduitType.Solid))
				{
					return Grid.OffsetCell(component.NaturalBuildingCell(), secondaryOutput.GetSecondaryConduitOffset(ConduitType.Solid));
				}
			}
			return Grid.OffsetCell(component.NaturalBuildingCell(), CellOffset.none);
		}
		return component.GetUtilityOutputCell();
	}
}
