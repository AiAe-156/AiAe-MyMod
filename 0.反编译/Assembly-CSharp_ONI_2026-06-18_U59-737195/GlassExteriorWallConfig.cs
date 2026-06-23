using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class GlassExteriorWallConfig : IBuildingConfig
{
	public const string ID = "GlassExteriorWall";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("GlassExteriorWall", 1, 1, "walls_glass_kanim", 30, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.GLASSES, 1600f, BuildLocationRule.NotInTiles, noise: NOISE_POLLUTION.NONE, decor: new EffectorValues
		{
			amount = 15,
			radius = 0
		});
		obj.Entombable = false;
		obj.Floodable = false;
		obj.Overheatable = false;
		obj.AudioCategory = "Glass";
		obj.AudioSize = "small";
		obj.BaseTimeUntilRepair = -1f;
		obj.DefaultAnimState = "off";
		obj.ObjectLayer = ObjectLayer.Backwall;
		obj.SceneLayer = Grid.SceneLayer.Backwall;
		obj.ForegroundLayer = Grid.SceneLayer.Backwall;
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
		obj.AddSearchTerms(SEARCH_TERMS.GLASS);
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
