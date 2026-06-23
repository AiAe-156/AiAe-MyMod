using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An options entry which represents Enum and displays a spinner with text options.
/// </summary>
public class SelectOneOptionsEntry : OptionsEntry
{
	/// <summary>
	/// Represents a selectable option.
	/// </summary>
	protected sealed class EnumOption : ITooltipListableOption, IListableOption
	{
		/// <summary>
		/// The option title.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// The option tool tip.
		/// </summary>
		public string ToolTip { get; }

		/// <summary>
		/// The value to assign if this option is chosen.
		/// </summary>
		public object Value { get; }

		public EnumOption(string title, string toolTip, object value)
		{
			Title = title ?? throw new ArgumentNullException("title");
			ToolTip = toolTip;
			Value = value;
		}

		public string GetProperName()
		{
			return Title;
		}

		public string GetToolTipText()
		{
			return ToolTip;
		}

		public override string ToString()
		{
			return $"Option[Title={Title},Value={Value}]";
		}
	}

	/// <summary>
	/// The chosen item in the array.
	/// </summary>
	protected EnumOption chosen;

	/// <summary>
	/// The realized item label.
	/// </summary>
	private GameObject comboBox;

	/// <summary>
	/// The available options to cycle through.
	/// </summary>
	protected readonly IList<EnumOption> options;

	public override object Value
	{
		get
		{
			return chosen?.Value;
		}
		set
		{
			string text = value?.ToString() ?? "";
			foreach (EnumOption option in options)
			{
				if (text == option.Value.ToString())
				{
					chosen = option;
					Update();
					break;
				}
			}
		}
	}

	/// <summary>
	/// Obtains the title and tool tip for an enumeration value.
	/// </summary>
	/// <param name="enumValue">The value in the enumeration.</param>
	/// <param name="fieldType">The type of the Enum field.</param>
	/// <returns>The matching Option</returns>
	private static EnumOption GetAttribute(object enumValue, Type fieldType)
	{
		if (enumValue == null)
		{
			throw new ArgumentNullException("enumValue");
		}
		string text = enumValue.ToString();
		string title = text;
		string toolTip = "";
		MemberInfo[] member = fieldType.GetMember(text, BindingFlags.Static | BindingFlags.Public);
		foreach (MemberInfo memberInfo in member)
		{
			if (!(memberInfo.DeclaringType == fieldType))
			{
				continue;
			}
			object[] customAttributes = memberInfo.GetCustomAttributes(inherit: false);
			foreach (object obj in customAttributes)
			{
				IOptionSpec optionSpec = obj as IOptionSpec;
				if (optionSpec != null)
				{
					if (string.IsNullOrEmpty(optionSpec.Title))
					{
						optionSpec = OptionsEntry.HandleDefaults(optionSpec, memberInfo);
					}
					title = OptionsEntry.LookInStrings(optionSpec.Title);
					toolTip = OptionsEntry.LookInStrings(optionSpec.Tooltip);
					break;
				}
				if (obj is EnumMemberAttribute { IsValueSetExplicitly: not false } enumMemberAttribute)
				{
					title = OptionsEntry.LookInStrings(enumMemberAttribute.Value);
					break;
				}
			}
			break;
		}
		return new EnumOption(title, toolTip, enumValue);
	}

	public SelectOneOptionsEntry(string field, IOptionSpec spec, Type fieldType)
		: base(field, spec)
	{
		Array values = Enum.GetValues(fieldType);
		if (values == null)
		{
			throw new ArgumentException("No values, or invalid values, for enum");
		}
		int length = values.Length;
		if (length == 0)
		{
			throw new ArgumentException("Enum has no declared members");
		}
		chosen = null;
		comboBox = null;
		options = new List<EnumOption>(length);
		for (int i = 0; i < length; i++)
		{
			options.Add(GetAttribute(values.GetValue(i), fieldType));
		}
	}

	public override GameObject GetUIComponent()
	{
		EnumOption enumOption = null;
		int num = 0;
		foreach (EnumOption option in options)
		{
			int valueOrDefault = (option.Title?.Trim()?.Length).GetValueOrDefault();
			if (enumOption == null && valueOrDefault > 0)
			{
				enumOption = option;
			}
			if (valueOrDefault > num)
			{
				num = valueOrDefault;
			}
		}
		comboBox = new PComboBox<EnumOption>("Select")
		{
			BackColor = PUITuning.Colors.ButtonPinkStyle,
			InitialItem = enumOption,
			Content = options,
			EntryColor = PUITuning.Colors.ButtonBlueStyle,
			TextStyle = PUITuning.Fonts.TextLightStyle,
			OnOptionSelected = UpdateValue
		}.SetMinWidthInCharacters(num).Build();
		Update();
		return comboBox;
	}

	/// <summary>
	/// Updates the displayed text to match the current item.
	/// </summary>
	private void Update()
	{
		if ((Object)(object)comboBox != (Object)null && chosen != null)
		{
			PComboBox<EnumOption>.SetSelectedItem(comboBox, (IListableOption)(object)chosen);
		}
	}

	/// <summary>
	/// Triggered when the value chosen from the combo box has been changed.
	/// </summary>
	/// <param name="selected">The value selected by the user.</param>
	private void UpdateValue(GameObject _, EnumOption selected)
	{
		if (selected != null)
		{
			chosen = selected;
		}
	}
}
