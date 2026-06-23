using System;
using System.Collections.Generic;
using System.Linq;
using KSerialization;
using UnityEngine;

namespace UtilLibs.BuildingPortUtils;

[SerializationConfig(/*Could not decode attribute arguments.*/)]
public abstract class PortConduitDispenserBase : KMonoBehaviour, ISaveLoadable
{
	protected Guid hasPipeGuid;

	protected Guid pipeBlockedGuid;

	[SerializeField]
	public CellOffset conduitOffset;

	[SerializeField]
	public CellOffset conduitOffsetFlipped;

	[SerializeField]
	public ConduitType conduitType;

	[SerializeField]
	public SimHashes[] elementFilter = null;

	[SerializeField]
	public Tag[] tagFilter = null;

	[SerializeField]
	public bool invertElementFilter = false;

	[SerializeField]
	public bool alwaysDispense;

	[SerializeField]
	public bool showFullPipeNotification = true;

	[SerializeField]
	public bool SkipSetOperational = false;

	[SerializeField]
	public Func<int, float> GetSolidConduitCapacityTarget = null;

	[SerializeField]
	public float SolidConduitCapacityTarget = 20f;

	private Flag outputConduitFlag;

	private Flag conduitBlockedFlag;

	private NetworkItem networkItem;

	[MyCmpReq]
	private readonly Operational operational;

	[MyCmpReq]
	[SerializeField]
	public Storage storage;

	private Handle<int> partitionerEntry;

	private int utilityCell = -1;

	private int elementOutputOffset;

	[MyCmpReq]
	private KSelectable selectable;

	[SerializeField]
	public PortDisplayOutput AssignedPort = null;

	public bool IsConnected_Cache;

	private bool wasFull = false;

	private bool wasConnected = true;

	public ConduitType TypeOfConduit => conduitType;

	public int UtilityCell => utilityCell;

