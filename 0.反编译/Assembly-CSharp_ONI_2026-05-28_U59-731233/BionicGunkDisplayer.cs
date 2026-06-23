using System.Text;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class BionicGunkDisplayer : AsPercentAmountDisplayer
{
	public BionicGunkDisplayer(GameUtil.TimeSlice deltaTimeSlice)
		: base(deltaTimeSlice)
	{
	}

	public override string GetTooltip(Amount master, AmountInstance instance)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		BionicOilMonitor.Instance sMI = instance.gameObject.GetSMI<BionicOilMonitor.Instance>();
		AmountInstance amountInstance = sMI?.oilAmount;
		stringBuilder.AppendFormat(master.description, formatter.GetFormattedValue(instance.value));
		stringBuilder.Append("\n\n");
		float num = instance.deltaAttribute.GetTotalDisplayValue();
		if (sMI != null)
		{
			float totalDisplayValue = amountInstance.deltaAttribute.GetTotalDisplayValue();
			if (totalDisplayValue < 0f)
			{
				num += Mathf.Abs(totalDisplayValue);
			}
		}
		if (formatter.DeltaTimeSlice == GameUtil.TimeSlice.PerCycle)
		{
			stringBuilder.AppendFormat(UI.CHANGEPERCYCLE, formatter.GetFormattedValue(ToPercent(num, instance), GameUtil.TimeSlice.PerCycle));
		}
		else
		{
			stringBuilder.AppendFormat(UI.CHANGEPERSECOND, formatter.GetFormattedValue(ToPercent(num, instance), GameUtil.TimeSlice.PerSecond));
		}
		if (sMI != null)
		{
			for (int i = 0; i != amountInstance.deltaAttribute.Modifiers.Count; i++)
			{
				AttributeModifier attributeModifier = amountInstance.deltaAttribute.Modifiers[i];
				float modifierContribution = amountInstance.deltaAttribute.GetModifierContribution(attributeModifier);
				if (modifierContribution < 0f)
				{
					float value = Mathf.Abs(modifierContribution);
					stringBuilder.Append("\n");
					stringBuilder.AppendFormat(UI.MODIFIER_ITEM_TEMPLATE, attributeModifier.GetDescription(), formatter.GetFormattedValue(ToPercent(value, instance), formatter.DeltaTimeSlice));
				}
			}
		}
		for (int j = 0; j != instance.deltaAttribute.Modifiers.Count; j++)
		{
			AttributeModifier attributeModifier2 = instance.deltaAttribute.Modifiers[j];
			float modifierContribution2 = instance.deltaAttribute.GetModifierContribution(attributeModifier2);
			stringBuilder.Append("\n");
			stringBuilder.AppendFormat(UI.MODIFIER_ITEM_TEMPLATE, attributeModifier2.GetDescription(), formatter.GetFormattedValue(ToPercent(modifierContribution2, instance), formatter.DeltaTimeSlice));
		}
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}
}
