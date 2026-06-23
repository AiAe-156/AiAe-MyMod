using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UtilLibs.MarkdownExport;

public class MD_EntityEntry : IMD_Entry
{
	private string NAMEKEY;

	private string DESCKEY;

	private string ID;

	internal static StringBuilder sb = new StringBuilder();

	public MD_EntityEntry(string id, string namekey = null, string descKey = null)
	{
		ID = id;
		NAMEKEY = namekey;
		DESCKEY = descKey;
	}

	public virtual string FormatAsMarkdown()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		sb.Clear();
		GameObject prefab = Assets.GetPrefab(Tag.op_Implicit(ID));
		if ((Object)(object)prefab == (Object)null)
		{
			throw new Exception("Entity with the id " + ID + " does not exist");
		}
		sb.AppendLine();
		sb.Append("## ");
		if (Util.IsNullOrWhiteSpace(NAMEKEY))
		{
			sb.AppendLine(MarkdownUtil.GetTagString(Tag.op_Implicit(ID)));
		}
		else
		{
			sb.AppendLine(MD_Localization.L(NAMEKEY));
		}
		sb.Append("| ![" + ID + "](/assets/images/entities/" + ID + ".png){width=\"100\"} |");
		if (Util.IsNullOrWhiteSpace(DESCKEY))
		{
			sb.Append(MarkdownUtil.GetTagString(Tag.op_Implicit(ID), desc: true));
		}
		else
		{
			sb.Append(MD_Localization.L(DESCKEY));
		}
		sb.AppendLine("|");
		sb.AppendLine("|-|-|");
		PrimaryElement val = default(PrimaryElement);
		if (prefab.TryGetComponent<PrimaryElement>(ref val))
		{
			sb.Append("|");
			sb.Append(MD_Localization.Strip(MD_Localization.L("STRINGS.UI.DETAILTABS.MATERIAL.NAME")));
			sb.Append("|");
			sb.Append(MarkdownUtil.GetTagStringWithIcon(val.Element.tag));
			sb.AppendLine("|");
			sb.Append("|");
			sb.Append(MD_Localization.Strip(MD_Localization.L("STRINGS.UI.SANDBOXTOOLS.SETTINGS.MASS.NAME")));
			sb.Append("|");
			sb.Append(val.MassPerUnit);
			sb.AppendLine("|");
		}
		DecorProvider val2 = default(DecorProvider);
		if (prefab.TryGetComponent<DecorProvider>(ref val2))
		{
			sb.Append("|");
			sb.Append(MD_Localization.Strip(MD_Localization.L("STRINGS.DUPLICANTS.ATTRIBUTES.DECOR.NAME")));
			sb.Append("|");
			sb.Append(MD_Localization.Strip(string.Format(MD_Localization.L("STRINGS.UI.BUILDINGEFFECTS.DECORPROVIDED"), "", val2.baseDecor, val2.baseRadius)));
			sb.AppendLine("|");
		}
		KPrefabID val3 = default(KPrefabID);
		if (prefab.TryGetComponent<KPrefabID>(ref val3) && val3.tags.Any())
		{
			sb.Append("|");
			sb.Append(MD_Localization.Strip(MD_Localization.L("ELEMENT_PROPERTIES")));
			sb.Append("|");
			IEnumerable<Tag> source = val3.tags.Where((Tag t) => t != GameTags.HasChores);
			string value = string.Join(", ", from text in Util.StableSort<string>(source.Select((Tag t) => MarkdownUtil.GetTagString(t)))
				where !text.Contains("MISSING")
				select text);
			sb.Append(value);
			sb.AppendLine("|");
		}
		return sb.ToString();
	}
}
