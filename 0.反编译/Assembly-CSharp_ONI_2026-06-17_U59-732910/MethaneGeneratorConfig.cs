using STRINGS;
using TUNING;
using UnityEngine;

public class MethaneGeneratorConfig : IBuildingConfig
{
	public const string ID = "MethaneGenerator";

	public const float FUEL_CONSUMPTION_RATE = 0.09f;

	private const float CO2_RATIO = 0.25f;

	public const float WATER_OUTPUT_TEMPERATURE = 313.15f;

	private const int WIDTH = 4;

	private const int HEIGHT = 3;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef obj = BuildingTemplates.CreateBuildingDef("MethaneGenerator", 4, 3, "generatormethane_kanim", 100, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER5, MATERIALS.RAW_METALS, 2400f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER5, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		obj.GeneratorWattageRating = 800f;
		obj.GeneratorBaseCapacity = obj.GeneratorWattageRating;
		obj.ExhaustKilowattsWhenActive = 2f;
		obj.SelfHeatKilowattsWhenActive = 8f;
		obj.ViewMode = OverlayModes.Power.ID;
		obj.AudioCategory = "Metal";
		obj.UtilityInputOffset = new CellOffset(0, 0);
		obj.UtilityOutputOffset = new CellOffset(2, 2);
		obj.RequiresPowerOutput = true;
		obj.PowerOutputOffset = new CellOffset(0, 0);
		obj.InputConduitType = ConduitType.Gas;
		obj.OutputConduitType = ConduitType.Gas;
		obj.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		obj.AddSearchTerms(SEARCH_TERMS.POWER);
		obj.AddSearchTerms(SEARCH_TERMS.GENERATOR);
		return obj;
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.GeneratorType);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.HeavyDutyGeneratorType);
		go.AddOrGet<LoopingSounds>();
		go.AddOrGet<Storage>().capacityKg = 50f;
		ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
		conduitConsumer.conduitType = ConduitType.Gas;
		conduitConsumer.consumptionRate = 0.90000004f;
		conduitConsumer.capacityTag = GameTags.CombustibleGas;
		conduitConsumer.capacityKG = 0.90000004f;
		conduitConsumer.forceAlwaysSatisfied = true;
		conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;
		EnergyGenerator energyGenerator = go.AddOrGet<EnergyGenerator>();
		energyGenerator.powerDistributionOrder = 8;
		energyGenerator.ignoreBatteryRefillPercent = true;
		energyGenerator.formula = new EnergyGenerator.Formula
		{
			inputs = new EnergyGenerator.InputItem[1]
			{
				new EnergyGenerator.InputItem(GameTags.Methane, 0.09f, 0.90000004f)
			},
			outputs = new EnergyGenerator.OutputItem[2]
			{
				new EnergyGenerator.OutputItem(SimHashes.DirtyWater, 0.0675f, store: false, new CellOffset(1, 1), 313.15f),
				new EnergyGenerator.OutputItem(SimHashes.CarbonDioxide, 0.0225f, store: true, new CellOffset(0, 2), 383.15f)
			}
		};
		ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
		conduitDispenser.conduitType = ConduitType.Gas;
		conduitDispenser.invertElementFilter = true;
		conduitDispenser.elementFilter = new SimHashes[2]
		{
			SimHashes.Methane,
			SimHashes.Syngas
		};
		Tinkerable.MakePowerTinkerable(go);
		go.AddOrGetDef<PoweredActiveController.Def>();
	}
}
