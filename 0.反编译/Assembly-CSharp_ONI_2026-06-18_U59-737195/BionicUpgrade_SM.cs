using System;

public class BionicUpgrade_SM<SMType, StateMachineInstanceType> : GameStateMachine<SMType, StateMachineInstanceType, IStateMachineTarget, BionicUpgrade_SM<SMType, StateMachineInstanceType>.Def> where SMType : GameStateMachine<SMType, StateMachineInstanceType, IStateMachineTarget, BionicUpgrade_SM<SMType, StateMachineInstanceType>.Def> where StateMachineInstanceType : BionicUpgrade_SM<SMType, StateMachineInstanceType>.BaseInstance
{
	public class Def : BaseDef
	{
		public string UpgradeID;

		public Func<Instance, Instance>[] StateMachinesWhenActive;

		public Def(string upgradeID)
		{
			UpgradeID = upgradeID;
		}

		public virtual string GetDescription()
		{
			return "";
		}
	}

	public abstract class BaseInstance : GameInstance, BionicUpgradeComponent.IWattageController
	{
		protected BionicBedTimeMonitor.Instance bedTimeMonitor;

		protected BionicBatteryMonitor.Instance batteryMonitor;

		protected BionicUpgradeComponent upgradeComponent;

		public bool IsInBedTimeChore
		{
			get
			{
				if (bedTimeMonitor == null)
				{
					return false;
				}
				return bedTimeMonitor.IsBedTimeChoreRunning;
			}
		}

		public bool IsOnline
		{
			get
			{
				if (batteryMonitor == null)
				{
					return false;
				}
				return batteryMonitor.IsOnline;
			}
		}

		public BionicUpgradeComponentConfig.BionicUpgradeData Data => BionicUpgradeComponentConfig.UpgradesData[base.def.UpgradeID];

		public BaseInstance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			batteryMonitor = base.gameObject.GetSMI<BionicBatteryMonitor.Instance>();
			bedTimeMonitor = base.gameObject.GetSMI<BionicBedTimeMonitor.Instance>();
			RegisterMonitorToUpgradeComponent();
		}

		private void RegisterMonitorToUpgradeComponent()
		{
			BionicUpgradesMonitor.UpgradeComponentSlot[] upgradeComponentSlots = base.gameObject.GetSMI<BionicUpgradesMonitor.Instance>().upgradeComponentSlots;
			foreach (BionicUpgradesMonitor.UpgradeComponentSlot upgradeComponentSlot in upgradeComponentSlots)
			{
				if (upgradeComponentSlot.HasUpgradeInstalled)
				{
					BionicUpgradeComponent installedUpgradeComponent = upgradeComponentSlot.installedUpgradeComponent;
					if (installedUpgradeComponent != null && !installedUpgradeComponent.HasWattageController)
					{
						upgradeComponent = installedUpgradeComponent;
						installedUpgradeComponent.SetWattageController(this);
						break;
					}
				}
			}
		}

		private void UnregisterMonitorToUpgradeComponent()
		{
			if (upgradeComponent != null)
			{
				upgradeComponent.SetWattageController(null);
			}
		}

		public abstract float GetCurrentWattageCost();

		public abstract string GetCurrentWattageCostName();

		protected override void OnCleanUp()
		{
			UnregisterMonitorToUpgradeComponent();
			base.OnCleanUp();
		}
	}

	public State Active;

	public State Inactive;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = Inactive;
	}

	public static bool IsOnline(BaseInstance smi)
	{
		return smi.IsOnline;
	}

	public static bool IsInBedTimeChore(BaseInstance smi)
	{
		return smi.IsInBedTimeChore;
	}
}
