public class RemoteWorkerGunkMonitor : StateMachineComponent<RemoteWorkerGunkMonitor.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, RemoteWorkerGunkMonitor, object>.GameInstance
	{
		public StatesInstance(RemoteWorkerGunkMonitor master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, RemoteWorkerGunkMonitor>
	{
		private State ok;

		private State high_gunk;

		private State full_gunk;

		public override void InitializeStates(out BaseState default_state)
		{
			base.InitializeStates(out default_state);
			default_state = ok;
			ok.Transition(full_gunk, IsFullOfGunk).Transition(high_gunk, IsGunkHigh);
			high_gunk.Transition(full_gunk, IsFullOfGunk).Transition(ok, IsGunkLevelOk).ToggleStatusItem(Db.Get().DuplicantStatusItems.RemoteWorkerHighGunkLevel);
			full_gunk.Transition(high_gunk, IsGunkHigh).Transition(ok, IsGunkLevelOk).ToggleStatusItem(Db.Get().DuplicantStatusItems.RemoteWorkerFullGunkLevel);
		}

		public static bool IsGunkLevelOk(StatesInstance smi)
		{
			return smi.master.Gunk < 16.000002f;
		}

		public static bool IsGunkHigh(StatesInstance smi)
		{
			if (smi.master.Gunk >= 16.000002f)
			{
				return smi.master.Gunk < 20.000002f;
			}
			return false;
		}

		public static bool IsFullOfGunk(StatesInstance smi)
		{
			return smi.master.Gunk >= 20.000002f;
		}
	}

	[MyCmpGet]
	private Storage storage;

	public const float CAPACITY_KG = 20.000002f;

	public const float HIGH_LEVEL = 16.000002f;

	public const float DRAIN_AMOUNT_KG_PER_S = 3.3333337f;

	public float Gunk => storage.GetMassAvailable(SimHashes.LiquidGunk);

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}

	public float GunkLevel()
	{
		return Gunk / 20.000002f;
	}
}
