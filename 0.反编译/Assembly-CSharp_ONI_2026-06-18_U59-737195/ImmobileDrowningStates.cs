using STRINGS;

public class ImmobileDrowningStates : GameStateMachine<ImmobileDrowningStates, ImmobileDrowningStates.Instance, IStateMachineTarget, ImmobileDrowningStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.HasTag, GameTags.Creatures.Drowning);
		}
	}

	public State drown;

	public State drown_pst;

	public State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = drown;
		State state = root;
		string text = CREATURES.STATUSITEMS.DROWNING.NAME;
		string tooltip = CREATURES.STATUSITEMS.DROWNING.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main).TagTransition(GameTags.Creatures.Drowning, drown_pst, on_remove: true);
		drown.PlayAnim("drown_pre").QueueAnim("drown_loop", loop: true);
		drown_pst.PlayAnim("drown_pst").OnAnimQueueComplete(behaviourcomplete);
		behaviourcomplete.BehaviourComplete(GameTags.Creatures.Drowning);
	}
}
