using System;
using System.Collections.Generic;
using KSerialization;
using ProcGenGame;
using TUNING;
using UnityEngine;

public class ClusterManager : KMonoBehaviour, ISaveLoadable
{
	public enum RocketStatesForAudio
	{
		Grounded,
		ReadyForLaunch,
		Launching,
		InSpace,
		Landing
	}

	public static int MAX_ROCKET_INTERIOR_COUNT = 16;

	public static RocketStatesForAudio RocketInteriorState = RocketStatesForAudio.Grounded;

	public static ClusterManager Instance;

	private ClusterGrid m_grid;

	[Serialize]
	private int m_numRings = 9;

	[Serialize]
	private int activeWorldIdx;

	public const byte INVALID_WORLD_IDX = byte.MaxValue;

	public static Color[] worldColors = new Color[6]
	{
		Color.HSVToRGB(0.15f, 0.3f, 0.5f),
		Color.HSVToRGB(0.3f, 0.3f, 0.5f),
		Color.HSVToRGB(0.45f, 0.3f, 0.5f),
		Color.HSVToRGB(0.6f, 0.3f, 0.5f),
		Color.HSVToRGB(0.75f, 0.3f, 0.5f),
		Color.HSVToRGB(0.9f, 0.3f, 0.5f)
	};

	private List<WorldContainer> m_worldContainers = new List<WorldContainer>();

	[MyCmpGet]
	private ClusterPOIManager m_clusterPOIsManager;

	private Dictionary<int, List<IAssignableIdentity>> minionsByWorld = new Dictionary<int, List<IAssignableIdentity>>();

	private MinionMigrationEventArgs migrationEvArg = new MinionMigrationEventArgs();

	private MigrationEventArgs critterMigrationEvArg = new MigrationEventArgs();

	private List<int> _worldIDs = new List<int>();

	private List<int> _discoveredAsteroidIds = new List<int>();

	private Tuple<int, int> activeWorldChangedData = new Tuple<int, int>(0, 0);

	public int worldCount => m_worldContainers.Count;

	public int activeWorldId => activeWorldIdx;

	public List<WorldContainer> WorldContainers => m_worldContainers;

	public Dictionary<int, List<IAssignableIdentity>> MinionsByWorld
	{
		get
		{
			minionsByWorld.Clear();
			for (int i = 0; i < Components.MinionAssignablesProxy.Count; i++)
			{
				if (!Components.MinionAssignablesProxy[i].GetTargetGameObject().HasTag(GameTags.Dead))
				{
					int id = Components.MinionAssignablesProxy[i].GetTargetGameObject().GetComponent<KMonoBehaviour>().GetMyWorld()
						.id;
					if (!minionsByWorld.ContainsKey(id))
					{
						minionsByWorld.Add(id, new List<IAssignableIdentity>());
					}
					minionsByWorld[id].Add(Components.MinionAssignablesProxy[i]);
				}
			}
			return minionsByWorld;
		}
	}

	public WorldContainer activeWorld => GetWorld(activeWorldId);

	public static void DestroyInstance()
	{
		Instance = null;
	}

	public ClusterPOIManager GetClusterPOIManager()
	{
		return m_clusterPOIsManager;
	}

	public void RegisterWorldContainer(WorldContainer worldContainer)
	{
		m_worldContainers.Add(worldContainer);
	}

	public void UnregisterWorldContainer(WorldContainer worldContainer)
	{
		BoxingTrigger(-1078710002, worldContainer.id);
		m_worldContainers.Remove(worldContainer);
	}

	public List<int> GetWorldIDsSorted()
	{
		ListPool<WorldContainer, ClusterManager>.PooledList pooledList = ListPool<WorldContainer, ClusterManager>.Allocate(m_worldContainers);
		pooledList.Sort((WorldContainer a, WorldContainer b) => a.DiscoveryTimestamp.CompareTo(b.DiscoveryTimestamp));
		_worldIDs.Clear();
		foreach (WorldContainer item in pooledList)
		{
			_worldIDs.Add(item.id);
		}
		pooledList.Recycle();
		return _worldIDs;
	}

