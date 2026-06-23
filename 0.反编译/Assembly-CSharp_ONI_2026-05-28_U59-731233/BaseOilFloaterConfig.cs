using System.Collections.Generic;
using Klei.AI;
using TUNING;
using UnityEngine;

public static class BaseOilFloaterConfig
{
	public const string EMOTION_FILE_NAME = "oilfloater_emotes_kanim";

	public static GameObject BaseOilFloater(string id, string name, string desc, string anim_file, string traitId, float warnLowTemp, float warnHighTemp, float lethalLowTemp, float lethalHighTemp, bool is_baby, string symbolOverridePrefix = null)
	{
		EffectorValues tIER = DECOR.BONUS.TIER1;
		KAnimFile anim = Assets.GetAnim(is_baby ? anim_file : "oilfloater_build_kanim");
		float defaultTemperature = (warnLowTemp + warnHighTemp) / 2f;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 50f, anim, "idle_loop", Grid.SceneLayer.Creatures, 1, 1, tIER, default(EffectorValues), SimHashes.Creature, null, defaultTemperature);
		gameObject.GetComponent<KPrefabID>().AddTag(GameTags.Creatures.Hoverer);
		gameObject.GetComponent<KPrefabID>().prefabInitFn += delegate(GameObject inst)
		{
			inst.GetAttributes().Add(Db.Get().Attributes.MaxUnderwaterTravelCost);
		};
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "oilfloater_build_kanim", symbolOverridePrefix, FactionManager.FactionID.Pest, traitId, "FloaterNavGrid", NavType.Hover, 32, 2f, "Meat", 2f, drownVulnerable: true, entombVulnerable: false, warnLowTemp, warnHighTemp, lethalLowTemp, lethalHighTemp);
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = CREATURES.SORTING.CRITTER_ORDER["Oilfloater"];
		pickupable.sortOrder = sortOrder;
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGetDef<ThreatMonitor.Def>();
		gameObject.AddOrGetDef<SubmergedMonitor.Def>();
		CreatureFallMonitor.Def def = gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		def.canSwim = true;
		gameObject.AddWeapon(1f, 1f);
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		string inhaleSound = "OilFloater_intake_air";
		if (is_baby)
		{
			inhaleSound = "OilFloaterBaby_intake_air";
		}
		KAnimFile anim2 = Assets.GetAnim("oilfloater_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new GrowUpStates.Def(), is_baby)
			.Add(new TrappedStates.Def())
			.Add(new IncubatingStates.Def(), is_baby)
			.Add(new BaggedStates.Def())
			.Add(new FallStates.Def())
			.Add(new StunnedStates.Def())
			.Add(new DrowningStates.Def())
			.Add(new DebugGoToStates.Def())
			.PushInterruptGroup()
			.Add(new CreatureSleepStates.Def())
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def(), !is_baby)
			.Add(new LayEggStates.Def(), !is_baby)
			.Add(new InhaleStates.Def
			{
				inhaleSound = inhaleSound
			})
			.Add(new DrinkMilkStates.Def())
			.Add(new SameSpotPoopStates.Def())
			.Add(new CallAdultStates.Def(), is_baby)
			.Add(new CritterCondoStates.Def(), !is_baby)
			.Add(new CritterEmoteStates.Def(anim2))
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.OilFloaterSpecies, symbolOverridePrefix);
		string sound = "OilFloater_move_LP";
		if (is_baby)
		{
			sound = "OilFloaterBaby_move_LP";
		}
		gameObject.AddOrGet<OilFloaterMovementSound>().sound = sound;
		return gameObject;
	}

	public static GameObject SetupDiet(GameObject prefab, Tag consumed_tag, Tag producedTag, float caloriesPerKg, float producedConversionRate, string diseaseId, float diseasePerKgProduced, float minPoopSizeInKg)
	{
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add(consumed_tag);
		Diet.Info[] infos = new Diet.Info[1]
		{
			new Diet.Info(hashSet, producedTag, caloriesPerKg, producedConversionRate, diseaseId, diseasePerKgProduced)
		};
		Diet diet = new Diet(infos);
		CreatureCalorieMonitor.Def def = prefab.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def.diet = diet;
		def.minConsumedCaloriesBeforePooping = minPoopSizeInKg * caloriesPerKg;
		GasAndLiquidConsumerMonitor.Def def2 = prefab.AddOrGetDef<GasAndLiquidConsumerMonitor.Def>();
		def2.diet = diet;
		return prefab;
	}
}
