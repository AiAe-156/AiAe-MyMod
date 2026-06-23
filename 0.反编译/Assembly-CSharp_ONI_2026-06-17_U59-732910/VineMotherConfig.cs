using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class VineMotherConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "VineMother";

	public const string SEED_ID = "VineMotherSeed";

	public const int MAX_BRANCH_NETWORK_COUNT = 12;

	public static SimHashes[] ALLOWED_ELEMENTS = new SimHashes[3]
	{
		SimHashes.Oxygen,
		SimHashes.CarbonDioxide,
		SimHashes.ContaminatedOxygen
	};

	public const float IRRIGATION_RATE = 0.15f;

	public const float TEMPERATURE_LETHAL_LOW = 273.15f;

	public const float TEMPERATURE_WARNING_LOW = 298.15f;

	public const float TEMPERATURE_WARNING_HIGH = 318.15f;

	public const float TEMPERATURE_LETHAL_HIGH = 378.15f;

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("VineMother", STRINGS.CREATURES.SPECIES.VINEMOTHER.NAME, STRINGS.CREATURES.SPECIES.VINEMOTHER.DESC, 2f, decor: DECOR.BONUS.TIER1, anim: Assets.GetAnim("vine_mother_kanim"), initialAnim: "object", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 308.15f);
		string text = "VineMotherOriginal";
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 273.15f, 298.15f, 318.15f, 378.15f, ALLOWED_ELEMENTS, pressure_sensitive: false, 0f, 0.15f, null, can_drown: true, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: false, 2400f, 0f, 2200f, text, STRINGS.CREATURES.SPECIES.VINEMOTHER.NAME);
		WiltCondition component = gameObject.GetComponent<WiltCondition>();
		component.WiltDelay = 0f;
		component.RecoveryDelay = 0f;
		KPrefabID component2 = gameObject.GetComponent<KPrefabID>();
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.HarvestableIDs, component2.PrefabID().ToString());
		gameObject.AddOrGet<Traits>();
		Db.Get().traits.Get(text);
		gameObject.GetComponent<Modifiers>().initialTraits.Add(text);
		VineMother.Def def = gameObject.AddOrGetDef<VineMother.Def>();
		def.BRANCH_PREFAB_NAME = "VineBranch";
		def.MAX_BRANCH_COUNT = 24;
		gameObject.AddOrGet<HarvestDesignatable>();
		EntityTemplates.ExtendPlantToIrrigated(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = GameTags.Water,
				massConsumptionRate = 0.15f
			}
		});
		string name = STRINGS.CREATURES.SPECIES.SEEDS.VINEMOTHER.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.VINEMOTHER.DESC;
		KAnimFile anim = Assets.GetAnim("seed_vine_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.VINEMOTHER.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Hidden, "VineMotherSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 12, domesticatedDescription, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.6f), "VineMother_preview", Assets.GetAnim("vine_mother_kanim"), "place", 1, 2);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
