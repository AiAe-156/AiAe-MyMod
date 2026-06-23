public class RemoteWorkerOilMonitor : StateMachineComponent<RemoteWorkerOilMonitor.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, RemoteWorkerOilMonitor, object>.GameInstance
	{
		public StatesInstance(RemoteWorkerOilMonitor master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, RemoteWorkerOilMonitor>
	{
		private State ok;

		private State low_oil;

		private State out_of_oil;

		public override void InitializeStates(out BaseState default_state)
		{
			base.InitializeStates(out default_state);
			default_state = ok;
			ok.Transition(out_of_oil, IsOutOfOil).Transition(low_oil, IsLowOnOil);
			low_oil.Transition(out_of_oil, IsOutOfOil).Transition(ok, IsOkForOil).ToggleStatusItem(Db.Get().DuplicantStatusItems.RemoteWorkerLowOil);
			out_of_oil.Transition(low_oil, IsLowOnOil).Transition(ok, IsOkForOil).ToggleStatusItem(Db.Get().DuplicantStatusItems.RemoteWorkerOutOfOil);
		}

		public static bool IsOkForOil(StatesInstance smi)
		{
			return smi.master.Oil > 4.0000005f;
		}

		public static bool IsLowOnOil(StatesInstance smi)
		{
			return smi.master.Oil >= float.Epsilon && smi.master.Oil < 4.0000005f;
		}

		public static bool IsOutOfOil(StatesInstance smi)
		{
			return smi.master.Oil < float.Epsilon;
		}
	}

	[MyCmpGet]
	private Storage storage;

	public const float CAPACITY_KG = 20.000002f;

	public const float LOW_LEVEL = 4.0000005f;

	public const float FILL_RATE_KG_PER_S = 2.5000002f;

	public const float CONSUMPTION_RATE_KG_PER_S = 1f / 30f;

	public float Oil => storage.GetMassAvailable(GameTags.LubricatingOil);

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}

	public float OilLevel()
	{
		return Oil / 20.000002f;
	}
}
