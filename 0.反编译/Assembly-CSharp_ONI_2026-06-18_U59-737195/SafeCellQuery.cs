public class SafeCellQuery : PathFinderQuery
{
	public enum SafeFlags
	{
		IsClear = 1,
		IsLightOk = 2,
		IsNotLadder = 4,
		IsNotTube = 8,
		CorrectTemperature = 16,
		IsNotRadiated = 32,
		IsBreathable = 64,
		IsNotLiquidOnMyFace = 128,
		IsNotLiquid = 256,
		IsNotSwimming = 512,
		AllSafeFlags = 511
	}

	private MinionBrain brain;

	private int targetCell;

	private int targetCost;

	public SafeFlags targetCellFlags;

	private bool avoid_light;

	private SafeFlags ignoredFlags;

	public SafeCellQuery Reset(MinionBrain brain, bool avoid_light, SafeFlags ignoredFlags = (SafeFlags)0)
	{
		this.brain = brain;
		targetCell = PathFinder.InvalidCell;
		targetCost = int.MaxValue;
		targetCellFlags = (SafeFlags)0;
		this.avoid_light = avoid_light;
		this.ignoredFlags = ignoredFlags;
		return this;
	}

	public static SafeFlags GetFlags(int cell, MinionBrain brain, bool avoid_light = false, SafeFlags ignoredFlags = (SafeFlags)0)
	{
		int num = Grid.CellAbove(cell);
		if (!Grid.IsValidCell(num))
		{
			return (SafeFlags)0;
		}
		if (Grid.Solid[cell] || Grid.Solid[num])
		{
			return (SafeFlags)0;
		}
		if (Grid.IsTileUnderConstruction[cell] || Grid.IsTileUnderConstruction[num])
		{
			return (SafeFlags)0;
		}
		bool flag = brain.IsCellClear(cell);
		bool num2 = (ignoredFlags & SafeFlags.IsNotLiquid) != 0 || !Grid.Element[cell].IsLiquid;
		bool flag2 = (ignoredFlags & SafeFlags.IsNotLiquidOnMyFace) != 0 || !Grid.Element[num].IsLiquid;
		bool flag3 = (ignoredFlags & SafeFlags.CorrectTemperature) != 0 || (Grid.Temperature[cell] > 285.15f && Grid.Temperature[cell] < 303.15f);
		bool flag4 = (ignoredFlags & SafeFlags.IsNotRadiated) != 0 || Grid.Radiation[cell] < 250f;
		bool flag5 = (ignoredFlags & SafeFlags.IsBreathable) != 0 || brain.OxygenBreather == null || GasBreatherFromWorldProvider.GetBestBreathableCellAroundSpecificCell(cell, Grid.DefaultOffset, brain.OxygenBreather).IsBreathable;
		bool num3 = !brain.Navigator.NavGrid.NavTable.IsValid(cell, NavType.Ladder) && !brain.Navigator.NavGrid.NavTable.IsValid(cell, NavType.Pole);
		bool flag6 = !brain.Navigator.NavGrid.NavTable.IsValid(cell, NavType.Tube);
		bool flag7 = !avoid_light || SleepChore.IsDarkAtCell(cell);
		if (cell == Grid.PosToCell(brain))
		{
			flag5 = (ignoredFlags & SafeFlags.IsBreathable) != 0 || brain.OxygenBreather == null || brain.OxygenBreather.HasOxygen;
		}
		SafeFlags safeFlags = (SafeFlags)0;
		if (flag)
		{
			safeFlags |= SafeFlags.IsClear;
		}
		if (flag3)
		{
			safeFlags |= SafeFlags.CorrectTemperature;
		}
		if (flag4)
		{
			safeFlags |= SafeFlags.IsNotRadiated;
		}
		if (flag5)
		{
			safeFlags |= SafeFlags.IsBreathable;
		}
		if (num3)
		{
			safeFlags |= SafeFlags.IsNotLadder;
		}
		if (flag6)
		{
			safeFlags |= SafeFlags.IsNotTube;
		}
		if (num2)
		{
			safeFlags |= SafeFlags.IsNotLiquid;
		}
		if (flag2)
		{
			safeFlags |= SafeFlags.IsNotLiquidOnMyFace;
		}
		if (flag7)
		{
			safeFlags |= SafeFlags.IsLightOk;
		}
		return safeFlags;
	}

	public override bool IsMatch(int cell, int parent_cell, int cost)
	{
		SafeFlags flags = GetFlags(cell, brain, avoid_light, ignoredFlags);
		bool num = flags > targetCellFlags;
		bool flag = flags == targetCellFlags && cost < targetCost;
		if (num || flag)
		{
			targetCellFlags = flags;
			targetCost = cost;
			targetCell = cell;
		}
		return (SafeFlags.AllSafeFlags & ~(flags | ignoredFlags)) == 0;
	}

	public override int GetResultCell()
	{
		return targetCell;
	}
}
