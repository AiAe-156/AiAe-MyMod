public class SafeCellMonitor : GameStateMachine<SafeCellMonitor, SafeCellMonitor.Instance, IStateMachineTarget, SafeCellMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		private SafeCellSensor safeCellSensor;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			safeCellSensor = GetComponent<Sensors>().GetSensor<SafeCellSensor>();
		}

		public bool IsAreaUnsafe()
		{
			return safeCellSensor.HasSafeCell();
		}
	}

	public State safe;

	public State danger;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = safe;
		root.ToggleUrge(Db.Get().Urges.MoveToSafety);
		safe.EventTransition(GameHashes.SafeCellDetected, danger, (Instance smi) => smi.IsAreaUnsafe());
		danger.EventTransition(GameHashes.SafeCellLost, safe, (Instance smi) => !smi.IsAreaUnsafe()).ToggleChore((Instance smi) => new MoveToSafetyChore(smi.master), safe);
	}
}
