using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

public class SelectOneOptionsEntry : OptionsEntry
{
	protected sealed class EnumOption : ITooltipListableOption, IListableOption
	{
		public string Title { get; }

		public string ToolTip { get; }

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

	protected EnumOption chosen;

	private GameObject comboBox;

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

	private static EnumOption GetAttribute(object enumValue, Type fieldType)
	{
		if (enumValue == null)
		{
			throw new ArgumentNullException("enumValue");
		}
		string text = enumValue.ToString();
		MemberInfo[] member = fieldType.GetMember(text, BindingFlags.Static | BindingFlags.Public);
		int num = member.Length;
		for (int i = 0; i < num; i++)
		{
			MemberInfo memberInfo = member[i];
			if (memberInfo.DeclaringType == fieldType)
			{
				return CreateOption(enumValue, memberInfo, text);
			}
		}
		return new EnumOption(text, "", enumValue);
	}

	private static EnumOption CreateOption(object enumValue, MemberInfo enumField, string valueName)
	{
		object[] customAttributes = enumField.GetCustomAttributes(inherit: false);
		int num = customAttributes.Length;
		string title = valueName;
		string toolTip = "";
		bool flag = true;
		bool flag2 = false;
		for (int i = 0; i < num && flag; i++)
		{
			object obj = customAttributes[i];
			IOptionSpec optionSpec = obj as IOptionSpec;
			if (optionSpec != null && !flag2)
			{
				if (string.IsNullOrEmpty(optionSpec.Title))
				{
					optionSpec = OptionsEntry.HandleDefaults(optionSpec, enumField);
				}
				title = OptionsEntry.LookInStrings(optionSpec.Title);
				toolTip = OptionsEntry.LookInStrings(optionSpec.Tooltip);
				flag2 = true;
			}
			if (obj is EnumMemberAttribute { IsValueSetExplicitly: not false } enumMemberAttribute && !flag2)
			{
				title = OptionsEntry.LookInStrings(enumMemberAttribute.Value);
				flag2 = true;
			}
			if (obj is IRequireFilter requireFilter && !requireFilter.Filter())
			{
				flag = false;
			}
		}
		if (!flag)
		{
			return null;
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
			EnumOption attribute = GetAttribute(values.GetValue(i), fieldType);
			if (attribute != null)
			{
				options.Add(attribute);
			}
		}
		if (options.Count < 1)
		{
			options.Add(new EnumOption(LocString.op_Implicit(PLibStrings.OPTIONS_FILTERED), "", values.GetValue(0)));
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

	private void Update()
	{
		if ((Object)(object)comboBox != (Object)null && chosen != null)
		{
			PComboBox<EnumOption>.SetSelectedItem(comboBox, (IListableOption)(object)chosen);
		}
	}

	private void UpdateValue(GameObject _, EnumOption selected)
	{
		if (selected != null)
		{
			chosen = selected;
		}
	}
}
