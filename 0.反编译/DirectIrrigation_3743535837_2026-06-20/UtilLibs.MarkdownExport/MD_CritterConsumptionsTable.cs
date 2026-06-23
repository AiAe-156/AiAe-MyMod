using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UtilLibs.MarkdownExport;

public class MD_CritterConsumptionsTable : IMD_Entry
{
	private List<Tuple<Tag, float, Tag, Tag>> infos;

	private static StringBuilder sb = new StringBuilder();

	public MD_CritterConsumptionsTable(List<Tuple<Tag, float, Tag, Tag>> _infos)
	{
		infos = _infos;
	}

	public string FormatAsMarkdown()
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		infos = (from info in infos
			orderby MarkdownUtil.GetTagString(info.Item4), MarkdownUtil.GetTagString(info.Item1)
			select info).ToList();
		sb.Clear();
		sb.AppendLine();
		sb.AppendLine("|" + MD_Localization.L("INPUTS_HEADER") + "|" + MD_Localization.L("STRINGS.BUILDING.STATUSITEMS.CRITTERCAPACITY.UNIT") + "|" + MD_Localization.L("OUTPUTS_HEADER") + "|");
		sb.AppendLine("|-|-|-|");
		foreach (Tuple<Tag, float, Tag, Tag> info in infos)
		{
			Tag item = info.Item4;
			Tag item2 = info.Item1;
			Tag item3 = info.Item3;
			float item4 = info.Item2;
			string value = Mathf.RoundToInt(item4 * 100f) + MD_Localization.L("STRINGS.UI.UNITSUFFIXES.PERCENT") + " ";
			sb.Append(MarkdownUtil.GetTagStringWithIcon(item2));
			sb.Append(" | ");
			sb.Append(MarkdownUtil.GetTagStringWithIcon(item));
			sb.Append(" | ");
			sb.Append(value);
			sb.Append(MarkdownUtil.GetTagStringWithIcon(item3));
			sb.AppendLine("|");
		}
		return sb.ToString();
	}
}
