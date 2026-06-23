using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

namespace Database;

public class MiscStatusItems : StatusItems
{
	public StatusItem AttentionRequired;

	public StatusItem MarkedForDisinfection;

	public StatusItem MarkedForCompost;

	public StatusItem MarkedForCompostInStorage;

	public StatusItem PendingClear;

	public StatusItem PendingClearNoStorage;

	public StatusItem Edible;

	public StatusItem WaitingForDig;

	public StatusItem WaitingForMop;

	public StatusItem OreMass;

	public StatusItem OreTemp;

	public StatusItem ElementalCategory;

	public StatusItem ElementalState;

	public StatusItem ElementalTemperature;

	public StatusItem ElementalMass;

	public StatusItem ElementalDisease;

	public StatusItem TreeFilterableTags;

	public StatusItem SublimationOverpressure;

	public StatusItem SublimationEmitting;

	public StatusItem SublimationBlocked;

	public StatusItem BuriedItem;

	public StatusItem SpoutOverPressure;

	public StatusItem SpoutEmitting;

	public StatusItem SpoutPressureBuilding;

	public StatusItem SpoutIdle;

	public StatusItem SpoutDormant;

	public StatusItem SpicedFood;

	public StatusItem RehydratedFood;

	public StatusItem OrderAttack;

	public StatusItem OrderCapture;

	public StatusItem PendingHarvest;

	public StatusItem NotMarkedForHarvest;

	public StatusItem PendingUproot;

	public StatusItem PickupableUnreachable;

	public StatusItem Prioritized;

	public StatusItem Using;

	public StatusItem Operating;

	public StatusItem Cleaning;

	public StatusItem RegionIsBlocked;

	public StatusItem NoClearLocationsAvailable;

	public StatusItem AwaitingStudy;

	public StatusItem Studied;

	public StatusItem StudiedGeyserTimeRemaining;

	public StatusItem Space;

	public StatusItem HighEnergyParticleCount;

	public StatusItem Durability;

	public StatusItem StoredItemDurability;

	public StatusItem ArtifactEntombed;

	public StatusItem TearOpen;

	public StatusItem TearClosed;

	public StatusItem ImpactorStatus;

	public StatusItem ImpactorHealth;

	public StatusItem LongRangeMissileTTI;

	public StatusItem ClusterMeteorRemainingTravelTime;

	public StatusItem MarkedForMove;

	public StatusItem MoveStorageUnreachable;

	public StatusItem GrowingBranches;

	public StatusItem BionicExplorerBooster;

	public StatusItem BionicExplorerBoosterReady;

	public StatusItem UnassignedBionicBooster;

	public StatusItem ElectrobankLifetimeRemaining;

	public StatusItem ElectrobankSelfCharging;

	public StatusItem ClusterMapHarvestableResource;

	public StatusItem BackwallMass;

	public StatusItem BackwallTemperature;

	public StatusItem BubbleContents;

	public StatusItem UnderwaterVentBuildUpProgress;

	public StatusItem UnderwaterVentEmiting;

	public StatusItem UnderwaterVentBlocked;

	public StatusItem UnderwaterVentBeingDrilled;

	public StatusItem MinnowPOIDehydratedStatus;

	public MiscStatusItems(ResourceSet parent)
		: base("MiscStatusItems", parent)
	{
		CreateStatusItems();
	}

	private StatusItem CreateStatusItem(string id, string prefix, string icon, StatusItem.IconType icon_type, NotificationType notification_type, bool allow_multiples, HashedString render_overlay, bool showWorldIcon = true, int status_overlays = 129022)
	{
		return Add(new StatusItem(id, prefix, icon, icon_type, notification_type, allow_multiples, render_overlay, showWorldIcon, status_overlays));
	}

	private StatusItem CreateStatusItem(string id, string name, string tooltip, string icon, StatusItem.IconType icon_type, NotificationType notification_type, bool allow_multiples, HashedString render_overlay, int status_overlays = 129022)
	{
		return Add(new StatusItem(id, name, tooltip, icon, icon_type, notification_type, allow_multiples, render_overlay, status_overlays));
	}

