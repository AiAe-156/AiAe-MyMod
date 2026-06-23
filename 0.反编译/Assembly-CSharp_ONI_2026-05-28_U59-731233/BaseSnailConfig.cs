using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class BaseSnailConfig
{
	private const string IDLE_LOOP = "idle_loop";

	private const string IDLE_LOOP_SAD = "idle_loop_sad";

	public static GameObject CreatePrefab(string id, string baseTraitId, string name, string description, string animFile, bool isBaby, string symbolPrefix, float warnLowTemp, float warnHighTemp, float lethalLowTemp, float lethalHighTemp, string moltPrefabId)
	{
		float mASS = SnailTuning.MASS;
		EffectorValues tIER = DECOR.BONUS.TIER0;
		KAnimFile anim = Assets.GetAnim(isBaby ? animFile : "snail_build_kanim");
		float defaultTemperature = (warnLowTemp + warnHighTemp) / 2f;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, description, mASS, anim, "idle_loop", Grid.SceneLayer.Creatures, 1, 1, tIER, default(EffectorValues), SimHashes.Creature, null, defaultTemperature);
		gameObject.AddTag(GameTags.Amphibious);
		ConfigureTraits(baseTraitId, name, !isBaby);
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: false, allow_mark_for_capture: true, use_gun_for_pickup: true);
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, animFile, isBaby ? null : "snail_build_kanim", symbolPrefix, FactionManager.FactionID.Prey, baseTraitId, isBaby ? "WalkerBabyNavGrid" : "WalkerNavGrid1x1NoJump", NavType.Floor, 16, 0.25f, "Meat", 0.5f, drownVulnerable: false, entombVulnerable: false, warnLowTemp, warnHighTemp, lethalLowTemp, lethalHighTemp);
		ChoreTable.Builder chore_table = CreateChoreTable(isBaby, animFile);
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		ThreatMonitor.Def def = gameObject.AddOrGetDef<ThreatMonitor.Def>();
		def.fleethresholdState = Health.HealthState.Dead;
		gameObject.AddWeapon(1f, 1f);
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.SnailSpecies, symbolPrefix);
		if (!isBaby)
		{
			UsesCondo(gameObject);
			DropsMolt(gameObject, moltPrefabId);
			MoistureMonitor.Def def2 = gameObject.AddOrGetDef<MoistureMonitor.Def>();
			def2.lubricant = SimHashes.Mucus;
			def2.onDryLandModifier = SnailTuning.MUCUS_PER_CYCLE_DRY_LAND_BONUS / 600f;
			def2.lubricantTemperatureKelvin = 311.15f;
			gameObject.AddOrGetDef<DesiccationMonitor.Def>();
		}
		gameObject.AddOrGet<Pickupable>().sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["Snail"];
		return gameObject;
	}

	private static void DropsMolt(GameObject prefab, string prefabId)
	{
		MoltDropperMonitor.Def def = prefab.AddOrGetDef<MoltDropperMonitor.Def>();
		def.onGrowDropID = prefabId;
		def.massToDrop = 10f;
		def.isReadyToMolt = IsReadyToMolt;
	}

	public static bool IsReadyToMolt(MoltDropperMonitor.Instance smi)
	{
		if (IsValidTimeToDropMolt(smi) && IsValidDropCell(smi) && !smi.prefabID.HasTag(GameTags.Creatures.Hungry))
		{
			return smi.prefabID.HasTag(GameTags.Creatures.Happy);
		}
		return false;
	}

	public static bool IsValidTimeToDropMolt(MoltDropperMonitor.Instance smi)
	{
		if (smi.spawnedThisCycle)
		{
			return false;
		}
		return smi.timeOfLastDrop <= 0f || GameClock.Instance.GetTime() - smi.timeOfLastDrop > 600f;
	}

	public static void OnSpawn(GameObject inst)
	{
		Navigator component = inst.GetComponent<Navigator>();
		component.transitionDriver.overrideLayers.Add(new SadSnailTransitionLayer(component));
	}

	private static void UsesCondo(GameObject prefab)
	{
		CritterCondoInteractMontior.Def def = prefab.AddOrGetDef<CritterCondoInteractMontior.Def>();
		def.requireCavity = false;
		def.condoPrefabTag = "CritterCondo";
	}

	private static ChoreTable.Builder CreateChoreTable(bool isBaby, string animFile)
	{
		ChoreTable.Builder builder = new ChoreTable.Builder();
		KAnimFile anim = Assets.GetAnim(animFile);
		builder.Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new GrowUpStates.Def(), isBaby)
			.Add(new TrappedStates.Def())
			.Add(new IncubatingStates.Def(), isBaby)
			.Add(new BaggedStates.Def())
			.Add(new FallStates.Def())
			.Add(new StunnedStates.Def())
			.Add(new DebugGoToStates.Def())
			.Add(new FleeStates.Def())
			.Add(new AttackStates.Def())
			.PushInterruptGroup()
			.Add(new MucusSecretionStates.Def())
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def(), !isBaby)
			.Add(new LayEggStates.Def(), !isBaby)
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def
			{
				shouldBeBehindMilkTank = true
			})
			.Add(new PoopStates.Def(anim, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP, canComplain: false))
			.Add(new CallAdultStates.Def(), isBaby)
			.Add(new CritterCondoStates.Def(), !isBaby)
			.PopInterruptGroup()
			.Add(new IdleStates.Def
			{
				customIdleAnim = CustomIdleAnim
			});
		return builder;
	}

	private static void ConfigureTraits(string baseTraitId, string name, bool isAdult)
	{
		Trait trait = Db.Get().CreateTrait(baseTraitId, name, name, null, should_save: false, null, positive_trait: true, is_valid_starter_trait: true);
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.maxAttribute.Id, SnailTuning.STANDARD_STOMACH_SIZE, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Calories.deltaAttribute.Id, (0f - SnailTuning.STANDARD_CALORIES_PER_CYCLE) / 600f, UI.TOOLTIPS.BASE_VALUE));
		trait.Add(new AttributeModifier(Db.Get().Amounts.HitPoints.maxAttribute.Id, 25f, name));
		trait.Add(new AttributeModifier(Db.Get().Amounts.Age.maxAttribute.Id, 25f, name));
		if (isAdult)
		{
			trait.Add(new AttributeModifier(Db.Get().Amounts.Moisture.deltaAttribute.Id, SnailTuning.DEFAULT_DRYING_RATE, name));
			trait.Add(new AttributeModifier(Db.Get().Amounts.Mucus.deltaAttribute.Id, SnailTuning.MUCUS_PER_CYCLE / 600f, STRINGS.CREATURES.MODIFIERS.MUCUS.BASE_RATE));
		}
	}

	private static HashedString CustomIdleAnim(IdleStates.Instance smi, ref HashedString pre_anim)
	{
		DesiccationMonitor.Instance sMI = smi.GetSMI<DesiccationMonitor.Instance>();
		return (sMI == null || !sMI.IsDesiccating()) ? "idle_loop" : "idle_loop_sad";
	}

	public static bool IsValidDropCell(MoltDropperMonitor.Instance smi)
	{
		return Grid.IsValidCell(Grid.PosToCell(smi.transform.GetPosition()));
	}

	public static Diet.Info[] SaltToDirtDiet()
	{
		return new Diet.Info[1]
		{
			new Diet.Info(new HashSet<Tag> { SimHashes.Salt.CreateTag() }, SimHashes.Dirt.CreateTag(), SnailTuning.CALORIES_PER_KG_OF_ORE, TUNING.CREATURES.CONVERSION_EFFICIENCY.NORMAL)
		};
	}

	public static Diet.Info[] SulfurToObsidianDiet()
	{
		return new Diet.Info[1]
		{
			new Diet.Info(new HashSet<Tag> { SimHashes.Sulfur.CreateTag() }, SimHashes.Obsidian.CreateTag(), SnailTuning.CALORIES_PER_KG_OF_ORE, TUNING.CREATURES.CONVERSION_EFFICIENCY.NORMAL)
		};
	}
}
