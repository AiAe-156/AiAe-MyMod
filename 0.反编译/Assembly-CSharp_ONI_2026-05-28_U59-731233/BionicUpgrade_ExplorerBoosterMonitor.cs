using System.Collections.Generic;
using STRINGS;

public class BionicUpgrade_ExplorerBoosterMonitor : BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, BionicUpgrade_ExplorerBoosterMonitor.Instance>
{
	public new class Def : BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.Def
	{
		public Def(string upgradeID)
			: base(upgradeID)
		{
		}

		public override string GetDescription()
		{
			return "BionicUpgrade_ExplorerBoosterMonitor.Def description not implemented";
		}
	}

	public class ActiveStates : State
	{
		public State gatheringData;

		public State discover;
	}

	public new class Instance : BaseInstance
	{
		private BionicUpgrade_ExplorerBooster.Instance explorerBooster;

		public bool IsReadyToDiscover => explorerBooster != null && explorerBooster.IsReady;

		public float CurrentProgress => (explorerBooster == null) ? 0f : explorerBooster.Progress;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, (BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.Def)def)
		{
		}

		public void Initialize()
		{
			BionicUpgradesMonitor.Instance sMI = base.gameObject.GetSMI<BionicUpgradesMonitor.Instance>();
			BionicUpgradesMonitor.UpgradeComponentSlot[] upgradeComponentSlots = sMI.upgradeComponentSlots;
			foreach (BionicUpgradesMonitor.UpgradeComponentSlot upgradeComponentSlot in upgradeComponentSlots)
			{
				if (upgradeComponentSlot.HasUpgradeInstalled)
				{
					BionicUpgradeComponent installedUpgradeComponent = upgradeComponentSlot.installedUpgradeComponent;
					BionicUpgrade_ExplorerBooster.Instance sMI2 = installedUpgradeComponent.GetSMI<BionicUpgrade_ExplorerBooster.Instance>();
					if (sMI2 != null && !sMI2.IsBeingMonitored)
					{
						explorerBooster = sMI2;
						sMI2.SetMonitor(this);
						break;
					}
				}
			}
		}

		protected override void OnCleanUp()
		{
			if (explorerBooster != null)
			{
				explorerBooster.SetMonitor(null);
			}
			base.OnCleanUp();
		}

		public void GatheringDataUpdate(float dt)
		{
			bool isReadyToDiscover = IsReadyToDiscover;
			float dataProgressDelta = ((dt == 0f) ? 0f : (dt / 600f));
			explorerBooster.AddData(dataProgressDelta);
			if (IsReadyToDiscover && !isReadyToDiscover)
			{
				base.sm.ReadyToDiscoverSignal.Trigger(this);
			}
		}

		public void ConsumeAllData()
		{
			explorerBooster.SetDataProgress(0f);
		}

		public Notification GetGeyserDiscoveredNotification()
		{
			return new Notification(DUPLICANTS.STATUSITEMS.BIONICEXPLORERBOOSTER.NOTIFICATION_NAME, NotificationType.MessageImportant, (List<Notification> notificationList, object data) => string.Concat(DUPLICANTS.STATUSITEMS.BIONICEXPLORERBOOSTER.NOTIFICATION_TOOLTIP, notificationList.ReduceMessages(countNames: false)));
		}

		public override float GetCurrentWattageCost()
		{
			if (IsInsideState(base.sm.Active))
			{
				return base.Data.WattageCost;
			}
			return 0f;
		}

