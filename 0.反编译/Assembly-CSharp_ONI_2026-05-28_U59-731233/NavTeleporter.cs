using System;
using System.Runtime.CompilerServices;
using TUNING;

public class NavTeleporter : KMonoBehaviour
{
	private NavTeleporter target;

	private int lastRegisteredCell = Grid.InvalidCell;

	public CellOffset offset;

	private int overrideCell = -1;

	private ulong cellChangeHandlerID = 0uL;

	private static readonly Action<object> OnCellChangedDispatcher = delegate(object obj)
	{
		Unsafe.As<NavTeleporter>(obj).OnCellChanged();
	};

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		GetComponent<KPrefabID>().AddTag(GameTags.NavTeleporters);
		Register();
		cellChangeHandlerID = Singleton<CellChangeMonitor>.Instance.RegisterCellChangedHandler(base.transform, OnCellChangedDispatcher, this, "NavTeleporterCellChanged");
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		int cell = GetCell();
		if (cell != Grid.InvalidCell)
		{
			Grid.HasNavTeleporter[cell] = false;
		}
		Singleton<CellChangeMonitor>.Instance.UnregisterCellChangedHandler(ref cellChangeHandlerID);
		Deregister();
		Components.NavTeleporters.Remove(this);
	}

	public void SetOverrideCell(int cell)
	{
		overrideCell = cell;
	}

	public int GetCell()
	{
		if (overrideCell >= 0)
		{
			return overrideCell;
		}
		int cell = Grid.PosToCell(this);
		return Grid.OffsetCell(cell, offset);
	}

	public void TwoWayTarget(NavTeleporter nt)
	{
		if (target != null)
		{
			if (nt != null)
			{
				nt.SetTarget(null);
			}
			BreakLink();
		}
		target = nt;
		if (target != null)
		{
			SetLink();
			if (nt != null)
			{
				nt.SetTarget(this);
			}
		}
	}

	public void EnableTwoWayTarget(bool enable)
	{
		if (enable)
		{
			target.SetLink();
			SetLink();
		}
		else
		{
			target.BreakLink();
			BreakLink();
		}
	}

	public void SetTarget(NavTeleporter nt)
	{
		if (target != null)
		{
			BreakLink();
		}
		target = nt;
		if (target != null)
		{
			SetLink();
		}
	}

	private void Register()
	{
		int cell = GetCell();
		if (!Grid.IsValidCell(cell))
		{
			lastRegisteredCell = Grid.InvalidCell;
			return;
		}
		Grid.HasNavTeleporter[cell] = true;
		Pathfinding.Instance.AddDirtyNavGridCell(cell);
		lastRegisteredCell = cell;
		if (target != null)
		{
			SetLink();
		}
	}

	private void SetLink()
	{
		int cell = target.GetCell();
		NavGrid navGrid = Pathfinding.Instance.GetNavGrid(DUPLICANTSTATS.STANDARD.BaseStats.NAV_GRID_NAME);
		navGrid.teleportTransitions[lastRegisteredCell] = cell;
		Pathfinding.Instance.AddDirtyNavGridCell(lastRegisteredCell);
	}

	public void Deregister()
	{
		if (lastRegisteredCell != Grid.InvalidCell)
		{
			BreakLink();
			Grid.HasNavTeleporter[lastRegisteredCell] = false;
			Pathfinding.Instance.AddDirtyNavGridCell(lastRegisteredCell);
			lastRegisteredCell = Grid.InvalidCell;
		}
	}

	private void BreakLink()
	{
		NavGrid navGrid = Pathfinding.Instance.GetNavGrid(DUPLICANTSTATS.STANDARD.BaseStats.NAV_GRID_NAME);
		navGrid.teleportTransitions.Remove(lastRegisteredCell);
		Pathfinding.Instance.AddDirtyNavGridCell(lastRegisteredCell);
	}

	private void OnCellChanged()
	{
		Deregister();
		Register();
		if (target != null)
		{
			NavTeleporter component = target.GetComponent<NavTeleporter>();
			if (component != null)
			{
				component.SetTarget(this);
			}
		}
	}
}
