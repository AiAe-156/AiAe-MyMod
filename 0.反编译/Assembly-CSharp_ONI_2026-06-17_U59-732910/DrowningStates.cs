using STRINGS;

public class DrowningStates : GameStateMachine<DrowningStates, DrowningStates.Instance, IStateMachineTarget, DrowningStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public int safeCell = Grid.InvalidCell;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.HasTag, GameTags.Creatures.Drowning);
		}
	}

	public class EscapeCellQuery : PathFinderQuery
	{
		private DrowningMonitor monitor;

		public EscapeCellQuery(DrowningMonitor monitor)
		{
			this.monitor = monitor;
		}

		public override bool IsMatch(int cell, int parent_cell, int cost)
		{
			return monitor.IsCellSafe(cell);
		}
	}

	public State drown;

	public State drown_pst;

	public State move_to_safe;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = drown;
		State state = root;
		string text = CREATURES.STATUSITEMS.DROWNING.NAME;
		string tooltip = CREATURES.STATUSITEMS.DROWNING.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main).TagTransition(GameTags.Creatures.Drowning, null, on_remove: true).ToggleCritterEmotion(Db.Get().CritterEmotions.Suffocating);
		drown.PlayAnim("drown_pre").QueueAnim("drown_loop", loop: true).Transition(drown_pst, UpdateSafeCell, UpdateRate.SIM_1000ms);
		drown_pst.PlayAnim("drown_pst").OnAnimQueueComplete(move_to_safe);
		move_to_safe.MoveTo((Instance smi) => smi.safeCell);
	}

	public bool UpdateSafeCell(Instance smi)
	{
		Navigator component = smi.GetComponent<Navigator>();
		EscapeCellQuery escapeCellQuery = new EscapeCellQuery(smi.GetComponent<DrowningMonitor>());
		component.RunQuery(escapeCellQuery);
		smi.safeCell = escapeCellQuery.GetResultCell();
		return smi.safeCell != Grid.InvalidCell;
	}
}
