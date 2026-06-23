using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

public static class FloodFill
{
	public interface IVisitTracker
	{
		bool Contains(int cell);

		bool Add(int cell);
	}

	public struct HashSetVisitTracker : IVisitTracker
	{
		public HashSet<int> visited;

		public readonly bool Contains(int cell)
		{
			return visited.Contains(cell);
		}

		public readonly bool Add(int cell)
		{
			return visited.Add(cell);
		}

		public static HashSetVisitTracker Default()
		{
			HashSet<int> value = tlVisited.Value;
			value.Clear();
			return new HashSetVisitTracker
			{
				visited = value
			};
		}
	}

	public readonly struct GenerationGrid : IVisitTracker
	{
		private readonly byte generation;

		private readonly byte[] grid;

		public GenerationGrid(byte generation, byte[] grid)
		{
			this.generation = generation;
			this.grid = grid;
		}

		public bool Contains(int cell)
		{
			return grid[cell] == generation;
		}

		public bool Add(int cell)
		{
			if (Contains(cell))
			{
				return false;
			}
			grid[cell] = generation;
			return true;
		}

		public static GenerationGrid Default()
		{
			byte[] value = tlGrid.Value;
			if (value.Length != Grid.CellCount)
			{
				Debug.Log("Resize FloodFill.GenerationGrid");
				tlGrid.Value = new byte[Grid.CellCount];
				value = tlGrid.Value;
				tlGeneration.Value = 0;
			}
			byte value2 = tlGeneration.Value;
			value2++;
			if (value2 == 0)
			{
				Debug.Log("Reset FloodFill.GenerationGrid");
				Array.Clear(value, 0, value.Length);
				tlGeneration.Value = 1;
				value2 = 1;
			}
			else
			{
				tlGeneration.Value = value2;
			}
			return new GenerationGrid(value2, value);
		}
	}

	public enum BoundaryCheckResult
	{
		Continue,
		Halt
	}

	public interface IBoundaryCondition
	{
		BoundaryCheckResult Check(int cell);
	}

	public readonly struct PredicateCondition : IBoundaryCondition
	{
		private readonly Func<int, BoundaryCheckResult> predicate;

		public PredicateCondition(Func<int, BoundaryCheckResult> predicate)
		{
			this.predicate = predicate;
		}

		public BoundaryCheckResult Check(int cell)
		{
			return predicate(cell);
		}
	}

