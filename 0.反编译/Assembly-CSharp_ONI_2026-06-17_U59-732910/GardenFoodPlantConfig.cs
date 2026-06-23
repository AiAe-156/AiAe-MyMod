using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class GardenFoodPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "GardenFoodPlant";

	public const string SEED_ID = "GardenFoodPlantSeed";

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC4;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("GardenFoodPlant", STRINGS.CREATURES.SPECIES.GARDENFOODPLANT.NAME, STRINGS.CREATURES.SPECIES.GARDENFOODPLANT.DESC, 1f, decor: DECOR.PENALTY.TIER1, anim: Assets.GetAnim("spike_fruit_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingBack, width: 1, height: 2);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 263.15f, 268.15f, 313.15f, 323.15f, new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, pressure_sensitive: true, 0f, 0.15f, "GardenFoodPlantFood", can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 4600f, "GardenFoodPlantOriginal", STRINGS.CREATURES.SPECIES.GARDENFOODPLANT.NAME);
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGetDef<PollinationMonitor.Def>();
		string name = STRINGS.CREATURES.SPECIES.SEEDS.GARDENFOODPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.GARDENFOODPLANT.DESC;
		KAnimFile anim = Assets.GetAnim("seed_spikefruit_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.GARDENFOODPLANT.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Harvest, "GardenFoodPlantSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 1, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f);
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Peat.CreateTag(),
				massConsumptionRate = 1f / 60f
			}
		});
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "GardenFoodPlant_preview", Assets.GetAnim("spike_fruit_kanim"), "place", 1, 2);
		SoundEventVolumeCache.instance.AddVolume("spike_fruit_kanim", "spike_fruit_harvest", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("spike_fruit_kanim", "spike_fruit_LP", NOISE_POLLUTION.CREATURES.TIER4);
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
