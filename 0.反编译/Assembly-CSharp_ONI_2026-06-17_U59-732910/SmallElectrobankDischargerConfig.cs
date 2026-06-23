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
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("SmallElectrobankDischarger", 1, 1, "electrobank_discharger_small_kanim", 30, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.ALL_METALS, 2400f, BuildLocationRule.OnFoundationRotatable, noise: NOISE_POLLUTION.NOISY.TIER1, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		obj.PermittedRotations = PermittedRotations.R360;
		obj.GeneratorWattageRating = 60f;
		obj.GeneratorBaseCapacity = 60f;
		obj.ExhaustKilowattsWhenActive = 0.125f;
		obj.SelfHeatKilowattsWhenActive = 0.5f;
		obj.RequiresPowerOutput = true;
		obj.PowerOutputOffset = new CellOffset(0, 0);
		obj.ViewMode = OverlayModes.Power.ID;
		obj.AudioCategory = "HollowMetal";
		obj.AudioSize = "small";
		obj.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		obj.AddSearchTerms(SEARCH_TERMS.BATTERY);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Prioritizable.AddRef(go);
		go.AddOrGet<LoopingSounds>();
		Storage storage = go.AddOrGet<Storage>();
		storage.showInUI = true;
		storage.capacityKg = 20f;
		storage.storageFilters = STORAGEFILTERS.POWER_BANKS;
		go.AddOrGet<TreeFilterable>().allResourceFilterLabelString = UI.UISIDESCREENS.TREEFILTERABLESIDESCREEN.ALLBUTTON;
		go.AddOrGet<ElectrobankDischarger>().wattageRating = 60f;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
		SymbolOverrideControllerUtil.AddToPrefab(go);
	}
}
