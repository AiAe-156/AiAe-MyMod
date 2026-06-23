using System;
using System.Collections.Generic;
using Klei.AI;
using Klei.CustomSettings;
using STRINGS;
using UnityEngine;

public class BionicBatteryMonitor : GameStateMachine<BionicBatteryMonitor, BionicBatteryMonitor.Instance, IStateMachineTarget, BionicBatteryMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public struct WattageModifier
	{
		public float potentialValue;

		public float value;

		public string name;

		public string id;

		public WattageModifier(string id, string name, float value, float potentialValue)
		{
			this.id = id;
			this.name = name;
			this.value = value;
			this.potentialValue = potentialValue;
		}
	}

	public class OnlineStates : State
	{
		public State idle;

		public UpkeepStates upkeep;

		public UpkeepStates critical;
	}

	public class UpkeepStates : State
	{
		public State seekElectrobank;
	}

	public class OfflineStates : State
	{
		public State waitingForBatteryDelivery;

		public RebootWorkableState waitingForBatteryInstallation;

		public State reboot;
	}

	public class RebootWorkableState : State
	{
		public State waiting;

		public State working;
	}

	public new class Instance : GameInstance
	{
		public Storage storage;

		public KPrefabID prefabID;

		private Schedulable schedulable;

		private AmountInstance BionicBattery;

		private ManualDeliveryKG manualDelivery;

		private ClosestElectrobankSensor closestElectrobankSensor;

		private KSelectable selectable;

		private MinionStorageDataHolder dataHolder;

		private Guid criticalBatteryStatusItemGuid;

		private Chore reanimateChore;

		public float Wattage => GetBaseWattage() + GetModifiersWattage();

		public bool IsOnline => base.sm.IsOnline.Get(this);

		public bool InUpkeepTime => schedulable.IsAllowed(Db.Get().ScheduleBlockTypes.Eat);

		public bool HaveInitialElectrobanksBeenSpawned => base.sm.InitialElectrobanksSpawned.Get(this);

		public bool HasSpaceForNewElectrobank => ElectrobankCount < ElectrobankCountCapacity;

		public int ElectrobankCount => ChargedElectrobankCount + DepletedElectrobankCount;

		public int ChargedElectrobankCount => base.sm.ChargedElectrobankCount.Get(this);

		public int DepletedElectrobankCount => base.sm.DepletedElectrobankCount.Get(this);

		public float CurrentCharge => BionicBattery.value;

		public int ElectrobankCountCapacity => (int)base.gameObject.GetAttributes().Get(Db.Get().Attributes.BionicBatteryCountCapacity.Id).GetTotalValue();

		public ReanimateBionicWorkable reanimateWorkable { get; private set; }

		public List<WattageModifier> Modifiers { get; set; } = new List<WattageModifier>();

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			storage = base.gameObject.GetComponents<Storage>().FindFirst((Storage s) => s.storageID == GameTags.StoragesIds.BionicBatteryStorage);
			reanimateWorkable = GetComponent<ReanimateBionicWorkable>();
			schedulable = GetComponent<Schedulable>();
			manualDelivery = GetComponent<ManualDeliveryKG>();
			selectable = GetComponent<KSelectable>();
			prefabID = GetComponent<KPrefabID>();
			dataHolder = GetComponent<MinionStorageDataHolder>();
			MinionStorageDataHolder minionStorageDataHolder = dataHolder;
			minionStorageDataHolder.OnCopyBegins = (Action<StoredMinionIdentity>)Delegate.Combine(minionStorageDataHolder.OnCopyBegins, new Action<StoredMinionIdentity>(OnCopyMinionBegins));
			BionicBattery = Db.Get().Amounts.BionicInternalBattery.Lookup(base.gameObject);
			Storage obj = storage;
			obj.onDestroyItemsDropped = (Action<List<GameObject>>)Delegate.Combine(obj.onDestroyItemsDropped, new Action<List<GameObject>>(OnBatteriesDroppedFromDeath));
			Storage obj2 = storage;
			obj2.OnStorageChange = (Action<GameObject>)Delegate.Combine(obj2.OnStorageChange, new Action<GameObject>(OnElectrobankStorageChanged));
			Subscribe(540773776, OnSkillsChanged);
			UpdateCapacityAmount();
			ApplyDifficultyModifiers();
		}

		public override void StartSM()
		{
			closestElectrobankSensor = GetComponent<Sensors>().GetSensor<ClosestElectrobankSensor>();
			ClosestElectrobankSensor obj = closestElectrobankSensor;
			obj.OnItemChanged = (Action<Electrobank>)Delegate.Combine(obj.OnItemChanged, new Action<Electrobank>(OnClosestElectrobankChanged));
			base.StartSM();
		}

		private void OnCopyMinionBegins(StoredMinionIdentity destination)
		{
			MinionStorageDataHolder.DataPackData dataPackData = new MinionStorageDataHolder.DataPackData();
			dataPackData.Bools = new bool[2] { HaveInitialElectrobanksBeenSpawned, IsOnline };
			MinionStorageDataHolder.DataPackData data = dataPackData;
			dataHolder.UpdateData<Instance>(data);
		}

		public override void PostParamsInitialized()
		{
			MinionStorageDataHolder.DataPack dataPack = dataHolder.GetDataPack<Instance>();
			if (dataPack != null && dataPack.IsStoringNewData)
			{
				MinionStorageDataHolder.DataPackData dataPackData = dataPack.ReadData();
				if (dataPackData != null)
				{
					bool value = ((dataPackData.Bools != null && dataPackData.Bools.Length != 0) ? dataPackData.Bools[0] : HasSpaceForNewElectrobank);
					bool value2 = ((dataPackData.Bools != null && dataPackData.Bools.Length > 1) ? dataPackData.Bools[1] : IsOnline);
					base.sm.InitialElectrobanksSpawned.Set(value, this);
					base.sm.IsOnline.Set(value2, this);
				}
			}
			RefreshCharge();
			base.PostParamsInitialized();
		}

		public void DropAllDischargedElectrobanks()
		{
			List<GameObject> list = new List<GameObject>();
			storage.Find(GameTags.EmptyPortableBattery, list);
			foreach (GameObject item in list)
			{
				storage.Drop(item);
			}
		}

		protected override void OnCleanUp()
		{
			if (dataHolder != null)
			{
				MinionStorageDataHolder minionStorageDataHolder = dataHolder;
				minionStorageDataHolder.OnCopyBegins = (Action<StoredMinionIdentity>)Delegate.Remove(minionStorageDataHolder.OnCopyBegins, new Action<StoredMinionIdentity>(OnCopyMinionBegins));
			}
			UpdateNotifications();
			base.OnCleanUp();
		}

		private void OnSkillsChanged(object o)
		{
			if (storage.capacityKg != (float)ElectrobankCountCapacity)
			{
				OnBatteryCapacityChanged();
			}
		}

		private void ApplyDifficultyModifiers()
		{
			SettingLevel currentQualitySetting = CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.BionicWattage);
			if (difficultyWattages.TryGetValue(currentQualitySetting.id, out var value))
			{
				Modifiers.Add(value);
			}
		}

		private void UpdateCapacityAmount()
		{
			int num = ElectrobankCountCapacity - 4;
			BionicBattery.maxAttribute.ClearModifiers();
			BionicBattery.maxAttribute.Add(new AttributeModifier(Db.Get().Amounts.BionicInternalBattery.maxAttribute.Id, 120000f * (float)num));
		}

		private void OnBatteryCapacityChanged()
		{
			UpdateCapacityAmount();
			for (int num = storage.Count - 1; num >= 0; num--)
			{
				if (storage.Count > ElectrobankCountCapacity)
				{
					GameObject gameObject = storage.items[num];
					Electrobank component = gameObject.GetComponent<Electrobank>();
					storage.Drop(gameObject);
					Vector3 position = gameObject.transform.position;
					position.z = Grid.GetLayerZ(Grid.SceneLayer.Ore);
					gameObject.transform.position = position;
					if (component != null && component.HasTag(GameTags.ChargedPortableBattery) && !component.IsFullyCharged)
					{
						component.RemovePower(component.Charge, dropWhenEmpty: true);
					}
				}
			}
			base.smi.storage.capacityKg = ElectrobankCountCapacity;
		}

		private void OnClosestElectrobankChanged(Electrobank newItem)
		{
			base.sm.OnClosestAvailableElectrobankChangedSignal.Trigger(this);
		}

		public float GetBaseWattage()
		{
			return 200f;
		}

		public float GetModifiersWattage()
		{
			float num = 0f;
			foreach (WattageModifier modifier in Modifiers)
			{
				num += modifier.value;
			}
			return num;
		}

		private void OnElectrobankStorageChanged(object o)
		{
			ReorganizeElectrobanks();
			RefreshCharge();
			base.smi.sm.OnElectrobankStorageChanged.Trigger(this);
		}

		public void ReorganizeElectrobanks()
		{
			storage.items.Sort(delegate(GameObject b1, GameObject b2)
			{
				Electrobank component = b1.GetComponent<Electrobank>();
				Electrobank component2 = b2.GetComponent<Electrobank>();
				if (component == null)
				{
					return -1;
				}
				return (component2 == null) ? 1 : component.Charge.CompareTo(component2.Charge);
			});
		}

		public void CreateWorkableChore()
		{
			if (reanimateChore == null)
			{
				reanimateChore = new WorkChore<ReanimateBionicWorkable>(Db.Get().ChoreTypes.RescueIncapacitated, reanimateWorkable, null, run_until_complete: true, null, null, null, allow_in_red_alert: true, null, ignore_schedule_block: false, only_when_operational: false, null, is_preemptable: false, allow_in_context_menu: true, allow_prioritization: false, PriorityScreen.PriorityClass.personalNeeds);
				reanimateChore.AddPrecondition(ChorePreconditions.instance.IsNotARobot);
			}
		}

		public void CancelWorkChore()
		{
			if (reanimateChore != null)
			{
				reanimateChore.Cancel("BionicBatteryMonitor.CancelChore");
				reanimateChore = null;
			}
		}

		public void SetOnlineState(bool online)
		{
			base.sm.IsOnline.Set(online, this);
			RefreshCharge();
		}

		public void SetManualDeliveryEnableState(bool enable)
		{
			if (!enable)
			{
				manualDelivery.capacity = 0f;
				manualDelivery.refillMass = 0f;
				manualDelivery.RequestedItemTag = null;
				manualDelivery.AbortDelivery("Manual delivery disabled");
				return;
			}
			Tag[] array = new Tag[GameTags.BionicIncompatibleBatteries.Count];
			GameTags.BionicIncompatibleBatteries.CopyTo(array, 0);
			base.smi.storage.capacityKg = ElectrobankCountCapacity;
			base.smi.manualDelivery.capacity = 1f;
			base.smi.manualDelivery.refillMass = 1f;
			base.smi.manualDelivery.MinimumMass = 1f;
			manualDelivery.ForbiddenTags = array;
			manualDelivery.RequestedItemTag = GameTags.ChargedPortableBattery;
		}

		public GameObject GetFirstDischargedElectrobankInInventory()
		{
			return storage.FindFirst(GameTags.EmptyPortableBattery);
		}

		public Electrobank GetClosestElectrobank()
		{
			return closestElectrobankSensor.GetItem();
		}

		public void RefreshCharge()
		{
			ListPool<GameObject, Instance>.PooledList pooledList = ListPool<GameObject, Instance>.Allocate();
			ListPool<GameObject, Instance>.PooledList pooledList2 = ListPool<GameObject, Instance>.Allocate();
			storage.Find(GameTags.ChargedPortableBattery, pooledList);
			storage.Find(GameTags.EmptyPortableBattery, pooledList2);
			float num = 0f;
			if (IsOnline)
			{
				for (int i = 0; i < pooledList.Count; i++)
				{
					Electrobank component = pooledList[i].GetComponent<Electrobank>();
					num += component.Charge;
				}
			}
			BionicBattery.SetValue(num);
			base.sm.ChargedElectrobankCount.Set(pooledList.Count, this);
			pooledList.Recycle();
			base.sm.DepletedElectrobankCount.Set(pooledList2.Count, this);
			pooledList2.Recycle();
			UpdateNotifications();
		}

		public void ConsumePower(float joules)
		{
			ListPool<GameObject, Instance>.PooledList pooledList = ListPool<GameObject, Instance>.Allocate();
			storage.Find(GameTags.ChargedPortableBattery, pooledList);
			float num = joules;
			for (int i = 0; i < pooledList.Count; i++)
			{
				Electrobank component = pooledList[i].GetComponent<Electrobank>();
				float joules2 = Mathf.Min(component.Charge, num);
				float num2 = component.RemovePower(joules2, dropWhenEmpty: false);
				num -= num2;
				WorldResourceAmountTracker<ElectrobankTracker>.Get().RegisterAmountConsumed(component.ID, num2);
			}
			RefreshCharge();
			pooledList.Recycle();
		}

		public void DebugAddCharge(float joules)
		{
			float num = MathF.Min(joules, (float)ElectrobankCountCapacity * 120000f - CurrentCharge);
			float num2 = num;
			ListPool<GameObject, Instance>.PooledList pooledList = ListPool<GameObject, Instance>.Allocate();
			storage.Find(GameTags.ChargedPortableBattery, pooledList);
			int num3 = 0;
			while (num2 > 0f && num3 < pooledList.Count)
			{
				Electrobank component = pooledList[num3].GetComponent<Electrobank>();
				float a = 120000f - component.Charge;
				float num4 = Mathf.Min(a, num2);
				component.AddPower(num4);
				num2 -= num4;
				num3++;
			}
			if (num2 > 0f && pooledList.Count < ElectrobankCountCapacity)
			{
				int num5 = storage.items.Count - 1;
				while (num2 > 0f && num5 >= 0)
				{
					GameObject gameObject = storage.items[num5];
					if (!(gameObject == null))
					{
						Electrobank component2 = gameObject.GetComponent<Electrobank>();
						if (component2 == null && gameObject.HasTag(GameTags.EmptyPortableBattery))
						{
							storage.Drop(gameObject);
							GameObject gameObject2 = Util.KInstantiate(Assets.GetPrefab("DisposableElectrobank_RawMetal"), base.transform.position);
							gameObject2.SetActive(value: true);
							component2 = gameObject2.GetComponent<Electrobank>();
							float joules2 = Mathf.Clamp(component2.Charge - num2, 0f, float.MaxValue);
							component2.RemovePower(joules2, dropWhenEmpty: true);
							num2 -= component2.Charge;
							storage.Store(gameObject2);
						}
					}
					num5--;
				}
			}
			if (num2 > 0f && storage.items.Count < ElectrobankCountCapacity)
			{
				do
				{
					GameObject gameObject3 = Util.KInstantiate(Assets.GetPrefab("DisposableElectrobank_RawMetal"), base.transform.position);
					gameObject3.SetActive(value: true);
					Electrobank component3 = gameObject3.GetComponent<Electrobank>();
					float joules3 = Mathf.Clamp(component3.Charge - num2, 0f, float.MaxValue);
					component3.RemovePower(joules3, dropWhenEmpty: true);
					num2 -= component3.Charge;
					storage.Store(gameObject3);
				}
				while (num2 > 0f && storage.items.Count < ElectrobankCountCapacity && num2 > 0f);
			}
			RefreshCharge();
			pooledList.Recycle();
		}

		private void UpdateNotifications()
		{
			criticalBatteryStatusItemGuid = selectable.ToggleStatusItem(Db.Get().DuplicantStatusItems.BionicCriticalBattery, criticalBatteryStatusItemGuid, ChargeIsBelowNotificationThreshold(base.smi) && !prefabID.HasTag(GameTags.Incapacitated) && !prefabID.HasTag(GameTags.Dead), base.gameObject);
		}

		public bool AddOrUpdateModifier(WattageModifier modifier, bool triggerCallbacks = true)
		{
			int num = Modifiers.FindIndex((WattageModifier mod) => mod.id == modifier.id);
			bool flag = false;
			if (num >= 0)
			{
				flag = Modifiers[num].name != modifier.name || Modifiers[num].value != modifier.value || Modifiers[num].potentialValue != modifier.potentialValue;
				Modifiers[num] = modifier;
			}
			else
			{
				Modifiers.Add(modifier);
				flag = true;
			}
			if (flag)
			{
				Modifiers.Sort((WattageModifier a, WattageModifier b) => b.value.CompareTo(a.value));
			}
			if (triggerCallbacks)
			{
				BoxingTrigger(1361471071, Wattage);
			}
			return flag;
		}

		public bool RemoveModifier(string modifierID, bool triggerCallbacks = true)
		{
			int num = Modifiers.FindIndex((WattageModifier mod) => mod.id == modifierID);
			if (num >= 0)
			{
				Modifiers.RemoveAt(num);
				if (triggerCallbacks)
				{
					BoxingTrigger(1361471071, Wattage);
				}
				Modifiers.Sort((WattageModifier a, WattageModifier b) => b.value.CompareTo(a.value));
				return true;
			}
			return false;
		}

		private void OnBatteriesDroppedFromDeath(List<GameObject> items)
		{
			if (items == null)
			{
				return;
			}
			for (int i = 0; i < items.Count; i++)
			{
				GameObject gameObject = items[i];
				Electrobank component = gameObject.GetComponent<Electrobank>();
				if (component != null && component.HasTag(GameTags.ChargedPortableBattery) && !component.IsFullyCharged)
				{
					component.RemovePower(component.Charge, dropWhenEmpty: true);
				}
			}
		}

		public void SpawnAndInstallInitialElectrobanks()
		{
			for (int i = 0; i < ElectrobankCountCapacity; i++)
			{
				GameObject gameObject = Util.KInstantiate(Assets.GetPrefab("DisposableElectrobank_RawMetal"), base.transform.position);
				gameObject.SetActive(value: true);
				storage.Store(gameObject);
			}
			RefreshCharge();
			SetOnlineState(online: true);
			base.sm.InitialElectrobanksSpawned.Set(value: true, this);
		}

		public void AutomaticallyDropAllDepletedElectrobanks()
		{
			List<GameObject> list = new List<GameObject>();
			storage.Find(GameTags.EmptyPortableBattery, list);
			for (int i = 0; i < list.Count; i++)
			{
				GameObject go = list[i];
				storage.Drop(go);
			}
		}
	}

	public const int DEFAULT_ELECTROBANK_COUNT = 4;

	public const int BIONIC_SKILL_EXTRA_BATTERY_COUNT = 2;

	public const int MAX_ELECTROBANK_COUNT = 6;

	public const float DEFAULT_WATTS = 200f;

	public const string INITIAL_ELECTROBANK_TYPE_ID = "DisposableElectrobank_RawMetal";

	public static readonly string ChargedBatteryIcon = "<sprite=\"oni_sprite_assets\" name=\"oni_sprite_assets_charged_electrobank\">";

	public static readonly string DischargedBatteryIcon = "<sprite=\"oni_sprite_assets\" name=\"oni_sprite_assets_discharged_electrobank\">";

	public static readonly string CriticalBatteryIcon = "<sprite=\"oni_sprite_assets\" name=\"oni_sprite_assets_critical_electrobank\">";

	public static readonly string SavingBatteryIcon = "<sprite=\"oni_sprite_assets\" name=\"oni_sprite_assets_saving_electrobank\">";

	public static readonly string EmptySlotBatteryIcon = "<sprite=\"oni_sprite_assets\" name=\"oni_sprite_assets_empty_slot_electrobank\">";

	private const string ANIM_NAME_REBOOT = "power_up";

	public State firstSpawn;

	public OnlineStates online;

	public OfflineStates offline;

	public Signal OnClosestAvailableElectrobankChangedSignal;

	public IntParameter ChargedElectrobankCount;

	public IntParameter DepletedElectrobankCount;

	private BoolParameter InitialElectrobanksSpawned;

	private BoolParameter IsOnline;

	private Signal OnElectrobankStorageChanged;

	private static readonly Dictionary<string, WattageModifier> difficultyWattages = new Dictionary<string, WattageModifier>
	{
		{
			"VeryHard",
			MakeDifficultyModifier("difficultyWattage", UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.BIONICPOWERUSE.LEVELS.VERYHARD.NAME, 200f)
		},
		{
			"Hard",
			MakeDifficultyModifier("difficultyWattage", UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.BIONICPOWERUSE.LEVELS.HARD.NAME, 100f)
		},
		{
			"Default",
			MakeDifficultyModifier("difficultyWattage", UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.BIONICPOWERUSE.LEVELS.DEFAULT.NAME, 0f)
		},
		{
			"Easy",
			MakeDifficultyModifier("difficultyWattage", UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.BIONICPOWERUSE.LEVELS.EASY.NAME, -100f)
		},
		{
			"VeryEasy",
			MakeDifficultyModifier("difficultyWattage", UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.BIONICPOWERUSE.LEVELS.VERYEASY.NAME, -150f)
		}
	};

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = firstSpawn;
		firstSpawn.ParamTransition(InitialElectrobanksSpawned, online, GameStateMachine<BionicBatteryMonitor, Instance, IStateMachineTarget, Def>.IsTrue).Enter(SpawnAndInstallInitialElectrobanks);
		online.TriggerOnEnter(GameHashes.BionicOnline).Transition(offline, DoesNotHaveCharge).Enter(ReorganizeElectrobankStorage)
			.Update(DischargeUpdate)
			.DefaultState(online.idle);
		online.idle.ParamTransition(ChargedElectrobankCount, online.critical, GameStateMachine<BionicBatteryMonitor, Instance, IStateMachineTarget, Def>.IsLTEOne_Int).OnSignal(OnElectrobankStorageChanged, online.upkeep, WantsToUpkeep).EventTransition(GameHashes.ScheduleBlocksChanged, online.upkeep, WantsToUpkeep)
			.EventTransition(GameHashes.ScheduleChanged, online.upkeep, WantsToUpkeep)
			.EventTransition(GameHashes.ScheduleBlocksTick, online.upkeep, WantsToUpkeep);
		online.upkeep.ParamTransition(ChargedElectrobankCount, online.critical, GameStateMachine<BionicBatteryMonitor, Instance, IStateMachineTarget, Def>.IsLTEOne_Int).EventTransition(GameHashes.ScheduleBlocksChanged, online.idle, GameStateMachine<BionicBatteryMonitor, Instance, IStateMachineTarget, Def>.Not(WantsToUpkeep)).EventTransition(GameHashes.ScheduleChanged, online.idle, GameStateMachine<BionicBatteryMonitor, Instance, IStateMachineTarget, Def>.Not(WantsToUpkeep))
			.EventTransition(GameHashes.ScheduleBlocksTick, online.idle, GameStateMachine<BionicBatteryMonitor, Instance, IStateMachineTarget, Def>.Not(WantsToUpkeep))
			.OnSignal(OnElectrobankStorageChanged, online.idle, (Instance smi, SignalParameter param) => !WantsToUpkeep(smi))
			.DefaultState(online.upkeep.seekElectrobank);
		online.upkeep.seekElectrobank.ToggleUrge(Db.Get().Urges.ReloadElectrobank).ToggleChore((Instance smi) => new ReloadElectrobankChore(smi.master), online.idle);
		online.critical.DefaultState(online.critical.seekElectrobank).ParamTransition(ChargedElectrobankCount, online.idle, GameStateMachine<BionicBatteryMonitor, Instance, IStateMachineTarget, Def>.IsGTOne_Int).DoTutorial(Tutorial.TutorialMessages.TM_BionicBattery);
		online.critical.seekElectrobank.ToggleUrge(Db.Get().Urges.ReloadElectrobank).ToggleRecurringChore((Instance smi) => new ReloadElectrobankChore(smi.master));
		offline.DefaultState(offline.waitingForBatteryDelivery).ToggleTag(GameTags.Incapacitated).ToggleRecurringChore((Instance smi) => new BeOfflineChore(smi.master))
			.ToggleUrge(Db.Get().Urges.BeOffline)
			.Enter(SetOffline)
			.Enter(DropAllDischargedElectrobanks)
			.TriggerOnEnter(GameHashes.BionicOffline);
		offline.waitingForBatteryDelivery.ParamTransition(ChargedElectrobankCount, offline.waitingForBatteryInstallation, GameStateMachine<BionicBatteryMonitor, Instance, IStateMachineTarget, Def>.IsGTZero_Int).Toggle("Enable Delivery of new Electrobanks", EnableManualDelivery, DisableManualDelivery).Toggle("Enable User Prioritization", EnablePrioritizationComponent, DisablePrioritizationComponent);
		offline.waitingForBatteryInstallation.Toggle("Enable User Prioritization", EnablePrioritizationComponent, DisablePrioritizationComponent).Enter(StartReanimateWorkChore).Exit(CancelReanimateWorkChore)
			.WorkableCompleteTransition(GetReanimateWorkable, offline.reboot)
			.DefaultState(offline.waitingForBatteryInstallation.waiting);
		offline.waitingForBatteryInstallation.waiting.ToggleStatusItem(Db.Get().DuplicantStatusItems.BionicWaitingForReboot).WorkableStartTransition(GetReanimateWorkable, offline.waitingForBatteryInstallation.working);
		offline.waitingForBatteryInstallation.working.WorkableStopTransition(GetReanimateWorkable, offline.waitingForBatteryInstallation.waiting);
		offline.reboot.PlayAnim("power_up").OnAnimQueueComplete(online).ScheduleGoTo(10f, online)
			.Exit(AutomaticallyDropAllDepletedElectrobanks)
			.Exit(SetOnline);
	}

	public static ReanimateBionicWorkable GetReanimateWorkable(Instance smi)
	{
		return smi.reanimateWorkable;
	}

	public static bool DoesNotHaveCharge(Instance smi)
	{
		return smi.CurrentCharge <= 0f;
	}

	public static bool IsCriticallyLow(Instance smi)
	{
		return smi.ChargedElectrobankCount <= 1;
	}

	public static bool ChargeIsBelowNotificationThreshold(Instance smi)
	{
		return smi.CurrentCharge <= 30000f;
	}

	public static bool IsAnyElectrobankAvailableToBeFetched(Instance smi)
	{
		return smi.GetClosestElectrobank() != null;
	}

	public static bool WantsToInstallNewBattery(Instance smi)
	{
		return IsCriticallyLow(smi) || (smi.InUpkeepTime && smi.ChargedElectrobankCount < smi.ElectrobankCountCapacity);
	}

	public static bool WantsToUpkeep(Instance smi)
	{
		return WantsToInstallNewBattery(smi);
	}

	public static bool WantsToUpkeep(Instance smi, SignalParameter param)
	{
		return WantsToInstallNewBattery(smi);
	}

	public static void SpawnAndInstallInitialElectrobanks(Instance smi)
	{
		smi.SpawnAndInstallInitialElectrobanks();
	}

	public static void RefreshCharge(Instance smi)
	{
		smi.RefreshCharge();
	}

	public static void EnableManualDelivery(Instance smi)
	{
		smi.SetManualDeliveryEnableState(enable: true);
	}

	public static void DisableManualDelivery(Instance smi)
	{
		smi.SetManualDeliveryEnableState(enable: false);
	}

	public static void StartReanimateWorkChore(Instance smi)
	{
		smi.CreateWorkableChore();
	}

	public static void CancelReanimateWorkChore(Instance smi)
	{
		smi.CancelWorkChore();
	}

	public static void SetOffline(Instance smi)
	{
		smi.SetOnlineState(online: false);
	}

	public static void SetOnline(Instance smi)
	{
		smi.SetOnlineState(online: true);
	}

	public static void AutomaticallyDropAllDepletedElectrobanks(Instance smi)
	{
		smi.AutomaticallyDropAllDepletedElectrobanks();
	}

	public static void ReorganizeElectrobankStorage(Instance smi)
	{
		smi.ReorganizeElectrobanks();
	}

	public static void DropAllDischargedElectrobanks(Instance smi)
	{
		smi.DropAllDischargedElectrobanks();
	}

	public static void EnablePrioritizationComponent(Instance smi)
	{
		Prioritizable.AddRef(smi.gameObject);
		smi.gameObject.Trigger(1980521255);
	}

	public static void DisablePrioritizationComponent(Instance smi)
	{
		Prioritizable.RemoveRef(smi.gameObject);
		smi.gameObject.Trigger(1980521255);
	}

	public static void DischargeUpdate(Instance smi, float dt)
	{
		float joules = Mathf.Min(dt * smi.Wattage, smi.CurrentCharge);
		smi.ConsumePower(joules);
	}

	private static WattageModifier MakeDifficultyModifier(string id, string desc, float watts)
	{
		return new WattageModifier(id, desc + ": <b>" + ((watts >= 0f) ? "+</b>" : "-</b>") + GameUtil.GetFormattedWattage(Mathf.Abs(watts)), watts, watts);
	}

	public static WattageModifier GetDifficultyModifier()
	{
		SettingLevel currentQualitySetting = CustomGameSettings.Instance.GetCurrentQualitySetting(CustomGameSettingConfigs.BionicWattage);
		if (difficultyWattages.TryGetValue(currentQualitySetting.id, out var value))
		{
			return value;
		}
		return difficultyWattages["Default"];
	}
}
