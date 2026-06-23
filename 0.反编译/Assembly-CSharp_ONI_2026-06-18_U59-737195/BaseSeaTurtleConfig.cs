using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BaseSeaTurtleConfig
{
	public const string EMOTION_FILE_NAME = "sea_fairy_emotes_kanim";

	private static float CALORIES_PER_KG_OF_FAIRY = SeaTurtleTuning.STANDARD_CALORIES_PER_CYCLE / 2f;

	public static GameObject CreatePrefab(string id, string base_trait_id, string name, string description, string anim_file, bool is_baby, string symbol_prefix, float warnLowTemp, float warnHighTemp, float lethalLowTemp, float lethalHighTemp)
	{
		int width = (is_baby ? 1 : 2);
		int height = (is_baby ? 1 : 2);
		EffectorValues tIER = DECOR.BONUS.TIER0;
		KAnimFile anim = Assets.GetAnim(is_baby ? anim_file : "turtle_build_kanim");
		float defaultTemperature = (warnLowTemp + warnHighTemp) / 2f;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, description, 200f, anim, "idle_loop", Grid.SceneLayer.Creatures, width, height, tIER, default(EffectorValues), SimHashes.Creature, null, defaultTemperature);
		if (!is_baby)
		{
			KBoxCollider2D kBoxCollider2D = gameObject.AddOrGet<KBoxCollider2D>();
			kBoxCollider2D.offset = new Vector2f(0f, kBoxCollider2D.offset.y);
		}
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.SwimmingCreature);
		component.AddTag(GameTags.Creatures.Swimmer);
		Trait trait = Db.Get().CreateTrait(base_trait_id, name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, SeaTurtleTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - SeaTurtleTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 100f, name));
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "turtle_build_kanim", null, FactionManager.FactionID.Prey, base_trait_id, is_baby ? "SwimmerNavGrid" : "SwimmerGrid2x2", NavType.Swim, 32, 2f, "ShellfishMeat", 6f, drownVulnerable: false, entombVulnerable: false, warnLowTemp, warnHighTemp, lethalLowTemp, lethalHighTemp);
		KAnimFile anim2 = Assets.GetAnim("sea_fairy_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new GrowUpStates.Def(), is_baby)
			.Add(new TrappedStates.Def())
			.Add(new IncubatingStates.Def(), is_baby)
			.Add(new BaggedStates.Def())
			.Add(new FallStates.Def
			{
				getLandAnim = GetLandAnim
			})
			.Add(new DebugGoToStates.Def())
			.Add(new FlopStates.Def
			{
				flipFacing = true
			})
			.PushInterruptGroup()
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def(), !is_baby)
			.Add(new LayEggStates.Def(), !is_baby)
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def
			{
				shouldBeBehindMilkTank = false,
				drinkCellOffsetGetFn = (is_baby ? new DrinkMilkStates.Def.DrinkCellOffsetGetFn(DrinkMilkStates.Def.DrinkCellOffsetGet_CritterOneByOne) : new DrinkMilkStates.Def.DrinkCellOffsetGetFn(DrinkMilkStates.Def.DrinkCellOffsetGet_TwoByTwo))
			})
			.Add(new PoopStates.Def(anim2, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP, canComplain: false))
			.Add(new MoveToLureStates.Def())
			.Add(new CritterCondoStates.Def
			{
				fgLayer = CritterCondo.CreatureFGLayerType.SmallCreatureLayer
			}, !is_baby)
			.Add(new CritterEmoteStates.Def(anim2))
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		gameObject.AddOrGetDef<CreatureFallMonitor.Def>().canSwim = true;
		gameObject.AddOrGetDef<FlopMonitor.Def>();
		gameObject.AddOrGetDef<FishOvercrowdingMonitor.Def>();
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGetDef<LureableMonitor.Def>().lures = new Tag[1] { GameTags.Creatures.FishTrapLure };
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.SeaTurtleSpecies, symbol_prefix);
		CritterCondoInteractMontior.Def def = gameObject.AddOrGetDef<CritterCondoInteractMontior.Def>();
		def.requireCavity = false;
		def.condoPrefabTag = "UnderwaterCritterCondo";
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add("SeaFairy");
		HashSet<Tag> hashSet2 = new HashSet<Tag>();
		hashSet2.Add("Nori");
		Diet diet = new Diet(new List<Diet.Info>
		{
			new Diet.Info(hashSet, SeaTurtleTuning.POOP_ELEMENT, 80000f, SeaTurtleTuning.POOP_KG_PER_CYCLE / 2f / 5f, null, 0f, produce_solid_tile: false, Diet.Info.FoodType.EatPrey),
			new Diet.Info(hashSet2, SeaTurtleTuning.POOP_ELEMENT, 40000f, SeaTurtleTuning.POOP_KG_PER_CYCLE / 2f / 10f)
		}.ToArray());
		CreatureCalorieMonitor.Def def2 = gameObject.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def2.diet = diet;
		def2.minConsumedCaloriesBeforePooping = SeaTurtleTuning.STANDARD_CALORIES_PER_CYCLE;
		SolidConsumerMonitor.Def def3 = gameObject.AddOrGetDef<SolidConsumerMonitor.Def>();
		def3.diet = diet;
		def3.sportHuntWhenOvercrowded = true;
		if (!string.IsNullOrEmpty(symbol_prefix))
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbol_prefix);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["SeaTurtle"];
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
