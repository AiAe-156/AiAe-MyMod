using STRINGS;
using TUNING;
using UnityEngine;

public class CanvasTallConfig : IBuildingConfig
{
	public const string ID = "CanvasTall";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("CanvasTall", 2, 3, "painting_tall_off_kanim", 30, 120f, new float[2] { 400f, 1f }, new string[2] { "Metal", "BuildingFiber" }, 1600f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NONE, decor: new EffectorValues
		{
			amount = 15,
			radius = 6
		});
		buildingDef.Floodable = false;
		buildingDef.SceneLayer = Grid.SceneLayer.InteriorWall;
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.BaseTimeUntilRepair = -1f;
		buildingDef.ViewMode = OverlayModes.Decor.ID;
		buildingDef.DefaultAnimState = "off";
		buildingDef.PermittedRotations = PermittedRotations.FlipH;
		buildingDef.RequiredSkillPerkID = Db.Get().SkillPerks.CanArt.Id;
		buildingDef.AddSearchTerms(SEARCH_TERMS.MORALE);
		buildingDef.AddSearchTerms(SEARCH_TERMS.ARTWORK);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<BuildingComplete>().isArtable = true;
		go.GetComponent<KPrefabID>().AddTag(GameTags.Decoration);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		SymbolOverrideControllerUtil.AddToPrefab(go);
		Artable artable = go.AddComponent<Painting>();
		artable.defaultAnimName = "off";
	}
}
