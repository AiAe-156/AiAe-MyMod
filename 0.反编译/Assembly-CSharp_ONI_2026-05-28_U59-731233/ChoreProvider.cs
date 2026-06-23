using System;
using System.Collections.Generic;
using UnityEngine;

[SkipSaveFileSerialization]
[AddComponentMenu("KMonoBehaviour/scripts/ChoreProvider")]
public class ChoreProvider : KMonoBehaviour
{
	private class ChoreProviderCollectContext : MultithreadedCollectChoreContext<List<Chore>>
	{
		public override void CollectChore(int index, List<Chore.Precondition.Context> succeed, List<Chore.Precondition.Context> incomplete, List<Chore.Precondition.Context> failed)
		{
			provider[index].CollectChores(consumerState, succeed, incomplete, failed, is_attempting_override: false);
		}
	}

	public Dictionary<int, List<Chore>> choreWorldMap = new Dictionary<int, List<Chore>>();

	private int worldParentChangedHandle = -1;

	private int minionMigrationHandle = -1;

	private int enitityMigrationHandle = -1;

	private int worldRemovedHandle = -1;

	private static ChoreProviderCollectContext batch_context = new ChoreProviderCollectContext();

	private static WorkItemCollection<MultithreadedCollectChoreContext<List<Chore>>.WorkBlock<ChoreProviderCollectContext>, ChoreProviderCollectContext> batch_work_items = new WorkItemCollection<MultithreadedCollectChoreContext<List<Chore>>.WorkBlock<ChoreProviderCollectContext>, ChoreProviderCollectContext>();

	public string Name { get; private set; }

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		worldParentChangedHandle = Game.Instance.Subscribe(880851192, OnWorldParentChanged);
		minionMigrationHandle = Game.Instance.Subscribe(586301400, OnMinionMigrated);
		enitityMigrationHandle = Game.Instance.Subscribe(1142724171, OnEntityMigrated);
	}

	protected override void OnSpawn()
	{
		if (ClusterManager.Instance != null)
		{
			worldRemovedHandle = ClusterManager.Instance.Subscribe(-1078710002, OnWorldRemoved);
		}
		base.OnSpawn();
		Name = base.name;
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		Game.Instance.Unsubscribe(ref worldParentChangedHandle);
		Game.Instance.Unsubscribe(ref minionMigrationHandle);
		Game.Instance.Unsubscribe(ref enitityMigrationHandle);
		if (ClusterManager.Instance != null)
		{
			ClusterManager.Instance.Unsubscribe(ref worldRemovedHandle);
		}
	}

	protected virtual void OnWorldRemoved(object data)
	{
		int value = ((Boxed<int>)data).value;
		int parentWorldId = ClusterManager.Instance.GetWorld(value).ParentWorldId;
		if (choreWorldMap.TryGetValue(parentWorldId, out var value2))
		{
			ClearWorldChores(value2, value);
		}
	}

	protected virtual void OnWorldParentChanged(object data)
	{
		if (data is WorldParentChangedEventArgs { lastParentId: not 255 } e && e.lastParentId != e.world.ParentWorldId && choreWorldMap.TryGetValue(e.lastParentId, out var value))
		{
			if (!choreWorldMap.TryGetValue(e.world.ParentWorldId, out var value2))
			{
				value2 = (choreWorldMap[e.world.ParentWorldId] = new List<Chore>());
			}
			TransferChores(value, value2, e.world.ParentWorldId);
		}
	}

	protected virtual void OnEntityMigrated(object data)
	{
		if (data is MigrationEventArgs e && e.entity == base.gameObject && e.prevWorldId != e.targetWorldId && choreWorldMap.TryGetValue(e.prevWorldId, out var value))
		{
			if (!choreWorldMap.TryGetValue(e.targetWorldId, out var value2))
			{
				value2 = (choreWorldMap[e.targetWorldId] = new List<Chore>());
			}
			TransferChores(value, value2, e.targetWorldId);
		}
	}

	protected virtual void OnMinionMigrated(object data)
	{
		if (data is MinionMigrationEventArgs e && e.minionId.gameObject == base.gameObject && e.prevWorldId != e.targetWorldId && choreWorldMap.TryGetValue(e.prevWorldId, out var value))
		{
			if (!choreWorldMap.TryGetValue(e.targetWorldId, out var value2))
			{
				value2 = (choreWorldMap[e.targetWorldId] = new List<Chore>());
			}
			TransferChores(value, value2, e.targetWorldId);
		}
	}

	protected void TransferChores<T>(List<T> oldChores, List<T> newChores, int transferId) where T : Chore
	{
		int num = oldChores.Count - 1;
		for (int num2 = num; num2 >= 0; num2--)
		{
			T val = oldChores[num2];
			if (val.isNull)
			{
				DebugUtil.DevLogError("[" + val.GetType().Name + "] " + val.GetReportName() + " has no target");
			}
			else if (val.gameObject.GetMyParentWorldId() == transferId)
			{
				newChores.Add(val);
				oldChores[num2] = oldChores[num];
				oldChores.RemoveAt(num--);
			}
		}
	}

	protected void ClearWorldChores<T>(List<T> chores, int worldId) where T : Chore
	{
		int num = chores.Count - 1;
		for (int num2 = num; num2 >= 0; num2--)
		{
			T val = chores[num2];
			if (val.gameObject.GetMyWorldId() == worldId)
			{
				chores[num2] = chores[num];
				chores.RemoveAt(num--);
			}
		}
	}

	public virtual void AddChore(Chore chore)
	{
		chore.provider = this;
		List<Chore> value = null;
		int myParentWorldId = chore.gameObject.GetMyParentWorldId();
		if (!choreWorldMap.TryGetValue(myParentWorldId, out value))
		{
			value = (choreWorldMap[myParentWorldId] = new List<Chore>());
		}
		value.Add(chore);
	}

	public virtual void RemoveChore(Chore chore)
	{
		if (chore != null)
		{
			chore.provider = null;
			List<Chore> value = null;
			int myParentWorldId = chore.gameObject.GetMyParentWorldId();
			if (choreWorldMap.TryGetValue(myParentWorldId, out value))
			{
				value.Remove(chore);
			}
		}
	}

	public virtual void CollectChores(ChoreConsumerState consumer_state, List<Chore.Precondition.Context> succeeded, List<Chore.Precondition.Context> failed_contexts)
	{
		List<Chore> value = null;
		int myParentWorldId = consumer_state.gameObject.GetMyParentWorldId();
		if (!choreWorldMap.TryGetValue(myParentWorldId, out value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			if (value[num].provider == null)
			{
				value[num].Cancel("no provider");
				value[num] = value[value.Count - 1];
				value.RemoveAt(value.Count - 1);
			}
		}
		int num2 = 48;
		if (value.Count > num2)
		{
			batch_context.Setup(value, consumer_state);
			batch_work_items.Reset(batch_context);
			for (int i = 0; i < value.Count; i += 16)
			{
				batch_work_items.Add(new MultithreadedCollectChoreContext<List<Chore>>.WorkBlock<ChoreProviderCollectContext>(i, Math.Min(i + 16, value.Count)));
			}
			GlobalJobManager.Run(batch_work_items);
			batch_context.Finish(succeeded, failed_contexts);
			return;
		}
		foreach (Chore item in value)
		{
			item.CollectChores(consumer_state, succeeded, failed_contexts, is_attempting_override: false);
		}
	}
}
