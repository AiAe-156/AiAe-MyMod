using System;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options;

/// <summary>
/// An options entry which represents float and displays a text field and slider.
/// This entry uses a logarithmic scale.
/// </summary>
public class LogFloatOptionsEntry : SlidingBaseOptionsEntry
{
	/// <summary>
	/// The format to use if none is provided.
	/// </summary>
	public const string DEFAULT_FORMAT = "F2";

	/// <summary>
	/// The realized text field.
	/// </summary>
	private GameObject textField;

	/// <summary>
	/// The value in the text field.
	/// </summary>
	private float value;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value is float num)
			{
				this.value = limits.ClampToRange(num);
				Update();
			}
		}
	}

	public LogFloatOptionsEntry(string field, IOptionSpec spec, LimitAttribute limit)
		: base(field, spec, limit)
	{
		if (limit == null)
		{
			throw new ArgumentNullException("limit");
		}
		if (limit.Minimum <= 0.0 || limit.Maximum <= 0.0)
		{
			throw new ArgumentOutOfRangeException("limit", "Logarithmic values must be positive");
		}
		textField = null;
		value = limit.ClampToRange(1f);
	}

	protected override PSliderSingle GetSlider()
	{
		return new PSliderSingle
		{
			OnValueChanged = OnSliderChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			MinValue = Mathf.Log((float)limits.Minimum),
			MaxValue = Mathf.Log((float)limits.Maximum),
			InitialValue = Mathf.Log(value)
		};
	}

	public override GameObject GetUIComponent()
	{
		textField = new PTextField
		{
			OnTextChanged = OnTextChanged,
			ToolTip = OptionsEntry.LookInStrings(base.Tooltip),
			Text = value.ToString(base.Format ?? "F2"),
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
		value = limits.ClampToRange(Mathf.Exp(newValue));
		Update();
	}

	/// <summary>
	/// Called when the input field's text is changed.
	/// </summary>
	/// <param name="text">The new text.</param>
	private void OnTextChanged(GameObject _, string text)
	{
		if (float.TryParse(text, out var result))
		{
			if (base.Format != null && base.Format.ToUpperInvariant().IndexOf('P') >= 0)
			{
				result *= 0.01f;
			}
			value = limits.ClampToRange(result);
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
			val.text = value.ToString(base.Format ?? "F2");
		}
		if ((Object)(object)slider != (Object)null)
		{
			PSliderSingle.SetCurrentValue(slider, Mathf.Log(Mathf.Max(float.Epsilon, value)));
		}
	}
}
