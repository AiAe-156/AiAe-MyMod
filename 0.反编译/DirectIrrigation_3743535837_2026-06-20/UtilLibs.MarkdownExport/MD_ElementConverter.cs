using System.Text;
using UnityEngine;

namespace UtilLibs.MarkdownExport;

public class MD_ElementConverter : IMD_Entry
{
	private ElementConverter converter;

	private static StringBuilder sb = new StringBuilder();

	public MD_ElementConverter(ElementConverter converter)
	{
		this.converter = converter;
	}

	public string FormatAsMarkdown()
	{
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		sb.Clear();
		sb.AppendLine("|" + MD_Localization.L("INPUTS_HEADER") + "|" + MD_Localization.L("OUTPUTS_HEADER") + "|");
		sb.AppendLine("|-|-|");
		sb.Append("|");
		if (converter.consumedElements != null)
		{
			ConsumedElement[] consumedElements = converter.consumedElements;
			foreach (ConsumedElement val in consumedElements)
			{
				sb.Append(MarkdownUtil.GetFormattedMass(val.Tag, val.MassConsumptionRate, (TimeSlice)2));
				sb.Append("<br>");
			}
		}
		sb.Append("|");
		if (converter.outputElements != null)
		{
			OutputElement[] outputElements = converter.outputElements;
			foreach (OutputElement val2 in outputElements)
			{
				string extraSuffix = string.Empty;
				if (!val2.useEntityTemperature)
				{
					extraSuffix = string.Format(MD_Localization.L("AT_TEMPERATURE"), GameUtil.GetTemperatureConvertedFromKelvin(val2.minOutputTemperature, (TemperatureUnit)0).ToString());
				}
				sb.Append(MarkdownUtil.GetFormattedMass(GameTagExtensions.CreateTag(val2.elementHash), val2.massGenerationRate, (TimeSlice)2, extraSuffix));
				sb.Append("<br>");
			}
		}
		OilWellCap val3 = default(OilWellCap);
		if (((Component)converter).TryGetComponent<OilWellCap>(ref val3))
		{
			string extraSuffix2 = string.Format(MD_Localization.L("AT_TEMPERATURE"), GameUtil.GetTemperatureConvertedFromKelvin(val3.gasTemperature, (TemperatureUnit)0).ToString());
			sb.Append(MarkdownUtil.GetFormattedMass(GameTagExtensions.CreateTag(val3.gasElement), val3.addGasRate, (TimeSlice)2, extraSuffix2));
			sb.Append("<br>");
		}
		sb.AppendLine("|");
		sb.AppendLine();
		return sb.ToString();
	}
}
