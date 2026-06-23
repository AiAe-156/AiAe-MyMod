using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/GameScenePartitioner")]
public class GameScenePartitioner : KMonoBehaviour
{
	public delegate Util.IterationInstruction VisitorRef<ContextType>(object obj, ref ContextType context);

	public ScenePartitionerLayer solidChangedLayer;

	public ScenePartitionerLayer backwallChangedLayer;

	public ScenePartitionerLayer liquidChangedLayer;

	public ScenePartitionerLayer digDestroyedLayer;

	public ScenePartitionerLayer fogOfWarChangedLayer;

	public ScenePartitionerLayer decorProviderLayer;

	public ScenePartitionerLayer attackableEntitiesLayer;

	public ScenePartitionerLayer fetchChoreLayer;

	public ScenePartitionerLayer pickupablesLayer;

	public ScenePartitionerLayer storedPickupablesLayer;

	public ScenePartitionerLayer pickupablesChangedLayer;

	public ScenePartitionerLayer gasConduitsLayer;

	public ScenePartitionerLayer liquidConduitsLayer;

	public ScenePartitionerLayer solidConduitsLayer;

	public ScenePartitionerLayer wiresLayer;

	public ScenePartitionerLayer[] objectLayers;

	public ScenePartitionerLayer noisePolluterLayer;

	public ScenePartitionerLayer validNavCellChangedLayer;

	public ScenePartitionerLayer dirtyNavCellUpdateLayer;

	public ScenePartitionerLayer trapsLayer;

	public ScenePartitionerLayer floorSwitchActivatorLayer;

	public ScenePartitionerLayer floorSwitchActivatorChangedLayer;

	public ScenePartitionerLayer collisionLayer;

	public ScenePartitionerLayer lure;

	public ScenePartitionerLayer plants;

	public ScenePartitionerLayer plantsChangedLayer;

	public ScenePartitionerLayer industrialBuildings;

	public ScenePartitionerLayer completeBuildings;

	public ScenePartitionerLayer prioritizableObjects;

	public ScenePartitionerLayer contactConductiveLayer;

	private ScenePartitioner partitioner;

	private static GameScenePartitioner instance;

	private KCompactedVector<ScenePartitionerEntry> scenePartitionerEntries = new KCompactedVector<ScenePartitionerEntry>();

	private List<int> changedCells = new List<int>();

	public static GameScenePartitioner Instance
	{
		get
		{
			Debug.Assert(instance != null);
			return instance;
		}
	}

	public bool Lookup(HandleVector<int>.Handle handle, out ScenePartitionerEntry entry)
	{
		if (!scenePartitionerEntries.IsValid(handle) || !scenePartitionerEntries.IsVersionValid(handle))
		{
			entry = null;
			return false;
		}
		entry = scenePartitionerEntries.GetData(handle);
		return true;
	}

	protected override void OnPrefabInit()
	{
		Debug.Assert(instance == null);
		instance = this;
		partitioner = new ScenePartitioner(16, 68, Grid.WidthInCells, Grid.HeightInCells);
		solidChangedLayer = partitioner.CreateMask("SolidChanged");
		backwallChangedLayer = partitioner.CreateMask("BackwallChanged");
		liquidChangedLayer = partitioner.CreateMask("LiquidChanged");
		digDestroyedLayer = partitioner.CreateMask("DigDestroyed");
		fogOfWarChangedLayer = partitioner.CreateMask("FogOfWarChanged");
		decorProviderLayer = partitioner.CreateMask("DecorProviders");
		attackableEntitiesLayer = partitioner.CreateMask("FactionedEntities");
		fetchChoreLayer = partitioner.CreateMask("FetchChores");
		pickupablesLayer = partitioner.CreateMask("Pickupables");
		storedPickupablesLayer = partitioner.CreateMask("StoredPickupables");
		pickupablesChangedLayer = partitioner.CreateMask("PickupablesChanged");
		plantsChangedLayer = partitioner.CreateMask("PlantsChanged");
		gasConduitsLayer = partitioner.CreateMask("GasConduit");
		liquidConduitsLayer = partitioner.CreateMask("LiquidConduit");
		solidConduitsLayer = partitioner.CreateMask("SolidConduit");
		noisePolluterLayer = partitioner.CreateMask("NoisePolluters");
		validNavCellChangedLayer = partitioner.CreateMask("validNavCellChangedLayer");
		dirtyNavCellUpdateLayer = partitioner.CreateMask("dirtyNavCellUpdateLayer");
		trapsLayer = partitioner.CreateMask("trapsLayer");
		floorSwitchActivatorLayer = partitioner.CreateMask("FloorSwitchActivatorLayer");
		floorSwitchActivatorChangedLayer = partitioner.CreateMask("FloorSwitchActivatorChangedLayer");
		collisionLayer = partitioner.CreateMask("Collision");
		lure = partitioner.CreateMask("Lure");
		plants = partitioner.CreateMask("Plants");
		industrialBuildings = partitioner.CreateMask("IndustrialBuildings");
		completeBuildings = partitioner.CreateMask("CompleteBuildings");
		prioritizableObjects = partitioner.CreateMask("PrioritizableObjects");
		contactConductiveLayer = partitioner.CreateMask("ContactConductiveLayer");
		objectLayers = new ScenePartitionerLayer[45];
		for (int i = 0; i < 45; i++)
		{
			ObjectLayer objectLayer = (ObjectLayer)i;
			objectLayers[i] = partitioner.CreateMask(objectLayer.ToString());
		}
	}

