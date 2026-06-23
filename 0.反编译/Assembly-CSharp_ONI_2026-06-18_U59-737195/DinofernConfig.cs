using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class DinofernConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Dinofern";

	public const string SEED_ID = "DinofernSeed";

	public const float CHLORINE_CONSUMPTION_RATE = 0.09f;

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
		GameObject obj = EntityTemplates.ExtendEntityToBasicPlant(EntityTemplates.CreatePlacedEntity("Dinofern", STRINGS.CREATURES.SPECIES.DINOFERN.NAME, STRINGS.CREATURES.SPECIES.DINOFERN.DESC, 1f, decor: DECOR.PENALTY.TIER1, anim: Assets.GetAnim("prehistoric_fern_kanim"), initialAnim: "idle_full", sceneLayer: Grid.SceneLayer.BuildingBack, width: 3, height: 3, noise: default(EffectorValues), element: SimHashes.Creature, additionalTags: null, defaultTemperature: 253.15f), 218.15f, 228.15f, 288.15f, 308.15f, crop_id: FernFoodConfig.ID, safe_elements: new SimHashes[1] { SimHashes.ChlorineGas }, pressure_sensitive: true, pressure_lethal_low: 0f, pressure_warning_low: 0.5f, can_drown: true, can_tinker: false, require_solid_tile: true, require_Backwall_Foundation: false, should_grow_old: true, max_age: 2400f, min_radiation: 0f, max_radiation: 2200f, baseTraitId: "DinofernOriginal", baseTraitName: STRINGS.CREATURES.SPECIES.DINOFERN.NAME);
		obj.AddOrGet<LoopingSounds>();
		obj.AddOrGet<StandardCropPlant>();
		obj.AddOrGet<Dinofern>();
		Storage storage = obj.AddOrGet<Storage>();
		storage.showInUI = false;
		storage.capacityKg = 1f;
		ElementConsumer elementConsumer = obj.AddOrGet<ElementConsumer>();
		elementConsumer.showInStatusPanel = true;
		elementConsumer.storeOnConsume = false;
		elementConsumer.elementToConsume = SimHashes.ChlorineGas;
		elementConsumer.configuration = ElementConsumer.Configuration.Element;
		elementConsumer.consumptionRadius = 4;
		elementConsumer.EnableConsumption(enabled: false);
		elementConsumer.sampleCellOffset = new Vector3(0f, 0f);
		elementConsumer.consumptionRate = 0.09f;
		string name = STRINGS.CREATURES.SPECIES.SEEDS.DINOFERN.NAME;
		string desc = STRINGS.CREATURES.SPECIES.SEEDS.DINOFERN.DESC;
		KAnimFile anim = Assets.GetAnim("seed_megafrond_kanim");
		List<Tag> additionalTags = new List<Tag> { GameTags.CropSeed };
		string domesticatedDescription = STRINGS.CREATURES.SPECIES.DINOFERN.DOMESTICATEDDESC;
		EntityTemplates.CreateAndRegisterPreviewForPlant(EntityTemplates.CreateAndRegisterSeedForPlant(obj, this, SeedProducer.ProductionType.Hidden, "DinofernSeed", name, desc, anim, "object", 1, additionalTags, SingleEntityReceptacle.ReceptacleDirection.Top, default(Tag), 20, domesticatedDescription, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f), "Dinofern_preview", Assets.GetAnim("prehistoric_fern_kanim"), "place", 3, 3);
		obj.AddTag(GameTags.BlockBuildOverPlantFeature);
		SoundEventVolumeCache.instance.AddVolume("oxy_fern_kanim", "MealLice_harvest", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("oxy_fern_kanim", "MealLice_LP", NOISE_POLLUTION.CREATURES.TIER4);
		return obj;
	}

	public void OnPrefabInit(GameObject prefab)
	{
		prefab.AddOrGet<StandardCropPlant>().anims = new StandardCropPlant.AnimSet
		{
			pre_grow = "expand",
			grow = "grow",
			grow_pst = "grow_pst",
			idle_full = "idle_full",
			wilt_base = "wilt",
			harvest = "harvest",
			waning = "waning"
		};
	}

	public void OnSpawn(GameObject inst)
	{
		inst.GetComponent<Dinofern>().SetConsumptionRate();
	}
}
