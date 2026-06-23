using System;
using Klei.AI;
using TUNING;
using UnityEngine;

public class BionicOxygenTankMonitor : GameStateMachine<BionicOxygenTankMonitor, BionicOxygenTankMonitor.Instance, IStateMachineTarget, BionicOxygenTankMonitor.Def>
{
	public interface IChore
	{
		bool IsConsumingOxygen();
	}

	public class Def : BaseDef
	{
	}

	public class ChoreState : State
	{
		public State running;

		public State ends;
	}

	public class SeekOxygenStates : State
	{
		public State enableSensors;

		public ChoreState oxygenCanisterMode;

		public ChoreState environmentAbsorbMode;
	}

	public class LowOxygenStates : State
	{
		public State idle;

		public SeekOxygenStates schedule;
	}

	public new class Instance : GameInstance, OxygenBreather.IGasProvider
	{
		public AttributeInstance airConsumptionRate;

		private Schedulable schedulable;

		private AmountInstance oxygenTankAmountInstance;

		private ClosestPickupableSensor<Pickupable>[] oxygenSourceSensors;

		private Pickupable closestOxygenSource;

		private Navigator navigator;

		private float movementRate;

		private AbsorbCellQuery query;

		private OxygenBreather oxygenBreather;

		private MinionBrain brain;

		private MinionStorageDataHolder dataHolder;

		private ChoreDriver choreDriver;

		public bool isRecoveringFromSuffocation = false;

		public bool IsAllowedToSeekOxygenBySchedule => ScheduleManager.Instance.IsAllowed(schedulable, Db.Get().ScheduleBlockTypes.Eat);

		public bool IsEmpty => AvailableOxygen == 0f;

		public float OxygenPercentage => AvailableOxygen / storage.capacityKg;

		public float AvailableOxygen => storage.GetMassAvailable(GameTags.Breathable);

		public float SpaceAvailableInTank => storage.capacityKg - AvailableOxygen;

		public int AbsorbOxygenCell { get; private set; } = Grid.InvalidCell;

		public Storage storage { get; private set; }

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			query = new AbsorbCellQuery();
			NameDisplayScreen.Instance.RegisterComponent(base.gameObject, this);
			Sensors component = GetComponent<Sensors>();
			schedulable = GetComponent<Schedulable>();
			navigator = GetComponent<Navigator>();
			AttributeConverterInstance movementSpeed = Db.Get().AttributeConverters.MovementSpeed.Lookup(navigator.gameObject);
			float movementSpeedMultiplier = BipedTransitionLayer.GetMovementSpeedMultiplier(movementSpeed);
			movementRate = movementSpeedMultiplier / SECONDS_PER_PATH_COST_UNIT;
			oxygenBreather = GetComponent<OxygenBreather>();
			brain = GetComponent<MinionBrain>();
			dataHolder = GetComponent<MinionStorageDataHolder>();
			MinionStorageDataHolder minionStorageDataHolder = dataHolder;
			minionStorageDataHolder.OnCopyBegins = (Action<StoredMinionIdentity>)Delegate.Combine(minionStorageDataHolder.OnCopyBegins, new Action<StoredMinionIdentity>(OnCopyMinionBegins));
			oxygenSourceSensors = new ClosestPickupableSensor<Pickupable>[1] { component.GetSensor<ClosestOxygenCanisterSensor>() };
			for (int i = 0; i < oxygenSourceSensors.Length; i++)
			{
				ClosestPickupableSensor<Pickupable> obj = oxygenSourceSensors[i];
				obj.OnItemChanged = (Action<Pickupable>)Delegate.Combine(obj.OnItemChanged, new Action<Pickupable>(OnOxygenSourceSensorItemChanged));
			}
			storage = base.gameObject.GetComponents<Storage>().FindFirst((Storage s) => s.storageID == GameTags.StoragesIds.BionicOxygenTankStorage);
			oxygenTankAmountInstance = Db.Get().Amounts.BionicOxygenTank.Lookup(base.gameObject);
			airConsumptionRate = Db.Get().Attributes.AirConsumptionRate.Lookup(base.gameObject);
			Storage obj2 = storage;
			obj2.OnStorageChange = (Action<GameObject>)Delegate.Combine(obj2.OnStorageChange, new Action<GameObject>(OnOxygenTankStorageChanged));
			choreDriver = base.gameObject.GetComponent<ChoreDriver>();
		}

		public bool ChoreIsRunning(ChoreType type)
		{
			if (choreDriver == null)
			{
				return false;
			}
			Chore currentChore = choreDriver.GetCurrentChore();
			if (currentChore == null)
			{
				return false;
			}
			return currentChore.choreType == type;
		}

