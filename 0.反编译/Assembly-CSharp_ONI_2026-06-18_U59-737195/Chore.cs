using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public abstract class Chore
{
	public delegate bool PreconditionFn(ref Precondition.Context context, object data);

	[DebuggerDisplay("{condition}")]
	public struct PreconditionInstance
	{
		public Precondition condition;

		public object data;
	}

	[DebuggerDisplay("{id}")]
	public struct Precondition
	{
		[DebuggerDisplay("{chore.GetType()}, {chore.gameObject.name}, {failedPreconditionId}")]
		public struct Context : IComparable<Context>, IEquatable<Context>
		{
			public PrioritySetting masterPriority;

			public int personalPriority;

			public int priority;

			public int priorityMod;

			public int interruptPriority;

			public int cost;

			public int consumerPriority;

			public Chore chore;

			public ChoreConsumerState consumerState;

			public int failedPreconditionId;

			public bool skippedPreconditions;

			public object data;

			public bool isAttemptingOverride;

			public ChoreType choreTypeForPermission;

			public bool skipMoreSatisfyingEarlyPrecondition;

			public Context(Chore chore, ChoreConsumerState consumer_state, bool is_attempting_override, object data = null)
			{
				masterPriority = chore.masterPriority;
				personalPriority = consumer_state.consumer.GetPersonalPriority(chore.choreType);
				priority = 0;
				priorityMod = chore.priorityMod;
				consumerPriority = 0;
				interruptPriority = 0;
				cost = 0;
				this.chore = chore;
				consumerState = consumer_state;
				failedPreconditionId = -1;
				skippedPreconditions = false;
				isAttemptingOverride = is_attempting_override;
				this.data = data;
				choreTypeForPermission = chore.choreType;
				skipMoreSatisfyingEarlyPrecondition = RootMenu.Instance != null && RootMenu.Instance.IsBuildingChorePanelActive();
				SetPriority(chore);
			}

			public void Set(Chore chore, ChoreConsumerState consumer_state, bool is_attempting_override, object data = null)
			{
				masterPriority = chore.masterPriority;
				priority = 0;
				priorityMod = chore.priorityMod;
				consumerPriority = 0;
				interruptPriority = 0;
				cost = 0;
				this.chore = chore;
				consumerState = consumer_state;
				failedPreconditionId = -1;
				skippedPreconditions = false;
				isAttemptingOverride = is_attempting_override;
				this.data = data;
				choreTypeForPermission = chore.choreType;
				SetPriority(chore);
			}

			public void SetPriority(Chore chore)
			{
				priority = (Game.Instance.advancedPersonalPriorities ? chore.choreType.explicitPriority : chore.choreType.priority);
				priorityMod = chore.priorityMod;
				interruptPriority = chore.choreType.interruptPriority;
			}

			public bool IsSuccess()
			{
				if (failedPreconditionId == -1)
				{
					return !skippedPreconditions;
				}
				return false;
			}

			public bool IsComplete()
			{
				return !skippedPreconditions;
			}

			public bool IsPotentialSuccess()
			{
				if (IsSuccess())
				{
					return true;
				}
				if (chore.driver == consumerState.choreDriver)
				{
					return true;
				}
				if (failedPreconditionId != -1)
				{
					if (failedPreconditionId >= 0 && failedPreconditionId < chore.GetPreconditions().Count)
					{
						return chore.GetPreconditions()[failedPreconditionId].condition.id == ChorePreconditions.instance.IsMoreSatisfyingLate.id;
					}
					DebugUtil.DevLogErrorFormat("failedPreconditionId out of range {0}/{1}", failedPreconditionId, chore.GetPreconditions().Count);
				}
				return false;
			}

			private void DoPreconditions(bool mainThreadOnly)
			{
				bool flag = Game.IsOnMainThread();
				List<PreconditionInstance> preconditions = chore.GetPreconditions();
				skippedPreconditions = false;
				for (int i = 0; i < preconditions.Count; i++)
				{
					PreconditionInstance preconditionInstance = preconditions[i];
					if (preconditionInstance.condition.canExecuteOnAnyThread)
					{
						if (mainThreadOnly)
						{
							continue;
						}
					}
					else if (!flag)
					{
						skippedPreconditions = true;
						continue;
					}
					if (!preconditionInstance.condition.fn(ref this, preconditionInstance.data))
					{
						failedPreconditionId = i;
						skippedPreconditions = false;
						break;
					}
				}
			}

			public void RunPreconditions()
			{
				DoPreconditions(mainThreadOnly: false);
			}

			public void FinishPreconditions()
			{
				DoPreconditions(mainThreadOnly: true);
			}

			public int CompareTo(Context obj)
			{
				bool flag = failedPreconditionId != -1;
				bool flag2 = obj.failedPreconditionId != -1;
				if (flag == flag2)
				{
					int num = masterPriority.priority_class - obj.masterPriority.priority_class;
					if (num != 0)
					{
						return num;
					}
					int num2 = personalPriority - obj.personalPriority;
					if (num2 != 0)
					{
						return num2;
					}
					int num3 = masterPriority.priority_value - obj.masterPriority.priority_value;
					if (num3 != 0)
					{
						return num3;
					}
					int num4 = priority - obj.priority;
					if (num4 != 0)
					{
						return num4;
					}
					int num5 = priorityMod - obj.priorityMod;
					if (num5 != 0)
					{
						return num5;
					}
					int num6 = consumerPriority - obj.consumerPriority;
					if (num6 != 0)
					{
						return num6;
					}
					int num7 = obj.cost - cost;
					if (num7 != 0)
					{
						return num7;
					}
					if (chore == null && obj.chore == null)
					{
						return 0;
					}
					if (chore == null)
					{
						return -1;
					}
					if (obj.chore == null)
					{
						return 1;
					}
					return chore.id - obj.chore.id;
				}
				if (!flag)
				{
					return 1;
				}
				return -1;
			}

			public override bool Equals(object obj)
			{
				Context obj2 = (Context)obj;
				return CompareTo(obj2) == 0;
			}

			public bool Equals(Context other)
			{
				return CompareTo(other) == 0;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public static bool operator ==(Context x, Context y)
			{
				return x.CompareTo(y) == 0;
			}

			public static bool operator !=(Context x, Context y)
			{
				return x.CompareTo(y) != 0;
			}

			public static bool ShouldFilter(string filter, string text)
			{
				if (!string.IsNullOrEmpty(filter))
				{
					if (!string.IsNullOrEmpty(text))
					{
						return text.ToLower().IndexOf(filter) < 0;
					}
					return true;
				}
				return false;
			}
		}

		public string id;

		public string description;

		public int sortOrder;

		public PreconditionFn fn;

		public bool canExecuteOnAnyThread;
	}

	public PrioritySetting masterPriority;

	public bool showAvailabilityInHoverText = true;

	public Action<Chore> onExit;

	public Action<Chore> onComplete;

	private static int nextId;

	public const int MAX_PLAYER_BASIC_PRIORITY = 9;

	public const int MIN_PLAYER_BASIC_PRIORITY = 1;

	public const int MAX_PLAYER_HIGH_PRIORITY = 0;

	public const int MIN_PLAYER_HIGH_PRIORITY = 0;

	public const int MAX_PLAYER_EMERGENCY_PRIORITY = 1;

	public const int MIN_PLAYER_EMERGENCY_PRIORITY = 1;

	public const int DEFAULT_BASIC_PRIORITY = 5;

	public const int MAX_BASIC_PRIORITY = 10;

	public const int MIN_BASIC_PRIORITY = 0;

	public static bool ENABLE_PERSONAL_PRIORITIES = true;

	public static PrioritySetting DefaultPrioritySetting = new PrioritySetting(PriorityScreen.PriorityClass.basic, 5);

	public abstract int id { get; protected set; }

	public abstract int priorityMod { get; protected set; }

	public abstract ChoreType choreType { get; protected set; }

	public abstract ChoreDriver driver { get; protected set; }

	public abstract ChoreDriver lastDriver { get; protected set; }

	public abstract bool isNull { get; }

	public abstract GameObject gameObject { get; }

	public abstract IStateMachineTarget target { get; protected set; }

	public abstract bool isComplete { get; protected set; }

	public abstract bool IsPreemptable { get; protected set; }

	public abstract ChoreConsumer overrideTarget { get; protected set; }

	public abstract Prioritizable prioritizable { get; protected set; }

	public abstract ChoreProvider provider { get; set; }

	public abstract bool runUntilComplete { get; set; }

	public abstract bool isExpanded { get; protected set; }

	public abstract bool SatisfiesUrge(Urge urge);

	public abstract bool IsValid();

	public abstract List<PreconditionInstance> GetPreconditions();

	public abstract bool CanPreempt(Precondition.Context context);

	public abstract void PrepareChore(ref Precondition.Context context);

	public abstract void Cancel(string reason);

	public abstract ReportManager.ReportType GetReportType();

	public abstract string GetReportName(string context = null);

	public abstract void AddPrecondition(Precondition precondition, object data = null);

	public abstract void CollectChores(ChoreConsumerState consumer_state, List<Precondition.Context> succeeded_contexts, List<Precondition.Context> incomplete_contexts, List<Precondition.Context> failed_contexts, bool is_attempting_override);

	public void CollectChores(ChoreConsumerState consumer_state, List<Precondition.Context> succeeded_contexts, List<Precondition.Context> failed_contexts, bool is_attempting_override)
	{
		CollectChores(consumer_state, succeeded_contexts, null, failed_contexts, is_attempting_override);
	}

	public abstract void Cleanup();

	public abstract void Fail(string reason);

	public abstract void Reserve(ChoreDriver reserver);

	public abstract void Begin(Precondition.Context context);

	public abstract bool InProgress();

	public virtual string ResolveString(string str)
	{
		return str;
	}

	public static int GetNextChoreID()
	{
		return ++nextId;
	}
}
public class Chore<StateMachineInstanceType> : StandardChoreBase, IStateMachineTarget where StateMachineInstanceType : StateMachine.Instance
{
	private int onTargetDestroyedHandlerID;

	public StateMachineInstanceType smi { get; protected set; }

	public override GameObject gameObject => target.gameObject;

	public Transform transform => target.gameObject.transform;

	public string name => gameObject.name;

	public override bool isNull => target.isNull;

	protected override StateMachine.Instance GetSMI()
	{
		return smi;
	}

	public int Subscribe(int hash, Action<object> handler)
	{
		return GetComponent<KPrefabID>().Subscribe(hash, handler);
	}

	public int Subscribe(int hash, Action<object, object> handler, object context)
	{
		return GetComponent<KPrefabID>().Subscribe(hash, handler, context);
	}

	public void Unsubscribe(int hash, Action<object> handler)
	{
		GetComponent<KPrefabID>().Unsubscribe(hash, handler);
	}

	public void Unsubscribe(int id)
	{
		GetComponent<KPrefabID>().Unsubscribe(id);
	}

	public void Unsubscribe(ref int id)
	{
		Unsubscribe(id);
		id = -1;
	}

	public void Trigger(int hash, object data = null)
	{
		GetComponent<KPrefabID>().Trigger(hash, data);
	}

	public ComponentType GetComponent<ComponentType>()
	{
		return target.GetComponent<ComponentType>();
	}

	public Chore(ChoreType chore_type, IStateMachineTarget target, ChoreProvider chore_provider, bool run_until_complete = true, Action<Chore> on_complete = null, Action<Chore> on_begin = null, Action<Chore> on_end = null, PriorityScreen.PriorityClass master_priority_class = PriorityScreen.PriorityClass.basic, int master_priority_value = 5, bool is_preemptable = false, bool allow_in_context_menu = true, int priority_mod = 0, bool add_to_daily_report = false, ReportManager.ReportType report_type = ReportManager.ReportType.WorkTime)
		: base(chore_type, target, chore_provider, run_until_complete, on_complete, on_begin, on_end, master_priority_class, master_priority_value, is_preemptable, allow_in_context_menu, priority_mod, add_to_daily_report, report_type)
	{
		onTargetDestroyedHandlerID = target.Subscribe(1969584890, OnTargetDestroyed);
		reportType = report_type;
		addToDailyReport = add_to_daily_report;
		if (addToDailyReport)
		{
			ReportManager.Instance.ReportValue(ReportManager.ReportType.ChoreStatus, 1f, chore_type.Name, GameUtil.GetChoreName(this, null));
		}
	}

	public override string ResolveString(string str)
	{
		if (!target.isNull)
		{
			str = str.Replace("{Target}", target.gameObject.GetProperName());
		}
		return base.ResolveString(str);
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (target != null)
		{
			target.Unsubscribe(ref onTargetDestroyedHandlerID);
		}
		if (onCleanup != null)
		{
			onCleanup(this);
		}
	}

	private void OnTargetDestroyed(object data)
	{
		Cancel("Target Destroyed");
	}

	public override bool CanPreempt(Precondition.Context context)
	{
		return base.CanPreempt(context);
	}
}
