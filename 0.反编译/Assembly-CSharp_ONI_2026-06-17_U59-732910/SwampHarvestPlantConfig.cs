using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class SwampHarvestPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SwampHarvestPlant";

	public const string SEED_ID = "SwampHarvestPlantSeed";

	public const float WATER_RATE = 1f / 15f;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("SwampHarvestPlant", STRINGS.CREATURES.SPECIES.SWAMPHARVESTPLANT.NAME, STRINGS.CREATURES.SPECIES.SWAMPHARVESTPLANT.DESC, 1f, decor: DECOR.PENALTY.TIER1, anim: Assets.GetAnim("swampcrop_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingBack, width: 1, height: 2);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 218.15f, 283.15f, 303.15f, 398.15f, crop_id: SwampFruitConfig.ID, safe_elements: new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, pressure_sensitive: true, pressure_lethal_low: 0f, pressure_warning_low: 0.15f, can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, max_age: 2400f, min_radiation: 0f, max_radiation: 4600f, baseTraitId: "SwampHarvestPlantOriginal", baseTraitName: gameObject.PrefabID().Name);
		gameObject.AddOrGet<IlluminationVulnerable>().SetPrefersDarkness(prefersDarkness: true);
		EntityTemplates.ExtendPlantToIrrigated(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = GameTags.DirtyWater,
				massConsumptionRate = 1f / 15f
			}
		});
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<LoopingSounds>();
		string name = STRINGS.CREATURES.SPECIES.SEEDS.SWAMPHARVESTPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.SWAMPHARVESTPLANT.DESC;
		KAnimFile anim = Assets.GetAnim("seed_swampcrop_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.SWAMPHARVESTPLANT.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Harvest, "SwampHarvestPlantSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 2, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), "SwampHarvestPlant_preview", Assets.GetAnim("swampcrop_kanim"), "place", 1, 2);
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
