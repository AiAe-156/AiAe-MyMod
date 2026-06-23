using Klei.AI;

public class PressureMonitor : GameStateMachine<PressureMonitor, PressureMonitor.Instance, IStateMachineTarget, PressureMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public class PressureStates : State
	{
		public State idle;

		public State immune;
	}

	public new class Instance : GameInstance
	{
		private Effects effects;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			effects = GetComponent<Effects>();
		}

		public bool IsImmuneToHighPressure()
		{
			return effects.HasImmunityTo(Db.Get().effects.Get("PoppedEarDrums"));
		}

		public bool IsInHighPressure()
		{
			int cell = Grid.PosToCell(base.gameObject);
			for (int i = 0; i < PRESSURE_TEST_OFFSET.Length; i++)
			{
				int num = Grid.OffsetCell(cell, PRESSURE_TEST_OFFSET[i]);
				if (Grid.IsValidCell(num) && Grid.Element[num].IsGas && Grid.Mass[num] > 4f)
				{
					return true;
				}
			}
			return false;
		}

		public void RemoveEffect()
		{
			effects.Remove("PoppedEarDrums");
		}

		public void AddEffect()
		{
			effects.Add("PoppedEarDrums", should_save: true);
		}
	}

	public const string OVER_PRESSURE_EFFECT_NAME = "PoppedEarDrums";

	public const float TIME_IN_PRESSURE_BEFORE_EAR_POPS = 3f;

	private static CellOffset[] PRESSURE_TEST_OFFSET = new CellOffset[2]
	{
		new CellOffset(0, 0),
		new CellOffset(0, 1)
	};

	public State safe;

	public PressureStates inPressure;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = safe;
		safe.Transition(inPressure, IsInPressureGas);
		inPressure.Transition(safe, GameStateMachine<PressureMonitor, Instance, IStateMachineTarget, Def>.Not(IsInPressureGas)).DefaultState(inPressure.idle);
		inPressure.idle.EventTransition(GameHashes.EffectImmunityAdded, inPressure.immune, IsImmuneToPressure).Update(HighPressureUpdate);
		inPressure.immune.EventTransition(GameHashes.EffectImmunityRemoved, inPressure.idle, GameStateMachine<PressureMonitor, Instance, IStateMachineTarget, Def>.Not(IsImmuneToPressure));
	}

	public static bool IsInPressureGas(Instance smi)
	{
		return smi.IsInHighPressure();
	}

	public static bool IsImmuneToPressure(Instance smi)
	{
		return smi.IsImmuneToHighPressure();
	}

	public static void RemoveOverpressureEffect(Instance smi)
	{
		smi.RemoveEffect();
	}

	public static void HighPressureUpdate(Instance smi, float dt)
	{
		if (smi.timeinstate > 3f)
		{
			smi.AddEffect();
		}
	}
}