	protected override void OnForcedCleanUp()
	{
		instance = null;
		partitioner.FreeResources();
		partitioner = null;
		solidChangedLayer = null;
		backwallChangedLayer = null;
		liquidChangedLayer = null;
		digDestroyedLayer = null;
		fogOfWarChangedLayer = null;
		decorProviderLayer = null;
		attackableEntitiesLayer = null;
		fetchChoreLayer = null;
		pickupablesLayer = null;
		storedPickupablesLayer = null;
		plantsChangedLayer = null;
		pickupablesChangedLayer = null;
		gasConduitsLayer = null;
		liquidConduitsLayer = null;
		solidConduitsLayer = null;
		noisePolluterLayer = null;
		validNavCellChangedLayer = null;
		dirtyNavCellUpdateLayer = null;
		trapsLayer = null;
		floorSwitchActivatorLayer = null;
		floorSwitchActivatorChangedLayer = null;
		contactConductiveLayer = null;
		objectLayers = null;
		scenePartitionerEntries.Clear();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		NavGrid navGrid = Pathfinding.Instance.GetNavGrid("MinionNavGrid");
		navGrid.OnNavGridUpdateComplete = (Action<List<int>>)Delegate.Combine(navGrid.OnNavGridUpdateComplete, new Action<List<int>>(OnNavGridUpdateComplete));
		NavTable navTable = navGrid.NavTable;
		navTable.OnValidCellChanged = (Action<int, NavType>)Delegate.Combine(navTable.OnValidCellChanged, new Action<int, NavType>(OnValidNavCellChanged));
	}

	public HandleVector<int>.Handle Add(string name, object obj, int x, int y, int width, int height, ScenePartitionerLayer layer, Action<object> event_callback)
	{
		ScenePartitionerEntry scenePartitionerEntry = ScenePartitionerEntry.EntryPool.Get();
		scenePartitionerEntry.Init(name, obj, x, y, width, height, layer, partitioner, event_callback);
		HandleVector<int>.Handle handle = scenePartitionerEntries.Allocate(scenePartitionerEntry);
		partitioner.Add(handle);
		return handle;
	}

	public HandleVector<int>.Handle Add(string name, object obj, Extents extents, ScenePartitionerLayer layer, Action<object> event_callback)
	{
		return Add(name, obj, extents.x, extents.y, extents.width, extents.height, layer, event_callback);
	}

	public HandleVector<int>.Handle Add(string name, object obj, int cell, ScenePartitionerLayer layer, Action<object> event_callback)
	{
		int x = 0;
		int y = 0;
		Grid.CellToXY(cell, out x, out y);
		return Add(name, obj, x, y, 1, 1, layer, event_callback);
	}

	public void AddGlobalLayerListener(ScenePartitionerLayer layer, Action<int, object> action)
	{
		layer.OnEvent = (Action<int, object>)Delegate.Combine(layer.OnEvent, action);
	}

	public void RemoveGlobalLayerListener(ScenePartitionerLayer layer, Action<int, object> action)
	{
		layer.OnEvent = (Action<int, object>)Delegate.Remove(layer.OnEvent, action);
	}

	public void TriggerEvent(List<int> cells, ScenePartitionerLayer layer, object event_data)
	{
		partitioner.TriggerEvent(cells, layer, event_data);
	}

	public void TriggerEvent(Extents extents, ScenePartitionerLayer layer, object event_data)
	{
		partitioner.TriggerEvent(extents.x, extents.y, extents.width, extents.height, layer, event_data);
	}

