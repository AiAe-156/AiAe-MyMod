using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class FlyTrapPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "FlyTrapPlant";

	public const string SEED_ID = "FlyTrapPlantSeed";

	public static readonly StandardCropPlant.AnimSet Default_StandardCropAnimSet = new StandardCropPlant.AnimSet
	{
		pre_grow = "grow_pre",
		grow = "grow",
		grow_pst = "grow_pst",
		idle_full = "idle_full",
		wilt_base = "wilt",
		harvest = "harvest",
		waning = "waning",
		grow_playmode = KAnim.PlayMode.Paused
	};

	public const int DIGESTION_DURATION_CYCLES = 12;

	public const float DIGESTION_DURATION = 7200f;

	public const int AMBER_PER_HARVEST_KG = 264;

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
		string name = STRINGS.CREATURES.SPECIES.FLYTRAPPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.FLYTRAPPLANT.DESC;
		EffectorValues tIER = DECOR.BONUS.TIER1;
		KAnimFile anim = Assets.GetAnim("ceiling_carnie_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.Hanging };
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("FlyTrapPlant", name, desc, 1f, anim, "idle_empty", Grid.SceneLayer.BuildingFront, 1, 2, tIER, default(EffectorValues), SimHashes.Creature, additionalTags, 291.15f);
		EntityTemplates.MakeHangingOffsets(gameObject, 1, 2);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 273.15f, 283.15f, 328.15f, 348.15f, null, pressure_sensitive: true, 0f, 0.15f, SimHashes.Amber.ToString(), can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 7400f, "FlyTrapPlantOriginal", STRINGS.CREATURES.SPECIES.FLYTRAPPLANT.NAME);
		gameObject.GetComponent<UprootedMonitor>().monitorCells = new CellOffset[1]
		{
			new CellOffset(0, 1)
		};
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<FlytrapConsumptionMonitor>();
		gameObject.AddOrGet<Growing>().MaxMaturityValuePercentageToSpawnWith = 0f;
		string name2 = STRINGS.CREATURES.SPECIES.SEEDS.FLYTRAPPLANT.NAME;
		string desc2 = STRINGS.CREATURES.SPECIES.SEEDS.FLYTRAPPLANT.DESC;
		KAnimFile anim2 = Assets.GetAnim("seed_ceiling_carnie_kanim");
		List<Tag> additionalTags2 = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.FLYTRAPPLANT.DOMESTICATEDDESC;
		EntityTemplates.MakeHangingOffsets(EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Harvest, "FlyTrapPlantSeed", name2, desc2, anim2, "object", 1, additionalTags2, SingleEntityReceptacle.ReceptacleDirection.Bottom, default(Tag), 4, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), "FlyTrapPlant_preview", Assets.GetAnim("ceiling_carnie_kanim"), "place", 1, 2), 1, 2);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		inst.AddOrGet<StandardCropPlant>().anims = Default_StandardCropAnimSet;
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
