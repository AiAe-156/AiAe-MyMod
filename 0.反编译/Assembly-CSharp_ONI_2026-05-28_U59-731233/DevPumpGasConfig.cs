using TUNING;
using UnityEngine;

public class DevPumpGasConfig : IBuildingConfig
{
	public const string ID = "DevPumpGas";

	private const ConduitType CONDUIT_TYPE = ConduitType.Gas;

	private ConduitPortInfo primaryPort = new ConduitPortInfo(ConduitType.Gas, new CellOffset(1, 1));

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("DevPumpGas", 2, 2, "dev_pump_gas_kanim", 100, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, MATERIALS.ALL_METALS, 9999f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NOISY.TIER2, decor: BUILDINGS.DECOR.PENALTY.TIER1);
		buildingDef.RequiresPowerInput = false;
		buildingDef.OutputConduitType = ConduitType.Gas;
		buildingDef.Floodable = false;
		buildingDef.Invincible = true;
		buildingDef.Overheatable = false;
		buildingDef.Entombable = false;
		buildingDef.ViewMode = OverlayModes.GasConduits.ID;
		buildingDef.AudioCategory = "Metal";
		buildingDef.UtilityOutputOffset = primaryPort.offset;
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, "DevPumpGas");
		buildingDef.DebugOnly = true;
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		base.ConfigureBuildingTemplate(go, prefab_tag);
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddTag(GameTags.DevBuilding);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.AddOrGet<LoopingSounds>();
		DevPump devPump = go.AddOrGet<DevPump>();
		devPump.elementState = Filterable.ElementState.Gas;
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = 20f;
		go.AddTag(GameTags.CorrosionProof);
		ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		conduitDispenser.conduitType = ConduitType.Gas;
		conduitDispenser.alwaysDispense = true;
		conduitDispenser.elementFilter = null;
		go.AddOrGetDef<OperationalController.Def>();
		go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayBehindConduits);
	}
}
