using System;
using System.Diagnostics;
using STRINGS;

public class ChoreDriver : StateMachineComponent<ChoreDriver.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, ChoreDriver, object>.GameInstance
	{
		private ChoreConsumer choreConsumer = null;

		[MyCmpGet]
		private Brain brain;

		public string masterProperName { get; private set; } = null;

		public KPrefabID masterPrefabId { get; private set; } = null;

		public Navigator navigator { get; private set; } = null;

		public WorkerBase worker { get; private set; } = null;

		[Conditional("ENABLE_LOGGER")]
		public void Log(string name, string param)
		{
		}

		public StatesInstance(ChoreDriver master)
			: base(master)
		{
			masterProperName = base.master.GetProperName();
			masterPrefabId = base.master.GetComponent<KPrefabID>();
			navigator = base.master.GetComponent<Navigator>();
			worker = base.master.GetComponent<WorkerBase>();
			choreConsumer = GetComponent<ChoreConsumer>();
			ChoreConsumer obj = choreConsumer;
			obj.choreRulesChanged = (System.Action)Delegate.Combine(obj.choreRulesChanged, new System.Action(OnChoreRulesChanged));
		}

		public void BeginChore()
		{
			Chore nextChore = GetNextChore();
			Chore chore = base.smi.sm.currentChore.Set(nextChore, base.smi);
			if (chore != null && chore.IsPreemptable && chore.driver != null)
			{
				chore.Fail("Preemption!");
			}
			base.smi.sm.nextChore.Set(null, base.smi);
			chore.onExit = (Action<Chore>)Delegate.Combine(chore.onExit, new Action<Chore>(OnChoreExit));
			chore.Begin(base.master.context);
			Trigger(-1988963660, (object)chore);
		}

		public void EndChore(string reason)
		{
			if (GetCurrentChore() != null)
			{
				Chore currentChore = GetCurrentChore();
				base.smi.sm.currentChore.Set(null, base.smi);
				currentChore.onExit = (Action<Chore>)Delegate.Remove(currentChore.onExit, new Action<Chore>(OnChoreExit));
				currentChore.Fail(reason);
				Trigger(1745615042, (object)currentChore);
			}
			if (base.smi.choreConsumer.prioritizeBrainIfNoChore)
			{
				Game.BrainScheduler.PrioritizeBrain(brain);
			}
		}

		private void OnChoreExit(Chore chore)
		{
			base.smi.sm.stop.Trigger(base.smi);
		}

		public Chore GetNextChore()
		{
			return base.smi.sm.nextChore.Get(base.smi);
		}

		public Chore GetCurrentChore()
		{
			return base.smi.sm.currentChore.Get(base.smi);
		}

		private void OnChoreRulesChanged()
		{
			Chore currentChore = GetCurrentChore();
			if (currentChore != null && !choreConsumer.IsPermittedOrEnabled(currentChore.choreType, currentChore))
			{
				EndChore("Permissions changed");
			}
		}
	}

	public class States : GameStateMachine<States, StatesInstance, ChoreDriver>
	{
		public ObjectParameter<Chore> currentChore;

		public ObjectParameter<Chore> nextChore;

		public Signal stop;

		public State nochore;

		public State haschore;

		private static bool IsLiveMinion(StatesInstance smi)
		{
			if (!smi.masterPrefabId.HasTag(GameTags.BaseMinion))
			{
				return false;
			}
			if (smi.masterPrefabId.HasTag(GameTags.Dead))
			{
				return false;
			}
			return true;
		}

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = nochore;
			saveHistory = true;
			nochore.Update(delegate(StatesInstance smi, float dt)
			{
				if (IsLiveMinion(smi))
				{
					ReportManager.Instance.ReportValueWithPrefabInstanceContext(ReportManager.ReportType.WorkTime, dt, smi.masterPrefabId, string.Format(UI.ENDOFDAYREPORT.NOTES.TIME_SPENT, DUPLICANTS.CHORES.THINKING.NAME));
				}
			}).ParamTransition(nextChore, haschore, (StatesInstance smi, Chore next_chore) => next_chore != null);
			haschore.Enter("BeginChore", delegate(StatesInstance smi)
			{
				smi.BeginChore();
			}).Update(delegate(StatesInstance smi, float dt)
			{
				if (IsLiveMinion(smi))
				{
					Chore chore = currentChore.Get(smi);
					if (chore != null)
					{
						ReportManager.ReportType reportType = chore.GetReportType();
						string note;
						if (smi.navigator.IsMoving())
						{
							reportType = ReportManager.ReportType.TravelTime;
							note = GameUtil.GetChoreName(chore, null);
						}
						else
						{
							Workable workable = smi.worker.GetWorkable();
							if (workable != null)
							{
								reportType = workable.GetReportType();
							}
							note = string.Format(UI.ENDOFDAYREPORT.NOTES.WORK_TIME, GameUtil.GetChoreName(chore, null));
						}
						ReportManager.Instance.ReportValueWithPrefabInstanceContext(reportType, dt, smi.masterPrefabId, note);
					}
				}
			}).Exit("EndChore", delegate(StatesInstance smi)
			{
				smi.EndChore("ChoreDriver.SignalStop");
			})
				.OnSignal(stop, nochore);
		}
	}

	[MyCmpAdd]
	private User user;

	private Chore.Precondition.Context context;

	public Chore GetCurrentChore()
	{
		return base.smi.GetCurrentChore();
	}

	public bool HasChore()
	{
		return base.smi.GetCurrentChore() != null;
	}

	public void StopChore()
	{
		base.smi.sm.stop.Trigger(base.smi);
	}

	public void SetChore(Chore.Precondition.Context context)
	{
		Chore currentChore = base.smi.GetCurrentChore();
		if (currentChore == context.chore)
		{
			return;
		}
		StopChore();
		if (context.chore.IsValid())
		{
			context.chore.PrepareChore(ref context);
			this.context = context;
			base.smi.sm.nextChore.Set(context.chore, base.smi);
			return;
		}
		string text = "Null";
		string text2 = "Null";
		if (currentChore != null)
		{
			text = currentChore.GetType().Name;
		}
		if (context.chore != null)
		{
			text2 = context.chore.GetType().Name;
		}
		string text3 = "Stopping chore " + text + " to start " + text2 + " but stopping the first chore cancelled the second one.";
		Debug.LogWarning(text3);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}
}
