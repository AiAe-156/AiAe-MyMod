namespace PeterHan.PLib.UI;

public interface ITooltipListableOption : IListableOption
{
	/// <summary>
	/// Retrieves the tool tip text for this option.
	/// </summary>
	/// <returns>The text to be shown on the tool tip.</returns>
	string GetToolTipText();
}
