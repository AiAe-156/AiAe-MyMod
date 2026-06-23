public class FishOvercrowdingMonitor : GameStateMachine<FishOvercrowdingMonitor, FishOvercrowdingMonitor.Instance, IStateMachineTarget, FishOvercrowdingMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		[MyCmpReq]
		private readonly KPrefabID prefabID;

		public KPrefabID PrefabID => prefabID;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}
	}

	private readonly State satisfied;

	private readonly State overcrowded;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = satisfied;
		root.Enter(Register).Exit(Unregister);
		satisfied.DoNothing();
		overcrowded.DoNothing();
	}

	private static void Register(Instance smi)
	{
		FishOvercrowingManager instance = FishOvercrowingManager.Instance;
		if (!(instance == null))
		{
			instance.Add(smi.PrefabID);
		}
	}

	private static void Unregister(Instance smi)
	{
		FishOvercrowingManager instance = FishOvercrowingManager.Instance;
		if (!(instance == null))
		{
			instance.Remove(smi.PrefabID);
		}
	}
}
