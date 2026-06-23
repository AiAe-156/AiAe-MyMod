using KSerialization;

[SerializationConfig(MemberSerialization.OptIn)]
public class OperationalControlledSwitch : CircuitSwitch
{
	private static readonly EventSystem.IntraObjectHandler<OperationalControlledSwitch> OnOperationalChangedDelegate = new EventSystem.IntraObjectHandler<OperationalControlledSwitch>(delegate(OperationalControlledSwitch component, object data)
	{
		component.OnOperationalChanged(data);
	});

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		manuallyControlled = false;
		Subscribe(-592767678, OnOperationalChangedDelegate);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
	}

	private void OnOperationalChanged(object data)
	{
		bool value = ((Boxed<bool>)data).value;
		SetState(value);
	}
}
