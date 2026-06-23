using KSerialization;

[SerializationConfig(MemberSerialization.OptIn)]
public class WaterPurifier : StateMachineComponent<WaterPurifier.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, WaterPurifier, object>.GameInstance
	{
		public StatesInstance(WaterPurifier smi)
			: base(smi)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, WaterPurifier>
	{
		public class OnStates : State
		{
			public State waiting;

			public State working_pre;

			public State working;

			public State working_pst;
		}

		public State off;

		public OnStates on;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = off;
			off.PlayAnim("off").EventTransition(GameHashes.OperationalChanged, on, (StatesInstance smi) => smi.master.operational.IsOperational);
			on.PlayAnim("on").EventTransition(GameHashes.OperationalChanged, off, (StatesInstance smi) => !smi.master.operational.IsOperational).DefaultState(on.waiting);
			on.waiting.EventTransition(GameHashes.OnStorageChange, on.working_pre, (StatesInstance smi) => smi.master.AnyConverterCanStart());
			on.working_pre.PlayAnim("working_pre").OnAnimQueueComplete(on.working);
			on.working.Enter(delegate(StatesInstance smi)
			{
				smi.master.UpdateConverterStatusVisibility();
				smi.master.operational.SetActive(value: true);
			}).QueueAnim("working_loop", loop: true).EventTransition(GameHashes.OnStorageChange, on.working_pst, (StatesInstance smi) => !smi.master.AnyConverterCanConvert())
				.EventHandler(GameHashes.OnStorageChange, delegate(StatesInstance smi)
				{
					smi.master.UpdateConverterStatusVisibility();
				})
				.Exit(delegate(StatesInstance smi)
				{
					smi.master.operational.SetActive(value: false);
				});
			on.working_pst.PlayAnim("working_pst").OnAnimQueueComplete(on.waiting);
		}
	}

	[MyCmpGet]
	private Operational operational;

	private ManualDeliveryKG[] deliveryComponents;

	private ElementConverter[] converters;

	private static readonly EventSystem.IntraObjectHandler<WaterPurifier> OnConduitConnectionChangedDelegate = new EventSystem.IntraObjectHandler<WaterPurifier>(delegate(WaterPurifier component, object data)
	{
		component.OnConduitConnectionChanged(data);
	});

	private bool AnyConverterCanStart()
	{
		ElementConverter[] array = converters;
		foreach (ElementConverter elementConverter in array)
		{
			if (elementConverter.HasEnoughMassToStartConverting())
			{
				return true;
			}
		}
		return false;
	}

	private bool AnyConverterCanConvert()
	{
		ElementConverter[] array = converters;
		foreach (ElementConverter elementConverter in array)
		{
			if (elementConverter.CanConvertAtAll())
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateConverterStatusVisibility()
	{
		ElementConverter[] array = converters;
		foreach (ElementConverter elementConverter in array)
		{
			bool flag = elementConverter.CanConvertAtAll();
			if (elementConverter.ShowInUI != flag)
			{
				elementConverter.smi.RemoveStatusItems();
				elementConverter.ShowInUI = flag;
				elementConverter.smi.AddStatusItems();
			}
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		deliveryComponents = GetComponents<ManualDeliveryKG>();
		converters = GetComponents<ElementConverter>();
		OnConduitConnectionChanged(GetComponent<ConduitConsumer>().IsConnected);
		Subscribe(-2094018600, OnConduitConnectionChangedDelegate);
		base.smi.StartSM();
	}

	private void OnConduitConnectionChanged(object data)
	{
		OnConduitConnectionChanged(((Boxed<bool>)data).value);
	}

	private void OnConduitConnectionChanged(bool is_connected)
	{
		ManualDeliveryKG[] array = deliveryComponents;
		foreach (ManualDeliveryKG manualDeliveryKG in array)
		{
			Element element = ElementLoader.GetElement(manualDeliveryKG.RequestedItemTag);
			if (element != null && element.IsLiquid)
			{
				manualDeliveryKG.Pause(is_connected, "pipe connected");
			}
		}
	}
}
