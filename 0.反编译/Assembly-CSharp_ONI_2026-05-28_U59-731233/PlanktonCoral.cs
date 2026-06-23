using Klei.AI;

public class PlanktonCoral : GameStateMachine<PlanktonCoral, PlanktonCoral.Instance, IStateMachineTarget, PlanktonCoral.Def>
{
	public class Def : BaseDef
	{
	}

	public class HealthyStates : State
	{
		public State breathing;

		public State clogged;
	}

	public new class Instance : GameInstance
	{
		private WiltCondition wiltCondition;

		private Growing growing;

		public AttributeModifier GrowModifier;

		public bool IsFullyGrown => growing != null && growing.IsGrown();

		public bool IsWilted => wiltCondition != null && wiltCondition.IsWilting();

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			wiltCondition = GetComponent<WiltCondition>();
			growing = GetComponent<Growing>();
		}
	}

	public const string INHALE_ANIM_NAME = "inhale";

	public const string EXHALE_ANIM_NAME = "exhale";

	public State wilted;

	public HealthyStates healthy;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = healthy;
		wilted.EventTransition(GameHashes.WiltRecover, healthy, GameStateMachine<PlanktonCoral, Instance, IStateMachineTarget, Def>.Not(IsWilted));
		healthy.DefaultState(healthy.breathing);
		healthy.breathing.EventTransition(GameHashes.Grow, healthy.clogged, IsFullyGrown).EventTransition(GameHashes.Wilt, wilted, IsWilted);
		healthy.clogged.EventTransition(GameHashes.Harvest, healthy.breathing);
	}

	public static bool IsWilted(Instance smi)
	{
		return smi.IsWilted;
	}

	public static bool IsFullyGrown(Instance smi)
	{
		return smi.IsFullyGrown;
	}
}