	public List<int> GetDiscoveredAsteroidIDsSorted()
	{
		ListPool<WorldContainer, ClusterManager>.PooledList pooledList = ListPool<WorldContainer, ClusterManager>.Allocate(m_worldContainers);
		pooledList.Sort((WorldContainer a, WorldContainer b) => a.DiscoveryTimestamp.CompareTo(b.DiscoveryTimestamp));
		_discoveredAsteroidIds.Clear();
		for (int num = 0; num < pooledList.Count; num++)
		{
			if (pooledList[num].IsDiscovered && !pooledList[num].IsModuleInterior)
			{
				_discoveredAsteroidIds.Add(pooledList[num].id);
			}
		}
		pooledList.Recycle();
		return _discoveredAsteroidIds;
	}

	public WorldContainer GetStartWorld()
	{
		foreach (WorldContainer worldContainer in WorldContainers)
		{
			if (worldContainer.IsStartWorld)
			{
				return worldContainer;
			}
		}
		return WorldContainers[0];
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Instance = this;
		SaveLoader instance = SaveLoader.Instance;
		instance.OnWorldGenComplete = (Action<Cluster>)Delegate.Combine(instance.OnWorldGenComplete, new Action<Cluster>(OnWorldGenComplete));
	}

	protected override void OnSpawn()
	{
		if (m_grid == null)
		{
			m_grid = new ClusterGrid(m_numRings);
		}
		UpdateWorldReverbSnapshot(activeWorldId);
		base.OnSpawn();
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
	}

	private void OnWorldGenComplete(Cluster clusterLayout)
	{
		m_numRings = clusterLayout.numRings;
		m_grid = new ClusterGrid(m_numRings);
		AxialI location = AxialI.ZERO;
		foreach (WorldGen world in clusterLayout.worlds)
		{
			WorldContainer worldContainer = CreateAsteroidWorldContainer(world);
			int id = worldContainer.id;
			Vector2I position = world.GetPosition();
			Vector2I vector2I = position + world.GetSize();
			if (world.isStartingWorld)
			{
				location = world.GetClusterLocation();
			}
			for (int i = position.y; i < vector2I.y; i++)
			{
				for (int j = position.x; j < vector2I.x; j++)
				{
					int num = Grid.XYToCell(j, i);
					Grid.WorldIdx[num] = (byte)id;
					Pathfinding.Instance.AddDirtyNavGridCell(num);
				}
			}
			if (world.isStartingWorld)
			{
				activeWorldIdx = id;
			}
		}
		this.GetSMI<ClusterFogOfWarManager.Instance>().RevealLocation(location, 1);
		m_clusterPOIsManager.PopulatePOIsFromWorldGen(clusterLayout);
	}

	private int GetNextWorldId()
	{
		HashSetPool<int, ClusterManager>.PooledHashSet pooledHashSet = HashSetPool<int, ClusterManager>.Allocate();
		foreach (WorldContainer worldContainer in m_worldContainers)
		{
			pooledHashSet.Add(worldContainer.id);
		}
		Debug.Assert(m_worldContainers.Count < 255, "Oh no! We're trying to generate our 255th world in this save, things are going to start going badly...");
		for (int i = 0; i < 255; i++)
		{
			if (!pooledHashSet.Contains(i))
			{
				pooledHashSet.Recycle();
				return i;
			}
		}
		pooledHashSet.Recycle();
		return 255;
	}

	private WorldContainer CreateAsteroidWorldContainer(WorldGen world)
	{
		int nextWorldId = GetNextWorldId();
		GameObject gameObject = Util.KInstantiate(Assets.GetPrefab("Asteroid"));
		WorldContainer component = gameObject.GetComponent<WorldContainer>();
		component.SetID(nextWorldId);
		component.SetWorldDetails(world);
		AsteroidGridEntity component2 = gameObject.GetComponent<AsteroidGridEntity>();
		if (world != null)
		{
			AxialI clusterLocation = world.GetClusterLocation();
			component2.Init(component.GetRandomName(), clusterLocation, world.Settings.world.asteroidIcon);
		}
		else
		{
			component2.Init("", AxialI.ZERO, "");
		}
		if (component.IsStartWorld)
		{
			OrbitalMechanics component3 = gameObject.GetComponent<OrbitalMechanics>();
			if (component3 != null)
			{
				component3.CreateOrbitalObject(Db.Get().OrbitalTypeCategories.backgroundEarth.Id);
			}
		}
		gameObject.SetActive(value: true);
		return component;
	}

