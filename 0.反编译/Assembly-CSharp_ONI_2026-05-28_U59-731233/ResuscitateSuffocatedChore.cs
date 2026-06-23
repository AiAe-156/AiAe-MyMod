using System;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class ResuscitateSuffocatedChore : Chore<ResuscitateSuffocatedChore.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, ResuscitateSuffocatedChore, object>.GameInstance
	{
		public KBatchedAnimController resucerController;

		public KBatchedAnimController rescueeController;

		public static SafeResuscitateCellQuery ResuscitateCellQuery = new SafeResuscitateCellQuery();

		public StatesInstance(ResuscitateSuffocatedChore master)
			: base(master)
		{
		}

		public static int FindClosestOxygenCellToIncapacitated(Navigator navigator, int start_cell)
		{
			if (navigator == null)
			{
				return Grid.InvalidCell;
			}
			OxygenBreather component = navigator.GetComponent<OxygenBreather>();
			if (component == null)
			{
				Debug.Assert(condition: false, "How is a non- oxygen breathing attempting to resuscitate?");
				return Grid.InvalidCell;
			}
			SafeResuscitateCellQuery safeResuscitateCellQuery = ResuscitateCellQuery.Reset(component);
			PathFinder.Run(potential_path: new PathFinder.PotentialPath(start_cell, NavType.Floor, PathFinder.PotentialPath.Flags.None), nav_grid: navigator.NavGrid, abilities: navigator.GetCurrentAbilities(), query: safeResuscitateCellQuery);
			return safeResuscitateCellQuery.GetResultCell();
		}
	}

	public class States : GameStateMachine<States, StatesInstance, ResuscitateSuffocatedChore>
	{
		public class HoldingSuffocated : State
		{
			public State pickup;

			public ApproachSubState<IApproachable> delivering;

			public State ditch;
		}

		public class Resuscitate : State
		{
			public State pre;

			public State loop;

			public State pst;
		}

		public ApproachSubState<IApproachable> approachSuffocated;

		public State failure;

		public State success;

		public HoldingSuffocated holding;

		public Resuscitate resuscitate;

		public TargetParameter rescueTarget;

		public TargetParameter deliverTarget;

		public TargetParameter rescuer;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = approachSuffocated;
			root.Enter(delegate(StatesInstance smi)
			{
				smi.sm.rescueTarget.Get(smi).Subscribe(1623392196, delegate
				{
					smi.GoTo(holding.ditch);
				});
			}).Exit(delegate(StatesInstance smi)
			{
				SyncControllers(smi, sync: false);
			});
			approachSuffocated.InitializeStates(rescuer, rescueTarget, holding.pickup, failure, Grid.DefaultOffset);
			holding.pickup.Target(rescuer).Enter(delegate(StatesInstance smi)
			{
				rescuer.Get(smi).GetComponent<Storage>().Store(rescueTarget.Get(smi));
				rescueTarget.Get(smi).transform.SetLocalPosition(Vector3.zero);
				KBatchedAnimTracker component = rescueTarget.Get(smi).GetComponent<KBatchedAnimTracker>();
				if (component != null)
				{
					component.symbol = new HashedString("snapTo_pivot");
					component.offset = new Vector3(0f, 0f, 1f);
				}
				PlayResuscitateAnim(smi, "pickup");
			}).EventTransition(GameHashes.AnimQueueComplete, holding.delivering);
			holding.delivering.InitializeStates(rescuer, deliverTarget, resuscitate.pre, holding.ditch).Enter(delegate(StatesInstance smi)
			{
				smi.rescueeController.Play("carry_loop", KAnim.PlayMode.Loop);
			}).Update(delegate(StatesInstance smi, float dt)
			{
				if (deliverTarget.Get(smi) == null)
				{
					smi.GoTo(holding.ditch);
				}
			});
			resuscitate.Enter(delegate(StatesInstance smi)
			{
				GameObject gameObject = rescuer.Get(smi).gameObject;
				if (!gameObject.IsNullOrDestroyed())
				{
					KAnimFile anim = Assets.GetAnim("anim_resuscitate_kanim");
					gameObject.GetComponent<KAnimControllerBase>().AddAnimOverrides(anim);
					KAnimFile anim2 = Assets.GetAnim("anim_drowning_kanim");
					KAnimControllerBase component = smi.master.GetComponent<KAnimControllerBase>();
					component.AddAnimOverrides(anim2);
					component.gameObject.transform.SetLocalPosition(new Vector3(0f, 0f, 1f));
				}
			}).Exit(delegate(StatesInstance smi)
			{
				GameObject gameObject = rescuer.Get(smi).gameObject;
				if (!gameObject.IsNullOrDestroyed())
				{
					KAnimFile anim = Assets.GetAnim("anim_resuscitate_kanim");
					gameObject.GetComponent<KAnimControllerBase>().RemoveAnimOverrides(anim);
					KAnimFile anim2 = Assets.GetAnim("anim_drowning_kanim");
					smi.master.GetComponent<KAnimControllerBase>().RemoveAnimOverrides(anim2);
				}
			});
			resuscitate.pre.Target(rescuer).Enter(delegate(StatesInstance smi)
			{
				SyncControllers(smi, sync: true);
				PlayResuscitateAnim(smi, "resuscitate_pre", synced: true);
			}).OnAnimQueueComplete(resuscitate.loop);
			resuscitate.loop.Target(rescuer).Enter(delegate(StatesInstance smi)
			{
				PlayResuscitateAnim(smi, "resuscitate_loop", synced: true);
			}).OnAnimQueueComplete(resuscitate.pst);
			resuscitate.pst.Target(rescuer).Enter(delegate(StatesInstance smi)
			{
				PlayResuscitateAnim(smi, "resuscitate_pst", synced: true);
			}).OnAnimQueueComplete(success);
			holding.ditch.PlayAnim("place").ScheduleGoTo(0.5f, failure).Exit(delegate(StatesInstance smi)
			{
				smi.master.DropIncapacitatedDuplicant();
			});
			failure.ReturnFailure();
			success.Enter(delegate(StatesInstance smi)
			{
				AmountInstance amountInstance = Db.Get().Amounts.Breath.Lookup(smi.gameObject);
				amountInstance.SetValue(amountInstance.GetMax());
				smi.Trigger(-1256572400);
			}).ReturnSuccess();
		}

		public static void PlayResuscitateAnim(StatesInstance smi, string anim_name, bool synced = false)
		{
			smi.resucerController.Play(anim_name);
			if (!synced)
			{
				smi.rescueeController.Play(anim_name);
			}
		}

		public static void SyncControllers(StatesInstance smi, bool sync)
		{
			KAnimSynchronizer synchronizer = smi.resucerController.GetSynchronizer();
			if (synchronizer != null)
			{
				if (sync)
				{
					synchronizer.Add(smi.rescueeController);
				}
				else
				{
					synchronizer.Remove(smi.rescueeController);
				}
			}
		}
	}

	public static Precondition CanReachIncapacitated = new Precondition
	{
		id = "CanReachIncapacitated",
		description = DUPLICANTS.CHORES.PRECONDITIONS.CAN_MOVE_TO,
		fn = delegate(ref Precondition.Context context, object data)
		{
			GameObject gameObject = (GameObject)data;
			if (gameObject == null)
			{
				return false;
			}
			int navigationCost = context.consumerState.navigator.GetNavigationCost(Grid.PosToCell(gameObject.transform.GetPosition()));
			if (-1 != navigationCost)
			{
				context.cost += navigationCost;
				return true;
			}
			return false;
		}
	};

	public static Precondition CanReachOxygenatedArea = new Precondition
	{
		id = "CanReachOxygenatedArea",
		description = DUPLICANTS.CHORES.PRECONDITIONS.CAN_MOVE_TO,
		fn = delegate(ref Precondition.Context context, object data)
		{
			if (context.chore.InProgress())
			{
				return true;
			}
			GameObject gameObject = (GameObject)data;
			if (gameObject == null)
			{
				return false;
			}
			Navigator navigator = context.consumerState.navigator;
			int cell = StatesInstance.FindClosestOxygenCellToIncapacitated(navigator, Grid.PosToCell(gameObject));
			if (!Grid.IsValidCell(cell))
			{
				return false;
			}
			int navigationCost = navigator.GetNavigationCost(cell);
			if (-1 != navigationCost)
			{
				context.cost += navigationCost;
				return true;
			}
			return false;
		}
	};

	public ResuscitateSuffocatedChore(IStateMachineTarget master, GameObject incapacitatedDuplicant)
		: base(Db.Get().ChoreTypes.RescueIncapacitated, master, (ChoreProvider)null, run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.personalNeeds, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new StatesInstance(this);
		runUntilComplete = true;
		AddPrecondition(ChorePreconditions.instance.NotChoreCreator, incapacitatedDuplicant.gameObject);
		AddPrecondition(ChorePreconditions.instance.IsNotARobot);
		AddPrecondition(CanReachIncapacitated, incapacitatedDuplicant);
		AddPrecondition(CanReachOxygenatedArea, incapacitatedDuplicant);
	}

	public override void Begin(Precondition.Context context)
	{
		base.smi.sm.rescuer.Set(context.consumerState.gameObject, base.smi);
		base.smi.sm.rescueTarget.Set(gameObject, base.smi);
		base.smi.resucerController = context.consumerState.gameObject.GetComponent<KBatchedAnimController>();
		base.smi.rescueeController = gameObject.GetComponent<KBatchedAnimController>();
		int cell = StatesInstance.FindClosestOxygenCellToIncapacitated(context.consumerState.navigator, Grid.PosToCell(gameObject));
		Vector3 pos = Grid.CellToPosCBC(cell, Grid.SceneLayer.Move);
		GameObject value = ChoreHelpers.CreateLocator("OxygenCell", pos);
		base.smi.sm.deliverTarget.Set(value, base.smi);
		base.Begin(context);
	}

	protected override void End(string reason)
	{
		DropIncapacitatedDuplicant();
		base.End(reason);
	}

	private void DropIncapacitatedDuplicant()
	{
		if (base.smi.sm.rescuer.Get(base.smi) != null && base.smi.sm.rescueTarget.Get(base.smi) != null)
		{
			Storage component = base.smi.sm.rescuer.Get(base.smi).GetComponent<Storage>();
			GameObject gameObject = base.smi.sm.rescueTarget.Get(base.smi);
			if (component.items.Contains(gameObject))
			{
				base.smi.sm.rescuer.Get(base.smi).GetComponent<Storage>().Drop(gameObject);
			}
		}
	}
}
