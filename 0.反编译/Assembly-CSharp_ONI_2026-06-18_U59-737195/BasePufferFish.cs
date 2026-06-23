using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class BasePufferFish
{
	private const string EMOTION_FILE_NAME = "blowfish_emotes_kanim";

	public static GameObject CreatePrefab(string id, string base_trait_id, string name, string description, string anim_file, bool is_baby, string symbol_prefix, float warnLowTemp, float warnHighTemp, float lethalLowTemp, float lethalHighTemp)
	{
		float mASS = PufferFishTuning.MASS;
		EffectorValues dECOR = PufferFishTuning.DECOR;
		KAnimFile anim = Assets.GetAnim(anim_file);
		float defaultTemperature = (warnLowTemp + warnHighTemp) / 2f;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, description, mASS, anim, "idle_loop", Grid.SceneLayer.Creatures, 1, 1, dECOR, default(EffectorValues), SimHashes.Creature, null, defaultTemperature);
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.SwimmingCreature);
		component.AddTag(GameTags.Creatures.Swimmer);
		Trait trait = Db.Get().CreateTrait(base_trait_id, name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, 2000000f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, -666.6667f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, PufferFishTuning.HITPOINTS, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, PufferFishTuning.LIFESPAN, name));
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: false, allow_mark_for_capture: true, use_gun_for_pickup: true);
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "blowfish_build_kanim", symbol_prefix, FactionManager.FactionID.Prey, base_trait_id, "SwimmerNavGrid", NavType.Swim, 32, 2f, "FishMeat", 1f, drownVulnerable: false, entombVulnerable: false, warnLowTemp, warnHighTemp, lethalLowTemp, lethalHighTemp);
		KAnimFile anim2 = Assets.GetAnim("blowfish_emotes_kanim");
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
			.Add(new VentBubbleStates.Def
			{
				element = SimHashes.Oxygen,
				dupebreathingAnimFiles = new KAnimFile[1] { Assets.GetAnim("anim_interact_blowfish_kanim") },
				dupebreathingAnims = new HashedString[2] { "blowfish_breath_pre", "blowfish_breathe_loop" },
				dupebreathingPst = new HashedString[1] { "blowfish_breath_pst" }
			}, !is_baby)
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
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.PufferFishSpecies, symbol_prefix);
		CritterCondoInteractMontior.Def def = gameObject.AddOrGetDef<CritterCondoInteractMontior.Def>();
		def.requireCavity = false;
		def.condoPrefabTag = "UnderwaterCritterCondo";
		new HashSet<Tag>();
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add("Lettuce");
		HashSet<Tag> hashSet2 = new HashSet<Tag>();
		hashSet2.Add(SeaLettuceConfig.ID);
		string[] eat_anims = new string[3] { "eat_pre", "eat_loop", "idle_loop" };
		Diet diet = new Diet(new List<Diet.Info>
		{
			new Diet.Info(hashSet, PufferFishTuning.POOP_ELEMENT, 400000f, 15f, null, 0f, produce_solid_tile: false, Diet.Info.FoodType.EatSolid, emmit_disease_on_cell: false, eat_anims),
			new Diet.Info(hashSet2, PufferFishTuning.POOP_ELEMENT, 400000f, 15f, null, 0f, produce_solid_tile: false, Diet.Info.FoodType.EatPlantDirectly, emmit_disease_on_cell: false, eat_anims)
		}.ToArray());
		CreatureCalorieMonitor.Def def2 = gameObject.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def2.diet = diet;
		def2.minConsumedCaloriesBeforePooping = 200000f;
		def2.minimumTimeBeforePooping = 0f;
		gameObject.AddOrGetDef<SolidConsumerMonitor.Def>().diet = diet;
		Storage storage = gameObject.AddComponent<Storage>();
		storage.capacityKg = PufferFishTuning.OXYGEN_STORAGE_CAPACITY;
		storage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
		gameObject.AddOrGet<UnderwaterBreathingLocation>().allowLandUse = false;
		gameObject.AddOrGet<UnderwaterBreathingLocationWorkable>();
		gameObject.AddOrGetDef<LureableMonitor.Def>().lures = new Tag[1] { GameTags.Creatures.FishTrapLure };
		if (!string.IsNullOrEmpty(symbol_prefix))
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbol_prefix);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["PufferFish"];
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
