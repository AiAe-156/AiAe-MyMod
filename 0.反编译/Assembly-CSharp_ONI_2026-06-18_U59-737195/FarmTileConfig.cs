using STRINGS;
using TUNING;
using UnityEngine;

public class FarmTileConfig : IBuildingConfig
{
	public const string ID = "FarmTile";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("FarmTile", 1, 1, "farmtilerotating_kanim", 100, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.FARMABLE, 1600f, BuildLocationRule.Tile, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.NONE);
		BuildingTemplates.CreateFoundationTileDef(obj);
		obj.Floodable = false;
		obj.Entombable = false;
		obj.Overheatable = false;
		obj.ForegroundLayer = Grid.SceneLayer.BuildingBack;
		obj.AudioCategory = "HollowMetal";
		obj.AudioSize = "small";
		obj.BaseTimeUntilRepair = -1f;
		obj.SceneLayer = Grid.SceneLayer.TileMain;
		obj.ConstructionOffsetFilter = BuildingDef.ConstructionOffsetFilter_OneDown;
		obj.PermittedRotations = PermittedRotations.FlipV;
		obj.DragBuild = true;
		obj.AddSearchTerms(SEARCH_TERMS.FOOD);
		obj.AddSearchTerms(SEARCH_TERMS.FARM);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(GameTags.CodexCategories.FarmBuilding);
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		SimCellOccupier simCellOccupier = go.AddOrGet<SimCellOccupier>();
		simCellOccupier.doReplaceElement = true;
		simCellOccupier.notifyOnMelt = true;
		go.AddOrGet<TileTemperature>();
		BuildingTemplates.CreateDefaultStorage(go).SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		PlantablePlot plantablePlot = go.AddOrGet<PlantablePlot>();
		plantablePlot.occupyingObjectRelativePosition = new Vector3(0f, 1f, 0f);
		plantablePlot.AddDepositTag(GameTags.CropSeed);
		plantablePlot.AddDepositTag(GameTags.WaterSeed);
		plantablePlot.AddAdditionalCriteria(ForbiddenTags);
		plantablePlot.SetFertilizationFlags(fertilizer: true, liquid_piping: false);
		go.AddOrGet<CopyBuildingSettings>().copyGroupTag = GameTags.Farm;
		go.AddOrGet<AnimTileable>();
		Prioritizable.AddRef(go);
		component.prefabInitFn += OnPrefabInit;
	}

	private void OnPrefabInit(GameObject instance)
	{
		instance.AddOrGet<PlantablePlot>().AddAdditionalCriteria(ForbiddenTags);
	}

	public static bool ForbiddenTags(GameObject objInQuestion)
	{
		KPrefabID component = objInQuestion.GetComponent<KPrefabID>();
		return !component.HasTag(GameTags.LargeSeed) && !component.HasTag(GameTags.BackwallSeed);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KBatchedAnimController>().initialBlendParameters = 4;
		GeneratedBuildings.RemoveLoopingSounds(go);
		go.GetComponent<KPrefabID>().AddTag(GameTags.FarmTiles);
		SetUpFarmPlotTags(go);
	}

	public static void SetUpFarmPlotTags(GameObject go)
	{
		go.GetComponent<KPrefabID>().prefabSpawnFn += delegate(GameObject inst)
		{
			Rotatable component = inst.GetComponent<Rotatable>();
			PlantablePlot component2 = inst.GetComponent<PlantablePlot>();
			switch (component.GetOrientation())
			{
			case Orientation.Neutral:
			case Orientation.FlipH:
				component2.SetReceptacleDirection(SingleEntityReceptacle.ReceptacleDirection.Top);
				break;
			case Orientation.R180:
			case Orientation.FlipV:
				component2.SetReceptacleDirection(SingleEntityReceptacle.ReceptacleDirection.Bottom);
				break;
			case Orientation.R90:
			case Orientation.R270:
				component2.SetReceptacleDirection(SingleEntityReceptacle.ReceptacleDirection.Side);
				break;
			case Orientation.NumRotations:
				break;
			}
		};
	}
}
