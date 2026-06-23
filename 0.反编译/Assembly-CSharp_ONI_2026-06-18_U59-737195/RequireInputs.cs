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
	private bool requireConduit;

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

	private bool requirementsMet;

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
		bool show = false;
		bool show2 = false;
		if (requirePower)
		{
			flag2 = energy.IsConnected;
			flag3 = energy.IsPowered;
			flag = flag && flag3 && flag2;
			show = VisualizeRequirement(Requirements.NeedPower) && flag2 && !flag3 && (button == null || button.IsEnabled);
			show2 = VisualizeRequirement(Requirements.NoWire) && !flag2;
		}
		int num = 0 | ((flag != requirementsMet && GetComponent<Light2D>() != null) ? 1 : 0);
		needPowerStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NeedPower, needPowerStatusGuid, show, this);
		noWireStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NoWireConnected, noWireStatusGuid, show2, this);
		int num2;
		int num3;
		if (conduitConsumer != null)
		{
			num2 = ((conduitConsumer.conduitType == ConduitType.Liquid) ? 1 : 0);
			if (num2 != 0)
			{
				num3 = (conduitConsumer.IsConnected ? 1 : 0);
				goto IL_010c;
			}
		}
		else
		{
			num2 = 0;
		}
		num3 = 0;
		goto IL_010c;
		IL_010c:
		bool flag4 = (byte)num3 != 0;
		bool flag5 = num2 != 0 && conduitConsumer.IsSatisfied;
		bool flag6 = num2 != 0 && conduitConsumer.enabled && requireConduitHasMass && requireConduit && VisualizeRequirement(Requirements.ConduitEmpty);
		bool flag7 = num2 != 0 && conduitConsumer.enabled && requireConduit && VisualizeRequirement(Requirements.ConduitConnected);
		flag = flag && (!flag6 || flag5);
		flag = flag && (!flag7 || flag4);
		liquidConduitEmptyStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.LiquidPipeEmpty, liquidConduitEmptyStatusGuid, VisualizeRequirement(Requirements.ConduitEmpty) && flag6 && !flag5, this);
		noLiquidConduitStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NeedLiquidIn, noLiquidConduitStatusGuid, VisualizeRequirement(Requirements.ConduitConnected) && flag7 && !flag4, conduitConsumer);
		if (num2 != 0)
		{
			bool flag8 = !flag7 || flag4;
			bool flag9 = !flag6 || flag5;
			if (flag8 != previouslyConnectedOpFlag)
			{
				operational.SetFlag(inputConnectedFlag, flag8);
				previouslyConnectedOpFlag = flag8;
			}
			if (flag9 != previouslySatisfiedOpFlag)
			{
				operational.SetFlag(pipesHaveMass, flag9);
				previouslySatisfiedOpFlag = flag9;
			}
		}
		int num4;
		int num5;
		if (conduitConsumer != null)
		{
			num4 = ((conduitConsumer.conduitType == ConduitType.Gas) ? 1 : 0);
			if (num4 != 0)
			{
				num5 = (conduitConsumer.IsConnected ? 1 : 0);
				goto IL_0290;
			}
		}
		else
		{
			num4 = 0;
		}
		num5 = 0;
		goto IL_0290;
		IL_0290:
		bool flag10 = (byte)num5 != 0;
		bool flag11 = num4 != 0 && conduitConsumer.IsSatisfied;
		bool flag12 = num4 != 0 && conduitConsumer.enabled && requireConduitHasMass && requireConduit && VisualizeRequirement(Requirements.ConduitEmpty);
		bool flag13 = num4 != 0 && conduitConsumer.enabled && requireConduit && VisualizeRequirement(Requirements.ConduitConnected);
		flag = flag && (!flag12 || flag11);
		flag = flag && (!flag13 || flag10);
		gasConduitEmptyStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.GasPipeEmpty, gasConduitEmptyStatusGuid, VisualizeRequirement(Requirements.ConduitEmpty) && flag12 && !flag11, this);
		noGasConduitStatusGuid = selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NeedGasIn, noGasConduitStatusGuid, VisualizeRequirement(Requirements.ConduitConnected) && flag13 && !flag10, conduitConsumer);
		if (num4 != 0)
		{
			bool flag14 = !flag13 || flag10;
			bool flag15 = !flag12 || flag11;
			if (flag14 != previouslyConnectedOpFlag)
			{
				operational.SetFlag(inputConnectedFlag, flag14);
				previouslyConnectedOpFlag = flag14;
			}
			if (flag15 != previouslySatisfiedOpFlag)
			{
				operational.SetFlag(pipesHaveMass, flag15);
				previouslySatisfiedOpFlag = flag15;
			}
		}
		requirementsMet = flag;
		if (num != 0)
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