		public override string GetCurrentWattageCostName()
		{
			float currentWattageCost = GetCurrentWattageCost();
			if (IsInsideState(base.sm.Active))
			{
				return string.Format(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.STANDARD_ACTIVE_TEMPLATE, upgradeComponent.GetProperName(), GameUtil.GetFormattedWattage(currentWattageCost));
			}
			return string.Format(DUPLICANTS.MODIFIERS.BIONIC_WATTS.TOOLTIP.STANDARD_INACTIVE_TEMPLATE, upgradeComponent.GetProperName(), GameUtil.GetFormattedWattage(upgradeComponent.PotentialWattage));
		}
	}

	public State attachToBooster;

	public new ActiveStates Active;

	public Signal ReadyToDiscoverSignal;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = attachToBooster;
		attachToBooster.Enter(FindAndAttachToInstalledBooster).GoTo(Inactive);
		Inactive.EventTransition(GameHashes.ScheduleBlocksChanged, Active, ShouldBeActive).EventTransition(GameHashes.ScheduleChanged, Active, ShouldBeActive).EventTransition(GameHashes.BionicOnline, Active, ShouldBeActive)
			.EventTransition(GameHashes.MinionMigration, (Instance smi) => Game.Instance, Active, ShouldBeActive)
			.TriggerOnEnter(GameHashes.BionicUpgradeWattageChanged);
		Active.EventTransition(GameHashes.ScheduleBlocksChanged, Inactive, GameStateMachine<BionicUpgrade_ExplorerBoosterMonitor, Instance, IStateMachineTarget, BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.Def>.Not(BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.IsInBedTimeChore)).EventTransition(GameHashes.ScheduleChanged, Inactive, GameStateMachine<BionicUpgrade_ExplorerBoosterMonitor, Instance, IStateMachineTarget, BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.Def>.Not(BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.IsInBedTimeChore)).EventTransition(GameHashes.BionicOffline, Inactive, GameStateMachine<BionicUpgrade_ExplorerBoosterMonitor, Instance, IStateMachineTarget, BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.Def>.Not(BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.IsOnline))
			.EventTransition(GameHashes.MinionMigration, (Instance smi) => Game.Instance, Inactive, GameStateMachine<BionicUpgrade_ExplorerBoosterMonitor, Instance, IStateMachineTarget, BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.Def>.Not(ShouldBeActive))
			.DefaultState(Active.gatheringData);
		Active.gatheringData.OnSignal(ReadyToDiscoverSignal, Active.discover, IsReadyToDiscoverAndThereIsSomethingToDiscover).ToggleStatusItem(Db.Get().DuplicantStatusItems.BionicExplorerBooster).Update(DataGatheringUpdate);
		Active.discover.Enter(ConsumeAllData).Enter(RevealUndiscoveredGeyser).EnterTransition(Inactive, GameStateMachine<BionicUpgrade_ExplorerBoosterMonitor, Instance, IStateMachineTarget, BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.Def>.Not(IsThereGeysersToDiscover))
			.GoTo(Active.gatheringData);
	}

	public static bool ShouldBeActive(Instance smi)
	{
		return BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.IsOnline(smi) && BionicUpgrade_SM<BionicUpgrade_ExplorerBoosterMonitor, Instance>.IsInBedTimeChore(smi) && IsThereGeysersToDiscover(smi);
	}

	public static bool IsReadyToDiscoverAndThereIsSomethingToDiscover(Instance smi, SignalParameter param)
	{
		return smi.IsReadyToDiscover && IsThereGeysersToDiscover(smi);
	}

	public static void ConsumeAllData(Instance smi)
	{
		smi.ConsumeAllData();
	}

	public static void FindAndAttachToInstalledBooster(Instance smi)
	{
		smi.Initialize();
	}

	public static void DataGatheringUpdate(Instance smi, float dt)
	{
		smi.GatheringDataUpdate(dt);
	}

	public static bool IsThereGeysersToDiscover(Instance smi)
	{
		WorldContainer myWorld = smi.GetMyWorld();
		if (myWorld.id != 255)
		{
			List<WorldGenSpawner.Spawnable> list = new List<WorldGenSpawner.Spawnable>();
			list.AddRange(SaveGame.Instance.worldGenSpawner.GeInfoOfUnspawnedWithType<Geyser>(myWorld.id));
			list.AddRange(SaveGame.Instance.worldGenSpawner.GetSpawnablesWithTag("GeyserGeneric", myWorld.id));
			list.AddRange(SaveGame.Instance.worldGenSpawner.GetSpawnablesWithTag("OilWell", myWorld.id));
			if (list.Count > 0)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static void RevealUndiscoveredGeyser(Instance smi)
	{
		WorldContainer myWorld = smi.GetMyWorld();
		if (myWorld.id == 255)
		{
			return;
		}
		List<WorldGenSpawner.Spawnable> list = new List<WorldGenSpawner.Spawnable>();
		list.AddRange(SaveGame.Instance.worldGenSpawner.GeInfoOfUnspawnedWithType<Geyser>(myWorld.id));
		list.AddRange(SaveGame.Instance.worldGenSpawner.GetSpawnablesWithTag("GeyserGeneric", myWorld.id));
		list.AddRange(SaveGame.Instance.worldGenSpawner.GetSpawnablesWithTag("OilWell", myWorld.id));
		if (list.Count > 0)
		{
			WorldGenSpawner.Spawnable random = list.GetRandom();
			Grid.CellToXY(random.cell, out var x, out var y);
			GridVisibility.Reveal(x, y, 4, 4f);
			Notifier notifier = smi.gameObject.AddOrGet<Notifier>();
			Notification geyserDiscoveredNotification = smi.GetGeyserDiscoveredNotification();
			int cell = random.cell;
			geyserDiscoveredNotification.customClickCallback = delegate
			{
				GameUtil.FocusCamera(cell);
			};
			notifier.Add(geyserDiscoveredNotification);
		}
	}
}
