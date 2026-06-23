using Klei.AI;
using STRINGS;
using TUNING;

[SkipSaveFileSerialization]
public class PrefersColder : StateMachineComponent<PrefersColder.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, PrefersColder, object>.GameInstance
	{
		public StatesInstance(PrefersColder master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, PrefersColder>
	{
		private AttributeModifier modifier = new AttributeModifier("ThermalConductivityBarrier", DUPLICANTSTATS.STANDARD.Temperature.Conductivity_Barrier_Modification.PUDGY, DUPLICANTS.TRAITS.NEEDS.PREFERSCOOLER.NAME);

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = root;
			root.ToggleAttributeModifier(DUPLICANTS.TRAITS.NEEDS.PREFERSCOOLER.NAME, (StatesInstance smi) => modifier);
		}
	}

	protected override void OnSpawn()
	{
		base.smi.StartSM();
	}
}
