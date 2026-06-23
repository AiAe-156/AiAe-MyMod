using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace UtilLibs;

public static class SanitationUtils
{
	private static HashSet<char> ForbiddenCharacters;

	private static HashSet<string> ForbiddenNames = new HashSet<string>
	{
		"CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6",
		"COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7",
		"LPT8", "LPT9"
	};

	public static string SanitizeName(string name)
	{
		string text = string.Empty;
		if (ForbiddenCharacters == null)
		{
			ForbiddenCharacters = new HashSet<char>();
			ForbiddenCharacters.UnionWith(Path.GetInvalidFileNameChars());
			ForbiddenCharacters.UnionWith(Path.GetInvalidFileNameChars());
			ForbiddenCharacters.UnionWith(Path.GetInvalidPathChars());
			ForbiddenCharacters.UnionWith(Util.defaultInvalidUserInputChars);
			ForbiddenCharacters.UnionWith(Util.additionalInvalidUserInputChars);
		}
		foreach (char c in name)
		{
			text += (ForbiddenCharacters.Contains(c) ? '_' : c);
		}
		text = text.Trim(' ');
		if (ForbiddenNames.Contains(text))
		{
			text = Path.GetRandomFileName();
		}
		return text;
	}

	public static string SanitizeModName(string modName)
	{
		Regex regex = new Regex("(\\/\\/.*)");
		Match match = regex.Match(modName);
		if (match.Success)
		{
			return modName.Substring(0, match.Index).TrimEnd(' ');
		}
		return modName;
	}

	public static string GetSanitizedNamePath(string source)
	{
		source = Path.GetFileName(source);
		source = ReplaceInvalidChars(source);
		if (ForbiddenNames.Contains(source.ToUpperInvariant()))
		{
			SgtLogger.l("file name was one of the forbidden ones, replacing..");
			source = Path.GetRandomFileName();
		}
		return source;
	}

	public static string ReplaceInvalidChars(string filename)
	{
		return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
	}
}
