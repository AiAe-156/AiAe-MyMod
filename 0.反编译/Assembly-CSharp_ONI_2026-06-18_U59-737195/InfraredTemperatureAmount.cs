using Klei.AI;
using UnityEngine;

public class InfraredTemperatureAmount : TemperatureOverlayInfraredVisualizerBase
{
	private AmountInstance temperatureAmount;

	private KBatchedAnimController controller;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		temperatureAmount = Db.Get().Amounts.Temperature.Lookup(base.gameObject);
		controller = GetComponent<KBatchedAnimController>();
	}

	protected override void TemperatureOverlayInfraredUpdate(Infrared.TemperatureOverlayInfraredData data)
	{
		if (temperatureAmount != null && controller != null)
		{
			float value = temperatureAmount.value;
			Color32 color = SimDebugView.Instance.NormalizedTemperature(value);
			controller.OverlayColour = color;
		}
	}

	protected override void TemperatureOverlayInfraredClear()
	{
		if (controller != null)
		{
			controller.OverlayColour = Color.black;
		}
	}
}
