using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class WaterCupsPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public static string ID = "WaterCups";

	public static string SEED_ID = "WaterCupsSeed";

	public static string BASETRAIT_ID = "WaterCupsOriginal";

	public static string PREVIEW_ID = "WaterCupsPreview";

	public static EffectorValues POSITIVE_DECOR_EFFECT = DECOR.BONUS.TIER3;

	public static EffectorValues NEGATIVE_DECOR_EFFECT = DECOR.PENALTY.TIER3;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(ID, STRINGS.CREATURES.SPECIES.WATERCUPS.NAME, STRINGS.CREATURES.SPECIES.WATERCUPS.DESC, 1f, Assets.GetAnim("watercups_kanim"), "idle", Grid.SceneLayer.BuildingFront, 1, 1, POSITIVE_DECOR_EFFECT, NOISE_POLLUTION.NONE, SimHashes.Creature, null, 298.15f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 288.15f, 293.15f, 323.15f, 373.15f, new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, pressure_sensitive: true, 0f, 0.15f, null, can_drown: true, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 2200f, BASETRAIT_ID, STRINGS.CREATURES.SPECIES.WATERCUPS.NAME);
		PrickleGrass prickleGrass = gameObject.AddOrGet<PrickleGrass>();
		prickleGrass.positive_decor_effect = POSITIVE_DECOR_EFFECT;
		prickleGrass.negative_decor_effect = NEGATIVE_DECOR_EFFECT;
		gameObject.AddOrGetDef<DecorPlantMonitor.Def>();
		string sEED_ID = SEED_ID;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.WATERCUPS.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.WATERCUPS.DESC;
		KAnimFile anim = Assets.GetAnim("seed_watercups_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.DecorSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.WATERCUPS.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Hidden, sEED_ID, name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 12, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.22f, 0.22f);
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, PREVIEW_ID, Assets.GetAnim("watercups_kanim"), "place", 1, 1);
		return gameObject;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
