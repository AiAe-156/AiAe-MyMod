using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ColdWheatConfig : IEntityConfig
{
	public const string ID = "ColdWheat";

	public const string SEED_ID = "ColdWheatSeed";

	public const float FERTILIZATION_RATE = 1f / 120f;

	public const float WATER_RATE = 1f / 30f;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("ColdWheat", STRINGS.CREATURES.SPECIES.COLDWHEAT.NAME, STRINGS.CREATURES.SPECIES.COLDWHEAT.DESC, 1f, decor: DECOR.BONUS.TIER1, anim: Assets.GetAnim("coldwheat_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 1, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 255f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 118.149994f, 218.15f, 278.15f, 358.15f, new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, pressure_sensitive: true, 0f, 0.15f, "ColdWheatSeed", can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 12200f, "ColdWheatOriginal", STRINGS.CREATURES.SPECIES.COLDWHEAT.NAME);
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = GameTags.Dirt,
				massConsumptionRate = 1f / 120f
			}
		});
		EntityTemplates.ExtendPlantToIrrigated(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = GameTags.Water,
				massConsumptionRate = 1f / 30f
			}
		});
		gameObject.AddOrGet<StandardCropPlant>();
		IHasDlcRestrictions dlcRestrictions = this as IHasDlcRestrictions;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.COLDWHEAT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.COLDWHEAT.DESC;
		KAnimFile anim = Assets.GetAnim("seed_coldwheat_kanim");
		EdiblesManager.FoodInfo cOLD_WHEAT_SEED = FOOD.FOOD_TYPES.COLD_WHEAT_SEED;
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.COLDWHEAT.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlantAsFood(gameObject, dlcRestrictions, SeedProducer.ProductionType.Crop, "ColdWheatSeed", name, desc, anim, cOLD_WHEAT_SEED, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 3, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.2f, 0.2f, null, "", ignoreDefaultSeedTag: true), "ColdWheat_preview", Assets.GetAnim("coldwheat_kanim"), "place", 1, 1);
		SoundEventVolumeCache.instance.AddVolume("coldwheat_kanim", "ColdWheat_grow", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("coldwheat_kanim", "ColdWheat_harvest", NOISE_POLLUTION.CREATURES.TIER3);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
