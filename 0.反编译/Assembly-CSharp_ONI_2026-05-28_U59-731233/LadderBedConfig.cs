using STRINGS;
using TUNING;
using UnityEngine;

public class LadderBedConfig : IBuildingConfig
{
	public static string ID = "LadderBed";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 2, 2, "ladder_bed_kanim", 10, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.OnFloorOrBuildingAttachPoint, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.NONE);
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.PermittedRotations = PermittedRotations.FlipH;
		buildingDef.attachablePosition = new CellOffset(0, 0);
		buildingDef.AttachmentSlotTag = GameTags.LadderBed;
		buildingDef.ObjectLayer = ObjectLayer.Building;
		buildingDef.AddSearchTerms(SEARCH_TERMS.BED);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.BedType);
		BuildingAttachPoint buildingAttachPoint = go.AddOrGet<BuildingAttachPoint>();
		buildingAttachPoint.points = new BuildingAttachPoint.HardPoint[1]
		{
			new BuildingAttachPoint.HardPoint(new CellOffset(0, 2), GameTags.LadderBed, null)
		};
		go.AddOrGet<AnimTileable>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		CellOffset[] offsets = new CellOffset[2]
		{
			new CellOffset(0, 0),
			new CellOffset(0, 1)
		};
		Ladder ladder = go.AddOrGet<Ladder>();
		ladder.upwardsMovementSpeedMultiplier = 0.75f;
		ladder.downwardsMovementSpeedMultiplier = 0.75f;
		ladder.offsets = offsets;
		LadderBed.Def def = go.AddOrGetDef<LadderBed.Def>();
		def.offsets = offsets;
		go.GetComponent<KAnimControllerBase>().initialAnim = "off";
		Bed bed = go.AddOrGet<Bed>();
		bed.effects = new string[2] { "LadderBedStamina", "BedHealth" };
		bed.workLayer = Grid.SceneLayer.BuildingFront;
		Sleepable sleepable = go.AddOrGet<Sleepable>();
		sleepable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_ladder_bed_kanim") };
		sleepable.workLayer = Grid.SceneLayer.BuildingFront;
		if (DlcManager.IsContentSubscribed("DLC3_ID"))
		{
			DefragmentationZone defragmentationZone = go.AddOrGet<DefragmentationZone>();
			defragmentationZone.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_bionic_kanim") };
			defragmentationZone.workLayer = Grid.SceneLayer.BuildingFront;
		}
		Ownable ownable = go.AddOrGet<Ownable>();
		ownable.slotID = Db.Get().AssignableSlots.Bed.Id;
		go.AddOrGetDef<RocketUsageRestriction.Def>();
	}
}
