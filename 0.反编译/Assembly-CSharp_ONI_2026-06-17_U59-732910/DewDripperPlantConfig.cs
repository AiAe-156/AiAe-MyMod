using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class DewDripperPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "DewDripperPlant";

	public const string SEED_ID = "DewDripperPlantSeed";

	public const float GROWTH_TIME = 1200f;

	public const float FERTILIZER_RATE = 1f / 60f;

	public const float PLANT_FIBER_PRODUCED_PER_CYCLE = 2f;

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
		string name = STRINGS.CREATURES.SPECIES.DEWDRIPPERPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.DEWDRIPPERPLANT.DESC;
		EffectorValues tIER = DECOR.BONUS.TIER0;
		KAnimFile anim = Assets.GetAnim("brackwood_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.Hanging };
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("DewDripperPlant", name, desc, 1f, anim, "idle_empty", Grid.SceneLayer.BuildingFront, 1, 2, tIER, default(EffectorValues), SimHashes.Creature, additionalTags, 253.15f);
		EntityTemplates.MakeHangingOffsets(gameObject, 1, 2);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 218.15f, 238.15f, 278.15f, 308.15f, null, pressure_sensitive: true, 0f, 0.25f, DewDripConfig.ID, can_drown: true, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 4600f, "DewDripperPlantOriginal", STRINGS.CREATURES.SPECIES.DEWDRIPPERPLANT.NAME);
		PressureVulnerable pressureVulnerable = gameObject.AddOrGet<PressureVulnerable>();
		pressureVulnerable.pressureWarning_High = 2f;
		pressureVulnerable.pressureLethal_High = 10f;
		gameObject.GetComponent<UprootedMonitor>().monitorCells = new CellOffset[1]
		{
			new CellOffset(0, 1)
		};
		gameObject.AddOrGet<PlantFiberProducer>().amount = 2f;
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<LoopingSounds>();
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.BrineIce.CreateTag(),
				massConsumptionRate = 1f / 60f
			}
		});
		string name2 = STRINGS.CREATURES.SPECIES.SEEDS.DEWDRIPPERPLANT.NAME;
		string desc2 = STRINGS.CREATURES.SPECIES.SEEDS.DEWDRIPPERPLANT.DESC;
		KAnimFile anim2 = Assets.GetAnim("seed_brackwood_kanim");
		List<Tag> additionalTags2 = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.DEWDRIPPERPLANT.DOMESTICATEDDESC;
		EntityTemplates.MakeHangingOffsets(EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Harvest, "DewDripperPlantSeed", name2, desc2, anim2, "object", 1, additionalTags2, SingleEntityReceptacle.ReceptacleDirection.Bottom, default(Tag), 5, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), "DewDripperPlant_preview", Assets.GetAnim("brackwood_kanim"), "place", 1, 2), 1, 2);
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