		public bool IsConsumingOxygen()
		{
			choreDriver = base.smi.GetComponent<ChoreDriver>();
			if (choreDriver == null)
			{
				return false;
			}
			if (!(choreDriver.GetCurrentChore() is IChore chore))
			{
				return false;
			}
			return chore.IsConsumingOxygen();
		}

		public Pickupable GetClosestOxygenSource()
		{
			return closestOxygenSource;
		}

		private void OnOxygenSourceSensorItemChanged(object o)
		{
			CompareOxygenSources();
		}

		private void OnOxygenTankStorageChanged(object o)
		{
			RefreshAmountInstance();
		}

		public void RefreshAmountInstance()
		{
			oxygenTankAmountInstance.SetValue(AvailableOxygen);
		}

		public void AddFirstTimeSpawnedOxygen()
		{
			storage.AddElement(SimHashes.Oxygen, storage.capacityKg - AvailableOxygen, INITIAL_OXYGEN_TEMP, byte.MaxValue, 0);
			base.sm.HasSpawnedBefore.Set(value: true, this);
		}

		private void OnCopyMinionBegins(StoredMinionIdentity destination)
		{
			MinionStorageDataHolder.DataPackData dataPackData = new MinionStorageDataHolder.DataPackData();
			dataPackData.Bools = new bool[1] { base.sm.HasSpawnedBefore.Get(this) };
			MinionStorageDataHolder.DataPackData data = dataPackData;
			dataHolder.UpdateData<Instance>(data);
		}

		public override void StartSM()
		{
			base.StartSM();
			RefreshAmountInstance();
		}

		public override void PostParamsInitialized()
		{
			MinionStorageDataHolder.DataPack dataPack = dataHolder.GetDataPack<Instance>();
			if (dataPack != null && dataPack.IsStoringNewData)
			{
				MinionStorageDataHolder.DataPackData dataPackData = dataPack.ReadData();
				if (dataPackData != null)
				{
					bool value = dataPackData.Bools[0];
					base.sm.HasSpawnedBefore.Set(value, this);
				}
			}
			base.PostParamsInitialized();
		}

		private void CompareOxygenSources()
		{
			Pickupable pickupable = null;
			float num = 2.1474836E+09f;
			for (int i = 0; i < oxygenSourceSensors.Length; i++)
			{
				ClosestPickupableSensor<Pickupable> closestPickupableSensor = oxygenSourceSensors[i];
				int itemNavCost = closestPickupableSensor.GetItemNavCost();
				if ((float)itemNavCost < num)
				{
					num = itemNavCost;
					pickupable = closestPickupableSensor.GetItem();
				}
			}
			if (pickupable != null && IsInsideState(base.sm.critical))
			{
				float num2 = num / movementRate;
				float num3 = num2 * oxygenBreather.ConsumptionRate;
				float value = oxygenBreather.GetAmounts().Get(Db.Get().Amounts.Breath).value;
				if (value < num3)
				{
					pickupable = null;
				}
			}
			if (closestOxygenSource != pickupable)
			{
				closestOxygenSource = pickupable;
				base.sm.ClosestOxygenSourceChanged.Trigger(this);
			}
		}

		public void UpdatePotentialCellToAbsorbOxygen(int previouslyReservedCell)
		{
			float breathPercentage = brain.GetAmounts().Get(Db.Get().Amounts.Breath).value / brain.GetAmounts().Get(Db.Get().Amounts.Breath).GetMax();
			query.Reset(brain, AreOxygenLevelsCritical(this), AvailableOxygen, breathPercentage, previouslyReservedCell, isRecoveringFromSuffocation);
			navigator.RunQuery(base.smi.query);
			int num = base.smi.query.GetResultCell();
			if (num == Grid.PosToCell(base.gameObject) && !GasBreatherFromWorldProvider.GetBestBreathableCellAroundSpecificCell(num, GasBreatherFromWorldProvider.DEFAULT_BREATHABLE_OFFSETS, oxygenBreather).IsBreathable)
			{
				num = PathFinder.InvalidCell;
			}
			bool flag = AbsorbOxygenCell != num;
			AbsorbOxygenCell = num;
			if (flag)
			{
				base.sm.AbsorbCellChangedSignal.Trigger(this);
			}
		}

		public float AddGas(Sim.MassConsumedCallback mass_cb_info)
		{
			return AddGas(ElementLoader.elements[mass_cb_info.elemIdx].id, mass_cb_info.mass, mass_cb_info.temperature, mass_cb_info.diseaseIdx, mass_cb_info.diseaseCount);
		}

