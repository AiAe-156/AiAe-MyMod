using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class KelpPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "KelpPlant";

	public const string SEED_ID = "KelpPlantSeed";

	public const int YIELD_UNITS_PER_HARVEST = 50;

	public const float LIFETIME_CYCLES = 5f;

	public const float FERTILIZATION_RATE = 1f / 60f;

	public static SimHashes[] ALLOWED_ELEMENTS = new SimHashes[7]
	{
		SimHashes.Water,
		SimHashes.DirtyWater,
		SimHashes.SaltWater,
		SimHashes.Brine,
		SimHashes.MurkyBrine,
		SimHashes.PhytoOil,
		SimHashes.NaturalResin
	};

	public const float CALCULATED_YIELD_MASS_PER_HARVEST = 50f;

	public const float CALCULATED_YIELD_MASS_PER_CYCLE = 10f;

	public const float CALCULATED_GROWTH_PER_CYCLE = 0.2f;

	public const float CALCULATED_LIFETIME_SEC = 3000f;

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
		string name = STRINGS.CREATURES.SPECIES.KELPPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.KELPPLANT.DESC;
		EffectorValues tIER = DECOR.BONUS.TIER1;
		KAnimFile anim = Assets.GetAnim("kelp_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.Hanging };
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("KelpPlant", name, desc, 4f, anim, "idle_empty", Grid.SceneLayer.BuildingFront, 1, 2, tIER, default(EffectorValues), SimHashes.Creature, additionalTags, 297.15f);
		EntityTemplates.MakeHangingOffsets(gameObject, 1, 2);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 253.15f, 263.15f, 358.15f, 373.15f, crop_id: KelpConfig.ID, baseTraitName: STRINGS.CREATURES.SPECIES.KELPPLANT.NAME, safe_elements: ALLOWED_ELEMENTS, pressure_sensitive: false, pressure_lethal_low: 0f, pressure_warning_low: 0.15f, can_drown: false, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, max_age: 2400f, min_radiation: 0f, max_radiation: 7400f, baseTraitId: "KelpPlantOriginal");
		gameObject.AddOrGet<PressureVulnerable>().allCellsMustBeSafe = true;
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.ToxicSand.ToString(),
				massConsumptionRate = 1f / 60f
			}
		});
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		gameObject.GetComponent<UprootedMonitor>().monitorCells = new CellOffset[1]
		{
			new CellOffset(0, 1)
		};
		gameObject.AddOrGet<StandardCropPlant>();
		string name2 = STRINGS.CREATURES.SPECIES.SEEDS.KELPPLANT.NAME;
		string desc2 = STRINGS.CREATURES.SPECIES.SEEDS.KELPPLANT.DESC;
		KAnimFile anim2 = Assets.GetAnim("seed_kelp_kanim");
		List<Tag> additionalTags2 = new List<Tag> { GameTags.WaterSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.KELPPLANT.DOMESTICATEDDESC;
		EntityTemplates.MakeHangingOffsets(EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Harvest, "KelpPlantSeed", name2, desc2, anim2, "object", 1, additionalTags2, SingleEntityReceptacle.ReceptacleDirection.Bottom, default(Tag), 4, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), "KelpPlant_preview", Assets.GetAnim("kelp_kanim"), "place", 1, 2), 1, 2);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
