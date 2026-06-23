using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class HEPBatteryConfig : IBuildingConfig
{
	public const string ID = "HEPBattery";

	public const float MIN_LAUNCH_INTERVAL = 1f;

	public const int MIN_SLIDER = 0;

	public const int MAX_SLIDER = 100;

	public const float HEP_CAPACITY = 1000f;

	public const float DISABLED_DECAY_RATE = 0.05f;

	public const string STORAGE_PORT_ID = "HEP_STORAGE";

	public const string FIRE_PORT_ID = "HEP_FIRE";

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("HEPBattery", 3, 3, "radbolt_battery_kanim", 30, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.REFINED_METALS, 800f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.AudioCategory = "Metal";
		buildingDef.AudioSize = "large";
		buildingDef.Floodable = false;
		buildingDef.Overheatable = false;
		buildingDef.ViewMode = OverlayModes.Radiation.ID;
		buildingDef.UseHighEnergyParticleInputPort = true;
		buildingDef.HighEnergyParticleInputOffset = new CellOffset(0, 1);
		buildingDef.UseHighEnergyParticleOutputPort = true;
		buildingDef.HighEnergyParticleOutputOffset = new CellOffset(0, 2);
		buildingDef.RequiresPowerInput = true;
		buildingDef.PowerInputOffset = new CellOffset(0, 0);
		buildingDef.EnergyConsumptionWhenActive = 120f;
		buildingDef.ExhaustKilowattsWhenActive = 0.25f;
		buildingDef.SelfHeatKilowattsWhenActive = 1f;
		buildingDef.AddLogicPowerPort = true;
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.RadiationIDs, "HEPBattery");
		buildingDef.LogicOutputPorts = new List<LogicPorts.Port> { LogicPorts.Port.OutputPort("HEP_STORAGE", new CellOffset(1, 1), STRINGS.BUILDINGS.PREFABS.HEPBATTERY.LOGIC_PORT_STORAGE, STRINGS.BUILDINGS.PREFABS.HEPBATTERY.LOGIC_PORT_STORAGE_ACTIVE, STRINGS.BUILDINGS.PREFABS.HEPBATTERY.LOGIC_PORT_STORAGE_INACTIVE) };
		buildingDef.LogicInputPorts = new List<LogicPorts.Port> { LogicPorts.Port.InputPort(HEPBattery.FIRE_PORT_ID, new CellOffset(0, 2), STRINGS.BUILDINGS.PREFABS.HEPBATTERY.LOGIC_PORT, STRINGS.BUILDINGS.PREFABS.HEPBATTERY.LOGIC_PORT_ACTIVE, STRINGS.BUILDINGS.PREFABS.HEPBATTERY.LOGIC_PORT_INACTIVE) };
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		Prioritizable.AddRef(go);
		HighEnergyParticleStorage highEnergyParticleStorage = go.AddOrGet<HighEnergyParticleStorage>();
		highEnergyParticleStorage.capacity = 1000f;
		highEnergyParticleStorage.autoStore = true;
		highEnergyParticleStorage.PORT_ID = "HEP_STORAGE";
		highEnergyParticleStorage.showCapacityStatusItem = true;
		highEnergyParticleStorage.showCapacityAsMainStatus = true;
		go.AddOrGet<LoopingSounds>();
		HEPBattery.Def def = go.AddOrGetDef<HEPBattery.Def>();
		def.minLaunchInterval = 1f;
		def.minSlider = 0f;
		def.maxSlider = 100f;
		def.particleDecayRate = 0.05f;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
