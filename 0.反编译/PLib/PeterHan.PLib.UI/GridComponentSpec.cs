using System;
using UnityEngine;

namespace PeterHan.PLib.UI;

/// <summary>
/// Stores the state of a component in a grid layout.
/// </summary>
public class GridComponentSpec
{
	/// <summary>
	/// The alignment of the component.
	/// </summary>
	public TextAnchor Alignment { get; set; }

	/// <summary>
	/// The column of the component.
	/// </summary>
	public int Column { get; set; }

	/// <summary>
	/// The number of columns this component spans.
	/// </summary>
	public int ColumnSpan { get; set; }

	/// <summary>
	/// The margin to allocate around each component.
	/// </summary>
	public RectOffset Margin { get; set; }

	/// <summary>
	/// The row of the component.
	/// </summary>
	public int Row { get; set; }

	/// <summary>
	/// The number of rows this component spans.
	/// </summary>
	public int RowSpan { get; set; }

	internal GridComponentSpec()
	{
	}

	/// <summary>
	/// Creates a new grid component specification. While the row and column are mandatory,
	/// the other attributes can be optionally specified in the initializer.
	/// </summary>
	/// <param name="row">The row to place the component.</param>
	/// <param name="column">The column to place the component.</param>
	public GridComponentSpec(int row, int column)
	{
		if (row < 0)
		{
			throw new ArgumentOutOfRangeException("row");
		}
		if (column < 0)
		{
			throw new ArgumentOutOfRangeException("column");
		}
		Alignment = (TextAnchor)4;
		Row = row;
		Column = column;
		Margin = null;
		RowSpan = 1;
		ColumnSpan = 1;
	}

	public override string ToString()
	{
		return $"GridComponentSpec[Row={Row:D},Column={Column:D},RowSpan={RowSpan:D},ColumnSpan={ColumnSpan:D}]";
	}
}
