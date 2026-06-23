using System;

public class CreatureDecorMonitor : GameStateMachine<CreatureDecorMonitor, CreatureDecorMonitor.Instance, IStateMachineTarget, CreatureDecorMonitor.Def>
{
	public class Def : BaseDef
	{
		public float DecorValueTreshold;
	}

	public new class Instance : GameInstance
	{
		public Action<float> OnHighDecorUpdate;

		public Action<float> OnLowDecorUpdate;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}
	}

	private const UpdateRate UPDATE_RATE = UpdateRate.SIM_4000ms;

	private State lowDecor;

	private State highDecor;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = lowDecor;
		lowDecor.UpdateTransition(highDecor, IsInHighDecor, UpdateRate.SIM_4000ms).Update(TriggerLowDecorUpdate, UpdateRate.SIM_4000ms).TriggerOnEnter(GameHashes.CreatureLowDecor);
		highDecor.UpdateTransition(lowDecor, IsInLowDecor, UpdateRate.SIM_4000ms).Update(TriggerHighDecorUpdate, UpdateRate.SIM_4000ms).TriggerOnEnter(GameHashes.CreatureHighDecor);
	}

	private static void TriggerHighDecorUpdate(Instance smi, float dt)
	{
		smi.OnHighDecorUpdate?.Invoke(dt);
	}

	private static void TriggerLowDecorUpdate(Instance smi, float dt)
	{
		smi.OnLowDecorUpdate?.Invoke(dt);
	}

	private static bool IsInHighDecor(Instance smi, float dt)
	{
		return Grid.Decor[Grid.PosToCell(smi)] >= smi.def.DecorValueTreshold;
	}

	private static bool IsInLowDecor(Instance smi, float dt)
	{
		return !IsInHighDecor(smi, dt);
	}
}
