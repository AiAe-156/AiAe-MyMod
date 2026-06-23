using System;
using UnityEngine;

public class FleeChore : Chore<FleeChore.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, FleeChore, object>.GameInstance
	{
		public StatesInstance(FleeChore master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, FleeChore>
	{
		public TargetParameter fleeFromTarget;

		public TargetParameter fleeToTarget;

		public TargetParameter self;

		public State planFleeRoute;

		public ApproachSubState<IApproachable> flee;

		public State cower;

		public State end;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = planFleeRoute;
			root.ToggleStatusItem(Db.Get().DuplicantStatusItems.Fleeing, null).ToggleNotification((StatesInstance smi) => new Notification(Db.Get().DuplicantStatusItems.Fleeing.notificationText, Db.Get().DuplicantStatusItems.Fleeing.notificationType, null, null, expires: true, 0f, null, null, smi.master.gameObject.transform));
			planFleeRoute.Enter(delegate(StatesInstance smi)
			{
				int fleeFromCell = Grid.PosToCell(fleeFromTarget.Get(smi));
				int startCell = Grid.PosToCell(smi.master.gameObject);
				int num = FloodFill.FindBest(RateCell, BoundaryCondition, startCell, 300);
				if (num != -1)
				{
					smi.sm.fleeToTarget.Set(smi.master.CreateLocator(Grid.CellToPos(num)), smi);
					smi.sm.fleeToTarget.Get(smi).name = "FleeLocator";
					if (num == fleeFromCell)
					{
						smi.GoTo(cower);
					}
					else
					{
						smi.GoTo(flee);
					}
				}
				else
				{
					smi.GoTo(cower);
				}
				FloodFill.BoundaryCheckResult BoundaryCondition(int cell)
				{
					return (!smi.master.CanFleeTo(cell)) ? FloodFill.BoundaryCheckResult.Halt : FloodFill.BoundaryCheckResult.Continue;
				}
				float RateCell(int cell)
				{
					int num2 = -1;
					if (!smi.master.nav.CanReach(cell))
					{
						return num2;
					}
					num2 += Grid.GetCellDistance(cell, fleeFromCell);
					if (smi.master.isInFavoredDirection(cell, fleeFromCell))
					{
						num2 += 8;
					}
					return num2;
				}
			});
			flee.InitializeStates(self, fleeToTarget, cower, cower, null, NavigationTactics.ReduceTravelDistance).ToggleAnims("anim_loco_run_insane_kanim", 2f);
			cower.ToggleAnims("anim_cringe_kanim", 4f).PlayAnim("cringe_pre").QueueAnim("cringe_loop")
				.QueueAnim("cringe_pst")
				.OnAnimQueueComplete(end);
			end.Enter(delegate(StatesInstance smi)
			{
				smi.StopSM("stopped");
			});
		}
	}

	private Navigator nav;

	public FleeChore(IStateMachineTarget target, GameObject enemy)
		: base(Db.Get().ChoreTypes.Flee, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.compulsory, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new StatesInstance(this);
		base.smi.sm.self.Set(gameObject, base.smi);
		nav = gameObject.GetComponent<Navigator>();
		base.smi.sm.fleeFromTarget.Set(enemy, base.smi);
	}

	private bool isInFavoredDirection(int cell, int fleeFromCell)
	{
		bool flag = ((Grid.CellToPos(fleeFromCell).x < gameObject.transform.GetPosition().x) ? true : false);
		bool flag2 = ((Grid.CellToPos(fleeFromCell).x < Grid.CellToPos(cell).x) ? true : false);
		return flag == flag2;
	}

	private bool CanFleeTo(int cell)
	{
		return nav.CanReach(cell) || nav.CanReach(Grid.OffsetCell(cell, -1, -1)) || nav.CanReach(Grid.OffsetCell(cell, 1, -1)) || nav.CanReach(Grid.OffsetCell(cell, -1, 1)) || nav.CanReach(Grid.OffsetCell(cell, 1, 1));
	}

	public GameObject CreateLocator(Vector3 pos)
	{
		return ChoreHelpers.CreateLocator("GoToLocator", pos);
	}

	protected override void OnStateMachineStop(string reason, StateMachine.Status status)
	{
		if (base.smi.sm.fleeToTarget.Get(base.smi) != null)
		{
			ChoreHelpers.DestroyLocator(base.smi.sm.fleeToTarget.Get(base.smi));
		}
		base.OnStateMachineStop(reason, status);
	}
}
