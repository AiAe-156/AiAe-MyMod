using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ButterflyPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "ButterflyPlant";

	public const string SEED_ID = "ButterflyPlantSeed";

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("ButterflyPlant", STRINGS.CREATURES.SPECIES.BUTTERFLYPLANT.NAME, STRINGS.CREATURES.SPECIES.BUTTERFLYPLANT.DESC, 1f, decor: DECOR.BONUS.TIER1, anim: Assets.GetAnim("pollinator_plant_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingBack, width: 1, height: 2);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 233.15f, 283.15f, 318.15f, 353.15f, new SimHashes[4]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide,
			SimHashes.ChlorineGas
		}, pressure_sensitive: true, 0f, 0.15f, "Butterfly", can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 7400f, "ButterflyPlantOriginal", STRINGS.CREATURES.SPECIES.BUTTERFLYPLANT.NAME);
		Object.DestroyImmediate(gameObject.GetComponent<MutantPlant>());
		Object.DestroyImmediate(gameObject.GetComponent<HarvestDesignatable>());
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = GameTags.Dirt,
				massConsumptionRate = 1f / 60f
			}
		});
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<LoopingSounds>();
		string name = STRINGS.CREATURES.SPECIES.SEEDS.BUTTERFLYPLANTSEED.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.BUTTERFLYPLANTSEED.DESC;
		KAnimFile anim = Assets.GetAnim("seed_pollinator_plant_kanim");
		EdiblesManager.FoodInfo bUTTERFLY_SEED = FOOD.FOOD_TYPES.BUTTERFLY_SEED;
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.BUTTERFLYPLANT.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlantAsFood(gameObject, this, SeedProducer.ProductionType.Crop, "ButterflyPlantSeed", name, desc, anim, bUTTERFLY_SEED, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 2, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f, null, "", ignoreDefaultSeedTag: true), "ButterflyPlant_preview", Assets.GetAnim("pollinator_plant_kanim"), "place", 1, 2);
		gameObject.AddOrGet<Growing>().maxAge = 0f;
		gameObject.AddOrGet<Crop>().cropSpawnOffset = new Vector3(-0.0365f, 1.26175f, 0f);
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
