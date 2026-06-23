using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class MilkProductionDisplayer : AsPercentAmountDisplayer, IVariableImageAmountDisplayer, IAmountDisplayer
{
	public Dictionary<Tag, string> IconPerElement = new Dictionary<Tag, string>();

	public MilkProductionDisplayer(GameUtil.TimeSlice deltaTimeSlice)
		: base(deltaTimeSlice)
	{
	}

	public MilkProductionDisplayer(GameUtil.TimeSlice deltaTimeSlice, Dictionary<Tag, string> customIconsPerElement)
		: base(deltaTimeSlice)
	{
		IconPerElement = customIconsPerElement;
	}

	public override string GetDescription(Amount master, AmountInstance instance)
	{
		Element element = ElementLoader.FindElementByHash(instance.gameObject.GetSMI<MilkProductionMonitor.Instance>().def.element);
		return $"{GameUtil.SafeStringFormat(CREATURES.STATS.MILKPRODUCTION.DISPLAYED_NAME, element.name)}: {formatter.GetFormattedValue(ToPercent(instance.value, instance))}";
	}

	public override string GetTooltipDescription(Amount master, AmountInstance instance)
	{
		Element element = ElementLoader.FindElementByHash(instance.gameObject.GetSMI<MilkProductionMonitor.Instance>().def.element);
		return string.Format(GameUtil.SafeStringFormat(master.description, element.name), formatter.GetFormattedValue(instance.value));
	}

	public Sprite GetIcon(Amount master, AmountInstance instance)
	{
		Element element = ElementLoader.FindElementByHash(instance.gameObject.GetSMI<MilkProductionMonitor.Instance>().def.element);
		if (IconPerElement.TryGetValue(element.tag, out var value))
		{
			return Assets.GetSprite(value);
		}
		return Assets.GetSprite(master.uiSprite);
	}
}