	public bool IsConnected
	{
		get
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = ((ObjectLayerIndexer)(ref Grid.Objects))[utilityCell, SharedConduitUtils.GetConduitLayer(conduitType)];
			BuildingComplete val2 = default(BuildingComplete);
			return (Object)(object)val != (Object)null && val.TryGetComponent<BuildingComplete>(ref val2);
		}
	}

	public void AssignPort(PortDisplayOutput port)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		AssignedPort = port;
		conduitType = port.type;
		conduitOffset = port.offset;
		conduitOffsetFlipped = port.offsetFlipped;
	}

	private List<Tag> GetFilterTags()
	{
		if (!invertElementFilter)
		{
			List<Tag> list = new List<Tag>();
			if (elementFilter != null)
			{
				list.AddRange(elementFilter.Select((SimHashes s) => GameTagExtensions.CreateTag(s)));
			}
			if (tagFilter != null)
			{
				list.AddRange(tagFilter);
			}
			return list;
		}
		return new List<Tag>();
	}

	public void SetConduitData(ConduitType type)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		conduitType = type;
	}

	private void OnConduitConnectionChanged(object data)
	{
		IsConnected_Cache = IsConnected;
		((KMonoBehaviour)this).BoxingTrigger(-2094018600, IsConnected_Cache);
		UpdateNotifications(isFull: false);
		if (GetSolidConduitCapacityTarget != null)
		{
			SolidConduitCapacityTarget = GetSolidConduitCapacityTarget(utilityCell);
		}
	}

	public override void OnSpawn()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		((KMonoBehaviour)this).OnSpawn();
		Building component = ((Component)this).GetComponent<Building>();
		utilityCell = component.GetCellWithOffset(((int)component.Orientation == 0) ? conduitOffset : conduitOffsetFlipped);
		outputConduitFlag = new Flag($"output_conduit_{utilityCell}_{conduitType}", (Type)1);
		conduitBlockedFlag = new Flag($"conduit_blocked_{utilityCell}_{conduitType}", (Type)0);
		networkItem = new NetworkItem(conduitType, (Endpoint)0, utilityCell, ((Component)this).gameObject);
		SharedConduitUtils.GetConduitMng(conduitType).AddToNetworks(utilityCell, (object)networkItem, true);
		int conduitLayer = SharedConduitUtils.GetConduitLayer(conduitType);
		ScenePartitionerLayer val = GameScenePartitioner.Instance.objectLayers[conduitLayer];
		partitionerEntry = GameScenePartitioner.Instance.Add("ConduitConsumer.OnSpawn", (object)((Component)this).gameObject, utilityCell, val, (Action<object>)OnConduitConnectionChanged);
		SharedConduitUtils.GetConduitFlow(conduitType).AddConduitUpdater((Action<float>)ConduitUpdate, (ConduitFlowPriority)100);
		OnConduitConnectionChanged(null);
		UpdateNotifications(isFull: false);
	}

	public override void OnCleanUp()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		SharedConduitUtils.GetConduitMng(conduitType).RemoveFromNetworks(utilityCell, (object)networkItem, true);
		SharedConduitUtils.GetConduitFlow(conduitType).RemoveConduitUpdater((Action<float>)ConduitUpdate);
		GameScenePartitioner.Instance.Free(ref partitionerEntry);
		((KMonoBehaviour)this).OnCleanUp();
	}

	protected virtual void ConduitUpdate(float dt)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if ((operational.IsOperational || alwaysDispense) && IsConnected_Cache)
		{
			PrimaryElement val = FindSuitableElement();
			if ((Object)(object)val != (Object)null)
			{
				IConduitFlow conduitFlow = SharedConduitUtils.GetConduitFlow(conduitType);
				ConduitFlow val2 = (ConduitFlow)(object)((conduitFlow is ConduitFlow) ? conduitFlow : null);
				if (val2 != null)
				{
					val.KeepZeroMassObject = true;
					float num = val2.AddElement(utilityCell, val.ElementID, val.Mass, val.Temperature, val.DiseaseIdx, val.DiseaseCount);
					if (num > 0f)
					{
						float num2 = num / val.Mass;
						int num3 = (int)(num2 * (float)val.DiseaseCount);
						val.ModifyDiseaseCount(-num3, "CustomConduitDispenser.ConduitUpdate");
						val.Mass -= num;
						((KMonoBehaviour)this).Trigger(-1697596308, (object)((Component)val).gameObject);
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					SolidConduitFlow val3 = (SolidConduitFlow)(object)((conduitFlow is SolidConduitFlow) ? conduitFlow : null);
					if (val3 != null)
					{
						flag = !val3.IsConduitEmpty(utilityCell);
						Pickupable val4 = default(Pickupable);
						if (!flag && ((Component)val).TryGetComponent<Pickupable>(ref val4))
						{
							if (val.Mass > SolidConduitCapacityTarget)
							{
								val4 = val4.Take(Mathf.Max(SolidConduitCapacityTarget, val.MassPerUnit));
							}
							val3.AddPickupable(utilityCell, val4);
						}
					}
				}
			}
		}
		UpdateNotifications(flag);
	}

	public virtual void UpdateNotifications(bool isFull)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!SkipSetOperational && wasConnected != IsConnected_Cache)
		{
			wasConnected = IsConnected_Cache;
			operational.SetFlag(outputConduitFlag, wasConnected);
			StatusItem outputStatusItem = ConduitDisplayPortPatching.GetOutputStatusItem(conduitType);
			if (outputStatusItem != null)
			{
				hasPipeGuid = selectable.ToggleStatusItem(outputStatusItem, hasPipeGuid, !wasConnected, (object)new Tuple<ConduitType, List<Tag>>(conduitType, GetFilterTags()));
			}
		}
		if (wasFull != isFull)
		{
			wasFull = isFull;
			if (showFullPipeNotification && !SkipSetOperational)
			{
				pipeBlockedGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.ConduitBlocked, pipeBlockedGuid, isFull, (object)null);
			}
		}
		operational.SetFlag(conduitBlockedFlag, !wasFull || SkipSetOperational);
	}

	protected virtual PrimaryElement FindSuitableElement()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Invalid comparison between Unknown and I4
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Invalid comparison between Unknown and I4
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Invalid comparison between Unknown and I4
		List<GameObject> items = storage.items;
		int count = items.Count;
		PrimaryElement val2 = default(PrimaryElement);
		for (int i = 0; i < count; i++)
		{
			int index = (i + elementOutputOffset) % count;
			GameObject val = items[index];
			if (!((Object)(object)val == (Object)null) && val.TryGetComponent<PrimaryElement>(ref val2) && !(val2.Mass <= 0f) && val2.Element != null)
			{
				bool flag = false;
				if ((int)conduitType == 2)
				{
					flag = val2.Element.IsLiquid;
				}
				else if ((int)conduitType == 1)
				{
					flag = val2.Element.IsGas;
				}
				else if ((int)conduitType == 3)
				{
					flag = val2.Element.IsSolid;
				}
				if (flag && ElementAllowedByFilter(val2))
				{
					elementOutputOffset = (elementOutputOffset + 1) % count;
					return val2;
				}
			}
		}
		return null;
	}

	private bool ElementAllowedByFilter(PrimaryElement primaryelement)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Invalid comparison between I4 and Unknown
		SimHashes elementID = primaryelement.ElementID;
		if ((elementFilter == null || !elementFilter.Any()) && (tagFilter == null || !tagFilter.Any()))
		{
			return true;
		}
		if (tagFilter != null && tagFilter.Any())
		{
			return KPrefabIDExtensions.HasAnyTags((Component)(object)primaryelement, tagFilter) != invertElementFilter;
		}
		bool flag = false;
		for (int i = 0; i != elementFilter.Length; i++)
		{
			if ((int)elementFilter[i] == (int)elementID)
			{
				flag = true;
			}
		}
		return flag != invertElementFilter;
	}
}
