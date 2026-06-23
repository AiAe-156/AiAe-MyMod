using STRINGS;
using TUNING;
using UnityEngine;

public class BeachChairConfig : IBuildingConfig
{
	public const string ID = "BeachChair";

	public static readonly int TAN_LUX = DUPLICANTSTATS.STANDARD.Light.HIGH_LIGHT;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("BeachChair", 2, 3, "beach_chair_kanim", 30, 60f, new float[2] { 400f, 2f }, new string[2] { "BuildableRaw", "BuildingFiber" }, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER4);
		buildingDef.Floodable = true;
		buildingDef.AudioCategory = "Metal";
		buildingDef.Overheatable = true;
		buildingDef.AddSearchTerms(SEARCH_TERMS.MORALE);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.RecBuilding);
		BeachChairWorkable beachChairWorkable = go.AddOrGet<BeachChairWorkable>();
		beachChairWorkable.basePriority = RELAXATION.PRIORITY.TIER4;
		BeachChair beachChair = go.AddOrGet<BeachChair>();
		beachChair.specificEffectUnlit = "BeachChairUnlit";
		beachChair.specificEffectLit = "BeachChairLit";
		beachChair.trackingEffect = "RecentlyBeachChair";
		RoomTracker roomTracker = go.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.RecRoom.Id;
		roomTracker.requirement = RoomTracker.Requirement.Recommended;
		go.AddOrGet<AnimTileable>();
		go.AddOrGetDef<RocketUsageRestriction.Def>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
