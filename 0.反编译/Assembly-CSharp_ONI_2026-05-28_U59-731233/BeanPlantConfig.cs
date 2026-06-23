using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class BeanPlantConfig : IEntityConfig
{
	public const string ID = "BeanPlant";

	public const string SEED_ID = "BeanPlantSeed";

	public const float FERTILIZATION_RATE = 1f / 120f;

	public const float WATER_RATE = 1f / 30f;

	public const float PLANT_FIBER_PRODUCED_PER_CYCLE = 42f;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("BeanPlant", STRINGS.CREATURES.SPECIES.BEAN_PLANT.NAME, STRINGS.CREATURES.SPECIES.BEAN_PLANT.DESC, 2f, decor: DECOR.BONUS.TIER1, anim: Assets.GetAnim("beanplant_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 258.15f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 198.15f, 248.15f, 273.15f, 323.15f, baseTraitName: STRINGS.CREATURES.SPECIES.BEAN_PLANT.NAME, safe_elements: new SimHashes[1] { SimHashes.CarbonDioxide }, pressure_sensitive: true, pressure_lethal_low: 0f, pressure_warning_low: 0.025f, crop_id: "BeanPlantSeed", can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, max_age: 2400f, min_radiation: 0f, max_radiation: 9800f, baseTraitId: "BeanPlantOriginal");
		EntityTemplates.ExtendPlantToIrrigated(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Ethanol.CreateTag(),
				massConsumptionRate = 1f / 30f
			}
		});
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Dirt.CreateTag(),
				massConsumptionRate = 1f / 120f
			}
		});
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		PlantFiberProducer plantFiberProducer = gameObject.AddOrGet<PlantFiberProducer>();
		plantFiberProducer.amount = 42f;
		IHasDlcRestrictions dlcRestrictions = this as IHasDlcRestrictions;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.BEAN_PLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.BEAN_PLANT.DESC;
		KAnimFile anim = Assets.GetAnim("seed_beanplant_kanim");
		EdiblesManager.FoodInfo bEAN = FOOD.FOOD_TYPES.BEAN;
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.BEAN_PLANT.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlantAsFood(gameObject, dlcRestrictions, SeedProducer.ProductionType.Crop, "BeanPlantSeed", name, desc, anim, bEAN, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 3, domesticatedDescription, EntityTemplates.CollisionShape.RECTANGLE, 0.6f, 0.3f, null, "", ignoreDefaultSeedTag: true);
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "BeanPlant_preview", Assets.GetAnim("beanplant_kanim"), "place", 1, 2);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