		public float AddGas(SimHashes element, float mass, float temperature, byte disseaseIDX = byte.MaxValue, int _disseaseCount = 0)
		{
			float num = Mathf.Min(mass, SpaceAvailableInTank);
			float result = mass - num;
			float num2 = num / mass;
			int disease_count = Mathf.CeilToInt((float)_disseaseCount * num2);
			storage.AddElement(element, num, temperature, disseaseIDX, disease_count);
			return result;
		}

		public void SetOxygenSourceSensorsActiveState(bool shouldItBeActive)
		{
			for (int i = 0; i < oxygenSourceSensors.Length; i++)
			{
				ClosestPickupableSensor<Pickupable> closestPickupableSensor = oxygenSourceSensors[i];
				closestPickupableSensor.SetActive(shouldItBeActive);
				if (shouldItBeActive)
				{
					closestPickupableSensor.Update();
				}
			}
		}

		public bool ConsumeGas(OxygenBreather oxygen_breather, float amount)
		{
			if (IsEmpty)
			{
				return false;
			}
			SimHashes mostRelevantItemElement = SimHashes.Vacuum;
			float aggregate_temperature = 0f;
			storage.ConsumeAndGetDisease(GameTags.Breathable, amount, out var _, out var disease_info, out aggregate_temperature, out mostRelevantItemElement);
			OxygenBreather.BreathableGasConsumed(oxygen_breather, mostRelevantItemElement, amount, aggregate_temperature, disease_info.idx, disease_info.count);
			return true;
		}

		public void OnSetOxygenBreather(OxygenBreather oxygen_breather)
		{
		}

		public void OnClearOxygenBreather(OxygenBreather oxygen_breather)
		{
		}

		public bool IsLowOxygen()
		{
			return OxygenPercentage <= 0f;
		}

		public bool HasOxygen()
		{
			return !IsEmpty;
		}

		public bool IsBlocked()
		{
			return false;
		}

		public bool ShouldEmitCO2()
		{
			return false;
		}

		public bool ShouldStoreCO2()
		{
			return false;
		}

