using System.Collections.Generic;
using UnityEngine;

namespace DirectIrrigation;

public sealed class LiquidPECStorageLimiter : KMonoBehaviour, ISim4000ms
{
	private Storage _plotStorage;

	private Tag _elementTag;

	private SimHashes _elementHash;

	private float _capKg = 100f;

	private const float Hysteresis = 1f;

	public PassiveElementConsumer Consumer { get; private set; }

	public void Configure(PassiveElementConsumer consumer, Storage plotStorage, SimHashes element, float capKg)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Consumer = consumer;
		_plotStorage = plotStorage;
		_elementHash = element;
		_elementTag = GameTagExtensions.CreateTag(element);
		_capKg = Mathf.Max(0.1f, capKg);
	}

	public void ApplyNow()
	{
		EvaluateAndToggle();
	}

	public void Sim4000ms(float dt)
	{
		EvaluateAndToggle();
	}

	private void EvaluateAndToggle()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Consumer == (Object)null || (Object)(object)_plotStorage == (Object)null)
		{
			return;
		}
		float elementMassInStorage = GetElementMassInStorage(_plotStorage, _elementTag, _elementHash);
		if (elementMassInStorage >= _capKg)
		{
			if (((Behaviour)Consumer).enabled)
			{
				((ElementConsumer)Consumer).EnableConsumption(false);
				((Behaviour)Consumer).enabled = false;
			}
		}
		else if (elementMassInStorage <= _capKg - 1f && !((Behaviour)Consumer).enabled)
		{
			((ElementConsumer)Consumer).EnableConsumption(true);
			((Behaviour)Consumer).enabled = true;
		}
	}

	private static float GetElementMassInStorage(Storage storage, Tag elementTag, SimHashes elementHash)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		float amountAvailable = storage.GetAmountAvailable(elementTag);
		if (!float.IsNaN(amountAvailable) && amountAvailable >= 0f)
		{
			return amountAvailable;
		}
		float num = 0f;
		List<GameObject> items = storage.items;
		if (items != null)
		{
			for (int i = 0; i < items.Count; i++)
			{
				GameObject val = items[i];
				if (!((Object)(object)val == (Object)null))
				{
					PrimaryElement component = val.GetComponent<PrimaryElement>();
					if (!((Object)(object)component == (Object)null) && component.ElementID == elementHash)
					{
						num += component.Mass;
					}
				}
			}
		}
		return num;
	}
}
