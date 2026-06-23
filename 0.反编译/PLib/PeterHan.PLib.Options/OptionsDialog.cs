using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using STRINGS;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// A dialog for handling mod options events.
/// </summary>
internal sealed class OptionsDialog
{
	/// <summary>
	/// The color of option category titles.
	/// </summary>
	private static readonly Color CATEGORY_TITLE_COLOR;

	/// <summary>
	/// The text style applied to option category titles.
	/// </summary>
	private static readonly TextStyleSetting CATEGORY_TITLE_STYLE;

	/// <summary>
	/// The margins inside the colored boxes in each config section.
	/// </summary>
	private static readonly int CATEGORY_MARGIN;

	/// <summary>
	/// The size of the mod preview image displayed.
	/// </summary>
	private static readonly Vector2 MOD_IMAGE_SIZE;

	/// <summary>
	/// The margins between the dialog edge and the colored boxes in each config section.
	/// </summary>
	private static readonly int OUTER_MARGIN;

	/// <summary>
	/// The default size of the Mod Settings dialog.
	/// </summary>
	private static readonly Vector2 SETTINGS_DIALOG_SIZE;

	/// <summary>
	/// The maximum size of the Mod Settings dialog before it gets scroll bars.
	/// </summary>
	private static readonly Vector2 SETTINGS_DIALOG_MAX_SIZE;

	/// <summary>
	/// The size of the toggle button on each (non-default) config section.
	/// </summary>
	private static readonly Vector2 TOGGLE_SIZE;

	/// <summary>
	/// If true, all categories begin collapsed.
	/// </summary>
	private readonly bool collapseCategories;

	/// <summary>
	/// The config file attribute for the options type, if present.
	/// </summary>
	private readonly ConfigFileAttribute configAttr;

	/// <summary>
	/// The currently active dialog.
	/// </summary>
	private KScreen dialog;

	/// <summary>
	/// The sprite to display for this mod.
	/// </summary>
	private Sprite modImage;

	/// <summary>
	/// Collects information from the ModInfoAttribute and KMod.Mod objects for display.
	/// </summary>
	private readonly ModDialogInfo displayInfo;

	/// <summary>
	/// The option entries in the dialog.
	/// </summary>
	private readonly IDictionary<string, ICollection<IOptionsEntry>> optionCategories;

	/// <summary>
	/// The options read from the config. It might contain hidden options so preserve its
	/// contents here.
	/// </summary>
	private object options;

	/// <summary>
	/// The type used to determine which options are visible.
	/// </summary>
	private readonly Type optionsType;

	/// <summary>
	/// The event to invoke when the dialog is closed.
	/// </summary>
	public Action<object> OnClose { get; set; }

