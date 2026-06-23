using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class BlueGrassConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "BlueGrass";

	public const string SEED_ID = "BlueGrassSeed";

	public const float CO2_RATE = 0.002f;

	public const float FERTILIZATION_RATE = 20f;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC2;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("BlueGrass", STRINGS.CREATURES.SPECIES.BLUE_GRASS.NAME, STRINGS.CREATURES.SPECIES.BLUE_GRASS.DESC, 2f, decor: DECOR.BONUS.TIER1, anim: Assets.GetAnim("bluegrass_kanim"), initialAnim: "idle_full", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 240f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 193.15f, 193.15f, 273.15f, 273.15f, baseTraitName: STRINGS.CREATURES.SPECIES.BLUE_GRASS.NAME, safe_elements: new SimHashes[1] { SimHashes.CarbonDioxide }, pressure_sensitive: true, pressure_lethal_low: 0f, pressure_warning_low: 0f, crop_id: "OxyRock", can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, max_age: 2400f, min_radiation: 0f, max_radiation: 2200f, baseTraitId: "BlueGrassOriginal");
		ElementConsumer elementConsumer = gameObject.AddOrGet<ElementConsumer>();
		elementConsumer.showInStatusPanel = true;
		elementConsumer.storeOnConsume = false;
		elementConsumer.elementToConsume = SimHashes.CarbonDioxide;
		elementConsumer.configuration = ElementConsumer.Configuration.Element;
		elementConsumer.consumptionRadius = 2;
		elementConsumer.EnableConsumption(enabled: true);
		elementConsumer.sampleCellOffset = new Vector3(0f, 0f);
		elementConsumer.consumptionRate = 0.0005f;
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Ice.CreateTag(),
				massConsumptionRate = 1f / 30f
			}
		});
		gameObject.GetComponent<UprootedMonitor>();
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<BlueGrass>();
		string name = STRINGS.CREATURES.SPECIES.SEEDS.BLUE_GRASS.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.BLUE_GRASS.DESC;
		KAnimFile anim = Assets.GetAnim("seed_bluegrass_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.BLUE_GRASS.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Harvest, "BlueGrassSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 4, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), "BlueGrass_preview", Assets.GetAnim("bluegrass_kanim"), "place", 1, 1);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
