using System;
using System.Collections.Generic;
using Klei.AI;
using UnityEngine;

public class RobotElectroBankMonitor : GameStateMachine<RobotElectroBankMonitor, RobotElectroBankMonitor.Instance, IStateMachineTarget, RobotElectroBankMonitor.Def>
{
	public class Def : BaseDef
	{
		public float lowBatteryWarningPercent;
	}

	public class PoweredState : State
	{
		public State highBattery;

		public State lowBattery;
	}

	public new class Instance : GameInstance
	{
		public Storage electroBankStorage;

		public Electrobank electrobank;

		public ManualDeliveryKG fetchBatteryChore;

		public AmountInstance bankAmount;

		[MyCmpReq]
		private SymbolOverrideController symbolOverrideController;

		[MyCmpReq]
		private KBatchedAnimController animController;

		private HashedString currentSymbolSwap;

		private HashSet<Tag> batteryTags = new HashSet<Tag>();

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			fetchBatteryChore = GetComponent<ManualDeliveryKG>();
			Storage[] components = master.gameObject.GetComponents<Storage>();
			foreach (Storage storage in components)
			{
				if (storage.storageID == GameTags.ChargedPortableBattery)
				{
					electroBankStorage = storage;
					break;
				}
			}
			foreach (GameObject item in Assets.GetPrefabsWithTag(GameTags.ChargedPortableBattery))
			{
				KPrefabID component = item.GetComponent<KPrefabID>();
				batteryTags.Add(component.PrefabTag);
			}
			bankAmount = Db.Get().Amounts.InternalElectroBank.Lookup(master.gameObject);
			electroBankStorage.Subscribe(-1697596308, ElectroBankStorageChange);
			ElectroBankStorageChange();
			TreeFilterable component2 = GetComponent<TreeFilterable>();
			component2.OnFilterChanged = (Action<HashSet<Tag>>)Delegate.Combine(component2.OnFilterChanged, new Action<HashSet<Tag>>(OnFilterChanged));
		}

		public void ElectroBankStorageChange(object data = null)
		{
			GameObject gameObject = (GameObject)data;
			if (gameObject != null)
			{
				Pickupable component = gameObject.GetComponent<Pickupable>();
				if (component.storage != null && component.storage.storageID == GameTags.ChargedPortableBattery)
				{
					if (electroBankStorage.Count > 0 && electroBankStorage.items[0] != null)
					{
						electrobank = electroBankStorage.items[0].GetComponent<Electrobank>();
						bankAmount.value = electrobank.Charge;
					}
					else
					{
						electrobank = null;
					}
				}
				else if (electroBankStorage.Count <= 0)
				{
					electrobank = null;
					bankAmount.value = 0f;
					DropDischargedElectroBank(gameObject);
				}
				fetchBatteryChore.Pause(electrobank != null && ChargeDecent(this), "Robot has sufficienct electrobank");
				base.sm.hasElectrobank.Set(electrobank != null, this);
			}
			else if (electrobank == null)
			{
				if (electroBankStorage.Count > 0 && electroBankStorage.items[0] != null)
				{
					electrobank = electroBankStorage.items[0].GetComponent<Electrobank>();
					bankAmount.value = electrobank.Charge;
				}
				else
				{
					electrobank = null;
					bankAmount.value = 0f;
				}
				fetchBatteryChore.Pause(electrobank != null && ChargeDecent(this), "Robot has sufficienct electrobank");
				base.sm.hasElectrobank.Set(electrobank != null, this);
			}
		}

		private void DropDischargedElectroBank(GameObject go)
		{
			Electrobank component = go.GetComponent<Electrobank>();
			if (component != null && component.HasTag(GameTags.ChargedPortableBattery) && !component.IsFullyCharged)
			{
				component.RemovePower(component.Charge, dropWhenEmpty: true);
			}
		}

