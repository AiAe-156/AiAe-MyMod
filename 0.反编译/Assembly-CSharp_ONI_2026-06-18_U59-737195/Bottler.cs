using System;
using KSerialization;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/Bottler")]
public class Bottler : Workable, IUserControlledCapacity
{
	private class Controller : GameStateMachine<Controller, Controller.Instance, Bottler>
	{
		public class OperationalStates : State
		{
			public State empty;

			public State filling;

			public State ready;
		}

		public new class Instance : GameInstance
		{
			public PrimaryElement LastElementStored;

			private Storage storage;

			public MeterController meter { get; private set; }

			public Instance(Bottler master)
				: base(master)
			{
				storage = GetComponent<Storage>();
				meter = new MeterController(GetComponent<KBatchedAnimController>(), "bottle", "off", Meter.Offset.UserSpecified, Grid.SceneLayer.BuildingFront, "bottle", "substance_tinter", "substance_tinter_cap");
				Subscribe(-1697596308, OnStorageChanged);
			}

			public void UpdateMeter()
			{
				UpdateMeterManually((LastElementStored == null) ? base.smi.master.GetFirstPrimaryElement() : LastElementStored);
			}

			public void UpdateMeterManually(PrimaryElement primary_element)
			{
				if (!(primary_element == null))
				{
					meter.meterController.SwapAnims(primary_element.Element.substance.anims);
					meter.meterController.Play(OreSizeVisualizerComponents.GetAnimForMass(primary_element.Mass), KAnim.PlayMode.Paused);
					Color32 colour = primary_element.Element.substance.colour;
					colour.a = byte.MaxValue;
					GameUtil.TintLiquidSymbolOnBuilding("meter_fill", meter.meterController, primary_element.Element);
					GameUtil.TintLiquidSymbolOnBuilding("water1", meter.meterController, primary_element.Element);
					GameUtil.TintLiquidSymbolOnBuilding("substance_tinter", meter.meterController, primary_element.Element);
					meter.SetSymbolTint(new KAnimHashedString("substance_tinter_cap"), colour);
				}
			}

			private void OnStorageChanged(object data)
			{
				GameObject gameObject = (GameObject)data;
				PrimaryElement primaryElement = ((gameObject == null) ? null : gameObject.GetComponent<PrimaryElement>());
				if (primaryElement != null)
				{
					LastElementStored = primaryElement;
				}
			}
		}

		public State nonoperational;

		public OperationalStates operational;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = nonoperational;
			root.Enter(delegate(Instance smi)
			{
				smi.master.storage.allowItemRemoval = false;
			});
			nonoperational.PlayAnim("off").TagTransition(GameTags.Operational, operational);
			operational.EnterTransition(operational.ready, IsFull).DefaultState(operational.empty).TagTransition(GameTags.Operational, nonoperational, on_remove: true);
			operational.empty.PlayAnim("off").EventHandlerTransition(GameHashes.OnStorageChange, operational.filling, (Instance smi, object o) => IsFull(smi));
			operational.filling.PlayAnim("working").Enter(delegate(Instance smi)
			{
				smi.UpdateMeter();
			}).OnAnimQueueComplete(operational.ready);
			operational.ready.EventTransition(GameHashes.OnStorageChange, operational.empty, GameStateMachine<Controller, Instance, Bottler, object>.Not(IsFull)).PlayAnim("ready").Enter(delegate(Instance smi)
			{
				smi.master.storage.allowItemRemoval = true;
			})
				.Exit(delegate(Instance smi)
				{
					smi.master.storage.allowItemRemoval = false;
				})
				.Enter(delegate(Instance smi)
				{
					smi.master.storage.allowItemRemoval = true;
					smi.UpdateMeter();
					foreach (GameObject item in smi.master.storage.items)
					{
						Pickupable component = item.GetComponent<Pickupable>();
						component.targetWorkable = smi.master;
						component.SetOffsets(new CellOffset[1] { smi.master.workCellOffset });
						component.OnReservationsChanged = (Action<Pickupable, bool, Pickupable.Reservation>)Delegate.Combine(component.OnReservationsChanged, new Action<Pickupable, bool, Pickupable.Reservation>(smi.master.OnReservationsChanged));
						component.KPrefabID.AddTag(smi.master.SourceTag);
						item.Trigger(-778359855, (object)smi.master.storage);
					}
				})
				.Exit(delegate(Instance smi)
				{
					smi.master.storage.allowItemRemoval = false;
					foreach (GameObject item2 in smi.master.storage.items)
					{
						Pickupable component = item2.GetComponent<Pickupable>();
						component.targetWorkable = component;
						component.SetOffsetTable(OffsetGroups.InvertedStandardTable);
						component.OnReservationsChanged = (Action<Pickupable, bool, Pickupable.Reservation>)Delegate.Remove(component.OnReservationsChanged, new Action<Pickupable, bool, Pickupable.Reservation>(smi.master.OnReservationsChanged));
						component.KPrefabID.RemoveTag(smi.master.SourceTag);
						component.GetSMI<FetchableMonitor.Instance>()?.SetForceUnfetchable(is_unfetchable: false);
						item2.Trigger(-778359855, (object)smi.master.storage);
					}
				});
		}

