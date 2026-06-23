using Klei.AI;
using TUNING;
using UnityEngine;

public static class BaseMosquitoConfig
{
	public const string EMOTION_FILE_NAME = "mosquito_emotes_kanim";

	public static GameObject BaseMosquito(string id, string name, string desc, string anim_file, string traitId, string symbol_override_prefix, bool isBaby, float warningLowTemperature, float warningHighTemperature, float lethalLowTemperature, float lethalHighTemperature, string poke_anim_pre, string poke_anim_loop, string poke_anim_pst, string goingToPokeStatusItemSTRAddress, string pokingStatusItemSTRAddress)
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 5f, decor: DECOR.PENALTY.TIER0, anim: Assets.GetAnim(isBaby ? anim_file : "mosquito_build_kanim"), initialAnim: "idle_loop", sceneLayer: Grid.SceneLayer.Creatures, width: 1, height: 1);
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, isBaby ? null : "mosquito_build_kanim", null, FactionManager.FactionID.Prey, traitId, isBaby ? "SwimmerNavGrid" : "FlyerNavGrid1x1", isBaby ? NavType.Swim : NavType.Hover, 32, 2f, null, 0f, !isBaby, entombVulnerable: true, warningLowTemperature, warningHighTemperature, lethalLowTemperature, lethalHighTemperature);
		if (!string.IsNullOrEmpty(symbol_override_prefix))
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbol_override_prefix);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = CREATURES.SORTING.CRITTER_ORDER["Mosquito"];
		pickupable.sortOrder = sortOrder;
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<Trappable>();
		LureableMonitor.Def def = gameObject.AddOrGetDef<LureableMonitor.Def>();
		def.lures = ((!isBaby) ? new Tag[1] { GameTags.Creatures.FlyersLure } : new Tag[1] { GameTags.Creatures.FishTrapLure });
		gameObject.AddOrGetDef<ThreatMonitor.Def>();
		OvercrowdingMonitor.Def def2 = gameObject.AddOrGetDef<OvercrowdingMonitor.Def>();
		if (!isBaby)
		{
			component.AddTag(GameTags.Creatures.Flyer);
			gameObject.AddOrGetDef<SubmergedMonitor.Def>();
			gameObject.AddOrGet<PokeMonitor>();
			component.prefabInitFn += delegate(GameObject inst)
			{
				inst.GetAttributes().Add(Db.Get().Attributes.MaxUnderwaterTravelCost);
			};
			def2.spaceRequiredPerCreature = CREATURES.SPACE_REQUIREMENTS.TIER1;
		}
		else
		{
			component.AddTag(GameTags.SwimmingCreature);
			component.AddTag(GameTags.Creatures.Swimmer);
			gameObject.AddComponent<Movable>();
			def2.spaceRequiredPerCreature = 0;
		}
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, !isBaby, !isBaby, isBaby);
		KAnimFile anim = Assets.GetAnim("mosquito_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new GrowUpStates.Def(), isBaby)
			.Add(new TrappedStates.Def())
			.Add(new IncubatingStates.Def(), isBaby)
			.Add(new BaggedStates.Def())
			.Add(new FallStates.Def
			{
				getLandAnim = GetLandAnim
			}, isBaby)
			.Add(new StunnedStates.Def())
			.Add(new DebugGoToStates.Def())
			.Add(new FlopStates.Def(), isBaby)
			.Add(new DrowningStates.Def(), !isBaby)
			.PushInterruptGroup()
			.Add(new CreatureSleepStates.Def())
			.Add(new FixedCaptureStates.Def())
			.Add(new LayEggStates.Def(), !isBaby)
			.Add(new AliveEntityPoker.Def
			{
				PokeAnim_Pre = poke_anim_pre,
				PokeAnim_Loop = poke_anim_loop,
				PokeAnim_Pst = poke_anim_pst,
				statusItemSTR_goingToPoke = goingToPokeStatusItemSTRAddress,
				statusItemSTR_poking = pokingStatusItemSTRAddress
			}, !isBaby)
			.Add(new MoveToLureStates.Def())
			.Add(new CritterEmoteStates.Def(anim))
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		CreatureFallMonitor.Def def3 = gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		def3.canSwim = isBaby;
		def3.checkHead = !isBaby;
		gameObject.AddOrGetDef<FixedCapturableMonitor.Def>();
		if (isBaby)
		{
			gameObject.AddOrGetDef<FlopMonitor.Def>();
			gameObject.AddOrGetDef<FishOvercrowdingMonitor.Def>();
			gameObject.AddOrGetDef<AquaticCreatureSuffocationMonitor.Def>();
		}
		gameObject.AddOrGet<LoopingSounds>();
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.MosquitoSpecies, symbol_override_prefix);
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
