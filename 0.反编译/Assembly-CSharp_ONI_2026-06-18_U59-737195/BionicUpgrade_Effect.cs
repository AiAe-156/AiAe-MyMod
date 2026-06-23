using Klei.AI;

public class BionicUpgrade_Effect : GameStateMachine<BionicUpgrade_Effect, BionicUpgrade_Effect.Instance, IStateMachineTarget, BionicUpgrade_Effect.Def>
{
	public class Def : BaseDef
	{
		public string EFFECT_NAME;
	}

	public new class Instance : GameInstance
	{
		private Effects effects;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			effects = GetComponent<Effects>();
		}

		public void ApplyEffect()
		{
			Effect newEffect = Db.Get().effects.Get(base.def.EFFECT_NAME);
			effects.Add(newEffect, should_save: false);
		}

		public void RemoveEffect()
		{
			Effect effect = Db.Get().effects.Get(base.def.EFFECT_NAME);
			effects.Remove(effect);
		}
	}

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = root;
		root.Enter(EnableEffect).Exit(DisableEffect);
	}

	public static void EnableEffect(Instance smi)
	{
		smi.ApplyEffect();
	}

	public static void DisableEffect(Instance smi)
	{
		smi.RemoveEffect();
	}
}
