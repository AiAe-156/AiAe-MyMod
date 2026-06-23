using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace PeterHan.PLib.Core;

/// <summary>
/// Handles localization of PLib for mods by automatically loading po files stored as
/// EmbeddedResources in PLibCore.dll and ILMerged with the mod assembly.
/// </summary>
public static class PLibLocalization
{
	/// <summary>
	/// The file extension used for localization files.
	/// </summary>
	public const string TRANSLATIONS_EXT = ".po";

	/// <summary>
	/// The Prefix of LogicalName of EmbeddedResources that stores the content of po files.
	/// Must match the specified value in the Directory.Build.targets file.
	/// </summary>
	private const string TRANSLATIONS_RES_PATH = "PeterHan.PLib.Core.PLibStrings.";

	internal static void LocalizeItself(Locale locale)
	{
		if (locale == null)
		{
			throw new ArgumentNullException("locale");
		}
		Localization.RegisterForTranslation(typeof(PLibStrings));
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string text = locale.Code;
		if (string.IsNullOrEmpty(text))
		{
			text = Localization.GetCurrentLanguageCode();
		}
		try
		{
			using Stream stream = executingAssembly.GetManifestResourceStream("PeterHan.PLib.Core.PLibStrings." + text + ".po");
			if (stream == null)
			{
				return;
			}
			List<string> list = new List<string>(128);
			using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
			{
				string item;
				while ((item = streamReader.ReadLine()) != null)
				{
					list.Add(item);
				}
			}
			Localization.OverloadStrings(Localization.ExtractTranslatedStrings(list.ToArray(), false));
		}
		catch (Exception thrown)
		{
			PUtil.LogWarning("Failed to load {0} localization for PLib Core:".F(text));
			PUtil.LogExcWarn(thrown);
		}
	}
}
