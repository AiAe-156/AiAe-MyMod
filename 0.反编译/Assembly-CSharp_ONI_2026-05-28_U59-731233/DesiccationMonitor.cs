using Klei.AI;
using UnityEngine;

public class DesiccationMonitor : GameStateMachine<DesiccationMonitor, DesiccationMonitor.Instance, IStateMachineTarget, DesiccationMonitor.Def>
{
	public class Def : BaseDef
	{
		public float desiccationDamagePerSecond = 0.1f;
	}

	public new class Instance : GameInstance
	{
		public float originalSpeed;

		public Navigator navigator;

		public AmountInstance moisture;

		public Health health;

		private static readonly Color32 dryColorDiff = new Color32(45, 45, 45, 0);

		private KBatchedAnimController kbac;

		public void ApplySadLook()
		{
			kbac.TintColour = new Color32
			{
				r = (byte)Mathf.Clamp(kbac.TintColour.r - dryColorDiff.r, 0, 255),
				g = (byte)Mathf.Clamp(kbac.TintColour.g - dryColorDiff.g, 0, 255),
				b = (byte)Mathf.Clamp(kbac.TintColour.b - dryColorDiff.b, 0, 255),
				a = kbac.TintColour.a
			};
		}

		public void RemoveSadLook()
		{
			kbac.TintColour = new Color32
			{
				r = (byte)Mathf.Clamp(kbac.TintColour.r + dryColorDiff.r, 0, 255),
				g = (byte)Mathf.Clamp(kbac.TintColour.g + dryColorDiff.g, 0, 255),
				b = (byte)Mathf.Clamp(kbac.TintColour.b + dryColorDiff.b, 0, 255),
				a = kbac.TintColour.a
			};
		}

		public float GetEstimatedTimeUntilDeath()
		{
			return base.smi.IsInsideState(base.smi.sm.desiccating) ? (health.hitPoints / base.def.desiccationDamagePerSecond) : float.NaN;
		}

		public bool IsDesiccating()
		{
			return base.smi.IsInsideState(base.smi.sm.desiccating);
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			moisture = Db.Get().Amounts.Moisture.Lookup(base.gameObject);
			health = master.GetComponent<Health>();
			navigator = base.smi.GetComponent<Navigator>();
			kbac = master.GetComponent<KBatchedAnimController>();
			originalSpeed = navigator.defaultSpeed;
		}
	}

	private State wet;

	private State dry;

	private State desiccating;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = wet;
		wet.Enter(delegate(Instance smi)
		{
			SetSpeedModifier(smi, 1f);
		}).UpdateTransition(dry, Dry);
		dry.UpdateTransition(wet, NotDry, UpdateRate.SIM_1000ms).UpdateTransition(desiccating, IsCompletelyDry, UpdateRate.SIM_1000ms).ToggleTag(GameTags.Creatures.Dry)
			.Enter(delegate(Instance smi)
			{
				SetSpeedModifier(smi, 0.66f);
			});
		desiccating.Enter(delegate(Instance smi)
		{
			SetSpeedModifier(smi, 0.33f);
			smi.ApplySadLook();
		}).Exit(delegate(Instance smi)
		{
			smi.RemoveSadLook();
		}).UpdateTransition(wet, (Instance smi, float dt) => !IsCompletelyDry(smi, dt))
			.ToggleStatusItem(Db.Get().CreatureStatusItems.Desiccation, (Instance smi) => smi)
			.ToggleTag(GameTags.Creatures.Dry)
			.Update(CheckDying, UpdateRate.SIM_4000ms);
	}

	private static void CheckDying(Instance smi, float dt)
	{
		smi.health.Damage(smi.def.desiccationDamagePerSecond * dt);
		if (smi.health.IsDefeated())
		{
			smi.Trigger(1506153353);
		}
	}

	private static bool IsMoisturized(Instance smi, float moistureTreshold)
	{
		return smi.moisture.value > moistureTreshold;
	}

	private static bool NotDry(Instance smi, float _)
	{
		return IsMoisturized(smi, 30f);
	}

	private static bool Dry(Instance smi, float _)
	{
		return !IsMoisturized(smi, 30f);
	}

	private static bool IsCompletelyDry(Instance smi, float _)
	{
		return smi.moisture.value <= 0f;
	}

	private static void SetSpeedModifier(Instance smi, float amount)
	{
		smi.navigator.defaultSpeed = smi.originalSpeed * amount;
	}
}
