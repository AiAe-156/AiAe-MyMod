public class GameNavGridsHelper
{
	public static CellOffset[] EmptyCellOffsets()
	{
		return new CellOffset[0];
	}

	public static NavOffset[] EmptyNavOffset()
	{
		return new NavOffset[0];
	}

	public static NavGrid.Transition LeftWallToFloor()
	{
		return WallToFloorTransition(0, 0, NavType.LeftWall, EmptyCellOffsets());
	}

	public static NavGrid.Transition RightWallToFloor()
	{
		return WallToFloorTransition(1, 1, NavType.RightWall, new CellOffset[1]
		{
			new CellOffset(0, 1)
		});
	}

	public static NavGrid.Transition CeilingToLeftWall()
	{
		return CeilingToWallTransition(0, 0, NavType.LeftWall);
	}

	public static NavGrid.Transition CeilingToRightWall()
	{
		return CeilingToWallTransition(-1, 1, NavType.RightWall);
	}

	public static NavGrid.Transition FloorToLeftWall()
	{
		return FloorToWallTransition(1, -1, NavType.LeftWall);
	}

	public static NavGrid.Transition FloorToRightWall()
	{
		return FloorToWallTransition(0, 0, NavType.RightWall);
	}

	public static NavGrid.Transition UpRightWall()
	{
		return WallTransition(0, 1, NavType.RightWall, EmptyCellOffsets(), isLooping: true);
	}

	public static NavGrid.Transition DownLeftWall()
	{
		return WallTransition(0, -1, NavType.LeftWall, EmptyCellOffsets(), isLooping: true);
	}

	public static NavGrid.Transition OneSideFloor()
	{
		return FloorTransition(1, 0, EmptyCellOffsets(), isLooping: true);
	}

	public static NavGrid.Transition OneUpDiagonalFloor()
	{
		return FloorTransition(1, 1, new CellOffset[1]
		{
			new CellOffset(0, 1)
		});
	}

	public static NavGrid.Transition OneDownDiagonalFloor()
	{
		return FloorTransition(1, -1, new CellOffset[1]
		{
			new CellOffset(1, 0)
		});
	}

	public static NavGrid.Transition RightWallToCeiling()
	{
		return WallToCeiling(0, 0, NavType.RightWall, EmptyCellOffsets());
	}

	public static NavGrid.Transition LeftWallToCeiling()
	{
		return WallToCeiling(-1, -1, NavType.LeftWall, new CellOffset[1]
		{
			new CellOffset(1, 0)
		});
	}

	public static NavGrid.Transition WallToCeiling(int x, int y, NavType direction, CellOffset[] voidOffsets, int cost = 1, string anim = "floor_wall_0_0")
	{
		return new NavGrid.Transition(direction, NavType.Ceiling, x, y, NavAxis.NA, is_looping: false, loop_has_pre: false, is_escape: true, cost, anim, voidOffsets, EmptyCellOffsets(), EmptyNavOffset(), EmptyNavOffset());
	}

	public static NavGrid.Transition CeilingToWallTransition(int x, int y, NavType direction, int cost = 1, string anim = "floor_wall_0_0")
	{
		return new NavGrid.Transition(NavType.Ceiling, direction, x, y, NavAxis.NA, is_looping: false, loop_has_pre: false, is_escape: true, cost, anim, EmptyCellOffsets(), EmptyCellOffsets(), EmptyNavOffset(), EmptyNavOffset());
	}

	public static NavGrid.Transition CeilingSide(bool looping = true, int cost = 1, string anim = "floor_floor_1_0")
	{
		return new NavGrid.Transition(NavType.Ceiling, NavType.Ceiling, -1, 0, NavAxis.NA, looping, loop_has_pre: true, is_escape: true, cost, anim, EmptyCellOffsets(), EmptyCellOffsets(), EmptyNavOffset(), EmptyNavOffset());
	}

	public static NavGrid.Transition WallToFloorTransition(int x, int y, NavType direction, CellOffset[] voidOffsets, int cost = 1, string anim = "floor_wall_0_0")
	{
		return new NavGrid.Transition(direction, NavType.Floor, x, y, NavAxis.NA, is_looping: false, loop_has_pre: false, is_escape: true, cost, anim, voidOffsets, EmptyCellOffsets(), EmptyNavOffset(), EmptyNavOffset());
	}

	public static NavGrid.Transition FloorToWallTransition(int x, int y, NavType direction, bool isLooping = false, int cost = 1, string anim = "floor_wall_0_0")
	{
		return new NavGrid.Transition(NavType.Floor, direction, x, y, NavAxis.NA, is_looping: false, loop_has_pre: false, is_escape: true, cost, anim, EmptyCellOffsets(), EmptyCellOffsets(), EmptyNavOffset(), EmptyNavOffset());
	}

	public static NavGrid.Transition WallTransition(int x, int y, NavType direction, CellOffset[] voidOffsets, bool isLooping = false, int cost = 1, string anim = "floor_floor_1_0")
	{
		return new NavGrid.Transition(direction, direction, x, y, NavAxis.NA, is_looping: true, loop_has_pre: true, is_escape: true, cost, anim, voidOffsets, EmptyCellOffsets(), EmptyNavOffset(), EmptyNavOffset());
	}

	public static NavGrid.Transition FloorTransition(int x, int y, CellOffset[] voidOffsets, bool isLooping = false, int cost = 1, string anim = "")
	{
		return new NavGrid.Transition(NavType.Floor, NavType.Floor, x, y, NavAxis.NA, isLooping, isLooping, is_escape: true, cost, anim, voidOffsets, EmptyCellOffsets(), EmptyNavOffset(), EmptyNavOffset(), critter: true, 0.5f);
	}
}
