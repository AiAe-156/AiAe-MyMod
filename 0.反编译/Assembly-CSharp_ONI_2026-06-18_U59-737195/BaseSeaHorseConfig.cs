using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BaseSeaHorseConfig
{
	public const string EMOTION_FILE_NAME = "seahorse_emotes_kanim";

	private const float CLAMS_EATEN_PER_CYCLE = 0.5f;

	private static float KG_PEARL_EATEN_PER_CYCLE = 3.125f;

	private static float CALORIES_PER_KG_OF_PEARL = SeaHorseTuning.STANDARD_CALORIES_PER_CYCLE / KG_PEARL_EATEN_PER_CYCLE;

	public const float SLIME_PER_CYCLE = 12f;

	public static float OUTPUT_EFFICIENCY = 12f / KG_PEARL_EATEN_PER_CYCLE;

	private static float MIN_POOP_SIZE_IN_KG = 25f;

	public static GameObject CreatePrefab(string id, string base_trait_id, string name, string description, string anim_file, bool is_baby, string symbol_prefix, float warnLowTemp, float warnHighTemp, float lethalLowTemp, float lethalHighTemp)
	{
		float mASS = SeaHorseTuning.MASS;
		int height = (is_baby ? 1 : 2);
		EffectorValues tIER = DECOR.BONUS.TIER3;
		KAnimFile anim = Assets.GetAnim(is_baby ? anim_file : "seahorse_build_kanim");
		float defaultTemperature = (warnLowTemp + warnHighTemp) / 2f;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, description, mASS, anim, "idle_loop", Grid.SceneLayer.Creatures, 1, height, tIER, default(EffectorValues), SimHashes.Creature, null, defaultTemperature);
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.SwimmingCreature);
		component.AddTag(GameTags.Creatures.Swimmer);
		Trait trait = Db.Get().CreateTrait(base_trait_id, name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, SeaHorseTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - SeaHorseTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 100f, name));
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: false, allow_mark_for_capture: true, use_gun_for_pickup: true);
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "seahorse_build_kanim", symbol_prefix, FactionManager.FactionID.Prey, base_trait_id, is_baby ? "SwimmerNavGrid" : "SwimmerNavGrid1x2", NavType.Swim, 32, 2f, "FishMeat", 1f, drownVulnerable: false, entombVulnerable: false, warnLowTemp, warnHighTemp, lethalLowTemp, lethalHighTemp);
		KAnimFile anim2 = Assets.GetAnim("seahorse_emotes_kanim");
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
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.SeaHorseSpecies, symbol_prefix);
		CritterCondoInteractMontior.Def def = gameObject.AddOrGetDef<CritterCondoInteractMontior.Def>();
		def.requireCavity = false;
		def.condoPrefabTag = "UnderwaterCritterCondo";
		Tag produced_element = SimHashes.SlimeMold.CreateTag();
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add(SimHashes.Pearl.CreateTag());
		Diet diet = new Diet(new List<Diet.Info>
		{
			new Diet.Info(hashSet, produced_element, CALORIES_PER_KG_OF_PEARL, OUTPUT_EFFICIENCY)
		}.ToArray());
		CreatureCalorieMonitor.Def def2 = gameObject.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def2.diet = diet;
		def2.minConsumedCaloriesBeforePooping = CALORIES_PER_KG_OF_PEARL * MIN_POOP_SIZE_IN_KG;
		gameObject.AddOrGetDef<SolidConsumerMonitor.Def>().diet = diet;
		gameObject.AddOrGetDef<LureableMonitor.Def>().lures = new Tag[1] { GameTags.Creatures.FishTrapLure };
		if (!string.IsNullOrEmpty(symbol_prefix))
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbol_prefix);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["SeaHorse"];
		pickupable.sortOrder = sortOrder;
		if (!is_baby)
		{
			FertilityShearable.Def def3 = gameObject.AddOrGetDef<FertilityShearable.Def>();
			def3.dropMass = 100f;
			def3.milkElement = SimHashes.FishMilk;
			def3.minimumFertility = 75f;
			def3.percentFertilityConsumedPerMilking = 0.5f;
			def3.requiresHappy = true;
			def3.suppressedByElderly = true;
		}
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
