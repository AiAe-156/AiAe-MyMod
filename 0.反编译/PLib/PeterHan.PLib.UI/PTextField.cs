using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

/// <summary>
/// A custom UI text field factory class.
/// </summary>
public sealed class PTextField : IUIComponent
{
	/// <summary>
	/// The valid text field types supported by this class.
	/// </summary>
	public enum FieldType
	{
		Text,
		Integer,
		Float
	}

	/// <summary>
	/// The text field's background color.
	/// </summary>
	public Color BackColor { get; set; }

	/// <summary>
	/// Retrieves the built-in field type used for Text Mesh Pro.
	/// </summary>
	private ContentType ContentType => (ContentType)(Type switch
	{
		FieldType.Float => 3, 
		FieldType.Integer => 2, 
		_ => 0, 
	});

	/// <summary>
	/// The flexible size bounds of this component.
	/// </summary>
	public Vector2 FlexSize { get; set; }

	/// <summary>
	/// The maximum number of characters in this text field.
	/// </summary>
	public int MaxLength { get; set; }

	/// <summary>
	/// The minimum width in units (not characters!) of this text field.
	/// </summary>
	public int MinWidth { get; set; }

	/// <summary>
	/// The placeholder text style (including color, font, and word wrap settings) if the
	/// field is empty.
	/// </summary>
	public TextStyleSetting PlaceholderStyle { get; set; }

	/// <summary>
	/// The placeholder text if the field is empty.
	/// </summary>
	public string PlaceholderText { get; set; }

	public string Name { get; }

	/// <summary>
	/// The text alignment in the text field.
	/// </summary>
	public TextAlignmentOptions TextAlignment { get; set; }

	/// <summary>
	/// The initial text in the text field.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// The text field's text color, font, word wrap settings, and font size.
	/// </summary>
	public TextStyleSetting TextStyle { get; set; }

	/// <summary>
	/// The tool tip text.
	/// </summary>
	public string ToolTip { get; set; }

	/// <summary>
	/// The field type.
	/// </summary>
	public FieldType Type { get; set; }

	/// <summary>
	/// The action to trigger on text change. It is passed the realized source object.
	/// </summary>
	public PUIDelegates.OnTextChanged OnTextChanged { get; set; }

	/// <summary>
	/// The callback to invoke when validating input.
	/// </summary>
	public OnValidateInput OnValidate { get; set; }

	public event PUIDelegates.OnRealize OnRealize;

