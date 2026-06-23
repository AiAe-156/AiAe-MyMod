using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class RoboPilotCommandModuleConfig : IBuildingConfig
{
	public const string ID = "RoboPilotCommandModule";

	public static float DATABANKCONSUMPTION = 2f;

	public static float DATABANKRANGE = 10000f / DATABANKCONSUMPTION;

	private const string TRIGGER_LAUNCH_PORT_ID = "TriggerLaunch";

	private const string LAUNCH_READY_PORT_ID = "LaunchReady";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC3;
	}

	public override string[] GetForbiddenDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("RoboPilotCommandModule", 5, 5, "robo_command_capsule_kanim", 1000, 60f, TUNING.BUILDINGS.ROCKETRY_MASS_KG.COMMAND_MODULE_MASS, new string[1] { SimHashes.Steel.ToString() }, 9999f, BuildLocationRule.BuildingAttachPoint, noise: NOISE_POLLUTION.NOISY.TIER2, decor: TUNING.BUILDINGS.DECOR.NONE);
		BuildingTemplates.CreateRocketBuildingDef(obj);
		obj.SceneLayer = Grid.SceneLayer.BuildingFront;
		obj.OverheatTemperature = 2273.15f;
		obj.Floodable = false;
		obj.AttachmentSlotTag = GameTags.Rocket;
		obj.ObjectLayer = ObjectLayer.Building;
		obj.RequiresPowerInput = false;
		obj.attachablePosition = new CellOffset(0, 0);
		obj.CanMove = true;
		obj.LogicInputPorts = new List<LogicPorts.Port> { LogicPorts.Port.InputPort("TriggerLaunch", new CellOffset(0, 1), STRINGS.BUILDINGS.PREFABS.COMMANDMODULE.LOGIC_PORT_LAUNCH, STRINGS.BUILDINGS.PREFABS.COMMANDMODULE.LOGIC_PORT_LAUNCH_ACTIVE, STRINGS.BUILDINGS.PREFABS.COMMANDMODULE.LOGIC_PORT_LAUNCH_INACTIVE) };
		obj.LogicOutputPorts = new List<LogicPorts.Port> { LogicPorts.Port.OutputPort("LaunchReady", new CellOffset(0, 2), STRINGS.BUILDINGS.PREFABS.COMMANDMODULE.LOGIC_PORT_READY, STRINGS.BUILDINGS.PREFABS.COMMANDMODULE.LOGIC_PORT_READY_ACTIVE, STRINGS.BUILDINGS.PREFABS.COMMANDMODULE.LOGIC_PORT_READY_INACTIVE) };
		obj.AddSearchTerms(SEARCH_TERMS.ROBOT);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		LaunchConditionManager launchConditionManager = go.AddOrGet<LaunchConditionManager>();
		launchConditionManager.triggerPort = "TriggerLaunch";
		launchConditionManager.statusPort = "LaunchReady";
		RoboPilotModule roboPilotModule = go.AddOrGet<RoboPilotModule>();
		roboPilotModule.consumeDataBanksOnLand = true;
		roboPilotModule.dataBankConsumption = 1;
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
		go.AddOrGet<CommandModule>().robotPilotControlled = true;
		go.AddOrGet<RobotCommandConditions>();
		go.AddOrGet<LaunchableRocket>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		BuildingTemplates.ExtendBuildingToRocketModule(go, "rocket_command_module_bg_kanim");
	}
}
