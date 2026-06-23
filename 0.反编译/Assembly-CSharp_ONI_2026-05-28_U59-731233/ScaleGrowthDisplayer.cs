using Klei.AI;
using STRINGS;

public class ScaleGrowthDisplayer : AsPercentAmountDisplayer
{
	public ScaleGrowthDisplayer(GameUtil.TimeSlice deltaTimeSlice)
		: base(deltaTimeSlice)
	{
	}

	public override string GetDescription(Amount master, AmountInstance instance)
	{
		Tag key = instance.gameObject.PrefabID();
		string arg = (CREATURES.STATS.SCALEGROWTH.GET_DISPLAYED_NAME().ContainsKey(key) ? ((string)CREATURES.STATS.SCALEGROWTH.GET_DISPLAYED_NAME()[key]) : master.Name);
		return $"{arg}: {formatter.GetFormattedValue(ToPercent(instance.value, instance))}";
	}

	public override string GetTooltipDescription(Amount master, AmountInstance instance)
	{
		Tag key = instance.gameObject.PrefabID();
		string text = (CREATURES.STATS.SCALEGROWTH.GET_TOOLTIP_PREFIX().ContainsKey(key) ? ((string)CREATURES.STATS.SCALEGROWTH.GET_TOOLTIP_PREFIX()[key]) : "");
		return string.Format(GameUtil.SafeStringFormat(master.description, text), formatter.GetFormattedValue(instance.value));
	}
}
