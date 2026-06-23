using Klei.AI;
using STRINGS;
using TUNING;

[SkipSaveFileSerialization]
public class PrefersWarmer : StateMachineComponent<PrefersWarmer.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, PrefersWarmer, object>.GameInstance
	{
		public StatesInstance(PrefersWarmer master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, PrefersWarmer>
	{
		private AttributeModifier modifier = new AttributeModifier("ThermalConductivityBarrier", DUPLICANTSTATS.STANDARD.Temperature.Conductivity_Barrier_Modification.SKINNY, DUPLICANTS.TRAITS.NEEDS.PREFERSWARMER.NAME);

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = root;
			root.ToggleAttributeModifier(DUPLICANTS.TRAITS.NEEDS.PREFERSWARMER.NAME, (StatesInstance smi) => modifier);
		}
	}

	protected override void OnSpawn()
	{
		base.smi.StartSM();
	}
}
