using STRINGS;

public class DropElementStates : GameStateMachine<DropElementStates, DropElementStates.Instance, IStateMachineTarget, DropElementStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.WantsToDropElements);
		}
	}

	public State dropping;

	public State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = dropping;
		State state = root;
		string text = CREATURES.STATUSITEMS.EXPELLING_GAS.NAME;
		string tooltip = CREATURES.STATUSITEMS.EXPELLING_GAS.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		dropping.PlayAnim("dirty").OnAnimQueueComplete(behaviourcomplete);
		behaviourcomplete.Enter("DropElement", delegate(Instance smi)
		{
			smi.GetSMI<ElementDropperMonitor.Instance>().DropPeriodicElement();
		}).QueueAnim("idle_loop", loop: true).BehaviourComplete(GameTags.Creatures.WantsToDropElements);
	}
}
