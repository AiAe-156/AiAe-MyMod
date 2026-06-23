using System;
using System.Collections.Generic;

public class GlobalChoreProvider : ChoreProvider, IRender200ms
{
	public struct Fetch
	{
		public FetchChore chore;

		public int idsHash;

		public int cost;

		public PrioritySetting priority;

		public Storage.FetchCategory category;

		public bool IsBetterThan(Fetch fetch)
		{
			if (category != fetch.category)
			{
				return false;
			}
			if (idsHash != fetch.idsHash)
			{
				return false;
			}
			if (chore.choreType != fetch.chore.choreType)
			{
				return false;
			}
			if (priority.priority_class > fetch.priority.priority_class)
			{
				return true;
			}
			if (priority.priority_class == fetch.priority.priority_class)
			{
				if (priority.priority_value > fetch.priority.priority_value)
				{
					return true;
				}
				if (priority.priority_value == fetch.priority.priority_value)
				{
					return cost <= fetch.cost;
				}
			}
			return false;
		}
	}

	private class GlobalChoreProviderMultithreader : MultithreadedCollectChoreContext<GlobalChoreProvider>
	{
		public override void CollectChore(int index, List<Chore.Precondition.Context> succeed, List<Chore.Precondition.Context> incomplete, List<Chore.Precondition.Context> failed)
		{
			provider.fetches[index].chore.CollectChoresFromGlobalChoreProvider(consumerState, succeed, incomplete, failed, is_attempting_override: false);
		}
	}

	private class FetchComparer : IComparer<Fetch>
	{
		public int Compare(Fetch a, Fetch b)
		{
			int num = b.priority.priority_class - a.priority.priority_class;
			if (num != 0)
			{
				return num;
			}
			int num2 = b.priority.priority_value - a.priority.priority_value;
			if (num2 != 0)
			{
				return num2;
			}
			return a.cost - b.cost;
		}
	}

	private struct FindTopPriorityTask : IWorkItem<object>
	{
		private int start;

		private int end;

		private List<Prioritizable> worldCollection;

		public bool found;

		public static bool abort;

		public FindTopPriorityTask(int start, int end, List<Prioritizable> worldCollection)
		{
			this.start = start;
			this.end = end;
			this.worldCollection = worldCollection;
			found = false;
		}

		public void Run(object context, int threadIndex)
		{
			if (abort)
			{
				return;
			}
			for (int i = start; i != end && worldCollection.Count > i; i++)
			{
				if (!(worldCollection[i] == null) && worldCollection[i].IsTopPriority())
				{
					found = true;
					break;
				}
			}
			if (found)
			{
				abort = true;
			}
		}
	}

	public static GlobalChoreProvider Instance;

	public Dictionary<int, List<FetchChore>> fetchMap = new Dictionary<int, List<FetchChore>>();

	public List<Fetch> fetches = new List<Fetch>();

	private static readonly FetchComparer Comparer = new FetchComparer();

	private ClearableManager clearableManager;

	private HashSet<Tag> storageFetchableTags = new HashSet<Tag>();

	private static GlobalChoreProviderMultithreader batch_context = new GlobalChoreProviderMultithreader();

	private static WorkItemCollection<MultithreadedCollectChoreContext<GlobalChoreProvider>.WorkBlock<GlobalChoreProviderMultithreader>, GlobalChoreProviderMultithreader> batch_work_items = new WorkItemCollection<MultithreadedCollectChoreContext<GlobalChoreProvider>.WorkBlock<GlobalChoreProviderMultithreader>, GlobalChoreProviderMultithreader>();

	public static void DestroyInstance()
	{
		Instance = null;
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
		clearableManager = new ClearableManager();
	}

	protected override void OnWorldRemoved(object data)
	{
		int value = ((Boxed<int>)data).value;
		int parentWorldId = ClusterManager.Instance.GetWorld(value).ParentWorldId;
		if (fetchMap.TryGetValue(parentWorldId, out var value2))
		{
			ClearWorldChores(value2, value);
		}
		base.OnWorldRemoved(data);
	}

	protected override void OnWorldParentChanged(object data)
	{
		if (!(data is WorldParentChangedEventArgs { lastParentId: not 255 } e))
		{
			return;
		}
		base.OnWorldParentChanged(data);
		if (fetchMap.TryGetValue(e.lastParentId, out var value))
		{
			if (!fetchMap.TryGetValue(e.world.ParentWorldId, out var value2))
			{
				value2 = (fetchMap[e.world.ParentWorldId] = new List<FetchChore>());
			}
			TransferChores(value, value2, e.world.ParentWorldId);
		}
	}

	public override void AddChore(Chore chore)
	{
		if (chore is FetchChore fetchChore)
		{
			int myParentWorldId = fetchChore.gameObject.GetMyParentWorldId();
			if (!fetchMap.TryGetValue(myParentWorldId, out var value))
			{
				value = (fetchMap[myParentWorldId] = new List<FetchChore>());
			}
			chore.provider = this;
			value.Add(fetchChore);
		}
		else
		{
			base.AddChore(chore);
		}
	}

