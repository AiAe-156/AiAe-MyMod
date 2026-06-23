using System;
using PeterHan.PLib.Core;

namespace PeterHan.PLib.UI;

/// <summary>
/// The specifications for one row in a grid layout.
/// </summary>
[Serializable]
public sealed class GridRowSpec
{
	/// <summary>
	/// The flexible height of this grid row. If there is space left after all rows
	/// get their nominal height, each row will get a fraction of the space left
	/// proportional to their FlexHeight value as a ratio to the total flexible
	/// height values.
	/// </summary>
	public float FlexHeight { get; }

	/// <summary>
	/// The nominal height of this grid row. If zero, the preferred height of the
	/// largest component is used. If there are no components in this row (possibly
	/// because the only components in this row all have row spans from other rows),
	/// the height will be zero!
	/// </summary>
	public float Height { get; }

	/// <summary>
	/// Creates a new grid row specification.
	/// </summary>
	/// <param name="height">The row's base width, or 0 to auto-size the row to the
	/// preferred height of its largest component.</param>
	/// <param name="flex">The percentage of the leftover height the row should occupy.</param>
	public GridRowSpec(float height = 0f, float flex = 0f)
	{
		if (height.IsNaNOrInfinity() || height < 0f)
		{
			throw new ArgumentOutOfRangeException("height");
		}
		if (flex.IsNaNOrInfinity() || flex < 0f)
		{
			throw new ArgumentOutOfRangeException("flex");
		}
		Height = height;
		FlexHeight = flex;
	}

	public override string ToString()
	{
		return $"GridRowSpec[Height={Height:F2}]";
	}
}
