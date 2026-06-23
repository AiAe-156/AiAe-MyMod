using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class OxyCoralConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "OxyCoral";

	public const string SEED_ID = "OxyCoralSeed";

	public const int MIN_LUX_REQUIRED = 2500;

	public const float MINIONS_SUPPORTED_PER_PLANT = 1.5f;

	public const float LIME_CONSUMPTION_RATE = 1f / 120f;

	public const float WATER_CONSUMPTION_RATE = 1f / 30f;

	public static float OXYGEN_PER_SECOND => DUPLICANTSTATS.STANDARD.BaseStats.OXYGEN_USED_PER_SECOND * 1.5f;

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
		GameObject template = EntityTemplates.CreatePlacedEntity("OxyCoral", STRINGS.CREATURES.SPECIES.OXYCORAL.NAME, STRINGS.CREATURES.SPECIES.OXYCORAL.DESC, 1f, decor: DECOR.BONUS.TIER2, anim: Assets.GetAnim("thalassaire_coral_kanim"), initialAnim: "grow", sceneLayer: Grid.SceneLayer.BuildingBack, width: 3, height: 2, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 303.15f);
		template = EntityTemplates.ExtendEntityToBasicPlant(template, 253.15f, 298.15f, 318.15f, 373.15f, new SimHashes[5]
		{
			SimHashes.Water,
			SimHashes.SaltWater,
			SimHashes.DirtyWater,
			SimHashes.Brine,
			SimHashes.MurkyBrine
		}, pressure_sensitive: false, 0f, 0.15f, null, can_drown: false, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 2200f, "OxyCoralOriginal", STRINGS.CREATURES.SPECIES.OXYCORAL.NAME);
		template.AddOrGet<LoopingSounds>();
		GameObject plant = template;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.OXYCORAL.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.OXYCORAL.DESC;
		KAnimFile anim = Assets.GetAnim("seed_thalassaire_coral_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.LargeSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.OXYCORAL.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(plant, this, SeedProducer.ProductionType.Hidden, "OxyCoralSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 20, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), "OxyCoral_preview", Assets.GetAnim("thalassaire_coral_kanim"), "place", 3, 2);
		EntityTemplates.ExtendPlantToIrrigated(template, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = ElementLoader.FindElementByHash(SimHashes.SaltWater).tag,
				massConsumptionRate = 1f / 30f
			}
		});
		EntityTemplates.ExtendPlantToFertilizable(template, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Lime.CreateTag(),
				massConsumptionRate = 1f / 120f
			}
		});
		template.AddTag(GameTags.BlockBuildOverPlantFeature);
		OxyCoral.Def def = template.AddOrGetDef<OxyCoral.Def>();
		def.OxygenProductionRate = OXYGEN_PER_SECOND;
		def.MinLuxRequired = 2500;
		def.OutputBubbleCells = new CellOffset[3]
		{
			new CellOffset(-1, 1),
			new CellOffset(0, 1),
			new CellOffset(1, 1)
		};
		template.AddOrGet<PressureVulnerable>().allCellsMustBeSafe = true;
		SoundEventVolumeCache.instance.AddVolume("oxy_fern_kanim", "MealLice_harvest", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("oxy_fern_kanim", "MealLice_LP", NOISE_POLLUTION.CREATURES.TIER4);
		return template;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