		public void UpdateBatteryState(HashedString newState)
		{
			if (currentSymbolSwap.IsValid)
			{
				symbolOverrideController.RemoveSymbolOverride(currentSymbolSwap);
			}
			KAnim.Build.Symbol symbol = animController.AnimFiles[0].GetData().build.GetSymbol(newState);
			symbolOverrideController.AddSymbolOverride(BATTER_SYMBOL, symbol);
			currentSymbolSwap = newState;
		}

		private void OnFilterChanged(HashSet<Tag> allowed_tags)
		{
			if (!(fetchBatteryChore != null))
			{
				return;
			}
			List<Tag> list = new List<Tag>();
			foreach (Tag batteryTag in batteryTags)
			{
				if (!allowed_tags.Contains(batteryTag))
				{
					list.Add(batteryTag);
				}
			}
			fetchBatteryChore.ForbiddenTags = list.ToArray();
		}
	}

	public static readonly HashedString BATTER_SYMBOL = "meter_target";

	public static readonly HashedString BATTER_FULL_SYMBOL = "battery_full";

	public static readonly HashedString BATTER_LOW_SYMBOL = "battery_low";

	public static readonly HashedString BATTER_DEAD_SYMBOL = "battery_dead";

	public PoweredState powered;

	public State deceased;

	public State powerdown;

	public BoolParameter hasElectrobank;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = powered;
		root.Enter(delegate(Instance smi)
		{
			smi.ElectroBankStorageChange();
		}).TagTransition(GameTags.Dead, deceased).TagTransition(GameTags.Creatures.Die, deceased);
		powered.DefaultState(powered.highBattery).ParamTransition(hasElectrobank, powerdown, GameStateMachine<RobotElectroBankMonitor, Instance, IStateMachineTarget, Def>.IsFalse).Update(delegate(Instance smi, float dt)
		{
			ConsumePower(smi, dt);
		});
		powered.highBattery.Transition(powered.lowBattery, GameStateMachine<RobotElectroBankMonitor, Instance, IStateMachineTarget, Def>.Not(ChargeDecent)).Enter(delegate(Instance smi)
		{
			UpdateBatteryMeter(smi, BATTER_FULL_SYMBOL);
		});
		powered.lowBattery.Enter(delegate(Instance smi)
		{
			RequestBattery(smi);
			UpdateBatteryMeter(smi, BATTER_LOW_SYMBOL);
		}).Transition(powered.highBattery, ChargeDecent).ToggleStatusItem((Instance smi) => Db.Get().RobotStatusItems.LowBatteryNoCharge);
		powerdown.Enter(delegate(Instance smi)
		{
			RequestBattery(smi);
		}).ToggleBehaviour(GameTags.Robots.Behaviours.NoElectroBank, (Instance smi) => true, delegate(Instance smi)
		{
			smi.GoTo(powered);
		});
		deceased.DoNothing();
	}

	private void UpdateBatteryMeter(Instance smi, HashedString symbol)
	{
		smi.UpdateBatteryState(symbol);
	}

	public static bool ChargeDecent(Instance smi)
	{
		float num = 0f;
		foreach (GameObject item in smi.electroBankStorage.items)
		{
			if (!(item == null))
			{
				num += item.GetComponent<Electrobank>().Charge;
			}
		}
		return num >= smi.def.lowBatteryWarningPercent * 120000f;
	}

	public static void ConsumePower(Instance smi, float dt)
	{
		if (smi.electrobank == null)
		{
			RequestBattery(smi);
			return;
		}
		float joules = Mathf.Min(dt * Mathf.Abs(smi.bankAmount.GetDelta()), smi.electrobank.Charge);
		smi.electrobank.RemovePower(joules, dropWhenEmpty: true);
		if (smi.electrobank != null)
		{
			smi.bankAmount.value = smi.electrobank.Charge;
		}
	}

	public static void RequestBattery(Instance smi)
	{
		if (smi.fetchBatteryChore.IsPaused)
		{
			smi.fetchBatteryChore.Pause(smi.electrobank != null && ChargeDecent(smi), "FlydoBattery");
		}
	}
}
