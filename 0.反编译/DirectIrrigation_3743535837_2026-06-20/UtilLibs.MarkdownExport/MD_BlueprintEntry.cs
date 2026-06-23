using System.Text;
using Database;

namespace UtilLibs.MarkdownExport;

internal class MD_BlueprintEntry : IMD_Entry
{
	private static StringBuilder sb = new StringBuilder();

	public string ID;

	public MD_BlueprintEntry(string id)
	{
		ID = id;
	}

	public string FormatAsMarkdown()
	{
		BuildingFacadeResource val = ((ResourceSet<BuildingFacadeResource>)(object)Db.GetBuildingFacades()).TryGet(ID);
		if (val == null)
		{
			return "MISSING SKIN RESOURCE: " + ID;
		}
		sb.Clear();
		sb.AppendLine("| ![" + ID + "](/assets/images/buildings/" + ID + ".png){width=\"100\"} |");
		sb.Append("|");
		sb.AppendLine(MD_Localization.L(MD_Localization.FindStringKey(((Resource)val).Name)));
		sb.AppendLine("|");
		sb.Append("|");
		sb.AppendLine(MD_Localization.L(MD_Localization.FindStringKey(((PermitResource)val).Description)));
		sb.AppendLine("|");
		return sb.ToString();
	}
}
