using Klei.AI;

[SkipSaveFileSerialization]
public class StarryEyed : StateMachineComponent<StarryEyed.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, StarryEyed, object>.GameInstance
	{
		public StatesInstance(StarryEyed master)
			: base(master)
		{
		}

		public bool IsInSpace()
		{
			WorldContainer myWorld = this.GetMyWorld();
			if (!myWorld)
			{
				return false;
			}
			int parentWorldId = myWorld.ParentWorldId;
			int id = myWorld.id;
			if ((bool)myWorld.GetComponent<Clustercraft>() && parentWorldId == id)
			{
				return true;
			}
			return false;
		}
	}

	public class States : GameStateMachine<States, StatesInstance, StarryEyed>
	{
		public State idle;

		public State inSpace;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = idle;
			root.Enter(delegate(StatesInstance smi)
			{
				if (smi.IsInSpace())
				{
					smi.GoTo(inSpace);
				}
			});
			idle.EventTransition(GameHashes.MinionMigration, (StatesInstance smi) => Game.Instance, inSpace, (StatesInstance smi) => smi.IsInSpace()).Enter(delegate(StatesInstance smi)
			{
				Effects component = smi.master.gameObject.GetComponent<Effects>();
				if (component != null && component.HasEffect("StarryEyed"))
				{
					component.Remove("StarryEyed");
				}
			});
			inSpace.EventTransition(GameHashes.MinionMigration, (StatesInstance smi) => Game.Instance, idle, (StatesInstance smi) => !smi.IsInSpace()).ToggleEffect("StarryEyed");
		}
	}

	private const string STARRY_EYED_EFFECT_NAME = "StarryEyed";

	protected override void OnSpawn()
	{
		base.smi.StartSM();
	}
}
