using STRINGS;
using TUNING;
using UnityEngine;

public class EspressoMachineConfig : IBuildingConfig
{
	public const string ID = "EspressoMachine";

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("EspressoMachine", 3, 3, "espresso_machine_kanim", 30, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.BONUS.TIER1);
		obj.ViewMode = OverlayModes.LiquidConduits.ID;
		obj.Floodable = true;
		obj.AudioCategory = "Metal";
		obj.Overheatable = true;
		obj.InputConduitType = ConduitType.Liquid;
		obj.UtilityInputOffset = new CellOffset(1, 2);
		obj.RequiresPowerInput = true;
		obj.PowerInputOffset = new CellOffset(1, 2);
		obj.EnergyConsumptionWhenActive = 480f;
		obj.SelfHeatKilowattsWhenActive = 1f;
		obj.AddSearchTerms(SEARCH_TERMS.MORALE);
		return obj;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		KPrefabID component = go.GetComponent<KPrefabID>();
		component.AddTag(RoomConstraints.ConstraintTags.RecBuilding);
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = 20f;
		storage.SetDefaultStoredItemModifiers(Storage.StandardFabricatorStorage);
		ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
		conduitConsumer.conduitType = ConduitType.Liquid;
		conduitConsumer.capacityTag = ElementLoader.FindElementByHash(SimHashes.Water).tag;
		conduitConsumer.capacityKG = 2f;
		conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;
		ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.RequestedItemTag = new Tag("SpiceNut");
		manualDeliveryKG.capacity = 10f;
		manualDeliveryKG.refillMass = 5f;
		manualDeliveryKG.MinimumMass = 1f;
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.MachineFetch.IdHash;
		go.AddOrGet<EspressoMachineWorkable>();
		go.AddOrGet<EspressoMachine>();
		RoomTracker roomTracker = go.AddOrGet<RoomTracker>();
		roomTracker.requiredRoomType = Db.Get().RoomTypes.RecRoom.Id;
		roomTracker.requirement = RoomTracker.Requirement.Recommended;
		component.prefabInitFn += OnInit;
	}

	private void OnInit(GameObject go)
	{
		EspressoMachineWorkable component = go.GetComponent<EspressoMachineWorkable>();
		KAnimFile[] value = new KAnimFile[1] { Assets.GetAnim("anim_interacts_espresso_machine_kanim") };
		component.workerTypeOverrideAnims.Add(MinionConfig.ID, value);
		component.workerTypeOverrideAnims.Add(BionicMinionConfig.ID, new KAnimFile[1] { Assets.GetAnim("anim_bionic_interacts_espresso_machine_kanim") });
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
