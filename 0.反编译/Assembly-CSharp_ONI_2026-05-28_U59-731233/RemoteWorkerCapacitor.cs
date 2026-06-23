using KSerialization;
using UnityEngine;

public class RemoteWorkerCapacitor : StateMachineComponent<RemoteWorkerCapacitor.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, RemoteWorkerCapacitor, object>.GameInstance
	{
		public StatesInstance(RemoteWorkerCapacitor master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, RemoteWorkerCapacitor>
	{
		private State ok;

		private State low_power;

		private State out_of_power;

		public override void InitializeStates(out BaseState default_state)
		{
			base.InitializeStates(out default_state);
			default_state = ok;
			root.ToggleStatusItem(Db.Get().DuplicantStatusItems.RemoteWorkerCapacitorStatus, (StatesInstance smi) => smi.master);
			ok.Transition(out_of_power, IsOutOfPower).Transition(low_power, IsLowPower);
			low_power.Transition(out_of_power, IsOutOfPower).Transition(ok, IsOkForPower).ToggleStatusItem(Db.Get().DuplicantStatusItems.RemoteWorkerLowPower);
			out_of_power.Transition(low_power, IsLowPower).Transition(ok, IsOkForPower).ToggleStatusItem(Db.Get().DuplicantStatusItems.RemoteWorkerOutOfPower);
		}

		public static bool IsOkForPower(StatesInstance smi)
		{
			return !smi.master.IsLowPower;
		}

		public static bool IsLowPower(StatesInstance smi)
		{
			return smi.master.IsLowPower && !smi.master.IsOutOfPower;
		}

		public static bool IsOutOfPower(StatesInstance smi)
		{
			return smi.master.IsOutOfPower;
		}
	}

	[Serialize]
	private float charge = 0f;

	public const float LOW_LEVEL = 12f;

	public const float POWER_USE_RATE_J_PER_S = -0.1f;

	public const float POWER_CHARGE_RATE_J_PER_S = 7.5f;

	public const float CAPACITY_J = 60f;

	public float ChargeRatio => charge / 60f;

	public float Charge => charge;

	public bool IsLowPower => charge < 12f;

	public bool IsOutOfPower => charge < float.Epsilon;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}

	public float ApplyDeltaEnergy(float delta)
	{
		float num = charge;
		charge = Mathf.Clamp(charge + delta, 0f, 60f);
		return charge - num;
	}
}
