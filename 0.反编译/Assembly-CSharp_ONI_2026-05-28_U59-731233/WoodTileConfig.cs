using STRINGS;
using TUNING;
using UnityEngine;

public class WoodTileConfig : IBuildingConfig
{
	public const string ID = "WoodTile";

	public static readonly int BlockTileConnectorID = Hash.SDBMLower("tiles_wood_tops");

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("WoodTile", 1, 1, "floor_wood_kanim", 100, 3f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.WOODS, 1600f, BuildLocationRule.Tile, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER1);
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
		buildingDef.BlockTileAtlas = Assets.GetTextureAtlas("tiles_wood");
		buildingDef.BlockTilePlaceAtlas = Assets.GetTextureAtlas("tiles_wood_place");
		buildingDef.BlockTileMaterial = Assets.GetMaterial("tiles_solid");
		buildingDef.DecorBlockTileInfo = Assets.GetBlockTileDecorInfo("tiles_wood_decor_info");
		buildingDef.DecorPlaceBlockTileInfo = Assets.GetBlockTileDecorInfo("tiles_wood_decor_place_info");
		buildingDef.POIUnlockable = true;
		buildingDef.AddSearchTerms(SEARCH_TERMS.TILE);
		buildingDef.AddSearchTerms(SEARCH_TERMS.LUMBER);
		buildingDef.AddSearchTerms(SEARCH_TERMS.MORALE);
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
		simCellOccupier.movementSpeedMultiplier = DUPLICANTSTATS.MOVEMENT_MODIFIERS.BONUS_2;
		simCellOccupier.notifyOnMelt = true;
		go.AddOrGet<TileTemperature>();
		KAnimGridTileVisualizer kAnimGridTileVisualizer = go.AddOrGet<KAnimGridTileVisualizer>();
		kAnimGridTileVisualizer.blockTileConnectorID = BlockTileConnectorID;
		BuildingHP buildingHP = go.AddOrGet<BuildingHP>();
		buildingHP.destroyOnDamaged = true;
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

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		base.DoPostConfigureUnderConstruction(go);
		go.AddOrGet<KAnimGridTileVisualizer>();
	}
}
