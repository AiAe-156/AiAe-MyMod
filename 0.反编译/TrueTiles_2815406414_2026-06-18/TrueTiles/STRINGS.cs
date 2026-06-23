namespace TrueTiles;

public class STRINGS
{
	public class TEXTUREPACKS
	{
		public class EXTERNAL_SAVE_DIALOG
		{
			public static LocString TEXT = LocString.op_Implicit("Save settings externally? This will make sure settings will stay after updates.");

			public static LocString BUTTON_EXTERNAL = LocString.op_Implicit("Sure (recommended)");

			public static LocString BUTTON_LOCAL = LocString.op_Implicit("No, keep local");
		}

		public class ALTAIRFLOW
		{
			public static LocString NAME = LocString.op_Implicit("Alt. Airflow");

			public static LocString DESCRIPTION = LocString.op_Implicit("Airflow tiles that resemble the vanilla design more.");
		}

		public class DEFAULT
		{
			public static LocString NAME = LocString.op_Implicit("Default");

			public static LocString DESCRIPTION = LocString.op_Implicit("The default look of True Tiles.");
		}

		public class CUTESYCARPET
		{
			public static LocString NAME = LocString.op_Implicit("Cutesy Carpets");

			public static LocString DESCRIPTION = LocString.op_Implicit("Kitty carpet!!! :3");
		}

		public static LocString INFO = LocString.op_Implicit("by <b>{0}</b>\n");

		public static LocString AUTHOR = LocString.op_Implicit("by <b>{0}</b>");

		public static LocString TEXTURE_COUNT = LocString.op_Implicit("{0} textures");
	}

	public class UI
	{
		public class SETTINGSDIALOG
		{
			public class TITLEBAR
			{
				public static LocString LABEL = LocString.op_Implicit("Tile Textures");
			}

			public class BUTTONS
			{
				public class CANCELBUTTON
				{
					public static LocString TEXT = LocString.op_Implicit("Cancel");
				}

				public class OK
				{
					public static LocString TEXT = LocString.op_Implicit("Apply");
				}

				public class EXTERNALSAVECONFIRM
				{
					public static LocString INFO = LocString.op_Implicit("Save settings outside mods folder");

					public static LocString TOOLTIP = LocString.op_Implicit("Recommended to enable. \nSaving settings outside means your changes will persist after the mod or game updates; but will not be removed when you uninstall the mod.\n\n The path to external settings is: {0}.");
				}
			}

			public class SCROLLVIEW
			{
				public class VIEWPORT
				{
					public class CONTENT
					{
						public class ENTRY
						{
							public class TITLE
							{
								public static LocString TEXT = LocString.op_Implicit("N/A");
							}

							public class BUTTONS
							{
								public class OPEN
								{
									public static LocString LABEL = LocString.op_Implicit("Open Folder");
								}
							}

							public static LocString INFO = LocString.op_Implicit("N/A");
						}
					}
				}
			}

			public static LocString BUTTON = LocString.op_Implicit("Manage");

			public static LocString VERSIONLABEL = LocString.op_Implicit("{0}");
		}
	}
}
