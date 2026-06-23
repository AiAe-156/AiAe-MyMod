using Klei.AI;

public class BionicWaterDamageMonitor : GameStateMachine<BionicWaterDamageMonitor, BionicWaterDamageMonitor.Instance, IStateMachineTarget, BionicWaterDamageMonitor.Def>
{
	public class Def : BaseDef
	{
		public static float ZapInterval = 10f;

		public bool IsElementIntolerable(Element element)
		{
			return element?.HasTag(GameTags.AnyWater) ?? false;
		}
	}

	public new class Instance : GameInstance
	{
		public Effects effects;

		[MyCmpGet]
		public KPrefabID kpid;

		public bool IsAffectedByWaterDamage => effects.HasEffect("BionicWaterStress");

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			effects = GetComponent<Effects>();
		}

		public Reactable GetZapReactable()
		{
			SelfEmoteReactable selfEmoteReactable = new SelfEmoteReactable(base.master.gameObject, Db.Get().Emotes.Minion.WaterDamage.Id, Db.Get().ChoreTypes.WaterDamageZap, 0f, Def.ZapInterval);
			Emote waterDamage = Db.Get().Emotes.Minion.WaterDamage;
			selfEmoteReactable.SetEmote(waterDamage);
			selfEmoteReactable.preventChoreInterruption = true;
			return selfEmoteReactable;
		}
	}

	public const string EFFECT_NAME = "BionicWaterStress";

	public State safe;

	public State suffering;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = safe;
		safe.Transition(suffering, IsSuffering);
		suffering.Transition(safe, GameStateMachine<BionicWaterDamageMonitor, Instance, IStateMachineTarget, Def>.Not(IsSuffering)).ToggleEffect("BionicWaterStress").ToggleReactable(ZapReactable);
	}

	private static Reactable ZapReactable(Instance smi)
	{
		return smi.GetZapReactable();
	}

	private static bool IsSuffering(Instance smi)
	{
		return IsFloorWetWithIntolerantSubstance(smi);
	}

	private static bool IsFloorWetWithIntolerantSubstance(Instance smi)
	{
		if (smi.master.gameObject.HasTag(GameTags.InTransitTube))
		{
			return false;
		}
		int num = Grid.PosToCell(smi);
		if (Grid.IsValidCell(num) && Grid.Element[num].IsLiquid && !smi.kpid.HasTag(GameTags.HasAirtightSuit) && smi.def.IsElementIntolerable(Grid.Element[num]) && (!smi.kpid.HasTag(GameTags.FeetProtection) || Grid.IsSubstantialLiquid(num, 0.1f)))
		{
			if (smi.kpid.HasTag(GameTags.FeetAndWaistProtection))
			{
				return Grid.IsSubstantialLiquid(num);
			}
			return true;
		}
		return false;
	}
}
