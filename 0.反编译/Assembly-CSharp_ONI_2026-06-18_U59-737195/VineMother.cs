using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class VineMother : PlantBranchGrowerBase<VineMother, VineMother.Instance, IStateMachineTarget, VineMother.Def>
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
		public bool IsNewGameSpawned;

		private Growing growing;

		private ReceptacleMonitor receptacleMonitor;

		private WiltCondition wiltCondition;

		public GameObject LeftBranch => base.sm.LeftBranch.Get(this);

		public GameObject RightBranch => base.sm.RightBranch.Get(this);

		public bool HasGrownAllBranches
		{
			get
			{
				if (LeftBranch != null)
				{
					return RightBranch != null;
				}
				return false;
			}
		}

		public bool IsGrown => growing.IsGrown();

		public bool IsWild => !receptacleMonitor.Replanted;

		public bool IsOnPlanterBox
		{
			get
			{
				if (!IsWild && receptacleMonitor.smi.ReceptacleObject != null && receptacleMonitor.smi.ReceptacleObject is PlantablePlot)
				{
					return (receptacleMonitor.smi.ReceptacleObject as PlantablePlot).IsOffGround;
				}
				return false;
			}
		}

		public int PlanterboxCell
		{
			get
			{
				if (!IsWild)
				{
					return Grid.PosToCell(receptacleMonitor.smi.ReceptacleObject);
				}
				return Grid.InvalidCell;
			}
		}

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
			if (LeftBranch == null)
			{
				int cell2 = Grid.OffsetCell(cell, CellOffset.left);
				if (VineBranch.IsCellAvailable(base.gameObject, cell2))
				{
					GameObject gameObject = SpawnBranchOnCell(cell2);
					base.sm.LeftBranch.Set(gameObject, this);
					if (IsNewGameSpawned)
					{
						gameObject.Trigger(1119167081);
					}
				}
			}
			if (RightBranch == null)
			{
				int cell3 = Grid.OffsetCell(cell, CellOffset.right);
				if (VineBranch.IsCellAvailable(base.gameObject, cell3))
				{
					GameObject gameObject2 = SpawnBranchOnCell(cell3);
					base.sm.RightBranch.Set(gameObject2, this);
					if (IsNewGameSpawned)
					{
						gameObject2.Trigger(1119167081);
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
			GameObject obj = Util.KInstantiate(Assets.GetPrefab(base.def.BRANCH_PREFAB_NAME), position);
			obj.SetActive(value: true);
			obj.GetSMI<VineBranch.Instance>().SetupRootInformation(this);
			return obj;
		}

		public void UpdateAutoHarvestValue()
		{
			HarvestDesignatable component = GetComponent<HarvestDesignatable>();
			if (component != null)
			{
				if (LeftBranch != null)
				{
					LeftBranch.GetSMI<VineBranch.Instance>()?.SetAutoHarvestInChainReaction(component.HarvestWhenReady);
				}
				if (RightBranch != null)
				{
					RightBranch.GetSMI<VineBranch.Instance>()?.SetAutoHarvestInChainReaction(component.HarvestWhenReady);
				}
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

	public TargetParameter LeftBranch;

	public TargetParameter RightBranch;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = growing;
		growing.InitializeStates(masterTarget, dead).DefaultState(growing.growing);
		growing.growing.ParamTransition(IsGrown, grown, GameStateMachine<VineMother, Instance, IStateMachineTarget, Def>.IsTrue).PlayAnim("grow", KAnim.PlayMode.Once).OnAnimQueueComplete(growing.growing_pst);
		growing.growing_pst.Enter(MarkAsGrown).PlayAnim("grow_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(grown);
		grown.InitializeStates(masterTarget, dead).DefaultState(grown.growingBranches);
		grown.growingBranches.EventTransition(GameHashes.Wilt, grown.wilt, (Instance smi) => smi.IsWilting).ParamTransition(LeftBranch, grown.idle, (Instance smi, GameObject b) => HasGrownAllBranches(smi)).ParamTransition(RightBranch, grown.idle, (Instance smi, GameObject b) => HasGrownAllBranches(smi))
			.PlayAnim("idle_full", KAnim.PlayMode.Loop)
			.Enter(SpawnBranchesIfNewGameSpawn)
			.Update(AttemptToSpawnBranches, UpdateRate.SIM_4000ms)
			.DefaultState(grown.growingBranches.growing);
		grown.growingBranches.growing.ParamTransition(LeftBranch, grown.growingBranches.blocked, (Instance smi, GameObject b) => HasNoBranches(smi)).ParamTransition(RightBranch, grown.growingBranches.blocked, (Instance smi, GameObject b) => HasNoBranches(smi));
		grown.growingBranches.blocked.ParamTransition(LeftBranch, grown.growingBranches.growing, GameStateMachine<VineMother, Instance, IStateMachineTarget, Def>.IsNotNull).ParamTransition(RightBranch, grown.growingBranches.growing, GameStateMachine<VineMother, Instance, IStateMachineTarget, Def>.IsNotNull);
		grown.idle.EventTransition(GameHashes.Wilt, grown.wilt, (Instance smi) => smi.IsWilting).ParamTransition(LeftBranch, grown.growingBranches, GameStateMachine<VineMother, Instance, IStateMachineTarget, Def>.IsNull).ParamTransition(RightBranch, grown.growingBranches, GameStateMachine<VineMother, Instance, IStateMachineTarget, Def>.IsNull)
			.PlayAnim("idle_full", KAnim.PlayMode.Loop);
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

	private static bool HasNoBranches(Instance smi)
	{
		if (smi.LeftBranch == null)
		{
			return smi.RightBranch == null;
		}
		return false;
	}

	private static bool HasGrownAllBranches(Instance smi)
	{
		return smi.HasGrownAllBranches;
	}

	private static void SpawnBranchesIfNewGameSpawn(Instance smi)
	{
		if (smi.IsNewGameSpawned)
		{
			AttemptToSpawnBranches(smi);
		}
	}

	private static void AttemptToSpawnBranches(Instance smi, float dt)
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
