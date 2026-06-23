using System.Text;
using Klei.AI;
using STRINGS;

public class StandardAmountDisplayer : IAmountDisplayer
{
	protected StandardAttributeFormatter formatter;

	protected StandardAttributeFormatter deltaFormatter;

	public GameUtil.IdentityDescriptorTense tense;

	public IAttributeFormatter Formatter => formatter;

	public GameUtil.TimeSlice DeltaTimeSlice
	{
		get
		{
			return formatter.DeltaTimeSlice;
		}
		set
		{
			formatter.DeltaTimeSlice = value;
		}
	}

	public StandardAmountDisplayer(GameUtil.UnitClass unitClass, GameUtil.TimeSlice deltaTimeSlice, StandardAttributeFormatter formatter = null, GameUtil.IdentityDescriptorTense tense = GameUtil.IdentityDescriptorTense.Normal)
	{
		this.tense = tense;
		if (formatter != null)
		{
			this.formatter = formatter;
		}
		else
		{
			this.formatter = new StandardAttributeFormatter(unitClass, deltaTimeSlice);
		}
		deltaFormatter = this.formatter;
	}

	public void SetDeltaFormatter(StandardAttributeFormatter deltaFormatter)
	{
		this.deltaFormatter = deltaFormatter;
	}

	public virtual string GetValueString(Amount master, AmountInstance instance)
	{
		if (!master.showMax)
		{
			return formatter.GetFormattedValue(instance.value);
		}
		return $"{formatter.GetFormattedValue(instance.value)} / {formatter.GetFormattedValue(instance.GetMax())}";
	}

	public virtual string GetDescription(Amount master, AmountInstance instance)
	{
		return $"{master.Name}: {GetValueString(master, instance)}";
	}

	public virtual string GetTooltip(Amount master, AmountInstance instance)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		if (master.description.IndexOf("{1}") > -1)
		{
			stringBuilder.AppendFormat(master.description, formatter.GetFormattedValue(instance.value), GameUtil.GetIdentityDescriptor(instance.gameObject, tense));
		}
		else
		{
			stringBuilder.AppendFormat(master.description, formatter.GetFormattedValue(instance.value));
		}
		stringBuilder.Append("\n\n");
		if (formatter.DeltaTimeSlice == GameUtil.TimeSlice.PerCycle)
		{
			stringBuilder.AppendFormat(UI.CHANGEPERCYCLE, deltaFormatter.GetFormattedValue(instance.deltaAttribute.GetTotalDisplayValue(), GameUtil.TimeSlice.PerCycle));
		}
		else if (formatter.DeltaTimeSlice == GameUtil.TimeSlice.PerSecond)
		{
			stringBuilder.AppendFormat(UI.CHANGEPERSECOND, deltaFormatter.GetFormattedValue(instance.deltaAttribute.GetTotalDisplayValue(), GameUtil.TimeSlice.PerSecond));
		}
		for (int i = 0; i != instance.deltaAttribute.Modifiers.Count; i++)
		{
			AttributeModifier attributeModifier = instance.deltaAttribute.Modifiers[i];
			stringBuilder.Append("\n");
			stringBuilder.AppendFormat(UI.MODIFIER_ITEM_TEMPLATE, attributeModifier.GetDescription(), deltaFormatter.GetFormattedModifier(attributeModifier));
		}
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}

	public string GetFormattedAttribute(AttributeInstance instance)
	{
		return formatter.GetFormattedAttribute(instance);
	}

	public string GetFormattedModifier(AttributeModifier modifier)
	{
		return formatter.GetFormattedModifier(modifier);
	}

	public string GetFormattedValue(float value, GameUtil.TimeSlice time_slice)
	{
		return formatter.GetFormattedValue(value, time_slice);
	}
}
