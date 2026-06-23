using STRINGS;
using TUNING;
using UnityEngine;

public class PeatGeneratorConfig : IBuildingConfig
{
	public const string ID = "PeatGenerator";

	private const float PEAT_BURN_RATE = 1f;

	public const float EXHAUST_LIQUID_RATE = 0.2f;

	public const float EXHAUST_GAS_RATE = 0.04f;

	private const float PEAT_CAPACITY = 600f;

	public const float CO2_OUTPUT_TEMPERATURE = 383.15f;

	public const float LIQUID_OUTPUT_TEMPERATURE = 313.15f;

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC4;
	}

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("PeatGenerator", 3, 2, "generatorpeat_kanim", 100, 120f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER5, MATERIALS.ALL_METALS, 2400f, BuildLocationRule.OnFloor, noise: NOISE_POLLUTION.NOISY.TIER5, decor: TUNING.BUILDINGS.DECOR.PENALTY.TIER2);
		buildingDef.GeneratorWattageRating = 480f;
		buildingDef.GeneratorBaseCapacity = buildingDef.GeneratorWattageRating;
		buildingDef.ExhaustKilowattsWhenActive = 4f;
		buildingDef.SelfHeatKilowattsWhenActive = 0.5f;
		buildingDef.RequiresPowerOutput = true;
		buildingDef.PowerOutputOffset = new CellOffset(0, 0);
		buildingDef.ViewMode = OverlayModes.Power.ID;
		buildingDef.AudioCategory = "HollowMetal";
		buildingDef.AudioSize = "large";
		buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
		buildingDef.AddSearchTerms(SEARCH_TERMS.POWER);
		buildingDef.AddSearchTerms(SEARCH_TERMS.GENERATOR);
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.GeneratorType);
		go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.HeavyDutyGeneratorType);
		EnergyGenerator energyGenerator = go.AddOrGet<EnergyGenerator>();
		energyGenerator.formula = new EnergyGenerator.Formula
		{
			inputs = new EnergyGenerator.InputItem[1]
			{
				new EnergyGenerator.InputItem(SimHashes.Peat.CreateTag(), 1f, 600f)
			},
			outputs = new EnergyGenerator.OutputItem[2]
			{
				new EnergyGenerator.OutputItem(SimHashes.CarbonDioxide, 0.04f, store: false, new CellOffset(0, 1), 383.15f),
				new EnergyGenerator.OutputItem(SimHashes.DirtyWater, 0.2f, store: false, new CellOffset(1, 1), 313.15f)
			}
		};
		energyGenerator.meterOffset = Meter.Offset.Infront;
		energyGenerator.SetSliderValue(50f, 0);
		energyGenerator.powerDistributionOrder = 9;
		Storage storage = go.AddOrGet<Storage>();
		storage.capacityKg = 600f;
		go.AddOrGet<LoopingSounds>();
		Prioritizable.AddRef(go);
		ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
		manualDeliveryKG.SetStorage(storage);
		manualDeliveryKG.RequestedItemTag = SimHashes.Peat.CreateTag();
		manualDeliveryKG.capacity = storage.capacityKg;
		manualDeliveryKG.refillMass = 100f;
		manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.PowerFetch.IdHash;
		Tinkerable.MakePowerTinkerable(go);
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
		go.AddOrGet<LogicOperationalController>();
		go.AddOrGetDef<PoweredActiveController.Def>();
	}
}
