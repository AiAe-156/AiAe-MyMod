using TUNING;
using UnityEngine;

public static class BaseSeaFairyConfig
{
	public const string EMOTION_FILE_NAME = "sea_fairy_emotes_kanim";

	public static GameObject BaseSeaFairy(string id, string name, string desc, string anim_file, string traitId, string symbolOverridePrefix = null)
	{
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 5f, decor: DECOR.BONUS.TIER0, anim: Assets.GetAnim("sea_fairy_build_kanim"), initialAnim: "idle_loop", sceneLayer: Grid.SceneLayer.Creatures, width: 1, height: 1);
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, "sea_fairy_build_kanim", null, FactionManager.FactionID.Pest, traitId, "SwimmerNavGrid", NavType.Swim, 32, 2f, "Nori", 10f, drownVulnerable: false, entombVulnerable: true, 283.15f, 318.15f, 233.15f, 353.15f);
		if (symbolOverridePrefix != null)
		{
			gameObject.AddOrGet<SymbolOverrideController>().ApplySymbolOverridesByAffix(Assets.GetAnim(anim_file), symbolOverridePrefix);
		}
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = CREATURES.SORTING.CRITTER_ORDER["SeaFairy"];
		pickupable.sortOrder = sortOrder;
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.SwimmingCreature);
		component.AddTag(GameTags.Creatures.Swimmer);
		CreatureFallMonitor.Def def = gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		def.canSwim = true;
		def.checkHead = false;
		gameObject.AddOrGetDef<FlopMonitor.Def>();
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGetDef<ThreatMonitor.Def>();
		gameObject.AddOrGetDef<OvercrowdingMonitor.Def>().spaceRequiredPerCreature = CREATURES.SPACE_REQUIREMENTS.TIER2;
		gameObject.AddOrGetDef<FishOvercrowdingMonitor.Def>();
		gameObject.AddOrGetDef<LureableMonitor.Def>().lures = new Tag[1] { GameTags.Creatures.FishTrapLure };
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		KAnimFile anim = Assets.GetAnim("sea_fairy_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new TrappedStates.Def())
			.Add(new BaggedStates.Def())
			.Add(new StunnedStates.Def())
			.Add(new DebugGoToStates.Def())
			.Add(new FleeStates.Def())
			.Add(new AttackStates.Def())
			.Add(new FallStates.Def
			{
				getLandAnim = GetLandAnim
			})
			.Add(new FlopStates.Def())
			.PushInterruptGroup()
			.Add(new FixedCaptureStates.Def())
			.Add(new MoveToLureStates.Def())
			.Add(new CritterEmoteStates.Def(anim))
			.PopInterruptGroup()
			.Add(new IdleStates.Def());
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.SeaFairySpecies, symbolOverridePrefix);
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
