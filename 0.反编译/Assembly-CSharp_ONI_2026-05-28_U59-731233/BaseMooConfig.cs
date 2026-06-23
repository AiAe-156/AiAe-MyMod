using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BaseMooConfig
{
	public const string EMOTION_FILE_NAME = "gassy_moo_emotes_kanim";

	public static GameObject BaseMoo(string id, string name, string desc, string traitId, string anim_file, List<BeckoningMonitor.SongChance> initialSongChances, bool is_baby, string symbol_override_prefix)
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 50f, decor: DECOR.BONUS.TIER0, anim: Assets.GetAnim(is_baby ? anim_file : "gassy_moo_build_kanim"), initialAnim: "idle_loop", sceneLayer: Grid.SceneLayer.Creatures, width: 2, height: 2);
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "gassy_moo_build_kanim", symbol_override_prefix, FactionManager.FactionID.Prey, traitId, "FlyerNavGrid2x2", NavType.Hover, 32, 2f, "Meat", 10f, drownVulnerable: true, entombVulnerable: true, 223.15f, 323.15f, 73.149994f, 473.15f);
		if (!string.IsNullOrEmpty(symbol_override_prefix))
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbol_override_prefix);
		}
		if (!is_baby)
		{
			KBoxCollider2D kBoxCollider2D = gameObject.AddOrGet<KBoxCollider2D>();
			kBoxCollider2D.offset = new Vector2f(0f, kBoxCollider2D.offset.y);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["Moo"];
		pickupable.sortOrder = sortOrder;
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.Creatures.Flyer);
		component.prefabInitFn += delegate(GameObject inst)
		{
			inst.GetAttributes().Add(Db.Get().Attributes.MaxUnderwaterTravelCost);
		};
		BeckoningMonitor.Def def = gameObject.AddOrGetDef<BeckoningMonitor.Def>();
		def.initialSongWeights = initialSongChances;
		def.caloriesPerCycle = MooTuning.WELLFED_CALORIES_PER_CYCLE;
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<Trappable>();
		LureableMonitor.Def def2 = gameObject.AddOrGetDef<LureableMonitor.Def>();
		def2.lures = new Tag[2]
		{
			SimHashes.BleachStone.CreateTag(),
			GameTags.Creatures.FlyersLure
		};
		gameObject.AddOrGetDef<ThreatMonitor.Def>();
		gameObject.AddOrGetDef<SubmergedMonitor.Def>();
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		gameObject.AddOrGetDef<RanchableMonitor.Def>();
		gameObject.AddOrGetDef<FixedCapturableMonitor.Def>();
		MilkProductionMonitor.Def def3 = gameObject.AddOrGetDef<MilkProductionMonitor.Def>();
		def3.CaloriesPerCycle = MooTuning.WELLFED_CALORIES_PER_CYCLE;
		def3.Capacity = MooTuning.MILK_CAPACITY;
		KAnimFile anim = Assets.GetAnim("gassy_moo_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new TrappedStates.Def())
			.Add(new BaggedStates.Def())
			.Add(new StunnedStates.Def())
			.Add(new DebugGoToStates.Def())
			.Add(new DrowningStates.Def())
			.PushInterruptGroup()
			.Add(new BeckonFromSpaceStates.Def())
			.Add(new CreatureSleepStates.Def())
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def
			{
				WaitCellOffset = 2
			})
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def
			{
				shouldBeBehindMilkTank = false,
				drinkCellOffsetGetFn = DrinkMilkStates.Def.DrinkCellOffsetGet_GassyMoo
			})
			.Add(new PoopStates.Def(anim, STRINGS.CREATURES.STATUSITEMS.EXPELLING_GAS.NAME, STRINGS.CREATURES.STATUSITEMS.EXPELLING_GAS.TOOLTIP, canComplain: false))
			.Add(new MoveToLureStates.Def())
			.Add(new CritterCondoStates.Def
			{
				working_anim = "cc_working_moo"
			}, !is_baby)
			.Add(new CritterEmoteStates.Def(anim), !is_baby)
			.PopInterruptGroup()
			.Add(new IdleStates.Def
			{
				customIdleAnim = CustomIdleAnim
			});
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.MooSpecies, symbol_override_prefix);
		CritterCondoInteractMontior.Def def4 = gameObject.AddOrGetDef<CritterCondoInteractMontior.Def>();
		def4.condoPrefabTag = "AirBorneCritterCondo";
		return gameObject;
	}

	public static void SetupBaseDiet(GameObject prefab, Tag producedTag)
	{
		Diet diet = ExpandDiet(null, prefab, "GasGrass".ToTag(), producedTag, MooTuning.CALORIES_PER_DAY_OF_PLANT_EATEN, MooTuning.KG_POOP_PER_DAY_OF_PLANT, Diet.Info.FoodType.EatPlantDirectly, MooTuning.MIN_POOP_SIZE_IN_KG);
		diet = ExpandDiet(diet, prefab, "PlantFiber".ToTag(), producedTag, MooTuning.CALORIES_PER_DAY_OF_SOLID_EATEN, MooTuning.POOP_KG_COVERSION_RATE_FOR_SOLID_DIET, Diet.Info.FoodType.EatSolid, MooTuning.MIN_POOP_SIZE_IN_KG);
		CreatureCalorieMonitor.Def def = prefab.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def.diet = diet;
		def.minConsumedCaloriesBeforePooping = MooTuning.MIN_POOP_SIZE_IN_CALORIES;
		SolidConsumerMonitor.Def def2 = prefab.AddOrGetDef<SolidConsumerMonitor.Def>();
		def2.diet = diet;
	}

	public static Diet ExpandDiet(Diet diet, GameObject prefab, Tag consumed_tag, Tag producedTag, float caloriesPerKg, float producedConversionRate, Diet.Info.FoodType foodType, float minPoopSizeInKg)
	{
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add(consumed_tag);
		Diet.Info[] array = ((diet != null) ? new Diet.Info[diet.infos.Length + 1] : new Diet.Info[1]);
		if (diet != null)
		{
			for (int i = 0; i < diet.infos.Length; i++)
			{
				array[i] = diet.infos[i];
			}
		}
		array[^1] = new Diet.Info(hashSet, producedTag, caloriesPerKg, producedConversionRate, null, 0f, produce_solid_tile: false, foodType);
		return new Diet(array);
	}

	private static HashedString CustomIdleAnim(IdleStates.Instance smi, ref HashedString pre_anim)
	{
		CreatureCalorieMonitor.Instance sMI = smi.GetSMI<CreatureCalorieMonitor.Instance>();
		return (sMI != null && sMI.stomach.IsReadyToPoop()) ? "idle_loop_full" : "idle_loop";
	}

	public static void OnSpawn(GameObject inst)
	{
		Navigator component = inst.GetComponent<Navigator>();
		component.transitionDriver.overrideLayers.Add(new FullPuftTransitionLayer(component));
	}
}
