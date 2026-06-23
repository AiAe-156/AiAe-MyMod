#define DEVELOPMENT_BUILD
using UnityEngine;

public class OffsetTracker
{
	public static bool isExecutingWithinJob;

	protected CellOffset[] offsets;

	protected int previousCell = Grid.InvalidCell;

	public virtual CellOffset[] GetOffsets(int current_cell)
	{
		if (current_cell != previousCell)
		{
			DevOnly.Assert(Game.IsOnMainThread(), "OffsetTracker::GetOffsets is not thread safe, but really should be");
			Debug.Assert(!isExecutingWithinJob, "OffsetTracker.GetOffsets() is making a mutating call but is currently executing within a job");
			UpdateCell(previousCell, current_cell);
			previousCell = current_cell;
		}
		if (offsets == null)
		{
			DevOnly.Assert(Game.IsOnMainThread(), "OffsetTracker::GetOffsets is not thread safe, but really should be");
			Debug.Assert(!isExecutingWithinJob, "OffsetTracker.GetOffsets() is making a mutating call but is currently executing within a job");
			UpdateOffsets(previousCell);
		}
		return offsets;
	}

	public virtual bool ValidateOffsets(int current_cell)
	{
		return current_cell == previousCell && offsets != null;
	}

	public void ForceRefresh()
	{
		int cell = previousCell;
		previousCell = Grid.InvalidCell;
		Refresh(cell);
	}

	public void Refresh(int cell)
	{
		GetOffsets(cell);
	}

	protected virtual void UpdateCell(int previous_cell, int current_cell)
	{
	}

	protected virtual void UpdateOffsets(int current_cell)
	{
	}

	public virtual void Clear()
	{
	}

	public virtual void DebugDrawExtents()
	{
	}

	public virtual void DebugDrawEditor()
	{
	}

	public virtual void DebugDrawOffsets(int cell)
	{
		CellOffset[] array = GetOffsets(cell);
		foreach (CellOffset offset in array)
		{
			int cell2 = Grid.OffsetCell(cell, offset);
			Gizmos.color = new Color(0f, 0.5f, 0f, 0.15f);
			Gizmos.DrawWireCube(Grid.CellToPosCCC(cell2, Grid.SceneLayer.Move), new Vector3(0.95f, 0.95f, 0.95f));
		}
	}
}
