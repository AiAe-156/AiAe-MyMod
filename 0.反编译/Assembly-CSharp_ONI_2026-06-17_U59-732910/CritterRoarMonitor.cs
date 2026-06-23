using UnityEngine;

public class CritterRoarMonitor : GameStateMachine<CritterRoarMonitor, CritterRoarMonitor.Instance, IStateMachineTarget, CritterRoarMonitor.Def>
{
	public class Def : BaseDef
	{
		public float SecondsPerRoarMax { get; private set; }

		public float Cooldown { get; private set; }

		public void Initialize(int roarsPerCycle, float cooldown)
		{
			SecondsPerRoarMax = 600f / (float)roarsPerCycle;
			Cooldown = cooldown;
		}
	}

	public new class Instance : GameInstance
	{
		private readonly float maxWait;

		private float wait;

		public Def Def { get; private set; }

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			Def = def;
			wait = Def.SecondsPerRoarMax;
			DebugUtil.DevAssert(Def.SecondsPerRoarMax >= Def.Cooldown, "Cooldown is so long so as to prevent us from achieving desired roars per cycle.");
			maxWait = Def.SecondsPerRoarMax - Def.Cooldown;
		}

		public float NextWaitDuration()
		{
			float num = Def.SecondsPerRoarMax - wait;
			wait = Random.Range(num, num + maxWait);
			return wait;
		}
	}

	public static Tag TAG = GameTags.Creatures.Behaviours.CritterRoarBehaviour;

	private readonly State wait;

	private readonly State roar;

	private readonly State cooldown;

	private static readonly Transition.ConditionCallback ALWAYS_TRUE = (Instance smi) => true;

	public override void InitializeStates(out BaseState defaultState)
	{
		defaultState = wait;
		wait.ScheduleGoTo((Instance smi) => smi.NextWaitDuration(), roar);
		roar.ToggleBehaviour(TAG, ALWAYS_TRUE, delegate(Instance smi)
		{
			smi.GoTo(cooldown);
		});
		cooldown.ScheduleGoTo((Instance smi) => smi.Def.Cooldown, wait);
	}
}
