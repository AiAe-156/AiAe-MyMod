using STRINGS;
using UnityEngine;

public class StompStates : GameStateMachine<StompStates, StompStates.Instance, IStateMachineTarget, StompStates.Def>
{
	public class Def : BaseDef
	{
	}

	public class StompState : State
	{
		public State pre;

		public State loop;

		public State pst;
	}

	public new class Instance : GameInstance
	{
		public CellOffset[] TargetOffsets;

		private OccupyArea occupyArea;

		public float StompLoopTimer => base.sm.stompingLoopTimer.Get(this);

		public GameObject CurrentTarget => base.sm.target.Get(this);

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.WantsToStomp);
			occupyArea = GetComponent<OccupyArea>();
			base.sm.stomper.Set(base.smi.gameObject, base.smi);
		}

		public void SetTarget(GameObject target)
		{
			base.smi.sm.target.Set(target, base.smi);
			if (CurrentTarget == null)
			{
				TargetOffsets = new CellOffset[1]
				{
					new CellOffset(0, 0)
				};
				return;
			}
			ListPool<CellOffset, Instance>.PooledList pooledList = ListPool<CellOffset, Instance>.Allocate();
			StompMonitor.Def.GetObjectCellsOffsetsWithExtraBottomPadding(CurrentTarget, pooledList);
			TargetOffsets = pooledList.ToArray();
			pooledList.Recycle();
		}

		public bool HarvestAnyOneIntersectingPlant()
		{
			int num = Grid.PosToCell(base.gameObject);
			int num2 = num;
			GameObject gameObject = null;
			bool result = false;
			for (int i = 0; i < occupyArea.OccupiedCellsOffsets.Length; i++)
			{
				num2 = Grid.OffsetCell(num, occupyArea.OccupiedCellsOffsets[i]);
				if (!Grid.IsValidCell(num2))
				{
					continue;
				}
				gameObject = Grid.Objects[num2, 5];
				gameObject = ((gameObject != null) ? gameObject : Grid.Objects[num2, 1]);
				if (!(gameObject == null))
				{
					Harvestable component = gameObject.GetComponent<Harvestable>();
					if (!(component == null) && component.CanBeHarvested)
					{
						component.Trigger(2127324410, (object)BoxedBools.True);
						component.Harvest();
						result = true;
						break;
					}
				}
			}
			return result;
		}
	}

	public const string PRE_STOMP_ANIM_NAME = "stomping_pre";

	public const string LOOP_STOMP_ANIM_NAME = "stomping_loop";

	public const string PST_STOMP_ANIM_NAME = "stomping_pst";

	private const int STOMP_LOOP_ANIM_FRAME_COUNT = 55;

	private const float STOMP_LOOP_ANIM_DURATION = 1.8333334f;

	public ApproachSubState<IApproachable> approach;

	public StompState stomp;

	public State complete;

	public State failed;

	public FloatParameter stompingLoopTimer;

	public TargetParameter stomper;

	public TargetParameter target;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.Never;
		default_state = approach;
		root.Enter(RefreshTarget);
		approach.InitializeStates(stomper, target, (Instance smi) => smi.TargetOffsets, stomp, failed).ToggleMainStatusItem(GetGoingToStompStatusItem).OnTargetLost(target, failed)
			.Target(target)
			.EventTransition(GameHashes.Harvest, failed)
			.EventTransition(GameHashes.Uprooted, failed)
			.EventTransition(GameHashes.QueueDestroyObject, failed);
		stomp.DefaultState(stomp.pre).ToggleMainStatusItem(GetStompingStatusItem);
		stomp.pre.Enter(ResetStompLoopTimer).PlayAnim("stomping_pre", KAnim.PlayMode.Once).OnAnimQueueComplete(stomp.loop);
		stomp.loop.ParamTransition(stompingLoopTimer, stomp.pst, GameStateMachine<StompStates, Instance, IStateMachineTarget, Def>.IsLTZero).PlayAnim("stomping_loop", KAnim.PlayMode.Loop).Update(StompUpdate);
		stomp.pst.PlayAnim("stomping_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(complete);
		complete.BehaviourComplete(GameTags.Creatures.WantsToStomp);
		failed.Enter(ReportFailure).EnterGoTo(null);
	}

	private static StatusItem GetGoingToStompStatusItem(Instance smi)
	{
		return GetStatusItem(smi, CREATURES.STATUSITEMS.GOING_TO_STOMP.NAME, CREATURES.STATUSITEMS.GOING_TO_STOMP.TOOLTIP);
	}

	private static StatusItem GetStompingStatusItem(Instance smi)
	{
		return GetStatusItem(smi, CREATURES.STATUSITEMS.STOMPING.NAME, CREATURES.STATUSITEMS.STOMPING.TOOLTIP);
	}

	private static StatusItem GetStatusItem(Instance smi, string name, string tooltip)
	{
		return new StatusItem(smi.GetCurrentState().longName, name, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString));
	}

	private static void ResetStompLoopTimer(Instance smi)
	{
		smi.sm.stompingLoopTimer.Set(0f, smi);
	}

	private static void StompUpdate(Instance smi, float dt)
	{
		if (smi.StompLoopTimer > 1.8333334f)
		{
			if (smi.HarvestAnyOneIntersectingPlant())
			{
				ResetStompLoopTimer(smi);
			}
			else
			{
				smi.sm.stompingLoopTimer.Set(-1f, smi);
			}
		}
		else
		{
			smi.sm.stompingLoopTimer.Set(smi.StompLoopTimer + dt, smi);
		}
	}

	private static void RefreshTarget(Instance smi)
	{
		StompMonitor.Instance sMI = smi.GetSMI<StompMonitor.Instance>();
		smi.SetTarget(sMI.Target);
	}

	private static void ReportFailure(Instance smi)
	{
		StompMonitor.Instance sMI = smi.GetSMI<StompMonitor.Instance>();
		sMI?.sm.StompStateFailed.Trigger(sMI);
	}
}
