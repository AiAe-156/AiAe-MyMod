public class RanchableMonitor : GameStateMachine<RanchableMonitor, RanchableMonitor.Instance, IStateMachineTarget, RanchableMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public RanchStation.Instance TargetRanchStation;

		private Navigator navComponent = null;

		private RanchedStates.Instance states = null;

		public ChoreConsumer ChoreConsumer { get; private set; }

		public Navigator NavComponent => navComponent;

		public RanchedStates.Instance States
		{
			get
			{
				if (states == null)
				{
					states = controller.GetSMI<RanchedStates.Instance>();
				}
				return states;
			}
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			ChoreConsumer = GetComponent<ChoreConsumer>();
			navComponent = GetComponent<Navigator>();
		}

		public bool ShouldGoGetRanched()
		{
			return TargetRanchStation != null && TargetRanchStation.IsRunning() && TargetRanchStation.IsRancherReady;
		}
	}

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = root;
		root.ToggleBehaviour(GameTags.Creatures.WantsToGetRanched, (Instance smi) => smi.ShouldGoGetRanched());
	}
}
