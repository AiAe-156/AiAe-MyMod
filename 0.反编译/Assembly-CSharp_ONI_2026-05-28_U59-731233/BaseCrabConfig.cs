using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BaseCrabConfig
{
	public const string EMOTION_FILE_NAME = "pincher_emotes_kanim";

	public static GameObject BaseCrab(string id, string name, string desc, string anim_file, string traitId, bool is_baby, string symbolOverridePrefix = null, string onDeathDropID = "CrabShell", float onDeathDropCount = 1f)
	{
		return BaseCrab(id, name, desc, anim_file, traitId, is_baby, symbolOverridePrefix, string.IsNullOrEmpty(onDeathDropID) ? null : new string[1] { onDeathDropID }, new float[1] { onDeathDropCount });
	}

	public static GameObject BaseCrab(string id, string name, string desc, string anim_file, string traitId, bool is_baby, string symbolOverridePrefix, string[] onDeathDropsID, float[] onDeathDropsCount)
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 100f, height: is_baby ? 1 : 2, decor: DECOR.BONUS.TIER0, anim: Assets.GetAnim(is_baby ? anim_file : "pincher_build_kanim"), initialAnim: "idle_loop", sceneLayer: Grid.SceneLayer.Creatures, width: 1);
		string navGridName = "WalkerNavGrid1x2";
		if (is_baby)
		{
			navGridName = "WalkerBabyNavGrid";
		}
		EntityTemplates.ExtendEntityToBasicCreature(new EntityTemplates.ExtendEntityToBasicCreatureData
		{
			isWarmBlooded = false,
			template = gameObject,
			anim_filename = anim_file,
			build_filename = (is_baby ? null : "pincher_build_kanim"),
			symbol_override_prefix = symbolOverridePrefix,
			faction = FactionManager.FactionID.Pest,
			initialTraitID = traitId,
			NavGridName = navGridName,
			onDeathDropsID = onDeathDropsID,
			onDeathDropsCount = onDeathDropsCount,
			entombVulnerable = false,
			drownVulnerable = false,
			warningLowTemperature = 273.15f,
			warningHighTemperature = 313.15f,
			lethalLowTemperature = 223.15f,
			lethalHighTemperature = 373.15f
		});
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["Crab"];
		pickupable.sortOrder = sortOrder;
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		ThreatMonitor.Def def = gameObject.AddOrGetDef<ThreatMonitor.Def>();
		def.fleethresholdState = Health.HealthState.Dead;
		def.friendlyCreatureTags = new Tag[1] { GameTags.Creatures.CrabFriend };
		def.maxSearchDistance = 12;
		def.offsets = CrabTuning.DEFEND_OFFSETS;
		gameObject.AddWeapon(2f, 3f);
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
		component.AddTag(GameTags.Creatures.CrabFriend);
		KAnimFile anim = Assets.GetAnim("pincher_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new GrowUpStates.Def(), is_baby)
			.Add(new TrappedStates.Def())
			.Add(new IncubatingStates.Def(), is_baby)
			.Add(new BaggedStates.Def())
			.Add(new FallStates.Def())
			.Add(new StunnedStates.Def())
			.Add(new DebugGoToStates.Def())
			.Add(new FleeStates.Def())
			.Add(new DefendStates.Def())
			.Add(new AttackStates.Def())
			.PushInterruptGroup()
			.Add(new CreatureSleepStates.Def())
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def(), !is_baby)
			.Add(new LayEggStates.Def(), !is_baby)
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def
			{
				shouldBeBehindMilkTank = true
			})
			.Add(new PoopStates.Def(anim, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP, canComplain: false))
			.Add(new CallAdultStates.Def(), is_baby)
			.Add(new CritterCondoStates.Def
			{
				entersBuilding = false
			}, !is_baby)
			.Add(new CritterEmoteStates.Def(anim))
			.PopInterruptGroup()
			.Add(new CreatureDiseaseCleaner.Def(30f))
			.Add(new IdleStates.Def());
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.CrabSpecies, symbolOverridePrefix);
		CritterCondoInteractMontior.Def def2 = gameObject.AddOrGetDef<CritterCondoInteractMontior.Def>();
		def2.requireCavity = false;
		def2.condoPrefabTag = "UnderwaterCritterCondo";
		gameObject.AddTag(GameTags.Amphibious);
		return gameObject;
	}

	public static List<Diet.Info> BasicDiet(Tag poopTag, float caloriesPerKg, float producedConversionRate, string diseaseId, float diseasePerKgProduced)
	{
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add(SimHashes.ToxicSand.CreateTag());
		hashSet.Add(RotPileConfig.ID.ToTag());
		return new List<Diet.Info>
		{
			new Diet.Info(hashSet, poopTag, caloriesPerKg, producedConversionRate, diseaseId, diseasePerKgProduced)
		};
	}

	public static List<Diet.Info> DietWithSlime(Tag poopTag, float caloriesPerKg, float producedConversionRate, string diseaseId, float diseasePerKgProduced)
	{
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add(SimHashes.ToxicSand.CreateTag());
		hashSet.Add(RotPileConfig.ID.ToTag());
		hashSet.Add(SimHashes.SlimeMold.CreateTag());
		return new List<Diet.Info>
		{
			new Diet.Info(hashSet, poopTag, caloriesPerKg, producedConversionRate, diseaseId, diseasePerKgProduced)
		};
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