	private void CreateDefaultAsteroidWorldContainer()
	{
		if (m_worldContainers.Count != 0)
		{
			return;
		}
		Debug.LogWarning("Cluster manager has no world containers, create a default using Grid settings.");
		WorldContainer worldContainer = CreateAsteroidWorldContainer(null);
		int id = worldContainer.id;
		for (int i = (int)worldContainer.minimumBounds.y; (float)i <= worldContainer.maximumBounds.y; i++)
		{
			for (int j = (int)worldContainer.minimumBounds.x; (float)j <= worldContainer.maximumBounds.x; j++)
			{
				int num = Grid.XYToCell(j, i);
				Grid.WorldIdx[num] = (byte)id;
				Pathfinding.Instance.AddDirtyNavGridCell(num);
			}
		}
	}

	public void InitializeWorldGrid()
	{
		if (SaveLoader.Instance.GameInfo.IsVersionOlderThan(7, 20))
		{
			CreateDefaultAsteroidWorldContainer();
		}
		bool flag = false;
		foreach (WorldContainer worldContainer2 in m_worldContainers)
		{
			WorldContainer worldContainer = worldContainer2;
			Vector2I worldOffset = worldContainer.WorldOffset;
			Vector2I vector2I = worldOffset + worldContainer.WorldSize;
			for (int i = worldOffset.y; i < vector2I.y; i++)
			{
				for (int j = worldOffset.x; j < vector2I.x; j++)
				{
					int num = Grid.XYToCell(j, i);
					Grid.WorldIdx[num] = (byte)worldContainer.id;
					Pathfinding.Instance.AddDirtyNavGridCell(num);
				}
			}
			flag |= worldContainer.IsDiscovered;
		}
		if (!flag)
		{
			Debug.LogWarning("No worlds have been discovered. Setting the active world to discovered");
			activeWorld.SetDiscovered();
		}
	}

	public void SetActiveWorld(int worldIdx)
	{
		int num = activeWorldIdx;
		if (num != worldIdx)
		{
			activeWorldIdx = worldIdx;
			activeWorldChangedData.first = activeWorldIdx;
			activeWorldChangedData.second = num;
			Game.Instance.Trigger(1983128072, (object)activeWorldChangedData);
			UpdateRocketInteriorAudio();
		}
	}

	public void TimelapseModeOverrideActiveWorld(int overrideValue)
	{
		activeWorldIdx = overrideValue;
	}

	public WorldContainer GetWorld(int id)
	{
		for (int i = 0; i < m_worldContainers.Count; i++)
		{
			if (m_worldContainers[i].id == id)
			{
				return m_worldContainers[i];
			}
		}
		return null;
	}

	public WorldContainer GetWorldFromPosition(Vector3 position)
	{
		foreach (WorldContainer worldContainer in m_worldContainers)
		{
			if (worldContainer.ContainsPoint(position))
			{
				return worldContainer;
			}
		}
		return null;
	}

	public float CountAllRations()
	{
		float result = 0f;
		foreach (WorldContainer worldContainer in m_worldContainers)
		{
			WorldResourceAmountTracker<RationTracker>.Get().CountAmount(null, worldContainer.worldInventory);
		}
		return result;
	}

	public Dictionary<Tag, float> GetAllWorldsAccessibleAmounts()
	{
		Dictionary<Tag, float> dictionary = new Dictionary<Tag, float>();
		foreach (WorldContainer worldContainer in m_worldContainers)
		{
			foreach (KeyValuePair<Tag, float> accessibleAmount in worldContainer.worldInventory.GetAccessibleAmounts())
			{
				if (dictionary.ContainsKey(accessibleAmount.Key))
				{
					dictionary[accessibleAmount.Key] += accessibleAmount.Value;
				}
				else
				{
					dictionary.Add(accessibleAmount.Key, accessibleAmount.Value);
				}
			}
		}
		return dictionary;
	}

