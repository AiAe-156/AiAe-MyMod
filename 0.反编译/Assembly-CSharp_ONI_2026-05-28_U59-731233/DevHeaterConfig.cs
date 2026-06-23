using TUNING;
using UnityEngine;

public class DevHeaterConfig : IBuildingConfig
{
	public const string ID = "DevHeater";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("DevHeater", 1, 1, "dev_generator_kanim", 100, 3f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER0, MATERIALS.ALL_METALS, 9999f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NOISY.TIER5, decor: BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.RequiresPowerInput = false;
		buildingDef.ViewMode = OverlayModes.Light.ID;
		buildingDef.AudioCategory = "HollowMetal";
		buildingDef.AudioSize = "large";
		buildingDef.Floodable = false;
		buildingDef.DebugOnly = true;
		buildingDef.Overheatable = false;
		SoundEventVolumeCache.instance.AddVolume("dev_lightgenerator_kanim", "PowerSwitch_on", NOISE_POLLUTION.NOISY.TIER3);
		SoundEventVolumeCache.instance.AddVolume("dev_lightgenerator_kanim", "PowerSwitch_off", NOISE_POLLUTION.NOISY.TIER3);
		return buildingDef;
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddTag(GameTags.DevBuilding);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<DirectVolumeHeater>();
	}
}
