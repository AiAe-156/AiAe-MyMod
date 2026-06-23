using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Klei.AI;
using STRINGS;
using UnityEngine;

namespace Database;

public class CreatureStatusItems : StatusItems
{
	public StatusItem Dead;

	public StatusItem HealthStatus;

	public StatusItem Hot;

	public StatusItem Hot_Crop;

	public StatusItem Scalding;

	public StatusItem Scolding;

	public StatusItem Cold;

	public StatusItem Cold_Crop;

	public StatusItem Crop_Too_Dark;

	public StatusItem Crop_Too_Bright;

	public StatusItem Crop_Blighted;

	public StatusItem Hypothermia;

	public StatusItem Hyperthermia;

	public StatusItem Suffocating;

	public StatusItem AquaticCreatureSuffocating;

	public StatusItem Hatching;

	public StatusItem Incubating;

	public StatusItem Drowning;

	public StatusItem Saturated;

	public StatusItem DryingOut;

	public StatusItem Growing;

	public StatusItem GrowingFruit;

	public StatusItem GermDiet;

	public StatusItem CarnivorousPlantAwaitingVictim;

	public StatusItem ReadyForHarvest;

	public StatusItem ReadyForHarvest_Branch;

	public StatusItem EnvironmentTooWarm;

	public StatusItem EnvironmentTooCold;

	public StatusItem NotSubmerged;

	public StatusItem Entombed;

	public StatusItem Wilting;

	public StatusItem WiltingDomestic;

	public StatusItem WiltingNonGrowing;

	public StatusItem WiltingNonGrowingDomestic;

	public StatusItem WrongAtmosphere;

	public StatusItem AtmosphericPressureTooLow;

	public StatusItem AtmosphericPressureTooHigh;

	public StatusItem Barren;

	public StatusItem NeedsFertilizer;

	public StatusItem NeedsIrrigation;

	public StatusItem WrongTemperature;

	public StatusItem WrongFertilizer;

	public StatusItem WrongIrrigation;

	public StatusItem WrongFertilizerMajor;

	public StatusItem WrongIrrigationMajor;

	public StatusItem CantAcceptFertilizer;

	public StatusItem CantAcceptIrrigation;

	public StatusItem Rotting;

	public StatusItem Fresh;

	public StatusItem Stale;

	public StatusItem Spoiled;

	public StatusItem Refrigerated;

	public StatusItem RefrigeratedFrozen;

	public StatusItem Unrefrigerated;

	public StatusItem SterilizingAtmosphere;

	public StatusItem ContaminatedAtmosphere;

	public StatusItem Old;

	public StatusItem ExchangingElementOutput;

	public StatusItem ExchangingElementConsume;

	public StatusItem Hungry;

	public StatusItem HiveHungry;

	public StatusItem NoSleepSpot;

	public StatusItem ProducingSugarWater;

	public StatusItem SugarWaterProductionPaused;

	public StatusItem SugarWaterProductionWilted;

	public StatusItem SpaceTreeBranchLightStatus;

	public StatusItem OriginalPlantMutation;

	public StatusItem UnknownMutation;

	public StatusItem SpecificPlantMutation;

	public StatusItem Crop_Too_NonRadiated;

	public StatusItem Crop_Too_Radiated;

	public StatusItem ElementGrowthGrowing;

	public StatusItem ElementGrowthStunted;

	public StatusItem ElementGrowthHalted;

	public StatusItem ElementGrowthComplete;

	public StatusItem LookingForFood;

	public StatusItem LookingForGas;

	public StatusItem LookingForLiquid;

	public StatusItem Beckoning;

	public StatusItem BeckoningBlocked;

	public StatusItem MilkProducer;

	public StatusItem MilkFull;

	public StatusItem GettingRanched;

	public StatusItem GettingMilked;

	public StatusItem TemperatureHotUncomfortable;

	public StatusItem TemperatureHotDeadly;

	public StatusItem TemperatureColdUncomfortable;

	public StatusItem TemperatureColdDeadly;

	public StatusItem TravelingToPollinate;

	public StatusItem Pollinating;

	public StatusItem BubbleGasProduction;

	public StatusItem NotPollinated;

	public StatusItem Desiccation;

	public StatusItem FishFullMilk;

	public StatusItem InkFull;

	public StatusItem PunchClamApproach;

	public StatusItem PunchClamAttack;

	public CreatureStatusItems(ResourceSet parent)
		: base("CreatureStatusItems", parent)
	{
		CreateStatusItems();
	}

