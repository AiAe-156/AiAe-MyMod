using STRINGS;
using TUNING;
using UnityEngine;

public class PlanterBoxConfig : IBuildingConfig
{
	public const string ID = "PlanterBox";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("PlanterBox", 1, 1, "planterbox_kanim", 10, 3f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.FARMABLE, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		obj.ForegroundLayer = Grid.SceneLayer.BuildingBack;
		obj.Overheatable = false;
		obj.Floodable = false;
		obj.AudioCategory = "Glass";
		obj.AudioSize = "large";
		obj.AddSearchTerms(SEARCH_TERMS.FOOD);
		obj.AddSearchTerms(SEARCH_TERMS.FARM);
		return obj;
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
		go.AddOrGet<CopyBuildingSettings>().copyGroupTag = GameTags.Farm;
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
		instance.AddOrGet<PlantablePlot>().AddAdditionalCriteria(FarmTileConfig.ForbiddenTags);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
