using STRINGS;
using TUNING;
using UnityEngine;

public class PlanterBoxConfig : IBuildingConfig
{
	public const string ID = "PlanterBox";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("PlanterBox", 1, 1, "planterbox_kanim", 10, 3f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.FARMABLE, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		buildingDef.ForegroundLayer = Grid.SceneLayer.BuildingBack;
		buildingDef.Overheatable = false;
		buildingDef.Floodable = false;
		buildingDef.AudioCategory = "Glass";
		buildingDef.AudioSize = "large";
		buildingDef.AddSearchTerms(SEARCH_TERMS.FOOD);
		buildingDef.AddSearchTerms(SEARCH_TERMS.FARM);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(GameTags.CodexCategories.FarmBuilding);
		Storage storage = go.AddOrGet<Storage>();
		PlantablePlot plantablePlot = go.AddOrGet<PlantablePlot>();
		plantablePlot.IsOffGround = true;
		plantablePlot.tagOnPlanted = GameTags.PlantedOnFloorVessel;
		plantablePlot.AddDepositTag(GameTags.CropSeed);
		plantablePlot.AddAdditionalCriteria(FarmTileConfig.ForbiddenTags);
		plantablePlot.SetFertilizationFlags(fertilizer: true, liquid_piping: false);
		CopyBuildingSettings copyBuildingSettings = go.AddOrGet<CopyBuildingSettings>();
		copyBuildingSettings.copyGroupTag = GameTags.Farm;
		BuildingTemplates.CreateDefaultStorage(go);
		storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		go.AddOrGet<DropAllWorkable>();
		go.AddOrGet<PlanterBox>();
		go.AddOrGet<AnimTileable>();
		Prioritizable.AddRef(go);
		component.prefabInitFn += OnPrefabInit;
	}

	private void OnPrefabInit(GameObject instance)
	{
		PlantablePlot plantablePlot = instance.AddOrGet<PlantablePlot>();
		plantablePlot.AddAdditionalCriteria(FarmTileConfig.ForbiddenTags);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
