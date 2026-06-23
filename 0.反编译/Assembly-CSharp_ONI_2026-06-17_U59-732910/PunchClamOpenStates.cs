public class PunchClamOpenStates : GameStateMachine<PunchClamOpenStates, PunchClamOpenStates.Instance, IStateMachineTarget, PunchClamOpenStates.Def>
{
	public class Def : BaseDef
	{
		public string PUNH_ANIM_PRE_NAME = "slap_pre";

		public string PUNH_ANIM_LOOP_NAME = "slap";

		public string PUNH_ANIM_PST_NAME = "slap_pst";
	}

	public new class Instance : GameInstance
	{
		public PunchClamMonitor.Instance punchClamMonitor;

		private Navigator navigator;

		public ClamHarvestable clam
		{
			get
			{
				if (!(base.sm.clamTarget.Get(this) == null))
				{
					return base.sm.clamTarget.Get(this).GetComponent<ClamHarvestable>();
				}
				return null;
			}
		}

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.WantsToPunchClam);
			navigator = GetComponent<Navigator>();
			punchClamMonitor = base.gameObject.GetSMI<PunchClamMonitor.Instance>();
		}

		public override void StartSM()
		{
			base.sm.clamTarget.Set(punchClamMonitor.Clam, this);
			base.StartSM();
		}

		public int GetBestCellToStand()
		{
			ClamHarvestable clamHarvestable = clam;
			if (clamHarvestable == null)
			{
				return Grid.InvalidCell;
			}
			int cell = Grid.PosToCell(clamHarvestable.transform.GetPosition());
			CellOffset[] clamCellOffsets = PunchClamMonitor.ClamCellOffsets;
			float num = float.MaxValue;
			int result = Grid.InvalidCell;
			CellOffset[] array = clamCellOffsets;
			foreach (CellOffset offset in array)
			{
				int num2 = Grid.OffsetCell(cell, offset);
				int navigationCost = navigator.GetNavigationCost(num2);
				if (navigationCost != -1 && (float)navigationCost < num)
				{
					num = navigationCost;
					result = num2;
				}
			}
			return result;
		}

		public void OpenClam()
		{
			if (clam != null)
			{
				clam.PunchOpen();
			}
		}
	}

	public State initialize;

	public State approach;

	public PreLoopPostState punch;

	public State openClam;

	public State exit;

	private TargetParameter clamTarget;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = initialize;
		initialize.ParamTransition(clamTarget, exit, GameStateMachine<PunchClamOpenStates, Instance, IStateMachineTarget, Def>.IsNull).GoTo(approach);
		approach.Target(masterTarget).ParamTransition(clamTarget, exit, GameStateMachine<PunchClamOpenStates, Instance, IStateMachineTarget, Def>.IsNull).ToggleMainStatusItem(Db.Get().CreatureStatusItems.PunchClamApproach)
			.MoveTo(GetBestCell, punch, exit)
			.Target(clamTarget)
			.EventHandlerTransition(GameHashes.WorkableCompleteWork, exit, CanNoLongerOpenClam);
		punch.ParamTransition(clamTarget, exit, GameStateMachine<PunchClamOpenStates, Instance, IStateMachineTarget, Def>.IsNull).DefaultState(punch.pre).ToggleMainStatusItem(Db.Get().CreatureStatusItems.PunchClamAttack);
		punch.pre.Face(clamTarget).PlayAnim((Instance smi) => smi.def.PUNH_ANIM_PRE_NAME).OnAnimQueueComplete(punch.loop);
		punch.loop.PlayAnim((Instance smi) => smi.def.PUNH_ANIM_LOOP_NAME, KAnim.PlayMode.Loop).ScheduleGoTo(2f, punch.pst);
		punch.pst.PlayAnim((Instance smi) => smi.def.PUNH_ANIM_PST_NAME).OnAnimQueueComplete(openClam);
		openClam.Enter(OpenClam).GoTo(exit);
		exit.BehaviourComplete(GameTags.Creatures.WantsToPunchClam);
	}

	public static int GetBestCell(Instance smi)
	{
		return smi.GetBestCellToStand();
	}

	public static void OpenClam(Instance smi)
	{
		smi.OpenClam();
	}

	public static bool CanNoLongerOpenClam(Instance smi, object o)
	{
		if (smi.clam != null)
		{
			return !smi.clam.IsClosedAndReadyForHarvesting;
		}
		return false;
	}
}
