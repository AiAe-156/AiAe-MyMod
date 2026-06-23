using System.Collections.Generic;
using System.IO;

namespace UtilLibs.MarkdownExport;

public class MD_Directory : IMD_File
{
	public string Name;

	public List<MD_Directory> SubDirectories = new List<MD_Directory>();

	public List<MD_Page> Files = new List<MD_Page>();

	public bool WriteDirectory = true;

	public MD_Directory(string targetDirectory)
	{
		Name = targetDirectory;
	}

	public MD_Directory SubDir(string subDirName)
	{
		if (SubDirectories == null)
		{
			SubDirectories = new List<MD_Directory>();
		}
		MD_Directory mD_Directory = new MD_Directory(subDirName);
		SubDirectories.Add(mD_Directory);
		return mD_Directory;
	}

	public MD_Page File(string fileName, string titleKey = null)
	{
		if (Files == null)
		{
			Files = new List<MD_Page>();
		}
		MD_Page mD_Page = new MD_Page(fileName, titleKey);
		Files.Add(mD_Page);
		return mD_Page;
	}

	public void CreateMarkdownFiles(string inheritedPath)
	{
		if (!WriteDirectory)
		{
			return;
		}
		string text = Path.Combine(inheritedPath, Name);
		Directory.CreateDirectory(text);
		foreach (MD_Directory subDirectory in SubDirectories)
		{
			subDirectory.CreateMarkdownFiles(text);
		}
		foreach (MD_Page file in Files)
		{
			file.CreateMarkdownFiles(text);
		}
	}
}
