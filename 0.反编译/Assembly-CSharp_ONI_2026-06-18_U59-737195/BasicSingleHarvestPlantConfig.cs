using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class BasicSingleHarvestPlantConfig : IEntityConfig
{
	public const string ID = "BasicSingleHarvestPlant";

	public const string SEED_ID = "BasicSingleHarvestPlantSeed";

	public const float DIRT_RATE = 1f / 60f;

	public const float PLANT_FIBER_PRODUCED_PER_CYCLE = 12f;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("BasicSingleHarvestPlant", STRINGS.CREATURES.SPECIES.BASICSINGLEHARVESTPLANT.NAME, STRINGS.CREATURES.SPECIES.BASICSINGLEHARVESTPLANT.DESC, 1f, decor: DECOR.PENALTY.TIER1, anim: Assets.GetAnim("meallice_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingBack, width: 1, height: 2);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 218.15f, 283.15f, 303.15f, 398.15f, new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, pressure_sensitive: true, 0f, 0.15f, "BasicPlantFood", can_drown: true, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 4600f, "BasicSingleHarvestPlantOriginal", STRINGS.CREATURES.SPECIES.BASICSINGLEHARVESTPLANT.NAME);
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		IHasDlcRestrictions dlcRestrictions = this as IHasDlcRestrictions;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.BASICSINGLEHARVESTPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.BASICSINGLEHARVESTPLANT.DESC;
		KAnimFile anim = Assets.GetAnim("seed_meallice_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.BASICSINGLEHARVESTPLANT.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, dlcRestrictions, SeedProducer.ProductionType.Harvest, "BasicSingleHarvestPlantSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 1, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f);
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = GameTags.Dirt,
				massConsumptionRate = 1f / 60f
			}
		});
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "BasicSingleHarvestPlant_preview", Assets.GetAnim("meallice_kanim"), "place", 1, 2);
		gameObject.AddOrGet<PlantFiberProducer>().amount = 12f;
		SoundEventVolumeCache.instance.AddVolume("meallice_kanim", "MealLice_harvest", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("meallice_kanim", "MealLice_LP", NOISE_POLLUTION.CREATURES.TIER4);
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