	internal static TextMeshProUGUI ConfigureField(TextMeshProUGUI component, TextStyleSetting style, TextAlignmentOptions alignment)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)component).alignment = alignment;
		((TMP_Text)component).autoSizeTextContainer = false;
		((Behaviour)component).enabled = true;
		((Graphic)component).color = style.textColor;
		((TMP_Text)component).font = style.sdfFont;
		((TMP_Text)component).fontSize = style.fontSize;
		((TMP_Text)component).fontStyle = style.style;
		((TMP_Text)component).overflowMode = (TextOverflowModes)0;
		return component;
	}

	/// <summary>
	/// Gets a text field's text.
	/// </summary>
	/// <param name="textField">The UI element to retrieve.</param>
	/// <returns>The current text in the field.</returns>
	public static string GetText(GameObject textField)
	{
		if ((Object)(object)textField == (Object)null)
		{
			throw new ArgumentNullException("textField");
		}
		TMP_InputField val = default(TMP_InputField);
		if (!textField.TryGetComponent<TMP_InputField>(ref val))
		{
			return "";
		}
		return val.text;
	}

	public PTextField()
		: this(null)
	{
	}

	public PTextField(string name)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		BackColor = PUITuning.Colors.BackgroundLight;
		FlexSize = Vector2.zero;
		MaxLength = 256;
		MinWidth = 32;
		Name = name ?? "TextField";
		PlaceholderText = null;
		Text = null;
		TextAlignment = (TextAlignmentOptions)514;
		TextStyle = PUITuning.Fonts.TextDarkStyle;
		PlaceholderStyle = TextStyle;
		ToolTip = "";
		Type = FieldType.Text;
	}

	/// <summary>
	/// Adds a handler when this text field is realized.
	/// </summary>
	/// <param name="onRealize">The handler to invoke on realization.</param>
	/// <returns>This text field for call chaining.</returns>
	public PTextField AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		OnRealize += onRealize;
		return this;
	}

	public GameObject Build()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = PUIElements.CreateUI(null, Name);
		TextStyleSetting val2 = TextStyle ?? PUITuning.Fonts.TextLightStyle;
		Image obj = val.AddComponent<Image>();
		obj.sprite = PUITuning.Images.BoxBorderWhite;
		obj.type = (Type)1;
		((Graphic)obj).color = val2.textColor;
		GameObject val3 = PUIElements.CreateUI(val, "Text Area", canvas: false);
		((Graphic)val3.AddComponent<Image>()).color = BackColor;
		RectMask2D obj2 = val3.AddComponent<RectMask2D>();
		GameObject val4 = PUIElements.CreateUI(val3, "Text");
		TextMeshProUGUI val5 = ConfigureField(val4.AddComponent<TextMeshProUGUI>(), val2, TextAlignment);
		((TMP_Text)val5).enableWordWrapping = false;
		((TMP_Text)val5).maxVisibleLines = 1;
		((Graphic)val5).raycastTarget = true;
		val.SetActive(false);
		TMP_InputField val6 = val.AddComponent<TMP_InputField>();
		val6.textComponent = (TMP_Text)(object)val5;
		val6.textViewport = Util.rectTransform(val3);
		val6.text = Text ?? "";
		((TMP_Text)val5).text = Text ?? "";
		if (PlaceholderText != null)
		{
			TextMeshProUGUI val7 = ConfigureField(PUIElements.CreateUI(val3, "Placeholder Text").AddComponent<TextMeshProUGUI>(), PlaceholderStyle ?? val2, TextAlignment);
			((TMP_Text)val7).maxVisibleLines = 1;
			((TMP_Text)val7).text = PlaceholderText;
			val6.placeholder = (Graphic)(object)val7;
		}
		ConfigureTextEntry(val6);
		PTextFieldEvents pTextFieldEvents = val.AddComponent<PTextFieldEvents>();
		pTextFieldEvents.OnTextChanged = OnTextChanged;
		pTextFieldEvents.OnValidate = OnValidate;
		pTextFieldEvents.TextObject = val4;
		PUIElements.SetToolTip(val, ToolTip);
		((Behaviour)obj2).enabled = true;
		PUIElements.SetAnchorOffsets(val4, new RectOffset());
		val.SetActive(true);
		RectTransform val8 = Util.rectTransform(val4);
		LayoutRebuilder.ForceRebuildLayoutImmediate(val8);
		LayoutElement obj3 = EntityTemplateExtensions.AddOrGet<LayoutElement>(PUIUtils.InsetChild(val, val3, Vector2.one, new Vector2((float)MinWidth, LayoutUtility.GetPreferredHeight(val8))));
		obj3.flexibleWidth = FlexSize.x;
		obj3.flexibleHeight = FlexSize.y;
		this.OnRealize?.Invoke(val);
		return val;
	}

	/// <summary>
	/// Sets up the text entry field.
	/// </summary>
	/// <param name="textEntry">The input field to configure.</param>
	private void ConfigureTextEntry(TMP_InputField textEntry)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		textEntry.characterLimit = Math.Max(1, MaxLength);
		textEntry.contentType = ContentType;
		((Behaviour)textEntry).enabled = true;
		textEntry.inputType = (InputType)0;
		((Selectable)textEntry).interactable = true;
		textEntry.isRichTextEditingAllowed = false;
		textEntry.keyboardType = (TouchScreenKeyboardType)0;
		textEntry.lineType = (LineType)0;
		((Selectable)textEntry).navigation = Navigation.defaultNavigation;
		textEntry.richText = false;
		textEntry.selectionColor = PUITuning.Colors.SelectionBackground;
		((Selectable)textEntry).transition = (Transition)0;
		textEntry.restoreOriginalTextOnEscape = true;
	}

	/// <summary>
	/// Sets the default Klei pink style as this text field's color and text style.
	/// </summary>
	/// <returns>This text field for call chaining.</returns>
	public PTextField SetKleiPinkStyle()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		TextStyle = PUITuning.Fonts.UILightStyle;
		BackColor = PUITuning.Colors.ButtonPinkStyle.inactiveColor;
		return this;
	}

	/// <summary>
	/// Sets the default Klei blue style as this text field's color and text style.
	/// </summary>
	/// <returns>This text field for call chaining.</returns>
	public PTextField SetKleiBlueStyle()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		TextStyle = PUITuning.Fonts.UILightStyle;
		BackColor = PUITuning.Colors.ButtonBlueStyle.inactiveColor;
		return this;
	}

	/// <summary>
	/// Sets the minimum (and preferred) width of this text field in characters.
	///
	/// The width is computed using the currently selected text style.
	/// </summary>
	/// <param name="chars">The number of characters to be displayed.</param>
	/// <returns>This text field for call chaining.</returns>
	public PTextField SetMinWidthInCharacters(int chars)
	{
		int num = Mathf.RoundToInt((float)chars * PUIUtils.GetEmWidth(TextStyle));
		if (num > 0)
		{
			MinWidth = num;
		}
		return this;
	}

	public override string ToString()
	{
		return $"PTextField[Name={Name},Type={Type}]";
	}
}
