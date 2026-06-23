using System;
using UnityEngine;

[SkipSaveFileSerialization]
[AddComponentMenu("KMonoBehaviour/scripts/RequireInputs")]
public class RequireInputs : KMonoBehaviour, ISim200ms
{
	[Flags]
	public enum Requirements
	{
		None = 0,
		NoWire = 1,
		NeedPower = 2,
		ConduitConnected = 4,
		ConduitEmpty = 8,
		AllPower = 3,
		AllConduit = 0xC,
		All = 0xF
	}

	[SerializeField]
	private bool requirePower = true;

	[SerializeField]
	private bool requireConduit = false;

	public bool requireConduitHasMass = true;

	public Requirements visualizeRequirements = Requirements.All;

	private static readonly Operational.Flag inputConnectedFlag = new Operational.Flag("inputConnected", Operational.Flag.Type.Requirement);

	private static readonly Operational.Flag pipesHaveMass = new Operational.Flag("pipesHaveMass", Operational.Flag.Type.Requirement);

	private Guid noWireStatusGuid = Guid.Empty;

	private Guid needPowerStatusGuid = Guid.Empty;

	private Guid liquidConduitEmptyStatusGuid = Guid.Empty;

	private Guid gasConduitEmptyStatusGuid = Guid.Empty;

	private Guid noLiquidConduitStatusGuid = Guid.Empty;

	private Guid noGasConduitStatusGuid = Guid.Empty;

	private bool requirementsMet = false;

	private BuildingEnabledButton button;

	private IEnergyConsumer energy;

	public ConduitConsumer conduitConsumer;

	[MyCmpReq]
	private KSelectable selectable;

	[MyCmpGet]
	private Operational operational;

	private bool previouslyConnectedOpFlag = true;

	private bool previouslySatisfiedOpFlag = true;

	public bool RequiresPower
	{
		get
		{
			return requirePower;
		}
		set
		{
			requirePower = value;
		}
	}

	public bool RequiresInputConduit
	{
		get
		{
			return requireConduit;
		}
		set
		{
			requireConduit = value;
		}
	}

	public bool RequirementsMet => requirementsMet;

	public void SetRequirements(bool power, bool conduit)
	{
		requirePower = power;
		requireConduit = conduit;
	}

	protected override void OnPrefabInit()
	{
		Bind();
	}

	protected override void OnSpawn()
	{
		CheckRequirements();
		Bind();
	}

	[ContextMenu("Bind")]
	private void Bind()
	{
		if (requirePower)
		{
			energy = GetComponent<IEnergyConsumer>();
			button = GetComponent<BuildingEnabledButton>();
		}
		if (requireConduit && !conduitConsumer)
		{
			conduitConsumer = GetComponent<ConduitConsumer>();
		}
		Operational component = GetComponent<Operational>();
		bool flag = component != null;
		previouslyConnectedOpFlag = flag && component.GetFlag(inputConnectedFlag);
		previouslySatisfiedOpFlag = flag && component.GetFlag(pipesHaveMass);
	}

	public void Sim200ms(float dt)
	{
		CheckRequirements();
	}

	private void CheckRequirements()
	{
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool show = false;
		bool show2 = false;
		if (requirePower)
		{
			flag3 = energy.IsConnected;
			flag4 = energy.IsPowered;
			flag = flag && flag4 && flag3;
			show = VisualizeRequirement(Requirements.NeedPower) && flag3 && !flag4 && (button == null || button.IsEnabled);
			show2 = VisualizeRequirement(Requirements.NoWire) && !flag3;
		}
		flag2 |= flag != requirementsMet && GetComponent<Light2D>() != null;
		needPowerStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NeedPower, needPowerStatusGuid, show, this);
		noWireStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NoWireConnected, noWireStatusGuid, show2, this);
		bool flag5 = conduitConsumer != null && conduitConsumer.conduitType == ConduitType.Liquid;
		bool flag6 = flag5 && conduitConsumer.IsConnected;
		bool flag7 = flag5 && conduitConsumer.IsSatisfied;
		bool flag8 = flag5 && conduitConsumer.enabled && requireConduitHasMass && requireConduit;
		bool flag9 = flag5 && conduitConsumer.enabled && requireConduit;
		flag = flag && (!flag8 || flag7);
		flag = flag && (!flag9 || flag6);
		liquidConduitEmptyStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.LiquidPipeEmpty, liquidConduitEmptyStatusGuid, VisualizeRequirement(Requirements.ConduitEmpty) && flag8 && !flag7, this);
		noLiquidConduitStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NeedLiquidIn, noLiquidConduitStatusGuid, VisualizeRequirement(Requirements.ConduitConnected) && flag9 && !flag6, conduitConsumer);
		if (flag5)
		{
			bool flag10 = !flag9 || flag6;
			bool flag11 = !flag8 || flag7;
			if (flag10 != previouslyConnectedOpFlag)
			{
				operational.SetFlag(inputConnectedFlag, flag10);
				previouslyConnectedOpFlag = flag10;
			}
			if (flag11 != previouslySatisfiedOpFlag)
			{
				operational.SetFlag(pipesHaveMass, flag11);
				previouslySatisfiedOpFlag = flag11;
			}
		}
		bool flag12 = conduitConsumer != null && conduitConsumer.conduitType == ConduitType.Gas;
		bool flag13 = flag12 && conduitConsumer.IsConnected;
		bool flag14 = flag12 && conduitConsumer.IsSatisfied;
		bool flag15 = flag12 && conduitConsumer.enabled && requireConduitHasMass && requireConduit;
		bool flag16 = flag12 && conduitConsumer.enabled && requireConduit;
		flag = flag && (!flag15 || flag14);
		flag = flag && (!flag16 || flag13);
		gasConduitEmptyStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.GasPipeEmpty, gasConduitEmptyStatusGuid, VisualizeRequirement(Requirements.ConduitEmpty) && flag15 && !flag14, this);
		noGasConduitStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NeedGasIn, noGasConduitStatusGuid, VisualizeRequirement(Requirements.ConduitConnected) && flag16 && !flag13, conduitConsumer);
		if (flag12)
		{
			bool flag17 = !flag16 || flag13;
			bool flag18 = !flag15 || flag14;
			if (flag17 != previouslyConnectedOpFlag)
			{
				operational.SetFlag(inputConnectedFlag, flag17);
				previouslyConnectedOpFlag = flag17;
			}
			if (flag18 != previouslySatisfiedOpFlag)
			{
				operational.SetFlag(pipesHaveMass, flag18);
				previouslySatisfiedOpFlag = flag18;
			}
		}
		requirementsMet = flag;
		if (flag2)
		{
			Room roomOfGameObject = Game.Instance.roomProber.GetRoomOfGameObject(base.gameObject);
			if (roomOfGameObject != null)
			{
				Game.Instance.roomProber.UpdateRoom(roomOfGameObject.cavity);
			}
		}
	}

	public bool VisualizeRequirement(Requirements r)
	{
		return (visualizeRequirements & r) == r;
	}
}