	private void CreateStatusItems()
	{
		Dead = new StatusItem("Dead", "CREATURES", "", StatusItem.IconType.Exclamation, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Hot = new StatusItem("Hot", "CREATURES", "", StatusItem.IconType.Exclamation, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Hot.resolveStringCallback = delegate(string str, object data)
		{
			TemperatureVulnerable temperatureVulnerable = (TemperatureVulnerable)data;
			return string.Format(str, GameUtil.GetFormattedTemperature(temperatureVulnerable.TemperatureWarningLow), GameUtil.GetFormattedTemperature(temperatureVulnerable.TemperatureWarningHigh));
		};
		Hot_Crop = new StatusItem("Hot_Crop", "CREATURES", "status_item_plant_temperature", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Hot_Crop.resolveStringCallback = delegate(string str, object data)
		{
			TemperatureVulnerable temperatureVulnerable = (TemperatureVulnerable)data;
			str = str.Replace("{low_temperature}", GameUtil.GetFormattedTemperature(temperatureVulnerable.TemperatureWarningLow));
			str = str.Replace("{high_temperature}", GameUtil.GetFormattedTemperature(temperatureVulnerable.TemperatureWarningHigh));
			return str;
		};
		Scalding = new StatusItem("Scalding", "CREATURES", "", StatusItem.IconType.Exclamation, NotificationType.DuplicantThreatening, allow_multiples: true, OverlayModes.None.ID);
		Scalding.resolveTooltipCallback = delegate(string str, object data)
		{
			float averageExternalTemperature = ((ScaldingMonitor.Instance)data).AverageExternalTemperature;
			float scaldingThreshold = ((ScaldingMonitor.Instance)data).GetScaldingThreshold();
			str = str.Replace("{ExternalTemperature}", GameUtil.GetFormattedTemperature(averageExternalTemperature));
			str = str.Replace("{TargetTemperature}", GameUtil.GetFormattedTemperature(scaldingThreshold));
			return str;
		};
		Scalding.AddNotification();
		Scolding = new StatusItem("Scolding", "CREATURES", "", StatusItem.IconType.Exclamation, NotificationType.DuplicantThreatening, allow_multiples: true, OverlayModes.None.ID);
		Scolding.resolveTooltipCallback = delegate(string str, object data)
		{
			float averageExternalTemperature = ((ScaldingMonitor.Instance)data).AverageExternalTemperature;
			float scoldingThreshold = ((ScaldingMonitor.Instance)data).GetScoldingThreshold();
			str = str.Replace("{ExternalTemperature}", GameUtil.GetFormattedTemperature(averageExternalTemperature));
			str = str.Replace("{TargetTemperature}", GameUtil.GetFormattedTemperature(scoldingThreshold));
			return str;
		};
		Scolding.AddNotification();
		Cold = new StatusItem("Cold", "CREATURES", "", StatusItem.IconType.Exclamation, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Cold.resolveStringCallback = delegate(string str, object data)
		{
			TemperatureVulnerable temperatureVulnerable = (TemperatureVulnerable)data;
			return string.Format(str, GameUtil.GetFormattedTemperature(temperatureVulnerable.TemperatureWarningLow), GameUtil.GetFormattedTemperature(temperatureVulnerable.TemperatureWarningHigh));
		};
		Cold_Crop = new StatusItem("Cold_Crop", "CREATURES", "status_item_plant_temperature", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Cold_Crop.resolveStringCallback = delegate(string str, object data)
		{
			TemperatureVulnerable temperatureVulnerable = (TemperatureVulnerable)data;
			str = str.Replace("low_temperature", GameUtil.GetFormattedTemperature(temperatureVulnerable.TemperatureWarningLow));
			str = str.Replace("high_temperature", GameUtil.GetFormattedTemperature(temperatureVulnerable.TemperatureWarningHigh));
			return str;
		};
		BubbleGasProduction = new StatusItem("BubbleGasProduction", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		BubbleGasProduction.resolveStringCallback = delegate(string str, object data)
		{
			Tuple<SimHashes, float> tuple = (Tuple<SimHashes, float>)data;
			Element element = ElementLoader.FindElementByHash(tuple.first);
			str = str.Replace("{ELEMENT}", element.name);
			str = str.Replace("{RATE}", GameUtil.GetFormattedMass(tuple.second, GameUtil.TimeSlice.PerSecond));
			return str;
		};
		Crop_Too_Dark = new StatusItem("Crop_Too_Dark", "CREATURES", "status_item_plant_light", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Crop_Too_Bright = new StatusItem("Crop_Too_Bright", "CREATURES", "status_item_plant_light", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Crop_Blighted = new StatusItem("Crop_Blighted", "CREATURES", "status_item_plant_blighted", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Hyperthermia = new StatusItem("Hyperthermia", "CREATURES", "", StatusItem.IconType.Exclamation, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		Hyperthermia.resolveTooltipCallback = delegate(string str, object data)
		{
			float value = ((TemperatureMonitor.Instance)data).temperature.value;
			float hyperthermiaThreshold = ((TemperatureMonitor.Instance)data).HyperthermiaThreshold;
			str = str.Replace("{InternalTemperature}", GameUtil.GetFormattedTemperature(value));
			str = str.Replace("{TargetTemperature}", GameUtil.GetFormattedTemperature(hyperthermiaThreshold));
			return str;
		};
		Hypothermia = new StatusItem("Hypothermia", "CREATURES", "", StatusItem.IconType.Exclamation, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		Hypothermia.resolveTooltipCallback = delegate(string str, object data)
		{
			float value = ((TemperatureMonitor.Instance)data).temperature.value;
			float hypothermiaThreshold = ((TemperatureMonitor.Instance)data).HypothermiaThreshold;
			str = str.Replace("{InternalTemperature}", GameUtil.GetFormattedTemperature(value));
			str = str.Replace("{TargetTemperature}", GameUtil.GetFormattedTemperature(hypothermiaThreshold));
			return str;
		};
		Suffocating = new StatusItem("Suffocating", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Hatching = new StatusItem("Hatching", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Incubating = new StatusItem("Incubating", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Drowning = new StatusItem("Drowning", "CREATURES", "status_item_flooded", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		Drowning.resolveStringCallback = (string str, object data) => str;
		AquaticCreatureSuffocating = new StatusItem("AquaticCreatureSuffocating", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		AquaticCreatureSuffocating.resolveTooltipCallback = delegate(string str, object data)
		{
			AquaticCreatureSuffocationMonitor.Instance instance = (AquaticCreatureSuffocationMonitor.Instance)data;
			str = GameUtil.SafeStringFormat(str, GameUtil.GetFormattedCycles(instance.TimeUntilDeath));
			return str;
		};
		ProducingSugarWater = new StatusItem("ProducingSugarWater", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Good, allow_multiples: false, OverlayModes.None.ID);
		ProducingSugarWater.resolveStringCallback = delegate(string str, object data)
		{
			SpaceTreePlant.Instance instance = (SpaceTreePlant.Instance)data;
			str = str.Replace("{0}", GameUtil.GetFormattedPercent(instance.CurrentProductionProgress * 100f));
			return str;
		};
		ProducingSugarWater.resolveTooltipCallback = delegate(string str, object data)
		{
			SpaceTreePlant.Instance instance = (SpaceTreePlant.Instance)data;
			PlantBranchGrower.Instance sMI = instance.GetSMI<PlantBranchGrower.Instance>();
			for (int i = 0; i < instance.def.OptimalAmountOfBranches; i++)
			{
				string text = CREATURES.STATUSITEMS.PRODUCINGSUGARWATER.BRANCH_LINE_MISSING;
				string newValue = SpaceTreeBranchConfig.BRANCH_NAMES[i];
				GameObject branch = sMI.GetBranch(i);
				if (branch != null)
				{
					SpaceTreeBranch.Instance sMI2 = branch.GetSMI<SpaceTreeBranch.Instance>();
					if (sMI2 != null && !sMI2.isMasterNull)
					{
						if (sMI2.IsBranchFullyGrown)
						{
							string formattedPercent = GameUtil.GetFormattedPercent(sMI2.Productivity * 100f);
							text = CREATURES.STATUSITEMS.PRODUCINGSUGARWATER.BRANCH_LINE;
							text = text.Replace("{1}", formattedPercent);
						}
						else
						{
							string formattedPercent2 = GameUtil.GetFormattedPercent(sMI2.GetcurrentGrowthPercentage() * 100f);
							text = CREATURES.STATUSITEMS.PRODUCINGSUGARWATER.BRANCH_LINE_GROWING;
							text = text.Replace("{1}", formattedPercent2);
						}
					}
				}
				text = text.Replace("{0}", newValue);
				string oldValue = "{BRANCH_" + i + "}";
				str = str.Replace(oldValue, text);
			}
			str = str.Replace("{0}", GameUtil.GetFormattedMass(instance.GetProductionSpeed() * 20f / instance.OptimalProductionDuration, GameUtil.TimeSlice.PerSecond));
			str = str.Replace("{1}", instance.def.OptimalAmountOfBranches.ToString());
			str = str.Replace("{2}", GameUtil.GetFormattedLux(10000));
			return str;
		};
		SugarWaterProductionPaused = new StatusItem("SugarWaterProductionPaused", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		SugarWaterProductionPaused.resolveStringCallback = delegate(string str, object data)
		{
			SpaceTreePlant.Instance instance = (SpaceTreePlant.Instance)data;
			str = str.Replace("{0}", GameUtil.GetFormattedPercent(instance.CurrentProductionProgress * 100f));
			return str;
		};
		SugarWaterProductionPaused.resolveTooltipCallback = delegate(string str, object data)
		{
			SpaceTreePlant.Instance instance = (SpaceTreePlant.Instance)data;
			PlantBranchGrower.Instance sMI = instance.GetSMI<PlantBranchGrower.Instance>();
			for (int i = 0; i < instance.def.OptimalAmountOfBranches; i++)
			{
				string text = CREATURES.STATUSITEMS.SUGARWATERPRODUCTIONPAUSED.BRANCH_LINE_MISSING;
				string newValue = SpaceTreeBranchConfig.BRANCH_NAMES[i];
				GameObject branch = sMI.GetBranch(i);
				if (branch != null)
				{
					SpaceTreeBranch.Instance sMI2 = branch.GetSMI<SpaceTreeBranch.Instance>();
					if (sMI2 != null && !sMI2.isMasterNull)
					{
						if (sMI2.IsBranchFullyGrown)
						{
							string formattedPercent = GameUtil.GetFormattedPercent(sMI2.Productivity * 100f);
							text = CREATURES.STATUSITEMS.SUGARWATERPRODUCTIONPAUSED.BRANCH_LINE;
							text = text.Replace("{1}", formattedPercent);
						}
						else
						{
							string formattedPercent2 = GameUtil.GetFormattedPercent(sMI2.GetcurrentGrowthPercentage() * 100f);
							text = CREATURES.STATUSITEMS.SUGARWATERPRODUCTIONPAUSED.BRANCH_LINE_GROWING;
							text = text.Replace("{1}", formattedPercent2);
						}
					}
				}
				text = text.Replace("{0}", newValue);
				string oldValue = "{BRANCH_" + i + "}";
				str = str.Replace(oldValue, text);
			}
			str = str.Replace("{0}", instance.def.OptimalAmountOfBranches.ToString());
			str = str.Replace("{1}", GameUtil.GetFormattedLux(10000));
			return str;
		};
		SugarWaterProductionWilted = new StatusItem("SugarWaterProductionWilted", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		SugarWaterProductionWilted.resolveStringCallback = delegate(string str, object data)
		{
			SpaceTreePlant.Instance instance = (SpaceTreePlant.Instance)data;
			str = str.Replace("{0}", GameUtil.GetFormattedPercent(instance.CurrentProductionProgress * 100f));
			return str;
		};
		SpaceTreeBranchLightStatus = new StatusItem("SpaceTreeBranchLightStatus", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Good, allow_multiples: false, OverlayModes.None.ID);
		SpaceTreeBranchLightStatus.resolveStringCallback = delegate(string str, object data)
		{
			SpaceTreeBranch.Instance instance = (SpaceTreeBranch.Instance)data;
			str = str.Replace("{0}", GameUtil.GetFormattedPercent(instance.Productivity * 100f));
			return str;
		};
		SpaceTreeBranchLightStatus.resolveTooltipCallback = delegate(string str, object data)
		{
			SpaceTreeBranch.Instance instance = (SpaceTreeBranch.Instance)data;
			str = str.Replace("{0}", GameUtil.GetFormattedLux(instance.def.OPTIMAL_LUX_LEVELS));
			str = str.Replace("{1}", GameUtil.GetFormattedLux(instance.CurrentAmountOfLux));
			return str;
		};
		Saturated = new StatusItem("Saturated", "CREATURES", "status_item_flooded", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		Saturated.resolveStringCallback = (string str, object data) => str;
		DryingOut = new StatusItem("DryingOut", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: true, 1026);
		DryingOut.resolveStringCallback = (string str, object data) => str;
		ReadyForHarvest = new StatusItem("ReadyForHarvest", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: true, 1026);
		ReadyForHarvest_Branch = new StatusItem("ReadyForHarvest_Branch", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: true, 1026);
		Growing = new StatusItem("Growing", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: true, 1026);
		Growing.resolveStringCallback = delegate(string str, object data)
		{
			IManageGrowingStates manageGrowingStates = (IManageGrowingStates)data;
			if (manageGrowingStates.GetCropComponent() != null)
			{
				float seconds = manageGrowingStates.TimeUntilNextHarvest();
				str = str.Replace("{TimeUntilNextHarvest}", GameUtil.GetFormattedCycles(seconds));
			}
			float val = 100f * manageGrowingStates.PercentGrown();
			str = str.Replace("{PercentGrow}", Math.Floor(Math.Max(val, 0f)).ToString("F0"));
			return str;
		};
		GrowingFruit = new StatusItem("GrowingFruit", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: true, 1026);
		GrowingFruit.resolveStringCallback = delegate(string str, object data)
		{
			IManageGrowingStates manageGrowingStates = (IManageGrowingStates)data;
			if (manageGrowingStates.GetCropComponent() != null)
			{
				float seconds = manageGrowingStates.TimeUntilNextHarvest();
				str = str.Replace("{TimeUntilNextHarvest}", GameUtil.GetFormattedCycles(seconds));
			}
			float val = 100f * manageGrowingStates.PercentGrown();
			str = str.Replace("{PercentGrow}", Math.Floor(Math.Max(val, 0f)).ToString("F0"));
			return str;
		};
		CarnivorousPlantAwaitingVictim = new StatusItem("CarnivorousPlantAwaitingVictim", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: true, 1026);
		CarnivorousPlantAwaitingVictim.resolveTooltipCallback = delegate(string str, object data)
		{
			string[] formattedPossiblePreyList = ((IPlantConsumeEntities)data).GetFormattedPossiblePreyList();
			string text = "";
			for (int i = 0; i < formattedPossiblePreyList.Length; i++)
			{
				text = text + "\n" + GameUtil.SafeStringFormat(CREATURES.STATUSITEMS.CARNIVOROUSPLANTAWAITINGVICTIM.TOOLTIP_ITEM, formattedPossiblePreyList[i]);
			}
			str += text;
			return str;
		};
		EnvironmentTooWarm = new StatusItem("EnvironmentTooWarm", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		EnvironmentTooWarm.resolveStringCallback = delegate(string str, object data)
		{
			float temp = Grid.Temperature[Grid.PosToCell(((TemperatureVulnerable)data).gameObject)];
			float temp2 = ((TemperatureVulnerable)data).TemperatureLethalHigh - 1f;
			str = str.Replace("{ExternalTemperature}", GameUtil.GetFormattedTemperature(temp));
			str = str.Replace("{TargetTemperature}", GameUtil.GetFormattedTemperature(temp2));
			return str;
		};
		EnvironmentTooCold = new StatusItem("EnvironmentTooCold", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		EnvironmentTooCold.resolveStringCallback = delegate(string str, object data)
		{
			float temp = Grid.Temperature[Grid.PosToCell(((TemperatureVulnerable)data).gameObject)];
			float temp2 = ((TemperatureVulnerable)data).TemperatureLethalLow + 1f;
			str = str.Replace("{ExternalTemperature}", GameUtil.GetFormattedTemperature(temp));
			str = str.Replace("{TargetTemperature}", GameUtil.GetFormattedTemperature(temp2));
			return str;
		};
		Entombed = new StatusItem("Entombed", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		Entombed.resolveStringCallback = (string str, object go) => str;
		Entombed.resolveTooltipCallback = delegate(string str, object go)
		{
			GameObject go2 = go as GameObject;
			return string.Format(str, GameUtil.GetIdentityDescriptor(go2));
		};
		NotSubmerged = new StatusItem("NotSubmerged", "CREATURES", "status_item_flooded", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		Wilting = new StatusItem("Wilting", "CREATURES", "status_item_need_plant", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false, 1026);
		Wilting.resolveStringCallback = delegate(string str, object data)
		{
			Growing growing = data as Growing;
			if (growing != null && data != null)
			{
				AmountInstance amountInstance = growing.gameObject.GetAmounts().Get(Db.Get().Amounts.Maturity);
				str = str.Replace("{TimeUntilNextHarvest}", GameUtil.GetFormattedCycles(Mathf.Min(amountInstance.GetMax(), growing.TimeUntilNextHarvest())));
			}
			str = str.Replace("{Reasons}", (data as KMonoBehaviour).GetComponent<WiltCondition>().WiltCausesString());
			return str;
		};
		WiltingDomestic = new StatusItem("WiltingDomestic", "CREATURES", "status_item_need_plant", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: true, 1026);
		WiltingDomestic.resolveStringCallback = delegate(string str, object data)
		{
			Growing growing = data as Growing;
			if (growing != null && data != null)
			{
				AmountInstance amountInstance = growing.gameObject.GetAmounts().Get(Db.Get().Amounts.Maturity);
				str = str.Replace("{TimeUntilNextHarvest}", GameUtil.GetFormattedCycles(Mathf.Min(amountInstance.GetMax(), growing.TimeUntilNextHarvest())));
			}
			str = str.Replace("{Reasons}", (data as KMonoBehaviour).GetComponent<WiltCondition>().WiltCausesString());
			return str;
		};
		WiltingNonGrowing = new StatusItem("WiltingNonGrowing", "CREATURES", "status_item_need_plant", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false, 1026);
		WiltingNonGrowing.resolveStringCallback = delegate(string str, object data)
		{
			str = CREATURES.STATUSITEMS.WILTING_NON_GROWING_PLANT.NAME;
			str = str.Replace("{Reasons}", (data as WiltCondition).WiltCausesString());
			return str;
		};
		WiltingNonGrowingDomestic = new StatusItem("WiltingNonGrowing", "CREATURES", "status_item_need_plant", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: true, 1026);
		WiltingNonGrowingDomestic.resolveStringCallback = delegate(string str, object data)
		{
			str = CREATURES.STATUSITEMS.WILTING_NON_GROWING_PLANT.NAME;
			str = str.Replace("{Reasons}", (data as WiltCondition).WiltCausesString());
			return str;
		};
		WrongAtmosphere = new StatusItem("WrongAtmosphere", "CREATURES", "status_item_plant_atmosphere", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		WrongAtmosphere.resolveStringCallback = delegate(string str, object data)
		{
			string text = "";
			foreach (Element safe_atmosphere in (data as PressureVulnerable).safe_atmospheres)
			{
				text = text + "\n    •  " + safe_atmosphere.name;
			}
			str = str.Replace("{elements}", text);
			return str;
		};
		AtmosphericPressureTooLow = new StatusItem("AtmosphericPressureTooLow", "CREATURES", "status_item_plant_atmosphere", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		AtmosphericPressureTooLow.resolveStringCallback = delegate(string str, object data)
		{
			PressureVulnerable pressureVulnerable = (PressureVulnerable)data;
			str = str.Replace("{low_mass}", GameUtil.GetFormattedMass(pressureVulnerable.pressureWarning_Low));
			str = str.Replace("{high_mass}", GameUtil.GetFormattedMass(pressureVulnerable.pressureWarning_High));
			return str;
		};
		AtmosphericPressureTooHigh = new StatusItem("AtmosphericPressureTooHigh", "CREATURES", "status_item_plant_atmosphere", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		AtmosphericPressureTooHigh.resolveStringCallback = delegate(string str, object data)
		{
			PressureVulnerable pressureVulnerable = (PressureVulnerable)data;
			str = str.Replace("{low_mass}", GameUtil.GetFormattedMass(pressureVulnerable.pressureWarning_Low));
			str = str.Replace("{high_mass}", GameUtil.GetFormattedMass(pressureVulnerable.pressureWarning_High));
			return str;
		};
		HealthStatus = new StatusItem("HealthStatus", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		HealthStatus.resolveStringCallback = delegate(string str, object data)
		{
			string newValue = "";
			switch ((Health.HealthState)data)
			{
			case Health.HealthState.Perfect:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.PERFECT.NAME;
				break;
			case Health.HealthState.Scuffed:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.SCUFFED.NAME;
				break;
			case Health.HealthState.Injured:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.INJURED.NAME;
				break;
			case Health.HealthState.Critical:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.CRITICAL.NAME;
				break;
			case Health.HealthState.Incapacitated:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.INCAPACITATED.NAME;
				break;
			case Health.HealthState.Dead:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.DEAD.NAME;
				break;
			}
			str = str.Replace("{healthState}", newValue);
			return str;
		};
		HealthStatus.resolveTooltipCallback = delegate(string str, object data)
		{
			string newValue = "";
			switch ((Health.HealthState)data)
			{
			case Health.HealthState.Perfect:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.PERFECT.TOOLTIP;
				break;
			case Health.HealthState.Scuffed:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.SCUFFED.TOOLTIP;
				break;
			case Health.HealthState.Injured:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.INJURED.TOOLTIP;
				break;
			case Health.HealthState.Critical:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.CRITICAL.TOOLTIP;
				break;
			case Health.HealthState.Incapacitated:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.INCAPACITATED.TOOLTIP;
				break;
			case Health.HealthState.Dead:
				newValue = MISC.STATUSITEMS.HEALTHSTATUS.DEAD.TOOLTIP;
				break;
			}
			str = str.Replace("{healthState}", newValue);
			return str;
		};
		Barren = new StatusItem("Barren", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		NeedsFertilizer = new StatusItem("NeedsFertilizer", "CREATURES", "status_item_plant_solid", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Func<string, object, string> resolveStringCallback = (string str, object data) => str;
		NeedsFertilizer.resolveStringCallback = resolveStringCallback;
		NeedsIrrigation = new StatusItem("NeedsIrrigation", "CREATURES", "status_item_plant_liquid", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Func<string, object, string> resolveStringCallback2 = (string str, object data) => str;
		NeedsIrrigation.resolveStringCallback = resolveStringCallback2;
		WrongFertilizer = new StatusItem("WrongFertilizer", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Func<string, object, string> resolveStringCallback3 = (string str, object data) => str;
		WrongFertilizer.resolveStringCallback = resolveStringCallback3;
		WrongFertilizerMajor = new StatusItem("WrongFertilizerMajor", "CREATURES", "status_item_fabricator_empty", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		WrongFertilizerMajor.resolveStringCallback = resolveStringCallback3;
		WrongIrrigation = new StatusItem("WrongIrrigation", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Func<string, object, string> resolveStringCallback4 = (string str, object data) => str;
		WrongIrrigation.resolveStringCallback = resolveStringCallback4;
		WrongIrrigationMajor = new StatusItem("WrongIrrigationMajor", "CREATURES", "status_item_fabricator_empty", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		WrongIrrigationMajor.resolveStringCallback = resolveStringCallback4;
		CantAcceptFertilizer = new StatusItem("CantAcceptFertilizer", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		Rotting = new StatusItem("Rotting", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Rotting.resolveStringCallback = (string str, object data) => str.Replace("{RotTemperature}", GameUtil.GetFormattedTemperature(277.15f));
		Fresh = new StatusItem("Fresh", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Fresh.resolveStringCallback = delegate(string str, object data)
		{
			Rottable.Instance instance = (Rottable.Instance)data;
			return str.Replace("{RotPercentage}", "(" + Util.FormatWholeNumber(instance.RotConstitutionPercentage * 100f) + "%)");
		};
		Fresh.resolveTooltipCallback = delegate(string str, object data)
		{
			Rottable.Instance instance = (Rottable.Instance)data;
			return str.Replace("{RotTooltip}", instance.GetToolTip());
		};
		Stale = new StatusItem("Stale", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		Stale.resolveStringCallback = delegate(string str, object data)
		{
			Rottable.Instance instance = (Rottable.Instance)data;
			return str.Replace("{RotPercentage}", "(" + Util.FormatWholeNumber(instance.RotConstitutionPercentage * 100f) + "%)");
		};
		Stale.resolveTooltipCallback = delegate(string str, object data)
		{
			Rottable.Instance instance = (Rottable.Instance)data;
			return str.Replace("{RotTooltip}", instance.GetToolTip());
		};
		Spoiled = new StatusItem("Spoiled", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		Func<string, object, string> resolveStringCallback5 = delegate(string str, object data)
		{
			IRottable rottable = (IRottable)data;
			return str.Replace("{RotTemperature}", GameUtil.GetFormattedTemperature(rottable.RotTemperature)).Replace("{PreserveTemperature}", GameUtil.GetFormattedTemperature(rottable.PreserveTemperature));
		};
		Refrigerated = new StatusItem("Refrigerated", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Refrigerated.resolveStringCallback = resolveStringCallback5;
		RefrigeratedFrozen = new StatusItem("RefrigeratedFrozen", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		RefrigeratedFrozen.resolveStringCallback = resolveStringCallback5;
		Unrefrigerated = new StatusItem("Unrefrigerated", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		Unrefrigerated.resolveStringCallback = resolveStringCallback5;
		SterilizingAtmosphere = new StatusItem("SterilizingAtmosphere", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ContaminatedAtmosphere = new StatusItem("ContaminatedAtmosphere", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		Old = new StatusItem("Old", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		Old.resolveTooltipCallback = delegate(string str, object data)
		{
			AgeMonitor.Instance instance = (AgeMonitor.Instance)data;
			return str.Replace("{TimeUntilDeath}", GameUtil.GetFormattedCycles(instance.CyclesUntilDeath * 600f));
		};
		ExchangingElementConsume = new StatusItem("ExchangingElementConsume", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ExchangingElementConsume.resolveStringCallback = delegate(string str, object data)
		{
			EntityElementExchanger.StatesInstance statesInstance = (EntityElementExchanger.StatesInstance)data;
			str = str.Replace("{ConsumeElement}", ElementLoader.FindElementByHash(statesInstance.master.consumedElement).tag.ProperName());
			str = str.Replace("{ConsumeRate}", GameUtil.GetFormattedMass(statesInstance.master.consumeRate, GameUtil.TimeSlice.PerSecond));
			return str;
		};
		ExchangingElementConsume.resolveTooltipCallback = delegate(string str, object data)
		{
			EntityElementExchanger.StatesInstance statesInstance = (EntityElementExchanger.StatesInstance)data;
			str = str.Replace("{ConsumeElement}", ElementLoader.FindElementByHash(statesInstance.master.consumedElement).tag.ProperName());
			str = str.Replace("{ConsumeRate}", GameUtil.GetFormattedMass(statesInstance.master.consumeRate, GameUtil.TimeSlice.PerSecond));
			return str;
		};
		ExchangingElementOutput = new StatusItem("ExchangingElementOutput", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ExchangingElementOutput.resolveStringCallback = delegate(string str, object data)
		{
			EntityElementExchanger.StatesInstance statesInstance = (EntityElementExchanger.StatesInstance)data;
			str = str.Replace("{OutputElement}", ElementLoader.FindElementByHash(statesInstance.master.emittedElement).tag.ProperName());
			str = str.Replace("{OutputRate}", GameUtil.GetFormattedMass(statesInstance.master.consumeRate * statesInstance.master.exchangeRatio, GameUtil.TimeSlice.PerSecond));
			return str;
		};
		ExchangingElementOutput.resolveTooltipCallback = delegate(string str, object data)
		{
			EntityElementExchanger.StatesInstance statesInstance = (EntityElementExchanger.StatesInstance)data;
			str = str.Replace("{OutputElement}", ElementLoader.FindElementByHash(statesInstance.master.emittedElement).tag.ProperName());
			str = str.Replace("{OutputRate}", GameUtil.GetFormattedMass(statesInstance.master.consumeRate * statesInstance.master.exchangeRatio, GameUtil.TimeSlice.PerSecond));
			return str;
		};
		Hungry = new StatusItem("Hungry", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Hungry.resolveTooltipCallback = delegate(string str, object data)
		{
			Diet diet = ((CreatureCalorieMonitor.Instance)data).stomach.diet;
			if (diet.consumedTags.Count > 0)
			{
				string[] array = diet.consumedTags.Select((KeyValuePair<Tag, float> t) => t.Key.ProperName()).ToArray();
				if (array.Length > 3)
				{
					array = new string[4]
					{
						array[0],
						array[1],
						array[2],
						"..."
					};
				}
				string newValue = string.Join(", ", array);
				return str + "\n" + UI.BUILDINGEFFECTS.DIET_CONSUMED.text.Replace("{Foodlist}", newValue);
			}
			return str;
		};
		HiveHungry = new StatusItem("HiveHungry", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		HiveHungry.resolveTooltipCallback = delegate(string str, object data)
		{
			Diet diet = ((BeehiveCalorieMonitor.Instance)data).stomach.diet;
			if (diet.consumedTags.Count > 0)
			{
				string[] array = diet.consumedTags.Select((KeyValuePair<Tag, float> t) => t.Key.ProperName()).ToArray();
				if (array.Length > 3)
				{
					array = new string[4]
					{
						array[0],
						array[1],
						array[2],
						"..."
					};
				}
				string newValue = string.Join(", ", array);
				return str + "\n" + UI.BUILDINGEFFECTS.DIET_STORED.text.Replace("{Foodlist}", newValue);
			}
			return str;
		};
		NoSleepSpot = new StatusItem("NoSleepSpot", "CREATURES", "", StatusItem.IconType.Exclamation, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		OriginalPlantMutation = new StatusItem("OriginalPlantMutation", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		UnknownMutation = new StatusItem("UnknownMutation", "CREATURES", "status_item_unknown_mutation", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		SpecificPlantMutation = new StatusItem("SpecificPlantMutation", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		SpecificPlantMutation.resolveStringCallback = delegate(string str, object data)
		{
			PlantMutation plantMutation = (PlantMutation)data;
			return str.Replace("{MutationName}", plantMutation.Name);
		};
		SpecificPlantMutation.resolveTooltipCallback = delegate(string str, object data)
		{
			PlantMutation plantMutation = (PlantMutation)data;
			str = str.Replace("{MutationName}", plantMutation.Name);
			return str + "\n" + plantMutation.GetTooltip();
		};
		Crop_Too_NonRadiated = new StatusItem("Crop_Too_NonRadiated", "CREATURES", "status_item_plant_light", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		Crop_Too_Radiated = new StatusItem("Crop_Too_Radiated", "CREATURES", "status_item_plant_light", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID, showWorldIcon: false);
		ElementGrowthGrowing = new StatusItem("Element_Growth_Growing", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ElementGrowthGrowing.resolveTooltipCallback = delegate(string str, object data)
		{
			ElementGrowthMonitor.Instance instance = (ElementGrowthMonitor.Instance)data;
			StringBuilder stringBuilder = new StringBuilder(str, str.Length * 2);
			stringBuilder.Replace("{templo}", GameUtil.GetFormattedTemperature(instance.def.minTemperature));
			stringBuilder.Replace("{temphi}", GameUtil.GetFormattedTemperature(instance.def.maxTemperature));
			if (instance.lastConsumedTemperature > 0f)
			{
				stringBuilder.Append("\n\n");
				stringBuilder.Append(CREATURES.STATUSITEMS.ELEMENT_GROWTH_GROWING.PREFERRED_TEMP);
				stringBuilder.Replace("{element}", ElementLoader.FindElementByHash(instance.lastConsumedElement).name);
				stringBuilder.Replace("{temperature}", GameUtil.GetFormattedTemperature(instance.lastConsumedTemperature));
			}
			return stringBuilder.ToString();
		};
		ElementGrowthStunted = new StatusItem("Element_Growth_Stunted", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		ElementGrowthStunted.resolveTooltipCallback = ElementGrowthGrowing.resolveTooltipCallback;
		ElementGrowthStunted.resolveStringCallback = delegate(string str, object data)
		{
			ElementGrowthMonitor.Instance instance = (ElementGrowthMonitor.Instance)data;
			string newValue = ((instance.lastConsumedTemperature < instance.def.minTemperature) ? CREATURES.STATUSITEMS.ELEMENT_GROWTH_STUNTED.TOO_COLD : CREATURES.STATUSITEMS.ELEMENT_GROWTH_STUNTED.TOO_HOT);
			str = str.Replace("{reason}", newValue);
			return str;
		};
		ElementGrowthHalted = new StatusItem("Element_Growth_Halted", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		ElementGrowthHalted.resolveTooltipCallback = ElementGrowthGrowing.resolveTooltipCallback;
		ElementGrowthComplete = new StatusItem("Element_Growth_Complete", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		ElementGrowthComplete.resolveTooltipCallback = ElementGrowthGrowing.resolveTooltipCallback;
		LookingForFood = new StatusItem("Hungry", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		LookingForGas = new StatusItem("LookingForGas", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		LookingForLiquid = new StatusItem("LookingForLiquid", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Beckoning = new StatusItem("Beckoning", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		BeckoningBlocked = new StatusItem("BeckoningBlocked", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		MilkProducer = new StatusItem("MilkProducer", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		MilkProducer.resolveStringCallback = delegate(string str, object data)
		{
			MilkProductionMonitor.Instance instance = (MilkProductionMonitor.Instance)data;
			str = str.Replace("{amount}", GameUtil.GetFormattedMass(instance.MilkPercentage));
			return str;
		};
		GettingRanched = new StatusItem("Getting_Ranched", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		GettingMilked = new StatusItem("Getting_Milked", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		MilkFull = new StatusItem("MilkFull", "CREATURES", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		TemperatureHotUncomfortable = new StatusItem("TemperatureHotUncomfortable", CREATURES.STATUSITEMS.TEMPERATURE_HOT_UNCOMFORTABLE.NAME, CREATURES.STATUSITEMS.TEMPERATURE_HOT_UNCOMFORTABLE.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		TemperatureHotDeadly = new StatusItem("TemperatureHotDeadly", CREATURES.STATUSITEMS.TEMPERATURE_HOT_DEADLY.NAME, CREATURES.STATUSITEMS.TEMPERATURE_HOT_DEADLY.TOOLTIP, "", StatusItem.IconType.Exclamation, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		TemperatureColdUncomfortable = new StatusItem("TemperatureColdUncomfortable", CREATURES.STATUSITEMS.TEMPERATURE_COLD_UNCOMFORTABLE.NAME, CREATURES.STATUSITEMS.TEMPERATURE_COLD_UNCOMFORTABLE.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		TemperatureColdDeadly = new StatusItem("TemperatureColdDeadly", CREATURES.STATUSITEMS.TEMPERATURE_COLD_DEADLY.NAME, CREATURES.STATUSITEMS.TEMPERATURE_COLD_DEADLY.TOOLTIP, "", StatusItem.IconType.Exclamation, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		TemperatureHotUncomfortable.resolveStringCallback = delegate(string str, object obj)
		{
			CritterTemperatureMonitor.Instance instance = (CritterTemperatureMonitor.Instance)obj;
			return string.Format(str, GameUtil.GetFormattedTemperature(instance.GetTemperatureInternal()), GameUtil.GetFormattedTemperature(instance.def.temperatureColdUncomfortable), GameUtil.GetFormattedTemperature(instance.def.temperatureHotUncomfortable), Effect.CreateTooltip(instance.sm.uncomfortableEffect, showDuration: false));
		};
		TemperatureHotDeadly.resolveStringCallback = delegate(string str, object obj)
		{
			CritterTemperatureMonitor.Instance instance = (CritterTemperatureMonitor.Instance)obj;
			return string.Format(str, GameUtil.GetFormattedTemperature(instance.GetTemperatureExternal()), GameUtil.GetFormattedTemperature(instance.def.temperatureColdDeadly), GameUtil.GetFormattedTemperature(instance.def.temperatureHotDeadly), Effect.CreateTooltip(instance.sm.deadlyEffect, showDuration: false));
		};
		TemperatureColdUncomfortable.resolveStringCallback = TemperatureHotUncomfortable.resolveStringCallback;
		TemperatureColdDeadly.resolveStringCallback = TemperatureHotDeadly.resolveStringCallback;
		TravelingToPollinate = new StatusItem("POLLINATING.MOVINGTO", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Pollinating = new StatusItem("POLLINATING.INTERACTING", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		NotPollinated = new StatusItem("NOT_POLLINATED", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Desiccation = new StatusItem("Desiccation", "CREATURES", string.Empty, StatusItem.IconType.Exclamation, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		Desiccation.SetResolveStringCallback((string str, object data) => (!(data is DesiccationMonitor.Instance instance)) ? str : string.Format(str, GameUtil.GetFormattedTime(instance.GetEstimatedTimeUntilDeath())));
		FishFullMilk = new StatusItem("FishMilkFull", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		InkFull = new StatusItem("InkFull", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		PunchClamApproach = new StatusItem("PUNCH_CLAM_APPROACH", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		PunchClamAttack = new StatusItem("PUNCH_CLAM_ATTACK", "CREATURES", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
	}
}
