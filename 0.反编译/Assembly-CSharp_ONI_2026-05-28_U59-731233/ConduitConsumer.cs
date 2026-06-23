using System;
using STRINGS;
using UnityEngine;

[SkipSaveFileSerialization]
[AddComponentMenu("KMonoBehaviour/scripts/ConduitConsumer")]
public class ConduitConsumer : KMonoBehaviour, IConduitConsumer
{
	public enum WrongElementResult
	{
		Destroy,
		Dump,
		Store
	}

	[SerializeField]
	public ConduitType conduitType;

	[SerializeField]
	public bool ignoreMinMassCheck = false;

	[SerializeField]
	public Tag capacityTag = GameTags.Any;

	[SerializeField]
	public float capacityKG = float.PositiveInfinity;

	[SerializeField]
	public bool forceAlwaysSatisfied = false;

	[SerializeField]
	public bool alwaysConsume = false;

	[SerializeField]
	public bool keepZeroMassObject = true;

	[SerializeField]
	public bool useSecondaryInput = false;

	[SerializeField]
	public bool isOn = true;

	[NonSerialized]
	public bool isConsuming = true;

	[NonSerialized]
	public bool consumedLastTick = true;

	[MyCmpReq]
	public Operational operational;

	[MyCmpReq]
	protected Building building;

	public Operational.State OperatingRequirement = Operational.State.Operational;

	public ISecondaryInput targetSecondaryInput;

	[MyCmpGet]
	public Storage storage;

	[MyCmpGet]
	private BuildingComplete m_buildingComplete;

	private int utilityCell = -1;

	public float consumptionRate = float.PositiveInfinity;

	public SimHashes lastConsumedElement = SimHashes.Vacuum;

	private HandleVector<int>.Handle partitionerEntry;

	private bool satisfied = false;

	public WrongElementResult wrongElementResult = WrongElementResult.Destroy;

	public Storage Storage => storage;

	public ConduitType ConduitType => conduitType;

	public bool IsConnected
	{
		get
		{
			GameObject gameObject = Grid.Objects[utilityCell, (conduitType == ConduitType.Gas) ? 12 : 16];
			return gameObject != null && m_buildingComplete != null;
		}
	}

	public bool CanConsume
	{
		get
		{
			bool result = false;
			if (IsConnected)
			{
				ConduitFlow conduitManager = GetConduitManager();
				result = conduitManager.GetContents(utilityCell).mass > 0f;
			}
			return result;
		}
	}

	public float stored_mass => (storage == null) ? 0f : ((capacityTag != GameTags.Any) ? storage.GetMassAvailable(capacityTag) : storage.MassStored());

