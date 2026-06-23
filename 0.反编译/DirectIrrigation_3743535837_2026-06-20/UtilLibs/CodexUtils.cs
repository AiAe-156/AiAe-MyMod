using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Klei;

namespace UtilLibs;

public static class CodexUtils
{
	private static string ModBasePath = Path.Combine(IO_Utils.ModPath, "codex");

	public static void PostProcess(List<CodexEntry> resultList)
	{
		foreach (CodexEntry result in resultList)
		{
			if (string.IsNullOrEmpty(result.sortString))
			{
				result.sortString = StringEntry.op_Implicit(Strings.Get(result.title));
			}
		}
		resultList.Sort((CodexEntry x, CodexEntry y) => x.sortString.CompareTo(y.sortString));
	}

	public static void CollectModdedCodexEntries(string category, List<CodexEntry> result, bool overrideExisting = false)
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		string path = ((category == "") ? ModBasePath : Path.Combine(ModBasePath, category));
		if (!Directory.Exists(path))
		{
			return;
		}
		string[] array = null;
		try
		{
			array = Directory.GetFiles(path, "*.yaml");
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)ex);
		}
		if (array == null || !array.Any())
		{
			return;
		}
		string category2 = category.ToUpper();
		string[] array2 = array;
		foreach (string text in array2)
		{
			SgtLogger.l("CodexEntry: " + text);
			if (CodexCache.IsSubEntryAtPath(text))
			{
				continue;
			}
			try
			{
				CodexEntry codexEntry = YamlIO.LoadFile<CodexEntry>(text, new ErrorHandler(CodexCache.YamlParseErrorCB), CodexCache.widgetTagMappings);
				if (codexEntry == null)
				{
					continue;
				}
				codexEntry.category = category2;
				if (Util.IsNullOrWhiteSpace(codexEntry.sortString))
				{
					codexEntry.sortString = StringEntry.op_Implicit(Strings.Get(codexEntry.title));
				}
				if (overrideExisting)
				{
					result.RemoveAll((CodexEntry entry) => entry.id == codexEntry.id);
				}
				result.Add(codexEntry);
				SgtLogger.l("added modded codex entry: " + codexEntry.id + " to category " + category);
			}
			catch (Exception ex2)
			{
				DebugUtil.DevLogErrorFormat("CodexCache.CollectEntries failed to load [{0}]: {1}", new object[2]
				{
					text,
					ex2.ToString()
				});
			}
		}
		PostProcess(result);
	}
}
