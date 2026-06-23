internal class RemoteWorkerDockAnimSM : StateMachineComponent<RemoteWorkerDockAnimSM.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, RemoteWorkerDockAnimSM, object>.GameInstance
	{
		public StatesInstance(RemoteWorkerDockAnimSM master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, RemoteWorkerDockAnimSM>
	{
		public class FullOrEmptyState : State
		{
			public State full;

			public State empty;
		}

		public FullOrEmptyState on;

		public FullOrEmptyState off;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = off;
			off.EnterTransition(off.full, HasWorkerStored).EnterTransition(off.empty, GameStateMachine<States, StatesInstance, RemoteWorkerDockAnimSM, object>.Not(HasWorkerStored)).Transition(on, IsOnline);
			off.full.QueueAnim("off_full").Transition(off.empty, GameStateMachine<States, StatesInstance, RemoteWorkerDockAnimSM, object>.Not(HasWorkerStored));
			off.empty.QueueAnim("off_empty").Transition(off.full, HasWorkerStored);
			on.EnterTransition(on.full, HasWorkerStored).EnterTransition(on.empty, GameStateMachine<States, StatesInstance, RemoteWorkerDockAnimSM, object>.Not(HasWorkerStored)).Transition(off, GameStateMachine<States, StatesInstance, RemoteWorkerDockAnimSM, object>.Not(IsOnline));
			on.full.QueueAnim("on_full").Transition(off.empty, GameStateMachine<States, StatesInstance, RemoteWorkerDockAnimSM, object>.Not(HasWorkerStored));
			on.empty.QueueAnim("on_empty").Transition(on.full, HasWorkerStored);
		}

		public static bool IsOnline(StatesInstance smi)
		{
			if (smi.master.operational.IsOperational)
			{
				return smi.master.dock.RemoteWorker != null;
			}
			return false;
		}

		public static bool HasWorkerStored(StatesInstance smi)
		{
			if (smi.master.dock.RemoteWorker != null)
			{
				return smi.master.dock.RemoteWorker.Docked;
			}
			return false;
		}
	}

	[MyCmpAdd]
	private RemoteWorkerDock dock;

	[MyCmpGet]
	private Operational operational;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}
}
