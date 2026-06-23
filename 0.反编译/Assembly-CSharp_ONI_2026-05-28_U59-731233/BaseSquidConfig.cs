using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BaseSquidConfig
{
	public const string EMOTION_FILE_NAME = "squid_emotes_kanim";

	public static GameObject CreatePrefab(string id, string base_trait_id, string name, string description, string anim_file, bool is_baby, string symbol_prefix, float warnLowTemp, float warnHighTemp, float lethalLowTemp, float lethalHighTemp)
	{
		int height = (is_baby ? 1 : 2);
		EffectorValues tIER = DECOR.BONUS.TIER0;
		KAnimFile anim = Assets.GetAnim(is_baby ? anim_file : "squid_build_kanim");
		float defaultTemperature = (warnLowTemp + warnHighTemp) / 2f;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, description, 200f, anim, "idle_loop", Grid.SceneLayer.Creatures, 1, height, tIER, default(EffectorValues), SimHashes.Creature, null, defaultTemperature);
		if (!is_baby)
		{
			KBoxCollider2D kBoxCollider2D = gameObject.AddOrGet<KBoxCollider2D>();
			kBoxCollider2D.offset = new Vector2f(0f, kBoxCollider2D.offset.y);
		}
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.SwimmingCreature);
		component.AddTag(GameTags.Creatures.Swimmer);
		component.AddTag(GameTags.Creatures.SquidFriend);
		Trait trait = Db.Get().CreateTrait(base_trait_id, name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, SquidTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - SquidTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 100f, name));
		gameObject.AddWeapon(2f, 3f);
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "squid_build_kanim", null, FactionManager.FactionID.Prey, base_trait_id, is_baby ? "SwimmerNavGrid" : "SwimmerNavGrid1x2", NavType.Swim, 32, 2f, "SquidMeat", 12f, drownVulnerable: false, entombVulnerable: false, warnLowTemp, warnHighTemp, lethalLowTemp, lethalHighTemp);
		ThreatMonitor.Def def = gameObject.AddOrGetDef<ThreatMonitor.Def>();
		def.fleethresholdState = Health.HealthState.Dead;
		def.friendlyCreatureTags = new Tag[1] { GameTags.Creatures.SquidFriend };
		def.maxSearchDistance = 12;
		def.offsets = CrabTuning.DEFEND_OFFSETS;
		MilkProductionMonitor.Def def2 = gameObject.AddOrGetDef<MilkProductionMonitor.Def>();
		def2.element = SimHashes.Ink;
		def2.CaloriesPerCycle = SquidTuning.WELLFED_CALORIES_PER_CYCLE;
		def2.Capacity = SquidTuning.INK_CAPACITY;
		def2.effectId = "SquidWellFed";
		KAnimFile anim2 = Assets.GetAnim("squid_emotes_kanim");
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
			.Add(new DefendStates.Def
			{
				preAnim = "attack_pre",
				attackAnim = "attack",
				pstAnim = "attack_pst",
				specialAttackAction = InkAttack
			})
			.PushInterruptGroup()
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def(), !is_baby)
			.Add(new LayEggStates.Def(), !is_baby)
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def())
			.Add(new PoopStates.Def(anim2, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP, canComplain: false))
			.Add(new MoveToLureStates.Def())
			.Add(new CritterCondoStates.Def
			{
				fgLayer = CritterCondo.CreatureFGLayerType.SquidLayer
			}, !is_baby)
			.Add(new CritterEmoteStates.Def(anim2))
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		CreatureFallMonitor.Def def3 = gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		def3.canSwim = true;
		def3.checkHead = true;
		gameObject.AddOrGetDef<FlopMonitor.Def>();
		gameObject.AddOrGetDef<FishOvercrowdingMonitor.Def>();
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGet<LoopingSounds>();
		LureableMonitor.Def def4 = gameObject.AddOrGetDef<LureableMonitor.Def>();
		def4.lures = new Tag[1] { GameTags.Creatures.FishTrapLure };
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.SquidSpecies, symbol_prefix);
		CritterCondoInteractMontior.Def def5 = gameObject.AddOrGetDef<CritterCondoInteractMontior.Def>();
		def5.requireCavity = false;
		def5.condoPrefabTag = "UnderwaterCritterCondo";
		Tag pOOP_ELEMENT = SquidTuning.POOP_ELEMENT;
		HashSet<Tag> hashSet = new HashSet<Tag>();
		hashSet.Add("TubeWorm");
		List<Diet.Info> list = new List<Diet.Info>
		{
			new Diet.Info(hashSet, pOOP_ELEMENT, SquidTuning.CALORIES_PER_GROWTH_EATEN, SquidTuning.GROWTH_TO_PRODUCT_EFFICIENCY, null, 0f, produce_solid_tile: false, Diet.Info.FoodType.EatPlantDirectly)
		};
		Diet diet = new Diet(list.ToArray());
		CreatureCalorieMonitor.Def def6 = gameObject.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def6.diet = diet;
		def6.minConsumedCaloriesBeforePooping = SquidTuning.STANDARD_CALORIES_PER_CYCLE;
		SolidConsumerMonitor.Def def7 = gameObject.AddOrGetDef<SolidConsumerMonitor.Def>();
		def7.diet = diet;
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["Squid"];
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

	private static void InkAttack(GameObject attacker, GameObject target)
	{
		if (!(target == null))
		{
			int num = Grid.PosToCell(attacker);
			int num2 = Grid.CellBelow(num);
			int num3 = num2;
			if (!Grid.IsValidCell(num2) || Grid.Solid[num2])
			{
				num3 = num;
			}
			if (Grid.IsValidCell(num3) && !Grid.Solid[num3])
			{
				PrimaryElement component = attacker.GetComponent<PrimaryElement>();
				SimMessages.AddRemoveSubstance(num3, SimHashes.Ink, CellEventLogger.Instance.ElementEmitted, 5f, component.Temperature, byte.MaxValue, 0);
			}
		}
	}
}
