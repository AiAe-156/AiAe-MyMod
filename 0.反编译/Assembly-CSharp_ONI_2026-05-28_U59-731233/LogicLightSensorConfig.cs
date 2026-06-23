using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class LogicLightSensorConfig : IBuildingConfig
{
	public static string ID = "LogicLightSensor";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 1, 1, "logiclightsensor_kanim", 30, 30f, new float[2]
		{
			TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0[0],
			TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0[0]
		}, new string[2] { "RefinedMetal", "Glasses" }, 1600f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER0);
		buildingDef.Overheatable = false;
		buildingDef.Floodable = false;
		buildingDef.Entombable = false;
		buildingDef.ViewMode = OverlayModes.Logic.ID;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "small";
		buildingDef.SceneLayer = Grid.SceneLayer.Building;
		buildingDef.AlwaysOperational = true;
		buildingDef.LogicOutputPorts = new List<LogicPorts.Port>();
		buildingDef.LogicOutputPorts.Add(LogicPorts.Port.OutputPort(LogicSwitch.PORT_ID, new CellOffset(0, 0), STRINGS.BUILDINGS.PREFABS.LOGICLIGHTSENSOR.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.LOGICLIGHTSENSOR.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.LOGICLIGHTSENSOR.LOGIC_PORT_INACTIVE, show_wire_missing_icon: true));
		SoundEventVolumeCache.instance.AddVolume("logiclightsensor_kanim", "PowerSwitch_on", NOISE_POLLUTION.NOISY.TIER3);
		SoundEventVolumeCache.instance.AddVolume("logiclightsensor_kanim", "PowerSwitch_off", NOISE_POLLUTION.NOISY.TIER3);
		GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);
		buildingDef.AddSearchTerms(SEARCH_TERMS.AUTOMATION);
		return buildingDef;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		LogicLightSensor logicLightSensor = go.AddOrGet<LogicLightSensor>();
		logicLightSensor.manuallyControlled = false;
		logicLightSensor.minBrightness = 0f;
		logicLightSensor.maxBrightness = 15000f;
		go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayInFrontOfConduits);
	}
}
