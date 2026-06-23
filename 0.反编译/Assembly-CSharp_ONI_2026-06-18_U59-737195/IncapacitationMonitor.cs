using Klei.AI;
using UnityEngine;

public class IncapacitationMonitor : GameStateMachine<IncapacitationMonitor, IncapacitationMonitor.Instance>
{
	public new class Instance : GameInstance
	{
		public Instance(IStateMachineTarget master)
			: base(master)
		{
			Health component = master.GetComponent<Health>();
			if ((bool)component)
			{
				component.canBeIncapacitated = true;
			}
		}

		public void Bleed(float dt, Instance smi)
		{
			smi.sm.bleedOutStamina.Delta(dt * (0f - smi.sm.baseBleedOutSpeed.Get(smi)), smi);
		}

		public void RecoverBleedOutStamina(float dt, Instance smi)
		{
			smi.sm.bleedOutStamina.Delta(Mathf.Min(dt * smi.sm.baseStaminaRecoverSpeed.Get(smi), smi.sm.maxBleedOutStamina.Get(smi) - smi.sm.bleedOutStamina.Get(smi)), smi);
		}

		public float GetBleedLifeTime(Instance smi)
		{
			return Mathf.Floor(smi.sm.bleedOutStamina.Get(smi) / smi.sm.baseBleedOutSpeed.Get(smi));
		}

		public Death GetCauseOfIncapacitation()
		{
			Health component = GetComponent<Health>();
			if (component.CauseOfIncapacitation == GameTags.RadiationSicknessIncapacitation)
			{
				return Db.Get().Deaths.Radiation;
			}
			if (component.CauseOfIncapacitation == GameTags.HitPointsDepleted)
			{
				return Db.Get().Deaths.Slain;
			}
			return Db.Get().Deaths.Generic;
		}

		public void ApplyRecoverEffect()
		{
			base.smi.Get<Effects>().Add("NearDeathExperience", should_save: true);
		}
	}

	public State healthy;

	public State recovered;

	public State incapacitated;

	public State die;

	private FloatParameter bleedOutStamina = new FloatParameter(120f);

	private FloatParameter baseBleedOutSpeed = new FloatParameter(1f);

	private FloatParameter baseStaminaRecoverSpeed = new FloatParameter(1f);

	private FloatParameter maxBleedOutStamina = new FloatParameter(120f);

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = healthy;
		base.serializable = SerializeType.Both_DEPRECATED;
		healthy.Update(delegate(Instance smi, float dt)
		{
			smi.RecoverBleedOutStamina(dt, smi);
		}).EventTransition(GameHashes.BecameIncapacitated, incapacitated);
		incapacitated.EventTransition(GameHashes.IncapacitationRecovery, recovered).ToggleTag(GameTags.Incapacitated).ToggleRecurringChore((Instance smi) => new BeIncapacitatedChore(smi.master))
			.ParamTransition(bleedOutStamina, die, GameStateMachine<IncapacitationMonitor, Instance, IStateMachineTarget, object>.IsLTEZero)
			.ToggleUrge(Db.Get().Urges.BeIncapacitated)
			.Update(delegate(Instance smi, float dt)
			{
				smi.Bleed(dt, smi);
			});
		recovered.Enter(delegate(Instance smi)
		{
			smi.ApplyRecoverEffect();
		}).GoTo(healthy);
		die.Enter(delegate(Instance smi)
		{
			smi.master.gameObject.GetSMI<DeathMonitor.Instance>().Kill(smi.GetCauseOfIncapacitation());
		});
	}
}
