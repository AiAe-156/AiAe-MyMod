using TUNING;

public class RoboDancer : GameStateMachine<RoboDancer, RoboDancer.Instance>
{
	public class OverjoyedStates : State
	{
		public State idle;

		public State dancing;

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

	public FloatParameter timeSpentDancing;

	public BoolParameter hasAudience;

	public State neutral;

	public OverjoyedStates overjoyed;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = neutral;
		root.TagTransition(GameTags.Dead, null);
		neutral.TagTransition(GameTags.Overjoyed, overjoyed);
		overjoyed.TagTransition(GameTags.Overjoyed, neutral, on_remove: true).DefaultState(overjoyed.idle).ParamTransition(timeSpentDancing, overjoyed.exitEarly, (Instance smi, float p) => p >= TRAITS.JOY_REACTIONS.ROBO_DANCER.DANCE_DURATION && !hasAudience.Get(smi))
			.Exit(delegate(Instance smi)
			{
				timeSpentDancing.Set(0f, smi);
			});
		overjoyed.idle.Enter(delegate(Instance smi)
		{
			if (smi.IsRecTime())
			{
				smi.GoTo(overjoyed.dancing);
			}
		}).ToggleStatusItem(Db.Get().DuplicantStatusItems.RoboDancerPlanning).EventTransition(GameHashes.ScheduleBlocksTick, overjoyed.dancing, (Instance smi) => smi.IsRecTime());
		overjoyed.dancing.ToggleStatusItem(Db.Get().DuplicantStatusItems.RoboDancerDancing).EventTransition(GameHashes.ScheduleBlocksTick, overjoyed.idle, (Instance smi) => !smi.IsRecTime()).ToggleChore((Instance smi) => new RoboDancerChore(smi.master), overjoyed.idle);
		overjoyed.exitEarly.Enter(delegate(Instance smi)
		{
			smi.ExitJoyReactionEarly();
		});
	}
}
