using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class CarrotPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "CarrotPlant";

	public const string SEED_ID = "CarrotPlantSeed";

	public const float Temperature_lethal_low = 118.149994f;

	public const float Temperature_warning_low = 218.15f;

	public const float Temperature_lethal_high = 269.15f;

	public const float Temperature_warning_high = 259.15f;

	public const float FERTILIZATION_RATE = 0.025f;

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("CarrotPlant", STRINGS.CREATURES.SPECIES.CARROTPLANT.NAME, STRINGS.CREATURES.SPECIES.CARROTPLANT.DESC, 1f, decor: DECOR.PENALTY.TIER1, anim: Assets.GetAnim("purpleroot_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingBack, width: 1, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 255f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 118.149994f, 218.15f, 259.15f, 269.15f, crop_id: CarrotConfig.ID, safe_elements: new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, pressure_sensitive: true, pressure_lethal_low: 0f, pressure_warning_low: 0.15f, can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, max_age: 2400f, min_radiation: 0f, max_radiation: 4600f, baseTraitId: "CarrotPlantOriginal", baseTraitName: STRINGS.CREATURES.SPECIES.CARROTPLANT.NAME);
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		gameObject.AddOrGet<LoopingSounds>();
		string name = STRINGS.CREATURES.SPECIES.SEEDS.CARROTPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.CARROTPLANT.DESC;
		KAnimFile anim = Assets.GetAnim("seed_purpleroot_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.CARROTPLANT.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Harvest, "CarrotPlantSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 1, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f);
		EntityTemplates.ExtendPlantToIrrigated(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Ethanol.CreateTag(),
				massConsumptionRate = 0.025f
			}
		});
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "CarrotPlant_preview", Assets.GetAnim("purpleroot_kanim"), "place", 1, 2);
		SoundEventVolumeCache.instance.AddVolume("bristleblossom_kanim", "PrickleFlower_harvest", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("bristleblossom_kanim", "PrickleFlower_grow", NOISE_POLLUTION.CREATURES.TIER3);
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
