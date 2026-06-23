using System;

namespace PeterHan.PLib.UI;

/// <summary>
/// A component in the grid with its placement information.
/// </summary>
[Serializable]
internal sealed class GridComponent<T> : GridComponentSpec where T : class
{
	/// <summary>
	/// The object to place here.
	/// </summary>
	public T Item { get; }

	internal GridComponent(GridComponentSpec spec, T item)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		base.Alignment = spec.Alignment;
		Item = item;
		base.Column = spec.Column;
		base.ColumnSpan = spec.ColumnSpan;
		base.Margin = spec.Margin;
		base.Row = spec.Row;
		base.RowSpan = spec.RowSpan;
	}
}