	public void TriggerEvent(int x, int y, int width, int height, ScenePartitionerLayer layer, object event_data)
	{
		partitioner.TriggerEvent(x, y, width, height, layer, event_data);
	}

	public void TriggerEvent(int cell, ScenePartitionerLayer layer, object event_data)
	{
		int x = 0;
		int y = 0;
		Grid.CellToXY(cell, out x, out y);
		TriggerEvent(x, y, 1, 1, layer, event_data);
	}

	[Obsolete("use Visit pattern instead")]
	public void GatherEntries(Extents extents, ScenePartitionerLayer layer, List<ScenePartitionerEntry> gathered_entries)
	{
		GatherEntries(extents.x, extents.y, extents.width, extents.height, layer, gathered_entries);
	}

	public void GatherEntries(int x_bottomLeft, int y_bottomLeft, int width, int height, ScenePartitionerLayer layer, List<ScenePartitionerEntry> gathered_entries)
	{
		partitioner.GatherEntries(x_bottomLeft, y_bottomLeft, width, height, layer, null, gathered_entries);
	}

	public void VisitEntries<ContextType>(int x, int y, int width, int height, ScenePartitionerLayer layer, Func<object, ContextType, Util.IterationInstruction> visitor, ContextType context) where ContextType : class
	{
		partitioner.VisitEntries(x, y, width, height, layer, visitor, context);
	}

	public void VisitEntries<ContextType>(int x, int y, int width, int height, ScenePartitionerLayer layer, VisitorRef<ContextType> visitor, ref ContextType context) where ContextType : struct
	{
		partitioner.VisitEntries(x, y, width, height, layer, visitor, ref context);
	}

	public void ReadonlyVisitEntries<ContextType>(int x, int y, int width, int height, ScenePartitionerLayer layer, Func<object, ContextType, Util.IterationInstruction> visitor, ContextType context) where ContextType : class
	{
		partitioner.ReadonlyVisitEntries(x, y, width, height, layer, visitor, context);
	}

	public void ReadonlyVisitEntries<ContextType>(int x, int y, int width, int height, ScenePartitionerLayer layer, VisitorRef<ContextType> visitor, ref ContextType context) where ContextType : struct
	{
		partitioner.ReadonlyVisitEntries(x, y, width, height, layer, visitor, ref context);
	}

	private void OnValidNavCellChanged(int cell, NavType nav_type)
	{
		changedCells.Add(cell);
	}

	private void OnNavGridUpdateComplete(List<int> dirty_nav_cells)
	{
		Instance.TriggerEvent(dirty_nav_cells, Instance.dirtyNavCellUpdateLayer, null);
		if (changedCells.Count > 0)
		{
			Instance.TriggerEvent(changedCells, Instance.validNavCellChangedLayer, null);
			changedCells.Clear();
		}
	}

	public void UpdatePosition(HandleVector<int>.Handle handle, int cell)
	{
		Vector2I vector2I = Grid.CellToXY(cell);
		UpdatePosition(handle, vector2I.x, vector2I.y);
	}

	public void UpdatePosition(HandleVector<int>.Handle handle, int x, int y)
	{
		if (handle.IsValid())
		{
			ScenePartitionerEntry data = scenePartitionerEntries.GetData(handle);
			data.UpdatePosition(handle, x, y);
		}
	}

	public void UpdatePosition(HandleVector<int>.Handle handle, Extents ext)
	{
		if (handle.IsValid())
		{
			ScenePartitionerEntry data = scenePartitionerEntries.GetData(handle);
			data.UpdatePosition(handle, ext);
		}
	}

	public void Free(ref HandleVector<int>.Handle handle)
	{
		if (handle.IsValid())
		{
			ScenePartitionerEntry data = scenePartitionerEntries.GetData(handle);
			data.Release(handle);
			scenePartitionerEntries.Free(handle);
			handle.Clear();
			ScenePartitionerEntry.EntryPool.Release(data);
		}
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		partitioner.Cleanup();
	}

	public bool DoDebugLayersContainItemsOnCell(int cell)
	{
		return partitioner.DoDebugLayersContainItemsOnCell(cell);
	}

	public List<ScenePartitionerLayer> GetLayers()
	{
		return partitioner.layers;
	}

	public void SetToggledLayers(HashSet<ScenePartitionerLayer> toggled_layers)
	{
		partitioner.toggledLayers = toggled_layers;
	}
}
