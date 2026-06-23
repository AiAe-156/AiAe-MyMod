using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class GasGrassConfig : IEntityConfig
{
	public const string ID = "GasGrass";

	public const string SEED_ID = "GasGrassSeed";

	public const float CHLORINE_FERTILIZATION_RATE = 0.00083333335f;

	public const float DIRT_FERTILIZATION_RATE = 1f / 24f;

	public const int PLANT_FIBER_KG_PER_HARVEST = 400;

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity("GasGrass", STRINGS.CREATURES.SPECIES.GASGRASS.NAME, STRINGS.CREATURES.SPECIES.GASGRASS.DESC, 1f, decor: DECOR.BONUS.TIER3, anim: Assets.GetAnim("gassygrass_kanim"), initialAnim: "idle_empty", sceneLayer: Grid.SceneLayer.BuildingFront, width: 1, height: 3, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 255f);
		EntityTemplates.ExtendEntityToBasicPlant(gameObject, 218.15f, 0f, 348.15f, 373.15f, null, pressure_sensitive: false, 0f, 0.15f, "PlantFiber", can_drown: true, can_tinker: true, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, 2400f, 0f, 12200f, "GasGrassOriginal", STRINGS.CREATURES.SPECIES.GASGRASS.NAME);
		EntityTemplates.ExtendPlantToIrrigated(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = GameTags.Chlorine,
				massConsumptionRate = 0.00083333335f
			}
		});
		EntityTemplates.ExtendPlantToFertilizable(gameObject, new PlantElementAbsorber.ConsumeInfo[1]
		{
			new PlantElementAbsorber.ConsumeInfo
			{
				tag = SimHashes.Dirt.CreateTag(),
				massConsumptionRate = 1f / 24f
			}
		});
		gameObject.AddOrGet<StandardCropPlant>();
		gameObject.AddOrGet<DirectlyEdiblePlant_Growth>();
		Modifiers component = gameObject.GetComponent<Modifiers>();
		Trait trait = Db.Get().traits.Get(component.initialTraits[0]);
		trait.Add(new AttributeModifier(Db.Get().PlantAttributes.MinLightLux.Id, 10000f, STRINGS.CREATURES.SPECIES.GASGRASS.NAME));
		component.initialAttributes.Add(Db.Get().PlantAttributes.MinLightLux.Id);
		IlluminationVulnerable illuminationVulnerable = gameObject.AddOrGet<IlluminationVulnerable>();
		illuminationVulnerable.SetPrefersDarkness();
		IHasDlcRestrictions dlcRestrictions = this as IHasDlcRestrictions;
		int productionType = (DlcManager.FeaturePlantMutationsEnabled() ? 2 : 0);
		string name = STRINGS.CREATURES.SPECIES.SEEDS.GASGRASS.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.GASGRASS.DESC;
		KAnimFile anim = Assets.GetAnim("seed_gassygrass_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.GASGRASS.DOMESTICATEDDESC;
		GameObject seed = EntityTemplates.CreateAndRegisterSeedForPlant(gameObject, dlcRestrictions, (SeedProducer.ProductionType)productionType, "GasGrassSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 22, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.2f, 0.2f);
		EntityTemplates.CreateAndRegisterPreviewForPlant(seed, "GasGrass_preview", Assets.GetAnim("gassygrass_kanim"), "place", 1, 1);
		SoundEventVolumeCache.instance.AddVolume("gassygrass_kanim", "GasGrass_grow", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("gassygrass_kanim", "GasGrass_harvest", NOISE_POLLUTION.CREATURES.TIER3);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
