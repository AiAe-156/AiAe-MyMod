using STRINGS;

public class CreatureSleepStates : GameStateMachine<CreatureSleepStates, CreatureSleepStates.Instance, IStateMachineTarget, CreatureSleepStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.Behaviours.SleepBehaviour);
		}
	}

	public State pre;

	public State loop;

	public State pst;

	public State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = pre;
		State state = root;
		string text = CREATURES.STATUSITEMS.SLEEPING.NAME;
		string tooltip = CREATURES.STATUSITEMS.SLEEPING.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		pre.QueueAnim("sleep_pre").OnAnimQueueComplete(loop);
		loop.QueueAnim("sleep_loop", loop: true).Transition(pst, ShouldWakeUp, UpdateRate.SIM_1000ms);
		pst.QueueAnim("sleep_pst").OnAnimQueueComplete(behaviourcomplete);
		behaviourcomplete.BehaviourComplete(GameTags.Creatures.Behaviours.SleepBehaviour);
	}

	public static bool ShouldWakeUp(Instance smi)
	{
		return !GameClock.Instance.IsNighttime();
	}
}
