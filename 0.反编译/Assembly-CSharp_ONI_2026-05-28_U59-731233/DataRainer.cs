using TUNING;

public class DataRainer : GameStateMachine<DataRainer, DataRainer.Instance>
{
	public class OverjoyedStates : State
	{
		public State idle;

		public State raining;

		public State exitEarly;
	}

	public new class Instance : GameInstance
	{
		public Instance(IStateMachineTarget master)
			: base(master)
		{
		}

		public bool IsRecTime()
		{
			return base.master.GetComponent<Schedulable>().IsAllowed(Db.Get().ScheduleBlockTypes.Recreation);
		}

		public void ExitJoyReactionEarly()
		{
			JoyBehaviourMonitor.Instance sMI = base.master.gameObject.GetSMI<JoyBehaviourMonitor.Instance>();
			sMI.sm.exitEarly.Trigger(sMI);
		}
	}

	public IntParameter databanksCreated;

	public static float databankSpawnInterval = 1.8f;

	public State neutral;

	public OverjoyedStates overjoyed;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = neutral;
		root.TagTransition(GameTags.Dead, null);
		neutral.TagTransition(GameTags.Overjoyed, overjoyed);
		overjoyed.TagTransition(GameTags.Overjoyed, neutral, on_remove: true).DefaultState(overjoyed.idle).ParamTransition(databanksCreated, overjoyed.exitEarly, (Instance smi, int p) => p >= TRAITS.JOY_REACTIONS.DATA_RAINER.NUM_MICROCHIPS)
			.Exit(delegate(Instance smi)
			{
				databanksCreated.Set(0, smi);
			});
		overjoyed.idle.Enter(delegate(Instance smi)
		{
			if (smi.IsRecTime())
			{
				smi.GoTo(overjoyed.raining);
			}
		}).ToggleStatusItem(Db.Get().DuplicantStatusItems.DataRainerPlanning).EventTransition(GameHashes.ScheduleBlocksTick, overjoyed.raining, (Instance smi) => smi.IsRecTime());
		overjoyed.raining.ToggleStatusItem(Db.Get().DuplicantStatusItems.DataRainerRaining).EventTransition(GameHashes.ScheduleBlocksTick, overjoyed.idle, (Instance smi) => !smi.IsRecTime()).ToggleChore((Instance smi) => new DataRainerChore(smi.master), overjoyed.idle);
		overjoyed.exitEarly.Enter(delegate(Instance smi)
		{
			smi.ExitJoyReactionEarly();
		});
	}
}
