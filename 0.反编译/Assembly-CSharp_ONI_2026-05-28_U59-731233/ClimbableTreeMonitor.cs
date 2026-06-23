using UnityEngine;

public class ClimbableTreeMonitor : GameStateMachine<ClimbableTreeMonitor, ClimbableTreeMonitor.Instance, IStateMachineTarget, ClimbableTreeMonitor.Def>
{
	public class Def : BaseDef
	{
		public float searchMinInterval = 60f;

		public float searchMaxInterval = 120f;
	}

	public new class Instance : GameInstance
	{
		private struct FindClimableTreeContext
		{
			public Navigator navigator;

			public ListPool<KMonoBehaviour, ClimbableTreeMonitor>.PooledList targets;
		}

		public GameObject climbTarget;

		public float nextSearchTime;

		private static GameScenePartitioner.VisitorRef<FindClimableTreeContext> FindClimbableTreeVisitor = FindClimbableTree;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			RefreshSearchTime();
		}

		private void RefreshSearchTime()
		{
			nextSearchTime = Time.time + Mathf.Lerp(base.def.searchMinInterval, base.def.searchMaxInterval, Random.value);
		}

		public bool UpdateHasClimbable()
		{
			if (climbTarget == null)
			{
				if (Time.time < nextSearchTime)
				{
					return false;
				}
				FindClimbableTree();
				RefreshSearchTime();
			}
			return climbTarget != null;
		}

		private static Util.IterationInstruction FindClimbableTree(object obj, ref FindClimableTreeContext context)
		{
			KMonoBehaviour kMonoBehaviour = obj as KMonoBehaviour;
			if (kMonoBehaviour.HasTag(GameTags.Creatures.ReservedByCreature))
			{
				return Util.IterationInstruction.Continue;
			}
			int cell = Grid.PosToCell(kMonoBehaviour);
			if (!context.navigator.CanReach(cell))
			{
				return Util.IterationInstruction.Continue;
			}
			ForestTreeSeedMonitor component = kMonoBehaviour.GetComponent<ForestTreeSeedMonitor>();
			StorageLocker component2 = kMonoBehaviour.GetComponent<StorageLocker>();
			if (component != null)
			{
				if (!component.ExtraSeedAvailable)
				{
					return Util.IterationInstruction.Continue;
				}
			}
			else
			{
				if (!(component2 != null))
				{
					return Util.IterationInstruction.Continue;
				}
				Storage component3 = component2.GetComponent<Storage>();
				if (!component3.allowItemRemoval)
				{
					return Util.IterationInstruction.Continue;
				}
				if (component3.IsEmpty())
				{
					return Util.IterationInstruction.Continue;
				}
			}
			context.targets.Add(kMonoBehaviour);
			return Util.IterationInstruction.Continue;
		}

		private void FindClimbableTree()
		{
			climbTarget = null;
			Vector3 position = base.master.transform.GetPosition();
			Extents extents = new Extents(Grid.PosToCell(position), 10);
			FindClimableTreeContext context = default(FindClimableTreeContext);
			context.navigator = GetComponent<Navigator>();
			context.targets = ListPool<KMonoBehaviour, ClimbableTreeMonitor>.Allocate();
			GameScenePartitioner.Instance.ReadonlyVisitEntries(extents.x, extents.y, extents.width, extents.height, GameScenePartitioner.Instance.plants, FindClimbableTreeVisitor, ref context);
			GameScenePartitioner.Instance.ReadonlyVisitEntries(extents.x, extents.y, extents.width, extents.height, GameScenePartitioner.Instance.completeBuildings, FindClimbableTreeVisitor, ref context);
			if (context.targets.Count > 0)
			{
				int index = Random.Range(0, context.targets.Count);
				KMonoBehaviour kMonoBehaviour = context.targets[index];
				climbTarget = kMonoBehaviour.gameObject;
			}
			context.targets.Recycle();
		}

		public void OnClimbComplete()
		{
			climbTarget = null;
		}
	}

	private const int MAX_NAV_COST = int.MaxValue;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = root;
		root.ToggleBehaviour(GameTags.Creatures.WantsToClimbTree, (Instance smi) => smi.UpdateHasClimbable(), delegate(Instance smi)
		{
			smi.OnClimbComplete();
		});
	}
}
