using STRINGS;
using TUNING;
using UnityEngine;

public class HighEnergyParticleSpawnerConfig : IBuildingConfig
{
	public const string ID = "HighEnergyParticleSpawner";

	public const float MIN_LAUNCH_INTERVAL = 2f;

	public const float RADIATION_SAMPLE_RATE = 0.2f;

	public const float HEP_PER_RAD = 0.1f;

	public const int MIN_SLIDER = 50;

	public const int MAX_SLIDER = 500;

	public const float DISABLED_CONSUMPTION_RATE = 0.05f;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("HighEnergyParticleSpawner", 1, 2, "radiation_collector_kanim", 30, 10f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.RAW_MINERALS, 1600f, BuildLocationRule.NotInTiles, noise: NOISE_POLLUTION.NONE, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER1);
		buildingDef.Floodable = false;
		buildingDef.AudioCategory = "Metal";
		buildingDef.Overheatable = false;
		buildingDef.ViewMode = OverlayModes.Radiation.ID;
		buildingDef.PermittedRotations = PermittedRotations.R360;
		buildingDef.UseHighEnergyParticleOutputPort = true;
		buildingDef.HighEnergyParticleOutputOffset = new CellOffset(0, 1);
		buildingDef.RequiresPowerInput = true;
		buildingDef.PowerInputOffset = new CellOffset(0, 0);
		buildingDef.EnergyConsumptionWhenActive = 480f;
		buildingDef.ExhaustKilowattsWhenActive = 1f;
		buildingDef.SelfHeatKilowattsWhenActive = 4f;
		buildingDef.DiseaseCellVisName = "RadiationSickness";
		buildingDef.UtilityOutputOffset = CellOffset.none;
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.RadiationIDs, "HighEnergyParticleSpawner");
		buildingDef.Deprecated = !Sim.IsRadiationEnabled();
		buildingDef.AddSearchTerms(SEARCH_TERMS.POWER);
		buildingDef.AddSearchTerms(SEARCH_TERMS.GENERATOR);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		Prioritizable.AddRef(go);
		HighEnergyParticleStorage highEnergyParticleStorage = go.AddOrGet<HighEnergyParticleStorage>();
		highEnergyParticleStorage.capacity = 500f;
		go.AddOrGet<LoopingSounds>();
		HighEnergyParticleSpawner highEnergyParticleSpawner = go.AddOrGet<HighEnergyParticleSpawner>();
		highEnergyParticleSpawner.minLaunchInterval = 2f;
		highEnergyParticleSpawner.radiationSampleRate = 0.2f;
		highEnergyParticleSpawner.minSlider = 50;
		highEnergyParticleSpawner.maxSlider = 500;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