	public void MigrateMinion(MinionIdentity minion, int targetID)
	{
		MigrateMinion(minion, targetID, minion.GetMyWorldId());
	}

	public void MigrateCritter(GameObject critter, int targetID)
	{
		MigrateCritter(critter, targetID, critter.GetMyWorldId());
	}

	public void MigrateCritter(GameObject critter, int targetID, int prevID)
	{
		critterMigrationEvArg.entity = critter;
		critterMigrationEvArg.prevWorldId = prevID;
		critterMigrationEvArg.targetWorldId = targetID;
		Game.Instance.Trigger(1142724171, (object)critterMigrationEvArg);
	}

	public void MigrateMinion(MinionIdentity minion, int targetID, int prevID)
	{
		if (!Instance.GetWorld(targetID).IsDiscovered)
		{
			Instance.GetWorld(targetID).SetDiscovered();
		}
		if (!Instance.GetWorld(targetID).IsDupeVisited)
		{
			Instance.GetWorld(targetID).SetDupeVisited();
		}
		migrationEvArg.minionId = minion;
		migrationEvArg.prevWorldId = prevID;
		migrationEvArg.targetWorldId = targetID;
		Game.Instance.assignmentManager.RemoveFromWorld(minion, migrationEvArg.prevWorldId);
		Game.Instance.Trigger(586301400, (object)migrationEvArg);
	}

	public int GetLandingBeaconLocation(int worldId)
	{
		foreach (LandingBeacon.Instance landingBeacon in Components.LandingBeacons)
		{
			if (landingBeacon.GetMyWorldId() == worldId && landingBeacon.CanBeTargeted())
			{
				return Grid.PosToCell(landingBeacon);
			}
		}
		return Grid.InvalidCell;
	}

	public int GetRandomClearCell(int worldId)
	{
		bool flag = false;
		int num = 0;
		while (!flag && num < 1000)
		{
			num++;
			int num2 = UnityEngine.Random.Range(0, Grid.CellCount);
			if (!Grid.Solid[num2] && !Grid.IsLiquid(num2) && Grid.WorldIdx[num2] == worldId)
			{
				return num2;
			}
		}
		num = 0;
		while (!flag && num < 1000)
		{
			num++;
			int num3 = UnityEngine.Random.Range(0, Grid.CellCount);
			if (!Grid.Solid[num3] && Grid.WorldIdx[num3] == worldId)
			{
				return num3;
			}
		}
		return Grid.InvalidCell;
	}

	private bool NotObstructedCell(int x, int y)
	{
		int cell = Grid.XYToCell(x, y);
		return Grid.IsValidCell(cell) && Grid.Objects[cell, 1] == null;
	}

	private int LowestYThatSeesSky(int topCellYPos, int x)
	{
		int num = topCellYPos;
		while (!ValidSurfaceCell(x, num))
		{
			num--;
		}
		return num;
	}

	private bool ValidSurfaceCell(int x, int y)
	{
		int i = Grid.XYToCell(x, y - 1);
		return Grid.Solid[i] || Grid.Foundation[i];
	}

	public int GetRandomSurfaceCell(int worldID, int width = 1, bool excludeTopBorderHeight = true)
	{
		WorldContainer worldContainer = m_worldContainers.Find((WorldContainer match) => match.id == worldID);
		int num = Mathf.RoundToInt(UnityEngine.Random.Range(worldContainer.minimumBounds.x + (float)(worldContainer.Width / 10), worldContainer.maximumBounds.x - (float)(worldContainer.Width / 10)));
		int num2 = Mathf.RoundToInt(worldContainer.maximumBounds.y);
		if (excludeTopBorderHeight)
		{
			num2 -= Grid.TopBorderHeight;
		}
		int num3 = num;
		int num4 = LowestYThatSeesSky(num2, num3);
		int num5 = 0;
		num5 = (NotObstructedCell(num3, num4) ? 1 : 0);
		while (num3 + 1 != num && num5 < width)
		{
			num3++;
			if ((float)num3 > worldContainer.maximumBounds.x)
			{
				num5 = 0;
				num3 = (int)worldContainer.minimumBounds.x;
			}
			int num6 = LowestYThatSeesSky(num2, num3);
			bool flag = NotObstructedCell(num3, num6);
			num5 = ((!(num6 == num4 && flag)) ? (flag ? 1 : 0) : (num5 + 1));
			num4 = num6;
		}
		if (num5 < width)
		{
			return -1;
		}
		return Grid.XYToCell(num3, num4);
	}

