using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class PrickleGrassConfig : IEntityConfig
{
	public const string ID = "PrickleGrass";

	public const string SEED_ID = "PrickleGrassSeed";

	public static readonly EffectorValues POSITIVE_DECOR_EFFECT = DECOR.BONUS.TIER3;

	public static readonly EffectorValues NEGATIVE_DECOR_EFFECT = DECOR.PENALTY.TIER3;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("PrickleGrass", STRINGS.CREATURES.SPECIES.PRICKLEGRASS.NAME, STRINGS.CREATURES.SPECIES.PRICKLEGRASS.DESC, 1f, decor: POSITIVE_DECOR_EFFECT, anim: Assets.GetAnim("bristlebriar_kanim"), initialAnim: "grow_seed", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 1);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 218.15f, 283.15f, 303.15f, 398.15f, new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, pressure_sensitive: true, 0f, 0.15f, null, can_drown: true, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 900f, "PrickleGrassOriginal", STRINGS.CREATURES.SPECIES.PRICKLEGRASS.NAME);
		PrickleGrass prickleGrass = gameObject.AddOrGet<PrickleGrass>();
		gameObject.AddOrGetDef<DecorPlantMonitor.Def>();
		prickleGrass.positive_decor_effect = POSITIVE_DECOR_EFFECT;
		prickleGrass.negative_decor_effect = NEGATIVE_DECOR_EFFECT;
		IHasDlcRestrictions dlcRestrictions = this as IHasDlcRestrictions;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.PRICKLEGRASS.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.PRICKLEGRASS.DESC;
		KAnimFile anim = Assets.GetAnim("seed_bristlebriar_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.DecorSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.PRICKLEGRASS.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, dlcRestrictions, SeedProducer.ProductionType.Hidden, "PrickleGrassSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 10, domesticatedDescription), "PrickleGrass_preview", Assets.GetAnim("bristlebriar_kanim"), "place", 1, 1);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
