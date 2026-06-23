using System.Collections.Generic;
using System.Linq;
using TUNING;
using UnityEngine;

public class RailGunPayloadOpenerConfig : IBuildingConfig
{
	public const string ID = "RailGunPayloadOpener";

	private ConduitPortInfo liquidOutputPort = new ConduitPortInfo(ConduitType.Liquid, new CellOffset(0, 0));

	private ConduitPortInfo gasOutputPort = new ConduitPortInfo(ConduitType.Gas, new CellOffset(1, 0));

	private ConduitPortInfo solidOutputPort = new ConduitPortInfo(ConduitType.Solid, new CellOffset(-1, 0));

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("RailGunPayloadOpener", 3, 3, "railgun_emptier_kanim", 250, 10f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER2, MATERIALS.ALL_METALS, 1600f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER5, decor: BUILDINGS.DECOR.NONE);
		buildingDef.Floodable = false;
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.DefaultAnimState = "on";
		buildingDef.RequiresPowerInput = true;
		buildingDef.PowerInputOffset = new CellOffset(0, 0);
		buildingDef.EnergyConsumptionWhenActive = 120f;
		buildingDef.SelfHeatKilowattsWhenActive = 0.5f;
		return buildingDef;
	}

	private void AttachPorts(GameObject go)
	{
		ConduitSecondaryOutput conduitSecondaryOutput = go.AddComponent<ConduitSecondaryOutput>();
		conduitSecondaryOutput.portInfo = liquidOutputPort;
		ConduitSecondaryOutput conduitSecondaryOutput2 = go.AddComponent<ConduitSecondaryOutput>();
		conduitSecondaryOutput2.portInfo = gasOutputPort;
		ConduitSecondaryOutput conduitSecondaryOutput3 = go.AddComponent<ConduitSecondaryOutput>();
		conduitSecondaryOutput3.portInfo = solidOutputPort;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		RailGunPayloadOpener railGunPayloadOpener = go.AddOrGet<RailGunPayloadOpener>();
		railGunPayloadOpener.liquidPortInfo = liquidOutputPort;
		railGunPayloadOpener.gasPortInfo = gasOutputPort;
		railGunPayloadOpener.solidPortInfo = solidOutputPort;
		railGunPayloadOpener.payloadStorage = go.AddComponent<Storage>();
		railGunPayloadOpener.payloadStorage.showInUI = true;
		railGunPayloadOpener.payloadStorage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		railGunPayloadOpener.payloadStorage.storageFilters = new List<Tag> { GameTags.RailGunPayloadEmptyable };
		railGunPayloadOpener.payloadStorage.capacityKg = 2000f;
		railGunPayloadOpener.resourceStorage = go.AddComponent<Storage>();
		railGunPayloadOpener.resourceStorage.showInUI = true;
		railGunPayloadOpener.resourceStorage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		List<Tag> first = STORAGEFILTERS.STORAGE_LOCKERS_STANDARD.Concat(STORAGEFILTERS.GASES).ToList();
		first = first.Concat(STORAGEFILTERS.LIQUIDS).ToList();
		railGunPayloadOpener.resourceStorage.storageFilters = first;
		railGunPayloadOpener.resourceStorage.capacityKg = 20000f;
		ManualDeliveryKG manualDeliveryKG = go.AddComponent<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(railGunPayloadOpener.payloadStorage);
		manualDeliveryKG.RequestedItemTag = GameTags.RailGunPayloadEmptyable;
		manualDeliveryKG.capacity = 2000f;
		manualDeliveryKG.refillMass = 200f;
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.MachineFetch.IdHash;
		manualDeliveryKG.operationalRequirement = Operational.State.None;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<BuildingCellVisualizer>();
		DropAllWorkable dropAllWorkable = go.AddOrGet<DropAllWorkable>();
		dropAllWorkable.dropWorkTime = 90f;
		dropAllWorkable.choreTypeID = Db.Get().ChoreTypes.Fetch.Id;
		dropAllWorkable.ConfigureMultitoolContext("build", EffectConfigs.BuildSplashId);
		RequireInputs component = go.GetComponent<RequireInputs>();
		component.SetRequirements(power: true, conduit: false);
		component.requireConduitHasMass = false;
	}

	public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
	{
		base.DoPostConfigurePreview(def, go);
		AttachPorts(go);
		go.AddOrGet<BuildingCellVisualizer>();
	}

	public override void DoPostConfigureUnderConstruction(GameObject go)
	{
		base.DoPostConfigureUnderConstruction(go);
		AttachPorts(go);
		go.AddOrGet<BuildingCellVisualizer>();
	}
}
