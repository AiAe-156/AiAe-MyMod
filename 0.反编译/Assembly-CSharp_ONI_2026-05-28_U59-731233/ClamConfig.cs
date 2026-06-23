using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ClamConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Clam";

	public const string SEED_ID = "ClamSeed";

	public const float LIFETIME_CYCLES = 8f;

	public const int HARVEST_YIELD_MASS = 50;

	public const float SAND_CONSUMPTION_RATE = 7f / 120f;

	public static StandardCropPlant.AnimSet CROP_PLANT_DEFAULT_ANIM_SET = new StandardCropPlant.AnimSet(StandardCropPlant.defaultAnimSet)
	{
		wilt_recover_base = "wilt_recover"
	};

	public static StandardCropPlant.AnimSet CROP_PLANT_CLOSED_ANIM_SET = new StandardCropPlant.AnimSet(StandardCropPlant.defaultAnimSet)
	{
		grow_pst = "idle_full_closed",
		idle_full = "idle_full_closed",
		wilt_recover_base = "wilt_recover"
	};

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("Clam", STRINGS.CREATURES.SPECIES.CLAM.NAME, STRINGS.CREATURES.SPECIES.CLAM.DESC, 1f, decor: DECOR.BONUS.TIER2, anim: Assets.GetAnim("clam_kanim"), initialAnim: "idle_full", sceneLayer: Grid.SceneLayer.BuildingBack, width: 3, height: 3, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 303.15f);
		gameObject.AddOrGet<ClamHarvestable>();
		gameObject = EntityTemplates.ExtendEntityToBasicPlant(gameObject, 273.15f, 298.15f, 318.15f, 373.15f, crop_id: SimHashes.Pearl.ToString(), safe_elements: PLANTS.SAFE_ELEMENTS.AllWaters, pressure_sensitive: false, pressure_lethal_low: 0f, pressure_warning_low: 0.15f, can_drown: false, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, max_age: 2400f, min_radiation: 0f, max_radiation: 2200f, baseTraitId: "ClamOriginal", baseTraitName: STRINGS.CREATURES.SPECIES.PLANKTONCORAL.NAME);
		StandardCropPlant standardCropPlant = gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<LoopingSounds>();
		GameObject plant = gameObject;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.CLAM.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.CLAM.DESC;
		KAnimFile anim = Assets.GetAnim("seed_clam_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.LargeSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.CLAM.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(plant, this, SeedProducer.ProductionType.Hidden, "ClamSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 20, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f);
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "Clam_preview", Assets.GetAnim("clam_kanim"), "place", 3, 3);
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Sand.CreateTag(),
				massConsumptionRate = 7f / 120f
			}
		});
		PressureVulnerable pressureVulnerable = gameObject.AddOrGet<PressureVulnerable>();
		pressureVulnerable.allCellsMustBeSafe = true;
		gameObject.AddOrGet<ClamPoopStation>();
		Growing component = gameObject.GetComponent<Growing>();
		component.shouldGrowOld = false;
		UprootedMonitor component2 = gameObject.GetComponent<UprootedMonitor>();
		component2.monitorCells = new CellOffset[3]
		{
			new CellOffset(0, -1),
			new CellOffset(-1, -1),
			new CellOffset(1, -1)
		};
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
		StandardCropPlant component = prefab.GetComponent<StandardCropPlant>();
		component.anims = CROP_PLANT_DEFAULT_ANIM_SET;
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
