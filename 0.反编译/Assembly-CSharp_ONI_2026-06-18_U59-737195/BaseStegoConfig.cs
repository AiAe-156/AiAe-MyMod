using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BaseStegoConfig
{
	public const string EMOTION_FILE_NAME = "stego_emotes_kanim";

	public static GameObject BaseStego(string id, string name, string desc, string anim_file, string traitId, bool is_baby, string symbolOverridePrefix = null)
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 400f, decor: DECOR.BONUS.TIER0, anim: Assets.GetAnim(is_baby ? anim_file : "stego_build_kanim"), initialAnim: "idle_loop", sceneLayer: Grid.SceneLayer.Creatures, width: 2, height: 2);
		KBoxCollider2D kBoxCollider2D = gameObject.AddOrGet<KBoxCollider2D>();
		kBoxCollider2D.offset = new Vector2f(0f, kBoxCollider2D.offset.y);
		gameObject.GetComponent<KBatchedAnimController>().Offset = new Vector3(0f, 0f, 0f);
		string navGridName = "WalkerNavGrid2x2";
		if (is_baby)
		{
			navGridName = "WalkerBabyNavGrid";
		}
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "stego_build_kanim", symbolOverridePrefix, FactionManager.FactionID.Pest, traitId, navGridName, NavType.Floor, 32, 1.5f, "DinosaurMeat", 12f, drownVulnerable: true, entombVulnerable: false, 293.15f, 343.15f, 173.15f, 373.15f);
		if (symbolOverridePrefix != null)
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbolOverridePrefix);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["Stego"];
		pickupable.sortOrder = sortOrder;
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		gameObject.AddOrGetDef<CritterRoarMonitor.Def>().Initialize(StegoTuning.ROARS_PER_CYCLE, StegoTuning.ROAR_COOLDOWN);
		gameObject.AddOrGetDef<ThreatMonitor.Def>().fleethresholdState = Health.HealthState.Dead;
		gameObject.AddWeapon(1f, 1f);
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.Creatures.Walker);
		component.prefabInitFn += delegate(GameObject inst)
		{
			inst.GetAttributes().Add(Db.Get().Attributes.MaxUnderwaterTravelCost);
		};
		if (!is_baby)
		{
			StompMonitor.Def def = gameObject.AddOrGetDef<StompMonitor.Def>();
			def.Cooldown = 60f;
			def.radius = 10;
		}
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		KAnimFile anim = Assets.GetAnim("stego_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new GrowUpStates.Def(), is_baby)
			.Add(new TrappedStates.Def())
			.Add(new IncubatingStates.Def(), is_baby)
			.Add(new BaggedStates.Def())
			.Add(new FallStates.Def())
			.Add(new StunnedStates.Def())
			.Add(new DrowningStates.Def())
			.Add(new DebugGoToStates.Def())
			.Add(new FleeStates.Def())
			.Add(new AttackStates.Def())
			.PushInterruptGroup()
			.Add(new CreatureSleepStates.Def(), is_baby)
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def
			{
				WaitCellOffset = 2
			}, !is_baby)
			.Add(new LayEggStates.Def(), !is_baby)
			.Add(new StompStates.Def(), !is_baby)
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def
			{
				shouldBeBehindMilkTank = false,
				drinkCellOffsetGetFn = (is_baby ? new DrinkMilkStates.Def.DrinkCellOffsetGetFn(DrinkMilkStates.Def.DrinkCellOffsetGet_CritterOneByOne) : new DrinkMilkStates.Def.DrinkCellOffsetGetFn(DrinkMilkStates.Def.DrinkCellOffsetGet_TwoByTwo))
			})
			.Add(new PoopStates.Def(anim, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP, canComplain: false))
			.Add(new CallAdultStates.Def(), is_baby)
			.Add(new CritterCondoStates.Def(), !is_baby)
			.Add(new CritterEmoteStates.Def(anim), !is_baby)
			.Add(new CritterRoarStates.Def())
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.StegoSpecies, symbolOverridePrefix);
		gameObject.AddOrGet<OccupyArea>().updateWithFacing = !is_baby;
		return gameObject;
	}

	public static List<Diet.Info> StandardDiets()
	{
		List<Diet.Info> list = new List<Diet.Info>();
		list.Add(new Diet.Info(new HashSet<Tag> { VineFruitConfig.ID }, StegoTuning.POOP_ELEMENT, StegoTuning.CALORIES_PER_KG_OF_ORE, StegoTuning.PEAT_PRODUCED_PER_CYCLE / StegoTuning.VINE_FOOD_PER_CYCLE));
		float num = FOOD.FOOD_TYPES.PRICKLEFRUIT.CaloriesPerUnit / FOOD.FOOD_TYPES.VINEFRUIT.CaloriesPerUnit;
		list.Add(new Diet.Info(new HashSet<Tag> { PrickleFruitConfig.ID }, StegoTuning.POOP_ELEMENT, StegoTuning.CALORIES_PER_KG_OF_ORE * num, StegoTuning.PEAT_PRODUCED_PER_CYCLE / (StegoTuning.VINE_FOOD_PER_CYCLE / num)));
		if (DlcManager.IsExpansion1Active())
		{
			float num2 = FOOD.FOOD_TYPES.SWAMPFRUIT.CaloriesPerUnit / FOOD.FOOD_TYPES.VINEFRUIT.CaloriesPerUnit;
			float num3 = 1.5f;
			list.Add(new Diet.Info(new HashSet<Tag> { SwampFruitConfig.ID }, StegoTuning.POOP_ELEMENT, StegoTuning.CALORIES_PER_KG_OF_ORE * num2, StegoTuning.PEAT_PRODUCED_PER_CYCLE / (StegoTuning.VINE_FOOD_PER_CYCLE / num2) * num3));
		}
		return list;
	}

	public static GameObject SetupDiet(GameObject prefab, List<Diet.Info> dietInfos, float referenceCaloriesPerKg, float minPoopSizeInKg)
	{
		Diet diet = new Diet(dietInfos.ToArray());
		CreatureCalorieMonitor.Def def = prefab.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def.diet = diet;
		def.minConsumedCaloriesBeforePooping = referenceCaloriesPerKg * minPoopSizeInKg;
		prefab.AddOrGetDef<SolidConsumerMonitor.Def>().diet = diet;
		return prefab;
	}
}
