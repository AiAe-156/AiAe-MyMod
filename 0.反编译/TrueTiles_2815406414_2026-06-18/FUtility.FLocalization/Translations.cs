using System;
using System.IO;
using KMod;

namespace FUtility.FLocalization;

public class Translations
{
	public static void RegisterForTranslation(Type root, bool generateTemplate = false)
	{
		Localization.RegisterForTranslation(root);
		LoadStrings();
		LocString.CreateLocStringKeys(root, (string)null);
		if (generateTemplate)
		{
			TemplateGenerator.GenerateStringsTemplate(root, Path.Combine(Manager.GetDirectory(), "strings_templates"));
		}
	}

	private static void LoadStrings()
	{
		Locale locale = Localization.GetLocale();
		string text = ((locale != null) ? locale.Code : null);
		if (!Util.IsNullOrWhiteSpace(text))
		{
			string text2 = Path.Combine(Utils.ModPath, "translations", text + ".po");
			if (File.Exists(text2))
			{
				Localization.OverloadStrings(Localization.LoadStringsFile(text2, false));
				Log.Info("Found translation file for " + text + ".");
			}
		}
	}
}
