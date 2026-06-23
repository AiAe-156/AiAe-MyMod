namespace UtilLibs.MarkdownExport;

public class MD_Text : IMD_Entry
{
	private string TextKey;

	public MD_Text(string textKey)
	{
		TextKey = textKey;
	}

	public string FormatAsMarkdown()
	{
		return MD_Localization.L(TextKey);
	}
}