	public bool IsPositionInActiveWorld(Vector3 pos)
	{
		if (activeWorld != null && !CameraController.Instance.ignoreClusterFX)
		{
			Vector2 vector = activeWorld.maximumBounds * Grid.CellSizeInMeters + new Vector2(1f, 1f);
			Vector2 vector2 = activeWorld.minimumBounds * Grid.CellSizeInMeters;
			if (pos.x < vector2.x || pos.x > vector.x || pos.y < vector2.y || pos.y > vector.y)
			{
				return false;
			}
		}
		return true;
	}

	public WorldContainer CreateRocketInteriorWorld(GameObject craft_go, string interiorTemplateName, System.Action callback)
	{
		Vector2I rOCKET_INTERIOR_SIZE = ROCKETRY.ROCKET_INTERIOR_SIZE;
		if (Grid.GetFreeGridSpace(rOCKET_INTERIOR_SIZE, out var offset))
		{
			int nextWorldId = GetNextWorldId();
			craft_go.AddComponent<WorldInventory>();
			WorldContainer worldContainer = craft_go.AddComponent<WorldContainer>();
			worldContainer.SetRocketInteriorWorldDetails(nextWorldId, rOCKET_INTERIOR_SIZE, offset);
			Vector2I vector2I = offset + rOCKET_INTERIOR_SIZE;
			for (int i = offset.y; i < vector2I.y; i++)
			{
				for (int j = offset.x; j < vector2I.x; j++)
				{
					int num = Grid.XYToCell(j, i);
					Grid.WorldIdx[num] = (byte)nextWorldId;
					Pathfinding.Instance.AddDirtyNavGridCell(num);
				}
			}
			Debug.Log($"Created new rocket interior id: {nextWorldId}, at {offset} with size {rOCKET_INTERIOR_SIZE}");
			worldContainer.PlaceInteriorTemplate(interiorTemplateName, delegate
			{
				if (callback != null)
				{
					callback();
				}
				craft_go.GetComponent<CraftModuleInterface>().TriggerEventOnCraftAndRocket(GameHashes.RocketInteriorComplete, null);
			});
			OrbitalMechanics orbitalMechanics = craft_go.AddOrGet<OrbitalMechanics>();
			orbitalMechanics.CreateOrbitalObject(Db.Get().OrbitalTypeCategories.landed.Id);
			BoxingTrigger(-1280433810, worldContainer.id);
			return worldContainer;
		}
		Debug.LogError("Failed to create rocket interior.");
		return null;
	}

