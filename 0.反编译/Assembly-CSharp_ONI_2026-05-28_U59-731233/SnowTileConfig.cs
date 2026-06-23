using STRINGS;
using TUNING;
using UnityEngine;

public class SnowTileConfig : IBuildingConfig
{
	public const string ID = "SnowTile";

	public static readonly int BlockTileConnectorID = Hash.SDBMLower("tiles_snow_tops");

	private SimHashes CONSTRUCTION_ELEMENT = SimHashes.Snow;

	private SimHashes STABLE_SNOW_ELEMENT = SimHashes.StableSnow;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("SnowTile", 1, 1, "floor_snow_kanim", 100, 3f, new float[1] { 30f }, new string[1] { CONSTRUCTION_ELEMENT.ToString() }, 1600f, BuildLocationRule.Tile, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.NONE);
		BuildingTemplates.CreateFoundationTileDef(buildingDef);
		buildingDef.Floodable = false;
		buildingDef.Overheatable = false;
		buildingDef.Entombable = false;
		buildingDef.UseStructureTemperature = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "small";
		buildingDef.BaseTimeUntilRepair = -1f;
		buildingDef.SceneLayer = Grid.SceneLayer.TileMain;
		buildingDef.isKAnimTile = true;
		buildingDef.BlockTileAtlas = Assets.GetTextureAtlas("tiles_snow");
		buildingDef.BlockTilePlaceAtlas = Assets.GetTextureAtlas("tiles_snow_place");
		buildingDef.BlockTileMaterial = Assets.GetMaterial("tiles_solid");
		buildingDef.DecorBlockTileInfo = Assets.GetBlockTileDecorInfo("tiles_snow_decor_info");
		buildingDef.DecorPlaceBlockTileInfo = Assets.GetBlockTileDecorInfo("tiles_snow_decor_place_info");
		buildingDef.Temperature = 263.15f;
		buildingDef.AddSearchTerms(SEARCH_TERMS.TILE);
		buildingDef.DragBuild = true;
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		SimCellOccupier simCellOccupier = go.AddOrGet<SimCellOccupier>();
		simCellOccupier.doReplaceElement = true;
		simCellOccupier.strengthMultiplier = 1.5f;
		simCellOccupier.notifyOnMelt = true;
		go.AddOrGet<TileTemperature>();
		KAnimGridTileVisualizer kAnimGridTileVisualizer = go.AddOrGet<KAnimGridTileVisualizer>();
		kAnimGridTileVisualizer.blockTileConnectorID = BlockTileConnectorID;
		BuildingHP buildingHP = go.AddOrGet<BuildingHP>();
		buildingHP.destroyOnDamaged = true;
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.prefabInitFn += BuildingComplete_OnInit;
		component.prefabSpawnFn += BuildingComplete_OnSpawn;
	}

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC2;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		GeneratedBuildings.RemoveLoopingSounds(go);
		go.GetComponent<KPrefabID>().AddTag(GameTags.FloorTiles);
	}

	private void BuildingComplete_OnInit(GameObject instance)
	{
		PrimaryElement component = instance.GetComponent<PrimaryElement>();
		component.SetElement(STABLE_SNOW_ELEMENT);
		Element element = component.Element;
		Deconstructable component2 = instance.GetComponent<Deconstructable>();
		if (component2 != null)
		{
			component2.constructionElements = new Tag[1] { CONSTRUCTION_ELEMENT.CreateTag() };
		}
	}

	private void BuildingComplete_OnSpawn(GameObject instance)
	{
		PrimaryElement component = instance.GetComponent<PrimaryElement>();
		component.SetElement(STABLE_SNOW_ELEMENT);
		Deconstructable component2 = instance.GetComponent<Deconstructable>();
		if (component2 != null)
		{
			component2.constructionElements = new Tag[1] { CONSTRUCTION_ELEMENT.CreateTag() };
		}
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		base.DoPostConfigureUnderConstruction(go);
		go.AddOrGet<KAnimGridTileVisualizer>();
	}
}
