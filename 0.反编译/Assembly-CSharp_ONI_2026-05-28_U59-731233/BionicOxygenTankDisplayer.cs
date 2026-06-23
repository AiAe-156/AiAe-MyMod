using System.Text;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class BionicOxygenTankDisplayer : StandardAmountDisplayer
{
	public BionicOxygenTankDisplayer(GameUtil.UnitClass unitClass, GameUtil.TimeSlice deltaTimeSlice)
		: base(unitClass, deltaTimeSlice)
	{
	}

	public override string GetTooltip(Amount master, AmountInstance instance)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		BionicOxygenTankMonitor.Instance sMI = instance.gameObject.GetSMI<BionicOxygenTankMonitor.Instance>();
		stringBuilder.AppendFormat(master.description, formatter.GetFormattedValue(instance.value));
		stringBuilder.Append("\n\n");
		stringBuilder.AppendFormat(DUPLICANTS.STATS.BIONICOXYGENTANK.TOOLTIP_MASS_LINE, GameUtil.GetFormattedMass(instance.value), GameUtil.GetFormattedMass(instance.GetMax()));
		if (sMI != null)
		{
			foreach (GameObject item in sMI.storage.items)
			{
				if (item != null)
				{
					PrimaryElement component = item.GetComponent<PrimaryElement>();
					if (component != null && component.Mass > 0f)
					{
						string arg = ((component.DiseaseIdx != byte.MaxValue && component.DiseaseCount > 0) ? string.Format(DUPLICANTS.STATS.BIONICOXYGENTANK.TOOLTIP_GERM_DETAIL, GameUtil.GetFormattedDisease(component.DiseaseIdx, component.DiseaseCount)) : "");
						stringBuilder.Append("\n");
						stringBuilder.AppendFormat(DUPLICANTS.STATS.BIONICOXYGENTANK.TOOLTIP_MASS_ROW_DETAIL, component.Element.name, GameUtil.GetFormattedMass(component.Mass), arg);
					}
				}
			}
		}
		stringBuilder.Append("\n\n");
		float num = instance.deltaAttribute.GetTotalDisplayValue();
		if (sMI != null)
		{
			AttributeInstance airConsumptionRate = sMI.airConsumptionRate;
			float totalValue = airConsumptionRate.GetTotalValue();
			num += totalValue;
		}
		stringBuilder.AppendFormat(UI.CHANGEPERSECOND, formatter.GetFormattedValue(num, GameUtil.TimeSlice.PerSecond));
		Debug.Assert(instance.deltaAttribute.Modifiers.Count <= 0, "BionicOxygenTankDisplayer has found an invalid AttributeModifier. This particular Amount should not use AttributeModifiers, the rate of breathing is defined by  Db.Get().Attributes.AirConsumptionRate");
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}
}
