using UnityEngine;

public class BuzzStates : GameStateMachine<BuzzStates, BuzzStates.Instance, IStateMachineTarget, BuzzStates.Def>
{
	public class Def : BaseDef
	{
		public delegate HashedString IdleAnimCallback(Instance smi, ref HashedString pre_anim);

		public IdleAnimCallback customIdleAnim;
	}

	public new class Instance : GameInstance
	{
		public Navigator navigator;

		public KBatchedAnimController kac;

		public KPrefabID kpid;

		public Facing facing;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			navigator = GetComponent<Navigator>();
			kac = GetComponent<KBatchedAnimController>();
			kpid = GetComponent<KPrefabID>();
			facing = GetComponent<Facing>();
		}
	}

	public class BuzzingStates : State
	{
		public State move;

		public State pause;
	}

	public class MoveCellQuery : PathFinderQuery
	{
		private NavType navType;

		private int targetCell = Grid.InvalidCell;

		private int maxIterations;

		public static MoveCellQuery Instance = new MoveCellQuery(NavType.Floor);

		public bool allowLiquid { get; set; }

		public MoveCellQuery(NavType navType)
		{
			this.navType = navType;
			maxIterations = Random.Range(5, 25);
		}

		public void Reset(NavType navType, bool allowLiquid)
		{
			this.navType = navType;
			maxIterations = Random.Range(5, 25);
			targetCell = Grid.InvalidCell;
			this.allowLiquid = allowLiquid;
		}

		public override bool IsMatch(int cell, int parent_cell, int cost)
		{
			if (!Grid.IsValidCell(cell))
			{
				return false;
			}
			bool flag = navType != NavType.Swim;
			bool flag2 = navType == NavType.Swim || allowLiquid;
			bool flag3 = Grid.IsSubstantialLiquid(cell);
			if (flag3 && !flag2)
			{
				return false;
			}
			if (!flag3 && !flag)
			{
				return false;
			}
			targetCell = cell;
			return --maxIterations <= 0;
		}

		public override int GetResultCell()
		{
			return targetCell;
		}
	}

	private IntParameter numMoves;

	private BuzzingStates buzz;

	public State idle;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = idle;
		root.Exit("StopNavigator", StopNavigator).ToggleMainStatusItem(IdleStates.IdleStatus).ToggleTag(GameTags.Idle);
		idle.Enter(PlayIdle).ToggleScheduleCallback("DoBuzz", GetIdleTime, GoBuzz);
		buzz.ParamTransition(numMoves, idle, GameStateMachine<BuzzStates, Instance, IStateMachineTarget, Def>.IsLTEZero_int);
		buzz.move.Enter(MoveToNewCell).EventTransition(GameHashes.DestinationReached, buzz.pause).EventTransition(GameHashes.NavigationFailed, buzz.pause);
		buzz.pause.Enter(BuzzPause);
	}

	private static float GetIdleTime(Instance smi)
	{
		return Random.Range(3, 10);
	}

	private static void GoBuzz(Instance smi)
	{
		smi.sm.numMoves.Set(Random.Range(4, 6), smi);
		smi.GoTo(smi.sm.buzz.move);
	}

	private static void BuzzPause(Instance smi)
	{
		smi.sm.numMoves.Set(smi.sm.numMoves.Get(smi) - 1, smi);
		smi.GoTo(smi.sm.buzz.move);
	}

	private static void StopNavigator(Instance smi)
	{
		smi.navigator.Stop();
	}

	private static void MoveToNewCell(Instance smi)
	{
		MoveCellQuery.Instance.Reset(smi.navigator.CurrentNavType, smi.kpid.HasTag(GameTags.Amphibious));
		smi.navigator.RunQuery(MoveCellQuery.Instance);
		smi.navigator.GoTo(MoveCellQuery.Instance.GetResultCell());
	}

	private static void PlayIdle(Instance smi)
	{
		NavType nav_type = smi.navigator.CurrentNavType;
		if (smi.facing.GetFacing())
		{
			nav_type = NavGrid.MirrorNavType(nav_type);
		}
		if (smi.def.customIdleAnim != null)
		{
			HashedString pre_anim = HashedString.Invalid;
			HashedString hashedString = smi.def.customIdleAnim(smi, ref pre_anim);
			if (hashedString != HashedString.Invalid)
			{
				if (pre_anim != HashedString.Invalid)
				{
					smi.kac.Play(pre_anim);
				}
				smi.kac.Queue(hashedString, KAnim.PlayMode.Loop);
				return;
			}
		}
		HashedString idleAnim = smi.navigator.NavGrid.GetIdleAnim(nav_type);
		smi.kac.Play(idleAnim, KAnim.PlayMode.Loop);
	}
}
