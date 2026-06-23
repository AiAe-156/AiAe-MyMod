using TUNING;
using UnityEngine;

public class HabitatModuleMediumConfig : IBuildingConfig
{
	public const string ID = "HabitatModuleMedium";

	private ConduitPortInfo gasInputPort = new ConduitPortInfo(ConduitType.Gas, new CellOffset(-2, 0));

	private ConduitPortInfo gasOutputPort = new ConduitPortInfo(ConduitType.Gas, new CellOffset(2, 0));

	private ConduitPortInfo liquidInputPort = new ConduitPortInfo(ConduitType.Liquid, new CellOffset(-2, 3));

	private ConduitPortInfo liquidOutputPort = new ConduitPortInfo(ConduitType.Liquid, new CellOffset(2, 3));

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("HabitatModuleMedium", 5, 4, "rocket_habitat_medium_module_kanim", 1000, 60f, BUILDINGS.ROCKETRY_MASS_KG.DENSE_TIER1, MATERIALS.RAW_METALS, 9999f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NOISY.TIER2, decor: BUILDINGS.DECOR.NONE);
		BuildingTemplates.CreateRocketBuildingDef(buildingDef);
		buildingDef.AttachmentSlotTag = GameTags.Rocket;
		buildingDef.SceneLayer = Grid.SceneLayer.Building;
		buildingDef.OverheatTemperature = 2273.15f;
		buildingDef.Floodable = false;
		buildingDef.ObjectLayer = ObjectLayer.Building;
		buildingDef.ForegroundLayer = Grid.SceneLayer.Front;
		buildingDef.RequiresPowerInput = false;
		buildingDef.attachablePosition = new CellOffset(0, 0);
		buildingDef.CanMove = true;
		buildingDef.Cancellable = false;
		buildingDef.ShowInBuildMenu = false;
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		go.AddOrGet<LoopingSounds>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.GetComponent<KPrefabID>().AddTag(GameTags.LaunchButtonRocketModule);
		AssignmentGroupController assignmentGroupController = go.AddOrGet<AssignmentGroupController>();
		assignmentGroupController.generateGroupOnStart = true;
		PassengerRocketModule passengerRocketModule = go.AddOrGet<PassengerRocketModule>();
		passengerRocketModule.interiorReverbSnapshot = AudioMixerSnapshots.Get().MediumRocketInteriorReverbSnapshot;
		ClustercraftExteriorDoor clustercraftExteriorDoor = go.AddOrGet<ClustercraftExteriorDoor>();
		clustercraftExteriorDoor.interiorTemplateName = "expansion1::interiors/habitat_medium";
		go.AddOrGetDef<SimpleDoorController.Def>();
		go.AddOrGet<NavTeleporter>();
		go.AddOrGet<AccessControl>();
		go.AddOrGet<LaunchableRocketCluster>();
		go.AddOrGet<RocketCommandConditions>();
		go.AddOrGet<RocketProcessConditionDisplayTarget>();
		go.AddOrGet<RocketLaunchConditionVisualizer>();
		CharacterOverlay characterOverlay = go.AddOrGet<CharacterOverlay>();
		characterOverlay.shouldShowName = true;
		BuildingAttachPoint buildingAttachPoint = go.AddOrGet<BuildingAttachPoint>();
		buildingAttachPoint.points = new BuildingAttachPoint.HardPoint[1]
		{
			new BuildingAttachPoint.HardPoint(new CellOffset(0, 4), GameTags.Rocket, null)
		};
		Storage storage = go.AddComponent<Storage>();
		storage.showInUI = false;
		storage.capacityKg = 10f;
		RocketConduitSender rocketConduitSender = go.AddComponent<RocketConduitSender>();
		rocketConduitSender.conduitStorage = storage;
		rocketConduitSender.conduitPortInfo = liquidInputPort;
		RocketConduitReceiver rocketConduitReceiver = go.AddComponent<RocketConduitReceiver>();
		rocketConduitReceiver.conduitPortInfo = liquidOutputPort;
		Storage storage2 = go.AddComponent<Storage>();
		storage2.showInUI = false;
		storage2.capacityKg = 1f;
		RocketConduitSender rocketConduitSender2 = go.AddComponent<RocketConduitSender>();
		rocketConduitSender2.conduitStorage = storage2;
		rocketConduitSender2.conduitPortInfo = gasInputPort;
		RocketConduitReceiver rocketConduitReceiver2 = go.AddComponent<RocketConduitReceiver>();
		rocketConduitReceiver2.conduitPortInfo = gasOutputPort;
	}

	private void AttachPorts(GameObject go)
	{
		ConduitSecondaryInput conduitSecondaryInput = go.AddComponent<ConduitSecondaryInput>();
		conduitSecondaryInput.portInfo = liquidInputPort;
		ConduitSecondaryOutput conduitSecondaryOutput = go.AddComponent<ConduitSecondaryOutput>();
		conduitSecondaryOutput.portInfo = liquidOutputPort;
		ConduitSecondaryInput conduitSecondaryInput2 = go.AddComponent<ConduitSecondaryInput>();
		conduitSecondaryInput2.portInfo = gasInputPort;
		ConduitSecondaryOutput conduitSecondaryOutput2 = go.AddComponent<ConduitSecondaryOutput>();
		conduitSecondaryOutput2.portInfo = gasOutputPort;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		BuildingTemplates.ExtendBuildingToRocketModuleCluster(go, null, ROCKETRY.BURDEN.MAJOR);
		Ownable ownable = go.AddOrGet<Ownable>();
		ownable.slotID = Db.Get().AssignableSlots.HabitatModule.Id;
		ownable.canBePublic = false;
		FakeFloorAdder fakeFloorAdder = go.AddOrGet<FakeFloorAdder>();
		fakeFloorAdder.floorOffsets = new CellOffset[5]
		{
			new CellOffset(-2, -1),
			new CellOffset(-1, -1),
			new CellOffset(0, -1),
			new CellOffset(1, -1),
			new CellOffset(2, -1)
		};
		fakeFloorAdder.initiallyActive = false;
		go.AddOrGet<BuildingCellVisualizer>();
		go.GetComponent<ReorderableBuilding>().buildConditions.Add(new LimitOneCommandModule());
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		base.DoPostConfigurePreview(def, go);
		go.AddOrGet<BuildingCellVisualizer>();
		AttachPorts(go);
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		base.DoPostConfigureUnderConstruction(go);
		go.AddOrGet<BuildingCellVisualizer>();
		AttachPorts(go);
	}
}
