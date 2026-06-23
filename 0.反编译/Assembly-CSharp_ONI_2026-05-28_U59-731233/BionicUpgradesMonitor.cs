using System;
using System.Collections.Generic;
using KSerialization;
using Klei.AI;
using TUNING;
using UnityEngine;

public class BionicUpgradesMonitor : GameStateMachine<BionicUpgradesMonitor, BionicUpgradesMonitor.Instance, IStateMachineTarget, BionicUpgradesMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public class SeekingStates : State
	{
		public State inProgress;

		public State failed;
	}

	public class ActiveStates : State
	{
		public State idle;

		public SeekingStates seeking;
	}

	public new class Instance : GameInstance
	{
		[SerializationConfig(MemberSerialization.OptIn)]
		private struct StorageDataHolderData
		{
			[Serialize]
			public bool initialUpgradesSpawned;

			[Serialize]
			public Tag[] upgradeComponentSlotsInstalledTags;
		}

		[Serialize]
		public UpgradeComponentSlot[] upgradeComponentSlots;

		private BionicBatteryMonitor.Instance batteryMonitor;

		private Storage upgradesStorage;

		private Ownables minionOwnables;

		private MinionStorageDataHolder dataHolder;

		private Navigator navigator;

		public bool IsOnline => batteryMonitor != null && batteryMonitor.IsOnline;

		public bool HasAnyUpgradeAssigned => upgradeComponentSlots != null && GetAnyAssignedSlot() != null;

		public bool HasAnyUpgradeInstalled => upgradeComponentSlots != null && GetAnyInstalledUpgradeSlot() != null;

		public int UnlockedSlotCount => Math.Clamp((int)base.gameObject.GetAttributes().Get(Db.Get().Attributes.BionicBoosterSlots.Id).GetTotalValue(), 0, 8);

		public int AssignedSlotCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < upgradeComponentSlots.Length; i++)
				{
					if (upgradeComponentSlots[i].assignedUpgradeComponent != null)
					{
						num++;
					}
				}
				return num;
			}
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			IAssignableIdentity component = GetComponent<IAssignableIdentity>();
			dataHolder = GetComponent<MinionStorageDataHolder>();
			MinionStorageDataHolder minionStorageDataHolder = dataHolder;
			minionStorageDataHolder.OnCopyBegins = (Action<StoredMinionIdentity>)Delegate.Combine(minionStorageDataHolder.OnCopyBegins, new Action<StoredMinionIdentity>(OnCopyMinionBegins));
			batteryMonitor = base.gameObject.GetSMI<BionicBatteryMonitor.Instance>();
			navigator = GetComponent<Navigator>();
			minionOwnables = component.GetSoleOwner();
			upgradesStorage = base.gameObject.GetComponents<Storage>().FindFirst((Storage s) => s.storageID == GameTags.StoragesIds.BionicUpgradeStorage);
			CreateUpgradeSlots();
			Subscribe(540773776, OnSlotCountAttributeChanged);
			Game.Instance.Trigger(-1523247426, (object)this);
		}

		private void OnCopyMinionBegins(StoredMinionIdentity destination)
		{
			Tag[] array = new Tag[upgradeComponentSlots.Length];
			for (int i = 0; i < upgradeComponentSlots.Length; i++)
			{
				array[i] = upgradeComponentSlots[i].InstalledUpgradeID;
			}
			MinionStorageDataHolder.DataPackData dataPackData = new MinionStorageDataHolder.DataPackData();
			dataPackData.Bools = new bool[1] { base.smi.sm.InitialUpgradeSpawned.Get(base.smi) };
			dataPackData.Tags = array;
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
					base.sm.InitialUpgradeSpawned.Set(dataPackData.Bools[0], base.smi);
					if (dataPackData.Tags != null)
					{
						for (int i = 0; i < Mathf.Min(dataPackData.Tags.Length, upgradeComponentSlots.Length); i++)
						{
							Tag installedUpgradePrefabID = dataPackData.Tags[i];
							upgradeComponentSlots[i].DeserializeAction_OverrideInstalledUpgradePrefabID(installedUpgradePrefabID);
						}
					}
				}
			}
			base.PostParamsInitialized();
		}

		protected override void OnCleanUp()
		{
			if (dataHolder != null)
			{
				MinionStorageDataHolder minionStorageDataHolder = dataHolder;
				minionStorageDataHolder.OnCopyBegins = (Action<StoredMinionIdentity>)Delegate.Remove(minionStorageDataHolder.OnCopyBegins, new Action<StoredMinionIdentity>(OnCopyMinionBegins));
			}
			base.OnCleanUp();
		}

		public void LockSlot(UpgradeComponentSlot slot)
		{
			UninstallUpgrade(slot);
			if (slot.HasUpgradeComponentAssigned && slot.HasSpawned)
			{
				slot.InternalUninstall();
			}
			slot.InternalLock();
		}

		public void UnlockSlot(UpgradeComponentSlot slot)
		{
			slot.InternalUnlock();
		}

		public void InstallUpgrade(BionicUpgradeComponent upgradeComponent)
		{
			UpgradeComponentSlot slotForAssignedUpgrade = GetSlotForAssignedUpgrade(upgradeComponent);
			if (slotForAssignedUpgrade != null)
			{
				slotForAssignedUpgrade.InternalInstall();
				Game.Instance.Trigger(-1523247426, (object)this);
			}
		}

		public void UninstallUpgrade(UpgradeComponentSlot slot)
		{
			if (slot != null && slot.HasUpgradeInstalled)
			{
				slot.InternalUninstall();
				Game.Instance.Trigger(-1523247426, (object)this);
			}
		}

		public void UpdateBatteryMonitorWattageModifiers()
		{
			bool flag = true;
			bool flag2 = false;
			for (int i = 0; i < upgradeComponentSlots.Length; i++)
			{
				flag &= upgradeComponentSlots[i].HasUpgradeInstalled;
				string text = "UPGRADE_SLOT_" + i;
				UpgradeComponentSlot upgradeComponentSlot = upgradeComponentSlots[i];
				if (!upgradeComponentSlot.HasUpgradeInstalled)
				{
					flag2 |= batteryMonitor.RemoveModifier(text, triggerCallbacks: false);
					continue;
				}
				BionicBatteryMonitor.WattageModifier modifier = new BionicBatteryMonitor.WattageModifier
				{
					id = text,
					name = upgradeComponentSlot.installedUpgradeComponent.CurrentWattageName,
					value = upgradeComponentSlot.installedUpgradeComponent.CurrentWattage,
					potentialValue = upgradeComponentSlot.installedUpgradeComponent.PotentialWattage
				};
				flag2 |= batteryMonitor.AddOrUpdateModifier(modifier, triggerCallbacks: false);
			}
			if (flag2)
			{
				batteryMonitor.Trigger(1361471071);
			}
			if (flag)
			{
				SaveGame.Instance.ColonyAchievementTracker.fullyBoostedBionic = true;
			}
		}

		private void OnSlotCountAttributeChanged(object data)
		{
			int unlockedSlotCount = UnlockedSlotCount;
			bool flag = false;
			for (int i = 0; i < upgradeComponentSlots.Length; i++)
			{
				UpgradeComponentSlot upgradeComponentSlot = upgradeComponentSlots[i];
				bool flag2 = i >= unlockedSlotCount;
				if (upgradeComponentSlot.IsLocked != flag2)
				{
					flag = true;
					if (flag2)
					{
						LockSlot(upgradeComponentSlot);
					}
					else
					{
						UnlockSlot(upgradeComponentSlot);
					}
				}
			}
			UpdateBatteryMonitorWattageModifiers();
			if (flag)
			{
				Trigger(1095596132);
			}
		}

		private void CreateUpgradeSlots()
		{
			AssignableSlot bionicUpgrade = Db.Get().AssignableSlots.BionicUpgrade;
			AssignableSlotInstance[] slots = minionOwnables.GetSlots(bionicUpgrade);
			upgradeComponentSlots = new UpgradeComponentSlot[8];
			for (int i = 0; i < upgradeComponentSlots.Length; i++)
			{
				UpgradeComponentSlot upgradeComponentSlot = new UpgradeComponentSlot();
				upgradeComponentSlots[i] = upgradeComponentSlot;
			}
		}

		public void InitializeSlots()
		{
			AssignableSlot bionicUpgrade = Db.Get().AssignableSlots.BionicUpgrade;
			AssignableSlotInstance[] slots = minionOwnables.GetSlots(bionicUpgrade);
			int unlockedSlotCount = UnlockedSlotCount;
			for (int i = 0; i < upgradeComponentSlots.Length; i++)
			{
				UpgradeComponentSlot slot = upgradeComponentSlots[i];
				InitializeUpgradeSlot(slot, slots[i]);
			}
			for (int j = 0; j < upgradeComponentSlots.Length; j++)
			{
				UpgradeComponentSlot upgradeComponentSlot = upgradeComponentSlots[j];
				upgradeComponentSlot.OnSpawn(this);
				bool flag = j >= unlockedSlotCount;
				if (flag != upgradeComponentSlot.IsLocked)
				{
					if (flag)
					{
						LockSlot(upgradeComponentSlot);
					}
					else
					{
						UnlockSlot(upgradeComponentSlot);
					}
				}
			}
		}

		private void InitializeUpgradeSlot(UpgradeComponentSlot slot, AssignableSlotInstance assignableSlotInstance)
		{
			slot.Initialize(assignableSlotInstance, upgradesStorage, this);
			slot.OnInstalledUpgradeReassigned = (Action<UpgradeComponentSlot, IAssignableIdentity>)Delegate.Combine(slot.OnInstalledUpgradeReassigned, new Action<UpgradeComponentSlot, IAssignableIdentity>(OnInstalledUpgradeComponentReassigned));
			slot.OnAssignedUpgradeChanged = (Action<UpgradeComponentSlot>)Delegate.Combine(slot.OnAssignedUpgradeChanged, new Action<UpgradeComponentSlot>(OnSlotAssignationChanged));
		}

		private void OnSlotAssignationChanged(UpgradeComponentSlot slot)
		{
			base.sm.UpgradeSlotAssignationChanged.Trigger(this);
		}

		private void OnInstalledUpgradeComponentReassigned(UpgradeComponentSlot slot, IAssignableIdentity new_assignee)
		{
			if (!slot.AssignedUpgradeMatchesInstalledUpgrade)
			{
				UninstallUpgrade(slot);
			}
		}

		private UpgradeComponentSlot GetSlotForAssignedUpgrade(BionicUpgradeComponent upgradeComponent)
		{
			for (int i = 0; i < upgradeComponentSlots.Length; i++)
			{
				UpgradeComponentSlot upgradeComponentSlot = upgradeComponentSlots[i];
				if (upgradeComponentSlot != null && !upgradeComponentSlot.HasUpgradeInstalled && upgradeComponentSlot.HasUpgradeComponentAssigned && upgradeComponentSlot.assignedUpgradeComponent == upgradeComponent)
				{
					return upgradeComponentSlot;
				}
			}
			return null;
		}

		public UpgradeComponentSlot GetAnyAssignedSlot()
		{
			for (int i = 0; i < upgradeComponentSlots.Length; i++)
			{
				UpgradeComponentSlot upgradeComponentSlot = upgradeComponentSlots[i];
				if (upgradeComponentSlot != null && !upgradeComponentSlot.HasUpgradeInstalled && upgradeComponentSlot.HasUpgradeComponentAssigned)
				{
					return upgradeComponentSlot;
				}
			}
			return null;
		}

		public UpgradeComponentSlot GetAnyReachableAssignedSlot()
		{
			for (int i = 0; i < upgradeComponentSlots.Length; i++)
			{
				UpgradeComponentSlot upgradeComponentSlot = upgradeComponentSlots[i];
				if (upgradeComponentSlot != null && !upgradeComponentSlot.HasUpgradeInstalled && upgradeComponentSlot.HasUpgradeComponentAssigned && IsBionicUpgradeComponentObjectAbleToBePickedUp(upgradeComponentSlot.assignedUpgradeComponent))
				{
					return upgradeComponentSlot;
				}
			}
			return null;
		}

		public bool IsBionicUpgradeComponentObjectAbleToBePickedUp(BionicUpgradeComponent upgradecComponent)
		{
			Pickupable component = upgradecComponent.GetComponent<Pickupable>();
			if (component == null)
			{
				return false;
			}
			if (component.KPrefabID.HasTag(GameTags.StoredPrivate))
			{
				return false;
			}
			if (!component.CouldBePickedUpByMinion(GetComponent<KPrefabID>().InstanceID))
			{
				return false;
			}
			if (navigator.CanReach(component))
			{
				return true;
			}
			return false;
		}

		private UpgradeComponentSlot GetAnyInstalledUpgradeSlot()
		{
			for (int i = 0; i < upgradeComponentSlots.Length; i++)
			{
				UpgradeComponentSlot upgradeComponentSlot = upgradeComponentSlots[i];
				if (upgradeComponentSlot != null && upgradeComponentSlot.HasUpgradeInstalled)
				{
					return upgradeComponentSlot;
				}
			}
			return null;
		}

		public UpgradeComponentSlot GetFirstEmptyAvailableSlot()
		{
			for (int i = 0; i < upgradeComponentSlots.Length; i++)
			{
				UpgradeComponentSlot upgradeComponentSlot = upgradeComponentSlots[i];
				if (!upgradeComponentSlot.IsLocked && !upgradeComponentSlot.HasUpgradeInstalled && !upgradeComponentSlot.HasUpgradeComponentAssigned)
				{
					return upgradeComponentSlot;
				}
			}
			return null;
		}

		public int CountBoosterAssignments(Tag boosterID)
		{
			int num = 0;
			UpgradeComponentSlot[] array = upgradeComponentSlots;
			foreach (UpgradeComponentSlot upgradeComponentSlot in array)
			{
				if (!(upgradeComponentSlot.assignedUpgradeComponent == null) && upgradeComponentSlot.assignedUpgradeComponent.PrefabID() == boosterID)
				{
					num++;
				}
			}
			return num;
		}
	}

	[SerializationConfig(MemberSerialization.OptIn)]
	public class UpgradeComponentSlot
	{
		private BionicUpgradeComponent _installedUpgradeComponent = null;

		private BionicUpgradeComponent _lastAssignedUpgradeComponent = null;

		[Serialize]
		private Tag installedUpgradePrefabID = Tag.Invalid;

		public Action<UpgradeComponentSlot, IAssignableIdentity> OnInstalledUpgradeReassigned;

		public Action<UpgradeComponentSlot> OnAssignedUpgradeChanged;

		private AssignableSlotInstance assignableSlotInstance;

		private Storage storage;

		private int installedUpgradeSubscribeCallbackIDX = -1;

		private StateMachine.Instance _upgradeSmi;

		private Instance master;

		public bool HasUpgradeInstalled => installedUpgradePrefabID != Tag.Invalid;

		public bool HasUpgradeComponentAssigned => assignableSlotInstance.IsAssigned() && !assignableSlotInstance.IsUnassigning();

		public bool AssignedUpgradeMatchesInstalledUpgrade => assignedUpgradeComponent == installedUpgradeComponent;

		public bool HasSpawned { get; private set; } = false;

		public bool IsLocked { get; private set; } = false;

		public float WattageCost => HasUpgradeInstalled ? installedUpgradeComponent.CurrentWattage : 0f;

		public Func<StateMachine.Instance, StateMachine.Instance> StateMachine => HasUpgradeInstalled ? installedUpgradeComponent.StateMachine : null;

		public Tag InstalledUpgradeID => installedUpgradePrefabID;

		public BionicUpgradeComponent assignedUpgradeComponent => assignableSlotInstance.IsUnassigning() ? null : (assignableSlotInstance.assignable as BionicUpgradeComponent);

		public BionicUpgradeComponent installedUpgradeComponent
		{
			get
			{
				if (HasUpgradeInstalled)
				{
					if (_installedUpgradeComponent == null)
					{
						Debug.LogWarning("Error on BionicUpgradeMonitor. storage does not contains bionic upgrade with id " + InstalledUpgradeID.ToString() + " this could be due to loading an old save on a new version");
						installedUpgradePrefabID = Tag.Invalid;
					}
					return _installedUpgradeComponent;
				}
				_installedUpgradeComponent = null;
				return null;
			}
		}

		public void DeserializeAction_OverrideInstalledUpgradePrefabID(Tag installedUpgradePrefabID)
		{
			this.installedUpgradePrefabID = installedUpgradePrefabID;
		}

		public void Initialize(AssignableSlotInstance assignableSlotInstance, Storage storage, Instance master)
		{
			this.assignableSlotInstance = assignableSlotInstance;
			this.assignableSlotInstance.assignables.GetComponent<MinionAssignablesProxy>().GetTargetGameObject().Subscribe(-1585839766, OnAssignablesChanged);
			this.storage = storage;
			this.master = master;
			_lastAssignedUpgradeComponent = assignedUpgradeComponent;
		}

		public AssignableSlotInstance GetAssignableSlotInstance()
		{
			return assignableSlotInstance;
		}

		public void OnSpawn(Instance smi)
		{
			if (HasUpgradeInstalled && _installedUpgradeComponent == null)
			{
				GameObject gameObject = null;
				int i = 0;
				List<GameObject> list = new List<GameObject>();
				storage.Find(InstalledUpgradeID, list);
				for (; i < list.Count; i++)
				{
					if (!(_installedUpgradeComponent == null))
					{
						break;
					}
					GameObject gameObject2 = list[i];
					bool flag = false;
					UpgradeComponentSlot[] upgradeComponentSlots = smi.upgradeComponentSlots;
					foreach (UpgradeComponentSlot upgradeComponentSlot in upgradeComponentSlots)
					{
						if (upgradeComponentSlot != this && upgradeComponentSlot.HasSpawned && !(upgradeComponentSlot.InstalledUpgradeID != InstalledUpgradeID) && upgradeComponentSlot.installedUpgradeComponent.gameObject == gameObject2)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						gameObject = gameObject2;
						break;
					}
				}
				if (gameObject != null)
				{
					_installedUpgradeComponent = gameObject.GetComponent<BionicUpgradeComponent>();
					StartBoosterSM();
				}
			}
			if (HasUpgradeInstalled && installedUpgradeComponent != null)
			{
				if (!HasUpgradeComponentAssigned)
				{
					installedUpgradeComponent.Assign(assignableSlotInstance.assignables.GetComponent<MinionAssignablesProxy>(), assignableSlotInstance);
				}
				SubscribeToInstallledUpgradeAssignable();
			}
			HasSpawned = true;
		}

		public void SubscribeToInstallledUpgradeAssignable()
		{
			UnsubscribeFromInstalledUpgradeAssignable();
			installedUpgradeSubscribeCallbackIDX = installedUpgradeComponent.Subscribe(684616645, OnInstalledComponentReassigned);
		}

		public void UnsubscribeFromInstalledUpgradeAssignable()
		{
			if (installedUpgradeSubscribeCallbackIDX != -1)
			{
				installedUpgradeComponent.Unsubscribe(installedUpgradeSubscribeCallbackIDX);
				installedUpgradeSubscribeCallbackIDX = -1;
			}
		}

		private void OnInstalledComponentReassigned(object obj)
		{
			IAssignableIdentity arg = ((obj == null) ? null : ((IAssignableIdentity)obj));
			OnInstalledUpgradeReassigned?.Invoke(this, arg);
		}

		private void OnAssignablesChanged(object o)
		{
			if (_lastAssignedUpgradeComponent != assignedUpgradeComponent)
			{
				_lastAssignedUpgradeComponent = assignedUpgradeComponent;
				OnAssignedUpgradeChanged?.Invoke(this);
			}
		}

		private void StartBoosterSM()
		{
			_upgradeSmi = installedUpgradeComponent.StateMachine(master);
			_upgradeSmi.StartSM();
		}

		public void InternalInstall()
		{
			if (!HasUpgradeInstalled && HasUpgradeComponentAssigned)
			{
				storage.Store(assignedUpgradeComponent.gameObject, hide_popups: true);
				installedUpgradePrefabID = assignedUpgradeComponent.PrefabID();
				_installedUpgradeComponent = assignedUpgradeComponent;
				SubscribeToInstallledUpgradeAssignable();
				StartBoosterSM();
				GameObject targetGameObject = assignableSlotInstance.assignables.GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
				if (targetGameObject != null)
				{
					targetGameObject.Trigger(2000325176);
				}
			}
		}

		public void InternalUninstall()
		{
			if (HasUpgradeInstalled)
			{
				UnsubscribeFromInstalledUpgradeAssignable();
				GameObject gameObject = installedUpgradeComponent.gameObject;
				installedUpgradeComponent.Unassign();
				storage.Drop(gameObject);
				installedUpgradePrefabID = Tag.Invalid;
				_installedUpgradeComponent = null;
				if (_upgradeSmi != null)
				{
					_upgradeSmi.StopSM("Uninstall");
					_upgradeSmi = null;
				}
				GameObject targetGameObject = assignableSlotInstance.assignables.GetComponent<MinionAssignablesProxy>().GetTargetGameObject();
				if (targetGameObject != null)
				{
					targetGameObject.Trigger(2000325176);
				}
			}
		}

		public void InternalLock()
		{
			IsLocked = true;
		}

		public void InternalUnlock()
		{
			IsLocked = false;
		}
	}

	public const int MAX_POSSIBLE_SLOT_COUNT = 8;

	public State initialize;

	public State firstSpawn;

	public State inactive;

	public ActiveStates active;

	private Signal UpgradeSlotAssignationChanged;

	private BoolParameter InitialUpgradeSpawned;

	public static void CreateAssignableSlots(MinionAssignablesProxy minionAssignablesProxy)
	{
		AssignableSlot bionicUpgrade = Db.Get().AssignableSlots.BionicUpgrade;
		int num = Mathf.Max(0, 7);
		for (int i = 0; i < num; i++)
		{
			string iDSufix = (i + 2).ToString();
			AddAssignableSlot(bionicUpgrade, iDSufix, minionAssignablesProxy);
		}
	}

	private static void AddAssignableSlot(AssignableSlot bionicUpgradeSlot, string IDSufix, MinionAssignablesProxy minionAssignablesProxy)
	{
		Ownables component = minionAssignablesProxy.GetComponent<Ownables>();
		if (bionicUpgradeSlot is OwnableSlot)
		{
			OwnableSlotInstance ownableSlotInstance = new OwnableSlotInstance(component, (OwnableSlot)bionicUpgradeSlot);
			ownableSlotInstance.ID += IDSufix;
			component.Add(ownableSlotInstance);
		}
		else if (bionicUpgradeSlot is EquipmentSlot)
		{
			Equipment component2 = component.GetComponent<Equipment>();
			EquipmentSlotInstance equipmentSlotInstance = new EquipmentSlotInstance(component2, (EquipmentSlot)bionicUpgradeSlot);
			equipmentSlotInstance.ID += IDSufix;
			component2.Add(equipmentSlotInstance);
		}
	}

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = initialize;
		initialize.Enter(InitializeSlots).EnterTransition(firstSpawn, IsFirstTimeSpawningThisBionic).EnterGoTo(inactive);
		firstSpawn.Enter(SpawnAndInstallInitialUpgrade);
		inactive.EventTransition(GameHashes.BionicOnline, active, IsBionicOnline).Enter(UpdateBatteryMonitorWattageModifiers);
		active.DefaultState(active.idle).EventTransition(GameHashes.BionicOffline, inactive, GameStateMachine<BionicUpgradesMonitor, Instance, IStateMachineTarget, Def>.Not(IsBionicOnline)).EventHandler(GameHashes.BionicUpgradeWattageChanged, UpdateBatteryMonitorWattageModifiers)
			.Enter(UpdateBatteryMonitorWattageModifiers);
		active.idle.OnSignal(UpgradeSlotAssignationChanged, active.seeking, WantsToInstallNewUpgrades);
		active.seeking.OnSignal(UpgradeSlotAssignationChanged, active.idle, DoesNotWantsToInstallNewUpgrades).DefaultState(active.seeking.inProgress);
		active.seeking.inProgress.ToggleChore((Instance smi) => new SeekAndInstallBionicUpgradeChore(smi.master), active.idle, active.seeking.failed);
		active.seeking.failed.EnterTransition(active.idle, DoesNotWantsToInstallNewUpgrades).GoTo(active.seeking.inProgress);
	}

	public static void InitializeSlots(Instance smi)
	{
		smi.InitializeSlots();
	}

	public static bool IsBionicOnline(Instance smi)
	{
		return smi.IsOnline;
	}

	public static bool WantsToInstallNewUpgrades(Instance smi, SignalParameter param)
	{
		return smi.HasAnyUpgradeAssigned;
	}

	public static bool DoesNotWantsToInstallNewUpgrades(Instance smi)
	{
		return DoesNotWantsToInstallNewUpgrades(smi, null);
	}

	public static bool DoesNotWantsToInstallNewUpgrades(Instance smi, SignalParameter param)
	{
		return !WantsToInstallNewUpgrades(smi, param);
	}

	public static bool HasUpgradesInstalled(Instance smi)
	{
		return smi.HasAnyUpgradeInstalled;
	}

	public static bool IsFirstTimeSpawningThisBionic(Instance smi)
	{
		return !smi.sm.InitialUpgradeSpawned.Get(smi);
	}

	public static void UpdateBatteryMonitorWattageModifiers(Instance smi)
	{
		smi.UpdateBatteryMonitorWattageModifiers();
	}

	public static void SpawnAndInstallInitialUpgrade(Instance smi)
	{
		Traits component = smi.GetComponent<Traits>();
		List<string> traitIds = component.GetTraitIds();
		string text = traitIds.Find((string t) => DUPLICANTSTATS.BIONICUPGRADETRAITS.Find((DUPLICANTSTATS.TraitVal st) => st.id == t).id == t);
		if (text != null)
		{
			Tag bionicUpgradePrefabIDWithTraitID = BionicUpgradeComponentConfig.GetBionicUpgradePrefabIDWithTraitID(text);
			GameObject gameObject = Util.KInstantiate(Assets.GetPrefab(bionicUpgradePrefabIDWithTraitID), smi.master.transform.position);
			gameObject.SetActive(value: true);
			IAssignableIdentity component2 = smi.GetComponent<IAssignableIdentity>();
			BionicUpgradeComponent component3 = gameObject.GetComponent<BionicUpgradeComponent>();
			component3.Assign(component2);
			smi.InstallUpgrade(component3);
		}
		smi.sm.InitialUpgradeSpawned.Set(value: true, smi);
		smi.GoTo(smi.sm.inactive);
	}
}