	private void CreateStatusItems()
	{
		AttentionRequired = CreateStatusItem("AttentionRequired", "MISC", "status_item_doubleexclamation", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Edible = CreateStatusItem("Edible", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Edible.resolveStringCallback = delegate(string str, object data)
		{
			Edible edible = (Edible)data;
			str = string.Format(str, GameUtil.GetFormattedCalories(edible.Calories));
			return str;
		};
		PendingClear = CreateStatusItem("PendingClear", "MISC", "status_item_pending_clear", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		PendingClearNoStorage = CreateStatusItem("PendingClearNoStorage", "MISC", "status_item_pending_clear", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		MarkedForCompost = CreateStatusItem("MarkedForCompost", "MISC", "status_item_pending_compost", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		MarkedForCompostInStorage = CreateStatusItem("MarkedForCompostInStorage", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		MarkedForDisinfection = CreateStatusItem("MarkedForDisinfection", "MISC", "status_item_disinfect", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.Disease.ID);
		NoClearLocationsAvailable = CreateStatusItem("NoClearLocationsAvailable", "MISC", "status_item_no_filter_set", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		WaitingForDig = CreateStatusItem("WaitingForDig", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		WaitingForMop = CreateStatusItem("WaitingForMop", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		OreMass = CreateStatusItem("OreMass", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		OreMass.resolveStringCallback = delegate(string str, object data)
		{
			GameObject gameObject = (GameObject)data;
			str = str.Replace("{Mass}", GameUtil.GetFormattedMass(gameObject.GetComponent<PrimaryElement>().Mass));
			return str;
		};
		UnderwaterVentBlocked = CreateStatusItem("UnderwaterVentBlocked", "MISC", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		UnderwaterVentBeingDrilled = CreateStatusItem("UnderwaterVentBeingDrilled", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		UnderwaterVentEmiting = CreateStatusItem("UnderwaterVentEmiting", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		UnderwaterVentEmiting.resolveStringCallback = delegate(string str, object data)
		{
			UnderwaterVent.Instance instance = (UnderwaterVent.Instance)data;
			str = str.Replace("{ELEMENT_NAME}", GameUtil.GetElementNameByElementHash(instance.def.data.BubbleElement));
			str = str.Replace("{RATE}", GameUtil.GetFormattedMass(instance.def.data.BubbleMassRate, GameUtil.TimeSlice.PerSecond));
			str = str.Replace("{TEMP}", GameUtil.GetFormattedTemperature(instance.def.data.BubbleTemp));
			return str;
		};
		UnderwaterVentBuildUpProgress = CreateStatusItem("UnderwaterVentBuildUpProgress", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		UnderwaterVentBuildUpProgress.resolveStringCallback = delegate(string str, object data)
		{
			UnderwaterVent.Instance instance = (UnderwaterVent.Instance)data;
			str = str.Replace("{PERCENTAGE}", GameUtil.GetFormattedPercent(instance.BuildUpProgress * 100f));
			return str;
		};
		OreTemp = CreateStatusItem("OreTemp", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		OreTemp.resolveStringCallback = delegate(string str, object data)
		{
			GameObject gameObject = (GameObject)data;
			str = str.Replace("{Temp}", GameUtil.GetFormattedTemperature(gameObject.GetComponent<PrimaryElement>().Temperature));
			return str;
		};
		ElementalState = CreateStatusItem("ElementalState", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ElementalState.resolveStringCallback = delegate(string str, object data)
		{
			Element element = ((Func<Element>)data)();
			str = str.Replace("{State}", element.GetStateString());
			return str;
		};
		ElementalCategory = CreateStatusItem("ElementalCategory", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ElementalCategory.resolveStringCallback = delegate(string str, object data)
		{
			Element element = ((Func<Element>)data)();
			str = str.Replace("{Category}", element.GetMaterialCategoryTag().ProperName());
			return str;
		};
		ElementalTemperature = CreateStatusItem("ElementalTemperature", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ElementalTemperature.resolveStringCallback = delegate(string str, object data)
		{
			CellSelectionObject cellSelectionObject = (CellSelectionObject)data;
			str = str.Replace("{Temp}", GameUtil.GetFormattedTemperature(cellSelectionObject.temperature));
			return str;
		};
		ElementalMass = CreateStatusItem("ElementalMass", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ElementalMass.resolveStringCallback = delegate(string str, object data)
		{
			CellSelectionObject cellSelectionObject = (CellSelectionObject)data;
			str = str.Replace("{Mass}", GameUtil.GetFormattedMass(cellSelectionObject.Mass));
			return str;
		};
		ElementalDisease = CreateStatusItem("ElementalDisease", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ElementalDisease.resolveStringCallback = delegate(string str, object data)
		{
			CellSelectionObject cellSelectionObject = (CellSelectionObject)data;
			str = str.Replace("{Disease}", GameUtil.GetFormattedDisease(cellSelectionObject.diseaseIdx, cellSelectionObject.diseaseCount));
			return str;
		};
		ElementalDisease.resolveTooltipCallback = delegate(string str, object data)
		{
			CellSelectionObject cellSelectionObject = (CellSelectionObject)data;
			str = str.Replace("{Disease}", GameUtil.GetFormattedDisease(cellSelectionObject.diseaseIdx, cellSelectionObject.diseaseCount, color: true));
			return str;
		};
		GrowingBranches = new StatusItem("GrowingBranches", "MISC", "", StatusItem.IconType.Info, NotificationType.Good, allow_multiples: false, OverlayModes.None.ID);
		TreeFilterableTags = CreateStatusItem("TreeFilterableTags", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		TreeFilterableTags.resolveStringCallback = delegate(string str, object data)
		{
			TreeFilterable treeFilterable = (TreeFilterable)data;
			str = str.Replace("{Tags}", treeFilterable.GetTagsAsStatus());
			return str;
		};
		SublimationEmitting = CreateStatusItem("SublimationEmitting", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		SublimationEmitting.resolveStringCallback = delegate(string str, object data)
		{
			CellSelectionObject cellSelectionObject = (CellSelectionObject)data;
			if (cellSelectionObject.element.sublimateId == (SimHashes)0)
			{
				return str;
			}
			str = str.Replace("{Element}", GameUtil.GetElementNameByElementHash(cellSelectionObject.element.sublimateId));
			str = str.Replace("{FlowRate}", GameUtil.GetFormattedMass(cellSelectionObject.FlowRate, GameUtil.TimeSlice.PerSecond));
			return str;
		};
		SublimationEmitting.resolveTooltipCallback = SublimationEmitting.resolveStringCallback;
		SublimationBlocked = CreateStatusItem("SublimationBlocked", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		SublimationBlocked.resolveStringCallback = delegate(string str, object data)
		{
			CellSelectionObject cellSelectionObject = (CellSelectionObject)data;
			if (cellSelectionObject.element.sublimateId == (SimHashes)0)
			{
				return str;
			}
			str = str.Replace("{Element}", cellSelectionObject.element.name);
			str = str.Replace("{SubElement}", GameUtil.GetElementNameByElementHash(cellSelectionObject.element.sublimateId));
			return str;
		};
		SublimationBlocked.resolveTooltipCallback = SublimationBlocked.resolveStringCallback;
		SublimationOverpressure = CreateStatusItem("SublimationOverpressure", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		SublimationOverpressure.resolveTooltipCallback = delegate(string str, object data)
		{
			CellSelectionObject cellSelectionObject = (CellSelectionObject)data;
			if (cellSelectionObject.element.sublimateId == (SimHashes)0)
			{
				return str;
			}
			str = str.Replace("{Element}", cellSelectionObject.element.name);
			str = str.Replace("{SubElement}", GameUtil.GetElementNameByElementHash(cellSelectionObject.element.sublimateId));
			return str;
		};
		Space = CreateStatusItem("Space", "MISC", "", StatusItem.IconType.Exclamation, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		BuriedItem = CreateStatusItem("BuriedItem", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		BackwallMass = CreateStatusItem("BackwallMass", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		BackwallMass.resolveStringCallback = delegate(string str, object data)
		{
			BackwallSelectionObject backwallSelectionObject = (BackwallSelectionObject)data;
			str = str.Replace("{Mass}", GameUtil.GetFormattedMass(backwallSelectionObject.Mass));
			return str;
		};
		BackwallTemperature = CreateStatusItem("BackwallTemperature", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		BackwallTemperature.resolveStringCallback = delegate(string str, object data)
		{
			BackwallSelectionObject backwallSelectionObject = (BackwallSelectionObject)data;
			str = str.Replace("{Temp}", GameUtil.GetFormattedTemperature(backwallSelectionObject.temperature));
			return str;
		};
		BubbleContents = CreateStatusItem("BubbleContents", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: true, OverlayModes.None.ID);
		BubbleContents.resolveStringCallback = delegate(string str, object data)
		{
			CellSelectionObject obj = (CellSelectionObject)data;
			string text = "";
			foreach (BubbleManager.CellBubbleInfo bubbleInfo in obj.bubbleInfos)
			{
				Element element = ElementLoader.FindElementByHash(bubbleInfo.element);
				if (text.Length > 0)
				{
					text += "\n";
				}
				text = string.Concat(text, element.name, " ", UI.TOOLS.GENERIC.BUBBLE_LABEL, ": ", GameUtil.GetFormattedMass(bubbleInfo.totalMass));
			}
			return text;
		};
		SpoutOverPressure = CreateStatusItem("SpoutOverPressure", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		SpoutOverPressure.resolveStringCallback = delegate(string str, object data)
		{
			Geyser.StatesInstance statesInstance = (Geyser.StatesInstance)data;
			Studyable component = statesInstance.GetComponent<Studyable>();
			str = ((statesInstance == null || !(component != null) || !component.Studied) ? str.Replace("{StudiedDetails}", "") : str.Replace("{StudiedDetails}", MISC.STATUSITEMS.SPOUTOVERPRESSURE.STUDIED.text.Replace("{Time}", GameUtil.GetFormattedCycles(statesInstance.master.RemainingEruptTime()))));
			return str;
		};
		SpoutEmitting = CreateStatusItem("SpoutEmitting", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		SpoutEmitting.resolveStringCallback = delegate(string str, object data)
		{
			Geyser.StatesInstance statesInstance = (Geyser.StatesInstance)data;
			Studyable component = statesInstance.GetComponent<Studyable>();
			str = ((statesInstance == null || !(component != null) || !component.Studied) ? str.Replace("{StudiedDetails}", "") : str.Replace("{StudiedDetails}", MISC.STATUSITEMS.SPOUTEMITTING.STUDIED.text.Replace("{Time}", GameUtil.GetFormattedCycles(statesInstance.master.RemainingEruptTime()))));
			return str;
		};
		SpoutPressureBuilding = CreateStatusItem("SpoutPressureBuilding", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		SpoutPressureBuilding.resolveStringCallback = delegate(string str, object data)
		{
			Geyser.StatesInstance statesInstance = (Geyser.StatesInstance)data;
			Studyable component = statesInstance.GetComponent<Studyable>();
			str = ((statesInstance == null || !(component != null) || !component.Studied) ? str.Replace("{StudiedDetails}", "") : str.Replace("{StudiedDetails}", MISC.STATUSITEMS.SPOUTPRESSUREBUILDING.STUDIED.text.Replace("{Time}", GameUtil.GetFormattedCycles(statesInstance.master.RemainingNonEruptTime()))));
			return str;
		};
		SpoutIdle = CreateStatusItem("SpoutIdle", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		SpoutIdle.resolveStringCallback = delegate(string str, object data)
		{
			Geyser.StatesInstance statesInstance = (Geyser.StatesInstance)data;
			Studyable component = statesInstance.GetComponent<Studyable>();
			str = ((statesInstance == null || !(component != null) || !component.Studied) ? str.Replace("{StudiedDetails}", "") : str.Replace("{StudiedDetails}", MISC.STATUSITEMS.SPOUTIDLE.STUDIED.text.Replace("{Time}", GameUtil.GetFormattedCycles(statesInstance.master.RemainingNonEruptTime()))));
			return str;
		};
		SpoutDormant = CreateStatusItem("SpoutDormant", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		SpicedFood = CreateStatusItem("SpicedFood", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		SpicedFood.resolveTooltipCallback = delegate(string baseString, object data)
		{
			string text = baseString;
			string text2 = "\n    • ";
			foreach (SpiceInstance item in (List<SpiceInstance>)data)
			{
				Tag id = item.Id;
				string text3 = "STRINGS.ITEMS.SPICES." + id.Name.ToUpper() + ".NAME";
				Strings.TryGet(text3, out var result);
				string text4 = ((result == null) ? ("MISSING " + text3) : result.String);
				text = text + text2 + text4;
				string linePrefix = "\n        • ";
				if (item.StatBonus != null)
				{
					text += Effect.CreateTooltip(item.StatBonus, showDuration: false, linePrefix, showHeader: false);
				}
			}
			return text;
		};
		RehydratedFood = CreateStatusItem("RehydratedFood", "MISC", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		OrderAttack = CreateStatusItem("OrderAttack", "MISC", "status_item_attack", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		OrderCapture = CreateStatusItem("OrderCapture", "MISC", "status_item_capture", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		PendingHarvest = CreateStatusItem("PendingHarvest", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		NotMarkedForHarvest = CreateStatusItem("NotMarkedForHarvest", "MISC", "status_item_building_disabled", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		NotMarkedForHarvest.conditionalOverlayCallback = (HashedString viewMode, object o) => !(viewMode != OverlayModes.None.ID);
		PendingUproot = CreateStatusItem("PendingUproot", "MISC", "status_item_pending_uproot", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		PickupableUnreachable = CreateStatusItem("PickupableUnreachable", "MISC", "", StatusItem.IconType.Exclamation, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Prioritized = CreateStatusItem("Prioritized", "MISC", "status_item_prioritized", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Using = CreateStatusItem("Using", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Using.resolveStringCallback = delegate(string str, object data)
		{
			Workable workable = (Workable)data;
			if (workable != null)
			{
				KSelectable component = workable.GetComponent<KSelectable>();
				if (component != null)
				{
					str = str.Replace("{Target}", component.GetName());
				}
			}
			return str;
		};
		Operating = CreateStatusItem("Operating", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Cleaning = CreateStatusItem("Cleaning", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		RegionIsBlocked = CreateStatusItem("RegionIsBlocked", "MISC", "status_item_solids_blocking", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		AwaitingStudy = CreateStatusItem("AwaitingStudy", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Studied = CreateStatusItem("Studied", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		HighEnergyParticleCount = CreateStatusItem("HighEnergyParticleCount", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		HighEnergyParticleCount.resolveStringCallback = delegate(string str, object data)
		{
			GameObject gameObject = (GameObject)data;
			str = GameUtil.GetFormattedHighEnergyParticles(gameObject.IsNullOrDestroyed() ? 0f : gameObject.GetComponent<HighEnergyParticle>().payload);
			return str;
		};
		Durability = CreateStatusItem("Durability", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		Durability.resolveStringCallback = delegate(string str, object data)
		{
			Durability component = ((GameObject)data).GetComponent<Durability>();
			str = str.Replace("{durability}", GameUtil.GetFormattedPercent(component.GetDurability() * 100f));
			return str;
		};
		BionicExplorerBooster = CreateStatusItem("BionicExplorerBooster", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		BionicExplorerBooster.resolveStringCallback = delegate(string str, object data)
		{
			BionicUpgrade_ExplorerBooster.Instance instance = (BionicUpgrade_ExplorerBooster.Instance)data;
			str = string.Format(str, GameUtil.GetFormattedPercent(instance.Progress * 100f));
			return str;
		};
		BionicExplorerBoosterReady = CreateStatusItem("BionicExplorerBoosterReady", "MISC", "", StatusItem.IconType.Info, NotificationType.Good, allow_multiples: false, OverlayModes.None.ID);
		UnassignedBionicBooster = CreateStatusItem("UnassignedBionicBooster", "MISC", "status_item_pending_upgrade", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ElectrobankLifetimeRemaining = CreateStatusItem("ElectrobankLifetimeRemaining", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ElectrobankLifetimeRemaining.resolveStringCallback = delegate(string str, object data)
		{
			SelfChargingElectrobank selfChargingElectrobank = (SelfChargingElectrobank)data;
			str = ((!(selfChargingElectrobank != null)) ? str.Replace("{0}", GameUtil.GetFormattedCycles(0f)) : str.Replace("{0}", GameUtil.GetFormattedCycles(selfChargingElectrobank.LifetimeRemaining)));
			return str;
		};
		ElectrobankSelfCharging = CreateStatusItem("ElectrobankSelfCharging", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ElectrobankSelfCharging.resolveStringCallback = delegate(string str, object data)
		{
			str = str.Replace("{0}", GameUtil.GetFormattedWattage((float)data));
			return str;
		};
		StoredItemDurability = CreateStatusItem("StoredItemDurability", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		StoredItemDurability.resolveStringCallback = delegate(string str, object data)
		{
			Durability component = ((GameObject)data).GetComponent<Durability>();
			float percent = ((component != null) ? (component.GetDurability() * 100f) : 100f);
			str = str.Replace("{durability}", GameUtil.GetFormattedPercent(percent));
			return str;
		};
		ClusterMeteorRemainingTravelTime = CreateStatusItem("ClusterMeteorRemainingTravelTime", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ClusterMeteorRemainingTravelTime.resolveStringCallback = delegate(string str, object data)
		{
			float seconds = ((ClusterMapMeteorShower.Instance)data).ArrivalTime - GameUtil.GetCurrentTimeInCycles() * 600f;
			str = str.Replace("{time}", GameUtil.GetFormattedCycles(seconds));
			return str;
		};
		ArtifactEntombed = CreateStatusItem("ArtifactEntombed", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		TearOpen = CreateStatusItem("TearOpen", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		TearClosed = CreateStatusItem("TearClosed", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ImpactorStatus = CreateStatusItem("LargeImpactorStatus", "MISC", "", StatusItem.IconType.Exclamation, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		ImpactorStatus.resolveStringCallback = delegate(string str, object data)
		{
			ClusterTraveler clusterTraveler = (ClusterTraveler)data;
			float seconds = 0f;
			if (data != null)
			{
				seconds = clusterTraveler.TravelETA(clusterTraveler.Destination);
			}
			return string.Format(str, GameUtil.GetFormattedCycles(seconds));
		};
		ImpactorStatus.resolveTooltipCallback = ImpactorStatus.resolveStringCallback;
		ImpactorHealth = CreateStatusItem("LargeImpactorHealth", "MISC", "", StatusItem.IconType.Exclamation, NotificationType.Bad, allow_multiples: false, OverlayModes.None.ID);
		ImpactorHealth.resolveStringCallback = delegate(string str, object data)
		{
			LargeImpactorStatus.Instance instance = (LargeImpactorStatus.Instance)data;
			int num = 0;
			int num2 = 0;
			if (data != null)
			{
				num = instance.Health;
				num2 = instance.def.MAX_HEALTH;
			}
			return string.Format(str, num, num2);
		};
		LongRangeMissileTTI = CreateStatusItem("LongRangeMissileTTI", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		LongRangeMissileTTI.resolveStringCallback = delegate(string str, object data)
		{
			ClusterMapLongRangeMissile.StatesInstance statesInstance = (ClusterMapLongRangeMissile.StatesInstance)data;
			string arg = "";
			float seconds = 0f;
			if (statesInstance != null)
			{
				GameObject gameObject = statesInstance.sm.targetObject.Get(statesInstance);
				if (gameObject != null)
				{
					arg = gameObject.GetProperName();
				}
				seconds = statesInstance.InterceptETA();
			}
			return string.Format(str, arg, GameUtil.GetFormattedCycles(seconds));
		};
		LongRangeMissileTTI.resolveTooltipCallback = LongRangeMissileTTI.resolveStringCallback;
		MarkedForMove = CreateStatusItem("MarkedForMove", "MISC", "status_item_manually_controlled", StatusItem.IconType.Custom, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		MoveStorageUnreachable = CreateStatusItem("MoveStorageUnreachable", "MISC", "status_item_manually_controlled", StatusItem.IconType.Custom, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
		ClusterMapHarvestableResource = CreateStatusItem("ClusterMapHarvestableResource", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, OverlayModes.None.ID);
		ClusterMapHarvestableResource.showInHoverCardOnly = true;
		ClusterMapHarvestableResource.resolveStringCallback = delegate(string str, object data)
		{
			List<StarmapHexCellInventory.SerializedItem> list = data as List<StarmapHexCellInventory.SerializedItem>;
			string text = "";
			for (int i = 0; i < list.Count; i++)
			{
				StarmapHexCellInventory.SerializedItem serializedItem = list[i];
				text = text + serializedItem.ID.ProperName() + ": " + (serializedItem.IsEntity ? GameUtil.GetFormattedUnits(serializedItem.Mass) : GameUtil.GetFormattedMass(serializedItem.Mass));
				if (i < list.Count - 1)
				{
					text += "\n";
				}
			}
			return GameUtil.SafeStringFormat(str, text);
		};
		ClusterMapHarvestableResource.resolveTooltipCallback = ClusterMapHarvestableResource.resolveStringCallback;
		MinnowPOIDehydratedStatus = CreateStatusItem("MinnowPOIDehydratedStatus", "MISC", "", StatusItem.IconType.Info, NotificationType.BadMinor, allow_multiples: false, OverlayModes.None.ID);
	}
}
