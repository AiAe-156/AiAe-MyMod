using System;
using System.Text;
using Database;

namespace UtilLibs.MarkdownExport;

public class MD_BlueprintCollectionEntry : IMD_Entry
{
	private static StringBuilder sb = new StringBuilder();

	private bool _title = true;

	public string BuildingID;

	public MD_BlueprintCollectionEntry(string buildingId, bool title = true)
	{
		BuildingID = buildingId;
		_title = title;
	}

	public string FormatAsMarkdown()
	{
		sb.Clear();
		if (_title)
		{
			sb.Append("## ");
			sb.AppendLine(MarkdownUtil.StrippedBuildingName(BuildingID));
			sb.AppendLine();
		}
		if (!SupplyClosetUtils.TryGetCollectionFor(BuildingID, out var collection))
		{
			throw new Exception("no skins for " + BuildingID + " registered");
		}
		sb.Append("|");
		sb.Append(MD_Localization.L("STRINGS.UI.UISIDESCREENS.TABS.SKIN"));
		sb.Append("|");
		sb.Append(MD_Localization.L("STRINGS.UI.VITALSSCREEN_NAME"));
		sb.AppendLine("|");
		sb.AppendLine("|-|-|");
		foreach (string item in collection)
		{
			BuildingFacadeResource val = ((ResourceSet<BuildingFacadeResource>)(object)Db.GetBuildingFacades()).TryGet(item);
			if (val == null)
			{
				throw new Exception("MISSING SKIN RESOURCE: " + item);
			}
			sb.Append("|");
			sb.Append("![" + item + "](/assets/images/buildings/" + item + ".png){height=\"100\"} ");
			sb.Append("|");
			sb.Append("**<font size=\"+1\">");
			sb.Append(MD_Localization.L(MD_Localization.FindStringKey(((Resource)val).Name)));
			sb.Append("</font>**");
			sb.Append("<br/>");
			sb.Append("<br/>");
			sb.Append(MD_Localization.L(MD_Localization.FindStringKey(((PermitResource)val).Description)));
			sb.AppendLine("|");
		}
		return sb.ToString();
	}
}
