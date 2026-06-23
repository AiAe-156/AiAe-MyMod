using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class PlanktonCoralConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "PlanktonCoral";

	public const string SEED_ID = "PlanktonCoralSeed";

	public const float LIFETIME_CYCLES = 4f;

	public const int HARVEST_YIELD_MASS = 80;

	public const float CALCULATED_YIELD_MASS_PER_HARVEST = 80f;

	public const float CALCULATED_YIELD_MASS_PER_CYCLE = 20f;

	public const float CALCULATED_GROWTH_PER_CYCLE = 0.25f;

	public const float CALCULATED_LIFETIME_SEC = 2400f;

	public const float CALCIUM_CARBONATE_CONSUMPTION_RATE = 0.025f;

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("PlanktonCoral", STRINGS.CREATURES.SPECIES.PLANKTONCORAL.NAME, STRINGS.CREATURES.SPECIES.PLANKTONCORAL.DESC, 1f, decor: DECOR.BONUS.TIER1, anim: Assets.GetAnim("parrotfish_coral_kanim"), initialAnim: "idle_full", sceneLayer: Grid.SceneLayer.Building, width: 2, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 303.15f);
		gameObject = EntityTemplates.ExtendEntityToBasicPlant(gameObject, 273.15f, 298.15f, 318.15f, 373.15f, crop_id: SimHashes.Phosphorite.ToString(), safe_elements: PLANTS.SAFE_ELEMENTS.AllWaters, pressure_sensitive: false, pressure_lethal_low: 0f, pressure_warning_low: 0.15f, can_drown: false, can_tinker: true, require_solid_tile: false, require_Backwall_Foundation: true, should_grow_old: true, max_age: 2400f, min_radiation: 0f, max_radiation: 2200f, baseTraitId: "PlanktonCoralOriginal", baseTraitName: STRINGS.CREATURES.SPECIES.PLANKTONCORAL.NAME);
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<LoopingSounds>();
		GameObject plant = gameObject;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.PLANKTONCORAL.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.PLANKTONCORAL.DESC;
		KAnimFile anim = Assets.GetAnim("seed_parrotfish_coral_kanim");
		List<Tag> additionalTags = new List<Tag>
		{
			GameTags.CropSeed,
			GameTags.BackwallSeed
		};
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.PLANKTONCORAL.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(plant, this, SeedProducer.ProductionType.Harvest, "PlanktonCoralSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 20, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), "PlanktonCoral_preview", Assets.GetAnim("parrotfish_coral_kanim"), "place", 2, 2);
		PlantElementAbsorber.ConsumeInfo[] fertilizers = new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Coquina.CreateTag(),
				massConsumptionRate = 1f / 30f
			}
		};
		EntityTemplates.ExtendPlantToFertilizable(gameObject, fertilizers);
		gameObject.AddOrGet<PressureVulnerable>().allCellsMustBeSafe = true;
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
		EntityTemplates.ExtendPlantEntityToRequireBackwall(prefab);
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
