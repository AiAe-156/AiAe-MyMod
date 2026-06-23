using System;
using System.Runtime.CompilerServices;
using STRINGS;
using UnityEngine;

namespace UtilLibs.BuildingPortUtils;

[SkipSaveFileSerialization]
public class PortConduitConsumer : KMonoBehaviour
{
	public enum WrongElementResult
	{
		Destroy,
		Dump,
		Store
	}

	protected Guid hasPipeGuid;

	protected Guid pipeBlockedGuid;

	[SerializeField]
	public bool showEmptyPipeNotification = false;

	[SerializeField]
	public CellOffset conduitOffset;

	[SerializeField]
	public CellOffset conduitOffsetFlipped;

	[SerializeField]
	public ConduitType conduitType;

	[SerializeField]
	public bool ignoreMinMassCheck;

	[SerializeField]
	public Tag capacityTag = GameTags.Any;

	[SerializeField]
	public float capacityKG = float.PositiveInfinity;

	[SerializeField]
	public bool forceAlwaysSatisfied = false;

	[SerializeField]
	public bool SkipSetOperational = false;

	[SerializeField]
	public bool alwaysConsume = false;

	[SerializeField]
	public bool keepZeroMassObject = true;

	[NonSerialized]
	public bool isConsuming = true;

	private NetworkItem networkItem;

	[MyCmpReq]
	private readonly Operational operational;

	[MyCmpReq]
	private readonly Building building;

	[MyCmpGet]
	public Storage storage;

	[MyCmpReq]
	private KSelectable selectable;

	private int utilityCell = -1;

	public float consumptionRate = float.PositiveInfinity;

	private Flag inputConduitFlag;

	private Handle<int> partitionerEntry;

	private bool satisfied;

	public WrongElementResult wrongElementResult;

	public SimHashes lastConsumedElement = (SimHashes)758759285;

	private bool IsConnected_Cache;

	private bool wasSatisfied = true;

	private bool wasConnected = true;

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

	public bool CanConsume => IsConnected_Cache && MassAvailable > 0f;

	public ConduitType TypeOfConduit => conduitType;

	public bool IsAlmostEmpty => !ignoreMinMassCheck && MassAvailable < ConsumptionRate * 30f;

	public bool IsEmpty => !ignoreMinMassCheck && (MassAvailable == 0f || MassAvailable < ConsumptionRate);

	public float ConsumptionRate => consumptionRate;

	public bool IsSatisfied
	{
		get
		{
			return satisfied || !isConsuming;
		}
		set
		{
			satisfied = value || forceAlwaysSatisfied;
		}
	}