	public void DestoryRocketInteriorWorld(int world_id, ClustercraftExteriorDoor door)
	{
		WorldContainer world = GetWorld(world_id);
		if (world == null || !world.IsModuleInterior)
		{
			Debug.LogError($"Attempting to destroy world id {world_id}. The world is not a valid rocket interior");
			return;
		}
		GameObject gameObject = door.GetComponent<RocketModuleCluster>().CraftInterface.gameObject;
		if (activeWorldId == world_id)
		{
			int parentWorldId = gameObject.GetComponent<WorldContainer>().ParentWorldId;
			if (parentWorldId == world_id)
			{
				SetActiveWorld(Instance.GetStartWorld().id);
			}
			else
			{
				SetActiveWorld(gameObject.GetComponent<WorldContainer>().ParentWorldId);
			}
		}
		OrbitalMechanics component = gameObject.GetComponent<OrbitalMechanics>();
		if (!component.IsNullOrDestroyed())
		{
			UnityEngine.Object.Destroy(component);
		}
		bool flag = gameObject.GetComponent<Clustercraft>().Status == Clustercraft.CraftStatus.InFlight;
		PrimaryElement moduleElemet = door.GetComponent<PrimaryElement>();
		AxialI clusterLocation = world.GetComponent<ClusterGridEntity>().Location;
		Vector3 rocketModuleWorldPos = door.transform.position;
		if (!flag)
		{
			world.EjectAllDupes(rocketModuleWorldPos);
		}
		else
		{
			world.SpacePodAllDupes(clusterLocation, moduleElemet.ElementID);
		}
		world.CancelChores();
		world.DestroyWorldBuildings(out var noRefundTiles);
		UnregisterWorldContainer(world);
		if (!flag)
		{
			GameScheduler.Instance.ScheduleNextFrame("ClusterManager.world.TransferResourcesToParentWorld", delegate
			{
				world.TransferResourcesToParentWorld(rocketModuleWorldPos + new Vector3(0f, 0.5f, 0f), noRefundTiles);
			});
			GameScheduler.Instance.ScheduleNextFrame("ClusterManager.DeleteWorldObjects", delegate
			{
				DeleteWorldObjects(world);
			});
		}
		else
		{
			GameScheduler.Instance.ScheduleNextFrame("ClusterManager.world.TransferResourcesToDebris", delegate
			{
				world.TransferResourcesToDebris(clusterLocation, noRefundTiles, moduleElemet.ElementID);
			});
			GameScheduler.Instance.ScheduleNextFrame("ClusterManager.DeleteWorldObjects", delegate
			{
				DeleteWorldObjects(world);
			});
		}
	}

	public void UpdateWorldReverbSnapshot(int worldId)
	{
		if (DlcManager.FeatureClusterSpaceEnabled())
		{
			AudioMixer.instance.Stop(AudioMixerSnapshots.Get().SmallRocketInteriorReverbSnapshot);
			AudioMixer.instance.Stop(AudioMixerSnapshots.Get().MediumRocketInteriorReverbSnapshot);
		}
		AudioMixer.instance.PauseSpaceVisibleSnapshot(pause: false);
		WorldContainer world = GetWorld(worldId);
		if (world.IsModuleInterior)
		{
			Clustercraft component = world.GetComponent<Clustercraft>();
			PassengerRocketModule passengerModule = component.ModuleInterface.GetPassengerModule();
			AudioMixer.instance.Start(passengerModule.interiorReverbSnapshot);
			AudioMixer.instance.PauseSpaceVisibleSnapshot(pause: true);
			UpdateRocketInteriorAudio();
		}
	}

	public void UpdateRocketInteriorAudio()
	{
		WorldContainer worldContainer = activeWorld;
		if (worldContainer != null && worldContainer.IsModuleInterior)
		{
			Vector3 vector = worldContainer.minimumBounds + new Vector2((float)worldContainer.Width * Grid.CellSizeInMeters, (float)worldContainer.Height * Grid.CellSizeInMeters) / 2f;
			Clustercraft component = worldContainer.GetComponent<Clustercraft>();
			RocketStatesForAudio rocketInteriorState = RocketStatesForAudio.Grounded;
			switch (component.Status)
			{
			case Clustercraft.CraftStatus.Grounded:
				rocketInteriorState = (component.LaunchRequested ? RocketStatesForAudio.ReadyForLaunch : RocketStatesForAudio.Grounded);
				break;
			case Clustercraft.CraftStatus.Launching:
				rocketInteriorState = RocketStatesForAudio.Launching;
				break;
			case Clustercraft.CraftStatus.InFlight:
				rocketInteriorState = RocketStatesForAudio.InSpace;
				break;
			case Clustercraft.CraftStatus.Landing:
				rocketInteriorState = RocketStatesForAudio.Landing;
				break;
			}
			RocketInteriorState = rocketInteriorState;
		}
	}

	private void DeleteWorldObjects(WorldContainer world)
	{
		Grid.FreeGridSpace(world.WorldSize, world.WorldOffset);
		WorldInventory worldInventory = null;
		if (world != null)
		{
			worldInventory = world.GetComponent<WorldInventory>();
		}
		if (worldInventory != null)
		{
			UnityEngine.Object.Destroy(worldInventory);
		}
		if (world != null)
		{
			UnityEngine.Object.Destroy(world);
		}
	}
}
