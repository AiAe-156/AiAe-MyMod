public class SafeResuscitateCellQuery : PathFinderQuery
{
	private int targetCell;

	private int targetCost;

	private OxygenBreather oxygenBreather;

	public SafeResuscitateCellQuery Reset(OxygenBreather oxygen_breather)
	{
		targetCell = PathFinder.InvalidCell;
		targetCost = int.MaxValue;
		oxygenBreather = oxygen_breather;
		return this;
	}

	public override bool IsMatch(int cell, int parent_cell, int cost)
	{
		int num = Grid.CellAbove(cell);
		if (!Grid.IsValidCell(num))
		{
			return false;
		}
		if (Grid.Solid[cell] || Grid.Solid[num])
		{
			return false;
		}
		int num2 = Grid.CellBelow(cell);
		if (!Grid.IsValidCell(num2) || !Grid.Solid[num2])
		{
			return false;
		}
		if (Grid.IsSubstantialLiquid(cell) || Grid.Element[num].IsLiquid)
		{
			return false;
		}
		if (oxygenBreather != null && !GasBreatherFromWorldProvider.GetBestBreathableCellAroundSpecificCell(cell, new CellOffset[2]
		{
			CellOffset.none,
			CellOffset.up
		}, oxygenBreather).IsBreathable)
		{
			return false;
		}
		if (cost < targetCost)
		{
			targetCost = cost;
			targetCell = cell;
		}
		return false;
	}

	public override int GetResultCell()
	{
		return targetCell;
	}
}
