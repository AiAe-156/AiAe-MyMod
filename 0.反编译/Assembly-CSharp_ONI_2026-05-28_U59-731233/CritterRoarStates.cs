public class CritterRoarStates : GameStateMachine<CritterRoarStates, CritterRoarStates.Instance, IStateMachineTarget, CritterRoarStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, TAG);
		}
	}

	private readonly State roar;

	private readonly State behaviourComplete;

	private const float FALLBACK_TIMEOUT = 10f;

	private static HashedString ANIM = "roar";

	private static readonly HashedString[] ANIM_SEQUENCE = new HashedString[1] { ANIM };

	private static Tag TAG = CritterRoarMonitor.TAG;

	public override void InitializeStates(out BaseState defaultState)
	{
		defaultState = roar;
		roar.PlayAnims((Instance smi) => ANIM_SEQUENCE).ScheduleGoTo(10f, behaviourComplete).OnAnimQueueComplete(behaviourComplete);
		behaviourComplete.BehaviourComplete(TAG);
	}
}