	public float MassAvailable
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			int inputCell = GetInputCell();
			IConduitFlow conduitFlow = SharedConduitUtils.GetConduitFlow(conduitType);
			ConduitFlow val = (ConduitFlow)(object)((conduitFlow is ConduitFlow) ? conduitFlow : null);
			if (val != null)
			{
				ConduitContents contents = val.GetContents(inputCell);
				return ((ConduitContents)(ref contents)).mass;
			}
			SolidConduitFlow val2 = (SolidConduitFlow)(object)((conduitFlow is SolidConduitFlow) ? conduitFlow : null);
			if (val2 != null)
			{
				Pickupable pickupable = val2.GetPickupable(val2.GetContents(inputCell).pickupableHandle);
				if ((Object)(object)pickupable != (Object)null)
				{
					return pickupable.TotalAmount;
				}
			}
			return 0f;
		}
	}

	public void AssignPort(PortDisplayInput port)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		conduitType = port.type;
		conduitOffset = port.offset;
		conduitOffsetFlipped = port.offsetFlipped;
	}

	public void SetConduitData(ConduitType type)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		conduitType = type;
	}

	private int GetInputCell()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Building component = ((Component)this).GetComponent<Building>();
		return component.GetCellWithOffset(((int)component.Orientation == 0) ? conduitOffset : conduitOffsetFlipped);
	}

	public override void OnSpawn()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		((KMonoBehaviour)this).OnSpawn();
		utilityCell = GetInputCell();
		inputConduitFlag = new Flag($"input_conduit_connected_{utilityCell}_{conduitType}", (Type)1);
		networkItem = new NetworkItem(conduitType, (Endpoint)1, utilityCell, ((Component)this).gameObject);
		SharedConduitUtils.GetConduitMng(conduitType).AddToNetworks(utilityCell, (object)networkItem, true);
		ScenePartitionerLayer val = GameScenePartitioner.Instance.objectLayers[SharedConduitUtils.GetConduitLayer(conduitType)];
		partitionerEntry = GameScenePartitioner.Instance.Add("ConduitConsumer.OnSpawn", (object)((Component)this).gameObject, utilityCell, val, (Action<object>)OnConduitConnectionChanged);
		SharedConduitUtils.GetConduitFlow(conduitType).AddConduitUpdater((Action<float>)ConduitUpdate, (ConduitFlowPriority)0);
		OnConduitConnectionChanged(null);
		UpdateNotifications();
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

	private void OnConduitConnectionChanged(object _)
	{
		IsConnected_Cache = IsConnected;
		((KMonoBehaviour)this).Trigger(-2094018600, (object)BoxedBools.Box(IsConnected_Cache));
		UpdateNotifications();
	}

	public virtual void UpdateNotifications()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!SkipSetOperational && IsConnected_Cache != wasConnected)
		{
			wasConnected = IsConnected_Cache;
			StatusItem inputStatusItem = ConduitDisplayPortPatching.GetInputStatusItem(conduitType);
			hasPipeGuid = selectable.ToggleStatusItem(inputStatusItem, hasPipeGuid, !wasConnected, (object)new Tuple<ConduitType, Tag>(conduitType, capacityTag));
			operational.SetFlag(inputConduitFlag, wasConnected);
		}
		if (showEmptyPipeNotification)
		{
			bool flag = IsConnected_Cache && IsSatisfied;
			if (wasSatisfied != flag)
			{
				wasSatisfied = flag;
				pipeBlockedGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.ConduitBlockedMultiples, pipeBlockedGuid, !wasSatisfied, (object)null);
			}
		}
	}

	private void ConduitUpdate(float dt)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (isConsuming)
		{
			Consume(dt, SharedConduitUtils.GetConduitFlow(conduitType));
		}
		UpdateNotifications();
	}

	private void Consume(float dt, IConduitFlow iConMng)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Invalid comparison between Unknown and I4
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Invalid comparison between Unknown and I4
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Invalid comparison between Unknown and I4
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Invalid comparison between Unknown and I4
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Invalid comparison between Unknown and I4
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		IsSatisfied = false;
		if (building.Def.CanMove)
		{
			utilityCell = GetInputCell();
		}
		if (!IsConnected_Cache)
		{
			return;
		}
		ConduitFlow val = (ConduitFlow)(object)((iConMng is ConduitFlow) ? iConMng : null);
		if (val != null)
		{
			ConduitContents contents = val.GetContents(utilityCell);
			if (!(((ConduitContents)(ref contents)).mass > 0f))
			{
				return;
			}
			IsSatisfied = true;
			if (!alwaysConsume && !operational.IsOperational)
			{
				return;
			}
			float num = ((!(capacityTag != GameTags.Any)) ? storage.MassStored() : storage.GetMassAvailable(capacityTag));
			float num2 = Mathf.Min(storage.RemainingCapacity(), capacityKG - num);
			float num3 = ConsumptionRate * dt;
			num3 = Mathf.Min(num3, num2);
			float num4 = 0f;
			if (num3 > 0f)
			{
				ConduitContents val2 = val.RemoveElement(utilityCell, num3);
				num4 = ((ConduitContents)(ref val2)).mass;
			}
			Element val3 = ElementLoader.FindElementByHash(contents.element);
			bool flag = val3.HasTag(capacityTag);
			if (num4 > 0f && capacityTag != GameTags.Any && !flag)
			{
				IsSatisfied = true;
				((KMonoBehaviour)this).BoxingTrigger<DamageSourceInfo>(-794517298, new DamageSourceInfo
				{
					damage = 1,
					source = LocString.op_Implicit(DAMAGESOURCES.BAD_INPUT_ELEMENT),
					popString = LocString.op_Implicit(DAMAGE_POPS.WRONG_ELEMENT)
				});
			}
			if (!flag && (int)wrongElementResult != 2 && (int)contents.element != 758759285 && !(capacityTag == GameTags.Any))
			{
				return;
			}
			if (num4 > 0f)
			{
				if (val3.id != lastConsumedElement)
				{
					DiscoveredResources.Instance.Discover(val3.tag, val3.materialCategory);
					lastConsumedElement = val3.id;
				}
				int num5 = (int)((float)contents.diseaseCount * (num4 / ((ConduitContents)(ref contents)).mass));
				Element val4 = ElementLoader.FindElementByHash(contents.element);
				ConduitType val5 = conduitType;
				if ((int)val5 == 1)
				{
					if (val4.IsGas)
					{
						storage.AddGasChunk(contents.element, num4, contents.temperature, contents.diseaseIdx, num5, keepZeroMassObject, false);
					}
					else
					{
						Debug.LogWarning((object)("Gas conduit consumer consuming non gas: " + ((object)Unsafe.As<SimHashes, SimHashes>(ref val4.id)/*cast due to .constrained prefix*/).ToString()));
					}
				}
				else if ((int)val5 == 2)
				{
					if (val4.IsLiquid)
					{
						storage.AddLiquid(contents.element, num4, contents.temperature, contents.diseaseIdx, num5, keepZeroMassObject, false);
					}
					else
					{
						Debug.LogWarning((object)("Liquid conduit consumer consuming non liquid: " + ((object)Unsafe.As<SimHashes, SimHashes>(ref val4.id)/*cast due to .constrained prefix*/).ToString()));
					}
				}
			}
			else if (num4 > 0f && (int)wrongElementResult == 1)
			{
				int num6 = (int)((float)contents.diseaseCount * (num4 / ((ConduitContents)(ref contents)).mass));
				SimMessages.AddRemoveSubstance(utilityCell, contents.element, CellEventLogger.Instance.ConduitConsumerWrongElement, num4, contents.temperature, contents.diseaseIdx, num6, true, -1);
			}
			return;
		}
		SolidConduitFlow val6 = (SolidConduitFlow)(object)((iConMng is SolidConduitFlow) ? iConMng : null);
		if (val6 == null)
		{
			return;
		}
		ConduitContents contents2 = val6.GetContents(utilityCell);
		IsSatisfied = false;
		if (!contents2.pickupableHandle.IsValid() || (!alwaysConsume && !operational.IsOperational))
		{
			return;
		}
		float num7 = ((capacityTag != GameTags.Any) ? storage.GetMassAvailable(capacityTag) : storage.MassStored());
		float num8 = Mathf.Min(storage.capacityKg, capacityKG);
		float num9 = Mathf.Max(0f, num8 - num7);
		IsSatisfied = true;
		if (!(num9 > 0f))
		{
			return;
		}
		Pickupable pickupable = val6.GetPickupable(contents2.pickupableHandle);
		if (pickupable.PrimaryElement.Mass <= num9 || pickupable.PrimaryElement.Mass > num8)
		{
			Pickupable val7 = val6.RemovePickupable(utilityCell);
			if ((Object)(object)val7 != (Object)null)
			{
				storage.Store(((Component)val7).gameObject, true, false, true, false);
			}
		}
	}
}
