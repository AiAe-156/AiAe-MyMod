using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class SaltySticksPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SaltySticksPlant";

	public const string SEED_ID = "SaltySticksPlantSeed";

	public const float FERTILIZER_RATE = 1f / 60f;

	public const int GROWTH_CYCLES = 4;

	public const float PLANT_FIBER_PRODUCED_PER_CYCLE = 2f;

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("SaltySticksPlant", STRINGS.CREATURES.SPECIES.SALTYSTICKSPLANT.NAME, STRINGS.CREATURES.SPECIES.SALTYSTICKSPLANT.DESC, 1f, decor: DECOR.PENALTY.TIER1, anim: Assets.GetAnim("salty_sticks_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingBack, width: 1, height: 2);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 273.15f, 283.15f, 313.15f, 318.15f, new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, pressure_sensitive: true, 0f, 0.15f, "SaltySticksFood", can_drown: true, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 9800f, "SaltySticksPlantOriginal", STRINGS.CREATURES.SPECIES.SALTYSTICKSPLANT.NAME);
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		string name = STRINGS.CREATURES.SPECIES.SEEDS.SALTYSTICKSPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.SALTYSTICKSPLANT.DESC;
		KAnimFile anim = Assets.GetAnim("seed_salty_sticks_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.SALTYSTICKSPLANT.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Harvest, "SaltySticksPlantSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 1, domesticatedDescription);
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Salt.CreateTag(),
				massConsumptionRate = 1f / 60f
			}
		});
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "SaltySticksPlant_preview", Assets.GetAnim("salty_sticks_kanim"), "place", 1, 2);
		gameObject.AddOrGet<PlantFiberProducer>().amount = 2f;
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
