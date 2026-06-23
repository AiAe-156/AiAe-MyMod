using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class RocketControlStationConfig : IBuildingConfig
{
	public static string ID = "RocketControlStation";

	public const float CONSOLE_WORK_TIME = 30f;

	public const float CONSOLE_IDLE_TIME = 120f;

	public const float WARNING_COOLDOWN = 30f;

	public const float DEFAULT_SPEED = 1f;

	public const float SLOW_SPEED = 0.5f;

	public const float SUPER_SPEED = 1.5f;

	public const float DEFAULT_PILOT_MODIFIER = 1f;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, 2, 2, "rocket_control_station_kanim", 30, 60f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.RAW_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER3, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER2);
		buildingDef.Overheatable = false;
		buildingDef.Repairable = false;
		buildingDef.Floodable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "large";
		buildingDef.DefaultAnimState = "off";
		buildingDef.OnePerWorld = true;
		buildingDef.RequiredSkillPerkID = Db.Get().SkillPerks.CanUseRocketControlStation.Id;
		buildingDef.LogicInputPorts = new List<LogicPorts.Port> { LogicPorts.Port.InputPort(RocketControlStation.PORT_ID, new CellOffset(0, 0), STRINGS.BUILDINGS.PREFABS.ROCKETCONTROLSTATION.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.ROCKETCONTROLSTATION.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.ROCKETCONTROLSTATION.LOGIC_PORT_INACTIVE) };
		buildingDef.AddSearchTerms(SEARCH_TERMS.ROCKET);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(GameTags.RocketInteriorBuilding);
		component.AddTag(GameTags.UniquePerWorld);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
		RocketControlStationIdleWorkable rocketControlStationIdleWorkable = go.AddOrGet<RocketControlStationIdleWorkable>();
		rocketControlStationIdleWorkable.workLayer = Grid.SceneLayer.BuildingUse;
		RocketControlStationLaunchWorkable rocketControlStationLaunchWorkable = go.AddOrGet<RocketControlStationLaunchWorkable>();
		rocketControlStationLaunchWorkable.workLayer = Grid.SceneLayer.BuildingUse;
		go.AddOrGet<RocketControlStation>();
		go.AddOrGetDef<PoweredController.Def>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.RocketInterior);
	}
}
