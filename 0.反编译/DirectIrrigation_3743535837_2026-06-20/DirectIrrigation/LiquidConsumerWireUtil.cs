using UnityEngine;

namespace DirectIrrigation;

internal static class LiquidConsumerWireUtil
{
	public static void WireLiquidPECs(GameObject plant, Storage plotStorage)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)plant == (Object)null || (Object)(object)plotStorage == (Object)null)
		{
			return;
		}
		PassiveElementConsumer[] components = plant.GetComponents<PassiveElementConsumer>();
		if (components == null || components.Length == 0)
		{
			return;
		}
		foreach (PassiveElementConsumer val in components)
		{
			if (!((Object)(object)val == (Object)null))
			{
				Element val2 = ElementLoader.FindElementByHash(((ElementConsumer)val).elementToConsume);
				if (val2 != null && val2.IsLiquid)
				{
					((ElementConsumer)val).storeOnConsume = true;
					((ElementConsumer)val).storage = plotStorage;
					((ElementConsumer)val).showDescriptor = false;
					((ElementConsumer)val).showInStatusPanel = false;
					((ElementConsumer)val).isRequired = false;
					((ElementConsumer)val).EnableConsumption(true);
					((Behaviour)val).enabled = true;
					EnsureLimiter(plant, val, plotStorage, ((ElementConsumer)val).elementToConsume, Mathf.Max(0.1f, ((ElementConsumer)val).capacityKG));
				}
			}
		}
	}

	private static void EnsureLimiter(GameObject plant, PassiveElementConsumer consumer, Storage plotStorage, SimHashes element, float capKg)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		LiquidPECStorageLimiter[] components = plant.GetComponents<LiquidPECStorageLimiter>();
		if (components != null)
		{
			foreach (LiquidPECStorageLimiter liquidPECStorageLimiter in components)
			{
				if ((Object)(object)liquidPECStorageLimiter != (Object)null && (Object)(object)liquidPECStorageLimiter.Consumer == (Object)(object)consumer)
				{
					liquidPECStorageLimiter.Configure(consumer, plotStorage, element, capKg);
					liquidPECStorageLimiter.ApplyNow();
					return;
				}
			}
		}
		LiquidPECStorageLimiter liquidPECStorageLimiter2 = plant.AddComponent<LiquidPECStorageLimiter>();
		liquidPECStorageLimiter2.Configure(consumer, plotStorage, element, capKg);
		liquidPECStorageLimiter2.ApplyNow();
	}
}
