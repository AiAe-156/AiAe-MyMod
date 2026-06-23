using System;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An options entry which represents a string and displays a text field.
/// </summary>
public class StringOptionsEntry : OptionsEntry
{
	/// <summary>
	/// The maximum entry length.
	/// </summary>
	private readonly int maxLength;

	/// <summary>
	/// The realized text field.
	/// </summary>
	private GameObject textField;

	/// <summary>
	/// The value in the text field.
	/// </summary>
	private string value;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = ((value == null) ? "" : value.ToString());
			Update();
		}
	}

	public StringOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
		: base(field, spec)
	{
		if (limit != null)
		{
			maxLength = Math.Max(2, (int)Math.Round(limit.Maximum));
		}
		else
		{
			maxLength = 256;
		}
		textField = null;
		value = "";
	}

	public override GameObject GetUIComponent()
	{
		textField = new PTextField
		{
			OnTextChanged = OnTextChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			Text = value.ToString(),
			MinWidth = 128,
			Type = PTextField.FieldType.Text,
			MaxLength = maxLength
		}.Build();
		Update();
		return textField;
	}

	/// <summary>
	/// Called when the input field's text is changed.
	/// </summary>
	/// <param name="text">The new text.</param>
	private void OnTextChanged(GameObject _, string text)
	{
		value = text;
		Update();
	}

	/// <summary>
	/// Updates the displayed value.
	/// </summary>
	private void Update()
	{
		TMP_InputField componentInChildren;
		if ((Object)(object)textField != (Object)null && (Object)(object)(componentInChildren = textField.GetComponentInChildren<TMP_InputField>()) != (Object)null)
		{
			componentInChildren.text = value;
		}
	}
}
