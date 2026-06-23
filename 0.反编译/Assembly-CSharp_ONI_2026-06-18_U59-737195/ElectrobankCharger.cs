using UnityEngine;

public class ElectrobankCharger : GameStateMachine<ElectrobankCharger, ElectrobankCharger.Instance, IStateMachineTarget, ElectrobankCharger.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		private Storage storage;

		public GameObject targetElectrobank;

		private MeterController meterController;

		public Storage Storage
		{
			get
			{
				if (storage == null)
				{
					storage = GetComponent<Storage>();
				}
				return storage;
			}
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			meterController = new MeterController(GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer);
		}

		public void ChargeInternal(Instance smi, float dt)
		{
			smi.sm.internalChargeAmount.Delta(dt * 400f, smi);
			UpdateMeter();
		}

		public void UpdateMeter()
		{
			meterController.SetPositionPercent(base.sm.internalChargeAmount.Get(base.smi) / 120000f);
		}

		public void TransferChargeToElectrobank()
		{
			targetElectrobank = Electrobank.ReplaceEmptyWithCharged(targetElectrobank, dropFromStorage: true);
			DequeueElectrobank();
		}

		public void DequeueElectrobank()
		{
			targetElectrobank = null;
			base.smi.sm.hasElectrobank.Set(value: false, base.smi);
			base.smi.sm.internalChargeAmount.Set(0f, base.smi);
			UpdateMeter();
		}

		public void QueueElectrobank(object data = null)
		{
			if (targetElectrobank == null)
			{
				for (int i = 0; i < Storage.items.Count; i++)
				{
					GameObject gameObject = Storage.items[i];
					if (gameObject != null && gameObject.HasTag(GameTags.EmptyPortableBattery))
					{
						targetElectrobank = gameObject;
						base.smi.sm.hasElectrobank.Set(value: true, base.smi);
						break;
					}
				}
			}
			UpdateMeter();
		}
	}

	public State noBattery;

	public State inoperational;

	public State charging;

	public State full;

	public FloatParameter internalChargeAmount;

	public BoolParameter hasElectrobank;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = noBattery;
		noBattery.PlayAnim("off").EventHandler(GameHashes.OnStorageChange, delegate(Instance smi, object data)
		{
			smi.QueueElectrobank();
		}).ParamTransition(hasElectrobank, charging, GameStateMachine<ElectrobankCharger, Instance, IStateMachineTarget, Def>.IsTrue)
			.Enter(delegate(Instance smi)
			{
				smi.QueueElectrobank();
			});
		inoperational.PlayAnim("off").EventTransition(GameHashes.OperationalChanged, charging, (Instance smi) => smi.master.GetComponent<Operational>().IsOperational);
		charging.QueueAnim("working_pre").QueueAnim("working_loop", loop: true).Enter(delegate(Instance smi)
		{
			smi.QueueElectrobank();
			smi.master.GetComponent<Operational>().SetActive(value: true);
		})
			.Exit(delegate(Instance smi)
			{
				smi.master.GetComponent<Operational>().SetActive(value: false);
			})
			.ToggleStatusItem(Db.Get().BuildingStatusItems.PowerBankChargerInProgress)
			.Update(delegate(Instance smi, float dt)
			{
				smi.ChargeInternal(smi, dt);
			}, UpdateRate.SIM_EVERY_TICK)
			.EventTransition(GameHashes.OperationalChanged, inoperational, (Instance smi) => !smi.master.GetComponent<Operational>().IsOperational)
			.ParamTransition(internalChargeAmount, full, (Instance smi, float dt) => internalChargeAmount.Get(smi) >= 120000f);
		full.PlayAnim("working_pst").Enter(delegate(Instance smi)
		{
			smi.TransferChargeToElectrobank();
		}).OnAnimQueueComplete(noBattery);
	}
}
