using System.Collections.Generic;

public class ClusterGridOneTimeResourceSpawner : GameStateMachine<ClusterGridOneTimeResourceSpawner, ClusterGridOneTimeResourceSpawner.Instance, IStateMachineTarget, ClusterGridOneTimeResourceSpawner.Def>
{
	public struct Data
	{
		public Tag itemID;

		public float mass;
	}

	public class Def : BaseDef
	{
		public List<Data> thingsToSpawn;
	}

	public new class Instance : GameInstance
	{
		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}

		public void SpawnResources()
		{
			StarmapHexCellInventory hexCellInventory = GetHexCellInventory();
			foreach (Data item in base.def.thingsToSpawn)
			{
				hexCellInventory.AddItem(item.itemID, item.mass, Element.State.Vacuum).RecalculateState();
			}
			base.sm.HasSpawnedResources.Set(value: true, this);
		}

		public StarmapHexCellInventory GetHexCellInventory()
		{
			ClusterGridEntity component = GetComponent<ClusterGridEntity>();
			return ClusterGrid.Instance.AddOrGetHexCellInventory(component.Location);
		}
	}

	public State enter;

	public State spawning;

	public State spawned;

	public BoolParameter HasSpawnedResources;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = enter;
		enter.ParamTransition(HasSpawnedResources, spawned, GameStateMachine<ClusterGridOneTimeResourceSpawner, Instance, IStateMachineTarget, Def>.IsTrue).ParamTransition(HasSpawnedResources, spawning, GameStateMachine<ClusterGridOneTimeResourceSpawner, Instance, IStateMachineTarget, Def>.IsFalse);
		spawning.ParamTransition(HasSpawnedResources, spawned, GameStateMachine<ClusterGridOneTimeResourceSpawner, Instance, IStateMachineTarget, Def>.IsTrue).Enter(SpawnResources);
		spawned.DoNothing();
	}

	public static void SpawnResources(Instance smi)
	{
		smi.SpawnResources();
	}
}
