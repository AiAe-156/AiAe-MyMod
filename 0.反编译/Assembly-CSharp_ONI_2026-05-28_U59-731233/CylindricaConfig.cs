using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class CylindricaConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Cylindrica";

	public const string SEED_ID = "CylindricaSeed";

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("Cylindrica", STRINGS.CREATURES.SPECIES.CYLINDRICA.NAME, STRINGS.CREATURES.SPECIES.CYLINDRICA.DESC, 1f, decor: POSITIVE_DECOR_EFFECT, anim: Assets.GetAnim("potted_cylindricafan_kanim"), initialAnim: "grow_seed", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 1, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 298.15f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 288.15f, 293.15f, 323.15f, 373.15f, new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, pressure_sensitive: true, 0f, 0.15f, null, can_drown: true, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 2200f, "CylindricaOriginal", STRINGS.CREATURES.SPECIES.CYLINDRICA.NAME);
		PrickleGrass prickleGrass = gameObject.AddOrGet<PrickleGrass>();
		gameObject.AddOrGetDef<DecorPlantMonitor.Def>();
		prickleGrass.positive_decor_effect = POSITIVE_DECOR_EFFECT;
		prickleGrass.negative_decor_effect = NEGATIVE_DECOR_EFFECT;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.CYLINDRICA.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.CYLINDRICA.DESC;
		KAnimFile anim = Assets.GetAnim("seed_potted_cylindricafan_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.DecorSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.CYLINDRICA.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Hidden, "CylindricaSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 12, domesticatedDescription);
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "Cylindrica_preview", Assets.GetAnim("potted_cylindricafan_kanim"), "place", 1, 1);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