	static OptionsDialog()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		CATEGORY_TITLE_COLOR = Color32.op_Implicit(new Color32((byte)143, (byte)150, (byte)175, byte.MaxValue));
		CATEGORY_MARGIN = 8;
		MOD_IMAGE_SIZE = new Vector2(192f, 192f);
		OUTER_MARGIN = 10;
		SETTINGS_DIALOG_SIZE = new Vector2(320f, 200f);
		SETTINGS_DIALOG_MAX_SIZE = new Vector2(800f, 600f);
		TOGGLE_SIZE = new Vector2(12f, 12f);
		CATEGORY_TITLE_STYLE = PUITuning.Fonts.UILightStyle.DeriveStyle(0, CATEGORY_TITLE_COLOR, (FontStyles)1);
	}

	/// <summary>
	/// Creates an options object using the default constructor if possible.
	/// </summary>
	/// <param name="type">The type of the object to create.</param>
	internal static object CreateOptions(Type type)
	{
		object result = null;
		try
		{
			ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
			if (constructor != null)
			{
				result = constructor.Invoke(null);
			}
		}
		catch (TargetInvocationException ex)
		{
			PUtil.LogExcWarn(ex.GetBaseException());
		}
		catch (AmbiguousMatchException thrown)
		{
			PUtil.LogException(thrown);
		}
		catch (MemberAccessException thrown2)
		{
			PUtil.LogException(thrown2);
		}
		return result;
	}

	/// <summary>
	/// Saves the mod enabled settings and restarts the game.
	/// </summary>
	private static void SaveAndRestart()
	{
		PGameUtils.SaveMods();
		App.instance.Restart();
	}

	internal OptionsDialog(Type optionsType)
	{
		OnClose = null;
		dialog = null;
		modImage = null;
		this.optionsType = optionsType ?? throw new ArgumentNullException("optionsType");
		optionCategories = OptionsEntry.BuildOptions(optionsType);
		options = null;
		ModInfoAttribute customAttribute = optionsType.GetCustomAttribute<ModInfoAttribute>();
		collapseCategories = customAttribute?.ForceCollapseCategories ?? false;
		configAttr = optionsType.GetCustomAttribute<ConfigFileAttribute>();
		displayInfo = new ModDialogInfo(optionsType, customAttribute?.URL, customAttribute?.Image);
	}

	/// <summary>
	/// Adds a category header to the dialog.
	/// </summary>
	/// <param name="container">The parent of the header.</param>
	/// <param name="category">The header title.</param>
	/// <param name="contents">The panel containing the options in this category.</param>
	private void AddCategoryHeader(PGridPanel container, string category, PGridPanel contents)
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		contents.AddColumn(new GridColumnSpec(0f, 1f)).AddColumn(new GridColumnSpec());
		if (!string.IsNullOrEmpty(category))
		{
			bool initialState = !collapseCategories;
			CategoryExpandHandler categoryExpandHandler = new CategoryExpandHandler(initialState);
			container.AddColumn(new GridColumnSpec()).AddColumn(new GridColumnSpec(0f, 1f)).AddRow(new GridRowSpec())
				.AddRow(new GridRowSpec(0f, 1f));
			container.AddChild(new PLabel("CategoryHeader")
			{
				Text = OptionsEntry.LookInStrings(category),
				TextStyle = CATEGORY_TITLE_STYLE,
				TextAlignment = (TextAnchor)7
			}.AddOnRealize(categoryExpandHandler.OnRealizeHeader), new GridComponentSpec(0, 1)
			{
				Margin = new RectOffset(OUTER_MARGIN, OUTER_MARGIN, 0, 0)
			}).AddChild(new PToggle("CategoryToggle")
			{
				Color = PUITuning.Colors.ComponentDarkStyle,
				InitialState = initialState,
				ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_TOGGLE),
				Size = TOGGLE_SIZE,
				OnStateChanged = categoryExpandHandler.OnExpandContract
			}.AddOnRealize(categoryExpandHandler.OnRealizeToggle), new GridComponentSpec(0, 0));
			contents.OnRealize += categoryExpandHandler.OnRealizePanel;
			container.AddChild(contents, new GridComponentSpec(1, 0)
			{
				ColumnSpan = 2
			});
		}
		else
		{
			container.AddColumn(new GridColumnSpec(0f, 1f)).AddRow(new GridRowSpec(0f, 1f)).AddChild(contents, new GridComponentSpec(0, 0));
		}
	}

	/// <summary>
	/// Fills in the mod info screen, assuming that infoAttr is non-null.
	/// </summary>
	/// <param name="optionsDialog">The dialog to populate.</param>
	private void AddModInfoScreen(PDialog optionsDialog)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		string image = displayInfo.Image;
		PPanel body = optionsDialog.Body;
		if ((Object)(object)modImage == (Object)null && !string.IsNullOrEmpty(image))
		{
			string modPath = PUtil.GetModPath(optionsType.Assembly);
			modImage = PUIUtils.LoadSpriteFile((modPath == null) ? image : Path.Combine(modPath, image));
		}
		PButton child = new PButton("ModSite")
		{
			Text = LocString.op_Implicit(PLibStrings.MOD_HOMEPAGE),
			ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_HOMEPAGE),
			OnClick = VisitModHomepage,
			Margin = PDialog.BUTTON_MARGIN
		}.SetKleiBlueStyle();
		PLabel child2 = new PLabel("ModVersion")
		{
			Text = displayInfo.Version,
			ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_VERSION),
			TextStyle = PUITuning.Fonts.UILightStyle,
			Margin = new RectOffset(0, 0, OUTER_MARGIN, 0)
		};
		string uRL = displayInfo.URL;
		if ((Object)(object)modImage != (Object)null)
		{
			if (optionCategories.Count > 0)
			{
				body.Direction = PanelDirection.Horizontal;
			}
			PPanel pPanel = new PPanel("ModInfo")
			{
				FlexSize = Vector2.up,
				Direction = PanelDirection.Vertical,
				Alignment = (TextAnchor)1
			}.AddChild(new PLabel("ModImage")
			{
				SpriteSize = MOD_IMAGE_SIZE,
				TextAlignment = (TextAnchor)0,
				Margin = new RectOffset(0, OUTER_MARGIN, 0, OUTER_MARGIN),
				Sprite = modImage
			});
			if (!string.IsNullOrEmpty(uRL))
			{
				pPanel.AddChild(child);
			}
			body.AddChild(pPanel.AddChild(child2));
		}
		else
		{
			if (!string.IsNullOrEmpty(uRL))
			{
				body.AddChild(child);
			}
			body.AddChild(child2);
		}
	}

	/// <summary>
	/// Closes the current dialog.
	/// </summary>
	private void CloseDialog()
	{
		if ((Object)(object)dialog != (Object)null)
		{
			dialog.Deactivate();
			dialog = null;
		}
		if ((Object)(object)modImage != (Object)null)
		{
			Object.Destroy((Object)(object)modImage);
			modImage = null;
		}
	}

	/// <summary>
	/// Fills in the actual mod option fields.
	/// </summary>
	/// <param name="optionsDialog">The dialog to populate.</param>
	private void FillModOptions(PDialog optionsDialog)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		PPanel body = optionsDialog.Body;
		RectOffset margin = new RectOffset(CATEGORY_MARGIN, CATEGORY_MARGIN, CATEGORY_MARGIN, CATEGORY_MARGIN);
		body.Margin = new RectOffset();
		PPanel pPanel = new PPanel("ScrollContent")
		{
			Spacing = OUTER_MARGIN,
			Direction = PanelDirection.Vertical,
			Alignment = (TextAnchor)1,
			FlexSize = Vector2.right
		};
		IDictionary<string, ICollection<IOptionsEntry>> dictionary = optionCategories;
		IEnumerable<IOptionsEntry> enumerable;
		if (this.options is IOptions options && (enumerable = options.CreateOptions()) != null)
		{
			dictionary = new Dictionary<string, ICollection<IOptionsEntry>>(optionCategories);
			foreach (IOptionsEntry item in enumerable)
			{
				OptionsEntry.AddToCategory(dictionary, item);
			}
		}
		foreach (KeyValuePair<string, ICollection<IOptionsEntry>> item2 in dictionary)
		{
			string key = item2.Key;
			ICollection<IOptionsEntry> value = item2.Value;
			if (value.Count <= 0)
			{
				continue;
			}
			string text = (string.IsNullOrEmpty(key) ? "Default" : key);
			int row = 0;
			PGridPanel pGridPanel = new PGridPanel("Category_" + text)
			{
				Margin = margin,
				BackColor = PUITuning.Colors.DialogDarkBackground,
				FlexSize = Vector2.right
			};
			PGridPanel pGridPanel2 = new PGridPanel("Entries")
			{
				FlexSize = Vector2.right
			};
			AddCategoryHeader(pGridPanel, item2.Key, pGridPanel2);
			foreach (IOptionsEntry item3 in value)
			{
				pGridPanel2.AddRow(new GridRowSpec());
				item3.CreateUIEntry(pGridPanel2, ref row);
				row++;
			}
			pPanel.AddChild(pGridPanel);
		}
		pPanel.AddChild(new PPanel("ConfigButtons")
		{
			Spacing = 10,
			Direction = PanelDirection.Horizontal,
			Alignment = (TextAnchor)4,
			FlexSize = Vector2.right
		}.AddChild(new PButton("ManualConfig")
		{
			Text = LocString.op_Implicit(PLibStrings.BUTTON_MANUAL),
			ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_MANUAL),
			OnClick = OnManualConfig,
			TextAlignment = (TextAnchor)4,
			Margin = PDialog.BUTTON_MARGIN
		}.SetKleiBlueStyle()).AddChild(new PButton("ResetConfig")
		{
			Text = LocString.op_Implicit(PLibStrings.BUTTON_RESET),
			ToolTip = LocString.op_Implicit(PLibStrings.TOOLTIP_RESET),
			OnClick = OnResetConfig,
			TextAlignment = (TextAnchor)4,
			Margin = PDialog.BUTTON_MARGIN
		}.SetKleiBlueStyle()));
		body.AddChild(new PScrollPane
		{
			ScrollHorizontal = false,
			ScrollVertical = (dictionary.Count > 0),
			Child = pPanel,
			FlexSize = Vector2.right,
			TrackSize = 8f,
			AlwaysShowHorizontal = false,
			AlwaysShowVertical = false
		});
	}

	/// <summary>
	/// Invoked when the manual config button is pressed.
	/// </summary>
	private void OnManualConfig(GameObject _)
	{
		string text = null;
		string configFilePath = POptions.GetConfigFilePath(optionsType);
		try
		{
			text = new Uri(Path.GetDirectoryName(configFilePath) ?? configFilePath).AbsoluteUri;
		}
		catch (UriFormatException thrown)
		{
			PUtil.LogWarning("Unable to convert parent of " + configFilePath + " to a URI:");
			PUtil.LogExcWarn(thrown);
		}
		if (!string.IsNullOrEmpty(text))
		{
			bool num = WriteOptions();
			CloseDialog();
			PUtil.LogDebug("Opening config folder: " + text);
			Application.OpenURL(text);
			if (num)
			{
				PromptForRestart();
			}
		}
	}

	/// <summary>
	/// Invoked when the dialog is closed.
	/// </summary>
	/// <param name="action">The action key taken.</param>
	private void OnOptionsSelected(string action)
	{
		if (!(action == "ok"))
		{
			if (action == "close")
			{
				OnClose?.Invoke(options);
			}
		}
		else if (WriteOptions())
		{
			PromptForRestart();
		}
	}

	/// <summary>
	/// Invoked when the reset to default button is pressed.
	/// </summary>
	private void OnResetConfig(GameObject _)
	{
		options = CreateOptions(optionsType);
		UpdateOptions();
	}

	/// <summary>
	/// Brings up a restart dialog.
	/// </summary>
	private void PromptForRestart()
	{
		PUIElements.ShowConfirmDialog(null, LocString.op_Implicit(PLibStrings.RESTART_REQUIRED), SaveAndRestart, null, LocString.op_Implicit(PLibStrings.RESTART_OK), LocString.op_Implicit(PLibStrings.RESTART_CANCEL));
	}

	/// <summary>
	/// Triggered when the Mod Options button is clicked.
	/// </summary>
	public void ShowDialog()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		string title = ((!string.IsNullOrEmpty(displayInfo.Title)) ? string.Format(LocString.op_Implicit(PLibStrings.DIALOG_TITLE), OptionsEntry.LookInStrings(displayInfo.Title)) : LocString.op_Implicit(PLibStrings.BUTTON_OPTIONS));
		CloseDialog();
		PDialog pDialog = new PDialog("ModOptions")
		{
			Title = title,
			Size = SETTINGS_DIALOG_SIZE,
			SortKey = 150f,
			DialogBackColor = PUITuning.Colors.OptionsBackground,
			DialogClosed = OnOptionsSelected,
			MaxSize = SETTINGS_DIALOG_MAX_SIZE,
			RoundToNearestEven = true
		}.AddButton("ok", LocString.op_Implicit(CONFIRMDIALOG.OK), LocString.op_Implicit(PLibStrings.TOOLTIP_OK), PUITuning.Colors.ButtonPinkStyle).AddButton("close", LocString.op_Implicit(CONFIRMDIALOG.CANCEL), LocString.op_Implicit(PLibStrings.TOOLTIP_CANCEL), PUITuning.Colors.ButtonBlueStyle);
		options = POptions.ReadSettings(POptions.GetConfigFilePath(optionsType), optionsType) ?? CreateOptions(optionsType);
		AddModInfoScreen(pDialog);
		FillModOptions(pDialog);
		GameObject obj = pDialog.Build();
		UpdateOptions();
		if (obj.TryGetComponent<KScreen>(ref dialog))
		{
			dialog.Activate();
		}
	}

	/// <summary>
	/// Calls the user OnOptionsChanged handler if present.
	/// </summary>
	/// <param name="newOptions">The updated options object.</param>
	private void TriggerUpdateOptions(object newOptions)
	{
		if (newOptions is IOptions options)
		{
			options.OnOptionsChanged();
		}
		OnClose?.Invoke(newOptions);
	}

	/// <summary>
	/// Updates the dialog with the latest options from the file.
	/// </summary>
	private void UpdateOptions()
	{
		if (options == null)
		{
			return;
		}
		foreach (KeyValuePair<string, ICollection<IOptionsEntry>> optionCategory in optionCategories)
		{
			foreach (IOptionsEntry item in optionCategory.Value)
			{
				item.ReadFrom(options);
			}
		}
	}

	/// <summary>
	/// If configured, opens the mod's home page in the default browser.
	/// </summary>
	private void VisitModHomepage(GameObject _)
	{
		if (!string.IsNullOrWhiteSpace(displayInfo.URL))
		{
			Application.OpenURL(displayInfo.URL);
		}
	}

	/// <summary>
	/// Writes the mod options to its config file.
	/// </summary>
	/// <returns>true if a restart is requested, or false otherwise.</returns>
	private bool WriteOptions()
	{
		bool flag = false;
		if (options != null)
		{
			if (options.GetType().GetCustomAttribute(typeof(RestartRequiredAttribute)) != null)
			{
				flag = true;
			}
			foreach (KeyValuePair<string, ICollection<IOptionsEntry>> optionCategory in optionCategories)
			{
				foreach (IOptionsEntry item in optionCategory.Value)
				{
					flag |= item.WriteTo(options) && item.RestartRequired;
				}
			}
			POptions.WriteSettings(options, POptions.GetConfigFilePath(optionsType), configAttr?.IndentOutput ?? false);
			TriggerUpdateOptions(options);
		}
		return flag;
	}
}
