using STRINGS;
using TUNING;
using UnityEngine;

public class BatteryMediumConfig : BaseBatteryConfig
{
	public const string ID = "BatteryMedium";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = CreateBuildingDef("BatteryMedium", 2, 2, 30, "batterymed_kanim", 60f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 800f, 0.25f, 1f, noise: NOISE_POLLUTION.NOISY.TIER1, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		SoundEventVolumeCache.instance.AddVolume("batterymed_kanim", "Battery_med_rattle", NOISE_POLLUTION.NOISY.TIER2);
		buildingDef.AddSearchTerms(SEARCH_TERMS.POWER);
		return buildingDef;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		Battery battery = go.AddOrGet<Battery>();
		battery.capacity = 40000f;
		battery.joulesLostPerSecond = 3.3333333f;
		base.DoPostConfigureComplete(go);
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		base.ConfigureBuildingTemplate(go, prefab_tag);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
	}
}
