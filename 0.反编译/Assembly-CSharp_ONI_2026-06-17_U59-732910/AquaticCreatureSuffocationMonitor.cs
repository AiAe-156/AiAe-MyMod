using UnityEngine;

public class AquaticCreatureSuffocationMonitor : GameStateMachine<AquaticCreatureSuffocationMonitor, AquaticCreatureSuffocationMonitor.Instance, IStateMachineTarget, AquaticCreatureSuffocationMonitor.Def>
{
	public class Def : BaseDef
	{
		public float DeathTimerDuration = 2400f;

		public float RecoveryModifier = 4f;
	}

	public new class Instance : GameInstance
	{
		private Pickupable pickupable;

		public float DeathTimerValue => base.sm.DeathTimer.Get(this);

		public float TimeUntilDeath => Mathf.Max(base.smi.def.DeathTimerDuration - DeathTimerValue, 0f);

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			pickupable = GetComponent<Pickupable>();
		}

		public bool CanBreath()
		{
			int cell = Grid.PosToCell(this);
			return !(pickupable.storage == null) || Grid.IsSubstantialLiquid(cell);
		}
	}

	public State safe;

	public State suffocating;

	public State die;

	public State dead;

	public FloatParameter DeathTimer;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = safe;
		root.TagTransition(GameTags.Dead, dead);
		safe.Transition(suffocating, IsSuffocating, UpdateRate.SIM_1000ms).Update(RecoveryDeathTimerUpdate);
		suffocating.ParamTransition(DeathTimer, die, CanNotHoldAnymore).Transition(safe, CanBreath, UpdateRate.SIM_1000ms).ToggleStatusItem(Db.Get().CreatureStatusItems.AquaticCreatureSuffocating)
			.Update(DeathTimerUpdate);
		die.Enter(Kill);
		dead.DoNothing();
	}

	public static bool IsSuffocating(Instance smi)
	{
		return !smi.CanBreath();
	}

	public static bool CanBreath(Instance smi)
	{
		return smi.CanBreath();
	}

	public static bool CanNotHoldAnymore(Instance smi, float deathTimerValue)
	{
		return deathTimerValue > smi.def.DeathTimerDuration;
	}

	public static void DeathTimerUpdate(Instance smi, float dt)
	{
		smi.sm.DeathTimer.Set(smi.DeathTimerValue + dt, smi);
	}

	public static void RecoveryDeathTimerUpdate(Instance smi, float dt)
	{
		if (smi.DeathTimerValue > 0f)
		{
			smi.sm.DeathTimer.Set(Mathf.Max(smi.DeathTimerValue - dt * smi.def.RecoveryModifier, 0f), smi);
		}
	}

	public static void Kill(Instance smi)
	{
		smi.gameObject.GetSMI<DeathMonitor.Instance>().Kill(Db.Get().Deaths.Suffocation);
	}
}
