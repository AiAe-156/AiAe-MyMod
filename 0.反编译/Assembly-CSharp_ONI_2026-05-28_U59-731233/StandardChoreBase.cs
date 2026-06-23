using System;
using System.Collections.Generic;

public abstract class StandardChoreBase : Chore
{
	private Action<Chore> onBegin;

	private Action<Chore> onEnd;

	public Action<Chore> onCleanup;

	private List<PreconditionInstance> preconditions = new List<PreconditionInstance>();

	private bool arePreconditionsDirty;

	public bool addToDailyReport;

	public ReportManager.ReportType reportType;

	public override int id { get; protected set; }

	public override int priorityMod { get; protected set; }

	public override ChoreType choreType { get; protected set; }

	public override ChoreDriver driver { get; protected set; }

	public override ChoreDriver lastDriver { get; protected set; }

	public override IStateMachineTarget target { get; protected set; }

	public override bool isComplete { get; protected set; }

	public override bool IsPreemptable { get; protected set; }

	public override ChoreConsumer overrideTarget { get; protected set; }

	public override Prioritizable prioritizable { get; protected set; }

	public override ChoreProvider provider { get; set; }

	public override bool runUntilComplete { get; set; }

	public override bool isExpanded { get; protected set; }

	public override bool SatisfiesUrge(Urge urge)
	{
		return urge == choreType.urge;
	}

	public override bool IsValid()
	{
		return provider != null && gameObject.GetMyWorldId() != -1;
	}

	public override bool CanPreempt(Precondition.Context context)
	{
		return IsPreemptable;
	}

	public override void PrepareChore(ref Precondition.Context context)
	{
	}

	public override string GetReportName(string context = null)
	{
		if (context == null || choreType.reportName == null)
		{
			return choreType.Name;
		}
		return string.Format(choreType.reportName, context);
	}

	public override void Cancel(string reason)
	{
		if (RemoveFromProvider())
		{
			if (addToDailyReport)
			{
				ReportManager.Instance.ReportValue(ReportManager.ReportType.ChoreStatus, -1f, choreType.Name, GameUtil.GetChoreName(this, null));
				SaveGame.Instance.ColonyAchievementTracker.LogSuitChore((driver != null) ? driver : lastDriver);
			}
			End(reason);
			Cleanup();
		}
	}

	public override void Cleanup()
	{
		ClearPrioritizable();
	}

	public override ReportManager.ReportType GetReportType()
	{
		return reportType;
	}

	public override void AddPrecondition(Precondition precondition, object data = null)
	{
		arePreconditionsDirty = true;
		preconditions.Add(new PreconditionInstance
		{
			condition = precondition,
			data = data
		});
	}

	public override void CollectChores(ChoreConsumerState consumer_state, List<Precondition.Context> succeeded_contexts, List<Precondition.Context> incomplete_contexts, List<Precondition.Context> failed_contexts, bool is_attempting_override)
	{
		Precondition.Context item = new Precondition.Context(this, consumer_state, is_attempting_override);
		item.RunPreconditions();
		if (!item.IsComplete())
		{
			incomplete_contexts.Add(item);
		}
		else if (item.IsSuccess())
		{
			succeeded_contexts.Add(item);
		}
		else
		{
			failed_contexts.Add(item);
		}
	}

	public override void Fail(string reason)
	{
		if (!(provider == null) && !(driver == null))
		{
			if (!runUntilComplete)
			{
				Cancel(reason);
			}
			else
			{
				End(reason);
			}
		}
	}

	public override void Reserve(ChoreDriver reserver)
	{
		if (driver != null && driver != reserver && reserver != null)
		{
			Debug.LogErrorFormat("Chore.Reserve: driver already set {0} {1} {2}, provider {3}, driver {4} -> {5}", id, GetType(), choreType.Id, provider, driver, reserver);
		}
		driver = reserver;
	}

	public override void Begin(Precondition.Context context)
	{
		if (driver != null && driver != context.consumerState.choreDriver)
		{
			Debug.LogErrorFormat("Chore.Begin driver already set {0} {1} {2}, provider {3}, driver {4} -> {5}", id, GetType(), choreType.Id, provider, driver, context.consumerState.choreDriver);
		}
		if (provider == null)
		{
			Debug.LogErrorFormat("Chore.Begin provider is null {0} {1} {2}, provider {3}, driver {4}", id, GetType(), choreType.Id, provider, driver);
		}
		driver = context.consumerState.choreDriver;
		StateMachine.Instance sMI = GetSMI();
		sMI.OnStop = (Action<string, StateMachine.Status>)Delegate.Combine(sMI.OnStop, new Action<string, StateMachine.Status>(OnStateMachineStop));
		KSelectable component = driver.GetComponent<KSelectable>();
		if (component != null)
		{
			component.SetStatusItem(Db.Get().StatusItemCategories.Main, GetStatusItem(), this);
		}
		sMI.StartSM();
		if (onBegin != null)
		{
			onBegin(this);
		}
	}

	public override bool InProgress()
	{
		return driver != null;
	}

	protected abstract StateMachine.Instance GetSMI();

