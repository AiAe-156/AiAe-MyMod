using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.PLib.UI.Layouts;

/// <summary>
/// A class which stores the results of a single grid layout calculation pass.
/// </summary>
internal sealed class GridLayoutResults
{
	/// <summary>
	/// The columns in the grid.
	/// </summary>
	public IList<GridColumnSpec> ColumnSpecs { get; }

	/// <summary>
	/// The components in the grid, in order of addition.
	/// </summary>
	public ICollection<SizedGridComponent> Components { get; }

	/// <summary>
	/// The number of columns in the grid.
	/// </summary>
	public int Columns { get; }

	/// <summary>
	/// The columns in the grid with their calculated widths.
	/// </summary>
	public IList<GridColumnSpec> ComputedColumnSpecs { get; }

	/// <summary>
	/// The rows in the grid with their calculated heights.
	/// </summary>
	public IList<GridRowSpec> ComputedRowSpecs { get; }

	/// <summary>
	/// The minimum total height.
	/// </summary>
	public float MinHeight { get; private set; }

	/// <summary>
	/// The minimum total width.
	/// </summary>
	public float MinWidth { get; private set; }

	/// <summary>
	/// The components which were laid out.
	/// </summary>
	public ICollection<SizedGridComponent>[,] Matrix { get; }

	/// <summary>
	/// The rows in the grid.
	/// </summary>
	public IList<GridRowSpec> RowSpecs { get; }

	/// <summary>
	/// The number of rows in the grid.
	/// </summary>
	public int Rows { get; }

	/// <summary>
	/// The total flexible height weights.
	/// </summary>
	public float TotalFlexHeight { get; private set; }

	/// <summary>
	/// The total flexible width weights.
	/// </summary>
	public float TotalFlexWidth { get; private set; }

	/// <summary>
	/// Builds a matrix of the components at each given location. Components only are
	/// entered at their origin cell (ignoring row and column span).
	/// </summary>
	/// <param name="rows">The maximum number of rows.</param>
	/// <param name="columns">The maximum number of columns.</param>
	/// <param name="components">The components to add.</param>
	/// <returns>A 2-D array of the components at a given row/column location.</returns>
	private static ICollection<SizedGridComponent>[,] GetMatrix(int rows, int columns, ICollection<SizedGridComponent> components)
	{
		ICollection<SizedGridComponent>[,] array = new ICollection<SizedGridComponent>[rows, columns];
		foreach (SizedGridComponent component in components)
		{
			int row = component.Row;
			int column = component.Column;
			if (row >= 0 && row < rows && column >= 0 && column < columns)
			{
				ICollection<SizedGridComponent> collection = array[row, column];
				if (collection == null)
				{
					collection = (array[row, column] = new List<SizedGridComponent>(8));
				}
				collection.Add(component);
			}
		}
		return array;
	}

	internal GridLayoutResults(IList<GridRowSpec> rows, IList<GridColumnSpec> columns, ICollection<GridComponent<GameObject>> components)
	{
		if (rows == null)
		{
			throw new ArgumentNullException("rows");
		}
		if (columns == null)
		{
			throw new ArgumentNullException("columns");
		}
		if (components == null)
		{
			throw new ArgumentNullException("components");
		}
		Columns = columns.Count;
		Rows = rows.Count;
		ColumnSpecs = columns;
		MinHeight = (MinWidth = 0f);
		RowSpecs = rows;
		ComputedColumnSpecs = new List<GridColumnSpec>(Columns);
		ComputedRowSpecs = new List<GridRowSpec>(Rows);
		TotalFlexHeight = (TotalFlexWidth = 0f);
		Components = new List<SizedGridComponent>(Math.Max(components.Count, 4));
		foreach (GridComponent<GameObject> component in components)
		{
			GameObject item = component.Item;
			if ((Object)(object)item != (Object)null)
			{
				Components.Add(new SizedGridComponent(component, item));
			}
		}
		Matrix = GetMatrix(Rows, Columns, Components);
	}

	/// <summary>
	/// Calculates the base height of each row, the minimum it gets before extra space
	/// is distributed.
	/// </summary>
	internal void CalcBaseHeights()
	{
		int rows = Rows;
		float minHeight = (TotalFlexHeight = 0f);
		MinHeight = minHeight;
		ComputedRowSpecs.Clear();
		for (int i = 0; i < rows; i++)
		{
			GridRowSpec gridRowSpec = RowSpecs[i];
			float num2 = gridRowSpec.Height;
			float flexHeight = gridRowSpec.FlexHeight;
			if (num2 <= 0f)
			{
				for (int j = 0; j < Columns; j++)
				{
					num2 = Math.Max(num2, PreferredHeightAt(i, j));
				}
			}
			if (flexHeight > 0f)
			{
				TotalFlexHeight += flexHeight;
			}
			ComputedRowSpecs.Add(new GridRowSpec(num2, flexHeight));
		}
		foreach (SizedGridComponent component in Components)
		{
			if (component.RowSpan > 1)
			{
				ExpandMultiRow(component);
			}
		}
		for (int k = 0; k < rows; k++)
		{
			MinHeight += ComputedRowSpecs[k].Height;
		}
	}

