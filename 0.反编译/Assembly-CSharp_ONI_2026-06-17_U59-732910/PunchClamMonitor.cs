using UnityEngine;

public class PunchClamMonitor : GameStateMachine<PunchClamMonitor, PunchClamMonitor.Instance, IStateMachineTarget, PunchClamMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		private KPrefabID reservedClamID;

		private Navigator navigator;

		public ClamHarvestable Clam
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

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			navigator = GetComponent<Navigator>();
		}

		public void SearchForClam(float dt)
		{
			Grid.PosToCell(this);
			foreach (ClamHarvestable clamHarvestable in Components.ClamHarvestables)
			{
				if (!(clamHarvestable == null) && clamHarvestable.IsClosedAndReadyForHarvesting && !clamHarvestable.HasTag(ReservedTag) && CanReachClam(clamHarvestable))
				{
					base.sm.clamTarget.Set(clamHarvestable.gameObject, this);
					break;
				}
			}
		}

		public void ToggleClamReservation(bool reserve)
		{
			if (reserve)
			{
				ClamHarvestable clam = Clam;
				if (!(clam == null))
				{
					reservedClamID = clam.GetComponent<KPrefabID>();
					reservedClamID.AddTag(ReservedTag);
				}
			}
			else if (reservedClamID != null)
			{
				reservedClamID.RemoveTag(ReservedTag);
				reservedClamID = null;
			}
		}

		public bool CanReachClam(ClamHarvestable targetClam)
		{
			if (Vector2.Distance((Vector2)base.smi.transform.position, (Vector2)targetClam.transform.position) > 32f)
			{
				return false;
			}
			int cell = Grid.PosToCell(targetClam.transform.GetPosition());
			CellOffset[] clamCellOffsets = ClamCellOffsets;
			foreach (CellOffset offset in clamCellOffsets)
			{
				int cell2 = Grid.OffsetCell(cell, offset);
				if (navigator.GetNavigationCost(cell2) != -1)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static Tag ReservedTag = new Tag("PunchClamReserved");

	public static CellOffset[] ClamCellOffsets = new CellOffset[2]
	{
		new CellOffset(-1, 0),
		new CellOffset(1, 0)
	};

	public State searching;

	public State found;

	private TargetParameter clamTarget;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.Never;
		default_state = searching;
		searching.ParamTransition(clamTarget, found, GameStateMachine<PunchClamMonitor, Instance, IStateMachineTarget, Def>.IsNotNull).Update(SearchUpdate, UpdateRate.SIM_4000ms);
		found.ParamTransition(clamTarget, searching, GameStateMachine<PunchClamMonitor, Instance, IStateMachineTarget, Def>.IsNull).Toggle("Toggle clam reservation", ReserveClam, UnreserveClam).ToggleBehaviour(GameTags.Creatures.WantsToPunchClam, (Instance smi) => true, ClearTarget);
	}

	public static void SearchUpdate(Instance smi, float dt)
	{
		smi.SearchForClam(dt);
	}

	public static void ReserveClam(Instance smi)
	{
		smi.ToggleClamReservation(reserve: true);
	}

	public static void UnreserveClam(Instance smi)
	{
		smi.ToggleClamReservation(reserve: false);
	}

	private static void ClearTarget(Instance smi)
	{
		smi.sm.clamTarget.Set(null, smi);
	}
}
