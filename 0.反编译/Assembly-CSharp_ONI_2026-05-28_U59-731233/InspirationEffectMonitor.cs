using UnityEngine;

public class InspirationEffectMonitor : GameStateMachine<InspirationEffectMonitor, InspirationEffectMonitor.Instance, IStateMachineTarget, InspirationEffectMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		private SpeechMonitor.Instance speechMonitorInstance;

		private ThoughtGraph.Instance thoughtGraphInstance;

		public KPrefabID Kpid { get; private set; }

		public KBatchedAnimController AnimController { get; private set; }

		public SpeechMonitor.Instance SpeechMonitorInstance
		{
			get
			{
				if (speechMonitorInstance == null)
				{
					speechMonitorInstance = base.master.gameObject.GetSMI<SpeechMonitor.Instance>();
				}
				return speechMonitorInstance;
			}
		}

		public ThoughtGraph.Instance ThoughtGraphInstance
		{
			get
			{
				if (thoughtGraphInstance == null)
				{
					thoughtGraphInstance = base.master.gameObject.GetSMI<ThoughtGraph.Instance>();
				}
				return thoughtGraphInstance;
			}
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			Kpid = master.GetComponent<KPrefabID>();
			AnimController = master.GetComponent<KBatchedAnimController>();
		}
	}

	public BoolParameter shouldCatchyTune;

	public FloatParameter inspirationTimeRemaining;

	public State idle;

	public State catchyTune;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = idle;
		idle.EventHandler(GameHashes.CatchyTune, OnCatchyTune).ParamTransition(shouldCatchyTune, catchyTune, (Instance smi, bool shouldCatchyTune) => shouldCatchyTune);
		catchyTune.Exit(delegate(Instance smi)
		{
			shouldCatchyTune.Set(value: false, smi);
		}).ToggleEffect("HeardJoySinger").ToggleThought(Db.Get().Thoughts.CatchyTune)
			.EventHandler(GameHashes.StartWork, TryThinkCatchyTune)
			.ToggleStatusItem(Db.Get().DuplicantStatusItems.JoyResponse_HeardJoySinger)
			.Enter(delegate(Instance smi)
			{
				SingCatchyTune(smi);
			})
			.Update(delegate(Instance smi, float dt)
			{
				TryThinkCatchyTune(smi, null);
				inspirationTimeRemaining.Delta(0f - dt, smi);
			}, UpdateRate.SIM_4000ms)
			.ParamTransition(inspirationTimeRemaining, idle, (Instance smi, float p) => p <= 0f);
	}

	private void OnCatchyTune(Instance smi, object data)
	{
		inspirationTimeRemaining.Set(600f, smi);
		shouldCatchyTune.Set(value: true, smi);
	}

	private void TryThinkCatchyTune(Instance smi, object data)
	{
		if (Random.Range(1, 101) > 66)
		{
			SingCatchyTune(smi);
		}
	}

	private void SingCatchyTune(Instance smi)
	{
		smi.ThoughtGraphInstance.AddThought(Db.Get().Thoughts.CatchyTune);
		if (!smi.SpeechMonitorInstance.IsPlayingSpeech() && SpeechMonitor.IsAllowedToPlaySpeech(smi.Kpid, smi.AnimController))
		{
			Db.Get().Thoughts.CatchyTune.PlayAsSpeech(smi.SpeechMonitorInstance);
		}
	}
}
