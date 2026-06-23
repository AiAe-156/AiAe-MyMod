using STRINGS;
using TUNING;
using UnityEngine;

public class ApothecaryConfig : IBuildingConfig
{
	public const string ID = "Apothecary";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("Apothecary", 2, 3, "apothecary_kanim", 30, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.NONE);
		obj.RequiresPowerInput = false;
		obj.EnergyConsumptionWhenActive = 0f;
		obj.ExhaustKilowattsWhenActive = 0.125f;
		obj.SelfHeatKilowattsWhenActive = 0.5f;
		obj.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(default(CellOffset));
		obj.AudioCategory = "Glass";
		obj.AudioSize = "large";
		obj.RequiredSkillPerkID = Db.Get().SkillPerks.CanCompound.Id;
		obj.AddSearchTerms(SEARCH_TERMS.MEDICINE);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Prioritizable.AddRef(go);
		go.AddOrGet<DropAllWorkable>();
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		Apothecary apothecary = go.AddOrGet<Apothecary>();
		BuildingTemplates.CreateComplexFabricatorStorage(go, apothecary);
		apothecary.inStorage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
		go.AddOrGet<ComplexFabricatorWorkable>();
		go.AddOrGet<FabricatorIngredientStatusManager>();
		go.AddOrGet<CopyBuildingSettings>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGetDef<PoweredActiveStoppableController.Def>();
		go.AddOrGet<LogicOperationalController>();
	}
}
