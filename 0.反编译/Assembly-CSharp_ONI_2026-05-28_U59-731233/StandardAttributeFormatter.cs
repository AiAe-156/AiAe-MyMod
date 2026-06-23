using System.Collections.Generic;
using System.Text;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class StandardAttributeFormatter : IAttributeFormatter
{
	public GameUtil.UnitClass unitClass;

	public GameUtil.TimeSlice DeltaTimeSlice { get; set; }

	public StandardAttributeFormatter(GameUtil.UnitClass unitClass, GameUtil.TimeSlice deltaTimeSlice)
	{
		this.unitClass = unitClass;
		DeltaTimeSlice = deltaTimeSlice;
	}

	public virtual string GetFormattedAttribute(AttributeInstance instance)
	{
		return GetFormattedValue(instance.GetTotalDisplayValue());
	}

	public virtual string GetFormattedModifier(AttributeModifier modifier)
	{
		return GetFormattedValue(modifier.Value, modifier.OverrideTimeSlice.HasValue ? modifier.OverrideTimeSlice.Value : DeltaTimeSlice);
	}

	public virtual string GetFormattedValue(float value, GameUtil.TimeSlice timeSlice = GameUtil.TimeSlice.None)
	{
		return unitClass switch
		{
			GameUtil.UnitClass.SimpleInteger => GameUtil.GetFormattedInt(value, timeSlice), 
			GameUtil.UnitClass.Mass => GameUtil.GetFormattedMass(value, timeSlice), 
			GameUtil.UnitClass.Temperature => GameUtil.GetFormattedTemperature(value, timeSlice, (timeSlice != GameUtil.TimeSlice.None) ? GameUtil.TemperatureInterpretation.Relative : GameUtil.TemperatureInterpretation.Absolute), 
			GameUtil.UnitClass.Percent => GameUtil.GetFormattedPercent(value, timeSlice), 
			GameUtil.UnitClass.Calories => GameUtil.GetFormattedCalories(value, timeSlice), 
			GameUtil.UnitClass.Distance => GameUtil.GetFormattedDistance(value), 
			GameUtil.UnitClass.Disease => GameUtil.GetFormattedDiseaseAmount(Mathf.RoundToInt(value)), 
			GameUtil.UnitClass.Radiation => GameUtil.GetFormattedRads(value, timeSlice), 
			GameUtil.UnitClass.Energy => GameUtil.GetFormattedJoules(value, "F1", timeSlice), 
			GameUtil.UnitClass.Power => GameUtil.GetFormattedWattage(value), 
			GameUtil.UnitClass.Lux => GameUtil.GetFormattedLux(Mathf.FloorToInt(value)), 
			GameUtil.UnitClass.Time => GameUtil.GetFormattedCycles(value), 
			GameUtil.UnitClass.Seconds => GameUtil.GetFormattedTime(value), 
			GameUtil.UnitClass.Cycles => GameUtil.GetFormattedCycles(value * 600f), 
			_ => GameUtil.GetFormattedSimple(value, timeSlice), 
		};
	}

	public virtual string GetTooltipDescription(Attribute master)
	{
		return master.Description;
	}

	public virtual string GetTooltip(Attribute master, AttributeInstance instance)
	{
		List<AttributeModifier> list = new List<AttributeModifier>();
		for (int i = 0; i < instance.Modifiers.Count; i++)
		{
			list.Add(instance.Modifiers[i]);
		}
		return GetTooltip(master, list, instance.GetComponent<AttributeConverters>());
	}

	public string GetTooltip(Attribute master, List<AttributeModifier> modifiers, AttributeConverters converters)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		stringBuilder.Append(GetTooltipDescription(master));
		stringBuilder.AppendFormat(DUPLICANTS.ATTRIBUTES.TOTAL_VALUE, GetFormattedValue(AttributeInstance.GetTotalDisplayValue(master, modifiers)), master.Name);
		if (master.BaseValue != 0f)
		{
			stringBuilder.AppendFormat(DUPLICANTS.ATTRIBUTES.BASE_VALUE, master.BaseValue);
		}
		List<AttributeModifier> list = new List<AttributeModifier>(modifiers);
		list.Sort((AttributeModifier p1, AttributeModifier p2) => p2.Value.CompareTo(p1.Value));
		for (int num = 0; num != list.Count; num++)
		{
			AttributeModifier attributeModifier = list[num];
			string formattedString = attributeModifier.GetFormattedString();
			if (formattedString != null)
			{
				stringBuilder.AppendFormat(DUPLICANTS.ATTRIBUTES.MODIFIER_ENTRY, attributeModifier.GetDescription(), formattedString);
			}
		}
		bool flag = true;
		if (converters != null && master.converters.Count > 0)
		{
			foreach (AttributeConverterInstance converter in converters.converters)
			{
				if (converter.converter.attribute != master)
				{
					continue;
				}
				string text = converter.DescriptionFromAttribute(converter.Evaluate(), converter.gameObject);
				if (text != null)
				{
					if (flag)
					{
						stringBuilder.AppendLine();
						flag = false;
					}
					stringBuilder.AppendLine();
					stringBuilder.Append(text);
				}
			}
		}
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}
}
