using TUNING;

public class PeeChoreMonitor : GameStateMachine<PeeChoreMonitor, PeeChoreMonitor.Instance>
{
	public new class Instance : GameInstance
	{
		public Instance(IStateMachineTarget master)
			: base(master)
		{
		}

		public bool IsCritical()
		{
			return IsInsideState(base.sm.critical);
		}
	}

	public State building;

	public State critical;

	public State paused;

	public State pee;

	private FloatParameter pee_fuse = new FloatParameter(DUPLICANTSTATS.STANDARD.Secretions.PEE_FUSE_TIME);

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = building;
		base.serializable = SerializeType.Both_DEPRECATED;
		building.Update(delegate(Instance smi, float dt)
		{
			pee_fuse.Delta(0f - dt, smi);
		}).Transition(paused, (Instance smi) => IsSleeping(smi)).Transition(critical, (Instance smi) => pee_fuse.Get(smi) <= 60f);
		critical.Update(delegate(Instance smi, float dt)
		{
			pee_fuse.Delta(0f - dt, smi);
		}).Transition(paused, (Instance smi) => IsSleeping(smi)).Transition(pee, (Instance smi) => pee_fuse.Get(smi) <= 0f)
			.Toggle("Components", delegate(Instance smi)
			{
				Components.CriticalBladders.Add(smi);
			}, delegate(Instance smi)
			{
				Components.CriticalBladders.Remove(smi);
			});
		paused.Transition(building, (Instance smi) => !IsSleeping(smi));
		pee.ToggleChore(CreatePeeChore, building);
	}

	private bool IsSleeping(Instance smi)
	{
		smi.master.gameObject.GetSMI<StaminaMonitor.Instance>()?.IsSleeping();
		return false;
	}

	private Chore CreatePeeChore(Instance smi)
	{
		return new PeeChore(smi.master);
	}
}
