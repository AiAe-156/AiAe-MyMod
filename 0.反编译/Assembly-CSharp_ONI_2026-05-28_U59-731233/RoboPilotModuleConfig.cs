using STRINGS;
using TUNING;
using UnityEngine;

public class RoboPilotModuleConfig : IBuildingConfig
{
	public const string ID = "RoboPilotModule";

	public override string[] GetRequiredDlcIds()
	{
		return new string[2] { "EXPANSION1_ID", "DLC3_ID" };
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("RoboPilotModule", 3, 4, "robot_rocket_control_station_kanim", 1000, 30f, TUNING.BUILDINGS.ROCKETRY_MASS_KG.HOLLOW_TIER1, new string[1] { SimHashes.Steel.ToString() }, 9999f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NOISY.TIER2, decor: TUNING.BUILDINGS.DECOR.NONE);
		BuildingTemplates.CreateRocketBuildingDef(buildingDef);
		buildingDef.DefaultAnimState = "grounded";
		buildingDef.AttachmentSlotTag = GameTags.Rocket;
		buildingDef.SceneLayer = Grid.SceneLayer.Building;
		buildingDef.OverheatTemperature = 2273.15f;
		buildingDef.Floodable = false;
		buildingDef.ObjectLayer = ObjectLayer.Building;
		buildingDef.RequiresPowerInput = false;
		buildingDef.CanMove = true;
		buildingDef.Cancellable = false;
		buildingDef.AddSearchTerms(SEARCH_TERMS.ROBOT);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		BuildingAttachPoint buildingAttachPoint = go.AddOrGet<BuildingAttachPoint>();
		buildingAttachPoint.points = new BuildingAttachPoint.HardPoint[1]
		{
			new BuildingAttachPoint.HardPoint(new CellOffset(0, 4), GameTags.Rocket, null)
		};
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		Prioritizable.AddRef(go);
		go.GetComponent<KPrefabID>().AddTag(GameTags.LaunchButtonRocketModule);
		RoboPilotModule roboPilotModule = go.AddOrGet<RoboPilotModule>();
		go.AddOrGet<LaunchableRocketCluster>();
		go.AddOrGet<RobotCommandConditions>();
		go.AddOrGet<RocketProcessConditionDisplayTarget>();
		go.AddOrGet<RocketLaunchConditionVisualizer>();
		Storage storage = go.AddComponent<Storage>();
		storage.showInUI = true;
		storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
		storage.capacityKg = 100f;
		ManualDeliveryKG manualDeliveryKG = go.AddComponent<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.MachineFetch.IdHash;
		manualDeliveryKG.capacity = storage.capacityKg;
		manualDeliveryKG.refillMass = 20f;
		manualDeliveryKG.RequestedItemTag = DatabankHelper.TAG;
		manualDeliveryKG.MinimumMass = 1f;
		BuildingTemplates.ExtendBuildingToRocketModuleCluster(go, null, ROCKETRY.BURDEN.MODERATE);
		go.GetComponent<ReorderableBuilding>().buildConditions.Add(new LimitOneRoboPilotModule());
	}
}
