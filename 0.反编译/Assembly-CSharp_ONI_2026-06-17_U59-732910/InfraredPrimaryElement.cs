using UnityEngine;

public class InfraredPrimaryElement : TemperatureOverlayInfraredVisualizerBase
{
	private PrimaryElement primaryElement;

	private KBatchedAnimController controller;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		primaryElement = GetComponent<PrimaryElement>();
		controller = GetComponent<KBatchedAnimController>();
	}

	protected override void TemperatureOverlayInfraredUpdate(Infrared.TemperatureOverlayInfraredData data)
	{
		if (primaryElement != null && controller != null)
		{
			float temperature = primaryElement.Temperature;
			Color32 color = SimDebugView.Instance.NormalizedTemperature(temperature);
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
