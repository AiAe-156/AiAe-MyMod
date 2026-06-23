using System.Collections.Generic;

namespace UtilLibs.MarkdownExport;

public class TranslationKeyBuilder
{
	private string internalID;

	public TranslationKeyBuilder(string formatKey, List<string> formatValueKeys)
	{
		internalID = formatKey.GetHashCode().ToString();
		foreach (string formatValueKey in formatValueKeys)
		{
			internalID += formatValueKey.GetHashCode();
		}
		MD_Localization.Add(internalID, formatKey, formatValueKeys);
	}

	public override string ToString()
	{
		return internalID.ToString();
	}

	public static implicit operator string(TranslationKeyBuilder info)
	{
		return info.ToString();
	}
}
