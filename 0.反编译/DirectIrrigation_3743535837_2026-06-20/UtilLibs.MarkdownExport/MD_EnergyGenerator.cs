using System.Text;

namespace UtilLibs.MarkdownExport;

public class MD_EnergyGenerator : IMD_Entry
{
	private Formula formula;

	private static StringBuilder sb = new StringBuilder();

	public MD_EnergyGenerator(EnergyGenerator gen)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		formula = gen.formula;
	}

	public string FormatAsMarkdown()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		sb.Clear();
		sb.AppendLine("|" + MD_Localization.L("INPUTS_HEADER") + "|" + MD_Localization.L("OUTPUTS_HEADER") + "|");
		sb.AppendLine("|-|-|");
		sb.Append("|");
		InputItem[] inputs = formula.inputs;
		foreach (InputItem val in inputs)
		{
			sb.Append(MarkdownUtil.GetFormattedMass(val.tag, val.consumptionRate, (TimeSlice)2));
			sb.Append("<br>");
		}
		sb.Append("| ");
		OutputItem[] outputs = formula.outputs;
		foreach (OutputItem val2 in outputs)
		{
			string extraSuffix = string.Empty;
			if (val2.minTemperature > 0f)
			{
				extraSuffix = string.Format(MD_Localization.L("AT_TEMPERATURE"), GameUtil.GetTemperatureConvertedFromKelvin(val2.minTemperature, (TemperatureUnit)0).ToString());
			}
			sb.Append(MarkdownUtil.GetFormattedMass(GameTagExtensions.CreateTag(val2.element), val2.creationRate, (TimeSlice)2, extraSuffix));
			sb.Append("<br>");
		}
		sb.AppendLine("|");
		sb.AppendLine();
		return sb.ToString();
	}
}
