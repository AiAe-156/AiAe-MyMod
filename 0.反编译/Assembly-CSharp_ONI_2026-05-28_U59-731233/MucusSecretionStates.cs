using UnityEngine;

public class MucusSecretionStates : GameStateMachine<MucusSecretionStates, MucusSecretionStates.Instance, IStateMachineTarget, MucusSecretionStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public Vector3 position;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.Behaviours.SecretingMucusBehavior);
		}
	}

	public State secretePre;

	public State behaviourComplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = secretePre;
		secretePre.QueueAnim("poop").Exit(Secrete).OnAnimQueueComplete(behaviourComplete);
		behaviourComplete.BehaviourComplete(GameTags.Creatures.Behaviours.SecretingMucusBehavior);
	}

	private static void Secrete(Instance smi)
	{
		smi.position = smi.transform.GetPosition();
		smi.GetSMI<MoistureMonitor.Instance>()?.ProduceLubricant();
	}
}
