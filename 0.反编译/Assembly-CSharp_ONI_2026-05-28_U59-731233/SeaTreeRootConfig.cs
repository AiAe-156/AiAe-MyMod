using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class SeaTreeRootConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SeaTree";

	public const string SEED_ID = "SeaTreeSeed";

	public const int MAX_BRANCH_COUNT = 8;

	public const float FERTILIZATION_KG_PER_CYCLE = 40f;

	public const float FERTILIZATION_RATE = 1f / 15f;

	public const float TEMPERATURE_LETHAL_LOW = 248.15f;

	public const float TEMPERATURE_WARNING_LOW = 295.15f;

	public const float TEMPERATURE_WARNING_HIGH = 310.15f;

	public const float TEMPERATURE_LETHAL_HIGH = 398.15f;

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("SeaTree", STRINGS.CREATURES.SPECIES.SEATREE.NAME, STRINGS.CREATURES.SPECIES.SEATREE.DESC, 2f, decor: DECOR.BONUS.TIER1, anim: Assets.GetAnim("sea_fairy_plant_kanim"), initialAnim: "grow", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 302.65f);
		string text = "SeaTreeOriginal";
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 248.15f, 295.15f, 310.15f, 398.15f, PLANTS.SAFE_ELEMENTS.AllWaters, pressure_sensitive: false, 0f, 0.15f, null, can_drown: false, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: false, 2400f, 0f, 2200f, text, STRINGS.CREATURES.SPECIES.SEATREE.NAME);
		PressureVulnerable pressureVulnerable = gameObject.AddOrGet<PressureVulnerable>();
		pressureVulnerable.allCellsMustBeSafe = true;
		WiltCondition component = gameObject.GetComponent<WiltCondition>();
		component.WiltDelay = 0f;
		component.RecoveryDelay = 0f;
		KPrefabID component2 = gameObject.GetComponent<KPrefabID>();
		GeneratedBuildings.RegisterWithOverlay(OverlayScreen.HarvestableIDs, component2.PrefabID().ToString());
		gameObject.AddOrGet<Traits>();
		Trait trait = Db.Get().traits.Get(text);
		Modifiers component3 = gameObject.GetComponent<Modifiers>();
		component3.initialTraits.Add(text);
		SeaTreeRoot.Def def = gameObject.AddOrGetDef<SeaTreeRoot.Def>();
		def.BRANCH_PREFAB_NAME = "SeaTreeBranch";
		def.MAX_BRANCH_COUNT = 8;
		gameObject.AddOrGet<HarvestDesignatable>();
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.ToxicMud.CreateTag(),
				massConsumptionRate = 1f / 15f
			}
		});
		string name = STRINGS.CREATURES.SPECIES.SEEDS.SEATREE.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.SEATREE.DESC;
		KAnimFile anim = Assets.GetAnim("seed_sea_plant_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.WaterSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.SEATREE.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Hidden, "SeaTreeSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 12, domesticatedDescription, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.6f);
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "SeaTree_preview", Assets.GetAnim("sea_fairy_plant_kanim"), "place", 1, 2);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
