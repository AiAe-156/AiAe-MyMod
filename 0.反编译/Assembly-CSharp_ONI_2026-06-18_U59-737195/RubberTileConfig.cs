using STRINGS;
using TUNING;
using UnityEngine;

public class RubberTileConfig : IBuildingConfig
{
	public const string ID = "RubberTile";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("RubberTile", 1, 1, "floor_rubber_kanim", 30, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, new string[1] { SimHashes.Rubber.CreateTag().ToString() }, 1600f, BuildLocationRule.Tile, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER0);
		BuildingTemplates.CreateFoundationTileDef(obj);
		obj.ThermalConductivity = 0.02f;
		obj.Floodable = false;
		obj.Overheatable = false;
		obj.Entombable = false;
		obj.UseStructureTemperature = false;
		obj.AudioCategory = "Metal";
		obj.AudioSize = "small";
		obj.BaseTimeUntilRepair = -1f;
		obj.SceneLayer = Grid.SceneLayer.TileMain;
		obj.isKAnimTile = true;
		obj.BlockTileAtlas = Assets.GetTextureAtlas("tiles_rubber");
		obj.BlockTilePlaceAtlas = Assets.GetTextureAtlas("tiles_rubber_place");
		obj.BlockTileMaterial = Assets.GetMaterial("tiles_solid");
		obj.DecorBlockTileInfo = Assets.GetBlockTileDecorInfo("tiles_rubber_decor_info");
		obj.DecorPlaceBlockTileInfo = Assets.GetBlockTileDecorInfo("tiles_rubber_decor_place_info");
		obj.AddSearchTerms(SEARCH_TERMS.TILE);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		SimCellOccupier simCellOccupier = go.AddOrGet<SimCellOccupier>();
		simCellOccupier.strengthMultiplier = 5f;
		simCellOccupier.doReplaceElement = true;
		simCellOccupier.notifyOnMelt = true;
		simCellOccupier.movementSpeedMultiplier = DUPLICANTSTATS.MOVEMENT_MODIFIERS.BONUS_2;
		go.AddOrGet<Insulator>();
		go.AddOrGet<TileTemperature>();
		go.AddOrGet<KAnimGridTileVisualizer>().blockTileConnectorID = TileConfig.BlockTileConnectorID;
		go.AddOrGet<BuildingHP>().destroyOnDamaged = true;
	}

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KPrefabID>().AddTag(GameTags.FloorTiles);
		go.GetComponent<KPrefabID>().AddTag(GameTags.PreventsSlipping);
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		base.DoPostConfigureUnderConstruction(go);
		go.AddOrGet<KAnimGridTileVisualizer>();
	}
}
