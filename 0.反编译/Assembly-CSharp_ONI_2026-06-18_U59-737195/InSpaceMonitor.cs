using Klei.AI;

public class InSpaceMonitor : GameStateMachine<InSpaceMonitor, InSpaceMonitor.Instance>
{
	public new class Instance : GameInstance
	{
		public Instance(IStateMachineTarget master)
			: base(master)
		{
		}

		public bool IsInSpace()
		{
			WorldContainer myWorld = this.GetMyWorld();
			if (!myWorld)
			{
				return false;
			}
			int parentWorldId = myWorld.ParentWorldId;
			int id = myWorld.id;
			if ((bool)myWorld.GetComponent<Clustercraft>() && parentWorldId == id)
			{
				return true;
			}
			return false;
		}
	}

	private const string SPACE_EFFECT_NAME = "SpaceBuzz";

	public State idle;

	public State inSpace;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = idle;
		root.Enter(delegate(Instance smi)
		{
			if (smi.IsInSpace())
			{
				smi.GoTo(inSpace);
			}
		});
		idle.Transition(inSpace, (Instance smi) => smi.IsInSpace(), UpdateRate.SIM_1000ms).Enter(delegate(Instance smi)
		{
			Effects component = smi.master.gameObject.GetComponent<Effects>();
			if (component != null && component.HasEffect("SpaceBuzz"))
			{
				component.Remove("SpaceBuzz");
			}
		});
		inSpace.Transition(idle, (Instance smi) => !smi.IsInSpace(), UpdateRate.SIM_1000ms).ToggleEffect("SpaceBuzz");
	}
}
