using System;
using System.Collections.Generic;

public class PathGrid
{
	private struct ProberCell
	{
		public int cost;

		public ushort queryId;

		public NavType navType;
	}

	private PathFinder.Cell[] Cells;

	private ProberCell[] ProberCells;

	private List<int> freshlyOccupiedCells;

	public NavType[] ValidNavTypes;

	public int[] NavTypeTable;

	public int widthInCells;

	public int heightInCells;

	public bool applyOffset;

	private int rootX;

	private int rootY;

	private ushort serialNo;

	public static readonly PathFinder.Cell InvalidCell = new PathFinder.Cell
	{
		cost = -1,
		parent = -1
	};

	private static readonly ProberCell InvalidProberCell = new ProberCell
	{
		cost = -1,
		queryId = 0,
		navType = NavType.Floor
	};

	public ulong AllocatedClassification
	{
		get
		{
			DebugUtil.Assert(widthInCells < 65535);
			DebugUtil.Assert(heightInCells < 65535);
			DebugUtil.Assert(ValidNavTypes.Length < 256);
			return (ulong)((((long)widthInCells << 16) + heightInCells << 8) + ValidNavTypes.Length);
		}
	}

	public ushort SerialNo => serialNo;

	public PathGrid(PathGrid other)
		: this(other.widthInCells, other.heightInCells, other.applyOffset, other.ValidNavTypes)
	{
	}

	public PathGrid(int width_in_cells, int height_in_cells, bool apply_offset, NavType[] valid_nav_types)
	{
		applyOffset = apply_offset;
		widthInCells = width_in_cells;
		heightInCells = height_in_cells;
		ValidNavTypes = valid_nav_types;
		int num = 0;
		NavTypeTable = new int[11];
		for (int i = 0; i < NavTypeTable.Length; i++)
		{
			NavTypeTable[i] = -1;
			for (int j = 0; j < ValidNavTypes.Length; j++)
			{
				if ((uint)ValidNavTypes[j] == (byte)i)
				{
					NavTypeTable[i] = num++;
					break;
				}
			}
		}
		DebugUtil.DevAssert(test: true, "Cell packs nav type into 4 bits!");
		Cells = new PathFinder.Cell[width_in_cells * height_in_cells * ValidNavTypes.Length];
		ProberCells = new ProberCell[width_in_cells * height_in_cells];
	}

	public void CloneNavTypes(PathGrid other)
	{
		DebugUtil.Assert(other.ValidNavTypes.Length == ValidNavTypes.Length);
		other.ValidNavTypes.CopyTo(ValidNavTypes, 0);
		int num = 0;
		for (int i = 0; i < NavTypeTable.Length; i++)
		{
			NavTypeTable[i] = -1;
			for (int j = 0; j < ValidNavTypes.Length; j++)
			{
				if ((uint)ValidNavTypes[j] == (byte)i)
				{
					NavTypeTable[i] = num++;
					break;
				}
			}
		}
	}

	public void ResetProberCells()
	{
		for (int i = 0; i < ProberCells.Length; i++)
		{
			ProberCells[i] = default(ProberCell);
		}
	}

	public void OnCleanUp()
	{
	}

	public void BeginUpdate(ushort new_serial_no, int root_cell, List<int> found_cells_list = null)
	{
		freshlyOccupiedCells = found_cells_list;
		if (applyOffset)
		{
			Grid.CellToXY(root_cell, out rootX, out rootY);
			rootX -= widthInCells / 2;
			rootY -= heightInCells / 2;
		}
		serialNo = new_serial_no;
	}

	public void EndUpdate()
	{
		freshlyOccupiedCells = null;
	}

	private bool IsValidSerialNo(ushort serialNo)
	{
		if (serialNo == this.serialNo)
		{
			return serialNo != 0;
		}
		return false;
	}

	public PathFinder.Cell GetCell(PathFinder.PotentialPath potential_path, out bool is_cell_in_range)
	{
		return GetCell(potential_path.cell, potential_path.navType, out is_cell_in_range);
	}

	public PathFinder.Cell GetCell(int cell, NavType nav_type, out bool is_cell_in_range)
	{
		int num = OffsetCell(cell);
		is_cell_in_range = -1 != num;
		if (!is_cell_in_range)
		{
			return InvalidCell;
		}
		if ((int)nav_type >= NavTypeTable.Length)
		{
			return InvalidCell;
		}
		if (num * ValidNavTypes.Length + NavTypeTable[(uint)nav_type] >= Cells.Length)
		{
			return InvalidCell;
		}
		PathFinder.Cell result = Cells[num * ValidNavTypes.Length + NavTypeTable[(uint)nav_type]];
		if (!IsValidSerialNo(result.queryId))
		{
			return InvalidCell;
		}
		return result;
	}