	public StandardChoreBase(ChoreType chore_type, IStateMachineTarget target, ChoreProvider chore_provider, bool run_until_complete, Action<Chore> on_complete, Action<Chore> on_begin, Action<Chore> on_end, PriorityScreen.PriorityClass priority_class, int priority_value, bool is_preemptable, bool allow_in_context_menu, int priority_mod, bool add_to_daily_report, ReportManager.ReportType report_type)
	{
		this.target = target;
		if (priority_value == int.MaxValue)
		{
			priority_class = PriorityScreen.PriorityClass.topPriority;
			priority_value = 2;
		}
		if (priority_value < 1 || priority_value > 9)
		{
			Debug.LogErrorFormat("Priority Value Out Of Range: {0}", priority_value);
		}
		masterPriority = new PrioritySetting(priority_class, priority_value);
		priorityMod = priority_mod;
		id = Chore.GetNextChoreID();
		if (chore_provider == null)
		{
			chore_provider = GlobalChoreProvider.Instance;
			DebugUtil.Assert(chore_provider != null);
		}
		choreType = chore_type;
		runUntilComplete = run_until_complete;
		onComplete = on_complete;
		onEnd = on_end;
		onBegin = on_begin;
		IsPreemptable = is_preemptable;
		AddPrecondition(ChorePreconditions.instance.IsValid);
		AddPrecondition(ChorePreconditions.instance.IsPermitted);
		AddPrecondition(ChorePreconditions.instance.IsPreemptable);
		AddPrecondition(ChorePreconditions.instance.HasUrge);
		AddPrecondition(ChorePreconditions.instance.IsMoreSatisfyingEarly);
		AddPrecondition(ChorePreconditions.instance.IsMoreSatisfyingLate);
		AddPrecondition(ChorePreconditions.instance.IsOverrideTargetNullOrMe);
		chore_provider.AddChore(this);
	}

	public virtual void SetPriorityMod(int priorityMod)
	{
		this.priorityMod = priorityMod;
	}

	public override List<PreconditionInstance> GetPreconditions()
	{
		if (arePreconditionsDirty)
		{
			lock (preconditions)
			{
				if (arePreconditionsDirty)
				{
					preconditions.Sort((PreconditionInstance x, PreconditionInstance y) => x.condition.sortOrder.CompareTo(y.condition.sortOrder));
					arePreconditionsDirty = false;
				}
			}
		}
		return preconditions;
	}

	protected void SetPrioritizable(Prioritizable prioritizable)
	{
		if (prioritizable != null && prioritizable.IsPrioritizable())
		{
			this.prioritizable = prioritizable;
			masterPriority = prioritizable.GetMasterPriority();
			prioritizable.onPriorityChanged = (Action<PrioritySetting>)Delegate.Combine(prioritizable.onPriorityChanged, new Action<PrioritySetting>(OnMasterPriorityChanged));
		}
	}

	private void ClearPrioritizable()
	{
		if (prioritizable != null)
		{
			Prioritizable obj = prioritizable;
			obj.onPriorityChanged = (Action<PrioritySetting>)Delegate.Remove(obj.onPriorityChanged, new Action<PrioritySetting>(OnMasterPriorityChanged));
		}
	}

	private void OnMasterPriorityChanged(PrioritySetting priority)
	{
		masterPriority = priority;
	}

	public void SetOverrideTarget(ChoreConsumer chore_consumer)
	{
		overrideTarget = chore_consumer;
		Fail("New override target");
	}

	protected virtual void End(string reason)
	{
		if (driver != null)
		{
			KSelectable component = driver.GetComponent<KSelectable>();
			if (component != null)
			{
				component.SetStatusItem(Db.Get().StatusItemCategories.Main, null);
			}
		}
		StateMachine.Instance sMI = GetSMI();
		sMI.OnStop = (Action<string, StateMachine.Status>)Delegate.Remove(sMI.OnStop, new Action<string, StateMachine.Status>(OnStateMachineStop));
		sMI.StopSM(reason);
		if (!(driver == null))
		{
			lastDriver = driver;
			driver = null;
			if (onEnd != null)
			{
				onEnd(this);
			}
			if (onExit != null)
			{
				onExit(this);
			}
			driver = null;
		}
	}

	protected void Succeed(string reason)
	{
		if (RemoveFromProvider())
		{
			isComplete = true;
			if (onComplete != null)
			{
				onComplete(this);
			}
			if (addToDailyReport)
			{
				ReportManager.Instance.ReportValue(ReportManager.ReportType.ChoreStatus, -1f, choreType.Name, GameUtil.GetChoreName(this, null));
				SaveGame.Instance.ColonyAchievementTracker.LogSuitChore((driver != null) ? driver : lastDriver);
			}
			End(reason);
			Cleanup();
		}
	}

	protected virtual StatusItem GetStatusItem()
	{
		return choreType.statusItem;
	}

	protected virtual void OnStateMachineStop(string reason, StateMachine.Status status)
	{
		if (status == StateMachine.Status.Success)
		{
			Succeed(reason);
		}
		else
		{
			Fail(reason);
		}
	}

	private bool RemoveFromProvider()
	{
		if (provider != null)
		{
			provider.RemoveChore(this);
			return true;
		}
		return false;
	}
}