	/// <summary>
	/// Calculates the base width of each row, the minimum it gets before extra space
	/// is distributed.
	/// </summary>
	internal void CalcBaseWidths()
	{
		int columns = Columns;
		float minWidth = (TotalFlexWidth = 0f);
		MinWidth = minWidth;
		ComputedColumnSpecs.Clear();
		for (int i = 0; i < columns; i++)
		{
			GridColumnSpec gridColumnSpec = ColumnSpecs[i];
			float num2 = gridColumnSpec.Width;
			float flexWidth = gridColumnSpec.FlexWidth;
			if (num2 <= 0f)
			{
				for (int j = 0; j < Rows; j++)
				{
					num2 = Math.Max(num2, PreferredWidthAt(j, i));
				}
			}
			if (flexWidth > 0f)
			{
				TotalFlexWidth += flexWidth;
			}
			ComputedColumnSpecs.Add(new GridColumnSpec(num2, flexWidth));
		}
		foreach (SizedGridComponent component in Components)
		{
			if (component.ColumnSpan > 1)
			{
				ExpandMultiColumn(component);
			}
		}
		for (int k = 0; k < columns; k++)
		{
			MinWidth += ComputedColumnSpecs[k].Width;
		}
	}

	/// <summary>
	/// For a multicolumn component, ratiometrically splits up any excess preferred size
	/// among the columns in its span that have a flexible width.
	/// </summary>
	/// <param name="component">The component to reallocate sizes.</param>
	private void ExpandMultiColumn(SizedGridComponent component)
	{
		float num = component.HorizontalSize.preferred;
		float num2 = 0f;
		int column = component.Column;
		int num3 = column + component.ColumnSpan;
		for (int i = column; i < num3; i++)
		{
			GridColumnSpec gridColumnSpec = ComputedColumnSpecs[i];
			if (gridColumnSpec.FlexWidth > 0f)
			{
				num2 += gridColumnSpec.FlexWidth;
			}
			num -= gridColumnSpec.Width;
		}
		if (!(num > 0f) || !(num2 > 0f))
		{
			return;
		}
		for (int j = column; j < num3; j++)
		{
			GridColumnSpec gridColumnSpec2 = ComputedColumnSpecs[j];
			float flexWidth = gridColumnSpec2.FlexWidth;
			if (flexWidth > 0f)
			{
				ComputedColumnSpecs[j] = new GridColumnSpec(gridColumnSpec2.Width + flexWidth * num / num2, flexWidth);
			}
		}
	}

	/// <summary>
	/// For a multirow component, ratiometrically splits up any excess preferred size
	/// among the rows in its span that have a flexible height.
	/// </summary>
	/// <param name="component">The component to reallocate sizes.</param>
	private void ExpandMultiRow(SizedGridComponent component)
	{
		float num = component.VerticalSize.preferred;
		float num2 = 0f;
		int row = component.Row;
		int num3 = row + component.RowSpan;
		for (int i = row; i < num3; i++)
		{
			GridRowSpec gridRowSpec = ComputedRowSpecs[i];
			if (gridRowSpec.FlexHeight > 0f)
			{
				num2 += gridRowSpec.FlexHeight;
			}
			num -= gridRowSpec.Height;
		}
		if (!(num > 0f) || !(num2 > 0f))
		{
			return;
		}
		for (int j = row; j < num3; j++)
		{
			GridRowSpec gridRowSpec2 = ComputedRowSpecs[j];
			float flexHeight = gridRowSpec2.FlexHeight;
			if (flexHeight > 0f)
			{
				ComputedRowSpecs[j] = new GridRowSpec(gridRowSpec2.Height + flexHeight * num / num2, flexHeight);
			}
		}
	}

	/// <summary>
	/// Retrieves the preferred height of a cell.
	/// </summary>
	/// <param name="row">The cell's row.</param>
	/// <param name="column">The cell's column.</param>
	/// <returns>The preferred height.</returns>
	private float PreferredHeightAt(int row, int column)
	{
		float num = 0f;
		ICollection<SizedGridComponent> collection = Matrix[row, column];
		if (collection != null && collection.Count > 0)
		{
			foreach (SizedGridComponent item in collection)
			{
				LayoutSizes verticalSize = item.VerticalSize;
				if (item.RowSpan < 2)
				{
					num = Math.Max(num, verticalSize.preferred);
				}
			}
		}
		return num;
	}

	/// <summary>
	/// Retrieves the preferred width of a cell.
	/// </summary>
	/// <param name="row">The cell's row.</param>
	/// <param name="column">The cell's column.</param>
	/// <returns>The preferred width.</returns>
	private float PreferredWidthAt(int row, int column)
	{
		float num = 0f;
		ICollection<SizedGridComponent> collection = Matrix[row, column];
		if (collection != null && collection.Count > 0)
		{
			foreach (SizedGridComponent item in collection)
			{
				LayoutSizes horizontalSize = item.HorizontalSize;
				if (item.ColumnSpan < 2)
				{
					num = Math.Max(num, horizontalSize.preferred);
				}
			}
		}
		return num;
	}
}
