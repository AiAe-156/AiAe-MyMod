using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using STRINGS;
using UnityEngine;

public class Navigator : StateMachineComponent<Navigator.StatesInstance>, ISaveLoadableDetails
{
	public class ActiveTransition
	{
		public int x;

		public int y;

		public bool isLooping;

		public NavType start;

		public NavType end;

		public HashedString preAnim;

		public HashedString anim;

		public float speed;

		public float animSpeed = 1f;

		public NavGrid.Transition navGridTransition;

		public void Init(NavGrid.Transition transition, float default_speed)
		{
			x = transition.x;
			y = transition.y;
			isLooping = transition.isLooping;
			start = transition.start;
			end = transition.end;
			preAnim = transition.preAnim;
			anim = transition.anim;
			speed = default_speed;
			animSpeed = transition.animSpeed;
			navGridTransition = transition;
		}

		public void Copy(ActiveTransition other)
		{
			x = other.x;
			y = other.y;
			isLooping = other.isLooping;
			start = other.start;
			end = other.end;
			preAnim = other.preAnim;
			anim = other.anim;
			speed = other.speed;
			animSpeed = other.animSpeed;
			navGridTransition = other.navGridTransition;
		}
	}

	public class StatesInstance : GameStateMachine<States, StatesInstance, Navigator, object>.GameInstance
	{
		public StatesInstance(Navigator master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, Navigator>
	{
		public class NormalStates : State
		{
			public State moving;

			public State arrived;

			public State failed;

			public State stopped;
		}

		public TargetParameter moveTarget;

		public BoolParameter isPaused = new BoolParameter(default_value: false);

		public NormalStates normal;

		public State paused;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = normal.stopped;
			saveHistory = true;
			normal.ParamTransition(isPaused, paused, GameStateMachine<States, StatesInstance, Navigator, object>.IsTrue);
			normal.moving.Enter(delegate(StatesInstance smi)
			{
				smi.BoxingTrigger(1027377649, GameHashes.ObjectMovementWakeUp);
			}).Update("UpdateNavigator", delegate(StatesInstance smi, float dt)
			{
				smi.master.SimEveryTick(dt);
			}, UpdateRate.SIM_EVERY_TICK, load_balance: true).Exit(delegate(StatesInstance smi)
			{
				smi.BoxingTrigger(1027377649, GameHashes.ObjectMovementSleep);
			});
			normal.arrived.TriggerOnEnter(GameHashes.DestinationReached).GoTo(normal.stopped);
			normal.failed.TriggerOnEnter(GameHashes.NavigationFailed).GoTo(normal.stopped);
			normal.stopped.Enter(delegate(StatesInstance smi)
			{
				smi.master.SubscribeUnstuckFunctions();
			}).DoNothing().Exit(delegate(StatesInstance smi)
			{
				smi.master.UnsubscribeUnstuckFunctions();
			});
			paused.ParamTransition(isPaused, normal, GameStateMachine<States, StatesInstance, Navigator, object>.IsFalse);
		}
	}

	public class Scanner<T> where T : KMonoBehaviour
	{
		private static readonly CellOffset[] NO_OFFSETS = new CellOffset[1]
		{
			new CellOffset(0, 0)
		};

		private readonly int radius;

		private readonly ScenePartitionerLayer layer;

		private readonly Func<T, bool> filterFn;

		private CellOffset[] offsets;

		private Action<T, List<CellOffset>> offsetsFn;

		private int? early_out_threshold;

		public Scanner(int radius, ScenePartitionerLayer layer, Func<T, bool> filterFn)
		{
			this.radius = radius;
			this.layer = layer;
			this.filterFn = filterFn;
			offsets = NO_OFFSETS;
			offsetsFn = null;
			early_out_threshold = null;
		}

		public void SetConstantOffsets(CellOffset[] offsets)
		{
			this.offsets = offsets;
		}

		public void SetDynamicOffsetsFn(Action<T, List<CellOffset>> offsetsFn)
		{
			this.offsetsFn = offsetsFn;
		}

