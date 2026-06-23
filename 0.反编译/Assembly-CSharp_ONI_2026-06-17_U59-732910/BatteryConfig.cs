using STRINGS;
using TUNING;
using UnityEngine;

public class BatteryConfig : BaseBatteryConfig
{
	public const string ID = "Battery";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = CreateBuildingDef("Battery", 1, 2, 30, "batterysm_kanim", 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.ALL_METALS, 800f, 0.25f, 1f, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		obj.Breakable = true;
		SoundEventVolumeCache.instance.AddVolume("batterysm_kanim", "Battery_rattle", NOISE_POLLUTION.NOISY.TIER1);
		obj.AddSearchTerms(SEARCH_TERMS.POWER);
		return obj;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		Battery battery = go.AddOrGet<Battery>();
		battery.capacity = 10000f;
		battery.joulesLostPerSecond = 1.6666666f;
		base.DoPostConfigureComplete(go);
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		base.ConfigureBuildingTemplate(go, prefab_tag);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
	}
}
