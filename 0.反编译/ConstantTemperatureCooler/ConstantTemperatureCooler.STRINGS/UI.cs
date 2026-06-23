using STRINGS;

namespace ConstantTemperatureCooler.STRINGS;

public static class UI
{
	public static class UISIDESCREENS
	{
		public static class AIRCONDITIONERTEMPERATURESIDESCREEN
		{
			public static LocString TITLE = LocString.op_Implicit("Cooling Temperature");

			public static LocString TOOLTIP = LocString.op_Implicit("This device will adjust the temperature of fluids passing through by " + UI.FormatAsKeyWord("{0}{1}") + ", consuming " + UI.FormatAsKeyWord("{2}{3}") + " of power to do so.");
		}
	}
}
