public class RemoteWorkTerminalSM : StateMachineComponent<RemoteWorkTerminalSM.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, RemoteWorkTerminalSM, object>.GameInstance
	{
		public StatesInstance(RemoteWorkTerminalSM master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, RemoteWorkTerminalSM>
	{
		public class OfflineStates : State
		{
			public State no_dock;
		}

		public State online;

		public OfflineStates offline;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = offline;
			offline.Transition(online, GameStateMachine<States, StatesInstance, RemoteWorkTerminalSM, object>.And(HasAssignedDock, IsOperational)).Transition(offline.no_dock, GameStateMachine<States, StatesInstance, RemoteWorkTerminalSM, object>.Not(HasAssignedDock));
			offline.no_dock.Transition(offline, HasAssignedDock).ToggleStatusItem(Db.Get().BuildingStatusItems.RemoteWorkTerminalNoDock);
			online.ToggleRecurringChore(CreateChore).Transition(offline, GameStateMachine<States, StatesInstance, RemoteWorkTerminalSM, object>.Not(GameStateMachine<States, StatesInstance, RemoteWorkTerminalSM, object>.And(HasAssignedDock, IsOperational)));
		}

		public static bool IsOperational(StatesInstance smi)
		{
			return smi.master.operational.IsOperational;
		}

		public static bool HasAssignedDock(StatesInstance smi)
		{
			return smi.master.terminal.CurrentDock != null;
		}

		public static Chore CreateChore(StatesInstance smi)
		{
			return new RemoteChore(smi.master.terminal);
		}
	}

	[MyCmpGet]
	private RemoteWorkTerminal terminal;

	[MyCmpGet]
	private Operational operational;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}
}
