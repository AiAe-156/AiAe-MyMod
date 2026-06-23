using System;
using UnityEngine;

namespace FUtility.FUI;

public class FNumberInputField : FInputField
{
	public int maxValue = int.MaxValue;

	public int minValue = int.MinValue;

	public float GetFloat
	{
		get
		{
			float.TryParse(inputField.text, out var result);
			return Mathf.Clamp(result, (float)minValue, (float)maxValue);
		}
	}

	public T GetValue<T>()
	{
		if (float.TryParse(inputField.text, out var result))
		{
			result = Mathf.Clamp(result, (float)minValue, (float)maxValue);
			return (T)Convert.ChangeType(result, typeof(T));
		}
		return default(T);
	}
}
