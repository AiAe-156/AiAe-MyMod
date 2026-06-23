using STRINGS;
using TUNING;
using UnityEngine;

public class BedConfig : IBuildingConfig
{
	public const string ID = "Bed";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("Bed", 2, 2, "bedlg_kanim", 10, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.RAW_MINERALS_OR_WOOD, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.NONE);
		obj.Overheatable = false;
		obj.AudioCategory = "Metal";
		obj.AddSearchTerms(SEARCH_TERMS.BED);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.BedType);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KAnimControllerBase>().initialAnim = "off";
		Bed bed = go.AddOrGet<Bed>();
		bed.effects = new string[2] { "BedStamina", "BedHealth" };
		bed.workLayer = Grid.SceneLayer.BuildingFront;
		Sleepable sleepable = go.AddOrGet<Sleepable>();
		sleepable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_sleep_bed_kanim") };
		sleepable.workLayer = Grid.SceneLayer.BuildingFront;
		if (DlcManager.IsContentSubscribed("DLC3_ID"))
		{
			DefragmentationZone defragmentationZone = go.AddOrGet<DefragmentationZone>();
			defragmentationZone.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_bionic_kanim") };
			defragmentationZone.workLayer = Grid.SceneLayer.BuildingFront;
		}
		go.AddOrGet<Ownable>().slotID = Db.Get().AssignableSlots.Bed.Id;
		go.AddOrGetDef<RocketUsageRestriction.Def>();
	}
}
