using STRINGS;
using UnityEngine;

public class IdleStates : GameStateMachine<IdleStates, IdleStates.Instance, IStateMachineTarget, IdleStates.Def>
{
	public class Def : BaseDef
	{
		public delegate HashedString IdleAnimCallback(Instance smi, ref HashedString pre_anim);

		public IdleAnimCallback customIdleAnim;

		public PriorityScreen.PriorityClass priorityClass = PriorityScreen.PriorityClass.basic;
	}

	public new class Instance : GameInstance
	{
		public Navigator navigator;

		public KPrefabID kpid;

		public KBatchedAnimController kac;

		public Facing facing;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			navigator = GetComponent<Navigator>();
			kpid = GetComponent<KPrefabID>();
			kac = GetComponent<KBatchedAnimController>();
			facing = GetComponent<Facing>();
			chore.masterPriority.priority_class = def.priorityClass;
		}
	}

	public class MoveCellQuery : PathFinderQuery
	{
		private NavType navType;

		private int targetCell = Grid.InvalidCell;

		private int maxIterations;

		public static MoveCellQuery Instance = new MoveCellQuery(NavType.Floor);

		public bool allowLiquid { get; set; }

		public bool submerged { get; set; }

		public bool lowerCellBias { get; set; }

		public MoveCellQuery(NavType navType)
		{
			Reset(navType);
		}

		public void Reset(NavType navType)
		{
			this.navType = navType;
			maxIterations = Random.Range(5, 25);
			targetCell = Grid.InvalidCell;
			allowLiquid = false;
			submerged = false;
			lowerCellBias = false;
		}

		public override bool IsMatch(int cell, int parent_cell, int cost)
		{
			if (!Grid.IsValidCell(cell))
			{
				return false;
			}
			if (Grid.ObjectLayers[9].ContainsKey(cell))
			{
				return false;
			}
			bool flag = submerged || Grid.IsNavigatableLiquid(cell);
			bool flag2 = navType != NavType.Swim;
			bool flag3 = navType == NavType.Swim || allowLiquid;
			if (flag && !flag3)
			{
				return false;
			}
			if (!flag && !flag2)
			{
				return false;
			}
			if (targetCell == Grid.InvalidCell || !lowerCellBias)
			{
				targetCell = cell;
			}
			else
			{
				int num = Grid.CellRow(targetCell);
				int num2 = Grid.CellRow(cell);
				if (num2 < num)
				{
					targetCell = cell;
				}
			}
			return --maxIterations <= 0;
		}

		public override int GetResultCell()
		{
			return targetCell;
		}
	}

	private State loop;

	private State move;

	public static StatusItem IdleStatus = new StatusItem("IdleStatus", CREATURES.STATUSITEMS.IDLE.NAME, CREATURES.STATUSITEMS.IDLE.TOOLTIP, "", StatusItem.IconType.Info, NotificationType.Messages, allow_multiples: false, OverlayModes.None.ID);

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = loop;
		root.Exit("StopNavigator", StopNavigator).ToggleMainStatusItem(IdleStatus).ToggleTag(GameTags.Idle);
		loop.Enter(PlayIdle).ToggleScheduleCallback("IdleMove", GetIdleTime, GoMove);
		move.Enter(MoveToNewCell).EventTransition(GameHashes.DestinationReached, loop).EventTransition(GameHashes.NavigationFailed, loop);
	}

	private static float GetIdleTime(Instance smi)
	{
		return Random.Range(3, 10);
	}

	private static void GoMove(Instance smi)
	{
		smi.GoTo(smi.sm.move);
	}

	private static void StopNavigator(Instance smi)
	{
		smi.navigator.Stop();
	}

	private static void MoveToNewCell(Instance smi)
	{
		if (smi.kpid.HasTag(GameTags.StationaryIdling))
		{
			smi.GoTo(smi.sm.loop);
			return;
		}
		MoveCellQuery instance = MoveCellQuery.Instance;
		instance.Reset(smi.navigator.CurrentNavType);
		instance.allowLiquid = smi.kpid.HasTag(GameTags.Amphibious);
		instance.submerged = smi.kpid.HasTag(GameTags.Creatures.Submerged);
		int num = Grid.PosToCell(smi.navigator);
		if (smi.navigator.CurrentNavType == NavType.Hover && CellSelectionObject.IsExposedToSpace(num))
		{
			int num2 = 0;
			int cell = num;
			for (int i = 0; i < 10; i++)
			{
				cell = Grid.CellBelow(cell);
				if (!Grid.IsValidCell(cell) || Grid.IsSolidCell(cell) || !CellSelectionObject.IsExposedToSpace(cell))
				{
					break;
				}
				num2++;
			}
			instance.lowerCellBias = num2 == 10;
		}
		smi.navigator.RunQuery(instance);
		if (smi.navigator.CanReach(instance.GetResultCell()))
		{
			smi.navigator.GoTo(instance.GetResultCell());
		}
		else
		{
			smi.GoTo(smi.sm.loop);
		}
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
