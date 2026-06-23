using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ExteriorWallConfig : IBuildingConfig
{
	public const string ID = "ExteriorWall";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("ExteriorWall", 1, 1, "walls_kanim", 30, 3f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.RAW_MINERALS_OR_WOOD, 1600f, BuildLocationRule.NotInTiles, noise: NOISE_POLLUTION.NONE, decor: new EffectorValues
		{
			amount = 10,
			radius = 0
		});
		buildingDef.Entombable = false;
		buildingDef.Floodable = false;
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "small";
		buildingDef.BaseTimeUntilRepair = -1f;
		buildingDef.DefaultAnimState = "off";
		buildingDef.ObjectLayer = ObjectLayer.Backwall;
		buildingDef.SceneLayer = Grid.SceneLayer.Backwall;
		buildingDef.PermittedRotations = PermittedRotations.R360;
		buildingDef.ReplacementLayer = ObjectLayer.ReplacementBackwall;
		buildingDef.ReplacementCandidateLayers = new List<ObjectLayer>
		{
			ObjectLayer.FoundationTile,
			ObjectLayer.Backwall
		};
		buildingDef.ReplacementTags = new List<Tag>
		{
			GameTags.FloorTiles,
			GameTags.Backwall
		};
		buildingDef.AddSearchTerms(SEARCH_TERMS.TILE);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		AnimTileable animTileable = go.AddOrGet<AnimTileable>();
		animTileable.objectLayer = ObjectLayer.Backwall;
		go.AddComponent<ZoneTile>();
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KBatchedAnimController>().initialBlendParameters = 0;
		go.GetComponent<KPrefabID>().AddTag(GameTags.Backwall);
		GeneratedBuildings.RemoveLoopingSounds(go);
	}
}
