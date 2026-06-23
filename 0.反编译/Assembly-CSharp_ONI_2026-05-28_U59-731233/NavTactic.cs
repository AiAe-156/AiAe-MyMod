using UnityEngine;

public class NavTactic
{
	private int _overlapPenalty = 3;

	private int _preferredRange = 0;

	private int _rangePenalty = 2;

	private int _pathCostPenalty = 1;

	private int _pathXCostPenalty = 0;

	private int _preferredX = 0;

	private int _pathYCostPenalty = 0;

	private int _preferredY = 0;

	public NavTactic(int preferredRange, int rangePenalty = 1, int overlapPenalty = 1, int pathCostPenalty = 1)
	{
		_overlapPenalty = overlapPenalty;
		_preferredRange = preferredRange;
		_rangePenalty = rangePenalty;
		_pathCostPenalty = pathCostPenalty;
	}

	public NavTactic(int preferredRange, int rangePenalty, int overlapPenalty, int pathCostPenalty, int xPenalty, int preferredX, int yPenalty, int preferredY)
	{
		_overlapPenalty = overlapPenalty;
		_preferredRange = preferredRange;
		_rangePenalty = rangePenalty;
		_pathCostPenalty = pathCostPenalty;
		_pathXCostPenalty = xPenalty;
		_preferredX = preferredX;
		_pathYCostPenalty = yPenalty;
		_preferredY = preferredY;
	}

	public int GetCellPreferences(int root, CellOffset[] offsets, Navigator navigator)
	{
		int result = NavigationReservations.InvalidReservation;
		int num = int.MaxValue;
		for (int i = 0; i < offsets.Length; i++)
		{
			int num2 = Grid.OffsetCell(root, offsets[i]);
			int num3 = 0;
			num3 += _overlapPenalty * NavigationReservations.Instance.GetOccupancyCount(num2);
			num3 += _rangePenalty * Mathf.Abs(_preferredRange - Grid.GetCellDistance(root, num2));
			num3 += _pathCostPenalty * Mathf.Max(navigator.GetNavigationCost(num2), 0);
			num3 += _pathXCostPenalty * Mathf.Abs(_preferredX - Mathf.Abs(Grid.CellColumn(root) - Grid.CellColumn(num2)));
			num3 += _pathYCostPenalty * Mathf.Abs(_preferredY - Mathf.Abs(Grid.CellRow(root) - Grid.CellRow(num2)));
			if (num3 < num && navigator.CanReach(num2))
			{
				num = num3;
				result = num2;
			}
		}
		return result;
	}
}
