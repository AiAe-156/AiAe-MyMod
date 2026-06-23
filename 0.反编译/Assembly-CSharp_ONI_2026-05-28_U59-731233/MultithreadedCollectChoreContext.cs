using System.Collections.Generic;

public abstract class MultithreadedCollectChoreContext<ProviderType>
{
	public struct WorkBlock<Parent> : IWorkItem<Parent> where Parent : MultithreadedCollectChoreContext<ProviderType>
	{
		private int start;

		private int end;

		public WorkBlock(int start, int end)
		{
			this.start = start;
			this.end = end;
		}

		void IWorkItem<Parent>.Run(Parent shared_data, int threadIndex)
		{
			for (int i = start; i < end; i++)
			{
				shared_data.DefaultCollectChore(i, threadIndex);
			}
		}
	}

	public ProviderType provider = default(ProviderType);

	public ChoreConsumerState consumerState = null;

	public ListPool<Chore.Precondition.Context, MultithreadedCollectChoreContext<ProviderType>>.PooledList[] succeeded = null;

	public ListPool<Chore.Precondition.Context, MultithreadedCollectChoreContext<ProviderType>>.PooledList[] failed = null;

	public ListPool<Chore.Precondition.Context, MultithreadedCollectChoreContext<ProviderType>>.PooledList[] incomplete = null;

	public MultithreadedCollectChoreContext()
	{
	}

	public void Setup(ProviderType provider, ChoreConsumerState consumerState)
	{
		this.provider = provider;
		this.consumerState = consumerState;
		if (succeeded == null || succeeded.Length != GlobalJobManager.ThreadCount)
		{
			SetupThreadContext();
		}
	}

	private void SetupThreadContext()
	{
		if (succeeded != null)
		{
			TearDownThreadContext();
		}
		int threadCount = GlobalJobManager.ThreadCount;
		succeeded = new ListPool<Chore.Precondition.Context, MultithreadedCollectChoreContext<ProviderType>>.PooledList[threadCount];
		failed = new ListPool<Chore.Precondition.Context, MultithreadedCollectChoreContext<ProviderType>>.PooledList[threadCount];
		incomplete = new ListPool<Chore.Precondition.Context, MultithreadedCollectChoreContext<ProviderType>>.PooledList[threadCount];
		for (int i = 0; i < threadCount; i++)
		{
			succeeded[i] = ListPool<Chore.Precondition.Context, MultithreadedCollectChoreContext<ProviderType>>.Allocate();
			failed[i] = ListPool<Chore.Precondition.Context, MultithreadedCollectChoreContext<ProviderType>>.Allocate();
			incomplete[i] = ListPool<Chore.Precondition.Context, MultithreadedCollectChoreContext<ProviderType>>.Allocate();
		}
	}

	private void TearDownThreadContext()
	{
		int threadCount = GlobalJobManager.ThreadCount;
		for (int i = 0; i < threadCount; i++)
		{
			succeeded[i].Recycle();
			failed[i].Recycle();
			incomplete[i].Recycle();
		}
		succeeded = null;
		failed = null;
		incomplete = null;
	}

	public void Finish(List<Chore.Precondition.Context> pass, List<Chore.Precondition.Context> fail)
	{
		int threadCount = GlobalJobManager.ThreadCount;
		for (int i = 0; i < threadCount; i++)
		{
			pass.AddRange(succeeded[i]);
			succeeded[i].Clear();
			fail.AddRange(failed[i]);
			failed[i].Clear();
			foreach (Chore.Precondition.Context item in incomplete[i])
			{
				item.FinishPreconditions();
				if (item.IsSuccess())
				{
					pass.Add(item);
				}
				else
				{
					fail.Add(item);
				}
			}
			incomplete[i].Clear();
		}
	}

	public abstract void CollectChore(int index, List<Chore.Precondition.Context> succeed, List<Chore.Precondition.Context> incomplete, List<Chore.Precondition.Context> failed);

	public void DefaultCollectChore(int index, int threadIndex)
	{
		CollectChore(index, succeeded[threadIndex], incomplete[threadIndex], failed[threadIndex]);
	}
}
