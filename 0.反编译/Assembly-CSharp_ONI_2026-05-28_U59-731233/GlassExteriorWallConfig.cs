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
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("GlassExteriorWall", 1, 1, "walls_glass_kanim", 30, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.GLASSES, 1600f, BuildLocationRule.NotInTiles, noise: NOISE_POLLUTION.NONE, decor: new EffectorValues
		{
			amount = 15,
			radius = 0
		});
		buildingDef.Entombable = false;
		buildingDef.Floodable = false;
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Glass";
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
		buildingDef.AddSearchTerms(SEARCH_TERMS.GLASS);
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
		KBatchedAnimController component = go.GetComponent<KBatchedAnimController>();
		component.initialBlendParameters = 0;
		go.GetComponent<KPrefabID>().AddTag(GameTags.Backwall);
		GeneratedBuildings.RemoveLoopingSounds(go);
	}
}
