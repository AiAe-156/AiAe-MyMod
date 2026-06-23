using Klei.AI;
using STRINGS;

public class LubricatedMovementMonitor : GameStateMachine<LubricatedMovementMonitor, LubricatedMovementMonitor.Instance, IStateMachineTarget, LubricatedMovementMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public AmountInstance moisture;

		public AttributeModifier movementMoistureModifier;

		public float movingDryRate = -5f / 6f;

		public Instance(IStateMachineTarget master)
			: base(master)
		{
			moisture = Db.Get().Amounts.Moisture.Lookup(base.gameObject);
			movementMoistureModifier = new AttributeModifier(moisture.amount.deltaAttribute.Id, movingDryRate, CREATURES.MODIFIERS.MOVEMENT_MOISTURE_LOSS.NAME);
		}
	}

	public State idle;

	public State moving;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = idle;
		idle.EnterTransition(moving, (Instance smi) => smi.GetComponent<Navigator>().IsMoving()).EventHandlerTransition(GameHashes.ObjectMovementStateChanged, moving, IsMoving);
		moving.ToggleAttributeModifier("DryingOutVeryFast", (Instance smi) => smi.movementMoistureModifier).EventHandlerTransition(GameHashes.ObjectMovementStateChanged, idle, (Instance smi, object data) => !IsMoving(smi, data));
	}

	private bool IsMoving(Instance smi, object data)
	{
		if (data is GameHashes gameHashes)
		{
			return gameHashes == GameHashes.ObjectMovementWakeUp;
		}
		return false;
	}
}
