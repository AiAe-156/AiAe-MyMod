using TUNING;
using UnityEngine;

public class LiquidHeaterConfig : IBuildingConfig
{
	public const string ID = "LiquidHeater";

	public const float MAX_SELF_HEAT = 64f;

	public const float MAX_EXHAUST_HEAT = 4000f;

	public const float MIN_POWER_USAGE = 960f;

	public const float MAX_POWER_USAGE = 4000f;

	public const float BUBBLE_POWER_THRESHOLD = 961f;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("LiquidHeater", 4, 1, "boiler_kanim", 30, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 3200f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.PENALTY.TIER1);
		buildingDef.RequiresPowerInput = true;
		buildingDef.Floodable = false;
		buildingDef.EnergyConsumptionWhenActive = 960f;
		buildingDef.ExhaustKilowattsWhenActive = 0f;
		buildingDef.SelfHeatKilowattsWhenActive = 0f;
		buildingDef.ViewMode = OverlayModes.Power.ID;
		buildingDef.AudioCategory = "SolidMetal";
		buildingDef.OverheatTemperature = 398.15f;
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(1, 0));
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddOrGet<LoopingSounds>();
		go.AddOrGet<KBatchedAnimHeatPostProcessingEffect>();
		SpaceHeater spaceHeater = go.AddOrGet<SpaceHeater>();
		spaceHeater.SetLiquidHeater();
		spaceHeater.produceHeat = true;
		spaceHeater.hasTargetTemperature = false;
		spaceHeater.minimumCellMass = 400f;
		spaceHeater.maxPower = 4000f;
		spaceHeater.minPower = 960f;
		spaceHeater.maxSelfHeatKWs = 64f;
		spaceHeater.maxExhaustedKWs = 4000f;
		go.AddOrGet<LiquidHeaterBubbleEmitter>().BubblePowerThreshold = 961f;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
		go.AddOrGetDef<PoweredActiveController.Def>();
	}
}