		public void SetEarlyOutThreshold(int early_out_threshold)
		{
			this.early_out_threshold = early_out_threshold;
		}

		private int NavCostFromConstantOffsets(Navigator navigator, T destinationObject, CellOffset[] offsets)
		{
			return navigator.GetNavigationCost(Grid.PosToCell(destinationObject.gameObject), offsets);
		}

		private int NavCostFromDynamicOffsets(Navigator navigator, T destinationObject, Action<T, List<CellOffset>> offsetsFn)
		{
			ListPool<CellOffset, Navigator>.PooledList pooledList = ListPool<CellOffset, Navigator>.Allocate();
			offsetsFn(destinationObject, pooledList);
			int navigationCost = navigator.GetNavigationCost(Grid.PosToCell(destinationObject.gameObject), pooledList);
			pooledList.Recycle();
			return navigationCost;
		}

		public T Scan(Vector2I gridPos, Navigator navigator)
		{
			ListPool<ScenePartitionerEntry, Navigator>.PooledList pooledList = ListPool<ScenePartitionerEntry, Navigator>.Allocate();
			GameScenePartitioner.Instance.GatherEntries(gridPos.x - radius, gridPos.y - radius, radius * 2, radius * 2, layer, pooledList);
			T val = null;
			int num = -1;
			if (early_out_threshold.HasValue)
			{
				pooledList.Shuffle();
				if (offsetsFn != null)
				{
					for (int i = 0; i < pooledList.Count; i++)
					{
						T val2 = pooledList[i].obj as T;
						if (!filterFn(val2))
						{
							continue;
						}
						int num2 = NavCostFromDynamicOffsets(navigator, val2, offsetsFn);
						if (num2 != -1 && (val == null || num2 < num))
						{
							val = val2;
							num = num2;
							if (num2 <= early_out_threshold.Value)
							{
								break;
							}
						}
					}
				}
				else
				{
					for (int j = 0; j < pooledList.Count; j++)
					{
						T val3 = pooledList[j].obj as T;
						if (!filterFn(val3))
						{
							continue;
						}
						int num3 = NavCostFromConstantOffsets(navigator, val3, offsets);
						if (num3 != -1 && (val == null || num3 < num))
						{
							val = val3;
							num = num3;
							if (num3 <= early_out_threshold.Value)
							{
								break;
							}
						}
					}
				}
			}
			else if (offsetsFn != null)
			{
				for (int k = 0; k < pooledList.Count; k++)
				{
					T val4 = pooledList[k].obj as T;
					if (filterFn(val4))
					{
						int num4 = NavCostFromDynamicOffsets(navigator, val4, offsetsFn);
						if (num4 != -1 && (val == null || num4 < num))
						{
							val = val4;
							num = num4;
						}
					}
				}
			}
			else
			{
				for (int l = 0; l < pooledList.Count; l++)
				{
					T val5 = pooledList[l].obj as T;
					if (filterFn(val5))
					{
						int num5 = NavCostFromConstantOffsets(navigator, val5, offsets);
						if (num5 != -1 && (val == null || num5 < num))
						{
							val = val5;
							num = num5;
						}
					}
				}
			}
			pooledList.Recycle();
			return val;
		}
	}

	public bool DebugDrawPath;

	[MyCmpAdd]
	public Facing facing;

	public float defaultSpeed = 1f;

	public TransitionDriver transitionDriver;

	public string NavGridName;

	public bool updateProber;

	public int maxProbeRadiusX;

	public int maxProbeRadiusY;

	public PathFinder.PotentialPath.Flags flags;

	private LoggerFSS log;

	public Dictionary<NavType, int> distanceTravelledByNavType;

	public Grid.SceneLayer sceneLayer = Grid.SceneLayer.Move;

	private PathFinderAbilities abilities;

	[MyCmpReq]
	public KBatchedAnimController animController;

	[NonSerialized]
	public PathFinder.Path path;

	public NavType CurrentNavType;

	private int AnchorCell;

	private KPrefabID targetLocator;

	public int cachedCell;

