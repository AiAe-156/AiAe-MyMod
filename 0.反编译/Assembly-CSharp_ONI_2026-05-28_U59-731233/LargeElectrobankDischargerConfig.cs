using STRINGS;
using TUNING;
using UnityEngine;

public class LargeElectrobankDischargerConfig : IBuildingConfig
{
	public const string ID = "LargeElectrobankDischarger";

	public const float DISCHARGE_RATE = 480f;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("LargeElectrobankDischarger", 2, 2, "electrobank_discharger_large_kanim", 30, 60f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.REFINED_METALS, 2400f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER1, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.GeneratorWattageRating = 480f;
		buildingDef.GeneratorBaseCapacity = 480f;
		buildingDef.ExhaustKilowattsWhenActive = 0.25f;
		buildingDef.SelfHeatKilowattsWhenActive = 1f;
		buildingDef.RequiresPowerOutput = true;
		buildingDef.PowerOutputOffset = new CellOffset(0, 0);
		buildingDef.ViewMode = OverlayModes.Power.ID;
		buildingDef.AudioCategory = "HollowMetal";
		buildingDef.AudioSize = "large";
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		buildingDef.AddSearchTerms(SEARCH_TERMS.BATTERY);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
		Prioritizable.AddRef(go);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		Storage storage = go.AddOrGet<Storage>();
		storage.showInUI = true;
		storage.capacityKg = 20f;
		storage.storageFilters = STORAGEFILTERS.POWER_BANKS;
		TreeFilterable treeFilterable = go.AddOrGet<TreeFilterable>();
		treeFilterable.allResourceFilterLabelString = UI.UISIDESCREENS.TREEFILTERABLESIDESCREEN.ALLBUTTON;
		ElectrobankDischarger electrobankDischarger = go.AddOrGet<ElectrobankDischarger>();
		electrobankDischarger.wattageRating = 480f;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
		SymbolOverrideControllerUtil.AddToPrefab(go);
	}
}
