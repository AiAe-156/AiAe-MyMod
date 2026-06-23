using System;
using UnityEngine;

public class IdleChore : Chore<IdleChore.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, IdleChore, object>.GameInstance
	{
		private IdleCellSensor idleCellSensor;

		public Navigator navigator { get; private set; }

		public KBatchedAnimController animController { get; private set; }

		public StatesInstance(IdleChore master, GameObject idler)
			: base(master)
		{
			base.sm.idler.Set(idler, base.smi);
			navigator = GetComponent<Navigator>();
			animController = GetComponent<KBatchedAnimController>();
			idleCellSensor = GetComponent<Sensors>().GetSensor<IdleCellSensor>();
		}

		public int GetIdleCell()
		{
			return idleCellSensor.GetCell();
		}

		public bool HasIdleCell()
		{
			return idleCellSensor.GetCell() != Grid.InvalidCell;
		}
	}

	public class States : GameStateMachine<States, StatesInstance, IdleChore>
	{
		public class IdleState : State
		{
			public State onfloor;

			public State onladder;

			public State ontube;

			public State onsuitmarker;

			public State hovering;

			public State swimming;

			public State move;
		}

		public BoolParameter isOnLadder;

		public BoolParameter isOnTube;

		public BoolParameter isOnSuitMarkerCell;

		public BoolParameter isHovering;

		public BoolParameter isSwimming;

		public TargetParameter idler;

		public IdleState idle;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = idle;
			Target(idler);
			idle.DefaultState(idle.onfloor).Enter("UpdateNavType", delegate(StatesInstance smi)
			{
				UpdateNavType(smi);
			}).Update("UpdateNavType", delegate(StatesInstance smi, float dt)
			{
				UpdateNavType(smi);
			})
				.ToggleStateMachine((StatesInstance smi) => new TaskAvailabilityMonitor.Instance(smi.master))
				.ToggleTag(GameTags.Idle);
			idle.onfloor.PlayAnim("idle_default", KAnim.PlayMode.Loop).ParamTransition(isOnLadder, idle.onladder, GameStateMachine<States, StatesInstance, IdleChore, object>.IsTrue).ParamTransition(isOnTube, idle.ontube, GameStateMachine<States, StatesInstance, IdleChore, object>.IsTrue)
				.ParamTransition(isOnSuitMarkerCell, idle.onsuitmarker, GameStateMachine<States, StatesInstance, IdleChore, object>.IsTrue)
				.ParamTransition(isHovering, idle.hovering, GameStateMachine<States, StatesInstance, IdleChore, object>.IsTrue)
				.ParamTransition(isSwimming, idle.swimming, GameStateMachine<States, StatesInstance, IdleChore, object>.IsTrue)
				.ToggleScheduleCallback("IdleMove", (StatesInstance smi) => UnityEngine.Random.Range(5, 15), delegate(StatesInstance smi)
				{
					smi.GoTo(idle.move);
				});
			idle.onladder.PlayAnim("ladder_idle", KAnim.PlayMode.Loop).ToggleScheduleCallback("IdleMove", (StatesInstance smi) => UnityEngine.Random.Range(5, 15), delegate(StatesInstance smi)
			{
				smi.GoTo(idle.move);
			});
			idle.ontube.PlayAnim("tube_idle_loop", KAnim.PlayMode.Loop).Update("IdleMove", delegate(StatesInstance smi, float dt)
			{
				if (smi.HasIdleCell())
				{
					smi.GoTo(idle.move);
				}
			}, UpdateRate.SIM_1000ms);
			idle.hovering.PlayAnim("hover_idle", KAnim.PlayMode.Loop).Update("IdleMove", delegate(StatesInstance smi, float dt)
			{
				if (smi.HasIdleCell())
				{
					smi.GoTo(idle.move);
				}
			}, UpdateRate.SIM_1000ms);
			idle.swimming.Enter("swimming", delegate(StatesInstance smi)
			{
				string text = "treading_loop";
				if (!Grid.IsSubstantialLiquid(smi.navigator.cachedCell))
				{
					text = "shallow_treading_loop";
				}
				smi.animController.Play(text, KAnim.PlayMode.Loop);
			}).Update("IdleMove", delegate(StatesInstance smi, float dt)
			{
				if (smi.HasIdleCell())
				{
					smi.GoTo(idle.move);
				}
			}, UpdateRate.SIM_1000ms);
			idle.onsuitmarker.PlayAnim("idle_default", KAnim.PlayMode.Loop).Enter(delegate(StatesInstance smi)
			{
				int cell = Grid.PosToCell(smi);
				Grid.TryGetSuitMarkerFlags(cell, out var flags, out var _);
				IdleSuitMarkerCellQuery idleSuitMarkerCellQuery = new IdleSuitMarkerCellQuery((flags & Grid.SuitMarker.Flags.Rotated) != 0, Grid.CellToXY(cell).X);
				smi.navigator.RunQuery(idleSuitMarkerCellQuery);
				smi.navigator.GoTo(idleSuitMarkerCellQuery.GetResultCell());
			}).EventTransition(GameHashes.DestinationReached, idle)
				.ToggleScheduleCallback("IdleMove", (StatesInstance smi) => UnityEngine.Random.Range(5, 15), delegate(StatesInstance smi)
				{
					smi.GoTo(idle.move);
				});
			idle.move.Transition(idle, (StatesInstance smi) => !smi.HasIdleCell()).TriggerOnEnter(GameHashes.BeginWalk).TriggerOnExit(GameHashes.EndWalk)
				.ToggleAnims("anim_loco_walk_kanim")
				.MoveTo((StatesInstance smi) => smi.GetIdleCell(), idle, idle)
				.Exit("UpdateNavType", delegate(StatesInstance smi)
				{
					UpdateNavType(smi);
				})
				.Exit("ClearWalk", delegate(StatesInstance smi)
				{
					smi.animController.Play("idle_default");
				});
		}

		public static void UpdateNavType(StatesInstance smi)
		{
			NavType currentNavType = smi.navigator.CurrentNavType;
			smi.sm.isOnLadder.Set(currentNavType == NavType.Ladder || currentNavType == NavType.Pole, smi);
			smi.sm.isOnTube.Set(currentNavType == NavType.Tube, smi);
			smi.sm.isHovering.Set(currentNavType == NavType.Hover, smi);
			smi.sm.isSwimming.Set(currentNavType == NavType.Swim, smi);
			int num = Grid.PosToCell(smi);
			smi.sm.isOnSuitMarkerCell.Set(Grid.IsValidCell(num) && Grid.HasSuitMarker[num], smi);
		}
	}

	public IdleChore(IStateMachineTarget target)
		: base(Db.Get().ChoreTypes.Idle, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.idle, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.IdleTime)
	{
		showAvailabilityInHoverText = false;
		base.smi = new StatesInstance(this, target.gameObject);
	}
}
