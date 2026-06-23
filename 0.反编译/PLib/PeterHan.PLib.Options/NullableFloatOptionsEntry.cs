using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An options entry which represents float? and displays a text field and slider.
/// </summary>
public class NullableFloatOptionsEntry : SlidingBaseOptionsEntry
{
	/// <summary>
	/// The realized text field.
	/// </summary>
	private GameObject textField;

	/// <summary>
	/// The value in the text field.
	/// </summary>
	private float? value;

	/// <summary>
	/// The text that is rendered for the current value of the entry.
	/// </summary>
	protected virtual string FieldText => value?.ToString(base.Format ?? "F2") ?? string.Empty;

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
			else if (value is float num)
			{
				this.value = num;
				Update();
			}
		}
	}

	public NullableFloatOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
		: base(field, spec, limit)
	{
		textField = null;
		value = null;
	}

	protected override PSliderSingle GetSlider()
	{
		float num = (float)limits.Minimum;
		float num2 = (float)limits.Maximum;
		return new PSliderSingle
		{
			OnValueChanged = OnSliderChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			MinValue = num,
			MaxValue = num2,
			InitialValue = 0.5f * (num + num2)
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
			MaxLength = 16,
			Type = PTextField.FieldType.Float
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
		if (limits != null)
		{
			value = limits.ClampToRange(newValue);
		}
		else
		{
			value = newValue;
		}
		Update();
	}

	/// <summary>
	/// Called when the input field's text is changed.
	/// </summary>
	/// <param name="text">The new text.</param>
	private void OnTextChanged(GameObject _, string text)
	{
		float result;
		if (string.IsNullOrWhiteSpace(text))
		{
			value = null;
		}
		else if (float.TryParse(text, out result))
		{
			if (base.Format != null && base.Format.ToUpperInvariant().IndexOf('P') >= 0)
			{
				result *= 0.01f;
			}
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
