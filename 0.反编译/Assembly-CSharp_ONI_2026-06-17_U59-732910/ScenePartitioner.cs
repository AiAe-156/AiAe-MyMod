using System;
using System.Collections.Generic;

public class ScenePartitioner : ISim1000ms
{
	private struct ScenePartitionerNode
	{
		public HybridListHashSet<HandleVector<int>.Handle> entries;

		public bool dirty;
	}

	private struct DirtyNode
	{
		public int layer;

		public int x;

		public int y;
	}

	public List<ScenePartitionerLayer> layers = new List<ScenePartitionerLayer>();

	private int nodeSize;

	private List<DirtyNode> dirtyNodes = new List<DirtyNode>();

	private ScenePartitionerNode[,,] nodes;

	private int queryId;

	public HashSet<ScenePartitionerLayer> toggledLayers = new HashSet<ScenePartitionerLayer>();

	public ScenePartitioner(int node_size, int layer_count, int scene_width, int scene_height)
	{
		nodeSize = node_size;
		int num = scene_width / node_size;
		int num2 = scene_height / node_size;
		nodes = new ScenePartitionerNode[layer_count, num2, num];
		for (int i = 0; i < nodes.GetLength(0); i++)
		{
			for (int j = 0; j < nodes.GetLength(1); j++)
			{
				for (int k = 0; k < nodes.GetLength(2); k++)
				{
					nodes[i, j, k].entries = new HybridListHashSet<HandleVector<int>.Handle>();
				}
			}
		}
		SimAndRenderScheduler.instance.Add(this);
	}

	public void FreeResources()
	{
		nodes = null;
	}

	[Obsolete]
	public ScenePartitionerLayer CreateMask(HashedString name)
	{
		foreach (ScenePartitionerLayer layer in layers)
		{
			if (layer.name == name)
			{
				return layer;
			}
		}
		ScenePartitionerLayer scenePartitionerLayer = new ScenePartitionerLayer(name, layers.Count);
		layers.Add(scenePartitionerLayer);
		DebugUtil.Assert(layers.Count <= nodes.GetLength(0));
		return scenePartitionerLayer;
	}

	public ScenePartitionerLayer CreateMask(string name)
	{
		foreach (ScenePartitionerLayer layer in layers)
		{
			if (layer.name == name)
			{
				return layer;
			}
		}
		HashCache.Get().Add(name);
		ScenePartitionerLayer scenePartitionerLayer = new ScenePartitionerLayer(name, layers.Count);
		layers.Add(scenePartitionerLayer);
		DebugUtil.Assert(layers.Count <= nodes.GetLength(0));
		return scenePartitionerLayer;
	}

	private int ClampNodeX(int x)
	{
		return Math.Min(Math.Max(x, 0), nodes.GetLength(2) - 1);
	}

	private int ClampNodeY(int y)
	{
		return Math.Min(Math.Max(y, 0), nodes.GetLength(1) - 1);
	}

	private Extents GetNodeExtents(int x, int y, int width, int height)
	{
		Extents result = default(Extents);
		result.x = ClampNodeX(x / nodeSize);
		result.y = ClampNodeY(y / nodeSize);
		result.width = 1 + ClampNodeX((x + width) / nodeSize) - result.x;
		result.height = 1 + ClampNodeY((y + height) / nodeSize) - result.y;
		return result;
	}

	private Extents GetNodeExtents(ScenePartitionerEntry entry)
	{
		return GetNodeExtents(entry.x, entry.y, entry.width, entry.height);
	}

	private void Insert(HandleVector<int>.Handle handle)
	{
		if (!GameScenePartitioner.Instance.Lookup(handle, out var entry))
		{
			Debug.LogWarning("Trying to put invalid handle go into scene partitioner");
			return;
		}
		if (entry.obj == null)
		{
			Debug.LogWarning("Trying to put null go into scene partitioner");
			return;
		}
		Extents nodeExtents = GetNodeExtents(entry);
		if (nodeExtents.x + nodeExtents.width > nodes.GetLength(2))
		{
			Debug.LogError(entry.obj.ToString() + " x/w " + nodeExtents.x + "/" + nodeExtents.width + " < " + nodes.GetLength(2));
		}
		if (nodeExtents.y + nodeExtents.height > nodes.GetLength(1))
		{
			Debug.LogError(entry.obj.ToString() + " y/h " + nodeExtents.y + "/" + nodeExtents.height + " < " + nodes.GetLength(1));
		}
		int layer = entry.layer;
		for (int i = nodeExtents.y; i < nodeExtents.y + nodeExtents.height; i++)
		{
			for (int j = nodeExtents.x; j < nodeExtents.x + nodeExtents.width; j++)
			{
				if (!nodes[layer, i, j].dirty)
				{
					nodes[layer, i, j].dirty = true;
					dirtyNodes.Add(new DirtyNode
					{
						layer = layer,
						x = j,
						y = i
					});
				}
				nodes[layer, i, j].entries.Add(handle);
			}
		}
	}