	private int reservedCell = NavigationReservations.InvalidReservation;

	private NavTactic tactic;

	public bool reportOccupation;

	public List<int> occupiedCells = new List<int>();

	private Action<int, object> OnBuildingTileChangedAction;

	private static readonly EventSystem.IntraObjectHandler<Navigator> OnDefeatedDelegate = new EventSystem.IntraObjectHandler<Navigator>(delegate(Navigator component, object data)
	{
		component.OnDefeated(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Navigator> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<Navigator>(delegate(Navigator component, object data)
	{
		component.OnRefreshUserMenu(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Navigator> OnSelectObjectDelegate = new EventSystem.IntraObjectHandler<Navigator>(delegate(Navigator component, object data)
	{
		component.OnSelectObject(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Navigator> OnStoreDelegate = new EventSystem.IntraObjectHandler<Navigator>(delegate(Navigator component, object data)
	{
		component.OnStore(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Navigator> OnQueueDestroyDelegate = new EventSystem.IntraObjectHandler<Navigator>(delegate(Navigator component, object data)
	{
		component.OnQueueDestroy();
	});

	public bool executePathProbeTaskAsync;

	public KMonoBehaviour target { get; set; }

	public CellOffset[] targetOffsets { get; private set; }

	public NavGrid NavGrid { get; private set; }

	public PathGrid PathGrid { get; set; }

	public void Serialize(BinaryWriter writer)
	{
		byte currentNavType = (byte)CurrentNavType;
		writer.Write(currentNavType);
		writer.Write(distanceTravelledByNavType.Count);
		foreach (KeyValuePair<NavType, int> item in distanceTravelledByNavType)
		{
			byte key = (byte)item.Key;
			writer.Write(key);
			writer.Write(item.Value);
		}
	}

	public void Deserialize(IReader reader)
	{
		NavType navType = (NavType)reader.ReadByte();
		if (!SaveLoader.Instance.GameInfo.IsVersionOlderThan(7, 11))
		{
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				NavType key = (NavType)reader.ReadByte();
				int value = reader.ReadInt32();
				if (distanceTravelledByNavType.ContainsKey(key))
				{
					distanceTravelledByNavType[key] = value;
				}
			}
		}
		bool flag = false;
		NavType[] validNavTypes = NavGrid.ValidNavTypes;
		for (int j = 0; j < validNavTypes.Length; j++)
		{
			if (validNavTypes[j] == navType)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			CurrentNavType = navType;
		}
	}

	protected override void OnPrefabInit()
	{
		transitionDriver = new TransitionDriver(this);
		targetLocator = Util.KInstantiate(Assets.GetPrefab(TargetLocator.ID)).GetComponent<KPrefabID>();
		targetLocator.gameObject.SetActive(value: true);
		log = new LoggerFSS("Navigator");
		simRenderLoadBalance = true;
		autoRegisterSimRender = false;
		NavGrid = Pathfinding.Instance.GetNavGrid(NavGridName);
		if (maxProbeRadiusX != 0 || maxProbeRadiusY != 0)
		{
			PathGrid = new PathGrid(maxProbeRadiusX * 2 + 1, maxProbeRadiusY * 2 + 1, apply_offset: true, NavGrid.ValidNavTypes);
		}
		else
		{
			PathGrid = new PathGrid(Grid.WidthInCells, Grid.HeightInCells, apply_offset: false, NavGrid.ValidNavTypes);
		}
		distanceTravelledByNavType = new Dictionary<NavType, int>();
		for (int i = 0; i < 11; i++)
		{
			distanceTravelledByNavType.Add((NavType)i, 0);
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Subscribe(1623392196, OnDefeatedDelegate);
		Subscribe(-1506500077, OnDefeatedDelegate);
		Subscribe(493375141, OnRefreshUserMenuDelegate);
		Subscribe(-1503271301, OnSelectObjectDelegate);
		Subscribe(856640610, OnStoreDelegate);
		Subscribe(1502190696, OnQueueDestroyDelegate);
		if (updateProber)
		{
			SimAndRenderScheduler.instance.Add(this);
		}
		if (executePathProbeTaskAsync)
		{
			AsyncPathProber.Instance.Register(this);
		}
		cachedCell = Grid.PosToCell(this);
		SetCurrentNavType(CurrentNavType);
		OnBuildingTileChangedAction = OnBuildingTileChanged;
		SubscribeUnstuckFunctions();
	}

	private void SubscribeUnstuckFunctions()
	{
		if (CurrentNavType == NavType.Tube)
		{
			GameScenePartitioner.Instance.AddGlobalLayerListener(GameScenePartitioner.Instance.objectLayers[1], OnBuildingTileChangedAction);
		}
	}

	private void UnsubscribeUnstuckFunctions()
	{
		GameScenePartitioner.Instance.RemoveGlobalLayerListener(GameScenePartitioner.Instance.objectLayers[1], OnBuildingTileChangedAction);
	}

	private void OnBuildingTileChanged(int cell, object building)
	{
		if (CurrentNavType == NavType.Tube && building == null)
		{
			bool flag = cell == Grid.PosToCell(this);
			if (base.smi != null && flag)
			{
				SetCurrentNavType(NavType.Floor);
				UnsubscribeUnstuckFunctions();
			}
		}
	}

	protected override void OnCleanUp()
	{
		UnsubscribeUnstuckFunctions();
		base.OnCleanUp();
	}

	protected void OnQueueDestroy()
	{
		if (executePathProbeTaskAsync)
		{
			AsyncPathProber.Instance.Unregister(this);
		}
		if (reportOccupation)
		{
			MinionGroupProber.Get().Vacate(occupiedCells);
		}
	}

	public PathGrid TakeResult(ref AsyncPathProber.WorkResult result)
	{
		PathGrid pathGrid = PathGrid;
		PathGrid = result.pathGrid;
		if (reportOccupation)
		{
			List<int> reachableCells = occupiedCells;
			MinionGroupProber.Get().OccupyST(result.newlyReachableCells);
			MinionGroupProber.Get().VacateST(result.noLongerReachableCells);
			occupiedCells = result.reachableCells;
			result.reachableCells = reachableCells;
		}
		return pathGrid;
	}

	public bool IsMoving()
	{
		return base.smi.IsInsideState(base.smi.sm.normal.moving);
	}

	public bool IsSwimming()
	{
		return CurrentNavType == NavType.Swim;
	}

	public bool GoTo(int cell, CellOffset[] offsets = null)
	{
		if (offsets == null)
		{
			offsets = new CellOffset[1];
		}
		targetLocator.transform.SetPosition(Grid.CellToPosCBC(cell, Grid.SceneLayer.Move));
		return GoTo(targetLocator, offsets, NavigationTactics.ReduceTravelDistance);
	}

	public bool GoTo(int cell, CellOffset[] offsets, NavTactic tactic)
	{
		if (offsets == null)
		{
			offsets = new CellOffset[1];
		}
		targetLocator.transform.SetPosition(Grid.CellToPosCBC(cell, Grid.SceneLayer.Move));
		return GoTo(targetLocator, offsets, tactic);
	}

	public void UpdateTarget(int cell)
	{
		targetLocator.transform.SetPosition(Grid.CellToPosCBC(cell, Grid.SceneLayer.Move));
	}

	public bool GoTo(KMonoBehaviour target, CellOffset[] offsets, NavTactic tactic)
	{
		if (tactic == null)
		{
			tactic = NavigationTactics.ReduceTravelDistance;
		}
		base.smi.GoTo(base.smi.sm.normal.moving);
		base.smi.sm.moveTarget.Set(target.gameObject, base.smi);
		this.tactic = tactic;
		this.target = target;
		targetOffsets = offsets;
		ClearReservedCell();
		AdvancePath();
		return IsMoving();
	}

	public void BeginTransition(NavGrid.Transition transition)
	{
		transitionDriver.EndTransition();
		base.smi.GoTo(base.smi.sm.normal.moving);
		transitionDriver.BeginTransition(this, transition, defaultSpeed);
	}

	private bool ValidatePath(ref PathFinder.Path path, out bool atNextNode)
	{
		atNextNode = false;
		bool flag = false;
		if (path.IsValid())
		{
			int target_cell = Grid.PosToCell(target);
			flag = reservedCell != NavigationReservations.InvalidReservation && CanReach(reservedCell);
			flag &= Grid.IsCellOffsetOf(reservedCell, target_cell, targetOffsets);
		}
		if (flag)
		{
			int num = Grid.PosToCell(this);
			flag = num == path.nodes[0].cell && CurrentNavType == path.nodes[0].navType;
			flag |= (atNextNode = num == path.nodes[1].cell && CurrentNavType == path.nodes[1].navType);
		}
		if (!flag)
		{
			return false;
		}
		PathFinderAbilities currentAbilities = GetCurrentAbilities();
		return PathFinder.ValidatePath(NavGrid, currentAbilities, ref path, flags);
	}

	private bool TryBuildPathFromCache(int cachedCell, int reservedCell, ref PathFinder.Path path)
	{
		bool atNextNode;
		if (PathGrid.BuildPath(cachedCell, reservedCell, CurrentNavType, ref path))
		{
			return ValidatePath(ref path, out atNextNode);
		}
		return false;
	}

	public void AdvancePath(bool trigger_advance = true)
	{
		cachedCell = Grid.PosToCell(this);
		if (target == null)
		{
			if (!Stop())
			{
				Trigger(-766531887);
			}
		}
		else if (cachedCell == reservedCell && CurrentNavType != NavType.Tube)
		{
			Stop(arrived_at_destination: true);
		}
		else
		{
			if (!ValidatePath(ref path, out var atNextNode))
			{
				int root = Grid.PosToCell(target);
				int cellPreferences = tactic.GetCellPreferences(root, targetOffsets, this);
				SetReservedCell(cellPreferences);
				if (reservedCell == NavigationReservations.InvalidReservation)
				{
					path.Clear();
				}
				else if (!TryBuildPathFromCache(cachedCell, reservedCell, ref path))
				{
					PathFinder.UpdatePath(potential_path: new PathFinder.PotentialPath(cachedCell, CurrentNavType, flags), nav_grid: NavGrid, abilities: GetCurrentAbilities(), query: PathFinderQueries.cellQuery.Reset(reservedCell), path: ref path);
					if (executePathProbeTaskAsync)
					{
						AsyncPathProber.Instance.ApplyNavigationFailedPenalty(this);
					}
				}
			}
			else if (atNextNode)
			{
				path.nodes.RemoveAt(0);
			}
			if (path.IsValid())
			{
				BeginTransition(NavGrid.transitions[path.nodes[1].transitionId]);
				distanceTravelledByNavType[CurrentNavType] = Mathf.Max(distanceTravelledByNavType[CurrentNavType] + 1, distanceTravelledByNavType[CurrentNavType]);
			}
			else if (path.HasArrived())
			{
				Stop(arrived_at_destination: true);
			}
			else
			{
				ClearReservedCell();
				Stop();
			}
		}
		if (trigger_advance)
		{
			Trigger(1347184327);
		}
	}

	public NavGrid.Transition GetNextTransition()
	{
		return NavGrid.transitions[path.nodes[1].transitionId];
	}

	public bool Stop(bool arrived_at_destination = false, bool play_idle = true)
	{
		target = null;
		targetOffsets = null;
		path.Clear();
		base.smi.sm.moveTarget.Set(null, base.smi);
		transitionDriver.EndTransition();
		if (play_idle)
		{
			HashedString idleAnim = NavGrid.GetIdleAnim(CurrentNavType);
			animController.Play(idleAnim, KAnim.PlayMode.Loop);
		}
		if (arrived_at_destination)
		{
			base.smi.GoTo(base.smi.sm.normal.arrived);
			return true;
		}
		if (base.smi.GetCurrentState() == base.smi.sm.normal.moving)
		{
			ClearReservedCell();
			base.smi.GoTo(base.smi.sm.normal.failed);
			return true;
		}
		return false;
	}

	private void SimEveryTick(float dt)
	{
		if (IsMoving())
		{
			transitionDriver.UpdateTransition(dt);
		}
	}

	public void UpdateProbe(bool forceUpdate = false)
	{
		if (forceUpdate || !executePathProbeTaskAsync)
		{
			if (reportOccupation)
			{
				ListPool<int, Navigator>.PooledList pooledList = ListPool<int, Navigator>.Allocate();
				PathProber.Run(this, pooledList);
				MinionGroupProber.Get().Occupy(pooledList);
				MinionGroupProber.Get().Vacate(occupiedCells);
				occupiedCells.Clear();
				occupiedCells.AddRange(pooledList);
				pooledList.Recycle();
			}
			else
			{
				PathProber.Run(this);
			}
		}
	}

	public void DrawPath()
	{
		if (base.gameObject.activeInHierarchy && IsMoving())
		{
			NavPathDrawer.Instance.DrawPath(animController.GetPivotSymbolPosition(), path);
		}
	}

	public void Pause(string reason)
	{
		base.smi.sm.isPaused.Set(value: true, base.smi);
	}

	public void Unpause(string reason)
	{
		base.smi.sm.isPaused.Set(value: false, base.smi);
	}

	private void OnDefeated(object data)
	{
		ClearReservedCell();
		Stop(arrived_at_destination: false, play_idle: false);
	}

	private void ClearReservedCell()
	{
		if (reservedCell != NavigationReservations.InvalidReservation)
		{
			NavigationReservations.Instance.RemoveOccupancy(reservedCell);
			reservedCell = NavigationReservations.InvalidReservation;
		}
	}

	private void SetReservedCell(int cell)
	{
		ClearReservedCell();
		reservedCell = cell;
		NavigationReservations.Instance.AddOccupancy(cell);
	}

	public int GetReservedCell()
	{
		return reservedCell;
	}

	public int GetAnchorCell()
	{
		return AnchorCell;
	}

	public bool IsValidNavType(NavType nav_type)
	{
		return NavGrid.HasNavTypeData(nav_type);
	}

	public void SetCurrentNavType(NavType nav_type)
	{
		CurrentNavType = nav_type;
		AnchorCell = NavTypeHelper.GetAnchorCell(nav_type, Grid.PosToCell(this));
		NavGrid.NavTypeData navTypeData = NavGrid.GetNavTypeData(CurrentNavType);
		KBatchedAnimController component = GetComponent<KBatchedAnimController>();
		Vector2 one = Vector2.one;
		if (navTypeData.flipX)
		{
			one.x = -1f;
		}
		if (navTypeData.flipY)
		{
			one.y = -1f;
		}
		component.navMatrix = Matrix2x3.Translate(navTypeData.animControllerOffset * 200f) * Matrix2x3.Rotate(navTypeData.rotation) * Matrix2x3.Scale(one);
	}

	private void OnRefreshUserMenu(object data)
	{
		if (!base.gameObject.HasTag(GameTags.Dead))
		{
			KIconButtonMenu.ButtonInfo button = ((NavPathDrawer.Instance.GetNavigator() != this) ? new KIconButtonMenu.ButtonInfo("action_navigable_regions", UI.USERMENUACTIONS.DRAWPATHS.NAME, OnDrawPaths, Action.NumActions, null, null, null, UI.USERMENUACTIONS.DRAWPATHS.TOOLTIP) : new KIconButtonMenu.ButtonInfo("action_navigable_regions", UI.USERMENUACTIONS.DRAWPATHS.NAME_OFF, OnDrawPaths, Action.NumActions, null, null, null, UI.USERMENUACTIONS.DRAWPATHS.TOOLTIP_OFF));
			Game.Instance.userMenu.AddButton(base.gameObject, button, 0.1f);
			Game.Instance.userMenu.AddButton(base.gameObject, new KIconButtonMenu.ButtonInfo("action_follow_cam", UI.USERMENUACTIONS.FOLLOWCAM.NAME, OnFollowCam, Action.NumActions, null, null, null, UI.USERMENUACTIONS.FOLLOWCAM.TOOLTIP), 0.3f);
		}
	}

	private void OnFollowCam()
	{
		if (CameraController.Instance.followTarget == base.transform)
		{
			CameraController.Instance.ClearFollowTarget();
		}
		else
		{
			CameraController.Instance.SetFollowTarget(base.transform);
		}
	}

	private void OnDrawPaths()
	{
		if (NavPathDrawer.Instance.GetNavigator() != this)
		{
			NavPathDrawer.Instance.SetNavigator(this);
		}
		else
		{
			NavPathDrawer.Instance.ClearNavigator();
		}
	}

	private void OnSelectObject(object data)
	{
		NavPathDrawer.Instance.ClearNavigator();
	}

	public void OnStore(object data)
	{
		if (data is Storage || (data != null && ((Boxed<bool>)data).value))
		{
			Stop();
		}
	}

	public PathFinderAbilities GetCurrentAbilities()
	{
		abilities.Refresh();
		return abilities;
	}

	public void SetAbilities(PathFinderAbilities abilities)
	{
		this.abilities = abilities;
	}

	public bool CanReach(IApproachable approachable)
	{
		return CanReach(approachable.GetCell(), approachable.GetOffsets());
	}

	public bool CanReach(int cell, CellOffset[] offsets)
	{
		foreach (CellOffset offset in offsets)
		{
			int cell2 = Grid.OffsetCell(cell, offset);
			if (CanReach(cell2))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanReach(int cell)
	{
		return GetNavigationCost(cell) != -1;
	}

	public int GetNavigationCost(int cell)
	{
		if (Grid.IsValidCell(cell))
		{
			return PathGrid.GetCost(cell);
		}
		return -1;
	}

	public int GetNavigationCostIgnoreProberOffset(int cell, CellOffset[] offsets)
	{
		return PathGrid.GetCostIgnoreProberOffset(cell, offsets);
	}

	public int GetNavigationCost(int cell, CellOffset[] offsets)
	{
		int num = -1;
		int num2 = offsets.Length;
		for (int i = 0; i < num2; i++)
		{
			int cell2 = Grid.OffsetCell(cell, offsets[i]);
			int navigationCost = GetNavigationCost(cell2);
			if (navigationCost != -1 && (num == -1 || navigationCost < num))
			{
				num = navigationCost;
			}
		}
		return num;
	}

	public int GetNavigationCost(int cell, IReadOnlyList<CellOffset> offsets)
	{
		int num = -1;
		int count = offsets.Count;
		for (int i = 0; i < count; i++)
		{
			int cell2 = Grid.OffsetCell(cell, offsets[i]);
			int navigationCost = GetNavigationCost(cell2);
			if (navigationCost != -1 && (num == -1 || navigationCost < num))
			{
				num = navigationCost;
			}
		}
		return num;
	}

	public int GetNavigationCost(IApproachable approachable)
	{
		return GetNavigationCost(approachable.GetCell(), approachable.GetOffsets());
	}

	public void RunQuery(PathFinderQuery query)
	{
		int cell = Grid.PosToCell(this);
		PathFinder.Run(potential_path: new PathFinder.PotentialPath(cell, CurrentNavType, flags), nav_grid: NavGrid, abilities: GetCurrentAbilities(), query: query);
	}

	public void SetFlags(PathFinder.PotentialPath.Flags new_flags)
	{
		flags |= new_flags;
	}

	public void ClearFlags(PathFinder.PotentialPath.Flags new_flags)
	{
		flags &= (PathFinder.PotentialPath.Flags)(byte)(~(int)new_flags);
	}

	[Conditional("ENABLE_DETAILED_NAVIGATOR_PROFILE_INFO")]
	public static void BeginDetailedSample(string region_name)
	{
	}

	[Conditional("ENABLE_DETAILED_NAVIGATOR_PROFILE_INFO")]
	public static void EndDetailedSample(string region_name)
	{
	}
}
