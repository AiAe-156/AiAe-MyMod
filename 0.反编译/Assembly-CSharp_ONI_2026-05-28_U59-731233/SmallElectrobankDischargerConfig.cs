using STRINGS;
using TUNING;
using UnityEngine;

public class SmallElectrobankDischargerConfig : IBuildingConfig
{
	public const string ID = "SmallElectrobankDischarger";

	public const float DISCHARGE_RATE = 60f;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("SmallElectrobankDischarger", 1, 1, "electrobank_discharger_small_kanim", 30, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.ALL_METALS, 2400f, BuildLocationRule.OnFoundationRotatable, noise: NOISE_POLLUTION.NOISY.TIER1, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		buildingDef.PermittedRotations = PermittedRotations.R360;
		buildingDef.GeneratorWattageRating = 60f;
		buildingDef.GeneratorBaseCapacity = 60f;
		buildingDef.ExhaustKilowattsWhenActive = 0.125f;
		buildingDef.SelfHeatKilowattsWhenActive = 0.5f;
		buildingDef.RequiresPowerOutput = true;
		buildingDef.PowerOutputOffset = new CellOffset(0, 0);
		buildingDef.ViewMode = OverlayModes.Power.ID;
		buildingDef.AudioCategory = "HollowMetal";
		buildingDef.AudioSize = "small";
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		buildingDef.AddSearchTerms(SEARCH_TERMS.BATTERY);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Prioritizable.AddRef(go);
		go.AddOrGet<LoopingSounds>();
		Storage storage = go.AddOrGet<Storage>();
		storage.showInUI = true;
		storage.capacityKg = 20f;
		storage.storageFilters = STORAGEFILTERS.POWER_BANKS;
		TreeFilterable treeFilterable = go.AddOrGet<TreeFilterable>();
		treeFilterable.allResourceFilterLabelString = UI.UISIDESCREENS.TREEFILTERABLESIDESCREEN.ALLBUTTON;
		ElectrobankDischarger electrobankDischarger = go.AddOrGet<ElectrobankDischarger>();
		electrobankDischarger.wattageRating = 60f;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
		SymbolOverrideControllerUtil.AddToPrefab(go);
	}
}
