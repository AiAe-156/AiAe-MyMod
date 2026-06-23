using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ExteriorWallConfig : IBuildingConfig
{
	public const string ID = "ExteriorWall";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("ExteriorWall", 1, 1, "walls_kanim", 30, 3f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.RAW_MINERALS_OR_WOOD, 1600f, BuildLocationRule.NotInTiles, noise: NOISE_POLLUTION.NONE, decor: new EffectorValues
		{
			amount = 10,
			radius = 0
		});
		obj.Entombable = false;
		obj.Floodable = false;
		obj.Overheatable = false;
		obj.AudioCategory = "Metal";
		obj.AudioSize = "small";
		obj.BaseTimeUntilRepair = -1f;
		obj.DefaultAnimState = "off";
		obj.ObjectLayer = ObjectLayer.Backwall;
		obj.SceneLayer = Grid.SceneLayer.Backwall;
		obj.PermittedRotations = PermittedRotations.R360;
		obj.ReplacementLayer = ObjectLayer.ReplacementBackwall;
		obj.ReplacementCandidateLayers = new List<ObjectLayer>
		{
			ObjectLayer.FoundationTile,
			ObjectLayer.Backwall
		};
		obj.ReplacementTags = new List<Tag>
		{
			GameTags.FloorTiles,
			GameTags.Backwall
		};
		obj.AddSearchTerms(SEARCH_TERMS.TILE);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.GetComponent<KPrefabID>();
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
		go.AddOrGet<AnimTileable>().objectLayer = ObjectLayer.Backwall;
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
