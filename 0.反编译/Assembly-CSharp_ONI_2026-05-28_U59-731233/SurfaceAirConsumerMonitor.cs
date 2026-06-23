public class SurfaceAirConsumerMonitor : GameStateMachine<SurfaceAirConsumerMonitor, SurfaceAirConsumerMonitor.Instance, IStateMachineTarget, SurfaceAirConsumerMonitor.Def>
{
	public class Def : BaseDef
	{
		public SimHashes element = SimHashes.Oxygen;

		public float minimumMassThreshold = 0.2f;

		public float cooldown = 600f;
	}

	public new class Instance : GameInstance
	{
		public int targetCell = Grid.InvalidCell;

		private Navigator navigator;

		private KPrefabID prefabID;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			navigator = master.GetComponent<Navigator>();
			prefabID = master.GetComponent<KPrefabID>();
		}

		public void FindSurfaceCell()
		{
			if (!prefabID.HasTag(GameTags.Creatures.WantsToConsumeAir) || targetCell == Grid.InvalidCell)
			{
				targetCell = Grid.InvalidCell;
				SurfaceCellQuery surfaceCellQuery = new SurfaceCellQuery(this, 25);
				navigator.RunQuery(surfaceCellQuery);
				if (surfaceCellQuery.success)
				{
					targetCell = surfaceCellQuery.GetResultCell();
				}
			}
		}

		public bool IsSurfaceLiquidCell(int cell)
		{
			if (!Grid.IsValidCell(cell))
			{
				return false;
			}
			if (!Grid.Element[cell].IsLiquid)
			{
				return false;
			}
			int num = Grid.CellAbove(cell);
			if (!Grid.IsValidCell(num))
			{
				return false;
			}
			Element element = Grid.Element[num];
			if (element.id != base.def.element)
			{
				return false;
			}
			if (Grid.Mass[num] < base.def.minimumMassThreshold)
			{
				return false;
			}
			int num2 = Grid.CellBelow(cell);
			if (!Grid.IsValidCell(num2) || !Grid.Element[num2].IsLiquid)
			{
				return false;
			}
			return true;
		}
	}

	public class SurfaceCellQuery : PathFinderQuery
	{
		public bool success;

		private Instance smi;

		private int maxIterations;

		public SurfaceCellQuery(Instance smi, int maxIterations)
		{
			this.smi = smi;
			this.maxIterations = maxIterations;
		}

		public override bool IsMatch(int cell, int parent_cell, int cost)
		{
			success = smi.IsSurfaceLiquidCell(cell);
			return success || --maxIterations <= 0;
		}
	}

	public State cooldown;

	public State looking;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = looking;
		looking.PreBrainUpdate(delegate(Instance smi)
		{
			smi.FindSurfaceCell();
		}).ToggleBehaviour(GameTags.Creatures.WantsToConsumeAir, (Instance smi) => smi.targetCell != Grid.InvalidCell, delegate(Instance smi)
		{
			smi.GoTo(cooldown);
		});
		cooldown.Enter(delegate(Instance smi)
		{
			smi.targetCell = Grid.InvalidCell;
		}).ScheduleGoTo((Instance smi) => smi.def.cooldown, looking);
	}
}
