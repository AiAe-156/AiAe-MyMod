using TUNING;
using UnityEngine;

public class DevLifeSupportConfig : IBuildingConfig
{
	public const string ID = "DevLifeSupport";

	private const float OXYGEN_GENERATION_RATE = 50.000004f;

	private const float OXYGEN_TEMPERATURE = 303.15f;

	private const float OXYGEN_MAX_PRESSURE = 1.5f;

	private const float CO2_CONSUMPTION_RATE = 50.000004f;

	public override BuildingDef CreateBuildingDef()
	{
		BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef("DevLifeSupport", 1, 1, "dev_life_support_kanim", 30, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER5, MATERIALS.RAW_MINERALS, 800f, BuildLocationRule.Anywhere, noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.PENALTY.TIER3);
		buildingDef.Floodable = false;
		buildingDef.Overheatable = false;
		buildingDef.AudioCategory = "HollowMetal";
		buildingDef.AudioSize = "large";
		buildingDef.DebugOnly = true;
		return buildingDef;
	}

	public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
	{
		go.AddTag(GameTags.DevBuilding);
		Storage storage = BuildingTemplates.CreateDefaultStorage(go);
		storage.showInUI = true;
		storage.capacityKg = 200f;
		storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
		CellOffset cellOffset = new CellOffset(0, 1);
		ElementEmitter elementEmitter = go.AddOrGet<ElementEmitter>();
		elementEmitter.outputElement = new ElementConverter.OutputElement(50.000004f, SimHashes.Oxygen, 303.15f, useEntityTemperature: false, storeOutput: false, cellOffset.x, cellOffset.y);
		elementEmitter.emissionFrequency = 1f;
		elementEmitter.maxPressure = 1.5f;
		ElementConsumer elementConsumer = go.AddOrGet<PassiveElementConsumer>();
		elementConsumer.elementToConsume = SimHashes.CarbonDioxide;
		elementConsumer.consumptionRate = 50.000004f;
		elementConsumer.capacityKG = 50.000004f;
		elementConsumer.consumptionRadius = 10;
		elementConsumer.showInStatusPanel = true;
		elementConsumer.sampleCellOffset = new Vector3(0f, 0f, 0f);
		elementConsumer.isRequired = false;
		elementConsumer.storeOnConsume = false;
		elementConsumer.showDescriptor = false;
		elementConsumer.ignoreActiveChanged = true;
		go.AddOrGet<DevLifeSupport>();
	}

	public override void DoPostConfigureComplete(GameObject go)
	{
	}
}
