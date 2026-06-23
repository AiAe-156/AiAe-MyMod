using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class DewPalmConfig : IEntityConfig, IHasDlcRestrictions
{
	public static readonly string ID = "DewPalm";

	public static readonly string SEED_ID = "DewPalmSeed";

	public static readonly string PREVIEW_ID = "DewPalmPreview";

	public static readonly string BASE_TRAIT_ID = "DewPalmOriginal";

	public const float GROWTH_CYCLES = 10f;

	public const int WOOD_HARVEST_YIELD = 700;

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
		string iD = ID;
		string name = STRINGS.CREATURES.SPECIES.DEWPALM.NAME;
		string desc = STRINGS.CREATURES.SPECIES.DEWPALM.DESC;
		KAnimFile anim = Assets.GetAnim("rubber_tree_kanim");
		EffectorValues tIER = DECOR.BONUS.TIER2;
		float hOT = TUNING.CREATURES.TEMPERATURE.HOT;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(iD, name, desc, 100f, anim, "idle_empty", Grid.SceneLayer.BuildingBack, 3, 4, tIER, default(EffectorValues), SimHashes.Creature, null, hOT);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, GameUtil.GetTemperatureConvertedToKelvin(16f, GameUtil.TemperatureUnit.Celsius), GameUtil.GetTemperatureConvertedToKelvin(24f, GameUtil.TemperatureUnit.Celsius), GameUtil.GetTemperatureConvertedToKelvin(54f, GameUtil.TemperatureUnit.Celsius), GameUtil.GetTemperatureConvertedToKelvin(56f, GameUtil.TemperatureUnit.Celsius), null, pressure_sensitive: true, 0f, 0.15f, SimHashes.PalmWood.ToString(), can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 12000f, 0f, 2200f, BASE_TRAIT_ID, STRINGS.CREATURES.SPECIES.DEWPALM.NAME);
		PlantElementAbsorber.ConsumeInfo[] fertilizers = new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Sulfur.CreateTag(),
				massConsumptionRate = 1f / 30f
			}
		};
		EntityTemplates.ExtendPlantToFertilizable(gameObject, fertilizers);
		gameObject.AddOrGet<StandardCropPlant>();
		string sEED_ID = SEED_ID;
		string name2 = STRINGS.CREATURES.SPECIES.SEEDS.DEWPALM.NAME;
		string desc2 = STRINGS.CREATURES.SPECIES.SEEDS.DEWPALM.DESC;
		KAnimFile anim2 = Assets.GetAnim("seed_rubbertree_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.LargeSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.DEWPALM.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, null, SeedProducer.ProductionType.Harvest, sEED_ID, name2, desc2, anim2, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 3, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.45f, 0.33f);
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, PREVIEW_ID, Assets.GetAnim("rubber_tree_kanim"), "place", 3, 4);
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
