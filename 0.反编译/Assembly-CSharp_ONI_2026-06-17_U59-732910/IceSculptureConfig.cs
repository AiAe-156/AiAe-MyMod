using STRINGS;
using TUNING;
using UnityEngine;

public class IceSculptureConfig : IBuildingConfig
{
	public const string ID = "IceSculpture";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("IceSculpture", 2, 2, "icesculpture_kanim", 10, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, new string[1] { "Ice" }, 273.15f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: new EffectorValues
		{
			amount = 35,
			radius = 8
		});
		obj.Floodable = false;
		obj.Overheatable = false;
		obj.AudioCategory = "Metal";
		obj.BaseTimeUntilRepair = -1f;
		obj.ViewMode = OverlayModes.Decor.ID;
		obj.DefaultAnimState = "slab";
		obj.PermittedRotations = PermittedRotations.FlipH;
		obj.Temperature = 253.15f;
		obj.RequiredSkillPerkID = Db.Get().SkillPerks.CanArt.Id;
		obj.AddSearchTerms(SEARCH_TERMS.STATUE);
		obj.AddSearchTerms(SEARCH_TERMS.MORALE);
		obj.AddSearchTerms(SEARCH_TERMS.ARTWORK);
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