	private void Withdraw(int layer, Extents extents, HandleVector<int>.Handle handle)
	{
		if (extents.x + extents.width > nodes.GetLength(2))
		{
			Debug.LogError(" x/w " + extents.x + "/" + extents.width + " < " + nodes.GetLength(2));
		}
		if (extents.y + extents.height > nodes.GetLength(1))
		{
			Debug.LogError(" y/h " + extents.y + "/" + extents.height + " < " + nodes.GetLength(1));
		}
		for (int i = extents.y; i < extents.y + extents.height; i++)
		{
			for (int j = extents.x; j < extents.x + extents.width; j++)
			{
				nodes[layer, i, j].entries.Remove(handle);
			}
		}
	}

	public void Add(HandleVector<int>.Handle entry)
	{
		Insert(entry);
	}

	public void UpdatePosition(int x, int y, HandleVector<int>.Handle handle)
	{
		if (GameScenePartitioner.Instance.Lookup(handle, out var entry))
		{
			Withdraw(entry.layer, GetNodeExtents(entry), handle);
			entry.x = x;
			entry.y = y;
			Insert(handle);
		}
	}

	public void UpdatePosition(Extents e, HandleVector<int>.Handle handle)
	{
		if (GameScenePartitioner.Instance.Lookup(handle, out var entry))
		{
			Withdraw(entry.layer, GetNodeExtents(entry), handle);
			entry.x = e.x;
			entry.y = e.y;
			entry.width = e.width;
			entry.height = e.height;
			Insert(handle);
		}
	}

	public void Remove(HandleVector<int>.Handle handle)
	{
		if (!GameScenePartitioner.Instance.Lookup(handle, out var entry))
		{
			return;
		}
		Extents nodeExtents = GetNodeExtents(entry);
		if (nodeExtents.x + nodeExtents.width > nodes.GetLength(2))
		{
			Debug.LogError(" x/w " + nodeExtents.x + "/" + nodeExtents.width + " < " + nodes.GetLength(2));
		}
		if (nodeExtents.y + nodeExtents.height > nodes.GetLength(1))
		{
			Debug.LogError(" y/h " + nodeExtents.y + "/" + nodeExtents.height + " < " + nodes.GetLength(1));
		}
		int layer = entry.layer;
		for (int i = nodeExtents.y; i < nodeExtents.y + nodeExtents.height; i++)
		{
			for (int j = nodeExtents.x; j < nodeExtents.x + nodeExtents.width; j++)
			{
				if (!nodes[layer, i, j].dirty)
				{
					nodes[layer, i, j].dirty = true;
					dirtyNodes.Add(new DirtyNode
					{
						layer = layer,
						x = j,
						y = i
					});
				}
			}
		}
		entry.obj = null;
	}

	public void Sim1000ms(float dt)
	{
		foreach (DirtyNode dirtyNode in dirtyNodes)
		{
			HybridListHashSet<HandleVector<int>.Handle> entries = nodes[dirtyNode.layer, dirtyNode.y, dirtyNode.x].entries;
			for (int num = entries.Count - 1; num >= 0; num--)
			{
				if (!GameScenePartitioner.Instance.Lookup(entries[num], out var _))
				{
					entries.Remove(entries[num]);
				}
			}
			nodes[dirtyNode.layer, dirtyNode.y, dirtyNode.x].dirty = false;
		}
		dirtyNodes.Clear();
	}

	public void TriggerEvent(IEnumerable<int> cells, ScenePartitionerLayer layer, object event_data)
	{
		queryId++;
		RunLayerGlobalEvent(cells, layer, event_data);
		foreach (int cell in cells)
		{
			int x = 0;
			int y = 0;
			Grid.CellToXY(cell, out x, out y);
			TriggerEventInternal(x, y, 1, 1, layer, event_data);
		}
	}

	public void TriggerEvent(int x, int y, int width, int height, ScenePartitionerLayer layer, object event_data)
	{
		queryId++;
		RunLayerGlobalEvent(x, y, width, height, layer, event_data);
		TriggerEventInternal(x, y, width, height, layer, event_data);
	}

