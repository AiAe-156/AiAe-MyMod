using System.Text;
using Klei.AI;
using STRINGS;

public class MaturityDisplayer : AsPercentAmountDisplayer
{
	public class MaturityAttributeFormatter : StandardAttributeFormatter
	{
		public MaturityAttributeFormatter()
			: base(GameUtil.UnitClass.Percent, GameUtil.TimeSlice.None)
		{
		}

		public override string GetFormattedModifier(AttributeModifier modifier)
		{
			float num = modifier.Value;
			GameUtil.TimeSlice timeSlice = base.DeltaTimeSlice;
			if (modifier.IsMultiplier)
			{
				num *= 100f;
				timeSlice = GameUtil.TimeSlice.None;
			}
			return GetFormattedValue(num, timeSlice);
		}
	}

	public MaturityDisplayer()
		: base(GameUtil.TimeSlice.PerCycle)
	{
		formatter = new MaturityAttributeFormatter();
	}

	public override string GetTooltipDescription(Amount master, AmountInstance instance)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		stringBuilder.Append(base.GetTooltipDescription(master, instance));
		Growing component = instance.gameObject.GetComponent<Growing>();
		if (component.IsGrowing())
		{
			float seconds = (instance.GetMax() - instance.value) / instance.GetDelta();
			if (component != null && component.IsGrowing())
			{
				stringBuilder.AppendFormat(CREATURES.STATS.MATURITY.TOOLTIP_GROWING_CROP, GameUtil.GetFormattedCycles(seconds), GameUtil.GetFormattedCycles(component.TimeUntilNextHarvest()));
			}
			else
			{
				stringBuilder.AppendFormat(CREATURES.STATS.MATURITY.TOOLTIP_GROWING, GameUtil.GetFormattedCycles(seconds));
			}
		}
		else if (component.ReachedNextHarvest())
		{
			stringBuilder.Append(CREATURES.STATS.MATURITY.TOOLTIP_GROWN);
		}
		else
		{
			stringBuilder.Append(CREATURES.STATS.MATURITY.TOOLTIP_STALLED);
		}
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public override string GetDescription(Amount master, AmountInstance instance)
	{
		Growing component = instance.gameObject.GetComponent<Growing>();
		if (component != null && component.IsGrowing())
		{
			return string.Format(CREATURES.STATS.MATURITY.AMOUNT_DESC_FMT, master.Name, formatter.GetFormattedValue(ToPercent(instance.value, instance)), GameUtil.GetFormattedCycles(component.TimeUntilNextHarvest()));
		}
		return base.GetDescription(master, instance);
	}
}
