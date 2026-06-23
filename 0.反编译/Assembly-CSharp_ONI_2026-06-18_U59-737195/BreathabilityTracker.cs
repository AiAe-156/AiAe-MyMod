using UnityEngine;

public class BreathabilityTracker : WorldTracker
{
	public BreathabilityTracker(int worldID)
		: base(worldID)
	{
	}

	public override void UpdateData()
	{
		float num = 0f;
		if (Components.LiveMinionIdentities.GetWorldItems(base.WorldID).Count == 0)
		{
			AddPoint(0f);
			return;
		}
		int num2 = 0;
		foreach (MinionIdentity worldItem in Components.LiveMinionIdentities.GetWorldItems(base.WorldID))
		{
			OxygenBreather component = worldItem.GetComponent<OxygenBreather>();
			if (component == null)
			{
				continue;
			}
			OxygenBreather.IGasProvider currentGasProvider = component.GetCurrentGasProvider();
			num2++;
			if (!component.IsOutOfOxygen)
			{
				num += 100f;
				if (currentGasProvider.IsLowOxygen())
				{
					num -= 50f;
				}
			}
		}
		num /= (float)num2;
		AddPoint(Mathf.RoundToInt(num));
	}

	public override string FormatValueString(float value)
	{
		return value + "%";
	}
}
