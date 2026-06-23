using Klei.AI;
using UnityEngine;

public struct InfraredVisualizerData
{
	public KAnimControllerBase controller;

	public AmountInstance temperatureAmount;

	public HandleVector<int>.Handle structureTemperature;

	public PrimaryElement primaryElement;

	public TemperatureVulnerable temperatureVulnerable;

	public CritterTemperatureMonitor.Instance critterTemperatureMonitorInstance;

	public void Update()
	{
	}

	public InfraredVisualizerData(GameObject go)
	{
		controller = null;
		temperatureAmount = null;
		structureTemperature = HandleVector<int>.InvalidHandle;
		primaryElement = null;
		temperatureVulnerable = null;
		critterTemperatureMonitorInstance = null;
	}
}
