using System;
using Klei.AI;
using TUNING;
using UnityEngine;

public class RoboDancerChore : Chore<RoboDancerChore.StatesInstance>, IWorkerPrioritizable
{
	public class States : GameStateMachine<States, StatesInstance, RoboDancerChore>
	{
		public class DancingStates : State
		{
			public State pre;

			public State variation_1;

			public State variation_2;

			public State pst;
		}

		public TargetParameter roboDancer;

		public State idle;

		public State goToStand;

		public DancingStates dancing;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = goToStand;
			Target(roboDancer);
			idle.EventTransition(GameHashes.ScheduleBlocksTick, goToStand, (StatesInstance smi) => !smi.IsRecTime());
			goToStand.MoveTo((StatesInstance smi) => smi.GetTargetCell(), dancing, idle);
			dancing.ToggleEffect("Dancing").ToggleAnims("anim_bionic_joy_kanim").DefaultState(dancing.pre)
				.Update(delegate(StatesInstance smi, float dt)
				{
					RoboDancer.Instance sMI = roboDancer.Get(smi).GetSMI<RoboDancer.Instance>();
					RoboDancer sm = sMI.sm;
					sm.hasAudience.Set(smi.HasAudience(), sMI);
					sm.timeSpentDancing.Set(sm.timeSpentDancing.Get(sMI) + dt, sMI);
				}, UpdateRate.SIM_33ms)
				.Exit(delegate(StatesInstance smi)
				{
					smi.ClearAudienceWorkables();
				});
			dancing.pre.QueueAnim("robotdance_pre").OnAnimQueueComplete(dancing.variation_1).Enter(delegate(StatesInstance smi)
			{
				smi.ClearAudienceWorkables();
				smi.CreateAudienceWorkables();
			});
			dancing.variation_1.QueueAnim("robotdance_loop").OnAnimQueueComplete(dancing.variation_2);
			dancing.variation_2.QueueAnim("robotdance_2_loop").OnAnimQueueComplete(dancing.pst);
			dancing.pst.QueueAnim("robotdance_pst").OnAnimQueueComplete(dancing.pre);
		}
	}

	public class StatesInstance : GameStateMachine<States, StatesInstance, RoboDancerChore, object>.GameInstance
	{
		private GameObject roboDancer;

		private GameObject[] audienceWorkables = new GameObject[4];

		private WatchRoboDancerWorkable[] watchWorkables;

		private static Precondition IsNotRoboHyped = new Precondition
		{
			id = "IsNotRoboHyped",
			description = "__ Duplicant hasn't watched the dance yet",
			fn = delegate(ref Precondition.Context context, object data)
			{
				if (context.consumerState.consumer == null)
				{
					return false;
				}
				return !context.consumerState.gameObject.GetComponent<Effects>().HasEffect(WatchRoboDancerWorkable.TRACKING_EFFECT);
			}
		};

		public StatesInstance(RoboDancerChore master, GameObject roboDancer)
			: base(master)
		{
			this.roboDancer = roboDancer;
			base.sm.roboDancer.Set(roboDancer, base.smi);
		}

		public bool IsRecTime()
		{
			return base.master.GetComponent<Schedulable>().IsAllowed(Db.Get().ScheduleBlockTypes.Recreation);
		}

		public int GetTargetCell()
		{
			Navigator component = GetComponent<Navigator>();
			float num = float.MaxValue;
			SocialGatheringPoint socialGatheringPoint = null;
			foreach (SocialGatheringPoint item in Components.SocialGatheringPoints.GetItems(Grid.WorldIdx[Grid.PosToCell(this)]))
			{
				float num2 = component.GetNavigationCost(Grid.PosToCell(item));
				if (num2 != -1f && num2 < num)
				{
					num = num2;
					socialGatheringPoint = item;
				}
			}
			if (socialGatheringPoint != null)
			{
				return Grid.PosToCell(socialGatheringPoint);
			}
			return Grid.PosToCell(base.master.gameObject);
		}

		public bool HasAudience()
		{
			if (base.smi.watchWorkables == null)
			{
				return false;
			}
			WatchRoboDancerWorkable[] array = base.smi.watchWorkables;
			for (int i = 0; i < array.Length; i++)
			{
				if ((bool)array[i].worker)
				{
					return true;
				}
			}
			return false;
		}

		public void CreateAudienceWorkables()
		{
			int num = Grid.PosToCell(base.gameObject);
			Vector3Int[] array = new Vector3Int[6]
			{
				Vector3Int.left * 3,
				Vector3Int.left * 2,
				Vector3Int.left,
				Vector3Int.right,
				Vector3Int.right * 2,
				Vector3Int.right * 3
			};
			int num2 = 0;
			for (int i = 0; i < audienceWorkables.Length; i++)
			{
				int cell = Grid.OffsetCell(num, array[i].x, array[i].y);
				if (Grid.IsValidCellInWorld(cell, Grid.WorldIdx[num]))
				{
					GameObject gameObject = ChoreHelpers.CreateLocator("WatchRoboDancerWorkable", Grid.CellToPos(cell));
					audienceWorkables[i] = gameObject;
					KSelectable kSelectable = gameObject.AddOrGet<KSelectable>();
					kSelectable.SetName("WatchRoboDancerWorkable");
					kSelectable.IsSelectable = false;
					WatchRoboDancerWorkable watchRoboDancerWorkable = gameObject.AddOrGet<WatchRoboDancerWorkable>();
					watchRoboDancerWorkable.owner = roboDancer;
					WorkChore<WatchRoboDancerWorkable> workChore = new WorkChore<WatchRoboDancerWorkable>(Db.Get().ChoreTypes.JoyReaction, watchRoboDancerWorkable, null, run_until_complete: true, null, null, null, allow_in_red_alert: true, Db.Get().ScheduleBlockTypes.Recreation, ignore_schedule_block: false, only_when_operational: true, null, is_preemptable: false, allow_in_context_menu: true, allow_prioritization: true, PriorityScreen.PriorityClass.high);
					workChore.AddPrecondition(ChorePreconditions.instance.IsNotARobot);
					workChore.AddPrecondition(IsNotRoboHyped, workChore);
					num2++;
				}
			}
			watchWorkables = new WatchRoboDancerWorkable[num2];
			for (int j = 0; j < num2; j++)
			{
				watchWorkables[j] = audienceWorkables[j].GetComponent<WatchRoboDancerWorkable>();
			}
		}

		public void ClearAudienceWorkables()
		{
			for (int i = 0; i < audienceWorkables.Length; i++)
			{
				if (!(audienceWorkables[i] == null))
				{
					WorkerBase worker = audienceWorkables[i].GetComponent<WatchRoboDancerWorkable>().worker;
					if (worker != null)
					{
						audienceWorkables[i].GetComponent<WatchRoboDancerWorkable>().CompleteWork(worker);
					}
					ChoreHelpers.DestroyLocator(audienceWorkables[i]);
				}
			}
			watchWorkables = null;
		}
	}

	private int basePriority = RELAXATION.PRIORITY.TIER1;

	public RoboDancerChore(IStateMachineTarget target)
		: base(Db.Get().ChoreTypes.JoyReaction, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.high, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.PersonalTime)
	{
		showAvailabilityInHoverText = false;
		base.smi = new StatesInstance(this, target.gameObject);
		AddPrecondition(ChorePreconditions.instance.IsNotRedAlert);
		AddPrecondition(ChorePreconditions.instance.IsScheduledTime, Db.Get().ScheduleBlockTypes.Recreation);
		AddPrecondition(ChorePreconditions.instance.CanDoWorkerPrioritizable, this);
	}

	public bool GetWorkerPriority(WorkerBase worker, out int priority)
	{
		priority = basePriority;
		return true;
	}
}
