using STRINGS;
using TUNING;
using UnityEngine;

public class WoodSculptureConfig : IBuildingConfig
{
	public const string ID = "WoodSculpture";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC2;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("WoodSculpture", 1, 1, "sculpture_wood_kanim", 10, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.WOODS, 800f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NONE, decor: new EffectorValues
		{
			amount = 3,
			radius = 4
		});
		buildingDef.SceneLayer = Grid.SceneLayer.InteriorWall;
		buildingDef.Floodable = false;
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.BaseTimeUntilRepair = -1f;
		buildingDef.ViewMode = OverlayModes.Decor.ID;
		buildingDef.DefaultAnimState = "slab";
		buildingDef.PermittedRotations = PermittedRotations.FlipH;
		buildingDef.RequiredSkillPerkID = Db.Get().SkillPerks.CanArt.Id;
		buildingDef.AddSearchTerms(SEARCH_TERMS.STATUE);
		buildingDef.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		buildingDef.AddSearchTerms(SEARCH_TERMS.MORALE);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<BuildingComplete>().isArtable = true;
		go.GetComponent<KPrefabID>().AddTag(GameTags.Decoration);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		Artable artable = go.AddComponent<LongRangeSculpture>();
		artable.defaultAnimName = "slab";
	}
}
