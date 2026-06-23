using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.Options;

/// <summary>
/// An attribute placed on an option field for a property used as mod options to define
/// minimum and maximum acceptable values.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class LimitAttribute : Attribute
{
	/// <summary>
	/// The maximum value (inclusive).
	/// </summary>
	public double Maximum { get; }

	/// <summary>
	/// The minimum value (inclusive).
	/// </summary>
	public double Minimum { get; }

	public LimitAttribute(double min, double max)
	{
		Minimum = (min.IsNaNOrInfinity() ? 0.0 : min);
		Maximum = ((max.IsNaNOrInfinity() || max < min) ? min : max);
	}

	/// <summary>
	/// Clamps the specified value to the range of this Limits object.
	/// </summary>
	/// <param name="value">The value to coerce.</param>
	/// <returns>The nearest value included by these limits to the specified value.</returns>
	public float ClampToRange(float value)
	{
		return value.InRange((float)Minimum, (float)Maximum);
	}

	/// <summary>
	/// Clamps the specified value to the range of this Limits object.
	/// </summary>
	/// <param name="value">The value to coerce.</param>
	/// <returns>The nearest value included by these limits to the specified value.</returns>
	public int ClampToRange(int value)
	{
		return value.InRange((int)Minimum, (int)Maximum);
	}

	/// <summary>
	/// Reports whether a value is in the range included in these limits.
	/// </summary>
	/// <param name="value">The value to check.</param>
	/// <returns>true if it is included in the limits, or false otherwise.</returns>
	public bool InRange(double value)
	{
		if (value >= Minimum)
		{
			return value <= Maximum;
		}
		return false;
	}

	public override string ToString()
	{
		return "{0:F2} to {1:F2}".F(Minimum, Maximum);
	}
}
