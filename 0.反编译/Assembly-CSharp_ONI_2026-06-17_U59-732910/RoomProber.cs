using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomProber : ISim1000ms
{
	public class Tuning : TuningData<Tuning>
	{
		public int maxRoomSize;
	}

	private struct Cell
	{
		public HandleVector<int>.Handle cavityID;

		public uint generation;

		public static Cell INVALID = new Cell
		{
			cavityID = HandleVector<int>.InvalidHandle,
			generation = 0u
		};
	}

	private struct Generation
	{
		private uint value;

		private readonly Cell[] grid;

		public Generation(Cell[] grid)
		{
			this.grid = grid;
			value = 1u;
		}

		public uint Next()
		{
			uint num = value++;
			if (num == 0)
			{
				Array.Fill(grid, Cell.INVALID);
				num = value++;
			}
			return num;
		}
	}

	private struct RefreshModule
	{
		private class CavityBuilder
		{
			public HandleVector<int>.Handle CavityID { get; private set; }

			public int MinX { get; private set; }

			public int MinY { get; private set; }

			public int MaxX { get; private set; }

			public int MaxY { get; private set; }

			public int NumCells { get; private set; }

			public void Reset(HandleVector<int>.Handle search_id)
			{
				CavityID = search_id;
				NumCells = 0;
				MinX = int.MaxValue;
				MinY = int.MaxValue;
				MaxX = 0;
				MaxY = 0;
			}

			public void AddCell(int flood_cell)
			{
				Grid.CellToXY(flood_cell, out var x, out var y);
				MinX = Math.Min(x, MinX);
				MinY = Math.Min(y, MinY);
				MaxX = Math.Max(x, MaxX);
				MaxY = Math.Max(y, MaxY);
				int numCells = NumCells + 1;
				NumCells = numCells;
			}
		}

		private struct BuildingId
		{
			public int prefab;

			public int instance;
		}

		private struct VisitTracker : FloodFill.IVisitTracker
		{
			private readonly Cell[] grid;

			public uint Generation { get; private set; }

			public VisitTracker(Cell[] grid, uint generation)
			{
				this.grid = grid;
				Generation = generation;
			}

			public bool Add(int cell)
			{
				Cell cell2 = grid[cell];
				if (cell2.generation != Generation)
				{
					grid[cell] = new Cell
					{
						cavityID = cell2.cavityID,
						generation = Generation
					};
					return true;
				}
				return false;
			}

			public readonly bool Contains(int cell)
			{
				return grid[cell].generation == Generation;
			}
		}

		private struct Visitor : FloodFill.IVisitor
		{
			private readonly Cell[] grid;

			private readonly List<int> cavityCells;

			private HandleVector<int>.Handle cavityID;

			private readonly CavityBuilder cavityBuilder;

			public readonly bool EarlyOut => false;

			public Visitor(Cell[] grid, List<int> cavityCells, HandleVector<int>.Handle cavityID, CavityBuilder cavityBuilder)
			{
				this.grid = grid;
				this.cavityCells = cavityCells;
				this.cavityID = cavityID;
				this.cavityBuilder = cavityBuilder;
			}

			public void VisitBoundary(int cell)
			{
				Cell cell2 = grid[cell];
				grid[cell] = new Cell
				{
					cavityID = HandleVector<int>.InvalidHandle,
					generation = cell2.generation
				};
			}

			public void VisitCell(int cell)
			{
				Cell cell2 = grid[cell];
				grid[cell] = new Cell
				{
					cavityID = cavityID,
					generation = cell2.generation
				};
				cavityCells.Add(cell);
				cavityBuilder.AddCell(cell);
			}
		}

		private readonly CavityBuilder cavityBuilder;

		private readonly RoomProber roomProber;

		private readonly List<int> dirtyCells;

		private readonly List<HandleVector<int>.Handle> condemnedCavities;

		private readonly List<CavityInfo> newCavities;

		private readonly List<KPrefabID> dirtyEntities;

		private readonly HashSet<HandleVector<int>.Handle> visitedCavities;

		private readonly HashSet<BuildingId> visitedBuildings;

		private Func<int, FloodFill.BoundaryCheckResult> cavityBoundary;

		public RefreshModule(RoomProber roomProber)
		{
			this.roomProber = roomProber;
			cavityBuilder = new CavityBuilder();
			dirtyCells = new List<int>();
			condemnedCavities = new List<HandleVector<int>.Handle>();
			newCavities = new List<CavityInfo>();
			dirtyEntities = new List<KPrefabID>();
			visitedCavities = new HashSet<HandleVector<int>.Handle>();
			visitedBuildings = new HashSet<BuildingId>();
			cavityBoundary = null;
		}

		public void Initialize()
		{
			cavityBoundary = (int cell) => IsCavityBoundary(cell) ? FloodFill.BoundaryCheckResult.Halt : FloodFill.BoundaryCheckResult.Continue;
		}

		public void Run()
		{
			CollectDirtyCells();
			CollectCondemnedCavities();
			BuildNewCavities();
			foreach (HandleVector<int>.Handle condemnedCavity in condemnedCavities)
			{
				CavityInfo data = roomProber.cavityInfos.GetData(condemnedCavity);
				dirtyEntities.Capacity = Math.Max(dirtyEntities.Capacity, dirtyEntities.Count + data.creatures.Count + data.otherEntities.Count);
				foreach (KPrefabID creature in data.creatures)
				{
					dirtyEntities.Add(creature);
				}
				foreach (KPrefabID otherEntity in data.otherEntities)
				{
					dirtyEntities.Add(otherEntity);
				}
				if (data.room != null)
				{
					roomProber.ClearRoom(data.room);
				}
				roomProber.cavityInfos.Free(condemnedCavity);
			}
			AddRoomContentsToCavities();
			roomProber.RefreshRooms(dirtyEntities);
			Recycle();
		}

		private readonly void Recycle()
		{
			dirtyCells.Clear();
			condemnedCavities.Clear();
			newCavities.Clear();
			dirtyEntities.Clear();
		}

		private unsafe readonly void CollectDirtyCells()
		{
			int* ptr = stackalloc int[5];
			*ptr = 0;
			ptr[1] = -Grid.WidthInCells;
			ptr[2] = -1;
			ptr[3] = 1;
			ptr[4] = Grid.WidthInCells;
			uint num = roomProber.generation.Next();
			foreach (int solidChange in roomProber.solidChanges)
			{
				for (int i = 0; i < 5; i++)
				{
					int num2 = solidChange + ptr[i];
					if (Grid.IsValidCell(num2))
					{
						Cell cell = roomProber.grid[num2];
						if (cell.generation != num)
						{
							roomProber.grid[num2] = new Cell
							{
								cavityID = cell.cavityID,
								generation = num
							};
							dirtyCells.Add(num2);
						}
					}
				}
			}
			roomProber.solidChanges.Clear();
		}

		private readonly void CollectCondemnedCavities()
		{
			uint num = roomProber.generation.Next();
			foreach (int dirtyCell in dirtyCells)
			{
				Cell cell = roomProber.grid[dirtyCell];
				if (cell.generation == num)
				{
					continue;
				}
				roomProber.grid[dirtyCell] = new Cell
				{
					cavityID = cell.cavityID,
					generation = num
				};
				if (!cell.cavityID.IsValid())
				{
					continue;
				}
				if (visitedCavities.Add(cell.cavityID))
				{
					condemnedCavities.Add(cell.cavityID);
				}
				foreach (int cell2 in roomProber.cavityInfos.GetData(cell.cavityID).cells)
				{
					roomProber.grid[cell2] = Cell.INVALID;
				}
			}
			visitedCavities.Clear();
		}

		private readonly void BuildNewCavities()
		{
			int num = 0;
			foreach (HandleVector<int>.Handle condemnedCavity in condemnedCavities)
			{
				num += roomProber.cavityInfos.GetData(condemnedCavity).NumCells;
			}
			dirtyCells.Capacity = Math.Max(dirtyCells.Capacity, dirtyCells.Count + num);
			foreach (HandleVector<int>.Handle condemnedCavity2 in condemnedCavities)
			{
				foreach (int cell in roomProber.cavityInfos.GetData(condemnedCavity2).cells)
				{
					dirtyCells.Add(cell);
				}
			}
			int num2 = ((condemnedCavities.Count <= 0) ? (-1) : 0);
			VisitTracker visited = new VisitTracker(roomProber.grid, roomProber.generation.Next());
			foreach (int dirtyCell in dirtyCells)
			{
				if (roomProber.grid[dirtyCell].generation == visited.Generation)
				{
					continue;
				}
				if (IsCavityBoundary(dirtyCell))
				{
					roomProber.grid[dirtyCell] = new Cell
					{
						cavityID = HandleVector<int>.InvalidHandle,
						generation = visited.Generation
					};
					continue;
				}
				CavityInfo cavityInfo = roomProber.CreateNewCavity();
				if (num2 >= 0)
				{
					CavityInfo data = roomProber.cavityInfos.GetData(condemnedCavities[num2]);
					cavityInfo.cells = data.cells;
					cavityInfo.cells.Clear();
					data.cells = null;
					num2++;
					if (num2 >= condemnedCavities.Count)
					{
						num2 = -1;
					}
				}
				else
				{
					cavityInfo.cells = new List<int>();
				}
				cavityBuilder.Reset(cavityInfo.handle);
				FloodFill.DepthTraverse(dirtyCell, new FloodFill.PredicateCondition(cavityBoundary), visited, default(FloodFill.NoMaxDepth), new Visitor(roomProber.grid, cavityInfo.cells, cavityInfo.handle, cavityBuilder));
				DebugUtil.DevAssert(cavityBuilder.NumCells > 0, "Degenerate cavities should have been detected and rejected prior to this point");
				cavityInfo.minX = cavityBuilder.MinX;
				cavityInfo.minY = cavityBuilder.MinY;
				cavityInfo.maxX = cavityBuilder.MaxX;
				cavityInfo.maxY = cavityBuilder.MaxY;
				newCavities.Add(cavityInfo);
			}
		}

		private void AddRoomContentsToCavities()
		{
			int maxRoomSize = TuningData<Tuning>.Get().maxRoomSize;
			foreach (CavityInfo newCavity in newCavities)
			{
				if (newCavity.NumCells > maxRoomSize)
				{
					continue;
				}
				foreach (int cell in newCavity.cells)
				{
					GameObject gameObject = Grid.Objects[cell, 1];
					if (gameObject == null)
					{
						gameObject = Grid.Objects[cell, 38];
					}
					if (gameObject == null)
					{
						continue;
					}
					KPrefabID component = gameObject.GetComponent<KPrefabID>();
					BuildingId item = new BuildingId
					{
						prefab = component.GetHashCode(),
						instance = component.InstanceID
					};
					if (visitedBuildings.Add(item))
					{
						if (component.HasTag(GameTags.RoomProberBuilding))
						{
							newCavity.AddBuilding(component);
						}
						else if (component.HasTag(GameTags.Plant))
						{
							newCavity.AddPlants(component);
						}
					}
				}
			}
			visitedBuildings.Clear();
		}
	}

	private Generation generation;

	public List<Room> rooms = new List<Room>();

	private readonly KCompactedVector<CavityInfo> cavityInfos = new KCompactedVector<CavityInfo>(1024);

	private readonly Cell[] grid;

	private readonly RefreshModule refresh;

	private readonly HashSet<int> solidChanges = new HashSet<int>();

	private bool dirty = true;

	public RoomProber()
	{
		CavityInfo cavityInfo = CreateNewCavity();
		cavityInfo.cells = new List<int>
		{
			Capacity = Grid.CellCount
		};
		for (int i = 0; i < Grid.CellCount; i++)
		{
			cavityInfo.cells.Add(i);
		}
		grid = new Cell[Grid.CellCount];
		Array.Fill(grid, new Cell
		{
			cavityID = cavityInfo.handle,
			generation = 0u
		});
		generation = new Generation(grid);
		solidChanges.Add(Grid.XYToCell(1, 1));
		refresh = new RefreshModule(this);
		refresh.Initialize();
		Refresh();
		Game instance = Game.Instance;
		instance.OnSpawnComplete = (System.Action)Delegate.Combine(instance.OnSpawnComplete, new System.Action(Refresh));
		World instance2 = World.Instance;
		instance2.OnSolidChanged = (Action<int>)Delegate.Combine(instance2.OnSolidChanged, new Action<int>(SolidChangedEvent));
		GameScenePartitioner.Instance.AddGlobalLayerListener(GameScenePartitioner.Instance.objectLayers[1], OnBuildingsChanged);
		GameScenePartitioner.Instance.AddGlobalLayerListener(GameScenePartitioner.Instance.objectLayers[2], OnBuildingsChanged);
	}

	private void SolidChangedEvent(int cell)
	{
		SolidChangedEvent(cell, ignoreDoors: true);
	}

	private void OnBuildingsChanged(int cell, object building)
	{
		if (GetCavityForCell(cell) != null)
		{
			solidChanges.Add(cell);
			dirty = true;
		}
	}

	public void TriggerBuildingChangedEvent(int cell, object building)
	{
		OnBuildingsChanged(cell, building);
	}

	public void SolidChangedEvent(int cell, bool ignoreDoors)
	{
		if (!ignoreDoors || !Grid.HasDoor[cell])
		{
			solidChanges.Add(cell);
			dirty = true;
		}
	}

	private CavityInfo CreateNewCavity()
	{
		CavityInfo cavityInfo = new CavityInfo();
		cavityInfo.handle = cavityInfos.Allocate(cavityInfo);
		return cavityInfo;
	}

	private static bool IsCavityBoundary(int cell)
	{
		if ((Grid.BuildMasks[cell] & (Grid.BuildFlags.Solid | Grid.BuildFlags.Foundation)) == 0)
		{
			return Grid.HasDoor[cell];
		}
		return true;
	}

	public void Refresh()
	{
		refresh.Run();
	}

	public void Sim1000ms(float dt)
	{
		if (dirty)
		{
			Refresh();
		}
	}

	private void CreateRoom(CavityInfo cavity)
	{
		Debug.Assert(cavity.room == null);
		Room room = (cavity.room = new Room
		{
			cavity = cavity
		});
		rooms.Add(room);
		room.roomType = Db.Get().RoomTypes.GetRoomType(room);
		AssignBuildingsToRoom(room);
	}

	private void ClearRoom(Room room)
	{
		UnassignBuildingsToRoom(room);
		room.CleanUp();
		rooms.Remove(room);
	}

	private void RefreshRooms(List<KPrefabID> dirtyEntities)
	{
		int maxRoomSize = TuningData<Tuning>.Get().maxRoomSize;
		foreach (CavityInfo data in cavityInfos.GetDataList())
		{
			if (!data.dirty)
			{
				continue;
			}
			Debug.Assert(data.room == null, "I expected info.room to always be null by this point");
			if (data.NumCells > 0)
			{
				if (data.NumCells <= maxRoomSize)
				{
					CreateRoom(data);
				}
				foreach (KPrefabID building in data.buildings)
				{
					building.Trigger(144050788, (object)data.room);
				}
				foreach (KPrefabID plant in data.plants)
				{
					plant.Trigger(144050788, (object)data.room);
				}
			}
			data.dirty = false;
		}
		foreach (KPrefabID dirtyEntity in dirtyEntities)
		{
			if (dirtyEntity != null)
			{
				dirtyEntity.Trigger(144050788);
			}
		}
		dirty = false;
	}

	private void AssignBuildingsToRoom(Room room)
	{
		Debug.Assert(room != null);
		RoomType roomType = room.roomType;
		if (roomType == Db.Get().RoomTypes.Neutral)
		{
			return;
		}
		foreach (KPrefabID building in room.buildings)
		{
			if (!(building == null) && !building.HasTag(GameTags.NotRoomAssignable) && building.TryGetComponent<Assignable>(out var component) && (roomType.primary_constraint == null || !roomType.primary_constraint.building_criteria(building)))
			{
				component.Assign(room);
			}
		}
	}

	private void UnassignKPrefabIDs(Room room, List<KPrefabID> buildings)
	{
		foreach (KPrefabID building in buildings)
		{
			if (!(building == null))
			{
				building.Trigger(144050788);
				if (building.TryGetComponent<Assignable>(out var component) && component.assignee == room)
				{
					component.Unassign();
				}
			}
		}
	}

	private void UnassignBuildingsToRoom(Room room)
	{
		Debug.Assert(room != null);
		UnassignKPrefabIDs(room, room.buildings);
		UnassignKPrefabIDs(room, room.plants);
	}

	public void UpdateRoom(CavityInfo cavity)
	{
		if (cavity == null)
		{
			return;
		}
		if (cavity.room != null)
		{
			ClearRoom(cavity.room);
			cavity.room = null;
		}
		CreateRoom(cavity);
		foreach (KPrefabID building in cavity.buildings)
		{
			if (building != null)
			{
				building.Trigger(144050788, (object)cavity.room);
			}
		}
		foreach (KPrefabID plant in cavity.plants)
		{
			if (plant != null)
			{
				plant.Trigger(144050788, (object)cavity.room);
			}
		}
	}

	public Room GetRoomOfGameObject(GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		int cell = Grid.PosToCell(go);
		if (!Grid.IsValidCell(cell))
		{
			return null;
		}
		return GetCavityForCell(cell)?.room;
	}

	public bool IsInRoomType(GameObject go, RoomType checkType)
	{
		Room roomOfGameObject = GetRoomOfGameObject(go);
		if (roomOfGameObject != null)
		{
			RoomType roomType = roomOfGameObject.roomType;
			return checkType == roomType;
		}
		return false;
	}

	private CavityInfo GetCavityInfo(HandleVector<int>.Handle id)
	{
		if (!id.IsValid())
		{
			return null;
		}
		return cavityInfos.GetData(id);
	}

	public CavityInfo GetCavityForCell(int cell)
	{
		if (!Grid.IsValidCell(cell))
		{
			return null;
		}
		HandleVector<int>.Handle cavityID = grid[cell].cavityID;
		return GetCavityInfo(cavityID);
	}
}
