using TUNING;
using UnityEngine;

public class SwimSafeCellQuery : PathFinderQuery
{
	private const float SECONDS_PER_COST = 0.1f;

	private const float BREATH_SAFETY_MARGIN_SECONDS = 15f;

	private MinionBrain brain;

	private int targetCell;

	private int targetCost;

	public SafeCellQuery.SafeFlags targetCellFlags;

	private bool avoid_light;

	private SafeCellQuery.SafeFlags ignoredFlags;

	private float maxSubmergedCost;

	private int[] routeSubmergedCost;

	private int targetRouteSubmergedCost;

	public SwimSafeCellQuery Reset(MinionBrain brain, bool avoid_light, SafeCellQuery.SafeFlags ignoredFlags, float currentBreathValue)
	{
		this.brain = brain;
		targetCell = PathFinder.InvalidCell;
		targetCost = int.MaxValue;
		targetCellFlags = (SafeCellQuery.SafeFlags)0;
		this.avoid_light = avoid_light;
		targetRouteSubmergedCost = int.MaxValue;
		this.ignoredFlags = ignoredFlags | SafeCellQuery.SafeFlags.IsNotLiquid | SafeCellQuery.SafeFlags.IsNotLiquidOnMyFace | SafeCellQuery.SafeFlags.IsNotSwimming;
		float bREATH_RATE = DUPLICANTSTATS.STANDARD.Breath.BREATH_RATE;
		float num = ((bREATH_RATE > 0f) ? (currentBreathValue / bREATH_RATE) : 0f);
		float num2 = Mathf.Max(0f, num - 15f);
		maxSubmergedCost = num2 / 0.1f;
		if (routeSubmergedCost == null || routeSubmergedCost.Length != Grid.CellCount)
		{
			routeSubmergedCost = new int[Grid.CellCount];
		}
		int num3 = Grid.PosToCell(brain);
		if (Grid.IsValidCell(num3))
		{
			routeSubmergedCost[num3] = 0;
		}
		return this;
	}

	private static bool IsCellSubmerged(int cell)
	{
		if (!Grid.Element[cell].IsLiquid)
		{
			return false;
		}
		int num = Grid.CellAbove(cell);
		if (Grid.IsValidCell(num))
		{
			return Grid.Element[num].IsLiquid;
		}
		return false;
	}

	public override bool IsMatch(int cell, int parent_cell, int cost)
	{
		SafeCellQuery.SafeFlags flags = SafeCellQuery.GetFlags(cell, brain, avoid_light, ignoredFlags);
		int num = 0;
		if (Grid.IsValidCell(parent_cell))
		{
			num = routeSubmergedCost[parent_cell];
		}
		int num3;
		if (IsCellSubmerged(cell))
		{
			int num2 = 9;
			num3 = num + num2;
		}
		else
		{
			num3 = 0;
		}
		routeSubmergedCost[cell] = num3;
		if ((float)num3 > maxSubmergedCost)
		{
			return false;
		}
		bool num4 = flags > targetCellFlags;
		bool num5 = flags == targetCellFlags;
		bool flag = num5 && num3 < targetRouteSubmergedCost;
		bool flag2 = num5 && num3 == targetRouteSubmergedCost && cost < targetCost;
		if (num4 || flag || flag2)
		{
			targetCellFlags = flags;
			targetRouteSubmergedCost = num3;
			targetCost = cost;
			targetCell = cell;
		}
		return (SafeCellQuery.SafeFlags.AllSafeFlags & ~(flags | ignoredFlags)) == 0;
	}

	public override int GetResultCell()
	{
		return targetCell;
	}
}
