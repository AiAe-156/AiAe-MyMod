using System.Text;
using Klei.AI;
using STRINGS;

public class QualityOfLifeAttributeFormatter : StandardAttributeFormatter
{
	public QualityOfLifeAttributeFormatter()
		: base(GameUtil.UnitClass.SimpleInteger, GameUtil.TimeSlice.None)
	{
	}

	public override string GetFormattedAttribute(AttributeInstance instance)
	{
		AttributeInstance attributeInstance = Db.Get().Attributes.QualityOfLifeExpectation.Lookup(instance.gameObject);
		return string.Format(DUPLICANTS.ATTRIBUTES.QUALITYOFLIFE.DESC_FORMAT, GetFormattedValue(instance.GetTotalDisplayValue()), GetFormattedValue(attributeInstance.GetTotalDisplayValue()));
	}

	public override string GetTooltip(Attribute master, AttributeInstance instance)
	{
		StringBuilder stringBuilder = GlobalStringBuilderPool.Alloc();
		stringBuilder.Append(base.GetTooltip(master, instance));
		AttributeInstance attributeInstance = Db.Get().Attributes.QualityOfLifeExpectation.Lookup(instance.gameObject);
		stringBuilder.Append("\n\n");
		stringBuilder.AppendFormat(DUPLICANTS.ATTRIBUTES.QUALITYOFLIFE.TOOLTIP_EXPECTATION, GetFormattedValue(attributeInstance.GetTotalDisplayValue()));
		if (instance.GetTotalDisplayValue() - attributeInstance.GetTotalDisplayValue() >= 0f)
		{
			stringBuilder.Append("\n\n");
			stringBuilder.Append(DUPLICANTS.ATTRIBUTES.QUALITYOFLIFE.TOOLTIP_EXPECTATION_OVER);
		}
		else
		{
			stringBuilder.Append("\n\n");
			stringBuilder.Append(DUPLICANTS.ATTRIBUTES.QUALITYOFLIFE.TOOLTIP_EXPECTATION_UNDER);
		}
		return GlobalStringBuilderPool.ReturnAndFree(stringBuilder);
	}
}
