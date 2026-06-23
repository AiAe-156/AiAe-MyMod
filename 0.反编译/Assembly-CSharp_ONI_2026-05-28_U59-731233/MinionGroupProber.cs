using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/MinionGroupProber")]
public class MinionGroupProber : KMonoBehaviour
{
	private static MinionGroupProber Instance;

	private int[] cells;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	public static MinionGroupProber Get()
	{
		return Instance;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
		cells = new int[Grid.CellCount];
	}

	public bool IsReachable(int cell)
	{
		if (!Grid.IsValidCell(cell))
		{
			return false;
		}
		return cells[cell] > 0;
	}

	public bool IsReachable(int cell, CellOffset[] offsets)
	{
		if (!Grid.IsValidCell(cell))
		{
			return false;
		}
		foreach (CellOffset offset in offsets)
		{
			if (IsReachable(Grid.OffsetCell(cell, offset)))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAllReachable(int cell, CellOffset[] offsets)
	{
		if (IsReachable(cell))
		{
			return true;
		}
		foreach (CellOffset offset in offsets)
		{
			if (IsReachable(Grid.OffsetCell(cell, offset)))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsReachable(Workable workable)
	{
		return IsReachable(Grid.PosToCell(workable), workable.GetOffsets());
	}

	public void Occupy(List<int> cells)
	{
		foreach (int cell in cells)
		{
			Interlocked.Increment(ref this.cells[cell]);
		}
	}

	public void OccupyST(List<int> cells)
	{
		foreach (int cell in cells)
		{
			this.cells[cell]++;
		}
	}

	public void Occupy(int cell)
	{
		Interlocked.Increment(ref cells[cell]);
	}

	public void Vacate(List<int> cells)
	{
		foreach (int cell in cells)
		{
			Interlocked.Decrement(ref this.cells[cell]);
		}
	}

	public void VacateST(List<int> cells)
	{
		foreach (int cell in cells)
		{
			this.cells[cell]--;
		}
	}

	public void Vacate(int cell)
	{
		Interlocked.Decrement(ref cells[cell]);
	}
}
