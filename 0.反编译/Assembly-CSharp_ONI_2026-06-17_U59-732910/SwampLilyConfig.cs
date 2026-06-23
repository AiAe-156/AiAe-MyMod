using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class SwampLilyConfig : IEntityConfig
{
	public static string ID = "SwampLily";

	public const string SEED_ID = "SwampLilySeed";

	public const float PLANT_FIBER_PRODUCED_PER_CYCLE = 24f;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("SwampLily", STRINGS.CREATURES.SPECIES.SWAMPLILY.NAME, STRINGS.CREATURES.SPECIES.SWAMPLILY.DESC, 1f, decor: DECOR.BONUS.TIER1, anim: Assets.GetAnim("swamplily_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingBack, width: 1, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 328.15f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 258.15f, 308.15f, 358.15f, 448.15f, new SimHashes[1] { SimHashes.ChlorineGas }, pressure_sensitive: true, 0f, 0.15f, SwampLilyFlowerConfig.ID, can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 4600f, ID + "Original", STRINGS.CREATURES.SPECIES.SWAMPLILY.NAME);
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		gameObject.AddOrGet<PlantFiberProducer>().amount = 24f;
		IHasDlcRestrictions dlcRestrictions = this as IHasDlcRestrictions;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.SWAMPLILY.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.SWAMPLILY.DESC;
		KAnimFile anim = Assets.GetAnim("seed_swampLily_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.SWAMPLILY.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, dlcRestrictions, SeedProducer.ProductionType.Harvest, "SwampLilySeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 21, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), ID + "_preview", Assets.GetAnim("swamplily_kanim"), "place", 1, 2);
		SoundEventVolumeCache.instance.AddVolume("swamplily_kanim", "SwampLily_grow", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("swamplily_kanim", "SwampLily_harvest", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("swamplily_kanim", "SwampLily_death", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("swamplily_kanim", "SwampLily_death_bloom", NOISE_POLLUTION.CREATURES.TIER3);
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.HarvestableIDs, ID);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
