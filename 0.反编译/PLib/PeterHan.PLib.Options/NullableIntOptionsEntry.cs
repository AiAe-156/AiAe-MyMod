using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An options entry which represents int? and displays a text field and slider.
/// </summary>
public class NullableIntOptionsEntry : SlidingBaseOptionsEntry
{
	/// <summary>
	/// The realized text field.
	/// </summary>
	private GameObject textField;

	/// <summary>
	/// The value in the text field.
	/// </summary>
	private int? value;

	/// <summary>
	/// The text that is rendered for the current value of the entry.
	/// </summary>
	protected virtual string FieldText => value?.ToString(base.Format ?? "D") ?? string.Empty;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value == null)
			{
				this.value = null;
				Update();
			}
			else if (value is int num)
			{
				this.value = num;
				Update();
			}
		}
	}

	public NullableIntOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
		: base(field, spec, limit)
	{
		textField = null;
		value = null;
	}

	protected override PSliderSingle GetSlider()
	{
		float num = (float)limits.Minimum;
		float maxValue = (float)limits.Maximum;
		return new PSliderSingle
		{
			OnValueChanged = OnSliderChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			MinValue = num,
			MaxValue = maxValue,
			InitialValue = num,
			IntegersOnly = true
		};
	}

	public override GameObject GetUIComponent()
	{
		textField = new PTextField
		{
			OnTextChanged = OnTextChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			Text = FieldText,
			MinWidth = 64,
			MaxLength = 10,
			Type = PTextField.FieldType.Integer
		}.Build();
		Update();
		return textField;
	}

	/// <summary>
	/// Called when the slider's value is changed.
	/// </summary>
	/// <param name="newValue">The new slider value.</param>
	private void OnSliderChanged(GameObject _, float newValue)
	{
		int num = Mathf.RoundToInt(newValue);
		if (limits != null)
		{
			num = limits.ClampToRange(num);
		}
		value = num;
		Update();
	}

	/// <summary>
	/// Called when the input field's text is changed.
	/// </summary>
	/// <param name="text">The new text.</param>
	private void OnTextChanged(GameObject _, string text)
	{
		int result;
		if (string.IsNullOrWhiteSpace(text))
		{
			value = null;
		}
		else if (int.TryParse(text, out result))
		{
			if (limits != null)
			{
				result = limits.ClampToRange(result);
			}
			value = result;
		}
		Update();
	}

	/// <summary>
	/// Updates the displayed value.
	/// </summary>
	protected override void Update()
	{
		GameObject obj = textField;
		TMP_InputField val = ((obj != null) ? obj.GetComponentInChildren<TMP_InputField>() : null);
		if ((Object)(object)val != (Object)null)
		{
			val.text = FieldText;
		}
		if ((Object)(object)slider != (Object)null && value.HasValue)
		{
			PSliderSingle.SetCurrentValue(slider, value.Value);
		}
	}
}
