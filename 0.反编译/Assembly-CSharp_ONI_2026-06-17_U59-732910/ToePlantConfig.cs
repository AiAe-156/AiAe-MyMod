using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class ToePlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "ToePlant";

	public const string SEED_ID = "ToePlantSeed";

	public static readonly EffectorValues POSITIVE_DECOR_EFFECT = DECOR.BONUS.TIER3;

	public static readonly EffectorValues NEGATIVE_DECOR_EFFECT = DECOR.PENALTY.TIER3;

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
		string name = STRINGS.CREATURES.SPECIES.TOEPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.TOEPLANT.DESC;
		EffectorValues pOSITIVE_DECOR_EFFECT = POSITIVE_DECOR_EFFECT;
		KAnimFile anim = Assets.GetAnim("potted_toes_kanim");
		float fREEZING_ = TUNING.CREATURES.TEMPERATURE.FREEZING_3;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("ToePlant", name, desc, 1f, anim, "grow_seed", Grid.SceneLayer.BuildingFront, 1, 1, pOSITIVE_DECOR_EFFECT, default(EffectorValues), SimHashes.Creature, null, fREEZING_);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, safe_elements: new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, temperature_lethal_low: TUNING.CREATURES.TEMPERATURE.FREEZING_10, temperature_warning_low: TUNING.CREATURES.TEMPERATURE.FREEZING_9, temperature_warning_high: TUNING.CREATURES.TEMPERATURE.FREEZING, temperature_lethal_high: TUNING.CREATURES.TEMPERATURE.COOL, pressure_sensitive: true, pressure_lethal_low: 0f, pressure_warning_low: 0.15f, crop_id: null, can_drown: true, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, max_age: 2400f, min_radiation: 0f, max_radiation: 2200f, baseTraitId: "ToePlantOriginal", baseTraitName: STRINGS.CREATURES.SPECIES.TOEPLANT.NAME);
		PrickleGrass prickleGrass = gameObject.AddOrGet<PrickleGrass>();
		gameObject.AddOrGetDef<DecorPlantMonitor.Def>();
		prickleGrass.positive_decor_effect = POSITIVE_DECOR_EFFECT;
		prickleGrass.negative_decor_effect = NEGATIVE_DECOR_EFFECT;
		string name2 = STRINGS.CREATURES.SPECIES.SEEDS.TOEPLANT.NAME;
		string desc2 = STRINGS.CREATURES.SPECIES.SEEDS.TOEPLANT.DESC;
		KAnimFile anim2 = Assets.GetAnim("seed_potted_toes_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.DecorSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.TOEPLANT.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Hidden, "ToePlantSeed", name2, desc2, anim2, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 12, domesticatedDescription), "ToePlant_preview", Assets.GetAnim("potted_toes_kanim"), "place", 1, 1);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
