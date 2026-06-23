using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class SeaTreeBranch : PlantBranchGrowerBase<SeaTreeBranch, SeaTreeBranch.Instance, IStateMachineTarget, SeaTreeBranch.Def>
{
	public class Def : PlantBranchGrowerBaseDef
	{
		public Tag SpawnCreatureID;

		public float GROWTH_RATE = 0.0016666667f;

		public float WILD_GROWTH_RATE = 0.00041666668f;
	}

	public class GrowingSpeedState : State
	{
		public State wild;

		public State domestic;
	}

	public class BranchAliveSubstate : PlantAliveSubState
	{
		public State InitializeStates(TargetParameter plant, TargetParameter root, State death_state, Signal dieSignal)
		{
			InitializeStates(plant, death_state);
			base.root.Target(plant).OnSignal(dieSignal, death_state).OnTargetLost(root, death_state)
				.Target(root)
				.EventHandler(GameHashes.Wilt, OnRootWilted)
				.EventHandler(GameHashes.WiltRecover, OnRootRecovered)
				.Target(plant);
			return this;
		}
	}

	public class GrowingStates : BranchAliveSubstate
	{
		public GrowingSpeedState growing;

		public State wilted;
	}

	public class FruitGrowingStates : State
	{
		public GrowingSpeedState growing;

		public State wilted;

		public State harvestReady;

		public State selfHarvestFromOld;

		public State spawnCritter;

		public State harvest;
	}

	public class GrownStates : BranchAliveSubstate
	{
		public FruitGrowingStates healthy;

		public State wilted;
	}

	public new class Instance : GameInstance, IManageGrowingStates, IWiltCause
	{
		public bool IsNewGameSpawned;

		public AttributeModifier baseGrowingRate;

		public AttributeModifier wildGrowingRate;

		public AttributeModifier baseFruitGrowingRate;

		public AttributeModifier wildFruitGrowingRate;

		public AttributeModifier getOldRate;

		public KBatchedAnimController animController;

		private AmountInstance maturity;

		private AmountInstance fruitMaturity;

		private AmountInstance oldAge;

		private WiltCondition wiltCondition;

		private SeaTreeRoot.Instance RootSMI;

		private Harvestable harvestable;

		private MeterController fruitMeter;

		private bool wasMarkedForDeadBeforeStartSM;

		public GameObject Root => base.sm.Root.Get(this);

		public GameObject Branch => base.sm.Branch.Get(this);

		public Instance BranchSMI
		{
			get
			{
				if (!(Branch == null))
				{
					return Branch.GetSMI<Instance>();
				}
				return null;
			}
		}

		public int MyBranchNumber => base.sm.BranchNumber.Get(this);

		public bool IsWild => base.sm.WildPlanted.Get(this);

		public bool MaxBranchNumberReached => MyBranchNumber >= 8;

		public bool IsOld => oldAge.value >= oldAge.GetMax();

		private bool IsRootWilting
		{
			get
			{
				if (RootSMI != null)
				{
					return RootSMI.IsWilting;
				}
				return false;
			}
		}

		public bool IsWilting
		{
			get
			{
				if (!wiltCondition.IsWilting())
				{
					return IsRootWilting;
				}
				return true;
			}
		}

		public bool IsGrown => GrowthPercentage >= 1f;

		public float GrowthPercentage => maturity.value / maturity.GetMax();

		public bool IsReadyForHarvest => FruitGrowthPercentage >= 1f;

		public float FruitGrowthPercentage => fruitMaturity.value / fruitMaturity.GetMax();

		public string WiltStateString => "    • " + DUPLICANTS.STATS.SEATREEROOTHEALTH.NAME;

		public WiltCondition.Condition[] Conditions => new WiltCondition.Condition[1] { WiltCondition.Condition.UnhealthyRoot };

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			Amounts amounts = base.gameObject.GetAmounts();
			maturity = amounts.Get(Db.Get().Amounts.Maturity);
			fruitMaturity = amounts.Get(Db.Get().Amounts.Maturity2);
			baseGrowingRate = new AttributeModifier(maturity.deltaAttribute.Id, def.GROWTH_RATE, CREATURES.STATS.MATURITY.GROWING);
			wildGrowingRate = new AttributeModifier(maturity.deltaAttribute.Id, def.WILD_GROWTH_RATE, CREATURES.STATS.MATURITY.GROWINGWILD);
			baseFruitGrowingRate = new AttributeModifier(fruitMaturity.deltaAttribute.Id, def.GROWTH_RATE, CREATURES.STATS.MATURITY.GROWING);
			wildFruitGrowingRate = new AttributeModifier(fruitMaturity.deltaAttribute.Id, def.WILD_GROWTH_RATE, CREATURES.STATS.MATURITY.GROWINGWILD);
			oldAge = amounts.Add(new AmountInstance(Db.Get().Amounts.OldAge, base.gameObject));
			oldAge.maxAttribute.ClearModifiers();
			oldAge.maxAttribute.Add(new AttributeModifier(Db.Get().Amounts.OldAge.maxAttribute.Id, 2400f));
			getOldRate = new AttributeModifier(oldAge.deltaAttribute.Id, 1f);
			wiltCondition = GetComponent<WiltCondition>();
			animController = GetComponent<KBatchedAnimController>();
			harvestable = GetComponent<Harvestable>();
			SetCellRegistrationAsPlant(doRegister: true);
			Subscribe(1119167081, OnSpawnedByDiscovery);
		}

		public override void StartSM()
		{
			wasMarkedForDeadBeforeStartSM = base.sm.MarkedForDeath.Get(this);
			base.master.gameObject.AddTag(GameTags.GrowingPlant);
			base.StartSM();
		}

		public override void PostParamsInitialized()
		{
			base.PostParamsInitialized();
			RootSMI = ((Root == null) ? null : Root.GetSMI<SeaTreeRoot.Instance>());
			if (wasMarkedForDeadBeforeStartSM)
			{
				base.sm.MarkedForDeath.Set(value: true, this);
			}
			HideAllFruitSymbols();
		}

		protected override void OnCleanUp()
		{
			DestroyFruitMeter();
			KillForwardBranch();
			SetCellRegistrationAsPlant(doRegister: false);
			base.OnCleanUp();
		}

		public void DestroySelf(object o)
		{
			CreatureHelpers.DeselectCreature(base.gameObject);
			Util.KDestroyGameObject(base.gameObject);
		}

		public void SetCellRegistrationAsPlant(bool doRegister)
		{
			int cell = Grid.PosToCell(this);
			if (doRegister && Grid.Objects[cell, 5] == null)
			{
				Grid.Objects[cell, 5] = base.gameObject;
			}
			else if (!doRegister && Grid.Objects[cell, 5] == base.gameObject)
			{
				Grid.Objects[cell, 5] = null;
			}
		}

		public void SetHarvestableState(bool canBeHarvested)
		{
			harvestable.SetCanBeHarvested(canBeHarvested);
		}

		public void SetAutoHarvestInChainReaction(bool autoharvest)
		{
			HarvestDesignatable component = GetComponent<HarvestDesignatable>();
			if (component != null)
			{
				component.SetHarvestWhenReady(autoharvest);
				if (BranchSMI != null)
				{
					BranchSMI.SetAutoHarvestInChainReaction(autoharvest);
				}
			}
		}

		public void ForceCancelHarvest()
		{
			harvestable.ForceCancelHarvest(true);
		}

		public void ResetOldAge()
		{
			oldAge.SetValue(0f);
		}

		private void OnSpawnedByDiscovery(object o)
		{
			float num = 1f - (float)MyBranchNumber / (float)base.def.MAX_BRANCH_COUNT;
			float num2 = ((UnityEngine.Random.Range(0f, 1f) <= num) ? 1f : UnityEngine.Random.Range(0f, 1f));
			maturity.SetValue(maturity.maxAttribute.GetTotalValue() * num2);
			if (IsGrown)
			{
				IsNewGameSpawned = true;
				fruitMaturity.SetValue(fruitMaturity.maxAttribute.GetTotalValue() * UnityEngine.Random.Range(0f, 1f));
			}
		}

		public void SpawnCritter(bool wasHarvestedByDupe)
		{
			GameObject gameObject = GetComponent<Crop>().SpawnAndGetConfiguredFruit(null, wasHarvestedByDupe);
			bool symbolVisible;
			Vector3 position = animController.GetSymbolTransform("bulb_meter_target", out symbolVisible).GetColumn(3);
			position.z = Grid.GetLayerZ(Grid.SceneLayer.Creatures);
			if (gameObject != null)
			{
				gameObject.transform.position = position;
				gameObject.SetActive(value: true);
				gameObject.GetComponent<PrimaryElement>().Temperature = base.gameObject.GetComponent<PrimaryElement>().Temperature;
			}
			else
			{
				DebugUtil.LogErrorArgs(base.gameObject, "failed at spawning critter for sea tree branch");
			}
		}

		public void ResetFruitGrowProgress()
		{
			fruitMaturity.SetValue(0f);
		}

		public void HideAllFruitSymbols()
		{
			animController.SetSymbolVisiblity("bulb_meter_target", is_visible: false);
		}

		public void CreateFruitMeter()
		{
			DestroyFruitMeter();
			fruitMeter = new MeterController(animController, "bulb_meter_target", "bulb_meter_grow", Meter.Offset.NoChange, Grid.SceneLayer.Building);
			base.sm.Fruit.Set(fruitMeter.gameObject, this);
		}

		private void DestroyFruitMeter()
		{
			if (fruitMeter != null)
			{
				fruitMeter.Unlink();
				Util.KDestroyGameObject(fruitMeter.gameObject);
				fruitMeter = null;
				base.sm.Fruit.Set(null, this);
			}
		}

		public void PlayAnimOnFruitMeter(string animName, KAnim.PlayMode playMode)
		{
			if (fruitMeter != null)
			{
				fruitMeter.meterController.Play(animName, playMode);
			}
		}

		public void UpdateFruitGrowMeterPosition()
		{
			if (fruitMeter != null)
			{
				if (fruitMeter.meterController.currentAnim != "bulb_meter_grow")
				{
					PlayAnimOnFruitMeter("bulb_meter_grow", KAnim.PlayMode.Paused);
				}
				fruitMeter.SetPositionPercent(FruitGrowthPercentage);
			}
		}

		private void KillForwardBranch()
		{
			if (Branch != null)
			{
				Instance sMI = Branch.GetSMI<Instance>();
				if (sMI != null)
				{
					sMI.sm.DieSignal.Trigger(sMI);
					sMI.sm.MarkedForDeath.Set(value: true, sMI);
				}
				base.sm.Branch.Set(null, this);
			}
		}

		public void SetupRootInformation(SeaTreeRoot.Instance root)
		{
			base.sm.BranchNumber.Set(1, this);
			base.sm.WildPlanted.Set(root.IsWild, this);
			base.sm.Root.Set(root.gameObject, this);
			RootSMI = ((Root == null) ? null : Root.GetSMI<SeaTreeRoot.Instance>());
			HarvestDesignatable component = root.GetComponent<HarvestDesignatable>();
			GetComponent<HarvestDesignatable>().SetHarvestWhenReady(component.HarvestWhenReady);
		}

		public void SetupFromPreviousBranchInformation(Instance previous_branch)
		{
			base.sm.BranchNumber.Set(previous_branch.MyBranchNumber + 1, this);
			base.sm.WildPlanted.Set(previous_branch.IsWild, this);
			base.sm.Root.Set(previous_branch.Root, this);
			RootSMI = ((Root == null) ? null : Root.GetSMI<SeaTreeRoot.Instance>());
			HarvestDesignatable component = previous_branch.GetComponent<HarvestDesignatable>();
			GetComponent<HarvestDesignatable>().SetHarvestWhenReady(component.HarvestWhenReady);
		}

		public void AttemptToSpawnBranch()
		{
			if (CanSpawnBranch())
			{
				int cellToSpawnBranch = GetCellToSpawnBranch();
				GameObject gameObject = SpawnBranchOnCell(cellToSpawnBranch);
				base.sm.Branch.Set(gameObject, this);
				if (IsNewGameSpawned)
				{
					gameObject.Trigger(1119167081);
				}
			}
			if (IsNewGameSpawned)
			{
				IsNewGameSpawned = false;
			}
		}

		private GameObject SpawnBranchOnCell(int cell)
		{
			Vector3 position = Grid.CellToPosCBC(cell, Grid.SceneLayer.BuildingFront);
			GameObject obj = Util.KInstantiate(Assets.GetPrefab(base.def.BRANCH_PREFAB_NAME), position);
			obj.SetActive(value: true);
			obj.GetSMI<Instance>().SetupFromPreviousBranchInformation(this);
			return obj;
		}

		private bool IsCellAvailable(int cell)
		{
			bool flag = CanGrowOnCell(base.gameObject, cell);
			if (flag && IsNewGameSpawned)
			{
				flag = SaveGame.Instance.worldGenSpawner.GetSpawnableInCell(cell) == null;
			}
			return flag;
		}

		public bool CanSpawnBranch()
		{
			bool flag = Branch == null && !MaxBranchNumberReached && IsGrown;
			if (flag)
			{
				int cellToSpawnBranch = GetCellToSpawnBranch();
				flag = flag && cellToSpawnBranch != Grid.InvalidCell && IsCellAvailable(cellToSpawnBranch);
			}
			return flag;
		}

		public int GetCellToSpawnBranch()
		{
			return Grid.OffsetCell(Grid.PosToCell(base.gameObject), 0, 1);
		}

		public float TimeUntilNextHarvest()
		{
			float num = ((maturity.GetDelta() <= 0f) ? 0f : ((maturity.GetMax() - maturity.value) / maturity.GetDelta()));
			float num2 = ((fruitMaturity.GetDelta() <= 0f) ? 0f : ((fruitMaturity.GetMax() - fruitMaturity.value) / fruitMaturity.GetDelta()));
			return num + num2;
		}

		public float GetCurrentGrowthPercentage()
		{
			if (!IsGrown)
			{
				return GrowthPercentage;
			}
			return FruitGrowthPercentage;
		}

		public float PercentGrown()
		{
			return GetCurrentGrowthPercentage();
		}

		public Crop GetCropComponent()
		{
			return GetComponent<Crop>();
		}

		public float DomesticGrowthTime()
		{
			return maturity.GetMax() / baseGrowingRate.Value;
		}

		public float WildGrowthTime()
		{
			return maturity.GetMax() / wildGrowingRate.Value;
		}

		public void OverrideMaturityLevel(float percent)
		{
			float value = maturity.GetMax() * percent;
			maturity.SetValue(value);
		}

		public bool IsWildPlanted()
		{
			return IsWild;
		}
	}

	public const string ANIM_PREFIX_COMMON_BRANCH = "branch_";

	public const string ANIM_PREFIX_END_BRANCH = "end_branch_";

	public const string ANIM_NAME_WILT_PREFIX = "wilted";

	public const string ANIM_NAME_GROWING = "grow";

	public const string ANIM_NAME_IDLE = "idle";

	public const string METER_TARGET_NAME = "bulb_meter_target";

	public const string METER_ANIM_NAME_WILT_PREFIX = "bulb_meter_wilt";

	public const string METER_ANIM_NAME_BIRTH = "bulb_meter_birth";

	public const string METER_ANIM_NAME_HARVEST = "bulb_meter_birth";

	public const string METER_ANIM_NAME_READY = "bulb_meter_ready";

	public const string METER_ANIM_NAME_GROWING = "bulb_meter_grow";

	public const string METER_DEFAULT_ANIM_NAME = "bulb_meter_grow";

	private const int WILT_LEVELS = 3;

	private static Dictionary<string, string[]> m_wilt = new Dictionary<string, string[]>();

	public TargetParameter Fruit;

	public TargetParameter Root;

	public TargetParameter Branch;

	public IntParameter BranchNumber;

	public BoolParameter WildPlanted;

	public BoolParameter MarkedForDeath;

	public Signal DieSignal;

	public State earlyDeathHandler;

	public GrowingStates undevelopedBranch;

	public GrownStates mature;

	public State dead;

	private static string GET_ANIM_NAME(bool isEndBranch, string animBaseName)
	{
		return (isEndBranch ? "end_branch_" : "branch_") + animBaseName;
	}

	private static string GetWiltAnimLevel(string baseSTR, float growingPercentage)
	{
		int num = ((growingPercentage < 0.75f) ? 1 : ((!(growingPercentage < 1f)) ? 3 : 2));
		if (baseSTR == null)
		{
			return null;
		}
		if (!m_wilt.ContainsKey(baseSTR))
		{
			m_wilt[baseSTR] = new string[3];
			for (int i = 0; i < 3; i++)
			{
				m_wilt[baseSTR][i] = baseSTR + (i + 1);
			}
		}
		return m_wilt[baseSTR][num - 1];
	}

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = earlyDeathHandler;
		earlyDeathHandler.ParamTransition(MarkedForDeath, dead, GameStateMachine<SeaTreeBranch, Instance, IStateMachineTarget, Def>.IsTrue).ParamTransition(Root, dead, GameStateMachine<SeaTreeBranch, Instance, IStateMachineTarget, Def>.IsNull).GoTo(undevelopedBranch);
		undevelopedBranch.InitializeStates(masterTarget, Root, dead, DieSignal).ParamTransition(MarkedForDeath, dead, GameStateMachine<SeaTreeBranch, Instance, IStateMachineTarget, Def>.IsTrue).ParamTransition(Root, dead, GameStateMachine<SeaTreeBranch, Instance, IStateMachineTarget, Def>.IsNull)
			.EventTransition(GameHashes.Grow, mature, (Instance smi) => smi.IsGrown)
			.UpdateTransition(mature, (Instance smi, float dt) => smi.IsGrown, UpdateRate.SIM_4000ms)
			.DefaultState(undevelopedBranch.growing);
		undevelopedBranch.wilted.PlayAnim((Func<Instance, string>)GetWiltAnim, KAnim.PlayMode.Loop).EventTransition(GameHashes.WiltRecover, undevelopedBranch.growing, (Instance smi) => !smi.IsWilting);
		undevelopedBranch.growing.EventTransition(GameHashes.Wilt, undevelopedBranch.wilted, (Instance smi) => smi.IsWilting).PlayAnim((Instance smi) => GetAnimName(smi, "grow"), KAnim.PlayMode.Paused).ToggleStatusItem(Db.Get().CreatureStatusItems.Growing, (Instance smi) => smi)
			.Enter(RefreshPositionPercent)
			.Update(RefreshPositionPercent, UpdateRate.SIM_4000ms)
			.EventHandler(GameHashes.ConsumePlant, RefreshPositionPercent)
			.DefaultState(undevelopedBranch.growing.wild);
		undevelopedBranch.growing.wild.ParamTransition(WildPlanted, undevelopedBranch.growing.domestic, GameStateMachine<SeaTreeBranch, Instance, IStateMachineTarget, Def>.IsFalse).ToggleAttributeModifier("Growing", (Instance smi) => smi.wildGrowingRate);
		undevelopedBranch.growing.domestic.ParamTransition(WildPlanted, undevelopedBranch.growing.wild, GameStateMachine<SeaTreeBranch, Instance, IStateMachineTarget, Def>.IsTrue).ToggleAttributeModifier("Growing", (Instance smi) => smi.baseGrowingRate);
		mature.InitializeStates(masterTarget, Root, dead, DieSignal).ParamTransition(MarkedForDeath, dead, GameStateMachine<SeaTreeBranch, Instance, IStateMachineTarget, Def>.IsTrue).ParamTransition(Root, dead, GameStateMachine<SeaTreeBranch, Instance, IStateMachineTarget, Def>.IsNull)
			.Enter(SpawnBrancheIfSpawnedByDiscovery)
			.Enter(SetupFruitMeter)
			.Update(SpawnBranchIfPossible, UpdateRate.SIM_4000ms)
			.DefaultState(mature.healthy);
		mature.healthy.PlayAnim((Instance smi) => GetAnimName(smi, "idle"), KAnim.PlayMode.Loop).DefaultState(mature.healthy.growing);
		mature.healthy.growing.EventTransition(GameHashes.Grow, mature.healthy.harvestReady, (Instance smi) => smi.IsReadyForHarvest).UpdateTransition(mature.healthy.harvestReady, (Instance smi, float dt) => smi.IsReadyForHarvest, UpdateRate.SIM_4000ms).EventTransition(GameHashes.Wilt, mature.wilted, (Instance smi) => smi.IsWilting)
			.ToggleStatusItem(Db.Get().CreatureStatusItems.GrowingFruit, (Instance smi) => smi)
			.Enter(UpdateFruitMeterGrowAnimations)
			.Update(UpdateFruitMeterGrowAnimations)
			.DefaultState(mature.healthy.growing.wild);
		mature.healthy.growing.wild.ParamTransition(WildPlanted, mature.healthy.growing.domestic, GameStateMachine<SeaTreeBranch, Instance, IStateMachineTarget, Def>.IsFalse).ToggleAttributeModifier("Fruit Growing", (Instance smi) => smi.wildFruitGrowingRate);
		mature.healthy.growing.domestic.ParamTransition(WildPlanted, mature.healthy.growing.wild, GameStateMachine<SeaTreeBranch, Instance, IStateMachineTarget, Def>.IsTrue).ToggleAttributeModifier("Fruit Growing", (Instance smi) => smi.baseFruitGrowingRate);
		mature.healthy.harvestReady.ToggleTag(GameTags.FullyGrown).EventTransition(GameHashes.Harvest, mature.healthy.harvest).Enter(MakeItHarvestable)
			.Enter(delegate(Instance smi)
			{
				PlayAnimsOnFruit(smi, "bulb_meter_ready", KAnim.PlayMode.Loop);
			})
			.ToggleAttributeModifier("GetOld", (Instance smi) => smi.getOldRate)
			.UpdateTransition(mature.healthy.selfHarvestFromOld, ShouldSelfHarvestFromOldAge, UpdateRate.SIM_4000ms)
			.Exit(ResetOldAge);
		mature.healthy.harvest.Target(Fruit).OnAnimQueueComplete(mature.healthy.growing).Target(masterTarget)
			.Enter(delegate(Instance smi)
			{
				PlayAnimsOnFruit(smi, "bulb_meter_birth", KAnim.PlayMode.Once);
			})
			.Enter(SpawnCritterHarvested)
			.Enter(MakeItNotHarvestable)
			.Enter(ResetFruitGrowProgress)
			.TriggerOnExit(GameHashes.HarvestComplete)
			.ScheduleGoTo(3f, mature.healthy.growing);
		mature.healthy.selfHarvestFromOld.Target(Fruit).OnAnimQueueComplete(mature.healthy.spawnCritter).Target(masterTarget)
			.Enter(delegate(Instance smi)
			{
				PlayAnimsOnFruit(smi, "bulb_meter_birth", KAnim.PlayMode.Once);
			})
			.Enter(ForceCancelHarvest)
			.Enter(MakeItNotHarvestable)
			.Enter(ResetOldAge)
			.Enter(ResetFruitGrowProgress)
			.TriggerOnExit(GameHashes.HarvestComplete)
			.ScheduleGoTo(3f, mature.healthy.spawnCritter);
		mature.healthy.spawnCritter.Enter(SpawnCritter).EnterGoTo(mature.healthy.growing);
		mature.wilted.PlayAnim((Func<Instance, string>)GetWiltAnim, KAnim.PlayMode.Loop).Enter(delegate(Instance smi)
		{
			PlayAnimsOnFruit(smi, GetFruitWiltAnim(smi), KAnim.PlayMode.Loop);
		}).EventTransition(GameHashes.WiltRecover, mature.healthy, (Instance smi) => !smi.IsWilting)
			.EventTransition(GameHashes.Harvest, mature.healthy.harvest);
		dead.Target(masterTarget).ToggleMainStatusItem(Db.Get().CreatureStatusItems.Dead).Enter(HarvestOnDeath)
			.Enter(delegate(Instance smi)
			{
				if (!smi.gameObject.GetComponent<KPrefabID>().HasTag(GameTags.Uprooted) && !smi.IsWild)
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

	private static bool ShouldSelfHarvestFromOldAge(Instance smi, float dt)
	{
		return smi.IsOld;
	}

	private static string GetWiltAnim(Instance smi)
	{
		return GetWiltAnimLevel(GetAnimName(smi, "wilted"), smi.GrowthPercentage);
	}

	private static string GetFruitWiltAnim(Instance smi)
	{
		return GetWiltAnimLevel("bulb_meter_wilt", smi.FruitGrowthPercentage);
	}

	private static void PlayAnimsOnFruit(Instance smi, string animName, KAnim.PlayMode playmode)
	{
		smi.PlayAnimOnFruitMeter(animName, playmode);
	}

	private static void UpdateFruitMeterGrowAnimations(Instance smi, float dt)
	{
		UpdateFruitMeterGrowAnimations(smi);
	}

	private static void UpdateFruitMeterGrowAnimations(Instance smi)
	{
		smi.UpdateFruitGrowMeterPosition();
	}

	private static void SetupFruitMeter(Instance smi)
	{
		smi.CreateFruitMeter();
	}

	private static void SpawnBranchIfPossible(Instance smi, float dt)
	{
		smi.AttemptToSpawnBranch();
	}

	private static void MakeItHarvestable(Instance smi)
	{
		smi.SetHarvestableState(canBeHarvested: true);
	}

	private static void ForceCancelHarvest(Instance smi)
	{
		smi.ForceCancelHarvest();
	}

	private static void MakeItNotHarvestable(Instance smi)
	{
		smi.SetHarvestableState(canBeHarvested: false);
	}

	private static void RefreshPositionPercent(Instance smi, float dt)
	{
		RefreshPositionPercent(smi);
	}

	private static void RefreshPositionPercent(Instance smi)
	{
		smi.animController.SetPositionPercent(smi.GrowthPercentage);
	}

	private static void ResetFruitGrowProgress(Instance smi)
	{
		smi.ResetFruitGrowProgress();
	}

	private static void ResetOldAge(Instance smi)
	{
		smi.ResetOldAge();
	}

	private static void SpawnCritterHarvested(Instance smi)
	{
		smi.SpawnCritter(wasHarvestedByDupe: true);
	}

	private static void SpawnCritter(Instance smi)
	{
		smi.SpawnCritter(wasHarvestedByDupe: false);
	}

	private static void OnRootRecovered(Instance smi)
	{
		smi.BoxingTrigger(912965142, data: true);
	}

	private static void OnRootWilted(Instance smi)
	{
		smi.BoxingTrigger(912965142, data: false);
	}

	public static string GetAnimName(Instance smi, string animName)
	{
		return GET_ANIM_NAME(smi.MaxBranchNumberReached, animName);
	}

	private static void SpawnBrancheIfSpawnedByDiscovery(Instance smi)
	{
		if (smi.IsNewGameSpawned)
		{
			SpawnBranchIfPossible(smi, 0f);
		}
	}

	public static void HarvestOnDeath(Instance smi)
	{
		_ = smi.IsReadyForHarvest;
	}

	private static Notification CreateDeathNotification(Instance smi)
	{
		return new Notification(CREATURES.STATUSITEMS.PLANTDEATH.NOTIFICATION, NotificationType.Bad, (List<Notification> notificationList, object data) => string.Concat(CREATURES.STATUSITEMS.PLANTDEATH.NOTIFICATION_TOOLTIP, notificationList.ReduceMessages(countNames: false)), "/t• " + smi.gameObject.GetProperName());
	}

	public static bool CanGrowOnCell(GameObject questionerObj, int cell)
	{
		int num = Grid.PosToCell(questionerObj);
		int num2 = Grid.WorldIdx[num];
		if (cell != Grid.InvalidCell && Grid.WorldIdx[cell] == num2 && Grid.IsLiquid(cell) && Grid.Objects[cell, 1] == null)
		{
			return Grid.Objects[cell, 5] == null;
		}
		return false;
	}
}
