using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An options entry which represents int and displays a text field and slider.
/// </summary>
public class IntOptionsEntry : SlidingBaseOptionsEntry
{
	/// <summary>
	/// The realized text field.
	/// </summary>
	private GameObject textField;

	/// <summary>
	/// The value in the text field.
	/// </summary>
	private int value;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value is int num)
			{
				this.value = num;
				Update();
			}
		}
	}

	public IntOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit = null)
		: base(field, spec, limit)
	{
		textField = null;
		value = 0;
	}

	protected override PSliderSingle GetSlider()
	{
		return new PSliderSingle
		{
			OnValueChanged = OnSliderChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			MinValue = (float)limits.Minimum,
			MaxValue = (float)limits.Maximum,
			InitialValue = value,
			IntegersOnly = true
		};
	}

	public override GameObject GetUIComponent()
	{
		textField = new PTextField
		{
			OnTextChanged = OnTextChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			Text = value.ToString(base.Format ?? "D"),
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
		if (int.TryParse(text, out var result))
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
			val.text = value.ToString(base.Format ?? "D");
		}
		if ((Object)(object)slider != (Object)null)
		{
			PSliderSingle.SetCurrentValue(slider, value);
		}
	}
}
