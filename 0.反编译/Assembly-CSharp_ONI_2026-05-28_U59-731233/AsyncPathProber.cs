using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine.Pool;

public static class AsyncPathProber
{
	public struct WorkResult
	{
		public Navigator navigator;

		public PathGrid pathGrid;

		public List<int> reachableCells;

		public List<int> newlyReachableCells;

		public List<int> noLongerReachableCells;

		public PathFinderAbilities abilitiesInstance;
	}

	public struct WorkOrder
	{
		public Navigator navigator;

		public NavGrid navGrid;

		public ulong gridClassification;

		public PathFinderAbilities abilities;

		public int originCell;

		public NavType startingNavType;

		public PathFinder.PotentialPath.Flags startingFlags;

		public ushort serialNo;

		public bool computeReachables;

		private static Comparison<int> WorkOrderIntSorter = (int lhs, int rhs) => lhs.CompareTo(rhs);

		public void Execute(PathFinder.PotentialList potentials, PathFinder.PotentialScratchPad scratch, ref WorkResult result)
		{
			if (result.pathGrid.SerialNo >= serialNo)
			{
				result.pathGrid.ResetProberCells();
			}
			PathProber.Run(originCell, abilities, navGrid, startingNavType, result.pathGrid, serialNo, scratch, potentials, startingFlags, result.reachableCells);
			result.abilitiesInstance = abilities;
			abilities = null;
			if (!computeReachables)
			{
				return;
			}
			result.reachableCells.Sort(WorkOrderIntSorter);
			int i = 0;
			int j = 0;
			while (i < navigator.occupiedCells.Count && j < result.reachableCells.Count)
			{
				if (navigator.occupiedCells[i] < result.reachableCells[j])
				{
					result.noLongerReachableCells.Add(navigator.occupiedCells[i]);
					i++;
				}
				else if (result.reachableCells[j] < navigator.occupiedCells[i])
				{
					result.newlyReachableCells.Add(result.reachableCells[j]);
					j++;
				}
				else
				{
					i++;
					j++;
				}
			}
			for (; i < navigator.occupiedCells.Count; i++)
			{
				result.noLongerReachableCells.Add(navigator.occupiedCells[i]);
			}
			for (; j < result.reachableCells.Count; j++)
			{
				result.newlyReachableCells.Add(result.reachableCells[j]);
			}
		}
	}

	private static class AsyncPathProbeWorker
	{
		public static void main(object _)
		{
			PathFinder.PotentialList potentials = new PathFinder.PotentialList();
			PathFinder.PotentialScratchPad scratch = new PathFinder.PotentialScratchPad(Pathfinding.Instance.MaxLinksPerCell());
			try
			{
				while (!Instance.Halting())
				{
					if (Instance.NextTask(out var order, out var result))
					{
						order.Execute(potentials, scratch, ref result);
						Instance.WorkCompleted(result);
					}
					else
					{
						Thread.Sleep(1);
					}
				}
			}
			catch (Exception source)
			{
				Instance.SetException(ExceptionDispatchInfo.Capture(source));
			}
		}
	}

	public class Manager
	{
		private const int kNovelProberPenalty = 10000;

		private const int kFailedNavigationPenalty = 10;

		private const int kNavigatorInFlightValue = -1;

		private Dictionary<Navigator, int> navigators = new Dictionary<Navigator, int>();

		private List<Navigator> navigatorOrdering = new List<Navigator>();

		private Comparison<Navigator> navigatorOrderer;

		private ushort activeSerialNo = 0;

		private Thread[] agents = null;

		private bool halting = false;

		private ExceptionDispatchInfo agentException = null;

		private List<WorkOrder> workQueue = new List<WorkOrder>();

		private List<WorkResult> finishedWork = new List<WorkResult>();

		private ConcurrentDictionary<ulong, ObjectPool<PathGrid>> gridPool = new ConcurrentDictionary<ulong, ObjectPool<PathGrid>>();

		private ObjectPool<List<int>> indexListPool = new ObjectPool<List<int>>(() => new List<int>(Grid.CellCount / 8), null, delegate(List<int> list)
		{
			list.Clear();
		}, null, collectionCheck: false, 12);

		private static int NavFailures;

		public bool Halting()
		{
			return halting;
		}

		public Manager()
		{
			navigatorOrderer = (Navigator lhs, Navigator rhs) => navigators.GetValueOrDefault(rhs, 0).CompareTo(navigators.GetValueOrDefault(lhs, 0));
		}

		public void SetException(ExceptionDispatchInfo ex)
		{
			lock (this)
			{
				agentException = ex;
			}
		}

		public void Register(Navigator nav)
		{
			lock (this)
			{
				if (navigators.ContainsKey(nav))
				{
					Debug.LogWarning("Double registration of navigator to AsyncManager: " + nav.ToString());
				}
				if (!gridPool.ContainsKey(nav.PathGrid.AllocatedClassification))
				{
					lock (this)
					{
						int width = nav.PathGrid.widthInCells;
						int height = nav.PathGrid.heightInCells;
						bool applyOffset = nav.PathGrid.applyOffset;
						NavType[] navTypes = new NavType[nav.PathGrid.ValidNavTypes.Length];
						nav.PathGrid.ValidNavTypes.CopyTo(navTypes, 0);
						gridPool[nav.PathGrid.AllocatedClassification] = new ObjectPool<PathGrid>(() => new PathGrid(width, height, applyOffset, navTypes), null, null, null, collectionCheck: false, 4 + agents.Length, 4 + agents.Length);
					}
				}
				navigators[nav] = 10000;
			}
		}

