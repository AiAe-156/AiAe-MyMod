using System;
using UnityEngine;

namespace UtilLibs.MarkdownExport;

public class MD_GeyserEntry : MD_EntityEntry
{
	private string NAMEKEY;

	private string DESCKEY;

	private string ID;

	public MD_GeyserEntry(string id, string namekey = null, string descKey = null)
		: base(id, namekey, descKey)
	{
		ID = id;
		NAMEKEY = namekey;
		DESCKEY = descKey;
	}

	public override string FormatAsMarkdown()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		MD_EntityEntry.sb.Clear();
		GameObject prefab = Assets.GetPrefab(Tag.op_Implicit(ID));
		if ((Object)(object)prefab == (Object)null)
		{
			throw new Exception("Geyser with the id " + ID + " does not exist");
		}
		MD_EntityEntry.sb.AppendLine();
		MD_EntityEntry.sb.Append("## ");
		if (Util.IsNullOrWhiteSpace(NAMEKEY))
		{
			MD_EntityEntry.sb.AppendLine(MarkdownUtil.GetTagString(Tag.op_Implicit(ID)));
		}
		else
		{
			MD_EntityEntry.sb.AppendLine(MD_Localization.L(NAMEKEY));
		}
		MD_EntityEntry.sb.AppendLine();
		if (Util.IsNullOrWhiteSpace(DESCKEY))
		{
			MD_EntityEntry.sb.AppendLine(MarkdownUtil.GetTagString(Tag.op_Implicit(ID), desc: true));
		}
		else
		{
			MD_EntityEntry.sb.AppendLine(MD_Localization.L(DESCKEY));
		}
		MD_EntityEntry.sb.AppendLine();
		MD_EntityEntry.sb.AppendLine("| | | |");
		MD_EntityEntry.sb.AppendLine("|-|-|-|");
		MD_EntityEntry.sb.Append("| ![" + ID + "](/assets/images/geysers/" + ID + ".png) {rowspan=\"3\"} ");
		OccupyArea val = default(OccupyArea);
		if (prefab.TryGetComponent<OccupyArea>(ref val))
		{
			MD_EntityEntry.sb.AppendLine("|" + MD_Localization.L("BUILDING_DIMENSIONS_LABEL") + " | " + string.Format(MD_Localization.L("BUILDING_DIMENSIONS_INFO"), val.GetWidthInCells(), val.GetHeightInCells()) + "|");
		}
		bool flag = false;
		GeyserConfigurator val2 = default(GeyserConfigurator);
		if (prefab.TryGetComponent<GeyserConfigurator>(ref val2))
		{
			GeyserType val3 = GeyserConfigurator.FindType(val2.presetType);
			float num = val3.maxRatePerCycle / 1000f;
			float num2 = val3.minRatePerCycle / 1000f;
			float mass = (num + num2) / 2f;
			string formattedMass = MarkdownUtil.GetFormattedMass(mass, (TimeSlice)2, (MetricMassFormat)0);
			string formattedMass2 = MarkdownUtil.GetFormattedMass(num2, (TimeSlice)2, (MetricMassFormat)0);
			string formattedMass3 = MarkdownUtil.GetFormattedMass(num, (TimeSlice)2, (MetricMassFormat)0);
			string text = string.Format(MD_Localization.L("AT_TEMPERATURE"), GameUtil.GetTemperatureConvertedFromKelvin(val3.temperature, (TemperatureUnit)0).ToString());
			MD_EntityEntry.sb.AppendLine("|" + MD_Localization.L("STRINGS.MISC.STATUSITEMS.SPOUTEMITTING.NAME").Replace("{StudiedDetails}", string.Empty) + " | " + MarkdownUtil.GetTagStringWithIcon(GameTagExtensions.CreateTag(val3.element)) + " " + text + "|&#8288 {: style=\"padding:0\"} |");
			MD_EntityEntry.sb.AppendLine("|" + MD_Localization.L("STRINGS.UI.BUILDINGEFFECTS.GEYSER_YEAR_AVR_OUTPUT") + " | " + formattedMass + " (" + formattedMass2 + " - " + formattedMass3 + ")|&#8288 {: style=\"padding:0\"} |");
			flag = true;
		}
		DecorProvider val4 = default(DecorProvider);
		if (prefab.TryGetComponent<DecorProvider>(ref val4) && !flag)
		{
			MD_EntityEntry.sb.Append("|");
			MD_EntityEntry.sb.Append(MD_Localization.Strip(MD_Localization.L("STRINGS.DUPLICANTS.ATTRIBUTES.DECOR.NAME")));
			MD_EntityEntry.sb.Append("|");
			string arg = ((val4.baseDecor > 0f) ? ("+" + val4.baseDecor) : val4.baseDecor.ToString());
			MD_EntityEntry.sb.Append(MD_Localization.Strip(string.Format(MD_Localization.L("STRINGS.UI.BUILDINGEFFECTS.DECORPROVIDED"), "", arg, val4.baseRadius)));
			MD_EntityEntry.sb.AppendLine("|&#8288 {: style=\"padding:0\"}|");
		}
		return MD_EntityEntry.sb.ToString();
	}
}
