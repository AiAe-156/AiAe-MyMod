public class IdleCellQuery : PathFinderQuery
{
	private MinionBrain brain;

	private int targetCell;

	private int maxCost;

	private bool canSwim;

	public IdleCellQuery Reset(MinionBrain brain, int max_cost, bool can_swim = false)
	{
		this.brain = brain;
		maxCost = max_cost;
		targetCell = Grid.InvalidCell;
		canSwim = can_swim;
		return this;
	}

	public override bool IsMatch(int cell, int parent_cell, int cost)
	{
		SafeCellQuery.SafeFlags flags = SafeCellQuery.GetFlags(cell, brain);
		if ((flags & SafeCellQuery.SafeFlags.IsClear) != 0 && (flags & SafeCellQuery.SafeFlags.IsNotLadder) != 0 && (flags & SafeCellQuery.SafeFlags.IsNotTube) != 0 && (flags & SafeCellQuery.SafeFlags.IsBreathable) != 0)
		{
			if ((flags & SafeCellQuery.SafeFlags.IsNotLiquid) != 0)
			{
				targetCell = cell;
			}
			else if (canSwim && (flags & SafeCellQuery.SafeFlags.IsNotLiquidOnMyFace) != 0)
			{
				targetCell = cell;
			}
		}
		return cost > maxCost;
	}

	public override int GetResultCell()
	{
		return targetCell;
	}
}
