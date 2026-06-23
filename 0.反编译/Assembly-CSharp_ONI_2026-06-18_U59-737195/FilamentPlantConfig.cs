using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class FilamentPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public static readonly string ID = "FilamentPlant";

	public static readonly string SEED_ID = "FilamentPlantSeed";

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
		GameObject obj = EntityTemplates.CreatePlacedEntity(ID, STRINGS.CREATURES.SPECIES.FILAMENTPLANT.NAME, STRINGS.CREATURES.SPECIES.FILAMENTPLANT.DESC, 1f, decor: DECOR.BONUS.TIER3, anim: Assets.GetAnim("potted_petta_pouf_kanim"), initialAnim: "idle", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 1, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 303.15f);
		EntityTemplates.ExtendEntityToBasicPlant(obj, 295.15f, 299.15f, 315.15f, 311.15f, PLANTS.SAFE_ELEMENTS.AllWaters, pressure_sensitive: false, 0f, 0.15f, null, can_drown: false, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 2200f, ID + "Original", STRINGS.CREATURES.SPECIES.FILAMENTPLANT.NAME);
		obj.AddOrGetDef<DecorPlantMonitor.Def>();
		PrickleGrass prickleGrass = obj.AddOrGet<PrickleGrass>();
		prickleGrass.positive_decor_effect = DECOR.BONUS.TIER3;
		prickleGrass.negative_decor_effect = DECOR.PENALTY.TIER3;
		string sEED_ID = SEED_ID;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.FILAMENTPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.FILAMENTPLANT.DESC;
		KAnimFile anim = Assets.GetAnim("seed_potted_petta_pouf_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.DecorSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.FILAMENTPLANT.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(obj, this, SeedProducer.ProductionType.Hidden, sEED_ID, name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 13, domesticatedDescription), ID + "_preview", Assets.GetAnim("filament_plant_kanim"), "place", 1, 1);
		return obj;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
