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
		AllSafeFlags = 1023
	}

	private MinionBrain brain;

	private int targetCell;

	private int targetCost;

	public SafeFlags targetCellFlags;

	private bool avoid_light;

	private SafeFlags ignoredFlags = (SafeFlags)0;

	private static CellOffset[] DupeBreathableOffset = new CellOffset[2]
	{
		CellOffset.none,
		CellOffset.up
	};

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
		bool flag2 = (ignoredFlags & SafeFlags.IsNotLiquid) != 0 || !Grid.Element[cell].IsLiquid;
		bool flag3 = (ignoredFlags & SafeFlags.IsNotLiquidOnMyFace) != 0 || !Grid.Element[num].IsLiquid;
		bool flag4 = (ignoredFlags & SafeFlags.CorrectTemperature) != 0 || (Grid.Temperature[cell] > 285.15f && Grid.Temperature[cell] < 303.15f);
		bool flag5 = (ignoredFlags & SafeFlags.IsNotRadiated) != 0 || Grid.Radiation[cell] < 250f;
		bool flag6 = (ignoredFlags & SafeFlags.IsBreathable) != 0 || brain.OxygenBreather == null || GasBreatherFromWorldProvider.GetBestBreathableCellAroundSpecificCell(cell, DupeBreathableOffset, brain.OxygenBreather).IsBreathable;
		bool flag7 = !brain.Navigator.NavGrid.NavTable.IsValid(cell, NavType.Ladder) && !brain.Navigator.NavGrid.NavTable.IsValid(cell, NavType.Pole);
		bool flag8 = !brain.Navigator.NavGrid.NavTable.IsValid(cell, NavType.Tube);
		bool flag9 = !brain.Navigator.NavGrid.NavTable.IsValid(cell, NavType.Swim);
		bool flag10 = !avoid_light || SleepChore.IsDarkAtCell(cell);
		if (cell == Grid.PosToCell(brain))
		{
			flag6 = (ignoredFlags & SafeFlags.IsBreathable) != 0 || brain.OxygenBreather == null || brain.OxygenBreather.HasOxygen;
		}
		SafeFlags safeFlags = (SafeFlags)0;
		if (flag)
		{
			safeFlags |= SafeFlags.IsClear;
		}
		if (flag4)
		{
			safeFlags |= SafeFlags.CorrectTemperature;
		}
		if (flag5)
		{
			safeFlags |= SafeFlags.IsNotRadiated;
		}
		if (flag6)
		{
			safeFlags |= SafeFlags.IsBreathable;
		}
		if (flag7)
		{
			safeFlags |= SafeFlags.IsNotLadder;
		}
		if (flag8)
		{
			safeFlags |= SafeFlags.IsNotTube;
		}
		if (flag2)
		{
			safeFlags |= SafeFlags.IsNotLiquid;
		}
		if (flag3)
		{
			safeFlags |= SafeFlags.IsNotLiquidOnMyFace;
		}
		if (flag10)
		{
			safeFlags |= SafeFlags.IsLightOk;
		}
		if (flag9)
		{
			safeFlags |= SafeFlags.IsNotSwimming;
		}
		return safeFlags;
	}

	public override bool IsMatch(int cell, int parent_cell, int cost)
	{
		SafeFlags flags = GetFlags(cell, brain, avoid_light, ignoredFlags);
		bool flag = flags > targetCellFlags;
		bool flag2 = flags == targetCellFlags && cost < targetCost;
		if (flag || flag2)
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
