using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class SpaceHeater : StateMachineComponent<SpaceHeater.StatesInstance>, IGameObjectEffectDescriptor, ISingleSliderControl, ISliderControl
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, SpaceHeater, object>.GameInstance
	{
		public StatesInstance(SpaceHeater master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, SpaceHeater>
	{
		public class OnlineStates : State
		{
			public State heating;

			public State overtemp;

			public State undermassliquid;

			public State undermassgas;
		}

		public State offline;

		public OnlineStates online;

		private StatusItem statusItemUnderMassLiquid;

		private StatusItem statusItemUnderMassGas;

		private StatusItem statusItemOverTemp;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = offline;
			base.serializable = SerializeType.Never;
			statusItemUnderMassLiquid = new StatusItem("statusItemUnderMassLiquid", BUILDING.STATUSITEMS.HEATINGSTALLEDLOWMASS_LIQUID.NAME, BUILDING.STATUSITEMS.HEATINGSTALLEDLOWMASS_LIQUID.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
			statusItemUnderMassGas = new StatusItem("statusItemUnderMassGas", BUILDING.STATUSITEMS.HEATINGSTALLEDLOWMASS_GAS.NAME, BUILDING.STATUSITEMS.HEATINGSTALLEDLOWMASS_GAS.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
			statusItemOverTemp = new StatusItem("statusItemOverTemp", BUILDING.STATUSITEMS.HEATINGSTALLEDHOTENV.NAME, BUILDING.STATUSITEMS.HEATINGSTALLEDHOTENV.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
			statusItemOverTemp.resolveStringCallback = delegate(string str, object obj)
			{
				StatesInstance statesInstance = (StatesInstance)obj;
				return string.Format(str, GameUtil.GetFormattedTemperature(statesInstance.master.TargetTemperature));
			};
			offline.Enter(RefreshHeatEffect).EventTransition(GameHashes.OperationalChanged, online, (StatesInstance smi) => smi.master.operational.IsOperational);
			online.EventTransition(GameHashes.OperationalChanged, offline, (StatesInstance smi) => !smi.master.operational.IsOperational).DefaultState(online.heating).Update("spaceheater_online", delegate(StatesInstance smi, float dt)
			{
				switch (smi.master.MonitorHeating(dt))
				{
				case MonitorState.NotEnoughLiquid:
					smi.GoTo(online.undermassliquid);
					break;
				case MonitorState.NotEnoughGas:
					smi.GoTo(online.undermassgas);
					break;
				case MonitorState.TooHot:
					smi.GoTo(online.overtemp);
					break;
				case MonitorState.ReadyToHeat:
					smi.GoTo(online.heating);
					break;
				}
			}, UpdateRate.SIM_4000ms);
			online.heating.Enter(RefreshHeatEffect).Enter(delegate(StatesInstance smi)
			{
				smi.master.operational.SetActive(value: true);
			}).ToggleStatusItem((StatesInstance smi) => smi.master.heatStatusItem, (StatesInstance smi) => smi)
				.Update(GenerateHeat)
				.Exit(delegate(StatesInstance smi)
				{
					smi.master.operational.SetActive(value: false);
				})
				.Exit(RefreshHeatEffect);
			online.undermassliquid.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Heat, statusItemUnderMassLiquid);
			online.undermassgas.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Heat, statusItemUnderMassGas);
			online.overtemp.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Heat, statusItemOverTemp);
		}
	}

	private enum MonitorState
	{
		ReadyToHeat,
		TooHot,
		NotEnoughLiquid,
		NotEnoughGas
	}

	public float targetTemperature = 308.15f;

	public float minimumCellMass = 0f;

	public int radius = 2;

	[SerializeField]
	public bool heatLiquid = false;

	public bool hasTargetTemperature = true;

	[Serialize]
	public float UserSliderSetting = 0f;

	public bool produceHeat = false;

	public float maxPower;

	public float minPower;

	public float maxSelfHeatKWs;

	public float maxExhaustedKWs;

	private StatusItem heatStatusItem;

	private StatusItem turboModeStatusItem;

	private HandleVector<int>.Handle structureTemperature;

	private Extents extents;

	private float overheatTemperature;

	[MyCmpReq]
	private Operational operational;

	[MyCmpReq]
	private PrimaryElement primaryElement;

	[MyCmpGet]
	private KBatchedAnimHeatPostProcessingEffect heatEffect;

	[MyCmpGet]
	private EnergyConsumer energyConsumer;

	[MyCmpGet]
	private LiquidHeaterBubbleEmitter bubbleEmitter;

	private List<int> monitorCells = new List<int>();

	public float TargetTemperature => targetTemperature;

	public float MinSelfHeatKWs => maxSelfHeatKWs * (minPower / maxPower);

	public float MinExhaustedKWs => maxExhaustedKWs * (minPower / maxPower);

	public float CurrentSelfHeatKW => Mathf.Lerp(MinSelfHeatKWs, maxSelfHeatKWs, UserSliderSetting);

	public float CurrentExhaustedKW => Mathf.Lerp(MinExhaustedKWs, maxExhaustedKWs, UserSliderSetting);

	public float CurrentPowerConsumption => Mathf.Lerp(minPower, maxPower, UserSliderSetting);

	public bool IsTurboModeActive => heatLiquid && UserSliderSetting > 0f;

	public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.SPACEHEATERSIDESCREEN.TITLE";

	public string SliderUnits => UI.UNITSUFFIXES.ELECTRICAL.WATT;

	public static void GenerateHeat(StatesInstance smi, float dt)
	{
		if (smi.master.produceHeat)
		{
			AddExhaustHeat(smi, dt);
			AddSelfHeat(smi, dt);
		}
	}

	private static float AddExhaustHeat(StatesInstance smi, float dt)
	{
		float currentExhaustedKW = smi.master.CurrentExhaustedKW;
		float num = ((smi.master.bubbleEmitter != null) ? smi.master.bubbleEmitter.DivertEnergy(currentExhaustedKW, dt) : 0f);
		float num2 = Mathf.Max(currentExhaustedKW - num, 0f);
		StructureTemperatureComponents.ExhaustHeat(smi.master.extents, num2, smi.master.overheatTemperature, dt);
		return num2;
	}

	public static void RefreshHeatEffect(StatesInstance smi)
	{
		if (smi.master.heatEffect != null && smi.master.produceHeat)
		{
			float heatBeingProducedValue = (smi.IsInsideState(smi.sm.online.heating) ? (smi.master.CurrentExhaustedKW + smi.master.CurrentSelfHeatKW) : 0f);
			smi.master.heatEffect.SetHeatBeingProducedValue(heatBeingProducedValue);
		}
	}

	private static float AddSelfHeat(StatesInstance smi, float dt)
	{
		float currentSelfHeatKW = smi.master.CurrentSelfHeatKW;
		GameComps.StructureTemperatures.ProduceEnergy(smi.master.structureTemperature, currentSelfHeatKW * dt, BUILDINGS.PREFABS.STEAMTURBINE2.HEAT_SOURCE, dt);
		return currentSelfHeatKW;
	}

	public void SetUserSpecifiedPowerConsumptionValue(float value)
	{
		if (produceHeat)
		{
			UserSliderSetting = (value - minPower) / (maxPower - minPower);
			RefreshHeatEffect(base.smi);
			energyConsumer.BaseWattageRating = CurrentPowerConsumption;
			RefreshTurboModeStatusItem();
		}
	}

	private void RefreshTurboModeStatusItem()
	{
		if (heatLiquid && turboModeStatusItem != null)
		{
			KSelectable component = GetComponent<KSelectable>();
			if (IsTurboModeActive)
			{
				component.SetStatusItem(Db.Get().StatusItemCategories.OperatingEnergy, turboModeStatusItem, this);
			}
			else
			{
				component.SetStatusItem(Db.Get().StatusItemCategories.OperatingEnergy, null);
			}
		}
	}

	protected override void OnPrefabInit()
	{
		if (produceHeat)
		{
			heatStatusItem = new StatusItem("OperatingEnergy", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
			heatStatusItem.resolveStringCallback = delegate(string str, object data)
			{
				StatesInstance statesInstance = (StatesInstance)data;
				float num = statesInstance.master.CurrentSelfHeatKW + statesInstance.master.CurrentExhaustedKW;
				str = string.Format(str, GameUtil.GetFormattedHeatEnergy(num * 1000f));
				return str;
			};
			heatStatusItem.resolveTooltipCallback = delegate(string str, object data)
			{
				StatesInstance statesInstance = (StatesInstance)data;
				float num = statesInstance.master.CurrentSelfHeatKW + statesInstance.master.CurrentExhaustedKW;
				str = str.Replace("{0}", GameUtil.GetFormattedHeatEnergy(num * 1000f));
				string text = string.Format(BUILDING.STATUSITEMS.OPERATINGENERGY.LINEITEM, BUILDING.STATUSITEMS.OPERATINGENERGY.OPERATING, GameUtil.GetFormattedHeatEnergy(statesInstance.master.CurrentSelfHeatKW * 1000f, GameUtil.HeatEnergyFormatterUnit.DTU_S));
				text += string.Format(BUILDING.STATUSITEMS.OPERATINGENERGY.LINEITEM, BUILDING.STATUSITEMS.OPERATINGENERGY.EXHAUSTING, GameUtil.GetFormattedHeatEnergy(statesInstance.master.CurrentExhaustedKW * 1000f, GameUtil.HeatEnergyFormatterUnit.DTU_S));
				str = str.Replace("{1}", text);
				return str;
			};
		}
		if (heatLiquid)
		{
			turboModeStatusItem = new StatusItem("TurboMode", "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
			turboModeStatusItem.resolveStringCallback = delegate(string str, object data)
			{
				SpaceHeater spaceHeater = (SpaceHeater)data;
				str = string.Format(str, GameUtil.GetFormattedWattage(spaceHeater.CurrentPowerConsumption), GameUtil.GetFormattedHeatEnergyRate((spaceHeater.CurrentSelfHeatKW + spaceHeater.CurrentExhaustedKW) * 1000f));
				return str;
			};
			turboModeStatusItem.resolveTooltipCallback = delegate(string str, object data)
			{
				SpaceHeater spaceHeater = (SpaceHeater)data;
				return string.Format(str, GameUtil.GetFormattedWattage(spaceHeater.CurrentPowerConsumption), GameUtil.GetFormattedHeatEnergyRate((spaceHeater.CurrentSelfHeatKW + spaceHeater.CurrentExhaustedKW) * 1000f));
			};
		}
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		GameScheduler.Instance.Schedule("InsulationTutorial", 2f, delegate
		{
			Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_Insulation);
		});
		extents = GetComponent<OccupyArea>().GetExtents();
		overheatTemperature = GetComponent<BuildingComplete>().Def.OverheatTemperature;
		structureTemperature = GameComps.StructureTemperatures.GetHandle(base.gameObject);
		base.smi.StartSM();
		SetUserSpecifiedPowerConsumptionValue(CurrentPowerConsumption);
	}

	public void SetLiquidHeater()
	{
		heatLiquid = true;
	}

	private MonitorState MonitorHeating(float dt)
	{
		monitorCells.Clear();
		int cell = Grid.PosToCell(base.transform.GetPosition());
		GameUtil.GetNonSolidCells(cell, radius, monitorCells);
		int num = 0;
		float num2 = 0f;
		foreach (int monitorCell in monitorCells)
		{
			if (Grid.Mass[monitorCell] > minimumCellMass && ((Grid.Element[monitorCell].IsGas && !heatLiquid) || (Grid.Element[monitorCell].IsLiquid && heatLiquid)))
			{
				num++;
				num2 += Grid.Temperature[monitorCell];
			}
		}
		if (num == 0)
		{
			return heatLiquid ? MonitorState.NotEnoughLiquid : MonitorState.NotEnoughGas;
		}
		if (hasTargetTemperature && num2 / (float)num >= targetTemperature)
		{
			return MonitorState.TooHot;
		}
		return MonitorState.ReadyToHeat;
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		if (hasTargetTemperature)
		{
			Descriptor item = default(Descriptor);
			item.SetupDescriptor(string.Format(UI.BUILDINGEFFECTS.HEATER_TARGETTEMPERATURE, GameUtil.GetFormattedTemperature(targetTemperature)), string.Format(UI.BUILDINGEFFECTS.TOOLTIPS.HEATER_TARGETTEMPERATURE, GameUtil.GetFormattedTemperature(targetTemperature)));
			list.Add(item);
		}
		return list;
	}

	public int SliderDecimalPlaces(int index)
	{
		return 0;
	}

	public float GetSliderMin(int index)
	{
		return (produceHeat && !heatLiquid) ? minPower : 0f;
	}

	public float GetSliderMax(int index)
	{
		return (produceHeat && !heatLiquid) ? maxPower : 0f;
	}

	public float GetSliderValue(int index)
	{
		return CurrentPowerConsumption;
	}

	public void SetSliderValue(float value, int index)
	{
		SetUserSpecifiedPowerConsumptionValue(value);
	}

	public string GetSliderTooltipKey(int index)
	{
		return "STRINGS.UI.UISIDESCREENS.SPACEHEATERSIDESCREEN.TOOLTIP";
	}

	string ISliderControl.GetSliderTooltip(int index)
	{
		return string.Format(Strings.Get("STRINGS.UI.UISIDESCREENS.SPACEHEATERSIDESCREEN.TOOLTIP"), GameUtil.GetFormattedHeatEnergyRate((CurrentSelfHeatKW + CurrentExhaustedKW) * 1000f));
	}
}
