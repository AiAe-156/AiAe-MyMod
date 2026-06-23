using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UtilLibs.MarkdownExport;

public class MD_TagsTable : IMD_Entry
{
	private HashSet<Tag> elementTags;

	private static StringBuilder sb = new StringBuilder();

	public MD_TagsTable(HashSet<Tag> substances)
	{
		elementTags = substances;
	}

	private static List<Tag> GetValidMaterialsFor(Tag materialTag)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		List<Tag> list = new List<Tag>();
		foreach (Element element in ElementLoader.elements)
		{
			if (element.tag == materialTag || element.HasTag(materialTag))
			{
				list.Add(element.tag);
			}
		}
		foreach (Tag materialBuildingElement in GameTags.MaterialBuildingElements)
		{
			if (materialBuildingElement != materialTag)
			{
				continue;
			}
			foreach (GameObject item in Assets.GetPrefabsWithTag(materialBuildingElement))
			{
				KPrefabID component = item.GetComponent<KPrefabID>();
				if ((Object)(object)component != (Object)null && !list.Contains(component.PrefabTag))
				{
					list.Add(component.PrefabTag);
				}
			}
		}
		return list;
	}

	public string FormatAsMarkdown()
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		sb.Clear();
		sb.AppendLine();
		sb.AppendLine("|<font size=\"+1\">" + MD_Localization.L("STRINGS.UI.UISIDESCREENS.TABS.MATERIAL") + "</font> | | <font size=\"+1\">" + MD_Localization.L("STRINGS.UI.CODEX.CATEGORYNAMES.ELEMENTS") + "</font> |");
		sb.AppendLine("|:-:|:-:|:-|");
		foreach (Tag item in from t in elementTags.Distinct()
			orderby MarkdownUtil.GetTagString(t)
			select t)
		{
			sb.Append("|");
			sb.Append("<font size=\"+1\">");
			sb.Append(MarkdownUtil.GetTagString(item));
			sb.Append("</font> <br/> <br/>");
			sb.Append("|");
			sb.Append(MarkdownUtil.GetTagString(item, desc: true));
			sb.Append("|");
			foreach (Tag item2 in GetValidMaterialsFor(item))
			{
				sb.Append(MarkdownUtil.GetTagStringWithIcon(item2) + "<br/>");
			}
			sb.AppendLine("|");
		}
		return sb.ToString();
	}
}
