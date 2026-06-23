using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class SeaLettuceConfig : IEntityConfig
{
	public static string ID = "SeaLettuce";

	public const string SEED_ID = "SeaLettuceSeed";

	public const string CROP_ID = "Lettuce";

	public const int GROWTH_CYCLES = 12;

	public const int YIELD_UNITS_PER_HARVEST = 12;

	public const float CALCULATED_YIELD_MASS_PER_HARVEST = 12f;

	public const float CALCULATED_YIELD_MASS_PER_CYCLE = 1f;

	public const float CALCULATED_GROWTH_PER_CYCLE = 1f / 12f;

	public const float WATER_RATE = 1f / 60f;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(ID, STRINGS.CREATURES.SPECIES.SEALETTUCE.NAME, STRINGS.CREATURES.SPECIES.SEALETTUCE.DESC, 1f, decor: DECOR.BONUS.TIER0, anim: Assets.GetAnim("sea_lettuce_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingBack, width: 1, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 308.15f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 248.15f, 295.15f, 338.15f, 398.15f, new SimHashes[3]
		{
			SimHashes.Water,
			SimHashes.SaltWater,
			SimHashes.Brine
		}, pressure_sensitive: false, 0f, 0.15f, "Lettuce", can_drown: false, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 7400f, ID + "Original", STRINGS.CREATURES.SPECIES.SEALETTUCE.NAME);
		EntityTemplates.ExtendPlantToIrrigated(gameObject, new PlantElementAbsorber.ConsumeInfo[3]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.SaltWater.CreateTag(),
				massConsumptionRate = 1f / 60f
			},
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Water.CreateTag(),
				massConsumptionRate = 1f / 60f
			},
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Brine.CreateTag(),
				massConsumptionRate = 1f / 60f
			}
		});
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		gameObject.AddOrGet<LoopingSounds>();
		IHasDlcRestrictions dlcRestrictions = this as IHasDlcRestrictions;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.SEALETTUCE.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.SEALETTUCE.DESC;
		KAnimFile anim = Assets.GetAnim("seed_sealettuce_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.WaterSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.SEALETTUCE.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, dlcRestrictions, SeedProducer.ProductionType.Harvest, "SeaLettuceSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 3, domesticatedDescription);
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, ID + "_preview", Assets.GetAnim("sea_lettuce_kanim"), "place", 1, 2);
		SoundEventVolumeCache.instance.AddVolume("sea_lettuce_kanim", "SeaLettuce_grow", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("sea_lettuce_kanim", "SeaLettuce_harvest", NOISE_POLLUTION.CREATURES.TIER3);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