		public static bool IsFull(Instance smi)
		{
			if (smi.master.storage.MassStored() >= smi.master.userMaxCapacity)
			{
				return smi.master.userMaxCapacity > 0f;
			}
			return false;
		}
	}

	[MyCmpAdd]
	private CopyBuildingSettings copyBuildingSettings;

	public Storage storage;

	public ConduitConsumer consumer;

	public CellOffset workCellOffset = new CellOffset(0, 0);

	[Serialize]
	public float userMaxCapacity = float.PositiveInfinity;

	private Controller.Instance smi;

	private int storageHandle;

	private MeterController workerMeter;

	private static readonly EventSystem.IntraObjectHandler<Bottler> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<Bottler>(delegate(Bottler component, object data)
	{
		component.OnCopySettings(data);
	});

	public float UserMaxCapacity
	{
		get
		{
			if (consumer != null)
			{
				return Mathf.Min(userMaxCapacity, storage.capacityKg);
			}
			return 0f;
		}
		set
		{
			userMaxCapacity = value;
			SetConsumerCapacity(value);
		}
	}

	public float AmountStored => storage.MassStored();

	public float MinCapacity => 0f;

	public float MaxCapacity => storage.capacityKg;

	public bool WholeValues => false;

	public LocString CapacityUnits => GameUtil.GetCurrentMassUnit();

	private Tag SourceTag
	{
		get
		{
			if (smi.master.consumer.conduitType != ConduitType.Gas)
			{
				return GameTags.LiquidSource;
			}
			return GameTags.GasSource;
		}
	}

	private Tag ElementTag
	{
		get
		{
			if (smi.master.consumer.conduitType != ConduitType.Gas)
			{
				return GameTags.Liquid;
			}
			return GameTags.Gas;
		}
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_bottler_kanim") };
		workAnims = new HashedString[1] { "pick_up" };
		workingPstComplete = null;
		workingPstFailed = null;
		synchronizeAnims = true;
		SetOffsets(new CellOffset[1] { workCellOffset });
		SetWorkTime(overrideAnims[0].GetData().GetAnim("pick_up").totalTime);
		resetProgressOnStop = true;
		showProgressBar = false;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		smi = new Controller.Instance(this);
		smi.StartSM();
		Subscribe(-905833192, OnCopySettingsDelegate);
		UpdateStoredItemState();
		SetConsumerCapacity(userMaxCapacity);
	}

	protected override void OnForcedCleanUp()
	{
		if (base.worker != null)
		{
			ChoreDriver component = base.worker.GetComponent<ChoreDriver>();
			if (component != null)
			{
				component.StopChore();
			}
			else
			{
				base.worker.StopWork();
			}
		}
		if (workerMeter != null)
		{
			CleanupBottleProxyObject();
		}
		base.OnForcedCleanUp();
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		base.OnStartWork(worker);
		CreateBottleProxyObject(worker);
	}

	private void CreateBottleProxyObject(WorkerBase worker)
	{
		if (workerMeter != null)
		{
			CleanupBottleProxyObject();
		}
		PrimaryElement primaryElement = smi.master.GetFirstPrimaryElement();
		WorkerBase.StartWorkInfo startWorkInfo = worker.GetStartWorkInfo();
		if (startWorkInfo != null && startWorkInfo is Pickupable.PickupableStartWorkInfo)
		{
			Pickupable.PickupableStartWorkInfo pickupableStartWorkInfo = (Pickupable.PickupableStartWorkInfo)startWorkInfo;
			if (pickupableStartWorkInfo != null && pickupableStartWorkInfo.originalPickupable != null)
			{
				primaryElement = pickupableStartWorkInfo.originalPickupable.PrimaryElement;
			}
		}
		if (!(primaryElement == null))
		{
			workerMeter = new MeterController(worker.GetComponent<KBatchedAnimController>(), "snapto_chest", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer, "snapto_chest");
			workerMeter.meterController.SwapAnims(primaryElement.Element.substance.anims);
			workerMeter.meterController.Play(OreSizeVisualizerComponents.GetAnimForMass(primaryElement.Mass), KAnim.PlayMode.Paused);
			Color32 colour = primaryElement.Element.substance.colour;
			colour.a = byte.MaxValue;
			GameUtil.TintLiquidSymbolOnBuilding("meter_fill", workerMeter.meterController, primaryElement.Element);
			GameUtil.TintLiquidSymbolOnBuilding("water1", workerMeter.meterController, primaryElement.Element);
			GameUtil.TintLiquidSymbolOnBuilding("substance_tinter", workerMeter.meterController, primaryElement.Element);
			workerMeter.SetSymbolTint(new KAnimHashedString("substance_tinter_cap"), colour);
			smi.UpdateMeterManually(primaryElement);
		}
	}

	private void CleanupBottleProxyObject()
	{
		if (workerMeter != null && !workerMeter.gameObject.IsNullOrDestroyed())
		{
			workerMeter.Unlink();
			workerMeter.gameObject.DeleteObject();
		}
		else
		{
			DebugUtil.DevLogError("Bottler finished work but could not clean up the proxy bottle object. workerMeter=" + workerMeter);
			KCrashReporter.ReportDevNotification("Bottle emptier could not clean up proxy object", Environment.StackTrace);
		}
		workerMeter = null;
	}

	protected override void OnStopWork(WorkerBase worker)
	{
		base.OnStopWork(worker);
		CleanupBottleProxyObject();
	}

	protected override void OnAbortWork(WorkerBase worker)
	{
		base.OnAbortWork(worker);
		GetAnimController().Play("ready");
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		Storage component = worker.GetComponent<Storage>();
		Pickupable.PickupableStartWorkInfo pickupableStartWorkInfo = (Pickupable.PickupableStartWorkInfo)worker.GetStartWorkInfo();
		if (pickupableStartWorkInfo.amount > 0f)
		{
			storage.TransferMass(component, pickupableStartWorkInfo.originalPickupable.KPrefabID.PrefabID(), pickupableStartWorkInfo.amount);
		}
		GameObject gameObject = component.FindFirst(pickupableStartWorkInfo.originalPickupable.KPrefabID.PrefabID());
		if (gameObject != null)
		{
			Pickupable component2 = gameObject.GetComponent<Pickupable>();
			component2.targetWorkable = component2;
			component2.RemoveTag(SourceTag);
			component2.GetSMI<FetchableMonitor.Instance>()?.SetForceUnfetchable(is_unfetchable: false);
			pickupableStartWorkInfo.setResultCb(gameObject);
		}
		else
		{
			pickupableStartWorkInfo.setResultCb(null);
		}
		base.OnCompleteWork(worker);
	}

	private void OnReservationsChanged(Pickupable _ignore, bool _ignore2, Pickupable.Reservation _ignore3)
	{
		bool forceUnfetchable = false;
		foreach (GameObject item in storage.items)
		{
			if (item.GetComponent<Pickupable>().ReservedAmount > 0f)
			{
				forceUnfetchable = true;
				break;
			}
		}
		foreach (GameObject item2 in storage.items)
		{
			item2.GetSMI<FetchableMonitor.Instance>()?.SetForceUnfetchable(forceUnfetchable);
		}
	}

	private void SetConsumerCapacity(float value)
	{
		if (consumer != null)
		{
			consumer.capacityKG = value;
			float num = storage.MassStored() - userMaxCapacity;
			if (num > 0f)
			{
				storage.DropSome(storage.FindFirstWithMass(smi.master.ElementTag).ElementID.CreateTag(), num, ventGas: false, dumpLiquid: false, new Vector3(0.8f, 0f, 0f));
			}
		}
	}

	protected override void OnCleanUp()
	{
		if (smi != null)
		{
			smi.StopSM("OnCleanUp");
		}
		base.OnCleanUp();
	}

	private PrimaryElement GetFirstPrimaryElement()
	{
		for (int i = 0; i < storage.Count; i++)
		{
			GameObject gameObject = storage[i];
			if (!(gameObject == null))
			{
				PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
				if (!(component == null))
				{
					return component;
				}
			}
		}
		return null;
	}

	private void UpdateStoredItemState()
	{
		storage.allowItemRemoval = smi != null && smi.GetCurrentState() == smi.sm.operational.ready;
		foreach (GameObject item in storage.items)
		{
			if (item != null)
			{
				item.Trigger(-778359855, (object)storage);
			}
		}
	}

	private void OnCopySettings(object data)
	{
		Bottler component = ((GameObject)data).GetComponent<Bottler>();
		UserMaxCapacity = component.UserMaxCapacity;
	}
}
