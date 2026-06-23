using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class TubeWormConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "TubeWorm";

	public const string SEED_ID = "TubeWormSeed";

	public const float LIFETIME_CYCLES = 8f;

	public const int HARVEST_YIELD = 200;

	public const float SULFUR_CONSUMPTION_RATE = 1f / 30f;

	public const float MURKY_BRINE_CONSUMPTION_RATE = 0.05f;

	public const SimHashes PRODUCT_ELEMENT = SimHashes.Polypropylene;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("TubeWorm", STRINGS.CREATURES.SPECIES.TUBEWORM.NAME, STRINGS.CREATURES.SPECIES.TUBEWORM.DESC, 1f, decor: DECOR.BONUS.TIER1, anim: Assets.GetAnim("tube_worm_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 348.15f);
		gameObject = EntityTemplates.ExtendEntityToBasicPlant(gameObject, 303.15f, 323.15f, 383.15f, 403.15f, crop_id: SimHashes.Polypropylene.ToString(), safe_elements: PLANTS.SAFE_ELEMENTS.MurkyWaters, pressure_sensitive: false, pressure_lethal_low: 0f, pressure_warning_low: 0.15f, can_drown: false, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, max_age: 2400f, min_radiation: 0f, max_radiation: 2200f, baseTraitId: "TubeWormOriginal", baseTraitName: STRINGS.CREATURES.SPECIES.TUBEWORM.NAME);
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		gameObject.AddOrGet<LoopingSounds>();
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Sulfur.CreateTag(),
				massConsumptionRate = 1f / 30f
			}
		});
		EntityTemplates.ExtendPlantToIrrigated(gameObject, new PlantElementAbsorber.ConsumeInfo[2]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.MurkyBrine.CreateTag(),
				massConsumptionRate = 0.05f
			},
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Brine.CreateTag(),
				massConsumptionRate = 0.05f
			}
		});
		gameObject.AddOrGet<PressureVulnerable>().allCellsMustBeSafe = true;
		GameObject plant = gameObject;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.TUBEWORM.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.TUBEWORM.DESC;
		KAnimFile anim = Assets.GetAnim("seed_tube_worm_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.TUBEWORM.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(plant, this, SeedProducer.ProductionType.Harvest, "TubeWormSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 21, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), "TubeWorm_preview", Assets.GetAnim("tube_worm_kanim"), "place", 1, 2);
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
