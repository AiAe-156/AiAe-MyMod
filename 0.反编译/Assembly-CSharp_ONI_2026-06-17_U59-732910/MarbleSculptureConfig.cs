using STRINGS;
using TUNING;
using UnityEngine;

public class MarbleSculptureConfig : IBuildingConfig
{
	public const string ID = "MarbleSculpture";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("MarbleSculpture", 2, 3, "sculpture_marble_kanim", 10, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.PRECIOUS_ROCKS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: new EffectorValues
		{
			amount = 20,
			radius = 8
		});
		obj.Floodable = false;
		obj.Overheatable = false;
		obj.AudioCategory = "Metal";
		obj.BaseTimeUntilRepair = -1f;
		obj.ViewMode = OverlayModes.Decor.ID;
		obj.DefaultAnimState = "slab";
		obj.PermittedRotations = PermittedRotations.FlipH;
		obj.RequiredSkillPerkID = Db.Get().SkillPerks.CanArt.Id;
		obj.AddSearchTerms(SEARCH_TERMS.STATUE);
		obj.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		obj.AddSearchTerms(SEARCH_TERMS.MORALE);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<BuildingComplete>().isArtable = true;
		go.GetComponent<KPrefabID>().AddTag(GameTags.Decoration);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddComponent<Sculpture>().defaultAnimName = "slab";
	}
}
