using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BaseRaptorConfig
{
	public const string EMOTION_FILE_NAME = "raptor_emotes_kanim";

	public static GameObject BaseRaptor(string id, string name, string desc, string anim_file, string traitId, bool is_baby, string symbolOverridePrefix = null)
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 400f, decor: DECOR.BONUS.TIER0, anim: Assets.GetAnim(is_baby ? anim_file : "raptor_build_kanim"), initialAnim: "idle_loop", sceneLayer: Grid.SceneLayer.Creatures, width: 2, height: 2);
		KBoxCollider2D kBoxCollider2D = gameObject.AddOrGet<KBoxCollider2D>();
		kBoxCollider2D.offset = new Vector2f(0f, kBoxCollider2D.offset.y);
		KBatchedAnimController component = gameObject.GetComponent<KBatchedAnimController>();
		component.Offset = new Vector3(0f, 0f, 0f);
		string navGridName = "WalkerNavGrid2x2";
		if (is_baby)
		{
			navGridName = "WalkerBabyNavGrid";
		}
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "raptor_build_kanim", null, FactionManager.FactionID.Predator, traitId, navGridName, NavType.Floor, 32, 2f, "DinosaurMeat", 5f, drownVulnerable: true, entombVulnerable: false, 223.15f, 288.15f, 173.15f, 373.15f);
		gameObject.AddOrGet<Navigator>();
		if (symbolOverridePrefix != null)
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbolOverridePrefix);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		gameObject.AddOrGetDef<CritterRoarMonitor.Def>().Initialize(RaptorTuning.ROARS_PER_CYCLE, RaptorTuning.ROAR_COOLDOWN);
		WorldSpawnableMonitor.Def def = gameObject.AddOrGetDef<WorldSpawnableMonitor.Def>();
		ThreatMonitor.Def def2 = gameObject.AddOrGetDef<ThreatMonitor.Def>();
		def2.fleethresholdState = Health.HealthState.Dead;
		gameObject.AddWeapon(1f, 1f);
		KPrefabID component2 = gameObject.GetComponent<KPrefabID>();
		component2.AddTag(GameTags.Creatures.Walker);
		component2.prefabInitFn += delegate(GameObject inst)
		{
			inst.GetAttributes().Add(Db.Get().Attributes.MaxUnderwaterTravelCost);
		};
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		KAnimFile anim = Assets.GetAnim("raptor_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new GrowUpStates.Def(), is_baby)
			.Add(new TrappedStates.Def())
			.Add(new IncubatingStates.Def(), is_baby)
			.Add(new BaggedStates.Def())
			.Add(new FallStates.Def())
			.Add(new StunnedStates.Def())
			.Add(new DrowningStates.Def())
			.Add(new DebugGoToStates.Def())
			.Add(new FleeStates.Def())
			.Add(new AttackStates.Def(), condition: false)
			.PushInterruptGroup()
			.Add(new CreatureSleepStates.Def())
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def(), !is_baby)
			.Add(new LayEggStates.Def(), !is_baby)
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def
			{
				shouldBeBehindMilkTank = false,
				drinkCellOffsetGetFn = (is_baby ? new DrinkMilkStates.Def.DrinkCellOffsetGetFn(DrinkMilkStates.Def.DrinkCellOffsetGet_CritterOneByOne) : new DrinkMilkStates.Def.DrinkCellOffsetGetFn(DrinkMilkStates.Def.DrinkCellOffsetGet_TwoByTwo))
			})
			.Add(new PoopStates.Def(anim, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP, canComplain: false))
			.Add(new CallAdultStates.Def(), is_baby)
			.Add(new CritterCondoStates.Def
			{
				entersBuilding = false
			}, !is_baby)
			.Add(new CritterEmoteStates.Def(anim), !is_baby)
			.Add(new CritterRoarStates.Def())
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.RaptorSpecies, symbolOverridePrefix);
		OccupyArea occupyArea = gameObject.AddOrGet<OccupyArea>();
		occupyArea.updateWithFacing = !is_baby;
		return gameObject;
	}

	public static List<Diet.Info> StandardDiets()
	{
		List<Diet.Info> list = new List<Diet.Info>();
		list.Add(new Diet.Info(new HashSet<Tag> { "DinosaurMeat", "Meat" }, RaptorTuning.POOP_ELEMENT, RaptorTuning.CALORIES_PER_UNIT_EATEN, RaptorTuning.BASE_PRODUCTION_RATE));
		HashSet<Tag> hashSet = new HashSet<Tag>
		{
			"Hatch", "HatchBaby", "HatchVeggie", "HatchVeggieBaby", "HatchMetal", "HatchMetalBaby", "HatchHard", "HatchHardBaby", "Squirrel", "SquirrelBaby",
			"SquirrelHug", "SquirrelHugBaby", "Mole", "MoleBaby", "MoleDelicacy", "MoleDelicacyBaby", "Oilfloater", "OilfloaterBaby", "OilfloaterDecor", "OilfloaterDecorBaby",
			"OilfloaterHighTemp", "OilfloaterHighTempBaby", "Drecko", "DreckoBaby", "DreckoPlastic", "DreckoPlasticBaby", "StegoBaby", "AlgaeStegoBaby", "Chameleon", "ChameleonBaby"
		};
		if (DlcManager.IsContentSubscribed("EXPANSION1_ID"))
		{
			hashSet.Add("DivergentWorm");
			hashSet.Add("DivergentWormBaby");
			hashSet.Add("DivergentBeetle");
			hashSet.Add("DivergentBeetleBaby");
			hashSet.Add("Staterpillar");
			hashSet.Add("StaterpillarBaby");
			hashSet.Add("StaterpillarGas");
			hashSet.Add("StaterpillarGasBaby");
			hashSet.Add("StaterpillarLiquid");
			hashSet.Add("StaterpillarLiquidBaby");
		}
		if (DlcManager.IsContentSubscribed("DLC2_ID"))
		{
			hashSet.Add("IceBellyBaby");
			hashSet.Add("GoldBellyBaby");
			hashSet.Add("WoodDeer");
			hashSet.Add("WoodDeerBaby");
			hashSet.Add("GlassDeer");
			hashSet.Add("GlassDeerBaby");
		}
		if (DlcManager.IsContentSubscribed("DLC5_ID"))
		{
			hashSet.Add("Snail");
			hashSet.Add("SnailBaby");
			hashSet.Add("SnailIron");
			hashSet.Add("SnailIronBaby");
		}
		list.Add(new Diet.Info(hashSet, RaptorTuning.POOP_ELEMENT, RaptorTuning.CALORIES_PER_UNIT_EATEN, RaptorTuning.PREY_PRODUCTION_RATE, null, 0f, produce_solid_tile: false, Diet.Info.FoodType.EatButcheredPrey));
		return list;
	}

	public static GameObject SetupDiet(GameObject prefab, List<Diet.Info> diet_infos)
	{
		Diet diet = new Diet(diet_infos.ToArray());
		CreatureCalorieMonitor.Def def = prefab.AddOrGetDef<CreatureCalorieMonitor.Def>();
		def.diet = diet;
		def.minConsumedCaloriesBeforePooping = RaptorTuning.CALORIES_PER_UNIT_EATEN * 0.1f;
		SolidConsumerMonitor.Def def2 = prefab.AddOrGetDef<SolidConsumerMonitor.Def>();
		def2.possibleEatPositionOffsets = new Vector3[2]
		{
			Vector2.left,
			Vector2.right
		};
		def2.navigatorSize = new Vector2(2f, 2f);
		def2.diet = diet;
		return prefab;
	}
}
