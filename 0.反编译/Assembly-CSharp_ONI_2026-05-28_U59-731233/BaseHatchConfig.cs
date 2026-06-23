using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BaseHatchConfig
{
	public const string EMOTION_FILE_NAME = "hatch_emotes_kanim";

	public static GameObject BaseHatch(string id, string name, string desc, string anim_file, string traitId, bool is_baby, string symbolOverridePrefix = null)
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 100f, decor: DECOR.BONUS.TIER0, anim: Assets.GetAnim(is_baby ? anim_file : "hatch_build_kanim"), initialAnim: "idle_loop", sceneLayer: Grid.SceneLayer.Creatures, width: 1, height: 1);
		string navGridName = "WalkerNavGrid1x1";
		if (is_baby)
		{
			navGridName = "WalkerBabyNavGrid";
		}
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "hatch_build_kanim", symbolOverridePrefix, FactionManager.FactionID.Pest, traitId, navGridName, NavType.Floor, 32, 2f, "Meat", 2f, drownVulnerable: true, entombVulnerable: false, 283.15f, 313.15f, 228.15f, 373.15f);
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["Hatch"];
		pickupable.sortOrder = sortOrder;
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		gameObject.AddOrGetDef<BurrowMonitor.Def>();
		WorldSpawnableMonitor.Def def = gameObject.AddOrGetDef<WorldSpawnableMonitor.Def>();
		def.adjustSpawnLocationCb = AdjustSpawnLocationCB;
		ThreatMonitor.Def def2 = gameObject.AddOrGetDef<ThreatMonitor.Def>();
		def2.fleethresholdState = Health.HealthState.Dead;
		gameObject.AddWeapon(1f, 1f);
		SoundEventVolumeCache.instance.AddVolume("hatch_kanim", "Hatch_voice_idle", NOISE_POLLUTION.CREATURES.TIER2);
		SoundEventVolumeCache.instance.AddVolume("FloorSoundEvent", "Hatch_footstep", NOISE_POLLUTION.CREATURES.TIER1);
		SoundEventVolumeCache.instance.AddVolume("hatch_kanim", "Hatch_land", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("hatch_kanim", "Hatch_chew", NOISE_POLLUTION.CREATURES.TIER3);
		SoundEventVolumeCache.instance.AddVolume("hatch_kanim", "Hatch_voice_hurt", NOISE_POLLUTION.CREATURES.TIER5);
		SoundEventVolumeCache.instance.AddVolume("hatch_kanim", "Hatch_voice_die", NOISE_POLLUTION.CREATURES.TIER5);
		SoundEventVolumeCache.instance.AddVolume("hatch_kanim", "Hatch_drill_emerge", NOISE_POLLUTION.CREATURES.TIER6);
		SoundEventVolumeCache.instance.AddVolume("hatch_kanim", "Hatch_drill_hide", NOISE_POLLUTION.CREATURES.TIER6);
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.Creatures.Walker);
		component.prefabInitFn += delegate(GameObject inst)
		{
			inst.GetAttributes().Add(Db.Get().Attributes.MaxUnderwaterTravelCost);
		};
		bool condition = !is_baby;
		KAnimFile anim = Assets.GetAnim("hatch_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new ExitBurrowStates.Def(), condition)
			.Add(new PlayAnimsStates.Def(GameTags.Creatures.Burrowed, loop: true, "idle_mound", STRINGS.CREATURES.STATUSITEMS.BURROWED.NAME, STRINGS.CREATURES.STATUSITEMS.BURROWED.TOOLTIP), condition)
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
			.Add(new RanchedStates.Def(), !is_baby)
			.Add(new PlayAnimsStates.Def(GameTags.Creatures.WantsToEnterBurrow, loop: false, "hide", STRINGS.CREATURES.STATUSITEMS.BURROWING.NAME, STRINGS.CREATURES.STATUSITEMS.BURROWING.TOOLTIP), condition)
			.Add(new LayEggStates.Def(), !is_baby)
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def
			{
				shouldBeBehindMilkTank = is_baby
			})
			.Add(new PoopStates.Def(anim, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP, canComplain: false))
			.Add(new CallAdultStates.Def(), is_baby)
			.Add(new CritterCondoStates.Def(), !is_baby)
			.Add(new CritterEmoteStates.Def(anim), !is_baby)
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.HatchSpecies, symbolOverridePrefix);
		return gameObject;
	}

	public static List<Diet.Info> BasicRockDiet(Tag poopTag, float caloriesPerKg, float producedConversionRate, string diseaseId, float diseasePerKgProduced)
	{
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add(SimHashes.Sand.CreateTag());
		hashSet.Add(SimHashes.SandStone.CreateTag());
		hashSet.Add(SimHashes.Clay.CreateTag());
		hashSet.Add(SimHashes.CrushedRock.CreateTag());
		hashSet.Add(SimHashes.Dirt.CreateTag());
		hashSet.Add(SimHashes.SedimentaryRock.CreateTag());
		hashSet.Add(SimHashes.Shale.CreateTag());
		hashSet.Add(SimHashes.SiltStone.CreateTag());
		hashSet.Add(SimHashes.Coquina.CreateTag());
		return new List<Diet.Info>
		{
			new Diet.Info(hashSet, poopTag, caloriesPerKg, producedConversionRate, diseaseId, diseasePerKgProduced)
		};
	}

	public static List<Diet.Info> HardRockDiet(Tag poopTag, float caloriesPerKg, float producedConversionRate, string diseaseId, float diseasePerKgProduced)
	{
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add(SimHashes.SedimentaryRock.CreateTag());
		hashSet.Add(SimHashes.IgneousRock.CreateTag());
		hashSet.Add(SimHashes.Obsidian.CreateTag());
		hashSet.Add(SimHashes.Granite.CreateTag());
		hashSet.Add(SimHashes.Basalt.CreateTag());
		return new List<Diet.Info>
		{
			new Diet.Info(hashSet, poopTag, caloriesPerKg, producedConversionRate, diseaseId, diseasePerKgProduced)
		};
	}

	public static List<Diet.Info> MetalDiet(Tag poopTag, float caloriesPerKg, float producedConversionRate, string diseaseId, float diseasePerKgProduced)
	{
		List<Diet.Info> list = new List<Diet.Info>();
		Tag[] basicMetalOres = GameTags.BasicMetalOres;
		foreach (Tag tag in basicMetalOres)
		{
			Tag produced_element = ((poopTag == GameTags.Metal) ? ElementLoader.FindElementByTag(tag).refinedMetalTarget.CreateTag() : poopTag);
			list.Add(new Diet.Info(new HashSet<Tag>(new Tag[1] { tag }), produced_element, caloriesPerKg, producedConversionRate, diseaseId, diseasePerKgProduced));
		}
		list.Add(new Diet.Info(new HashSet<Tag>(new Tag[1] { SimHashes.GoldAmalgam.CreateTag() }), (poopTag == GameTags.Metal) ? SimHashes.Gold.CreateTag() : poopTag, caloriesPerKg, producedConversionRate, diseaseId, diseasePerKgProduced));
		list.Add(new Diet.Info(new HashSet<Tag>(new Tag[1] { SimHashes.Wolframite.CreateTag() }), (poopTag == GameTags.Metal) ? SimHashes.Tungsten.CreateTag() : poopTag, caloriesPerKg, producedConversionRate, diseaseId, diseasePerKgProduced));
		return list;
	}

	public static List<Diet.Info> VeggieDiet(Tag poopTag, float caloriesPerKg, float producedConversionRate, string diseaseId, float diseasePerKgProduced)
	{
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add(SimHashes.Dirt.CreateTag());
		hashSet.Add(SimHashes.SlimeMold.CreateTag());
		hashSet.Add(SimHashes.Algae.CreateTag());
		hashSet.Add(SimHashes.Fertilizer.CreateTag());
		hashSet.Add(SimHashes.ToxicSand.CreateTag());
		hashSet.Add(SimHashes.ToxicMud.CreateTag());
		hashSet.Add(SimHashes.Corallium.CreateTag());
		return new List<Diet.Info>
		{
			new Diet.Info(hashSet, poopTag, caloriesPerKg, producedConversionRate, diseaseId, diseasePerKgProduced)
		};
	}

	public static List<Diet.Info> FoodDiet(Tag poopTag, float caloriesPerKg, float producedConversionRate, string diseaseId, float diseasePerKgProduced)
	{
		List<Diet.Info> list = new List<Diet.Info>();
		foreach (EdiblesManager.FoodInfo allLoadedFoodType in EdiblesManager.GetAllLoadedFoodTypes())
		{
			if (allLoadedFoodType.CaloriesPerUnit > 0f)
			{
				HashSet<Tag> hashSet = new HashSet<Tag>();
				hashSet.Add(new Tag(allLoadedFoodType.Id));
				list.Add(new Diet.Info(hashSet, poopTag, allLoadedFoodType.CaloriesPerUnit, producedConversionRate, diseaseId, diseasePerKgProduced));
			}
		}
		return list;
	}

	public static GameObject SetupDiet(GameObject prefab, List<Diet.Info> diet_infos, float referenceCaloriesPerKg, float minPoopSizeInKg)
	{
		Diet diet = new Diet(diet_infos.ToArray());
		CreatureCalorieMonitor.Def def = prefab.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def.diet = diet;
		def.minConsumedCaloriesBeforePooping = referenceCaloriesPerKg * minPoopSizeInKg;
		SolidConsumerMonitor.Def def2 = prefab.AddOrGetDef<SolidConsumerMonitor.Def>();
		def2.diet = diet;
		return prefab;
	}

	private static int AdjustSpawnLocationCB(int cell)
	{
		while (!Grid.Solid[cell])
		{
			int num = Grid.CellBelow(cell);
			if (!Grid.IsValidCell(cell))
			{
				break;
			}
			cell = num;
		}
		return cell;
	}
}
