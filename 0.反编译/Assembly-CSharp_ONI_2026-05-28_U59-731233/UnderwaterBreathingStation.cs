public class UnderwaterBreathingStation : GameStateMachine<UnderwaterBreathingStation, UnderwaterBreathingStation.Instance, IStateMachineTarget, UnderwaterBreathingStation.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public UnderwaterBreathingLocation location;

		private Storage storage;

		private MeterController meter;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			storage = GetComponent<Storage>();
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			meter = new MeterController(component, "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.BuildingBack);
		}

		public override void StartSM()
		{
			location = GetComponent<UnderwaterBreathingLocation>();
			base.StartSM();
			RefreshMeter();
		}

		protected override void OnCleanUp()
		{
			location.UnmarkCells();
			base.OnCleanUp();
		}

		public void RefreshMeter()
		{
			float positionPercent = storage.MassStored() / storage.capacityKg;
			meter.SetPositionPercent(positionPercent);
		}
	}

	private const string METER_TARGET_NAME = "meter_target";

	private const string METER_ANIM_NAME = "meter";

	public State off;

	public State on;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = off;
		root.EventHandler(GameHashes.OnStorageChange, RefreshMeter);
		off.EventTransition(GameHashes.OperationalChanged, on, (Instance smi) => smi.GetComponent<Operational>().IsOperational).Enter(RemoveCells);
		on.EventTransition(GameHashes.OperationalChanged, off, (Instance smi) => !smi.GetComponent<Operational>().IsOperational).Enter(AddCells).Exit(RemoveCells);
	}

	private static void RefreshMeter(Instance smi)
	{
		smi.RefreshMeter();
	}

	private static void AddCells(Instance smi)
	{
		smi.location.MarkCells();
	}

	private static void RemoveCells(Instance smi)
	{
		smi.location.UnmarkCells();
	}
}
