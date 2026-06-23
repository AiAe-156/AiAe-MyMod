using System.Collections.Generic;
using UnityEngine.Pool;

public static class PathProber
{
	public const int InvalidHandle = -1;

	public const int InvalidIdx = -1;

	public const int InvalidCell = -1;

	public const int InvalidCost = -1;

	public const ushort InvalidSerialNo = 0;

	private static ObjectPool<PathFinder.PotentialScratchPad> ScratchPadPool = new ObjectPool<PathFinder.PotentialScratchPad>(() => new PathFinder.PotentialScratchPad(Pathfinding.Instance.MaxLinksPerCell()), null, null, null, collectionCheck: false, 1, 4);

	private static ObjectPool<PathFinder.PotentialList> PotentialListPool = new ObjectPool<PathFinder.PotentialList>(() => new PathFinder.PotentialList(), null, delegate(PathFinder.PotentialList list)
	{
		list.Clear();
	}, null, collectionCheck: false, 1, 4);

	public static void Run(int root_cell, PathFinderAbilities abilities, NavGrid nav_grid, NavType starting_nav_type, PathGrid path_grid, ushort serialNo, PathFinder.PotentialScratchPad scratchPad, PathFinder.PotentialList potentials, PathFinder.PotentialPath.Flags flags, List<int> found_cells = null)
	{
		path_grid.BeginUpdate(serialNo, root_cell, found_cells);
		NavType nav_type = starting_nav_type;
		PathFinder.Cell cell_data = path_grid.GetCell(root_cell, starting_nav_type, out var is_cell_in_range);
		PathFinder.AddPotential(new PathFinder.PotentialPath(root_cell, nav_type, flags), Grid.InvalidCell, NavType.NumNavTypes, 0, 0, potentials, path_grid, ref cell_data);
		while (potentials.Count > 0)
		{
			KeyValuePair<int, PathFinder.PotentialPath> keyValuePair = potentials.Next();
			cell_data = path_grid.GetCell(keyValuePair.Value, out is_cell_in_range);
			if (cell_data.cost == keyValuePair.Key)
			{
				PathFinder.AddPotentials(scratchPad, keyValuePair.Value, cell_data.cost, ref abilities, null, nav_grid.maxLinksPerCell, nav_grid.Links, potentials, path_grid, cell_data.parent, cell_data.parentNavType);
			}
		}
		path_grid.EndUpdate();
	}

	public static void Run(Navigator navigator, List<int> found_cells = null)
	{
		PathFinder.PotentialScratchPad potentialScratchPad = ScratchPadPool.Get();
		PathFinder.PotentialList potentialList = PotentialListPool.Get();
		ushort num = (ushort)(navigator.PathGrid.SerialNo + 1);
		if (num == 0)
		{
			num++;
			navigator.PathGrid.ResetProberCells();
		}
		PathFinderAbilities currentAbilities = navigator.GetCurrentAbilities();
		Run(navigator.cachedCell, currentAbilities, navigator.NavGrid, navigator.CurrentNavType, navigator.PathGrid, num, potentialScratchPad, potentialList, navigator.flags, found_cells);
		ScratchPadPool.Release(potentialScratchPad);
		PotentialListPool.Release(potentialList);
	}
}
