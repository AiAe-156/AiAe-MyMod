using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/FishOvercrowingManager")]
public class FishOvercrowingManager : KMonoBehaviour, ISim1000ms
{
	private readonly struct Cell
	{
		private readonly int generation;

		private readonly int pondIndex;

		public int Generation => generation;

		public int PondIndex => pondIndex;

		public Cell(int generation, int pondIndex)
		{
			this.generation = generation;
			this.pondIndex = pondIndex;
		}
	}

	public class Pond
	{
		public List<KPrefabID> fishes;

		public List<KPrefabID> eggs;

		public int cellCount;

		public OvercrowdingMonitor.Occupancy occupancy = new OvercrowdingMonitor.Occupancy();

		public int FishCount => fishes.Count;

		public int EggCount => eggs.Count;
	}

	private readonly struct VisitTracker : FloodFill.IVisitTracker
	{
		private readonly Cell[] grid;

		private readonly Cell cell;

		public VisitTracker(Cell[] grid, Cell cell)
		{
			this.grid = grid;
			this.cell = cell;
		}

		public bool Add(int cellIndex)
		{
			if (!Contains(cellIndex))
			{
				grid[cellIndex] = cell;
				return true;
			}
			return false;
		}

		public bool Contains(int cellIndex)
		{
			return grid[cellIndex].Generation == cell.Generation;
		}
	}

	private readonly struct Visitor : FloodFill.IVisitor
	{
		private readonly Pond pond;

		public bool EarlyOut => false;

		public Visitor(Pond pond)
		{
			this.pond = pond;
		}

		public void VisitCell(int _)
		{
			pond.cellCount++;
		}

		public void VisitBoundary(int cell)
		{
		}
	}

	public static FishOvercrowingManager Instance;

	private readonly List<KPrefabID> allAquaticEntities = new List<KPrefabID>();

	private readonly List<Pond> ponds = new List<Pond>();

	private Cell[] grid;

	private int nextGeneration = 2;

	private static readonly Func<int, FloodFill.BoundaryCheckResult> isLiquidCell = (int cell) => (!Grid.IsNavigatableLiquidUnsafe(cell)) ? FloodFill.BoundaryCheckResult.Halt : FloodFill.BoundaryCheckResult.Continue;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		Instance = this;
		grid = new Cell[Grid.CellCount];
	}

	public void Add(KPrefabID aquaticEntity)
	{
		allAquaticEntities.Add(aquaticEntity);
	}

	public void Remove(KPrefabID aquaticEntity)
	{
		if (aquaticEntity.IsNullOrDestroyed())
		{
			return;
		}
		for (int num = allAquaticEntities.Count - 1; num >= 0; num--)
		{
			KPrefabID kPrefabID = allAquaticEntities[num];
			if (!kPrefabID.IsNullOrDestroyed() && kPrefabID.InstanceID == aquaticEntity.InstanceID)
			{
				allAquaticEntities.RemoveAt(num);
				break;
			}
		}
	}

	public void Sim1000ms(float dt)
	{
		int num = nextGeneration++;
		if (num == 0)
		{
			Array.Fill(grid, new Cell(0, -1));
			num = nextGeneration++;
		}
		for (int i = 0; i != ponds.Count; i++)
		{
			Pond pond = ponds[i];
			pond.fishes.Clear();
			pond.eggs.Clear();
			pond.cellCount = 0;
			pond.occupancy.dirty = true;
		}
		int num2 = ((ponds.Count == 0) ? (-1) : 0);
		foreach (KPrefabID allAquaticEntity in allAquaticEntities)
		{
			if (allAquaticEntity.IsNullOrDestroyed())
			{
				continue;
			}
			int num3 = Grid.PosToCell(allAquaticEntity);
			if (!Grid.IsValidCell(num3))
			{
				continue;
			}
			bool flag = false;
			Cell cell = grid[num3];
			flag = cell.Generation != num;
			int num4;
			if (!flag)
			{
				num4 = cell.PondIndex;
			}
			else if (num2 != -1 && num2 < ponds.Count)
			{
				num4 = num2;
				num2++;
				if (num2 == ponds.Count)
				{
					num2 = -1;
				}
			}
			else
			{
				Pond item = new Pond
				{
					fishes = new List<KPrefabID>(),
					eggs = new List<KPrefabID>()
				};
				ponds.Add(item);
				num4 = ponds.Count - 1;
			}
			Pond pond2 = ponds[num4];
			if (allAquaticEntity.HasTag(GameTags.Egg))
			{
				pond2.eggs.Add(allAquaticEntity);
			}
			else
			{
				pond2.fishes.Add(allAquaticEntity);
			}
			if (flag)
			{
				FloodFill.DepthTraverse(num3, new FloodFill.PredicateCondition(isLiquidCell), new VisitTracker(grid, new Cell(num, num4)), default(FloodFill.NoMaxDepth), new Visitor(pond2));
			}
		}
		if (num2 != -1)
		{
			int num5 = ponds.Count - num2;
			if (num5 > 0)
			{
				ponds.RemoveRange(num2, num5);
			}
		}
		allAquaticEntities.RemoveAll(Util.IsNullOrDestroyed);
	}

	public Pond GetPond(int cell)
	{
		if (!Grid.IsValidCell(cell))
		{
			return null;
		}
		Cell cell2 = grid[cell];
		return (cell2.Generation == nextGeneration - 1) ? ponds[cell2.PondIndex] : null;
	}

	public int GetFishInPondCount(int cell, HashSet<Tag> accepted_tags)
	{
		int num = 0;
		Pond pond = GetPond(cell);
		if (pond == null)
		{
			return 0;
		}
		foreach (KPrefabID fish in pond.fishes)
		{
			if (!fish.HasTag(GameTags.Creatures.Bagged) && !fish.HasTag(GameTags.Trapped) && accepted_tags.Contains(fish.PrefabTag))
			{
				num++;
			}
		}
		return num;
	}
}
