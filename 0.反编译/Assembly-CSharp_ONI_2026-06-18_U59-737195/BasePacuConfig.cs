using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BasePacuConfig
{
	public const string EMOTION_FILE_NAME = "pacu_emotes_kanim";

	private static float KG_ORE_EATEN_PER_CYCLE = 7.5f;

	private static float CALORIES_PER_KG_OF_ORE = PacuTuning.STANDARD_CALORIES_PER_CYCLE / KG_ORE_EATEN_PER_CYCLE;

	public const float UNITS_OF_ALGAE_FROM_ONE_UNIT_OF_KELP = 2.6666667f;

	public static float KG_KELP_EATEN_PER_CYCLE = 20f;

	public static float KELP_TO_PRODUCT_EFFICIENCY = TUNING.CREATURES.CONVERSION_EFFICIENCY.NORMAL;

	private static float CALORIES_PER_KG_OF_KELP = PacuTuning.STANDARD_CALORIES_PER_CYCLE / KG_KELP_EATEN_PER_CYCLE;

	private static float KELP_PLANTS_PER_PACU = KG_KELP_EATEN_PER_CYCLE / 10f;

	private static float KELP_GROWTH_EATEN_PER_CYCLE = 0.2f * KELP_PLANTS_PER_PACU;

	private static float CALORIES_PER_GROWTH_EATEN = PacuTuning.STANDARD_CALORIES_PER_CYCLE / (KELP_GROWTH_EATEN_PER_CYCLE * 5f);

	private static float GROWTH_TO_PRODUCT_EFFICIENCY = KELP_TO_PRODUCT_EFFICIENCY * 10f;

	private static float MIN_POOP_SIZE_IN_KG = 25f;

	public static Action<object, object> OnCaloriesConsumed = delegate(object context, object data)
	{
		if (Boxed<CreatureCalorieMonitor.CaloriesConsumedEvent>.Unbox(data).tag == "FishFood".ToTag())
		{
			((Effects)context).Add("AteWellPreparedFishFood", should_save: true);
		}
	};

	public static GameObject CreatePrefab(string id, string base_trait_id, string name, string description, string anim_file, bool is_baby, string symbol_prefix, float warnLowTemp, float warnHighTemp, float lethalLowTemp, float lethalHighTemp)
	{
		float mASS = PacuTuning.MASS;
		EffectorValues tIER = DECOR.BONUS.TIER0;
		KAnimFile anim = Assets.GetAnim(is_baby ? anim_file : "pacu_build_kanim");
		float defaultTemperature = (warnLowTemp + warnHighTemp) / 2f;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, description, mASS, anim, "idle_loop", Grid.SceneLayer.Creatures, 1, 1, tIER, default(EffectorValues), SimHashes.Creature, null, defaultTemperature);
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.SwimmingCreature);
		component.AddTag(GameTags.Creatures.Swimmer);
		Trait trait = Db.Get().CreateTrait(base_trait_id, name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, PacuTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - PacuTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 25f, name));
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: false, allow_mark_for_capture: true, use_gun_for_pickup: true);
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "pacu_build_kanim", symbol_prefix, FactionManager.FactionID.Prey, base_trait_id, "SwimmerNavGrid", NavType.Swim, 32, 2f, "FishMeat", 1f, drownVulnerable: false, entombVulnerable: false, warnLowTemp, warnHighTemp, lethalLowTemp, lethalHighTemp);
		KAnimFile anim2 = Assets.GetAnim("pacu_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new GrowUpStates.Def(), is_baby)
			.Add(new TrappedStates.Def())
			.Add(new IncubatingStates.Def(), is_baby)
			.Add(new BaggedStates.Def())
			.Add(new FallStates.Def
			{
				getLandAnim = GetLandAnim
			})
			.Add(new DebugGoToStates.Def())
			.Add(new FlopStates.Def())
			.PushInterruptGroup()
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def(), !is_baby)
			.Add(new LayEggStates.Def(), !is_baby)
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def())
			.Add(new PoopStates.Def(anim2, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP, canComplain: false))
			.Add(new MoveToLureStates.Def())
			.Add(new CritterCondoStates.Def(), !is_baby)
			.Add(new CritterEmoteStates.Def(anim2))
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		gameObject.AddOrGetDef<CreatureFallMonitor.Def>().canSwim = true;
		gameObject.AddOrGetDef<FlopMonitor.Def>();
		gameObject.AddOrGetDef<FishOvercrowdingMonitor.Def>();
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGet<LoopingSounds>();
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.PacuSpecies, symbol_prefix);
		CritterCondoInteractMontior.Def def = gameObject.AddOrGetDef<CritterCondoInteractMontior.Def>();
		def.requireCavity = false;
		def.condoPrefabTag = "UnderwaterCritterCondo";
		Tag tag = SimHashes.ToxicSand.CreateTag();
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add(SimHashes.Algae.CreateTag());
		List<Diet.Info> list = new List<Diet.Info>
		{
			new Diet.Info(hashSet, tag, CALORIES_PER_KG_OF_ORE, TUNING.CREATURES.CONVERSION_EFFICIENCY.GOOD_0)
		};
		if (DlcManager.GetActiveDLCIds().Contains("DLC4_ID"))
		{
			HashSet<Tag> hashSet2 = new HashSet<Tag>();
			hashSet2.Add(KelpConfig.ID);
			HashSet<Tag> hashSet3 = new HashSet<Tag>();
			hashSet3.Add("KelpPlant");
			list.AddRange(new List<Diet.Info>
			{
				new Diet.Info(hashSet2, tag, CALORIES_PER_KG_OF_KELP, KELP_TO_PRODUCT_EFFICIENCY),
				new Diet.Info(hashSet3, tag, CALORIES_PER_GROWTH_EATEN, GROWTH_TO_PRODUCT_EFFICIENCY, null, 0f, produce_solid_tile: false, Diet.Info.FoodType.EatPlantDirectly)
			});
		}
		list.AddRange(SeedDiet(tag, PacuTuning.STANDARD_CALORIES_PER_CYCLE, 3f));
		HashSet<Tag> hashSet4 = new HashSet<Tag>();
		hashSet4.Add("FishFood".ToTag());
		list.Add(new Diet.Info(hashSet4, tag, PacuTuning.STANDARD_CALORIES_PER_CYCLE, 6f));
		Diet diet = new Diet(list.ToArray());
		CreatureCalorieMonitor.Def def2 = gameObject.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def2.diet = diet;
		def2.minConsumedCaloriesBeforePooping = CALORIES_PER_KG_OF_ORE * MIN_POOP_SIZE_IN_KG;
		gameObject.AddOrGetDef<SolidConsumerMonitor.Def>().diet = diet;
		gameObject.AddOrGetDef<LureableMonitor.Def>().lures = new Tag[1] { GameTags.Creatures.FishTrapLure };
		if (!string.IsNullOrEmpty(symbol_prefix))
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbol_prefix);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["Pacu"];
		pickupable.sortOrder = sortOrder;
		component.prefabSpawnFn += SubscribeFishFoodEffect;
		return gameObject;
	}

	public static List<Diet.Info> SeedDiet(Tag poopTag, float caloriesPerSeed, float producedConversionRate)
	{
		List<Diet.Info> list = new List<Diet.Info>();
		foreach (GameObject item in Assets.GetPrefabsWithComponent<PlantableSeed>())
		{
			GameObject prefab = Assets.GetPrefab(item.GetComponent<PlantableSeed>().PlantID);
			KPrefabID component = item.GetComponent<KPrefabID>();
			if (!prefab.HasTag(GameTags.DeprecatedContent) && !component.HasTag("KelpPlantSeed"))
			{
				SeedProducer component2 = prefab.GetComponent<SeedProducer>();
				if (component2 == null || component2.seedInfo.productionType == SeedProducer.ProductionType.Harvest || component2.seedInfo.productionType == SeedProducer.ProductionType.Crop || component2.seedInfo.productionType == SeedProducer.ProductionType.HarvestOnly)
				{
					HashSet<Tag> hashSet = new HashSet<Tag>();
					hashSet.Add(new Tag(component.GetComponent<KPrefabID>().PrefabID()));
					list.Add(new Diet.Info(hashSet, poopTag, caloriesPerSeed, producedConversionRate));
				}
			}
		}
		return list;
	}

	private static string GetLandAnim(FallStates.Instance smi)
	{
		if (smi.GetSMI<CreatureFallMonitor.Instance>().CanSwimAtCurrentLocation())
		{
			return "idle_loop";
		}
		return "flop_loop";
	}

	public static void SubscribeFishFoodEffect(GameObject fishGameObject)
	{
		Effects component = fishGameObject.GetComponent<Effects>();
		fishGameObject.Subscribe(-2038961714, OnCaloriesConsumed, component);
	}
}
