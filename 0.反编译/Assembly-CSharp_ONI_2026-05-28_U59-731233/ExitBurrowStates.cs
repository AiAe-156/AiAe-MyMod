using STRINGS;

public class ExitBurrowStates : GameStateMachine<ExitBurrowStates, ExitBurrowStates.Instance, IStateMachineTarget, ExitBurrowStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.WantsToExitBurrow);
		}
	}

	private State exiting;

	private State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = exiting;
		State state = root;
		string text = CREATURES.STATUSITEMS.EMERGING.NAME;
		string tooltip = CREATURES.STATUSITEMS.EMERGING.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		exiting.PlayAnim("emerge").Enter(MoveToCellAbove).OnAnimQueueComplete(behaviourcomplete);
		behaviourcomplete.BehaviourComplete(GameTags.Creatures.WantsToExitBurrow);
	}

	private static void MoveToCellAbove(Instance smi)
	{
		smi.transform.SetPosition(Grid.CellToPosCBC(Grid.CellAbove(Grid.PosToCell(smi.transform.GetPosition())), Grid.SceneLayer.Creatures));
	}
}
