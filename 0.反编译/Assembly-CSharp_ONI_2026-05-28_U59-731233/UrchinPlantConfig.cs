using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class UrchinPlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public static readonly SimHashes FertilizerElement = SimHashes.RefinedCarbon;

	public const float FERTILIZATION_RATE = 1f / 120f;

	public const int LIFETIME_CYCLES = 16;

	public const int UNITS_PER_HARVEST = 1;

	public const string ID = "UrchinPlant";

	public const string SEED_ID = "UrchinPlantSeed";

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
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("UrchinPlant", STRINGS.CREATURES.SPECIES.URCHINPLANT.NAME, STRINGS.CREATURES.SPECIES.URCHINPLANT.DESC, 4f, decor: DECOR.BONUS.TIER2, anim: Assets.GetAnim("urchin_plant_kanim"), initialAnim: "idle_full", sceneLayer: Grid.SceneLayer.Building, width: 2, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 323.15f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 303.15f, 313.15f, 353.15f, 383.15f, new SimHashes[5]
		{
			SimHashes.MurkyBrine,
			SimHashes.Brine,
			SimHashes.SaltWater,
			SimHashes.DirtyWater,
			SimHashes.Ink
		}, pressure_sensitive: false, 0f, 0.15f, "Urchin", can_drown: false, can_tinker: false, require_solid_tile: false, require_Backwall_Foundation: true, should_grow_old: true, 2400f, 0f, 2200f, "UrchinPlantOriginal", STRINGS.CREATURES.SPECIES.URCHINPLANT.NAME);
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = FertilizerElement.CreateTag(),
				massConsumptionRate = 1f / 120f
			}
		});
		gameObject.AddOrGet<StandardCropPlant>();
		PressureVulnerable pressureVulnerable = gameObject.AddOrGet<PressureVulnerable>();
		pressureVulnerable.allCellsMustBeSafe = true;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.URCHINPLANT.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.URCHINPLANT.DESC;
		KAnimFile anim = Assets.GetAnim("seed_urchin_plant_kanim");
		List<Tag> additionalTags = new List<Tag>
		{
			GameTags.CropSeed,
			GameTags.BackwallSeed
		};
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.URCHINPLANT.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, this, SeedProducer.ProductionType.Harvest, "UrchinPlantSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 2, domesticatedDescription);
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "UrchinPlant_preview", Assets.GetAnim("urchin_plant_kanim"), "place", 2, 2);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
		EntityTemplates.ExtendPlantEntityToRequireBackwall(inst);
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
