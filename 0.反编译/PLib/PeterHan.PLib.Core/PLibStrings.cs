using STRINGS;

namespace PeterHan.PLib.Core;

/// <summary>
/// Strings used in PLib.
/// </summary>
public static class PLibStrings
{
	/// <summary>
	/// The button used to manually edit the mod configuration.
	/// </summary>
	public static LocString BUTTON_MANUAL = LocString.op_Implicit("MANUAL CONFIG");

	/// <summary>
	/// The button used to reset the configuration to its default value.
	/// </summary>
	public static LocString BUTTON_RESET = LocString.op_Implicit("RESET TO DEFAULT");

	/// <summary>
	/// The text shown on the Done button.
	/// </summary>
	public static LocString BUTTON_OK = OPTIONS_SCREEN.BACK;

	/// <summary>
	/// The text shown on the Options button.
	/// </summary>
	public static LocString BUTTON_OPTIONS = MAINMENU.OPTIONS;

	/// <summary>
	/// The dialog title used for options, where {0} is substituted with the mod friendly name.
	/// </summary>
	public static LocString DIALOG_TITLE = LocString.op_Implicit("Options for {0}");

	public static LocString KEY_HOME = LocString.op_Implicit("Home");

	public static LocString KEY_END = LocString.op_Implicit("End");

	public static LocString KEY_DELETE = LocString.op_Implicit("Delete");

	public static LocString KEY_PAGEUP = LocString.op_Implicit("Page Up");

	public static LocString KEY_PAGEDOWN = LocString.op_Implicit("Page Down");

	public static LocString KEY_SYSRQ = LocString.op_Implicit("SysRq");

	public static LocString KEY_PRTSCREEN = LocString.op_Implicit("Print Screen");

	public static LocString KEY_PAUSE = LocString.op_Implicit("Pause");

	public static LocString KEY_ARROWLEFT = LocString.op_Implicit("Left Arrow");

	public static LocString KEY_ARROWUP = LocString.op_Implicit("Up Arrow");

	public static LocString KEY_ARROWRIGHT = LocString.op_Implicit("Right Arrow");

	public static LocString KEY_ARROWDOWN = LocString.op_Implicit("Down Arrow");

	/// <summary>
	/// The title used for the PLib key bind category.
	/// </summary>
	public static LocString KEY_CATEGORY_TITLE = LocString.op_Implicit("Mods");

	/// <summary>
	/// The abbreviation text shown on the Blue field.
	/// </summary>
	public static LocString LABEL_B = LocString.op_Implicit("B");

	/// <summary>
	/// The abbreviation text shown on the Green field.
	/// </summary>
	public static LocString LABEL_G = LocString.op_Implicit("G");

	/// <summary>
	/// The abbreviation text shown on the Red field.
	/// </summary>
	public static LocString LABEL_R = LocString.op_Implicit("R");

	/// <summary>
	/// The mod version in Mod Options if retrieved from the default AssemblyVersion, where
	/// {0} is substituted with the version text.
	/// </summary>
	public static LocString MOD_ASSEMBLY_VERSION = LocString.op_Implicit("Assembly Version: {0}");

	/// <summary>
	/// The button text which goes to the mod's home page when clicked.
	/// </summary>
	public static LocString MOD_HOMEPAGE = LocString.op_Implicit("Mod Homepage");

	/// <summary>
	/// The mod version in Mod Options if specified via AssemblyFileVersion, where {0} is
	/// substituted with the version text.
	/// </summary>
	public static LocString MOD_VERSION = LocString.op_Implicit("Mod Version: {0}");

	/// <summary>
	/// The cancel button in the restart dialog.
	/// </summary>
	public static LocString RESTART_CANCEL = RESTART.CANCEL;

	/// <summary>
	/// The OK button in the restart dialog.
	/// </summary>
	public static LocString RESTART_OK = RESTART.OK;

	/// <summary>
	/// Displayed in the menu when mods report as being outdated.
	/// </summary>
	public static LocString MAINMENU_UPDATE = LocString.op_Implicit("\n\n<color=#FFCC00>{0:D} mods may be out of date</color>");