	private ProberCell GetProberCell(int cell)
	{
		int num = OffsetCell(cell);
		if (num == -1)
		{
			return InvalidProberCell;
		}
		return ProberCells[num];
	}

	public void SetCell(PathFinder.PotentialPath potential_path, ref PathFinder.Cell cell_data)
	{
		int num = OffsetCell(potential_path.cell);
		if (-1 == num)
		{
			return;
		}
		cell_data.queryId = serialNo;
		int num2 = NavTypeTable[(uint)potential_path.navType];
		int num3 = num * ValidNavTypes.Length + num2;
		Cells[num3] = cell_data;
		if (potential_path.navType != NavType.Tube)
		{
			ProberCell proberCell = ProberCells[num];
			if (cell_data.queryId != proberCell.queryId && freshlyOccupiedCells != null)
			{
				freshlyOccupiedCells.Add(potential_path.cell);
			}
			if (cell_data.queryId != proberCell.queryId || cell_data.cost < proberCell.cost)
			{
				proberCell.queryId = cell_data.queryId;
				proberCell.cost = cell_data.cost;
				proberCell.navType = potential_path.navType;
				ProberCells[num] = proberCell;
			}
		}
	}

	public int GetCostIgnoreProberOffset(int cell, CellOffset[] offsets)
	{
		int num = -1;
		foreach (CellOffset offset in offsets)
		{
			int num2 = Grid.OffsetCell(cell, offset);
			if (Grid.IsValidCell(num2))
			{
				ProberCell proberCell = ProberCells[num2];
				if (IsValidSerialNo(proberCell.queryId) && (num == -1 || proberCell.cost < num))
				{
					num = proberCell.cost;
				}
			}
		}
		return num;
	}

	public int GetCost(int cell)
	{
		int num = OffsetCell(cell);
		if (-1 == num)
		{
			return -1;
		}
		ProberCell proberCell = ProberCells[num];
		if (!IsValidSerialNo(proberCell.queryId))
		{
			return -1;
		}
		return proberCell.cost;
	}

	private int OffsetCell(int cell)
	{
		if (applyOffset)
		{
			Grid.CellToXY(cell, out var x, out var y);
			if (x < rootX || x >= rootX + widthInCells || y < rootY || y >= rootY + heightInCells)
			{
				return -1;
			}
			int num = x - rootX;
			return (y - rootY) * widthInCells + num;
		}
		return cell;
	}

	public bool BuildPath(int source_cell, int target_cell, NavType current_nav_type, ref PathFinder.Path path)
	{
		if (path.nodes != null)
		{
			path.nodes.Clear();
		}
		path.cost = -1;
		if (target_cell == PathFinder.InvalidCell || GetCost(target_cell) == -1 || GetCost(source_cell) == -1)
		{
			return false;
		}
		bool is_cell_in_range = false;
		PathFinder.Cell cell = GetCell(target_cell, GetProberCell(target_cell).navType, out is_cell_in_range);
		path.Clear();
		path.cost = cell.cost;
		int cost = path.cost;
		while (target_cell != PathFinder.InvalidCell)
		{
			path.AddNode(new PathFinder.Path.Node
			{
				cell = target_cell,
				navType = cell.navType,
				transitionId = cell.transitionId
			});
			if (target_cell == source_cell && cell.navType == current_nav_type)
			{
				path.nodes.Reverse();
				return true;
			}
			if (target_cell != PathFinder.InvalidCell)
			{
				target_cell = cell.parent;
				cell = GetCell(target_cell, cell.parentNavType, out is_cell_in_range);
			}
			if (cell.cost >= cost && target_cell != PathFinder.InvalidCell)
			{
				KCrashReporter.ReportDevNotification("Invalid Cost Progression", Environment.StackTrace, $"{source_cell}x{current_nav_type} -> {target_cell} via path of length {path.nodes.Count} cell_data.cost: {cell.cost} previousCost: {cost} cell_data.navType: {cell.navType}");
				break;
			}
			cost = cell.cost;
		}
		path.Clear();
		return false;
	}
}
