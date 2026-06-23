using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class BulbloomConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Bulbloom";

	public const string SEED_ID = "BulbloomSeed";

	public static readonly EffectorValues POSITIVE_DECOR_EFFECT = DECOR.BONUS.TIER3;

	public static readonly EffectorValues NEGATIVE_DECOR_EFFECT = DECOR.PENALTY.TIER3;

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("Bulbloom", STRINGS.CREATURES.SPECIES.BULBLOOM.NAME, STRINGS.CREATURES.SPECIES.BULBLOOM.DESC, 1f, decor: POSITIVE_DECOR_EFFECT, anim: Assets.GetAnim("bulbloom_kanim"), initialAnim: "grow_seed", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 1, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 333.15f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 293.15f, 313.15f, 353.15f, 383.15f, PLANTS.SAFE_ELEMENTS.MurkyWaters, pressure_sensitive: false, 0f, 0.15f, null, can_drown: false, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 2200f, "BulbloomOriginal", STRINGS.CREATURES.SPECIES.BULBLOOM.NAME);
		PrickleGrass prickleGrass = gameObject.AddOrGet<PrickleGrass>();
		gameObject.AddOrGetDef<DecorPlantMonitor.Def>();
		prickleGrass.positive_decor_effect = POSITIVE_DECOR_EFFECT;
		prickleGrass.negative_decor_effect = NEGATIVE_DECOR_EFFECT;
		Light2D light2D = gameObject.AddOrGet<Light2D>();
		light2D.Color = new Color(0.4f, 0.5f, 1f, 0.15f);
		light2D.overlayColour = LIGHT2D.LIGHT_OVERLAY;
		light2D.Range = 2f;
		light2D.Direction = LIGHT2D.DEFAULT_DIRECTION;
		light2D.Offset = new Vector2(0.05f, 0.5f);
		light2D.shape = LightShape.Circle;
		light2D.drawOverlay = true;
		light2D.Lux = DUPLICANTSTATS.STANDARD.Light.LOW_LIGHT;
		gameObject.AddComponent<PlantGlowController>();
		string name = STRINGS.CREATURES.SPECIES.SEEDS.BULBLOOM.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.BULBLOOM.DESC;
		KAnimFile anim = Assets.GetAnim("seed_bulbloom_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.DecorSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.BULBLOOM.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Hidden, "BulbloomSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 13, domesticatedDescription);
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "Bulbloom_preview", Assets.GetAnim("bulbloom_kanim"), "place", 1, 1);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
