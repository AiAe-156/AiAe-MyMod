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
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("RoboPilotModule", 3, 4, "robot_rocket_control_station_kanim", 1000, 30f, TUNING.BUILDINGS.ROCKETRY_MASS_KG.HOLLOW_TIER1, new string[1] { SimHashes.Steel.ToString() }, 9999f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NOISY.TIER2, decor: TUNING.BUILDINGS.DECOR.NONE);
		BuildingTemplates.CreateRocketBuildingDef(obj);
		obj.DefaultAnimState = "grounded";
		obj.AttachmentSlotTag = GameTags.Rocket;
		obj.SceneLayer = Grid.SceneLayer.Building;
		obj.OverheatTemperature = 2273.15f;
		obj.Floodable = false;
		obj.ObjectLayer = ObjectLayer.Building;
		obj.RequiresPowerInput = false;
		obj.CanMove = true;
		obj.Cancellable = false;
		obj.AddSearchTerms(SEARCH_TERMS.ROBOT);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.AddOrGet<BuildingAttachPoint>().points = new BuildingAttachPoint.HardPoint[1]
		{
			new BuildingAttachPoint.HardPoint(new CellOffset(0, 4), GameTags.Rocket, null)
		};
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		Prioritizable.AddRef(go);
		go.GetComponent<KPrefabID>().AddTag(GameTags.LaunchButtonRocketModule);
		go.AddOrGet<RoboPilotModule>();
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