		public void Unregister(Navigator nav)
		{
			lock (this)
			{
				if (!navigators.Remove(nav))
				{
					Debug.LogWarning("Unregister of unknown navigator from AsyncManager: " + nav.ToString());
				}
			}
		}

		public void WorkCompleted(WorkResult result)
		{
			lock (this)
			{
				finishedWork.Add(result);
			}
		}

		public bool NextTask(out WorkOrder order, out WorkResult result)
		{
			lock (this)
			{
				if (workQueue.Count > 0)
				{
					order = workQueue[0];
					workQueue.RemoveAt(0);
					if (navigators.ContainsKey(order.navigator))
					{
						result = new WorkResult
						{
							navigator = order.navigator,
							pathGrid = gridPool[order.gridClassification].Get(),
							newlyReachableCells = indexListPool.Get(),
							noLongerReachableCells = indexListPool.Get(),
							reachableCells = indexListPool.Get()
						};
						navigators[order.navigator] = -1;
						return true;
					}
				}
			}
			order = default(WorkOrder);
			result = default(WorkResult);
			return false;
		}

		private WorkOrder makeWorkOrder(Navigator nav)
		{
			PathFinderAbilities currentAbilities = nav.GetCurrentAbilities();
			return new WorkOrder
			{
				navigator = nav,
				navGrid = nav.NavGrid,
				gridClassification = nav.PathGrid.AllocatedClassification,
				abilities = currentAbilities.Clone(),
				originCell = nav.cachedCell,
				startingNavType = nav.CurrentNavType,
				startingFlags = nav.flags,
				serialNo = activeSerialNo,
				computeReachables = nav.reportOccupation
			};
		}

		public void TickFrame()
		{
			lock (this)
			{
				if (agentException != null)
				{
					agentException.Throw();
					agentException = null;
				}
				activeSerialNo++;
				if (activeSerialNo == 0)
				{
					activeSerialNo++;
				}
				for (int i = 0; i < finishedWork.Count; i++)
				{
					WorkResult result = finishedWork[i];
					PathGrid pathGrid = result.pathGrid;
					if (navigators.ContainsKey(result.navigator))
					{
						pathGrid = result.navigator.TakeResult(ref result);
						navigators[result.navigator] = 0;
					}
					if (pathGrid != null)
					{
						gridPool[pathGrid.AllocatedClassification].Release(pathGrid);
					}
					indexListPool.Release(result.reachableCells);
					indexListPool.Release(result.newlyReachableCells);
					indexListPool.Release(result.noLongerReachableCells);
					result.abilitiesInstance.RecycleClone();
				}
				finishedWork.Clear();
				foreach (KeyValuePair<Navigator, int> navigator in navigators)
				{
					navigatorOrdering.Add(navigator.Key);
				}
				for (int num = navigatorOrdering.Count - 1; num >= 0; num--)
				{
					Navigator key = navigatorOrdering[num];
					int num2 = navigators[key];
					if (num2 == -1)
					{
						navigatorOrdering.RemoveAtSwap(num);
					}
					else
					{
						navigators[key] = num2 + 1;
					}
				}
				navigatorOrdering.Sort(navigatorOrderer);
				for (int j = 0; j < workQueue.Count; j++)
				{
					workQueue[j].abilities.RecycleClone();
				}
				workQueue.Clear();
				for (int k = 0; k < navigatorOrdering.Count; k++)
				{
					if (workQueue.Count >= 4)
					{
						break;
					}
					WorkOrder item = makeWorkOrder(navigatorOrdering[k]);
					if (Grid.IsValidCell(item.originCell))
					{
						workQueue.Add(item);
					}
					else
					{
						item.abilities.RecycleClone();
					}
				}
				navigatorOrdering.Clear();
			}
		}

		public void ApplyNavigationFailedPenalty(Navigator nav)
		{
			NavFailures++;
			lock (this)
			{
				if (navigators.TryGetValue(nav, out var value) && value >= 0)
				{
					navigators[nav] = value + 10;
				}
			}
		}

		public void Start(int agentCount)
		{
			agents = new Thread[agentCount];
			halting = false;
			for (int i = 0; i < agentCount; i++)
			{
				agents[i] = new Thread(AsyncPathProbeWorker.main);
				agents[i].Start();
			}
		}

		public void Shutdown()
		{
			lock (this)
			{
				halting = true;
			}
			Thread[] array = agents;
			foreach (Thread thread in array)
			{
				thread.Join();
			}
		}
	}

	public const int kMaxProbersPerFrame = 4;

	public static Manager Instance { get; private set; }

	public static void CreateInstance(int count)
	{
		DebugUtil.Assert(Instance == null);
		Instance = new Manager();
		Instance.Start(count);
	}

	public static void DestroyInstance()
	{
		Instance.Shutdown();
		Instance = null;
	}
}