		protected override void OnCleanUp()
		{
			if (dataHolder != null)
			{
				MinionStorageDataHolder minionStorageDataHolder = dataHolder;
				minionStorageDataHolder.OnCopyBegins = (Action<StoredMinionIdentity>)Delegate.Remove(minionStorageDataHolder.OnCopyBegins, new Action<StoredMinionIdentity>(OnCopyMinionBegins));
			}
			if (storage != null)
			{
				Storage obj = storage;
				obj.OnStorageChange = (Action<GameObject>)Delegate.Remove(obj.OnStorageChange, new Action<GameObject>(OnOxygenTankStorageChanged));
			}
			base.OnCleanUp();
		}
	}

	public const SimHashes INITIAL_TANK_ELEMENT = SimHashes.Oxygen;

	public static readonly Tag INITIAL_TANK_ELEMENT_TAG = SimHashes.Oxygen.CreateTag();

	public const float SAFE_TRESHOLD = 0.85f;

	public const float CRITICAL_TRESHOLD = 0f;

	public const float OXYGEN_TANK_CAPACITY_IN_SECONDS = 2400f;

	public static readonly float OXYGEN_TANK_CAPACITY_KG = 2400f * DUPLICANTSTATS.BIONICS.BaseStats.OXYGEN_USED_PER_SECOND;

	public static float INITIAL_OXYGEN_TEMP = DUPLICANTSTATS.BIONICS.Temperature.Internal.IDEAL;

	public static float SECONDS_PER_PATH_COST_UNIT = 0.3f;

	public State fistSpawn;

	public State safe;

	public LowOxygenStates low;

	public SeekOxygenStates critical;

	private BoolParameter HasSpawnedBefore;

	public Signal AbsorbCellChangedSignal;

	public Signal OxygenSourceItemLostSignal;

	public Signal ClosestOxygenSourceChanged;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = fistSpawn;
		fistSpawn.ParamTransition(HasSpawnedBefore, safe, GameStateMachine<BionicOxygenTankMonitor, Instance, IStateMachineTarget, Def>.IsTrue).Enter(StartWithFullTank);
		safe.Transition(low, GameStateMachine<BionicOxygenTankMonitor, Instance, IStateMachineTarget, Def>.Not(AreOxygenLevelsSafe));
		low.DefaultState(low.idle);
		low.idle.Transition(critical, AreOxygenLevelsCritical).Transition(safe, AreOxygenLevelsSafe).ScheduleChange(low.schedule, IsAllowedToSeekOxygenBySchedule);
		low.schedule.ToggleUrge(Db.Get().Urges.FindOxygenRefill).DefaultState(low.schedule.enableSensors).Transition(critical, AreOxygenLevelsCriticalAndNotConsumingOxygen)
			.Exit(DisableOxygenSourceSensors);
		low.schedule.enableSensors.Enter(EnableOxygenSourceSensors).GoTo(low.schedule.oxygenCanisterMode);
		low.schedule.oxygenCanisterMode.DefaultState(low.schedule.oxygenCanisterMode.running);
		low.schedule.oxygenCanisterMode.running.ScheduleChange(low.idle, IsNotAllowedToSeekOxygenSourceItemsByScheduleAndSeekChoreHasNotBegun).OnSignal(OxygenSourceItemLostSignal, low.schedule.environmentAbsorbMode, NoOxygenSourceAvailableButAbsorbCellAvailable).OnSignal(AbsorbCellChangedSignal, low.schedule.environmentAbsorbMode, (Instance smi, SignalParameter param) => !FindOxygenSourceChoreIsRunning(smi) && NoOxygenSourceAvailableButAbsorbCellAvailable(smi))
			.Update(UpdateAbsorbCellIfNoOxygenSourceAvailable)
			.ToggleChore((Instance smi) => new FindAndConsumeOxygenSourceChore(smi.master, critical: false), low.schedule.oxygenCanisterMode.ends, low.schedule.oxygenCanisterMode.ends);
		low.schedule.oxygenCanisterMode.ends.EnterTransition(safe, AreOxygenLevelsSafe).GoTo(low.idle);
		low.schedule.environmentAbsorbMode.DefaultState(low.schedule.environmentAbsorbMode.running);
		low.schedule.environmentAbsorbMode.running.ScheduleChange(low.idle, IsNotAllowedToSeekOxygenSourceItemsByScheduleAndAbsorbChoreHasNotBegun).OnSignal(ClosestOxygenSourceChanged, low.schedule.oxygenCanisterMode, OxygenSourceItemAvailableAndAbsorbChoreNotStarted).ToggleChore((Instance smi) => new BionicMassOxygenAbsorbChore(smi.master, critical: false), low.schedule.environmentAbsorbMode.ends, low.schedule.environmentAbsorbMode.ends);
		low.schedule.environmentAbsorbMode.ends.EnterTransition(safe, AreOxygenLevelsSafe).GoTo(low.idle);
		critical.ToggleUrge(Db.Get().Urges.FindOxygenRefill).Exit(DisableOxygenSourceSensors).DefaultState(critical.enableSensors)
			.ToggleExpression(Db.Get().Expressions.RecoverBreath)
			.Update(delegate(Instance smi, float dt)
			{
				if (smi.master.gameObject.GetAmounts().Get("Breath").value <= DUPLICANTSTATS.BIONICS.Breath.SUFFOCATE_AMOUNT)
				{
					smi.isRecoveringFromSuffocation = true;
				}
			})
			.Exit(delegate(Instance smi)
			{
				smi.isRecoveringFromSuffocation = false;
			});
		critical.enableSensors.Enter(EnableOxygenSourceSensors).GoTo(critical.oxygenCanisterMode);
		critical.oxygenCanisterMode.DefaultState(critical.oxygenCanisterMode.running);
		critical.oxygenCanisterMode.running.OnSignal(ClosestOxygenSourceChanged, critical.environmentAbsorbMode, (Instance smi, SignalParameter param) => !FindOxygenSourceChoreIsRunning(smi) && NoOxygenSourceAvailableButAbsorbCellAvailable(smi)).OnSignal(OxygenSourceItemLostSignal, critical.environmentAbsorbMode, NoOxygenSourceAvailableButAbsorbCellAvailable).OnSignal(AbsorbCellChangedSignal, critical.environmentAbsorbMode, (Instance smi, SignalParameter param) => !FindOxygenSourceChoreIsRunning(smi) && NoOxygenSourceAvailableButAbsorbCellAvailable(smi))
			.Update(UpdateAbsorbCellIfNoOxygenSourceAvailable)
			.ToggleChore((Instance smi) => new FindAndConsumeOxygenSourceChore(smi.master, critical: true), critical.oxygenCanisterMode.ends, critical.oxygenCanisterMode.ends);
		critical.oxygenCanisterMode.ends.EnterTransition(low, GameStateMachine<BionicOxygenTankMonitor, Instance, IStateMachineTarget, Def>.Not(AreOxygenLevelsCritical)).GoTo(critical.oxygenCanisterMode.running);
		critical.environmentAbsorbMode.DefaultState(critical.environmentAbsorbMode.running);
		critical.environmentAbsorbMode.running.OnSignal(ClosestOxygenSourceChanged, critical.oxygenCanisterMode, OxygenSourceItemAvailableAndAbsorbChoreNotStarted).ToggleChore((Instance smi) => new BionicMassOxygenAbsorbChore(smi.master, critical: true), critical.environmentAbsorbMode.ends, critical.environmentAbsorbMode.ends);
		critical.environmentAbsorbMode.ends.EnterTransition(low, GameStateMachine<BionicOxygenTankMonitor, Instance, IStateMachineTarget, Def>.Not(AreOxygenLevelsCritical)).GoTo(critical.oxygenCanisterMode);
	}

	public static bool IsAllowedToSeekOxygenBySchedule(Instance smi)
	{
		return smi.IsAllowedToSeekOxygenBySchedule;
	}

	public static bool IsNotAllowedToSeekOxygenSourceItemsByScheduleAndSeekChoreHasNotBegun(Instance smi)
	{
		return !IsAllowedToSeekOxygenBySchedule(smi) && !FindOxygenSourceChoreIsRunning(smi);
	}

	public static bool IsNotAllowedToSeekOxygenSourceItemsByScheduleAndAbsorbChoreHasNotBegun(Instance smi)
	{
		return !IsAllowedToSeekOxygenBySchedule(smi) && !AbsorbChoreIsRunning(smi);
	}

	public static bool AreOxygenLevelsSafe(Instance smi)
	{
		return smi.OxygenPercentage >= 0.85f;
	}

	public static bool AreOxygenLevelsCritical(Instance smi)
	{
		return smi.OxygenPercentage <= 0f;
	}

	public static bool AreOxygenLevelsCriticalAndNotConsumingOxygen(Instance smi)
	{
		return AreOxygenLevelsCritical(smi) && !IsConsumingOxygen(smi);
	}

	public static bool IsThereAnOxygenSourceItemAvailable(Instance smi)
	{
		return smi.GetClosestOxygenSource() != null;
	}

	public static bool AbsorbCellUnavailable(Instance smi)
	{
		return smi.AbsorbOxygenCell == Grid.InvalidCell;
	}

	public static bool AbsorbCellAvailable(Instance smi)
	{
		return smi.AbsorbOxygenCell != Grid.InvalidCell;
	}

	public static bool NoOxygenSourceAvailable(Instance smi)
	{
		return smi.GetClosestOxygenSource() == null;
	}

	public static bool NoOxygenSourceAvailableButAbsorbCellAvailable(Instance smi)
	{
		return NoOxygenSourceAvailableButAbsorbCellAvailable(smi, null);
	}

	public static bool NoOxygenSourceAvailableButAbsorbCellAvailable(Instance smi, SignalParameter param)
	{
		return NoOxygenSourceAvailable(smi) && AbsorbCellAvailable(smi);
	}

	public static bool OxygenSourceItemAvailableAndAbsorbChoreNotStarted(Instance smi, SignalParameter param)
	{
		return IsThereAnOxygenSourceItemAvailable(smi) && !AbsorbChoreIsRunning(smi);
	}

	public static bool AbsorbChoreIsRunning(Instance smi)
	{
		return ChoreIsRunning(smi, Db.Get().ChoreTypes.BionicAbsorbOxygen) || ChoreIsRunning(smi, Db.Get().ChoreTypes.BionicAbsorbOxygen_Critical);
	}

	public static bool FindOxygenSourceChoreIsRunning(Instance smi)
	{
		return ChoreIsRunning(smi, Db.Get().ChoreTypes.FindOxygenSourceItem) || ChoreIsRunning(smi, Db.Get().ChoreTypes.FindOxygenSourceItem_Critical);
	}

	public static bool ChoreIsRunning(Instance smi, ChoreType type)
	{
		return smi.ChoreIsRunning(type);
	}

	public static bool IsConsumingOxygen(Instance smi)
	{
		return smi.IsConsumingOxygen();
	}

	public static void StartWithFullTank(Instance smi)
	{
		smi.AddFirstTimeSpawnedOxygen();
	}

	public static void EnableOxygenSourceSensors(Instance smi)
	{
		smi.SetOxygenSourceSensorsActiveState(shouldItBeActive: true);
	}

	public static void DisableOxygenSourceSensors(Instance smi)
	{
		smi.SetOxygenSourceSensorsActiveState(shouldItBeActive: false);
	}

	public static void UpdateAbsorbCellIfNoOxygenSourceAvailable(Instance smi, float dt)
	{
		if (NoOxygenSourceAvailable(smi))
		{
			smi.UpdatePotentialCellToAbsorbOxygen(Grid.InvalidCell);
		}
	}
}
