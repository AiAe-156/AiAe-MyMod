using Klei.AI;
using TUNING;
using UnityEngine;

public static class BaseButterflyConfig
{
	public const string EMOTION_FILE_NAME = "pollinator_emotes_kanim";

	public static GameObject BaseButterfly(string id, string name, string desc, string anim_file, string traitId, string symbolOverridePrefix = null)
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 5f, decor: DECOR.BONUS.TIER0, anim: Assets.GetAnim(anim_file), initialAnim: "idle_loop", sceneLayer: Grid.SceneLayer.Creatures, width: 1, height: 1);
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, "pollinator_build_kanim", null, FactionManager.FactionID.Pest, traitId, "FlyerNavGrid1x1", NavType.Hover, 32, 2f, "ButterflyPlantSeed", 1f, drownVulnerable: true, entombVulnerable: true, 283.15f, 318.15f, 233.15f, 353.15f);
		if (symbolOverridePrefix != null)
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbolOverridePrefix);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = CREATURES.SORTING.CRITTER_ORDER["Butterfly"];
		pickupable.sortOrder = sortOrder;
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.Creatures.Flyer);
		component.prefabInitFn += delegate(GameObject inst)
		{
			inst.GetAttributes().Add(Db.Get().Attributes.MaxUnderwaterTravelCost);
		};
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGetDef<ThreatMonitor.Def>();
		gameObject.AddOrGetDef<SubmergedMonitor.Def>();
		gameObject.AddOrGetDef<PollinateMonitor.Def>().radius = 10;
		gameObject.AddOrGetDef<OvercrowdingMonitor.Def>().spaceRequiredPerCreature = CREATURES.SPACE_REQUIREMENTS.TIER2;
		gameObject.AddOrGetDef<LureableMonitor.Def>().lures = new Tag[2]
		{
			GameTags.Algae,
			GameTags.Creatures.FlyersLure
		};
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		KAnimFile anim = Assets.GetAnim("pollinator_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new TrappedStates.Def())
			.Add(new BaggedStates.Def())
			.Add(new StunnedStates.Def())
			.Add(new DrowningStates.Def())
			.Add(new DebugGoToStates.Def())
			.Add(new FleeStates.Def())
			.Add(new AttackStates.Def())
			.PushInterruptGroup()
			.Add(new FixedCaptureStates.Def())
			.Add(new ApproachBehaviourStates.Def(PollinateMonitor.ID, GameTags.Creatures.WantsToPollinate)
			{
				preAnim = "pollinate_pre",
				loopAnim = "pollinate_loop",
				pstAnim = "pollinate_pst"
			})
			.Add(new CritterEmoteStates.Def(anim))
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.ButterflySpecies, symbolOverridePrefix);
		return gameObject;
	}
}
