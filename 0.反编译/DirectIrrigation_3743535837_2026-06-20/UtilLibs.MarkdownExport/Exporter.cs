using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UtilLibs.MarkdownExport;

public class Exporter
{
	private string TargetDirectory;

	public MD_Directory root;

	public static Exporter Instance;

	public Dictionary<string, Dictionary<Tag, List<Tag>>> RandomRecipeResults = new Dictionary<string, Dictionary<Tag, List<Tag>>>();

	public Dictionary<string, Dictionary<Tag, List<Tag>>> RandomRecipeOccurences = new Dictionary<string, Dictionary<Tag, List<Tag>>>();

	private static string entityExportPath;

	private static HashSet<Tag> ToExportEntities;

	public static Exporter Create(string directory)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(directory));
		Exporter exporter = new Exporter();
		exporter.TargetDirectory = directory;
		exporter.root = new MD_Directory(exporter.TargetDirectory);
		Instance = exporter;
		return exporter;
	}

	public void Export(string localizeKey = null)
	{
		if (root == null)
		{
			throw new InvalidOperationException("Root directory is not set.");
		}
		if (localizeKey != null)
		{
			MD_Localization.SetLocalization(localizeKey);
		}
		root.CreateMarkdownFiles("");
	}

	public Exporter EntityIconPath(string path)
	{
		entityExportPath = path;
		return this;
	}

	public static void WriteUISprite(string path, string fileName, KAnimFile kanimFile)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)kanimFile == (Object)null))
		{
			Sprite uISpriteFromMultiObjectAnim = Def.GetUISpriteFromMultiObjectAnim(kanimFile, "ui", false, "");
			if ((Object)(object)uISpriteFromMultiObjectAnim != (Object)null && (Object)(object)uISpriteFromMultiObjectAnim != (Object)(object)Assets.GetSprite(HashedString.op_Implicit("unknown")))
			{
				MarkdownUtil.WriteUISpriteToFile(uISpriteFromMultiObjectAnim, path, fileName);
			}
		}
	}

	public unsafe void ExportEntityIcons()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (entityExportPath == null)
		{
			return;
		}
		foreach (Tag toExportEntity in ToExportEntities)
		{
			GameObject prefab = Assets.GetPrefab(toExportEntity);
			if (!((Object)(object)prefab == (Object)null))
			{
				Sprite first = Def.GetUISprite((object)prefab, "ui", false).first;
				if ((Object)(object)first != (Object)null && (Object)(object)first != (Object)(object)Assets.GetSprite(HashedString.op_Implicit("unknown")))
				{
					MarkdownUtil.WriteUISpriteToFile(first, entityExportPath, ((object)(*(Tag*)(&toExportEntity))/*cast due to .constrained prefix*/).ToString());
				}
			}
		}
	}

	internal static void AddEntity(Tag tag)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (ToExportEntities == null)
		{
			ToExportEntities = new HashSet<Tag>();
		}
		ToExportEntities.Add(tag);
	}
}