	private void TriggerEventInternal(int x, int y, int width, int height, ScenePartitionerLayer layer, object event_data)
	{
		Extents nodeExtents = GetNodeExtents(x, y, width, height);
		int num = Math.Min(nodeExtents.y + nodeExtents.height, nodes.GetLength(1));
		int num2 = Math.Max(nodeExtents.y, 0);
		int num3 = Math.Max(nodeExtents.x, 0);
		int num4 = Math.Min(nodeExtents.x + nodeExtents.width, nodes.GetLength(2));
		int layer2 = layer.layer;
		for (int i = num2; i < num; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				HybridListHashSet<HandleVector<int>.Handle> entries = nodes[layer2, i, j].entries;
				for (int num5 = entries.Count - 1; num5 >= 0; num5--)
				{
					if (GameScenePartitioner.Instance.Lookup(entries[num5], out var entry))
					{
						if (x + width - 1 >= entry.x && x <= entry.x + entry.width - 1 && y + height - 1 >= entry.y && y <= entry.y + entry.height - 1 && entry.queryId != queryId && entry.eventCallback != null && entry.obj != null)
						{
							entry.queryId = queryId;
							entry.eventCallback(event_data);
						}
					}
					else
					{
						entries.Remove(entries[num5]);
					}
				}
			}
		}
	}

	private void RunLayerGlobalEvent(IEnumerable<int> cells, ScenePartitionerLayer layer, object event_data)
	{
		if (layer.OnEvent == null)
		{
			return;
		}
		foreach (int cell in cells)
		{
			layer.OnEvent(cell, event_data);
		}
	}

	private void RunLayerGlobalEvent(int x, int y, int width, int height, ScenePartitionerLayer layer, object event_data)
	{
		if (layer.OnEvent == null)
		{
			return;
		}
		for (int i = y; i < y + height; i++)
		{
			for (int j = x; j < x + width; j++)
			{
				int num = Grid.XYToCell(j, i);
				if (Grid.IsValidCell(num))
				{
					layer.OnEvent(num, event_data);
				}
			}
		}
	}

	public void GatherEntries(int x, int y, int width, int height, ScenePartitionerLayer layer, object event_data, List<ScenePartitionerEntry> gathered_entries)
	{
		GatherEntries(x, y, width, height, layer, event_data, gathered_entries, ++queryId);
	}

	public void GatherEntries(int x, int y, int width, int height, ScenePartitionerLayer layer, object event_data, List<ScenePartitionerEntry> gathered_entries, int query_id)
	{
		Extents nodeExtents = GetNodeExtents(x, y, width, height);
		int num = Math.Min(nodeExtents.y + nodeExtents.height, nodes.GetLength(1));
		int num2 = Math.Max(nodeExtents.y, 0);
		int num3 = Math.Max(nodeExtents.x, 0);
		int num4 = Math.Min(nodeExtents.x + nodeExtents.width, nodes.GetLength(2));
		int layer2 = layer.layer;
		for (int i = num2; i < num; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				HybridListHashSet<HandleVector<int>.Handle> entries = nodes[layer2, i, j].entries;
				for (int num5 = entries.Count - 1; num5 >= 0; num5--)
				{
					if (GameScenePartitioner.Instance.Lookup(entries[num5], out var entry))
					{
						if (x + width - 1 >= entry.x && x <= entry.x + entry.width - 1 && y + height - 1 >= entry.y && y <= entry.y + entry.height - 1 && entry.queryId != queryId)
						{
							entry.queryId = queryId;
							gathered_entries.Add(entry);
						}
					}
					else
					{
						entries.Remove(entries[num5]);
					}
				}
			}
		}
	}

	public void VisitEntries<ContextType>(int x, int y, int width, int height, ScenePartitionerLayer layer, Func<object, ContextType, Util.IterationInstruction> visitor, ContextType context) where ContextType : class
	{
		Extents nodeExtents = GetNodeExtents(x, y, width, height);
		queryId++;
		int num = Math.Min(nodeExtents.y + nodeExtents.height, nodes.GetLength(1));
		int num2 = Math.Max(nodeExtents.y, 0);
		int num3 = Math.Max(nodeExtents.x, 0);
		int num4 = Math.Min(nodeExtents.x + nodeExtents.width, nodes.GetLength(2));
		int layer2 = layer.layer;
		for (int i = num2; i < num; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				HybridListHashSet<HandleVector<int>.Handle> entries = nodes[layer2, i, j].entries;
				for (int num5 = entries.Count - 1; num5 >= 0; num5--)
				{
					if (GameScenePartitioner.Instance.Lookup(entries[num5], out var entry))
					{
						if (x + width - 1 >= entry.x && x <= entry.x + entry.width - 1 && y + height - 1 >= entry.y && y <= entry.y + entry.height - 1 && entry.queryId != queryId)
						{
							entry.queryId = queryId;
							if (visitor(entry.obj, context) == Util.IterationInstruction.Halt)
							{
								return;
							}
						}
					}
					else
					{
						entries.Remove(entries[num5]);
					}
				}
			}
		}
	}

	public void VisitEntries<ContextType>(int x, int y, int width, int height, ScenePartitionerLayer layer, GameScenePartitioner.VisitorRef<ContextType> visitor, ref ContextType context) where ContextType : struct
	{
		Extents nodeExtents = GetNodeExtents(x, y, width, height);
		queryId++;
		int num = Math.Min(nodeExtents.y + nodeExtents.height, nodes.GetLength(1));
		int num2 = Math.Max(nodeExtents.y, 0);
		int num3 = Math.Max(nodeExtents.x, 0);
		int num4 = Math.Min(nodeExtents.x + nodeExtents.width, nodes.GetLength(2));
		int layer2 = layer.layer;
		for (int i = num2; i < num; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				HybridListHashSet<HandleVector<int>.Handle> entries = nodes[layer2, i, j].entries;
				for (int num5 = entries.Count - 1; num5 >= 0; num5--)
				{
					if (GameScenePartitioner.Instance.Lookup(entries[num5], out var entry))
					{
						if (x + width - 1 >= entry.x && x <= entry.x + entry.width - 1 && y + height - 1 >= entry.y && y <= entry.y + entry.height - 1 && entry.queryId != queryId)
						{
							entry.queryId = queryId;
							if (visitor(entry.obj, ref context) == Util.IterationInstruction.Halt)
							{
								return;
							}
						}
					}
					else
					{
						entries.Remove(entries[num5]);
					}
				}
			}
		}
	}

	public void ReadonlyVisitEntries<ContextType>(int x, int y, int width, int height, ScenePartitionerLayer layer, Func<object, ContextType, Util.IterationInstruction> visitor, ContextType context) where ContextType : class
	{
		Extents nodeExtents = GetNodeExtents(x, y, width, height);
		int num = Math.Min(nodeExtents.y + nodeExtents.height, nodes.GetLength(1));
		int num2 = Math.Max(nodeExtents.y, 0);
		int num3 = Math.Max(nodeExtents.x, 0);
		int num4 = Math.Min(nodeExtents.x + nodeExtents.width, nodes.GetLength(2));
		int layer2 = layer.layer;
		for (int i = num2; i < num; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				HybridListHashSet<HandleVector<int>.Handle> entries = nodes[layer2, i, j].entries;
				for (int num5 = entries.Count - 1; num5 >= 0; num5--)
				{
					if (GameScenePartitioner.Instance.Lookup(entries[num5], out var entry) && x + width - 1 >= entry.x && x <= entry.x + entry.width - 1 && y + height - 1 >= entry.y && y <= entry.y + entry.height - 1 && visitor(entry.obj, context) == Util.IterationInstruction.Halt)
					{
						return;
					}
				}
			}
		}
	}

	public void ReadonlyVisitEntries<ContextType>(int x, int y, int width, int height, ScenePartitionerLayer layer, GameScenePartitioner.VisitorRef<ContextType> visitor, ref ContextType context) where ContextType : struct
	{
		Extents nodeExtents = GetNodeExtents(x, y, width, height);
		int num = Math.Min(nodeExtents.y + nodeExtents.height, nodes.GetLength(1));
		int num2 = Math.Max(nodeExtents.y, 0);
		int num3 = Math.Max(nodeExtents.x, 0);
		int num4 = Math.Min(nodeExtents.x + nodeExtents.width, nodes.GetLength(2));
		int layer2 = layer.layer;
		for (int i = num2; i < num; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				HybridListHashSet<HandleVector<int>.Handle> entries = nodes[layer2, i, j].entries;
				for (int num5 = entries.Count - 1; num5 >= 0; num5--)
				{
					if (GameScenePartitioner.Instance.Lookup(entries[num5], out var entry) && x + width - 1 >= entry.x && x <= entry.x + entry.width - 1 && y + height - 1 >= entry.y && y <= entry.y + entry.height - 1 && visitor(entry.obj, ref context) == Util.IterationInstruction.Halt)
					{
						return;
					}
				}
			}
		}
	}

	public void Cleanup()
	{
		SimAndRenderScheduler.instance.Remove(this);
	}

	private static Util.IterationInstruction checkForAnyObjectHelper(object obj, ref bool found)
	{
		found = true;
		return Util.IterationInstruction.Halt;
	}

	public bool DoDebugLayersContainItemsOnCell(int cell)
	{
		int x = 0;
		int y = 0;
		Grid.CellToXY(cell, out x, out y);
		List<ScenePartitionerEntry> list = new List<ScenePartitionerEntry>();
		foreach (ScenePartitionerLayer toggledLayer in toggledLayers)
		{
			list.Clear();
			bool context = false;
			GameScenePartitioner.Instance.VisitEntries(x, y, 1, 1, toggledLayer, checkForAnyObjectHelper, ref context);
			if (context)
			{
				return true;
			}
		}
		return false;
	}
}
