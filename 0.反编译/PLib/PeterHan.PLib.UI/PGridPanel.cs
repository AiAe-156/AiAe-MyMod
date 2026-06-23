using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.PLib.UI;

/// <summary>
/// A panel which lays out its components using grid-type constraints.
/// </summary>
public class PGridPanel : PContainer, IDynamicSizable, IUIComponent
{
	/// <summary>
	/// The children of this panel.
	/// </summary>
	private readonly ICollection<GridComponent<IUIComponent>> children;

	/// <summary>
	/// The columns in this panel.
	/// </summary>
	private readonly IList<GridColumnSpec> columns;

	/// <summary>
	/// The rows in this panel.
	/// </summary>
	private readonly IList<GridRowSpec> rows;

	/// <summary>
	/// The number of columns currently defined.
	/// </summary>
	public int Columns => columns.Count;

	public bool DynamicSize { get; set; }

	/// <summary>
	/// The number of rows currently defined.
	/// </summary>
	public int Rows => rows.Count;

	public PGridPanel()
		: this(null)
	{
	}

	public PGridPanel(string name)
		: base(name ?? "GridPanel")
	{
		children = new List<GridComponent<IUIComponent>>(16);
		columns = new List<GridColumnSpec>(16);
		rows = new List<GridRowSpec>(16);
		DynamicSize = true;
		base.Margin = null;
	}

	/// <summary>
	/// Adds a child to this panel.
	/// </summary>
	/// <param name="child">The child to add.</param>
	/// <param name="spec">The location where the child will be placed.</param>
	/// <returns>This panel for call chaining.</returns>
	public PGridPanel AddChild(IUIComponent child, GridComponentSpec spec)
	{
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		if (spec == null)
		{
			throw new ArgumentNullException("spec");
		}
		children.Add(new GridComponent<IUIComponent>(spec, child));
		return this;
	}

	/// <summary>
	/// Adds a column to this panel.
	/// </summary>
	/// <param name="column">The specification for that column.</param>
	/// <returns>This panel for call chaining.</returns>
	public PGridPanel AddColumn(GridColumnSpec column)
	{
		if (column == null)
		{
			throw new ArgumentNullException("column");
		}
		columns.Add(column);
		return this;
	}

	/// <summary>
	/// Adds a handler when this panel is realized.
	/// </summary>
	/// <param name="onRealize">The handler to invoke on realization.</param>
	/// <returns>This panel for call chaining.</returns>
	public PGridPanel AddOnRealize(PUIDelegates.OnRealize onRealize)
	{
		base.OnRealize += onRealize;
		return this;
	}

	/// <summary>
	/// Adds a row to this panel.
	/// </summary>
	/// <param name="row">The specification for that row.</param>
	/// <returns>This panel for call chaining.</returns>
	public PGridPanel AddRow(GridRowSpec row)
	{
		if (row == null)
		{
			throw new ArgumentNullException("row");
		}
		rows.Add(row);
		return this;
	}

	public override GameObject Build()
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		if (Columns < 1)
		{
			throw new InvalidOperationException("At least one column must be defined");
		}
		if (Rows < 1)
		{
			throw new InvalidOperationException("At least one row must be defined");
		}
		GameObject val = PUIElements.CreateUI(null, base.Name);
		SetImage(val);
		PGridLayoutGroup pGridLayoutGroup = val.AddComponent<PGridLayoutGroup>();
		pGridLayoutGroup.Margin = base.Margin;
		foreach (GridColumnSpec column in columns)
		{
			pGridLayoutGroup.AddColumn(column);
		}
		foreach (GridRowSpec row in rows)
		{
			pGridLayoutGroup.AddRow(row);
		}
		foreach (GridComponent<IUIComponent> child in children)
		{
			pGridLayoutGroup.AddComponent(child.Item.Build(), child);
		}
		if (!DynamicSize)
		{
			pGridLayoutGroup.LockLayout();
		}
		pGridLayoutGroup.flexibleWidth = base.FlexSize.x;
		pGridLayoutGroup.flexibleHeight = base.FlexSize.y;
		InvokeRealize(val);
		return val;
	}

	public override string ToString()
	{
		return $"PGridPanel[Name={base.Name},Rows={Rows:D},Columns={Columns:D}]";
	}
}
