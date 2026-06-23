using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class BaseParrotFish
{
	public const string EMOTION_FILE_NAME = "fish_bioluminescent_emotes_kanim";

	public static float KG_CORAL_PICKUPABLE_EATEN_PER_CYCLE = 10f;

	private static float CALORIES_PER_KG_OF_ORE = ParrotFishTuning.STANDARD_CALORIES_PER_CYCLE / KG_CORAL_PICKUPABLE_EATEN_PER_CYCLE;

	public static float CORAL_PICKUPABLE_TO_PRODUCT_EFFICIENCY = TUNING.CREATURES.CONVERSION_EFFICIENCY.NORMAL;

	private static float CALORIES_PER_KG_OF_CORAL_PICKUPABLE = ParrotFishTuning.STANDARD_CALORIES_PER_CYCLE / KG_CORAL_PICKUPABLE_EATEN_PER_CYCLE;

	private static float CORAL_PLANT_PER_FISH = KG_CORAL_PICKUPABLE_EATEN_PER_CYCLE / 20f;

	private static float CORAL_GROWTH_EATEN_PER_CYCLE = 0.25f * CORAL_PLANT_PER_FISH;

	private static float CALORIES_PER_GROWTH_EATEN = ParrotFishTuning.STANDARD_CALORIES_PER_CYCLE / (CORAL_GROWTH_EATEN_PER_CYCLE * 4f);

	private static float GROWTH_TO_PRODUCT_EFFICIENCY = CORAL_PICKUPABLE_TO_PRODUCT_EFFICIENCY * 20f;

	private static float MIN_POOP_SIZE_IN_KG = KG_CORAL_PICKUPABLE_EATEN_PER_CYCLE * CORAL_PICKUPABLE_TO_PRODUCT_EFFICIENCY * 0.9f;

	public static GameObject CreatePrefab(string id, string base_trait_id, string name, string description, string anim_file, bool is_baby, string symbol_prefix, float warnLowTemp, float warnHighTemp, float lethalLowTemp, float lethalHighTemp)
	{
		float mASS = ParrotFishTuning.MASS;
		EffectorValues tIER = DECOR.BONUS.TIER0;
		KAnimFile anim = Assets.GetAnim(is_baby ? anim_file : "fish_bioluminescent_build_kanim");
		float defaultTemperature = (warnLowTemp + warnHighTemp) / 2f;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, description, mASS, anim, "idle_loop", Grid.SceneLayer.Creatures, 1, 1, tIER, default(EffectorValues), SimHashes.Creature, null, defaultTemperature);
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.SwimmingCreature);
		component.AddTag(GameTags.Creatures.Swimmer);
		Trait trait = Db.Get().CreateTrait(base_trait_id, name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, ParrotFishTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - ParrotFishTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 25f, name));
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: false, allow_mark_for_capture: true, use_gun_for_pickup: true);
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "fish_bioluminescent_build_kanim", symbol_prefix, FactionManager.FactionID.Prey, base_trait_id, "SwimmerNavGrid", NavType.Swim, 32, 2f, "FishMeat", 1f, drownVulnerable: false, entombVulnerable: false, warnLowTemp, warnHighTemp, lethalLowTemp, lethalHighTemp);
		Light2D light2D = gameObject.AddOrGet<Light2D>();
		light2D.Color = LIGHT2D.PARROTFISH_COLOR;
		light2D.overlayColour = LIGHT2D.PARROTFISH_OVERLAYCOLOR;
		light2D.Range = 5f;
		light2D.Angle = 0f;
		light2D.Direction = LIGHT2D.PARROTFISH_DIRECTION;
		light2D.Offset = LIGHT2D.PARROTFISH_OFFSET;
		light2D.shape = LightShape.Circle;
		light2D.drawOverlay = true;
		light2D.Lux = 5000;
		light2D.IntensityAnimation = 0.2f;
		gameObject.AddOrGet<LightSymbolTracker>().targetSymbol = "snapTo_light_locator";
		gameObject.AddOrGetDef<CreatureLightToggleController.Def>();
		KAnimFile anim2 = Assets.GetAnim("fish_bioluminescent_emotes_kanim");
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
			.Add(new SurfaceAirConsumerStates.Def
			{
				effectId = "SurfaceAirConsumed",
				consumptionRate = 3f,
				consumeDuration = 6f
			})
			.Add(new CritterEmoteStates.Def(anim2))
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		CreatureFallMonitor.Def def = gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		def.canSwim = true;
		def.checkHead = false;
		gameObject.AddOrGetDef<FlopMonitor.Def>();
		gameObject.AddOrGetDef<FishOvercrowdingMonitor.Def>();
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGet<LoopingSounds>();
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.ParrotFishSpecies, symbol_prefix);
		CritterCondoInteractMontior.Def def2 = gameObject.AddOrGetDef<CritterCondoInteractMontior.Def>();
		def2.requireCavity = false;
		def2.condoPrefabTag = "UnderwaterCritterCondo";
		SurfaceAirConsumerMonitor.Def def3 = gameObject.AddOrGetDef<SurfaceAirConsumerMonitor.Def>();
		def3.element = SimHashes.Oxygen;
		def3.minimumMassThreshold = 2f;
		def3.cooldown = 600f;
		Effect effect = new Effect("SurfaceAirConsumed", STRINGS.CREATURES.MODIFIERS.SURFACEAIRCONSUMED.NAME, STRINGS.CREATURES.MODIFIERS.SURFACEAIRCONSUMED.TOOLTIP, 600f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect.Add(new AttributeModifier(Db.Get().CritterAttributes.Happiness.Id, 2f, STRINGS.CREATURES.MODIFIERS.SURFACEAIRCONSUMED.NAME));
		Db.Get().effects.Add(effect);
		Tag produced_element = SimHashes.Lime.CreateTag();
		new HashSet<Tag>();
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add(SimHashes.Phosphorite.CreateTag());
		HashSet<Tag> hashSet2 = new HashSet<Tag>();
		hashSet2.Add("PlanktonCoral");
		Diet diet = new Diet(new List<Diet.Info>
		{
			new Diet.Info(hashSet, produced_element, CALORIES_PER_KG_OF_CORAL_PICKUPABLE, CORAL_PICKUPABLE_TO_PRODUCT_EFFICIENCY),
			new Diet.Info(hashSet2, produced_element, CALORIES_PER_GROWTH_EATEN, GROWTH_TO_PRODUCT_EFFICIENCY, null, 0f, produce_solid_tile: false, Diet.Info.FoodType.EatPlantDirectly)
		}.ToArray());
		CreatureCalorieMonitor.Def def4 = gameObject.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def4.diet = diet;
		def4.minConsumedCaloriesBeforePooping = CALORIES_PER_KG_OF_ORE * MIN_POOP_SIZE_IN_KG;
		gameObject.AddOrGetDef<SolidConsumerMonitor.Def>().diet = diet;
		gameObject.AddOrGetDef<LureableMonitor.Def>().lures = new Tag[1] { GameTags.Creatures.FishTrapLure };
		if (!string.IsNullOrEmpty(symbol_prefix))
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbol_prefix);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["ParrotFish"];
		pickupable.sortOrder = sortOrder;
		return gameObject;
	}

	private static string GetLandAnim(FallStates.Instance smi)
	{
		if (smi.GetSMI<CreatureFallMonitor.Instance>().CanSwimAtCurrentLocation())
		{
			return "idle_loop";
		}
		return "flop_loop";
	}
}