	public readonly struct ElementCheck : IBoundaryCondition
	{
		private readonly bool stop_at_solid;

		private readonly bool stop_at_liquid;

		public ElementCheck(bool stop_at_solid, bool stop_at_liquid)
		{
			DebugUtil.DevAssert(stop_at_solid || stop_at_liquid, "No sense in running this if it never does anything");
			this.stop_at_solid = stop_at_solid;
			this.stop_at_liquid = stop_at_liquid;
		}

		public BoundaryCheckResult Check(int cell)
		{
			Element element = Grid.Element[cell];
			if (stop_at_solid && element.IsSolid)
			{
				return BoundaryCheckResult.Halt;
			}
			if (stop_at_liquid && element.IsLiquid)
			{
				return BoundaryCheckResult.Halt;
			}
			return BoundaryCheckResult.Continue;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public readonly struct NoBoundary : IBoundaryCondition
	{
		public BoundaryCheckResult Check(int cell)
		{
			return BoundaryCheckResult.Continue;
		}
	}

	public interface IMaxDepth
	{
		bool Check(int cellDepth);
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct NoMaxDepth : IMaxDepth
	{
		public readonly bool Check(int cellDepth)
		{
			return true;
		}
	}

	public readonly struct MaxDepth : IMaxDepth
	{
		private readonly int value;

		public MaxDepth(int maxDepth)
		{
			value = maxDepth;
		}

		public bool Check(int cellDepth)
		{
			return cellDepth < value;
		}
	}

	public interface IVisitor
	{
		bool EarlyOut { get; }

		void VisitCell(int cell);

		void VisitBoundary(int cell);
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct DoNothing : IVisitor
	{
		public readonly bool EarlyOut => false;

		public readonly void VisitCell(int cell)
		{
		}

		public readonly void VisitBoundary(int cell)
		{
		}
	}

	public readonly struct Collector : IVisitor
	{
		private readonly List<int> cells;

		public bool EarlyOut => false;

		public void VisitCell(int cell)
		{
			cells.Add(cell);
		}

		public void VisitBoundary(int cell)
		{
		}

		public Collector(List<int> cells)
		{
			this.cells = cells;
		}
	}

	public class Finder : IVisitor
	{
		private readonly Func<int, bool> criteria;

		private int foundCell = Grid.InvalidCell;

		public int Cell => foundCell;

		public bool EarlyOut => foundCell != Grid.InvalidCell;

		public void VisitCell(int cell)
		{
			if (criteria(cell))
			{
				foundCell = cell;
			}
		}

		public void VisitBoundary(int cell)
		{
		}

		public Finder(Func<int, bool> criteria)
		{
			this.criteria = criteria;
		}
	}

	public class Scorer : IVisitor
	{
		private readonly Func<int, float> rateCell;

		private int threshold;

		private float bestScore;

		private int bestCell;

		public int BestCell => bestCell;

		public bool EarlyOut => threshold == 0;

		public Scorer(Func<int, float> rateCell, int threshold)
		{
			this.rateCell = rateCell;
			this.threshold = threshold;
			bestScore = float.NegativeInfinity;
			bestCell = Grid.InvalidCell;
		}

		public void VisitCell(int cell)
		{
			float num = rateCell(cell);
			if (num > bestScore)
			{
				bestScore = num;
				bestCell = cell;
			}
			if (threshold > 0)
			{
				threshold--;
			}
		}

		public void VisitBoundary(int cell)
		{
		}
	}

	private class RetainedQueue
	{
		private ulong[] buffer;

		private int head;

		private int tail;

		private int count;

		public int Count => count;

		public RetainedQueue(int initialCapacity = 1024)
		{
			buffer = new ulong[initialCapacity];
		}

		public void Enqueue(ulong item)
		{
			if (count == buffer.Length)
			{
				Grow();
			}
			buffer[tail] = item;
			tail = (tail + 1) % buffer.Length;
			count++;
		}

		public ulong Dequeue()
		{
			ulong result = buffer[head];
			head = (head + 1) % buffer.Length;
			count--;
			return result;
		}

		public void Clear()
		{
			head = 0;
			tail = 0;
			count = 0;
		}

		public void EnsureCapacity(int capacity)
		{
			if (buffer.Length < capacity)
			{
				Resize(capacity);
			}
		}

		private void Grow()
		{
			int newCapacity = buffer.Length * 2;
			Resize(newCapacity);
		}

		private void Resize(int newCapacity)
		{
			ulong[] destinationArray = new ulong[newCapacity];
			if (head < tail)
			{
				Array.Copy(buffer, head, destinationArray, 0, count);
			}
			else
			{
				Array.Copy(buffer, head, destinationArray, 0, buffer.Length - head);
				Array.Copy(buffer, 0, destinationArray, buffer.Length - head, tail);
			}
			buffer = destinationArray;
			head = 0;
			tail = count;
		}
	}

	private enum QueuedFrom : byte
	{
		None,
		Left,
		Right,
		Up,
		Down
	}

	private interface IRay
	{
		RetainedQueue ForwardQueue { get; }

		RetainedQueue PortQueue { get; }

		RetainedQueue StarboardQueue { get; }

		int Forward(int cell);

		int Port(int cell);

		int Starboard(int cell);
	}

	private struct NorthRay : IRay
	{
		private RetainedQueue above;

		private RetainedQueue left;

		private RetainedQueue right;

		public readonly RetainedQueue ForwardQueue => above;

		public readonly RetainedQueue PortQueue => left;

		public readonly RetainedQueue StarboardQueue => right;

		public readonly int Forward(int cell)
		{
			return Grid.CellAbove(cell);
		}

		public readonly int Port(int cell)
		{
			return Grid.CellLeft(cell);
		}

		public readonly int Starboard(int cell)
		{
			return Grid.CellRight(cell);
		}

		public static NorthRay Default()
		{
			return new NorthRay
			{
				above = tlAbove.Value,
				left = tlLeft.Value,
				right = tlRight.Value
			};
		}
	}

	private struct SouthRay : IRay
	{
		private RetainedQueue below;

		private RetainedQueue right;

		private RetainedQueue left;

		public readonly RetainedQueue ForwardQueue => below;

		public readonly RetainedQueue PortQueue => right;

		public readonly RetainedQueue StarboardQueue => left;

		public readonly int Forward(int cell)
		{
			return Grid.CellBelow(cell);
		}

		public readonly int Port(int cell)
		{
			return Grid.CellRight(cell);
		}

		public readonly int Starboard(int cell)
		{
			return Grid.CellLeft(cell);
		}

		public static SouthRay Default()
		{
			return new SouthRay
			{
				below = tlBelow.Value,
				right = tlRight.Value,
				left = tlLeft.Value
			};
		}
	}

	private struct EastRay : IRay
	{
		private RetainedQueue right;

		private RetainedQueue above;

		private RetainedQueue below;

		public readonly RetainedQueue ForwardQueue => right;

		public readonly RetainedQueue PortQueue => above;

		public readonly RetainedQueue StarboardQueue => below;

		public readonly int Forward(int cell)
		{
			return Grid.CellRight(cell);
		}

		public readonly int Port(int cell)
		{
			return Grid.CellAbove(cell);
		}

		public readonly int Starboard(int cell)
		{
			return Grid.CellBelow(cell);
		}

		public static EastRay Default()
		{
			return new EastRay
			{
				right = tlRight.Value,
				above = tlAbove.Value,
				below = tlBelow.Value
			};
		}
	}

	private struct WestRay : IRay
	{
		private RetainedQueue left;

		private RetainedQueue below;

		private RetainedQueue above;

		public readonly RetainedQueue ForwardQueue => left;

		public readonly RetainedQueue PortQueue => below;

		public readonly RetainedQueue StarboardQueue => above;

		public readonly int Forward(int cell)
		{
			return Grid.CellLeft(cell);
		}

		public readonly int Port(int cell)
		{
			return Grid.CellBelow(cell);
		}

		public readonly int Starboard(int cell)
		{
			return Grid.CellAbove(cell);
		}

		public static WestRay Default()
		{
			return new WestRay
			{
				left = tlLeft.Value,
				below = tlBelow.Value,
				above = tlAbove.Value
			};
		}
	}

	private interface ILateralRay
	{
		int FromSourceCell(int sourceCellIndex);

		void Enqueue(ulong cell);
	}

	private readonly struct PortRay<Ray> : ILateralRay where Ray : IRay
	{
		private readonly Ray ray;

		public int FromSourceCell(int sourceCellIndex)
		{
			return ray.Port(sourceCellIndex);
		}

		public void Enqueue(ulong cell)
		{
			ray.PortQueue.Enqueue(cell);
		}

		public PortRay(Ray ray)
		{
			this.ray = ray;
		}
	}

	private readonly struct StarboardRay<Ray> : ILateralRay where Ray : IRay
	{
		private readonly Ray ray;

		public int FromSourceCell(int sourceCellIndex)
		{
			return ray.Starboard(sourceCellIndex);
		}

		public void Enqueue(ulong cell)
		{
			ray.StarboardQueue.Enqueue(cell);
		}

		public StarboardRay(Ray ray)
		{
			this.ray = ray;
		}
	}

	private static readonly ThreadLocal<RetainedQueue> tlOpen = new ThreadLocal<RetainedQueue>(() => new RetainedQueue());

	private static readonly ThreadLocal<HashSet<int>> tlVisited = new ThreadLocal<HashSet<int>>(() => new HashSet<int>());

	private static readonly ThreadLocal<byte[]> tlGrid = new ThreadLocal<byte[]>(() => new byte[Grid.CellCount]);

	private static readonly ThreadLocal<byte> tlGeneration = new ThreadLocal<byte>(() => 0);

	private const ulong LEFT_BITS = 2305843009213693952uL;

	private const ulong RIGHT_BITS = 4611686018427387904uL;

	private const ulong UP_BITS = 6917529027641081856uL;

	private const ulong DOWN_BITS = 9223372036854775808uL;

	private const ulong INDEX_MASK = 4294967295uL;

	private const int INDEX_SHIFT = 0;

	private const int INDEX_MAX = int.MaxValue;

	private const ulong DEPTH_MASK = 2305843004918726656uL;

	private const int DEPTH_SHIFT = 32;

	private const uint DEPTH_MAX = 536870911u;

	private const ulong QUEUED_FROM_MASK = 16140901064495857664uL;

	private const int QUEUED_FROM_SHIFT = 61;

	private static readonly ThreadLocal<RetainedQueue> tlAbove = new ThreadLocal<RetainedQueue>(() => new RetainedQueue());

	private static readonly ThreadLocal<RetainedQueue> tlBelow = new ThreadLocal<RetainedQueue>(() => new RetainedQueue());

	private static readonly ThreadLocal<RetainedQueue> tlLeft = new ThreadLocal<RetainedQueue>(() => new RetainedQueue());

	private static readonly ThreadLocal<RetainedQueue> tlRight = new ThreadLocal<RetainedQueue>(() => new RetainedQueue());

	public static void BreadthTraverse<BoundaryCondition, VisitTracker, MyMaxDepth, Visitor>(int startCell, BoundaryCondition boundaryCondition, VisitTracker visited, MyMaxDepth maxDepth, Visitor visitor) where BoundaryCondition : IBoundaryCondition where VisitTracker : IVisitTracker where MyMaxDepth : IMaxDepth where Visitor : IVisitor
	{
		VerifyConstants();
		if (!maxDepth.Check(0))
		{
			return;
		}
		RetainedQueue value = tlOpen.Value;
		value.Clear();
		value.Enqueue((ulong)startCell);
		while (value.Count > 0)
		{
			ulong num = value.Dequeue();
			int cell = (int)(num & 0xFFFFFFFFu);
			if (!Grid.IsValidCell(cell) || !visited.Add(cell))
			{
				continue;
			}
			if (boundaryCondition.Check(cell) == BoundaryCheckResult.Halt)
			{
				visitor.VisitBoundary(cell);
				continue;
			}
			visitor.VisitCell(cell);
			if (visitor.EarlyOut)
			{
				break;
			}
			uint num2 = (uint)((int)((num & 0x1FFFFFFF00000000L) >> 32) + 1);
			DebugUtil.DevAssert(num2 <= 536870911, "nextDepth overflowed allocated bitfield");
			DebugUtil.DevAssert(num2 <= int.MaxValue, "nextDepth cannot be cast to int");
			if (maxDepth.Check((int)num2))
			{
				ulong num3 = (ulong)num2 << 32;
				value.Enqueue((ulong)Grid.CellLeft(cell) | num3);
				value.Enqueue((ulong)Grid.CellRight(cell) | num3);
				value.Enqueue((ulong)Grid.CellAbove(cell) | num3);
				value.Enqueue((ulong)Grid.CellBelow(cell) | num3);
			}
		}
	}

	public static void BreadthVisit(int startCell, Func<int, BoundaryCheckResult> boundaryCondition, HashSet<int> visitedCells)
	{
		BreadthTraverse(startCell, new PredicateCondition(boundaryCondition), new HashSetVisitTracker
		{
			visited = visitedCells
		}, default(NoMaxDepth), default(DoNothing));
	}

	public static void BreadthVisit(int startCell, Func<int, BoundaryCheckResult> boundaryCondition, int maxDepth)
	{
		BreadthTraverse(startCell, new PredicateCondition(boundaryCondition), GenerationGrid.Default(), new MaxDepth(maxDepth), default(DoNothing));
	}

	public static void BreadthVisit(int startCell, Func<int, BoundaryCheckResult> boundaryCondition)
	{
		BreadthTraverse(startCell, new PredicateCondition(boundaryCondition), GenerationGrid.Default(), default(NoMaxDepth), default(DoNothing));
	}

	public static void BreadthCollect(int startCell, Func<int, BoundaryCheckResult> boundaryCondition, HashSet<int> visitedCells, List<int> validCells)
	{
		BreadthTraverse(startCell, new PredicateCondition(boundaryCondition), new HashSetVisitTracker
		{
			visited = visitedCells
		}, default(NoMaxDepth), new Collector(validCells));
	}

	public static void BreadthCollect(int startCell, Func<int, BoundaryCheckResult> boundaryCondition, HashSet<int> visitedCells, List<int> validCells, int maxDepth)
	{
		BreadthTraverse(startCell, new PredicateCondition(boundaryCondition), new HashSetVisitTracker
		{
			visited = visitedCells
		}, new MaxDepth(maxDepth), new Collector(validCells));
	}

	public static void BreadthCollect(int startCell, Func<int, BoundaryCheckResult> boundaryCondition, List<int> validCells, int maxDepth)
	{
		BreadthTraverse(startCell, new PredicateCondition(boundaryCondition), GenerationGrid.Default(), new MaxDepth(maxDepth), new Collector(validCells));
	}

	public static int Find<MaxDepth>(Func<int, bool> criteria, int startCell, MaxDepth maxDepth, bool stopAtSolid, bool stopAtLiquid) where MaxDepth : IMaxDepth
	{
		Finder finder = new Finder(criteria);
		ElementCheck boundaryCondition = new ElementCheck(stopAtSolid, stopAtLiquid);
		if (maxDepth.Check(10))
		{
			BreadthTraverse(startCell, boundaryCondition, GenerationGrid.Default(), maxDepth, finder);
		}
		else
		{
			BreadthTraverse(startCell, boundaryCondition, HashSetVisitTracker.Default(), maxDepth, finder);
		}
		return finder.Cell;
	}

	public static bool Any<MaxDepth>(Func<int, bool> fn, int start_cell, MaxDepth max_depth, bool stop_at_solid, bool stop_at_liquid) where MaxDepth : IMaxDepth
	{
		return Find(fn, start_cell, max_depth, stop_at_solid, stop_at_liquid) != -1;
	}

	public static int FindBest(Func<int, float> rateCell, Func<int, BoundaryCheckResult> boundaryCondition, int startCell, int maxCellEvaluations = -1)
	{
		if (maxCellEvaluations == 0)
		{
			return Grid.InvalidCell;
		}
		Scorer scorer = new Scorer(rateCell, maxCellEvaluations);
		BreadthTraverse(startCell, new PredicateCondition(boundaryCondition), GenerationGrid.Default(), default(NoMaxDepth), scorer);
		return scorer.BestCell;
	}

	public static void BreadthTraverseNoBacktrack<BoundaryCondition, VisitTracker, MyMaxDepth, Visitor>(int startCell, BoundaryCondition boundaryCondition, VisitTracker visited, MyMaxDepth maxDepth, Visitor visitor) where BoundaryCondition : IBoundaryCondition where VisitTracker : IVisitTracker where MyMaxDepth : IMaxDepth where Visitor : IVisitor
	{
		VerifyConstants();
		if (!maxDepth.Check(0))
		{
			return;
		}
		RetainedQueue value = tlOpen.Value;
		value.Clear();
		value.Enqueue((ulong)startCell);
		while (value.Count > 0)
		{
			ulong num = value.Dequeue();
			int cell = (int)(num & 0xFFFFFFFFu);
			if (!Grid.IsValidCell(cell) || !visited.Add(cell))
			{
				continue;
			}
			if (boundaryCondition.Check(cell) == BoundaryCheckResult.Halt)
			{
				visitor.VisitBoundary(cell);
				continue;
			}
			visitor.VisitCell(cell);
			if (visitor.EarlyOut)
			{
				break;
			}
			uint num2 = (uint)((int)((num & 0x1FFFFFFF00000000L) >> 32) + 1);
			DebugUtil.DevAssert(num2 <= 536870911, "nextDepth overflowed allocated bitfield");
			DebugUtil.DevAssert(num2 <= int.MaxValue, "nextDepth cannot be cast to int");
			if (maxDepth.Check((int)num2))
			{
				byte num3 = (byte)((num & 0xE000000000000000uL) >> 61);
				ulong num4 = (ulong)num2 << 32;
				if (num3 != 1)
				{
					value.Enqueue((ulong)Grid.CellLeft(cell) | num4 | 0x4000000000000000L);
				}
				if (num3 != 2)
				{
					value.Enqueue((ulong)Grid.CellRight(cell) | num4 | 0x2000000000000000L);
				}
				if (num3 != 3)
				{
					value.Enqueue((ulong)Grid.CellAbove(cell) | num4 | 0x8000000000000000uL);
				}
				if (num3 != 4)
				{
					value.Enqueue((ulong)Grid.CellBelow(cell) | num4 | 0x6000000000000000L);
				}
			}
		}
	}

	public static void DepthTraverse<BoundaryCondition, VisitTracker, MyMaxDepth, Visitor>(int origin, BoundaryCondition boundaryCondition, VisitTracker visited, MyMaxDepth maxDepth, Visitor visitor) where BoundaryCondition : IBoundaryCondition where VisitTracker : IVisitTracker where MyMaxDepth : IMaxDepth where Visitor : IVisitor
	{
		VerifyConstants();
		if (!maxDepth.Check(0) || !Grid.IsValidCell(origin) || !visited.Add(origin))
		{
			return;
		}
		BoundaryCheckResult boundaryCheckResult = boundaryCondition.Check(origin);
		ulong originCell;
		if (boundaryCheckResult == BoundaryCheckResult.Continue)
		{
			visitor.VisitCell(origin);
			if (visitor.EarlyOut)
			{
				return;
			}
			RetainedQueue value = tlAbove.Value;
			RetainedQueue value2 = tlBelow.Value;
			RetainedQueue value3 = tlLeft.Value;
			RetainedQueue value4 = tlRight.Value;
			value.Clear();
			value2.Clear();
			value3.Clear();
			value4.Clear();
			originCell = (ulong)origin | 0uL;
			SeedDirection(value);
			SeedDirection(value2);
			SeedDirection(value3);
			SeedDirection(value4);
			NorthRay ray = NorthRay.Default();
			SouthRay ray2 = SouthRay.Default();
			WestRay ray3 = WestRay.Default();
			EastRay ray4 = EastRay.Default();
			while (value3.Count + value4.Count + value.Count + value2.Count > 0)
			{
				bool flag = false;
				if (ray.ForwardQueue.Count > 0)
				{
					flag = !CastRay(ray, boundaryCondition, visited, maxDepth, visitor);
				}
				else if (ray2.ForwardQueue.Count > 0)
				{
					flag = !CastRay(ray2, boundaryCondition, visited, maxDepth, visitor);
				}
				if (flag)
				{
					break;
				}
				while (ray3.ForwardQueue.Count > 0)
				{
					flag = !CastRay(ray3, boundaryCondition, visited, maxDepth, visitor);
					if (flag)
					{
						break;
					}
				}
				while (ray4.ForwardQueue.Count > 0)
				{
					flag = !CastRay(ray4, boundaryCondition, visited, maxDepth, visitor);
					if (flag)
					{
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		else
		{
			DebugUtil.DevAssert(boundaryCheckResult == BoundaryCheckResult.Halt, "unexpected CheckResult value");
			visitor.VisitBoundary(origin);
		}
		void SeedDirection(RetainedQueue rays)
		{
			rays.Enqueue(originCell);
		}
	}

	public static void DepthVisit(int startCell, Func<int, BoundaryCheckResult> boundaryCondition, HashSet<int> visitedCells)
	{
		DepthTraverse(startCell, new PredicateCondition(boundaryCondition), new HashSetVisitTracker
		{
			visited = visitedCells
		}, default(NoMaxDepth), default(DoNothing));
	}

	public static void DepthVisit(int startCell, Func<int, BoundaryCheckResult> boundaryCondition)
	{
		DepthTraverse(startCell, new PredicateCondition(boundaryCondition), GenerationGrid.Default(), default(NoMaxDepth), default(DoNothing));
	}

	public static void DepthCollect(int startCell, Func<int, BoundaryCheckResult> boundaryCondition, HashSet<int> visitedCells, List<int> validCells, int maxDepth)
	{
		DepthTraverse(startCell, new PredicateCondition(boundaryCondition), new HashSetVisitTracker
		{
			visited = visitedCells
		}, new MaxDepth(maxDepth), new Collector(validCells));
	}

	public static void DepthCollect(int startCell, Func<int, BoundaryCheckResult> boundaryCondition, HashSet<int> visitedCells, List<int> validCells)
	{
		DepthTraverse(startCell, new PredicateCondition(boundaryCondition), new HashSetVisitTracker
		{
			visited = visitedCells
		}, default(NoMaxDepth), new Collector(validCells));
	}

	public static void DepthCollect(int startCell, Func<int, BoundaryCheckResult> boundaryCondition, List<int> validCells, int maxDepth)
	{
		DepthTraverse(startCell, new PredicateCondition(boundaryCondition), GenerationGrid.Default(), new MaxDepth(maxDepth), new Collector(validCells));
	}

	public static void DepthCollect(int startCell, Func<int, BoundaryCheckResult> boundaryCondition, List<int> validCells)
	{
		DepthTraverse(startCell, new PredicateCondition(boundaryCondition), GenerationGrid.Default(), default(NoMaxDepth), new Collector(validCells));
	}

	private static void VerifyConstants()
	{
		DebugUtil.DevAssert(Grid.CellCount <= int.MaxValue, "Too few bits allocated to INDEX to handle all grid cells.");
		DebugUtil.DevAssert(test: true, "Unsigned DEPTH_MAX must not be larger than the maximum Int32 as that is the user-facing type for depth");
	}

	private static bool CastLateralRay<Ray, LateralRay, BoundaryCondition, VisitTracker, Visitor>(int originIndex, uint originDepth, int cellCount, Ray ray, LateralRay lateralRay, BoundaryCondition boundaryCondition, VisitTracker visited, Visitor visitor) where Ray : IRay where LateralRay : ILateralRay where BoundaryCondition : IBoundaryCondition where VisitTracker : IVisitTracker where Visitor : IVisitor
	{
		int num = lateralRay.FromSourceCell(originIndex);
		if (!Grid.IsValidCell(num))
		{
			return true;
		}
		bool flag = false;
		for (int i = 0; i != cellCount; i++)
		{
			num = ray.Forward(num);
			if (!visited.Add(num))
			{
				continue;
			}
			flag = boundaryCondition.Check(num) == BoundaryCheckResult.Continue;
			if (flag)
			{
				visitor.VisitCell(num);
				if (visitor.EarlyOut)
				{
					return false;
				}
				ulong num2 = (ulong)(originDepth + i + 2);
				DebugUtil.DevAssert(num2 <= 536870911, "cellDepth overflowed allocated bitfield");
				lateralRay.Enqueue((ulong)num | (num2 << 32));
			}
			else
			{
				visitor.VisitBoundary(num);
			}
		}
		if (flag)
		{
			ulong num3 = (ulong)(originDepth + cellCount + 1);
			DebugUtil.DevAssert(num3 <= 536870911, "cellDepth overflowed allocated bitfield");
			ray.ForwardQueue.Enqueue((ulong)num | (num3 << 32));
		}
		return true;
	}

	private static bool CastRay<Ray, BoundaryCondition, VisitTracker, MyMaxDepth, Visitor>(Ray ray, BoundaryCondition boundaryCondition, VisitTracker visited, MyMaxDepth maxDepth, Visitor visitor) where Ray : IRay where BoundaryCondition : IBoundaryCondition where VisitTracker : IVisitTracker where MyMaxDepth : IMaxDepth where Visitor : IVisitor
	{
		ulong num = ray.ForwardQueue.Dequeue();
		int num2 = (int)(num & 0xFFFFFFFFu);
		uint num3 = (uint)((num & 0x1FFFFFFF00000000L) >> 32);
		uint num4 = num3;
		int cell = num2;
		for (; maxDepth.Check((int)num4); num4++)
		{
			cell = ray.Forward(cell);
			if (!Grid.IsValidCell(cell) || !visited.Add(cell))
			{
				break;
			}
			if (boundaryCondition.Check(cell) == BoundaryCheckResult.Halt)
			{
				visitor.VisitBoundary(cell);
				break;
			}
			visitor.VisitCell(cell);
			if (visitor.EarlyOut)
			{
				return false;
			}
		}
		DebugUtil.DevAssert(num4 <= int.MaxValue, "depth cannot be cast to int");
		int num5 = (int)(num4 - num3);
		if (num5 == 0)
		{
			return true;
		}
		if (!CastLateralRay(num2, num3, num5, ray, new PortRay<Ray>(ray), boundaryCondition, visited, visitor))
		{
			return false;
		}
		if (!CastLateralRay(num2, num3, num5, ray, new StarboardRay<Ray>(ray), boundaryCondition, visited, visitor))
		{
			return false;
		}
		return true;
	}
}
