using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class SeaTreeRoot : PlantBranchGrowerBase<SeaTreeRoot, SeaTreeRoot.Instance, IStateMachineTarget, SeaTreeRoot.Def>
{
	public class Def : PlantBranchGrowerBaseDef
	{
	}

	public class GrowingBranchesStates : State
	{
		public State growing;

		public State blocked;
	}

	public class GrownStates : PlantAliveSubState
	{
		public GrowingBranchesStates growingBranches;

		public State idle;

		public State wilt;
	}

	public class GrowingStates : PlantAliveSubState
	{
		public State growing;

		public State growing_pst;
	}

	public new class Instance : GameInstance
	{
		public bool IsNewGameSpawned = false;

		private Growing growing;

		private ReceptacleMonitor receptacleMonitor;

		private WiltCondition wiltCondition;

		public GameObject Branch => base.sm.Branch.Get(this);

		public bool HasABranch => Branch != null;

		public bool IsGrown => growing.IsGrown();

		public bool IsWild => !receptacleMonitor.Replanted;

		public bool IsOnPlanterBox => !IsWild && receptacleMonitor.smi.ReceptacleObject != null && receptacleMonitor.smi.ReceptacleObject is PlantablePlot && (receptacleMonitor.smi.ReceptacleObject as PlantablePlot).IsOffGround;

		public int PlanterboxCell => IsWild ? Grid.InvalidCell : Grid.PosToCell(receptacleMonitor.smi.ReceptacleObject);

		public bool IsWilting => wiltCondition.IsWilting();

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			growing = GetComponent<Growing>();
			receptacleMonitor = GetComponent<ReceptacleMonitor>();
			wiltCondition = GetComponent<WiltCondition>();
			Subscribe(1119167081, OnSpawnedByDiscovered);
			Subscribe(-266953818, delegate
			{
				UpdateAutoHarvestValue();
			});
		}

		public void AttemptToSpawnBranches()
		{
			int cell = Grid.PosToCell(base.gameObject);
			if (Branch == null)
			{
				int cell2 = Grid.OffsetCell(cell, new CellOffset(0, 2));
				if (SeaTreeBranch.CanGrowOnCell(base.gameObject, cell2))
				{
					GameObject gameObject = SpawnBranchOnCell(cell2);
					base.sm.Branch.Set(gameObject, this);
					if (IsNewGameSpawned)
					{
						gameObject.Trigger(1119167081);
					}
				}
			}
			if (IsNewGameSpawned)
			{
				IsNewGameSpawned = false;
			}
		}

		public void DestroySelf(object o)
		{
			CreatureHelpers.DeselectCreature(base.gameObject);
			Util.KDestroyGameObject(base.gameObject);
		}

		private void OnSpawnedByDiscovered(object o)
		{
			IsNewGameSpawned = true;
			MarkAsGrown(this);
		}

		private GameObject SpawnBranchOnCell(int cell)
		{
			Vector3 position = Grid.CellToPosCBC(cell, Grid.SceneLayer.BuildingFront);
			GameObject gameObject = Util.KInstantiate(Assets.GetPrefab(base.def.BRANCH_PREFAB_NAME), position);
			gameObject.SetActive(value: true);
			SeaTreeBranch.Instance sMI = gameObject.GetSMI<SeaTreeBranch.Instance>();
			sMI.SetupRootInformation(this);
			return gameObject;
		}

		public void UpdateAutoHarvestValue()
		{
			HarvestDesignatable component = GetComponent<HarvestDesignatable>();
			if (component != null && Branch != null)
			{
				Branch.GetSMI<SeaTreeBranch.Instance>()?.SetAutoHarvestInChainReaction(component.HarvestWhenReady);
			}
		}
	}

	private const string GROW_ANIM_NAME = "grow";

	private const string GROW_PST_ANIM_NAME = "grow_pst";

	private const string IDLE_ANIM_NAME = "idle_full";

	private const string WILT_ANIM_NAME = "wilt3";

	public State dead;

	public GrowingStates growing;

	public GrownStates grown;

	public BoolParameter IsGrown;

	public TargetParameter Branch;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = growing;
		growing.InitializeStates(masterTarget, dead).DefaultState(growing.growing);
		growing.growing.ParamTransition(IsGrown, grown, GameStateMachine<SeaTreeRoot, Instance, IStateMachineTarget, Def>.IsTrue).PlayAnim("grow", KAnim.PlayMode.Once).OnAnimQueueComplete(growing.growing_pst);
		growing.growing_pst.Enter(MarkAsGrown).PlayAnim("grow_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(grown);
		grown.InitializeStates(masterTarget, dead).DefaultState(grown.growingBranches);
		grown.growingBranches.EventTransition(GameHashes.Wilt, grown.wilt, (Instance smi) => smi.IsWilting).ParamTransition(Branch, grown.idle, (Instance smi, GameObject b) => HasGrownBranch(smi)).PlayAnim("idle_full", KAnim.PlayMode.Loop)
			.Enter(SpawnBranchIfNewGameSpawn)
			.Update(AttemptToSpawnBranch, UpdateRate.SIM_4000ms)
			.DefaultState(grown.growingBranches.growing);
		grown.growingBranches.growing.ParamTransition(Branch, grown.growingBranches.blocked, (Instance smi, GameObject b) => HasNoBranch(smi));
		grown.growingBranches.blocked.ParamTransition(Branch, grown.growingBranches.growing, GameStateMachine<SeaTreeRoot, Instance, IStateMachineTarget, Def>.IsNotNull);
		grown.idle.EventTransition(GameHashes.Wilt, grown.wilt, (Instance smi) => smi.IsWilting).ParamTransition(Branch, grown.growingBranches, GameStateMachine<SeaTreeRoot, Instance, IStateMachineTarget, Def>.IsNull).PlayAnim("idle_full", KAnim.PlayMode.Loop);
		grown.wilt.EventTransition(GameHashes.WiltRecover, grown.idle, (Instance smi) => !smi.IsWilting).PlayAnim("wilt3", KAnim.PlayMode.Loop);
		dead.ToggleMainStatusItem(Db.Get().CreatureStatusItems.Dead).Enter(delegate(Instance smi)
		{
			if (!smi.IsWild && !smi.GetComponent<KPrefabID>().HasTag(GameTags.Uprooted))
			{
				Notifier notifier = smi.gameObject.AddOrGet<Notifier>();
				Notification notification = CreateDeathNotification(smi);
				notifier.Add(notification);
			}
			GameUtil.KInstantiate(Assets.GetPrefab(EffectConfigs.PlantDeathId), smi.transform.GetPosition(), Grid.SceneLayer.FXFront).SetActive(value: true);
			smi.Trigger(1623392196);
			smi.DestroySelf(null);
		});
	}

	private static void MarkAsGrown(Instance smi)
	{
		smi.sm.IsGrown.Set(value: true, smi);
	}

	private static bool HasNoBranch(Instance smi)
	{
		return smi.Branch == null;
	}

	private static bool HasGrownBranch(Instance smi)
	{
		return smi.HasABranch;
	}

	private static void SpawnBranchIfNewGameSpawn(Instance smi)
	{
		if (smi.IsNewGameSpawned)
		{
			AttemptToSpawnBranches(smi);
		}
	}

	private static void AttemptToSpawnBranch(Instance smi, float dt)
	{
		AttemptToSpawnBranches(smi);
	}

	private static void AttemptToSpawnBranches(Instance smi)
	{
		smi.AttemptToSpawnBranches();
	}

	public static Notification CreateDeathNotification(Instance smi)
	{
		return new Notification(CREATURES.STATUSITEMS.PLANTDEATH.NOTIFICATION, NotificationType.Bad, (List<Notification> notificationList, object data) => string.Concat(CREATURES.STATUSITEMS.PLANTDEATH.NOTIFICATION_TOOLTIP, notificationList.ReduceMessages(countNames: false)), "/t• " + smi.gameObject.GetProperName());
	}
}
