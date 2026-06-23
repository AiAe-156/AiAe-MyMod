using TUNING;
using UnityEngine;

public class DevPumpSolidConfig : IBuildingConfig
{
	public const string ID = "DevPumpSolid";

	private const ConduitType CONDUIT_TYPE = ConduitType.Solid;

	private ConduitPortInfo primaryPort = new ConduitPortInfo(ConduitType.Solid, new CellOffset(1, 1));

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("DevPumpSolid", 2, 2, "dev_pump_solid_kanim", 100, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, MATERIALS.ALL_METALS, 9999f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NOISY.TIER2, decor: BUILDINGS.DECOR.PENALTY.TIER1);
		buildingDef.RequiresPowerInput = false;
		buildingDef.OutputConduitType = ConduitType.Solid;
		buildingDef.Floodable = false;
		buildingDef.Invincible = true;
		buildingDef.Overheatable = false;
		buildingDef.Entombable = false;
		buildingDef.ViewMode = OverlayModes.SolidConveyor.ID;
		buildingDef.AudioCategory = "Metal";
		buildingDef.UtilityOutputOffset = primaryPort.offset;
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.SolidConveyorIDs, "DevPumpSolid");
		buildingDef.DebugOnly = true;
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddTag(GameTags.DevBuilding);
		base.ConfigureBuildingTemplate(go, prefab_tag);
		GeneratedBuildings.MakeBuildingAlwaysOperational(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.AddOrGet<LoopingSounds>();
		DevPump devPump = go.AddOrGet<DevPump>();
		devPump.elementState = Filterable.ElementState.Solid;
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = 20f;
		go.AddTag(GameTags.CorrosionProof);
		SolidConduitDispenser solidConduitDispenser = go.AddOrGet<SolidConduitDispenser>();
		solidConduitDispenser.alwaysDispense = true;
		solidConduitDispenser.elementFilter = null;
		go.AddOrGetDef<OperationalController.Def>();
		go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayBehindConduits);
	}
}
