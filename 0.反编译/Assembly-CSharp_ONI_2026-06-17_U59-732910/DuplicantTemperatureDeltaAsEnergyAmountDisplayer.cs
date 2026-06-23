using System.Text;
using Klei.AI;
using STRINGS;
using TUNING;

public class DuplicantTemperatureDeltaAsEnergyAmountDisplayer : StandardAmountDisplayer
{
	public DuplicantTemperatureDeltaAsEnergyAmountDisplayer(GameUtil.UnitClass unitClass, GameUtil.TimeSlice timeSlice)
		: base(unitClass, timeSlice)
	{
	}

	public override string GetTooltip(Amount master, AmountInstance instance)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		stringBuilder.AppendFormat(master.description, formatter.GetFormattedValue(instance.value), formatter.GetFormattedValue(DUPLICANTSTATS.STANDARD.Temperature.Internal.IDEAL));
		float num = ElementLoader.FindElementByHash(SimHashes.Creature).specificHeatCapacity * DUPLICANTSTATS.STANDARD.BaseStats.DEFAULT_MASS * 1000f;
		float num2 = 0f;
		float num3 = 0f;
		for (int i = 0; i != instance.deltaAttribute.Modifiers.Count; i++)
		{
			AttributeModifier attributeModifier = instance.deltaAttribute.Modifiers[i];
			float value = attributeModifier.Value;
			if (attributeModifier.GetDescription() == CreatureSimTemperatureTransfer.RESULT_MODIFIER_NAME)
			{
				num2 = value * 5f;
				num3 += value * 5f;
			}
			else
			{
				num3 += value;
			}
		}
		stringBuilder.Append("\n\n");
		if (formatter.DeltaTimeSlice == GameUtil.TimeSlice.PerCycle)
		{
			stringBuilder.AppendFormat(UI.CHANGEPERCYCLE, formatter.GetFormattedValue(num3, GameUtil.TimeSlice.PerCycle));
		}
		else
		{
			stringBuilder.AppendFormat(UI.CHANGEPERSECOND, formatter.GetFormattedValue(num3, GameUtil.TimeSlice.PerSecond));
			stringBuilder.Append("\n");
			stringBuilder.AppendFormat(UI.CHANGEPERSECOND, GameUtil.GetFormattedJoules(num3 * num));
		}
		for (int j = 0; j != instance.deltaAttribute.Modifiers.Count; j++)
		{
			AttributeModifier attributeModifier2 = instance.deltaAttribute.Modifiers[j];
			float num4 = attributeModifier2.Value;
			string description = attributeModifier2.GetDescription();
			if (description == CreatureSimTemperatureTransfer.RESULT_MODIFIER_NAME)
			{
				num4 = num2;
			}
			stringBuilder.Append("\n");
			stringBuilder.AppendFormat(UI.MODIFIER_ITEM_TEMPLATE, description, GameUtil.GetFormattedHeatEnergyRate(num4 * num * 1f));
		}
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}
}
