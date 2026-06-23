public class SetNavOrientationOnSpawnMonitor : GameStateMachine<SetNavOrientationOnSpawnMonitor, SetNavOrientationOnSpawnMonitor.Instance, IStateMachineTarget, SetNavOrientationOnSpawnMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		private int setSpawnOrientationHandler = -1;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			setSpawnOrientationHandler = Subscribe(1119167081, SetSpawnOrientation);
		}

		public void SetSpawnOrientation(object o)
		{
			int cell = Grid.PosToCell(this);
			if (Grid.IsValidCell(cell))
			{
				int num = Grid.CellAbove(cell);
				int num2 = Grid.CellBelow(cell);
				if (Grid.IsValidCell(num) && Grid.Solid[num] && (!Grid.IsValidCell(num2) || !Grid.Solid[num2]))
				{
					Navigator component = base.gameObject.GetComponent<Navigator>();
					component.CurrentNavType = NavType.Ceiling;
				}
			}
		}

		protected override void OnCleanUp()
		{
			Unsubscribe(ref setSpawnOrientationHandler);
			base.OnCleanUp();
		}
	}

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = root;
	}
}
