using System;

namespace UtilLibs.UI.FUI.Unity_UI_Extensions.Scripts.Utilities;

[Serializable]
public struct MinMaxValues
{
	public const float FLOAT_TOL = 0.01f;

	public float minValue;

	public float maxValue;

	public float minLimit;

	public float maxLimit;

	public static MinMaxValues DEFUALT = new MinMaxValues(25f, 75f, 0f, 100f);

	public MinMaxValues(float minValue, float maxValue, float minLimit, float maxLimit)
	{
		this.minValue = minValue;
		this.maxValue = maxValue;
		this.minLimit = minLimit;
		this.maxLimit = maxLimit;
	}

	public MinMaxValues(float minValue, float maxValue)
	{
		this.minValue = minValue;
		this.maxValue = maxValue;
		minLimit = minValue;
		maxLimit = maxValue;
	}

	public bool IsAtMinAndMax()
	{
		return Math.Abs(minValue - minLimit) < 0.01f && Math.Abs(maxValue - maxLimit) < 0.01f;
	}

	public override string ToString()
	{
		return $"Values(min:{minValue}, max:{maxValue}) | Limits(min:{minLimit}, max:{maxLimit})";
	}
}
