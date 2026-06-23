using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class CritterTrapPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "CritterTrapPlant";

	public const float WATER_RATE = 1f / 60f;

	public const float GAS_RATE = 1f / 24f;

	public const float GAS_VENT_THRESHOLD = 33.25f;

	private static Tag[] AllowedPreyTags = new Tag[2]
	{
		GameTags.Creatures.Walker,
		GameTags.Creatures.Hoverer
	};

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
		string name = STRINGS.CREATURES.SPECIES.CRITTERTRAPPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.CRITTERTRAPPLANT.DESC;
		EffectorValues tIER = DECOR.BONUS.TIER1;
		KAnimFile anim = Assets.GetAnim("venus_critter_trap_kanim");
		float fREEZING_ = TUNING.CREATURES.TEMPERATURE.FREEZING_3;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("CritterTrapPlant", name, desc, 4f, anim, "idle_open", Grid.SceneLayer.BuildingBack, 1, 2, tIER, default(EffectorValues), SimHashes.Creature, null, fREEZING_);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, TUNING.CREATURES.TEMPERATURE.FREEZING_10, TUNING.CREATURES.TEMPERATURE.FREEZING_9, TUNING.CREATURES.TEMPERATURE.FREEZING, TUNING.CREATURES.TEMPERATURE.COOL, null, pressure_sensitive: false, 0f, 0.15f, "PlantMeat", can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: false, 2400f, 0f, 2200f, "CritterTrapPlantOriginal", STRINGS.CREATURES.SPECIES.CRITTERTRAPPLANT.NAME);
		Object.DestroyImmediate(gameObject.GetComponent<MutantPlant>());
		CritterTrapPlant critterTrapPlant = gameObject.AddOrGet<CritterTrapPlant>();
		critterTrapPlant.CONSUMABLE_TAGs = AllowedPreyTags;
		critterTrapPlant.gasOutputRate = 1f / 24f;
		critterTrapPlant.outputElement = SimHashes.Hydrogen;
		critterTrapPlant.gasVentThreshold = 33.25f;
		TrapTrigger trapTrigger = gameObject.AddOrGet<TrapTrigger>();
		trapTrigger.trappableCreatures = AllowedPreyTags;
		trapTrigger.trappedOffset = new Vector2(0.5f, 0f);
		trapTrigger.enabled = false;
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<Storage>();
		Tag tag = ElementLoader.FindElementByHash(SimHashes.DirtyWater).tag;
		EntityTemplates.ExtendPlantToIrrigated(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = tag,
				massConsumptionRate = 1f / 60f
			}
		});
		string name2 = STRINGS.CREATURES.SPECIES.SEEDS.CRITTERTRAPPLANT.NAME;
		string desc2 = STRINGS.CREATURES.SPECIES.SEEDS.CRITTERTRAPPLANT.DESC;
		KAnimFile anim2 = Assets.GetAnim("seed_critter_trap_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.CRITTERTRAPPLANT.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Hidden, "CritterTrapPlantSeed", name2, desc2, anim2, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 21, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), "CritterTrapPlant_preview", Assets.GetAnim("venus_critter_trap_kanim"), "place", 1, 2);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		CritterTrapPlant component = inst.GetComponent<CritterTrapPlant>();
		inst.GetComponent<TrapTrigger>().customConditionsToTrap = component.IsEntityEdible;
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
