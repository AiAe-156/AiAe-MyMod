using STRINGS;

public class CallAdultStates : GameStateMachine<CallAdultStates, CallAdultStates.Instance, IStateMachineTarget, CallAdultStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.Behaviours.CallAdultBehaviour);
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
		pre.QueueAnim("call_pre").OnAnimQueueComplete(loop);
		loop.QueueAnim("call_loop").OnAnimQueueComplete(pst);
		pst.QueueAnim("call_pst").OnAnimQueueComplete(behaviourcomplete);
		behaviourcomplete.BehaviourComplete(GameTags.Creatures.Behaviours.CallAdultBehaviour);
	}
}
