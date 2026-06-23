using UnityEngine;

public class PokeMonitor : StateMachineComponent<PokeMonitor.Instance>
{
	public class States : GameStateMachine<States, Instance, PokeMonitor>
	{
		public TargetParameter target;

		public State noTarget;

		public State hasTarget;

		public override void InitializeStates(out BaseState default_state)
		{
			base.serializable = SerializeType.Never;
			default_state = noTarget;
			noTarget.ParamTransition(target, hasTarget, GameStateMachine<States, PokeMonitor.Instance, PokeMonitor, object>.IsNotNull);
			hasTarget.ParamTransition(target, noTarget, GameStateMachine<States, PokeMonitor.Instance, PokeMonitor, object>.IsNull).ToggleBehaviour(GameTags.Creatures.UrgeToPoke, (PokeMonitor.Instance smi) => true, ClearTarget);
		}
	}

	public class Instance : GameStateMachine<States, Instance, PokeMonitor, object>.GameInstance
	{
		public CellOffset[] TargetOffsets = new CellOffset[1]
		{
			new CellOffset(0, 0)
		};

		public GameObject Target => base.sm.target.Get(this);

		public Instance(PokeMonitor master)
			: base(master)
		{
		}

		public void InitiatePoke(GameObject target)
		{
			InitiatePoke(target, new CellOffset[1]
			{
				new CellOffset(0, 0)
			});
		}

		public void InitiatePoke(GameObject target, CellOffset[] pokeOffesets)
		{
			base.sm.target.Set(target, this);
			TargetOffsets = pokeOffesets;
		}

		public void AbortPoke()
		{
			base.sm.target.Set(null, this);
			TargetOffsets = new CellOffset[1]
			{
				new CellOffset(0, 0)
			};
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}

	private static void ClearTarget(Instance smi)
	{
		smi.AbortPoke();
	}
}
