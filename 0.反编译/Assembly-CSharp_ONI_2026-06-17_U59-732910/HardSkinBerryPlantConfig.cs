using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class HardSkinBerryPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "HardSkinBerryPlant";

	public const string SEED_ID = "HardSkinBerryPlantSeed";

	public const float Temperature_lethal_low = 118.149994f;

	public const float Temperature_warning_low = 218.15f;

	public const float Temperature_lethal_high = 269.15f;

	public const float Temperature_warning_high = 259.15f;

	public const float FERTILIZATION_RATE = 1f / 120f;

	public const float PLANT_FIBER_PRODUCED_PER_CYCLE = 12f;

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("HardSkinBerryPlant", STRINGS.CREATURES.SPECIES.HARDSKINBERRYPLANT.NAME, STRINGS.CREATURES.SPECIES.HARDSKINBERRYPLANT.DESC, 1f, decor: DECOR.PENALTY.TIER1, anim: Assets.GetAnim("ice_berry_bush_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingBack, width: 1, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 255f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 118.149994f, 218.15f, 259.15f, 269.15f, new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, pressure_sensitive: true, 0f, 0.15f, "HardSkinBerry", can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 4600f, "HardSkinBerryPlantOriginal", STRINGS.CREATURES.SPECIES.HARDSKINBERRYPLANT.NAME);
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		gameObject.AddOrGet<LoopingSounds>();
		string name = STRINGS.CREATURES.SPECIES.SEEDS.HARDSKINBERRYPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.HARDSKINBERRYPLANT.DESC;
		KAnimFile anim = Assets.GetAnim("seed_ice_berry_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.HARDSKINBERRYPLANT.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Harvest, "HardSkinBerryPlantSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 1, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f);
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Phosphorite.CreateTag(),
				massConsumptionRate = 1f / 120f
			}
		});
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "HardSkinBerryPlant_preview", Assets.GetAnim("ice_berry_bush_kanim"), "place", 1, 2);
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