	public float space_remaining_kg
	{
		get
		{
			float num = capacityKG - stored_mass;
			return (storage == null) ? num : Mathf.Min(storage.RemainingCapacity(), num);
		}
	}

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
			ConduitFlow conduitManager = GetConduitManager();
			int inputCell = GetInputCell(conduitManager.conduitType);
			return conduitManager.GetContents(inputCell).mass;
		}
	}

	public void SetConduitData(ConduitType type)
	{
		conduitType = type;
	}

	private ConduitFlow GetConduitManager()
	{
		return conduitType switch
		{
			ConduitType.Gas => Game.Instance.gasConduitFlow, 
			ConduitType.Liquid => Game.Instance.liquidConduitFlow, 
			_ => null, 
		};
	}

	protected virtual int GetInputCell(ConduitType inputConduitType)
	{
		if (useSecondaryInput)
		{
			ISecondaryInput[] components = GetComponents<ISecondaryInput>();
			ISecondaryInput[] array = components;
			foreach (ISecondaryInput secondaryInput in array)
			{
				if (secondaryInput.HasSecondaryConduitType(inputConduitType))
				{
					return Grid.OffsetCell(building.NaturalBuildingCell(), secondaryInput.GetSecondaryConduitOffset(inputConduitType));
				}
			}
			Debug.LogWarning("No secondaryInput of type was found");
			return Grid.OffsetCell(building.NaturalBuildingCell(), components[0].GetSecondaryConduitOffset(inputConduitType));
		}
		return building.GetUtilityInputCell();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		GameScheduler.Instance.Schedule("PlumbingTutorial", 2f, delegate
		{
			Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_Plumbing);
		});
		ConduitFlow conduitManager = GetConduitManager();
		utilityCell = GetInputCell(conduitManager.conduitType);
		ScenePartitionerLayer layer = GameScenePartitioner.Instance.objectLayers[(conduitType == ConduitType.Gas) ? 12 : 16];
		partitionerEntry = GameScenePartitioner.Instance.Add("ConduitConsumer.OnSpawn", base.gameObject, utilityCell, layer, OnConduitConnectionChanged);
		GetConduitManager().AddConduitUpdater(ConduitUpdate);
		OnConduitConnectionChanged(null);
	}

	protected override void OnCleanUp()
	{
		GetConduitManager().RemoveConduitUpdater(ConduitUpdate);
		GameScenePartitioner.Instance.Free(ref partitionerEntry);
		base.OnCleanUp();
	}

	private void OnConduitConnectionChanged(object data)
	{
		Trigger(-2094018600, (object)BoxedBools.Box(IsConnected));
	}

	public void SetOnState(bool onState)
	{
		isOn = onState;
	}

	private void ConduitUpdate(float dt)
	{
		if (isConsuming && isOn)
		{
			ConduitFlow conduitManager = GetConduitManager();
			Consume(dt, conduitManager);
		}
	}

	private void Consume(float dt, ConduitFlow conduit_mgr)
	{
		IsSatisfied = false;
		consumedLastTick = false;
		if (building.Def.CanMove)
		{
			utilityCell = GetInputCell(conduit_mgr.conduitType);
		}
		if (!IsConnected)
		{
			return;
		}
		ConduitFlow.ConduitContents contents = conduit_mgr.GetContents(utilityCell);
		if (contents.mass <= 0f)
		{
			return;
		}
		IsSatisfied = true;
		if (!alwaysConsume && !operational.MeetsRequirements(OperatingRequirement))
		{
			return;
		}
		float a = ConsumptionRate * dt;
		a = Mathf.Min(a, space_remaining_kg);
		Element element = ElementLoader.FindElementByHash(contents.element);
		if (contents.element != lastConsumedElement)
		{
			DiscoveredResources.Instance.Discover(element.tag, element.materialCategory);
		}
		float num = 0f;
		if (a > 0f)
		{
			ConduitFlow.ConduitContents conduitContents = conduit_mgr.RemoveElement(utilityCell, a);
			num = conduitContents.mass;
			lastConsumedElement = conduitContents.element;
		}
		bool flag = element.HasTag(capacityTag);
		if (num > 0f && capacityTag != GameTags.Any && !flag)
		{
			BoxingTrigger(-794517298, new BuildingHP.DamageSourceInfo
			{
				damage = 1,
				source = BUILDINGS.DAMAGESOURCES.BAD_INPUT_ELEMENT,
				popString = UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.WRONG_ELEMENT
			});
		}
		if (flag || wrongElementResult == WrongElementResult.Store || contents.element == SimHashes.Vacuum || capacityTag == GameTags.Any)
		{
			if (!(num > 0f))
			{
				return;
			}
			consumedLastTick = true;
			int disease_count = (int)((float)contents.diseaseCount * (num / contents.mass));
			Element element2 = ElementLoader.FindElementByHash(contents.element);
			switch (conduitType)
			{
			case ConduitType.Liquid:
				if (element2.IsLiquid)
				{
					storage.AddLiquid(contents.element, num, contents.temperature, contents.diseaseIdx, disease_count, keepZeroMassObject, do_disease_transfer: false);
				}
				else
				{
					Debug.LogWarning("Liquid conduit consumer consuming non liquid: " + element2.id);
				}
				break;
			case ConduitType.Gas:
				if (element2.IsGas)
				{
					storage.AddGasChunk(contents.element, num, contents.temperature, contents.diseaseIdx, disease_count, keepZeroMassObject, do_disease_transfer: false);
				}
				else
				{
					Debug.LogWarning("Gas conduit consumer consuming non gas: " + element2.id);
				}
				break;
			}
		}
		else if (num > 0f)
		{
			consumedLastTick = true;
			if (wrongElementResult == WrongElementResult.Dump)
			{
				int disease_count2 = (int)((float)contents.diseaseCount * (num / contents.mass));
				int gameCell = Grid.PosToCell(base.transform.GetPosition());
				SimMessages.AddRemoveSubstance(gameCell, contents.element, CellEventLogger.Instance.ConduitConsumerWrongElement, num, contents.temperature, contents.diseaseIdx, disease_count2);
			}
		}
	}
}
