using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class SpiceVineConfig : IEntityConfig
{
	public const string ID = "SpiceVine";

	public const string SEED_ID = "SpiceVineSeed";

	public const float FERTILIZATION_RATE = 0.0016666667f;

	public const float WATER_RATE = 7f / 120f;

	public const float PLANT_FIBER_PRODUCED_PER_CYCLE = 16f;

	public GameObject CreatePrefab()
	{
		string name = STRINGS.CREATURES.SPECIES.SPICE_VINE.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SPICE_VINE.DESC;
		EffectorValues tIER = DECOR.BONUS.TIER1;
		KAnimFile anim = Assets.GetAnim("vinespicenut_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.Hanging };
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("SpiceVine", name, desc, 2f, anim, "idle_empty", Grid.SceneLayer.BuildingFront, 1, 3, tIER, default(EffectorValues), SimHashes.Creature, additionalTags, 320f);
		EntityTemplates.MakeHangingOffsets(gameObject, 1, 3);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 258.15f, 308.15f, 358.15f, 448.15f, null, pressure_sensitive: true, 0f, 0.15f, SpiceNutConfig.ID, can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 9800f, "SpiceVineOriginal", STRINGS.CREATURES.SPECIES.SPICE_VINE.NAME);
		Tag tag = ElementLoader.FindElementByHash(SimHashes.DirtyWater).tag;
		EntityTemplates.ExtendPlantToIrrigated(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = tag,
				massConsumptionRate = 7f / 120f
			}
		});
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = GameTags.Phosphorite,
				massConsumptionRate = 0.0016666667f
			}
		});
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		UprootedMonitor component = gameObject.GetComponent<UprootedMonitor>();
		component.monitorCells = new CellOffset[1]
		{
			new CellOffset(0, 1)
		};
		gameObject.AddOrGet<StandardCropPlant>();
		IHasDlcRestrictions dlcRestrictions = this as IHasDlcRestrictions;
		string name2 = STRINGS.CREATURES.SPECIES.SEEDS.SPICE_VINE.NAME;
		string desc2 = STRINGS.CREATURES.SPECIES.SEEDS.SPICE_VINE.DESC;
		KAnimFile anim2 = Assets.GetAnim("seed_spicenut_kanim");
		List<Tag> additionalTags2 = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.SPICE_VINE.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, dlcRestrictions, SeedProducer.ProductionType.Harvest, "SpiceVineSeed", name2, desc2, anim2, "object", 1, additionalTags2, SingleEntityReceptacle.ReceptacleDirection.Bottom, default(Tag), 4, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f);
		GameObject template = EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "SpiceVine_preview", Assets.GetAnim("vinespicenut_kanim"), "place", 1, 3);
		PlantFiberProducer plantFiberProducer = gameObject.AddOrGet<PlantFiberProducer>();
		plantFiberProducer.amount = 16f;
		EntityTemplates.MakeHangingOffsets(template, 1, 3);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
