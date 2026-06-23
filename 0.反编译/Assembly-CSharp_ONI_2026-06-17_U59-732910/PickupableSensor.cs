public class PickupableSensor : Sensor
{
	private Navigator navigator;

	private WorkerBase worker;

	public PickupableSensor(Sensors sensors)
		: base(sensors)
	{
		worker = GetComponent<WorkerBase>();
		navigator = GetComponent<Navigator>();
	}

	public override void Update()
	{
		GlobalChoreProvider.Instance.UpdateFetches(navigator);
		Game.Instance.fetchManager.UpdatePickups(navigator, worker);
	}
}