	/// <summary>
	/// Displayed in the menu when a mod reports as being outdated.
	/// </summary>
	public static LocString MAINMENU_UPDATE_1 = LocString.op_Implicit("\n\n<color=#FFCC00>1 mod may be out of date</color>");

	/// <summary>
	/// The details tooltip when AVC detects a mod to be outdated.
	/// </summary>
	public static LocString OUTDATED_TOOLTIP = LocString.op_Implicit("This mod is out of date!\nNew version: <b>{0}</b>\n\nUpdate local mods manually, or use <b>Mod Updater</b> to force update Steam mods");

	/// <summary>
	/// Displayed when AVC detects a mod to be outdated.
	/// </summary>
	public static LocString OUTDATED_WARNING = LocString.op_Implicit("<b><style=\"logic_off\">Outdated!</style></b>");

	/// <summary>
	/// The message prompting the user to restart.
	/// </summary>
	public static LocString RESTART_REQUIRED = LocString.op_Implicit("Oxygen Not Included must be restarted for these options to take effect.");

	/// <summary>
	/// The tooltip on the BLUE field in color pickers.
	/// </summary>
	public static LocString TOOLTIP_BLUE = LocString.op_Implicit("Blue");

	/// <summary>
	/// The tooltip on the CANCEL button.
	/// </summary>
	public static LocString TOOLTIP_CANCEL = LocString.op_Implicit("Discard changes.");

	/// <summary>
	/// The tooltip on the GREEN field in color pickers.
	/// </summary>
	public static LocString TOOLTIP_GREEN = LocString.op_Implicit("Green");

	/// <summary>
	/// The tooltip on the Mod Homepage button.
	/// </summary>
	public static LocString TOOLTIP_HOMEPAGE = LocString.op_Implicit("Visit the mod's website.");

	/// <summary>
	/// The tooltip on the Hue slider in color pickers.
	/// </summary>
	public static LocString TOOLTIP_HUE = LocString.op_Implicit("Hue");

	/// <summary>
	/// The tooltip on the MANUAL CONFIG button.
	/// </summary>
	public static LocString TOOLTIP_MANUAL = LocString.op_Implicit("Opens the folder containing the full mod configuration.");

	/// <summary>
	/// The tooltip for cycling to the next item.
	/// </summary>
	public static LocString TOOLTIP_NEXT = LocString.op_Implicit("Next");

	/// <summary>
	/// The tooltip on the OK button.
	/// </summary>
	public static LocString TOOLTIP_OK = LocString.op_Implicit("Save these options. Some mods may require a restart for the options to take effect.");

	/// <summary>
	/// The tooltip for cycling to the previous item.
	/// </summary>
	public static LocString TOOLTIP_PREVIOUS = LocString.op_Implicit("Previous");

	/// <summary>
	/// The tooltip on the RED field in color pickers.
	/// </summary>
	public static LocString TOOLTIP_RED = LocString.op_Implicit("Red");

	/// <summary>
	/// The tooltip on the RESET TO DEFAULT button.
	/// </summary>
	public static LocString TOOLTIP_RESET = LocString.op_Implicit("Resets the mod configuration to default values.");

	/// <summary>
	/// The tooltip on the Saturation slider in color pickers.
	/// </summary>
	public static LocString TOOLTIP_SATURATION = LocString.op_Implicit("Saturation");

	/// <summary>
	/// The tooltip for each category visibility toggle.
	/// </summary>
	public static LocString TOOLTIP_TOGGLE = LocString.op_Implicit("Show or hide this options category");

	/// <summary>
	/// The tooltip on the Value slider in color pickers.
	/// </summary>
	public static LocString TOOLTIP_VALUE = LocString.op_Implicit("Value");

	/// <summary>
	/// The tooltip for the mod version.
	/// </summary>
	public static LocString TOOLTIP_VERSION = LocString.op_Implicit("The currently installed version of this mod.\n\nCompare this version with the mod's Release Notes to see if it is outdated.");
}