	public override void RemoveChore(Chore chore)
	{
		if (chore is FetchChore fetchChore)
		{
			int myParentWorldId = fetchChore.gameObject.GetMyParentWorldId();
			if (fetchMap.TryGetValue(myParentWorldId, out var value))
			{
				value.Remove(fetchChore);
			}
			chore.provider = null;
		}
		else
		{
			base.RemoveChore(chore);
		}
	}

	public void UpdateFetches(Navigator navigator)
	{
		List<FetchChore> value = null;
		int myParentWorldId = navigator.gameObject.GetMyParentWorldId();
		if (!fetchMap.TryGetValue(myParentWorldId, out value))
		{
			return;
		}
		fetches.Clear();
		for (int num = value.Count - 1; num >= 0; num--)
		{
			FetchChore fetchChore = value[num];
			if (!(fetchChore.driver != null) && (!(fetchChore.automatable != null) || !fetchChore.automatable.GetAutomationOnly()))
			{
				if (fetchChore.provider == null)
				{
					fetchChore.Cancel("no provider");
					value[num] = value[value.Count - 1];
					value.RemoveAt(value.Count - 1);
				}
				else
				{
					Storage destination = fetchChore.destination;
					if (!(destination == null))
					{
						int navigationCost = navigator.GetNavigationCost(destination);
						if (navigationCost != -1)
						{
							fetches.Add(new Fetch
							{
								chore = fetchChore,
								idsHash = fetchChore.tagsHash,
								cost = navigationCost,
								priority = fetchChore.masterPriority,
								category = destination.fetchCategory
							});
						}
					}
				}
			}
		}
		if (fetches.Count > 0)
		{
			fetches.Sort(Comparer);
			int i = 1;
			int num2 = 0;
			for (; i < fetches.Count; i++)
			{
				if (!fetches[num2].IsBetterThan(fetches[i]))
				{
					num2++;
					fetches[num2] = fetches[i];
				}
			}
			fetches.RemoveRange(num2 + 1, fetches.Count - num2 - 1);
		}
		clearableManager.CollectAndSortClearables(navigator);
	}

	public override void CollectChores(ChoreConsumerState consumer_state, List<Chore.Precondition.Context> succeeded, List<Chore.Precondition.Context> failed_contexts)
	{
		base.CollectChores(consumer_state, succeeded, failed_contexts);
		clearableManager.CollectChores(fetches, consumer_state, succeeded, failed_contexts);
		if (fetches.Count > 48)
		{
			batch_context.Setup(this, consumer_state);
			batch_work_items.Reset(batch_context);
			for (int i = 0; i < fetches.Count; i += 16)
			{
				batch_work_items.Add(new MultithreadedCollectChoreContext<GlobalChoreProvider>.WorkBlock<GlobalChoreProviderMultithreader>(i, Math.Min(i + 16, fetches.Count)));
			}
			GlobalJobManager.Run(batch_work_items);
			batch_context.Finish(succeeded, failed_contexts);
		}
		else
		{
			for (int j = 0; j < fetches.Count; j++)
			{
				fetches[j].chore.CollectChoresFromGlobalChoreProvider(consumer_state, succeeded, failed_contexts, is_attempting_override: false);
			}
		}
	}

	public HandleVector<int>.Handle RegisterClearable(Clearable clearable)
	{
		return clearableManager.RegisterClearable(clearable);
	}

	public void UnregisterClearable(HandleVector<int>.Handle handle)
	{
		clearableManager.UnregisterClearable(handle);
	}

	protected override void OnLoadLevel()
	{
		base.OnLoadLevel();
		Instance = null;
	}

	public void Render200ms(float dt)
	{
		UpdateStorageFetchableBits();
	}

	private void UpdateStorageFetchableBits()
	{
		ChoreType storageFetch = Db.Get().ChoreTypes.StorageFetch;
		ChoreType foodFetch = Db.Get().ChoreTypes.FoodFetch;
		storageFetchableTags.Clear();
		List<int> worldIDsSorted = ClusterManager.Instance.GetWorldIDsSorted();
		for (int i = 0; i < worldIDsSorted.Count; i++)
		{
			if (!fetchMap.TryGetValue(worldIDsSorted[i], out var value))
			{
				continue;
			}
			for (int j = 0; j < value.Count; j++)
			{
				FetchChore fetchChore = value[j];
				if ((fetchChore.choreType == storageFetch || fetchChore.choreType == foodFetch) && (bool)fetchChore.destination)
				{
					int cell = Grid.PosToCell(fetchChore.destination);
					if (MinionGroupProber.Get().IsReachable(cell, fetchChore.destination.GetOffsets(cell)))
					{
						storageFetchableTags.UnionWith(fetchChore.tags);
					}
				}
			}
		}
	}

	public bool ClearableHasDestination(Pickupable pickupable)
	{
		KPrefabID kPrefabID = pickupable.KPrefabID;
		return storageFetchableTags.Contains(kPrefabID.PrefabTag);
	}
}
