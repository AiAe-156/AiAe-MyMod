using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.UI;

/// <summary>
/// The specifications for one column in a grid layout.
/// </summary>
[Serializable]
public sealed class GridColumnSpec
{
	/// <summary>
	/// The flexible width of this grid column. If there is space left after all
	/// columns get their nominal width, each column will get a fraction of the space
	/// left proportional to their FlexWidth value as a ratio to the total flexible
	/// width values.
	/// </summary>
	public float FlexWidth { get; }

	/// <summary>
	/// The nominal width of this grid column. If zero, the preferred width of the
	/// largest component is used. If there are no components in this column (possibly
	/// because the only components in this row all have column spans from other
	/// columns), the width will be zero!
	/// </summary>
	public float Width { get; }

	/// <summary>
	/// Creates a new grid column specification.
	/// </summary>
	/// <param name="width">The column's base width, or 0 to auto-size the column to the
	/// preferred width of its largest component.</param>
	/// <param name="flex">The percentage of the leftover width the column should occupy.</param>
	public GridColumnSpec(float width = 0f, float flex = 0f)
	{
		if (width.IsNaNOrInfinity() || width < 0f)
		{
			throw new ArgumentOutOfRangeException("width");
		}
		if (flex.IsNaNOrInfinity() || flex < 0f)
		{
			throw new ArgumentOutOfRangeException("flex");
		}
		Width = width;
		FlexWidth = flex;
	}

	public override string ToString()
	{
		return $"GridColumnSpec[Width={Width:F2}]";
	}
}
