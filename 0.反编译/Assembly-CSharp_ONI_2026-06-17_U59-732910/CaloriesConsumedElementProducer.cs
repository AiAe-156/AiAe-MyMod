using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class CaloriesConsumedElementProducer : KMonoBehaviour, IGameObjectEffectDescriptor
{
	public SimHashes producedElement;

	public float kgProducedPerKcalConsumed = 1f;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		CaloriesConsumedSecondaryExcretionMonitor.Instance instance = new CaloriesConsumedSecondaryExcretionMonitor.Instance(base.gameObject.GetComponent<StateMachineController>());
		instance.sm.producedElement = producedElement;
		instance.sm.kgProducedPerKcalConsumed = kgProducedPerKcalConsumed;
		instance.StartSM();
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		return new List<Descriptor>
		{
			new Descriptor(UI.BUILDINGEFFECTS.DIET_ADDITIONAL_PRODUCED.Replace("{Items}", ElementLoader.GetElement(producedElement.CreateTag()).name), UI.BUILDINGEFFECTS.TOOLTIPS.DIET_ADDITIONAL_PRODUCED.Replace("{Items}", ElementLoader.GetElement(producedElement.CreateTag()).name))
		};
	}
}
