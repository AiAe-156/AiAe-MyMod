using STRINGS;
using TUNING;
using UnityEngine;

public class PropBeachChairConfig : IBuildingConfig
{
	public const string ID = "PropBeachChair";

	public static readonly int TAN_LUX = DUPLICANTSTATS.STANDARD.Light.HIGH_LIGHT;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("PropBeachChair", 2, 3, "beachchair_tattered_kanim", 30, 60f, new float[2] { 400f, 2f }, new string[2] { "BuildableRaw", "BuildingFiber" }, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER4);
		obj.Floodable = true;
		obj.AudioCategory = "Metal";
		obj.Overheatable = true;
		obj.ShowInBuildMenu = false;
		obj.AddSearchTerms(SEARCH_TERMS.MORALE);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(GameTags.HideFromCodex);
		component.AddTag(RoomConstraints.ConstraintTags.RecBuilding);
		go.AddOrGet<BeachChairWorkable>().basePriority = RELAXATION.PRIORITY.TIER4;
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
