using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BaseBellyConfig
{
	public const string EMOTION_FILE_NAME = "ice_belly_emotes_kanim";

	public static GameObject BaseBelly(string id, string name, string desc, string anim_file, string traitId, bool is_baby, string symbolOverridePrefix = null)
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 400f, decor: DECOR.BONUS.TIER0, anim: Assets.GetAnim(is_baby ? anim_file : "ice_belly_build_kanim"), initialAnim: "idle_loop", sceneLayer: Grid.SceneLayer.Creatures, width: 2, height: 2);
		KBoxCollider2D kBoxCollider2D = gameObject.AddOrGet<KBoxCollider2D>();
		kBoxCollider2D.offset = new Vector2f(0f, kBoxCollider2D.offset.y);
		gameObject.GetComponent<KBatchedAnimController>().Offset = new Vector3(0f, 0f, 0f);
		string navGridName = "WalkerNavGrid2x2";
		if (is_baby)
		{
			navGridName = "WalkerBabyNavGrid";
		}
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: true, gameObject, anim_file, is_baby ? null : "ice_belly_build_kanim", symbolOverridePrefix, FactionManager.FactionID.Pest, traitId, navGridName, NavType.Floor, 32, 2f, "Meat", 14f, drownVulnerable: true, entombVulnerable: false, 303.15f, 343.15f, 173.15f, 373.15f);
		gameObject.AddOrGet<Navigator>();
		if (symbolOverridePrefix != null)
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbolOverridePrefix);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["IceBelly"];
		pickupable.sortOrder = sortOrder;
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		gameObject.AddOrGetDef<WorldSpawnableMonitor.Def>();
		gameObject.AddOrGetDef<ThreatMonitor.Def>().fleethresholdState = Health.HealthState.Dead;
		gameObject.AddWeapon(1f, 1f);
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.Creatures.Walker);
		component.prefabInitFn += delegate(GameObject inst)
		{
			inst.GetAttributes().Add(Db.Get().Attributes.MaxUnderwaterTravelCost);
		};
		bool condition = !is_baby;
		KAnimFile anim = Assets.GetAnim("ice_belly_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new PlayAnimsStates.Def(GameTags.Creatures.Burrowed, loop: true, "idle_mound", STRINGS.CREATURES.STATUSITEMS.BURROWED.NAME, STRINGS.CREATURES.STATUSITEMS.BURROWED.TOOLTIP), condition)
			.Add(new GrowUpStates.Def(), is_baby)
			.Add(new TrappedStates.Def())
			.Add(new IncubatingStates.Def(), is_baby)
			.Add(new BaggedStates.Def())
			.Add(new FallStates.Def())
			.Add(new StunnedStates.Def())
			.Add(new DrowningStates.Def())
			.Add(new DebugGoToStates.Def())
			.Add(new FleeStates.Def())
			.Add(new AttackStates.Def(), condition)
			.PushInterruptGroup()
			.Add(new CreatureSleepStates.Def())
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def
			{
				WaitCellOffset = 2
			}, !is_baby)
			.Add(new PlayAnimsStates.Def(GameTags.Creatures.WantsToEnterBurrow, loop: false, "hide", STRINGS.CREATURES.STATUSITEMS.BURROWING.NAME, STRINGS.CREATURES.STATUSITEMS.BURROWING.TOOLTIP), condition)
			.Add(new LayEggStates.Def(), !is_baby)
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def
			{
				shouldBeBehindMilkTank = false,
				drinkCellOffsetGetFn = (is_baby ? new DrinkMilkStates.Def.DrinkCellOffsetGetFn(DrinkMilkStates.Def.DrinkCellOffsetGet_CritterOneByOne) : new DrinkMilkStates.Def.DrinkCellOffsetGetFn(DrinkMilkStates.Def.DrinkCellOffsetGet_TwoByTwo))
			})
			.Add(new PoopStates.Def(anim, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP, canComplain: false))
			.Add(new CallAdultStates.Def(), is_baby)
			.Add(new CritterCondoStates.Def(), !is_baby)
			.Add(new CritterEmoteStates.Def(anim))
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.BellySpecies, symbolOverridePrefix);
		gameObject.AddOrGet<OccupyArea>().updateWithFacing = !is_baby;
		return gameObject;
	}

	public static Diet.Info BasicDiet(Tag foodTag, Tag poopTag, float caloriesPerKg, float producedConversionRate, string diseaseId, float diseasePerKgProduced)
	{
		return new Diet.Info(new HashSet<Tag> { foodTag }, poopTag, caloriesPerKg, producedConversionRate, diseaseId, diseasePerKgProduced, produce_solid_tile: false, Diet.Info.FoodType.EatPlantDirectly, emmit_disease_on_cell: true);
	}

	public static List<Diet.Info> StandardDiets()
	{
		return new List<Diet.Info>
		{
			BasicDiet("CarrotPlant", "IceBellyPoop", BellyTuning.CALORIES_PER_UNIT_EATEN / BellyTuning.CONSUMABLE_PLANT_MATURITY_LEVELS, 67.474f / BellyTuning.CONSUMABLE_PLANT_MATURITY_LEVELS, BellyTuning.GERM_ID_EMMITED_ON_POOP, 1000f),
			new Diet.Info(new HashSet<Tag> { CarrotConfig.ID }, "IceBellyPoop", BellyTuning.CALORIES_PER_UNIT_EATEN / 1f, 67.474f, BellyTuning.GERM_ID_EMMITED_ON_POOP, 1000f, produce_solid_tile: false, Diet.Info.FoodType.EatSolid, emmit_disease_on_cell: true),
			new Diet.Info(new HashSet<Tag> { "FriesCarrot" }, "IceBellyPoop", FOOD.FOOD_TYPES.FRIES_CARROT.CaloriesPerUnit / 0.75928f, 120.01f, BellyTuning.GERM_ID_EMMITED_ON_POOP, 1000f, produce_solid_tile: false, Diet.Info.FoodType.EatSolid, emmit_disease_on_cell: true),
			BasicDiet("BeanPlant", "IceBellyPoop", BellyTuning.CALORIES_PER_UNIT_EATEN / BellyTuning.CONSUMABLE_PLANT_MATURITY_LEVELS / 0.744f, 67.474f / BellyTuning.CONSUMABLE_PLANT_MATURITY_LEVELS / 0.744f, BellyTuning.GERM_ID_EMMITED_ON_POOP, 1000f),
			new Diet.Info(new HashSet<Tag> { "BeanPlantSeed" }, "IceBellyPoop", 1100000f, 18.570995f, BellyTuning.GERM_ID_EMMITED_ON_POOP, 1000f, produce_solid_tile: false, Diet.Info.FoodType.EatSolid, emmit_disease_on_cell: true)
		};
	}

	public static GameObject SetupDiet(GameObject prefab, List<Diet.Info> diet_infos, float referenceCaloriesPerKg, float minPoopSizeInKg)
	{
		Diet diet = new Diet(diet_infos.ToArray());
		CreatureCalorieMonitor.Def def = prefab.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def.diet = diet;
		def.minConsumedCaloriesBeforePooping = referenceCaloriesPerKg * minPoopSizeInKg;
		prefab.AddOrGetDef<SolidConsumerMonitor.Def>().diet = diet;
		return prefab;
	}
}
