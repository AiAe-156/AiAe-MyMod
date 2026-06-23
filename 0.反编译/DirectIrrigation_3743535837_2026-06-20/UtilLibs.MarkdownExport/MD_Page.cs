using System.Collections.Generic;
using System.IO;

namespace UtilLibs.MarkdownExport;

public class MD_Page : IMD_File
{
	public string FileName;

	public string TitleKey;

	public List<IMD_Entry> Entries = new List<IMD_Entry>();

	public bool WritePage = true;

	public MD_Page(string fileName, string titleKey = null)
	{
		FileName = fileName;
		TitleKey = ((titleKey == null) ? fileName : titleKey);
	}

	public MD_BuildingEntry AddBuilding(string buildingId)
	{
		MD_BuildingEntry mD_BuildingEntry = new MD_BuildingEntry(buildingId);
		if (Entries == null)
		{
			Entries = new List<IMD_Entry>(1) { mD_BuildingEntry };
		}
		else
		{
			Entries.Add(mD_BuildingEntry);
		}
		return mD_BuildingEntry;
	}

	public MD_Page Add(IMD_Entry child)
	{
		Entries.Add(child);
		return this;
	}

	public void CreateMarkdownFiles(string inheritedPath)
	{
		if (!WritePage)
		{
			return;
		}
		FileInfo fileInfo = new FileInfo(Path.Combine(inheritedPath, FileName + MD_Localization.GetSuffix() + ".md"));
		FileStream stream = fileInfo.Open(FileMode.Create);
		using StreamWriter streamWriter = new StreamWriter(stream);
		streamWriter.WriteLine("# " + MD_Localization.L(TitleKey.ToUpperInvariant()));
		foreach (IMD_Entry entry in Entries)
		{
			streamWriter.WriteLine(entry.FormatAsMarkdown());
		}
	}
}
