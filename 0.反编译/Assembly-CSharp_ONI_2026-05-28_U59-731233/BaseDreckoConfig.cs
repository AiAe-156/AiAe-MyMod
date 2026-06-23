using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public static class BaseDreckoConfig
{
	public const string EMOTION_FILE_NAME = "drecko_emotes_kanim";

	public static GameObject BaseDrecko(string id, string name, string desc, string anim_file, string trait_id, bool is_baby, string symbol_override_prefix, float warnLowTemp, float warnHighTemp, float lethalLowTemp, float lethalHighTemp)
	{
		EffectorValues tIER = DECOR.BONUS.TIER0;
		KAnimFile anim = Assets.GetAnim(is_baby ? anim_file : "drecko_build_kanim");
		float defaultTemperature = (warnLowTemp + warnHighTemp) / 2f;
		GameObject gameObject = EntityTemplates.CreatePlacedEntity(id, name, desc, 200f, anim, "idle_loop", Grid.SceneLayer.Creatures, 1, 1, tIER, default(EffectorValues), SimHashes.Creature, null, defaultTemperature);
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.Creatures.Walker);
		component.prefabInitFn += delegate(GameObject inst)
		{
			inst.GetAttributes().Add(Db.Get().Attributes.MaxUnderwaterTravelCost);
		};
		string navGridName = "DreckoNavGrid";
		if (is_baby)
		{
			navGridName = "DreckoBabyNavGrid";
		}
		EntityTemplates.ExtendEntityToBasicCreature(isWarmBlooded: false, gameObject, anim_file, is_baby ? null : "drecko_build_kanim", symbol_override_prefix, FactionManager.FactionID.Pest, trait_id, navGridName, NavType.Floor, 32, 1f, "Meat", 2f, drownVulnerable: true, entombVulnerable: false, warnLowTemp, warnHighTemp, lethalLowTemp, lethalHighTemp);
		Pickupable pickupable = gameObject.AddOrGet<Pickupable>();
		int sortOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER["Drecko"];
		pickupable.sortOrder = sortOrder;
		gameObject.AddOrGet<Trappable>();
		gameObject.AddOrGetDef<CreatureFallMonitor.Def>();
		gameObject.AddOrGet<LoopingSounds>();
		ThreatMonitor.Def def = gameObject.AddOrGetDef<ThreatMonitor.Def>();
		def.fleethresholdState = Health.HealthState.Dead;
		gameObject.AddWeapon(1f, 1f);
		EntityTemplates.CreateAndRegisterBaggedCreature(gameObject, must_stand_on_top_for_pickup: true, allow_mark_for_capture: true);
		KAnimFile anim2 = Assets.GetAnim("drecko_emotes_kanim");
		ChoreTable.Builder chore_table = new ChoreTable.Builder().Add(new DeathStates.Def()).Add(new AnimInterruptStates.Def()).Add(new GrowUpStates.Def(), is_baby)
			.Add(new TrappedStates.Def())
			.Add(new IncubatingStates.Def(), is_baby)
			.Add(new BaggedStates.Def())
			.Add(new FallStates.Def())
			.Add(new StunnedStates.Def())
			.Add(new DrowningStates.Def())
			.Add(new DebugGoToStates.Def())
			.Add(new FleeStates.Def())
			.Add(new AttackStates.Def(), !is_baby)
			.PushInterruptGroup()
			.Add(new FixedCaptureStates.Def())
			.Add(new RanchedStates.Def(), !is_baby)
			.Add(new LayEggStates.Def(), !is_baby)
			.Add(new EatStates.Def())
			.Add(new DrinkMilkStates.Def
			{
				shouldBeBehindMilkTank = is_baby
			})
			.Add(new PoopStates.Def(anim2, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME, STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP, canComplain: false))
			.Add(new CallAdultStates.Def(), is_baby)
			.Add(new CritterCondoStates.Def(), !is_baby)
			.Add(new CritterEmoteStates.Def(anim2))
			.PopInterruptGroup()
			.Add(new CreatureSleepStates.Def())
			.Add(new IdleStates.Def
			{
				customIdleAnim = CustomIdleAnim
			});
		EntityTemplates.AddCreatureBrain(gameObject, chore_table, GameTags.Creatures.Species.DreckoSpecies, symbol_override_prefix);
		return gameObject;
	}

	private static HashedString CustomIdleAnim(IdleStates.Instance smi, ref HashedString pre_anim)
	{
		CellOffset offset = new CellOffset(0, -1);
		bool facing = smi.GetComponent<Facing>().GetFacing();
		switch (smi.GetComponent<Navigator>().CurrentNavType)
		{
		case NavType.Floor:
			offset = (facing ? new CellOffset(1, -1) : new CellOffset(-1, -1));
			break;
		case NavType.Ceiling:
			offset = (facing ? new CellOffset(1, 1) : new CellOffset(-1, 1));
			break;
		}
		HashedString result = "idle_loop";
		int num = Grid.OffsetCell(Grid.PosToCell(smi), offset);
		if (Grid.IsValidCell(num) && !Grid.Solid[num])
		{
			pre_anim = "idle_loop_hang_pre";
			result = "idle_loop_hang";
		}
		return result;
	}
}
