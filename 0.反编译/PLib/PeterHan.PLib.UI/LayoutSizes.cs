using System;
using UnityEngine;

namespace PeterHan.PLib.UI;

/// <summary>
/// A class representing the size sets of a particular component.
/// </summary>
internal struct LayoutSizes
{
	/// <summary>
	/// The flexible dimension value.
	/// </summary>
	public float flexible;

	/// <summary>
	/// If true, this component should be ignored completely.
	/// </summary>
	public bool ignore;

	/// <summary>
	/// The minimum dimension value.
	/// </summary>
	public float min;

	/// <summary>
	/// The preferred dimension value.
	/// </summary>
	public float preferred;

	/// <summary>
	/// The source of these values.
	/// </summary>
	public readonly GameObject source;

	internal LayoutSizes(GameObject source)
		: this(source, 0f, 0f, 0f)
	{
	}

	internal LayoutSizes(GameObject source, float min, float preferred, float flexible)
	{
		ignore = false;
		this.source = source;
		this.flexible = flexible;
		this.min = min;
		this.preferred = preferred;
	}

	/// <summary>
	/// Adds another set of layout sizes to this one.
	/// </summary>
	/// <param name="other">The size values to add.</param>
	public void Add(LayoutSizes other)
	{
		flexible += other.flexible;
		min += other.min;
		preferred += other.preferred;
	}

	/// <summary>
	/// Enlarges this layout size, if necessary, using the values from another.
	/// </summary>
	/// <param name="other">The minimum size values to enforce.</param>
	public void Max(LayoutSizes other)
	{
		flexible = Math.Max(flexible, other.flexible);
		min = Math.Max(min, other.min);
		preferred = Math.Max(preferred, other.preferred);
	}

	public override string ToString()
	{
		return $"LayoutSizes[min={min:F2},preferred={preferred:F2},flexible={flexible:F2}]";
	}
}
