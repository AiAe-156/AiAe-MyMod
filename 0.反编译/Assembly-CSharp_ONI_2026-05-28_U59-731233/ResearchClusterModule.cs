public class ResearchClusterModule : GameStateMachine<ResearchClusterModule, ResearchClusterModule.Instance, IStateMachineTarget, ResearchClusterModule.Def>
{
	public class Def : BaseDef
	{
	}

	public class InSpaceStates : State
	{
		public State idle;

		public State collecting;

		public State full;
	}

	public new class Instance : GameInstance
	{
		private Storage storage;

		private RocketModuleHexCellCollector.Instance collector;

		private Clustercraft clustercraft;

		public bool IsStorageFull => storage != null && storage.IsFull();

		public bool IsCollectingDatabanks => collector != null && collector.IsCollecting;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			storage = GetComponent<Storage>();
			collector = base.gameObject.GetSMI<RocketModuleHexCellCollector.Instance>();
		}

		public override void StartSM()
		{
			clustercraft = GetComponent<RocketModuleCluster>().CraftInterface.GetComponent<Clustercraft>();
			base.sm.ClusterCraft.Set(clustercraft.gameObject, this);
			base.StartSM();
		}

		public void DropInventory()
		{
			storage.DropAll();
		}
	}

	public State grounded;

	public InSpaceStates space;

	public TargetParameter ClusterCraft = null;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = grounded;
		root.EventHandler(GameHashes.RocketLanded, DropInventory);
		grounded.TagTransition(GameTags.RocketNotOnGround, space);
		space.TagTransition(GameTags.RocketNotOnGround, grounded, on_remove: true).DefaultState(space.idle);
		space.idle.EventHandlerTransition(GameHashes.OnStorageChange, space.full, IsStorageFull).Target(ClusterCraft).EventHandlerTransition(GameHashes.TagsChanged, space.collecting, IsCollectingDatabanks);
		space.collecting.EventHandlerTransition(GameHashes.OnStorageChange, space.full, IsStorageFull).Target(ClusterCraft).EventHandlerTransition(GameHashes.TagsChanged, space.collecting, IsNotCollectingDatabanks);
		space.full.EventHandlerTransition(GameHashes.OnStorageChange, space.idle, StorageIsNotFull);
	}

	public static void DropInventory(Instance smi)
	{
		smi.DropInventory();
	}

	public static bool IsNotCollectingDatabanks(Instance smi, object o)
	{
		return !smi.IsCollectingDatabanks;
	}

	public static bool IsCollectingDatabanks(Instance smi, object o)
	{
		return smi.IsCollectingDatabanks;
	}

	public static bool IsStorageFull(Instance smi, object o)
	{
		return smi.IsStorageFull;
	}

	public static bool StorageIsNotFull(Instance smi, object o)
	{
		return !smi.IsStorageFull;
	}
}
