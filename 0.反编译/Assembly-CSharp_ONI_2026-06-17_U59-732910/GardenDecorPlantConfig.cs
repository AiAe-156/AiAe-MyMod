using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class GardenDecorPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "GardenDecorPlant";

	public const string SEED_ID = "GardenDecorPlantSeed";

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("GardenDecorPlant", STRINGS.CREATURES.SPECIES.GARDENDECORPLANT.NAME, STRINGS.CREATURES.SPECIES.GARDENDECORPLANT.DESC, 1f, decor: DECOR.BONUS.TIER3, anim: Assets.GetAnim("discplant_kanim"), initialAnim: "grow_seed", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 1);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 263.15f, 268.15f, 313.15f, 323.15f, new SimHashes[3]
		{
			SimHashes.Oxygen,
			SimHashes.ContaminatedOxygen,
			SimHashes.CarbonDioxide
		}, pressure_sensitive: false, 0f, 0.15f, null, can_drown: true, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 2200f, "GardenDecorPlantOriginal", STRINGS.CREATURES.SPECIES.GARDENDECORPLANT.NAME);
		PrickleGrass prickleGrass = gameObject.AddOrGet<PrickleGrass>();
		gameObject.AddOrGetDef<DecorPlantMonitor.Def>();
		prickleGrass.positive_decor_effect = DECOR.BONUS.TIER3;
		prickleGrass.negative_decor_effect = DECOR.PENALTY.TIER3;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.GARDENDECORPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.GARDENDECORPLANT.DESC;
		KAnimFile anim = Assets.GetAnim("seed_discplant_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.DecorSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.GARDENDECORPLANT.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Hidden, "GardenDecorPlantSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 13, domesticatedDescription), "GardenDecorPlant_preview", Assets.GetAnim("discplant_kanim"), "place", 1, 1);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
