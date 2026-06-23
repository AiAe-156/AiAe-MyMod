using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using KSerialization;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/Workable/Constructable")]
public class Constructable : Workable, ISaveLoadable
{
	public struct ReplaceCallbackParameters
	{
		public ObjectLayer TileLayer;

		public WorkerBase Worker;
	}

	[MyCmpAdd]
	private Storage storage;

	[MyCmpAdd]
	private Notifier notifier;

	[MyCmpAdd]
	private Prioritizable prioritizable;

	[MyCmpReq]
	private Building building;

	[MyCmpGet]
	private Rotatable rotatable;

	private Notification invalidLocation;

	private float initialTemperature = -1f;

	[Serialize]
	private bool isPrioritized;

	private FetchList2 fetchList;

	private Chore buildChore;

	private bool materialNeedsCleared = false;

	private bool hasUnreachableDigs;

	private bool finished = false;

	private bool unmarked = false;

	public bool isDiggingRequired = true;

	private bool waitForFetchesBeforeDigging;

	private bool hasLadderNearby;

	private Extents ladderDetectionExtents;

	[Serialize]
	public bool IsReplacementTile = false;

	private HandleVector<int>.Handle solidPartitionerEntry;

	private HandleVector<int>.Handle digPartitionerEntry;

	private HandleVector<int>.Handle backwallPartitionerEntry;

	private HandleVector<int>.Handle ladderPartitionerEntry;

	private readonly HashSet<Uprootable> pendingUproots = new HashSet<Uprootable>();

	private LoggerFSS log = new LoggerFSS("Constructable");

	[Serialize]
	private Tag[] selectedElementsTags;

	private Element[] selectedElements;

	[Serialize]
	private int[] ids;

	private static readonly EventSystem.IntraObjectHandler<Constructable> OnReachableChangedDelegate = new EventSystem.IntraObjectHandler<Constructable>(delegate(Constructable component, object data)
	{
		component.OnReachableChanged(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Constructable> OnCancelDelegate = new EventSystem.IntraObjectHandler<Constructable>(delegate(Constructable component, object data)
	{
		component.OnCancel(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Constructable> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<Constructable>(delegate(Constructable component, object data)
	{
		component.OnRefreshUserMenu(data);
	});

	public Recipe Recipe => building.Def.CraftRecipe;

	public IList<Tag> SelectedElementsTags
	{
		get
		{
			return selectedElementsTags;
		}
		set
		{
			if (selectedElementsTags == null || selectedElementsTags.Length != value.Count)
			{
				selectedElementsTags = new Tag[value.Count];
			}
			value.CopyTo(selectedElementsTags, 0);
		}
	}

	public override string GetConversationTopic()
	{
		return building.Def.PrefabID;
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		float num = 0f;
		float num2 = 0f;
		bool flag = true;
		foreach (GameObject item in storage.items)
		{
			if (!(item == null))
			{
				PrimaryElement component = item.GetComponent<PrimaryElement>();
				if (!(component == null))
				{
					num += component.Mass;
					num2 += component.Temperature * component.Mass;
					flag = flag && component.HasTag(GameTags.Liquifiable);
				}
			}
		}
		if (num <= 0f)
		{
			DebugUtil.LogWarningArgs(base.gameObject, "uhhh this constructable is about to generate a nan", "Item Count: ", storage.items.Count);
			return;
		}
		if (flag)
		{
			initialTemperature = Mathf.Min(num2 / num, 318.15f);
		}
		else
		{
			initialTemperature = Mathf.Clamp(num2 / num, 0f, 318.15f);
		}
		KAnimGraphTileVisualizer component2 = GetComponent<KAnimGraphTileVisualizer>();
		UtilityConnections connections = ((!(component2 == null)) ? component2.Connections : ((UtilityConnections)0));
		bool flag2 = true;
		if (IsReplacementTile)
		{
			int cell = Grid.PosToCell(base.transform.GetLocalPosition());
			GameObject replacementCandidate = building.Def.GetReplacementCandidate(cell);
			if (replacementCandidate != null)
			{
				flag2 = false;
				SimCellOccupier component3 = replacementCandidate.GetComponent<SimCellOccupier>();
				if (component3 != null)
				{
					component3.DestroySelf(delegate
					{
						if (this != null && base.gameObject != null)
						{
							FinishConstruction(connections, worker);
						}
					});
				}
				else
				{
					Conduit component4 = replacementCandidate.GetComponent<Conduit>();
					if (component4 != null)
					{
						ConduitFlow flowManager = component4.GetFlowManager();
						flowManager.MarkForReplacement(cell);
					}
					BuildingComplete component5 = replacementCandidate.GetComponent<BuildingComplete>();
					if (component5 != null)
					{
						component5.Subscribe(-21016276, delegate
						{
							FinishConstruction(connections, worker);
						});
					}
					else
					{
						Debug.LogWarning("Why am I trying to replace a: " + replacementCandidate.name);
						FinishConstruction(connections, worker);
					}
				}
				KAnimGraphTileVisualizer component6 = replacementCandidate.GetComponent<KAnimGraphTileVisualizer>();
				if (component6 != null)
				{
					component6.skipCleanup = true;
				}
				Deconstructable component7 = replacementCandidate.GetComponent<Deconstructable>();
				if (component7 != null)
				{
					component7.SpawnItemsFromConstruction(worker);
				}
				Boxed<ReplaceCallbackParameters> boxed = Boxed<ReplaceCallbackParameters>.Get(new ReplaceCallbackParameters
				{
					TileLayer = building.Def.TileLayer,
					Worker = worker
				});
				replacementCandidate.Trigger(1606648047, (object)boxed);
				Boxed<ReplaceCallbackParameters>.Release(boxed);
				replacementCandidate.DeleteObject();
			}
		}
		if (flag2)
		{
			FinishConstruction(connections, worker);
		}
		PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Building, GetComponent<KSelectable>().GetName(), base.transform);
	}

	private void FinishConstruction(UtilityConnections connections, WorkerBase workerForGameplayEvent)
	{
		Rotatable component = GetComponent<Rotatable>();
		Orientation orientation = ((component != null) ? component.GetOrientation() : Orientation.Neutral);
		int cell = Grid.PosToCell(base.transform.GetLocalPosition());
		if (IsReplacementTile && building.Def.PlacementOffsets.Length > 1)
		{
			building.Def.RunOnArea(cell, orientation, delegate(int offset_cell)
			{
				if (offset_cell != cell)
				{
					GameObject replacementCandidate = building.Def.GetReplacementCandidate(offset_cell);
					if (!(replacementCandidate == null))
					{
						SimCellOccupier component6 = replacementCandidate.GetComponent<SimCellOccupier>();
						if (component6 != null)
						{
							component6.DestroySelf(null);
						}
						Deconstructable component7 = replacementCandidate.GetComponent<Deconstructable>();
						if (component7 != null)
						{
							component7.SpawnItemsFromConstruction(workerForGameplayEvent);
						}
						replacementCandidate.DeleteObject();
					}
				}
			});
		}
		UnmarkArea();
		GameObject gameObject = building.Def.Build(cell, orientation, storage, selectedElementsTags, initialTemperature, GetComponent<BuildingFacade>().CurrentFacade, playsound: true, GameClock.Instance.GetTime());
		BonusEvent.GameplayEventData gameplayEventData = new BonusEvent.GameplayEventData();
		gameplayEventData.building = gameObject.GetComponent<BuildingComplete>();
		gameplayEventData.workable = this;
		gameplayEventData.worker = workerForGameplayEvent;
		gameplayEventData.eventTrigger = GameHashes.NewBuilding;
		GameplayEventManager.Instance.Trigger(-1661515756, (object)gameplayEventData);
		gameObject.transform.rotation = base.transform.rotation;
		Rotatable component2 = gameObject.GetComponent<Rotatable>();
		if (component2 != null)
		{
			component2.SetOrientation(orientation);
		}
		KAnimGraphTileVisualizer component3 = GetComponent<KAnimGraphTileVisualizer>();
		if (component3 != null)
		{
			KAnimGraphTileVisualizer component4 = gameObject.GetComponent<KAnimGraphTileVisualizer>();
			component4.Connections = connections;
			component3.skipCleanup = true;
		}
		KSelectable component5 = GetComponent<KSelectable>();
		if (component5 != null && component5.IsSelected && gameObject.GetComponent<KSelectable>() != null)
		{
			component5.Unselect();
			if (PlayerController.Instance.ActiveTool.name == "SelectTool")
			{
				((SelectTool)PlayerController.Instance.ActiveTool).SelectNextFrame(gameObject.GetComponent<KSelectable>());
			}
		}
		gameObject.Trigger(2121280625, (object)this);
		storage.ConsumeAllIgnoringDisease();
		finished = true;
		this.DeleteObject();
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		invalidLocation = new Notification(MISC.NOTIFICATIONS.INVALIDCONSTRUCTIONLOCATION.NAME, NotificationType.BadMinor, (List<Notification> notificationList, object data) => string.Concat(MISC.NOTIFICATIONS.INVALIDCONSTRUCTIONLOCATION.TOOLTIP, notificationList.ReduceMessages(countNames: false)));
		faceTargetWhenWorking = true;
		Subscribe(-1432940121, OnReachableChangedDelegate);
		if (rotatable == null)
		{
			MarkArea();
		}
		int techTierForItem = Db.Get().TechItems.GetTechTierForItem(building.Def.PrefabID);
		if (techTierForItem > 2)
		{
			requireMinionToWork = true;
		}
		workerStatusItem = Db.Get().DuplicantStatusItems.Building;
		workingStatusItem = null;
		attributeConverter = Db.Get().AttributeConverters.ConstructionSpeed;
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.MOST_DAY_EXPERIENCE;
		minimumAttributeMultiplier = 0.75f;
		skillExperienceSkillGroup = Db.Get().SkillGroups.Building.Id;
		skillExperienceMultiplier = SKILLS.MOST_DAY_EXPERIENCE;
		Prioritizable.AddRef(base.gameObject);
		synchronizeAnims = false;
		multitoolContext = "build";
		multitoolHitEffectTag = EffectConfigs.BuildSplashId;
		workingPstComplete = null;
		workingPstFailed = null;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		CellOffset[][] table = OffsetGroups.InvertedStandardTable;
		if (building.Def.IsTilePiece)
		{
			table = OffsetGroups.InvertedStandardTableWithCorners;
		}
		CellOffset[] array = building.Def.PlacementOffsets;
		if (rotatable != null)
		{
			array = new CellOffset[building.Def.PlacementOffsets.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = rotatable.GetRotatedCellOffset(building.Def.PlacementOffsets[i]);
			}
		}
		CellOffset[][] offsetTable = OffsetGroups.BuildReachabilityTable(array, table, building.Def.ConstructionOffsetFilter);
		SetOffsetTable(offsetTable);
		storage.SetOffsetTable(offsetTable);
		Subscribe(2127324410, OnCancelDelegate);
		if (rotatable != null)
		{
			MarkArea();
		}
		fetchList = new FetchList2(storage, Db.Get().ChoreTypes.BuildFetch);
		PrimaryElement component = GetComponent<PrimaryElement>();
		Element element = ElementLoader.GetElement(SelectedElementsTags[0]);
		Debug.Assert(element != null, "Missing primary element for Constructable");
		component.ElementID = element.id;
		float b = component.Element.highTemp - 10f;
		float temperature = (component.Temperature = Mathf.Min(building.Def.Temperature, b));
		component.Temperature = temperature;
		Recipe.Ingredient[] allIngredients = Recipe.GetAllIngredients(selectedElementsTags);
		foreach (Recipe.Ingredient ingredient in allIngredients)
		{
			fetchList.Add(ingredient.tag, null, ingredient.amount);
			MaterialNeeds.UpdateNeed(ingredient.tag, ingredient.amount, base.gameObject.GetMyWorldId());
		}
		if (!building.Def.IsTilePiece)
		{
			base.gameObject.layer = LayerMask.NameToLayer("Construction");
		}
		building.RunOnArea(delegate(int offset_cell)
		{
			if (base.gameObject.GetComponent<ConduitBridge>() == null)
			{
				GameObject gameObject3 = Grid.Objects[offset_cell, 7];
				if (gameObject3 != null)
				{
					gameObject3.DeleteObject();
				}
			}
		});
		if (IsReplacementTile)
		{
			GameObject gameObject = null;
			if (building.Def.ReplacementLayer != ObjectLayer.NumLayers)
			{
				int cell = Grid.PosToCell(base.transform.GetPosition());
				gameObject = Grid.Objects[cell, (int)building.Def.ReplacementLayer];
				if (gameObject == null || gameObject == base.gameObject)
				{
					Grid.Objects[cell, (int)building.Def.ReplacementLayer] = base.gameObject;
					if (base.gameObject.GetComponent<SimCellOccupier>() != null)
					{
						int renderLayer = LayerMask.NameToLayer("Overlay");
						World.Instance.blockTileRenderer.AddBlock(renderLayer, building.Def, IsReplacementTile, SimHashes.Void, cell);
					}
					TileVisualizer.RefreshCell(cell, building.Def.TileLayer, building.Def.ReplacementLayer);
				}
				else
				{
					Debug.LogError("multiple replacement tiles on the same cell!");
					Util.KDestroyGameObject(base.gameObject);
				}
				GameObject gameObject2 = Grid.Objects[cell, (int)building.Def.ObjectLayer];
				if (gameObject2 != null)
				{
					Deconstructable component2 = gameObject2.GetComponent<Deconstructable>();
					if (component2 != null)
					{
						component2.CancelDeconstruction();
					}
				}
			}
		}
		bool flag = building.Def.BuildingComplete.GetComponent<Ladder>();
		waitForFetchesBeforeDigging = flag || (bool)building.Def.BuildingComplete.GetComponent<SimCellOccupier>() || (bool)building.Def.BuildingComplete.GetComponent<Door>() || (bool)building.Def.BuildingComplete.GetComponent<LiquidPumpingStation>();
		if (flag)
		{
			int x = 0;
			int y = 0;
			int cell2 = Grid.PosToCell(this);
			Grid.CellToXY(cell2, out x, out y);
			int y2 = y - 3;
			ladderDetectionExtents = new Extents(x, y2, 1, 5);
			ladderPartitionerEntry = GameScenePartitioner.Instance.Add("Constructable.OnNearbyBuildingLayerChanged", base.gameObject, ladderDetectionExtents, GameScenePartitioner.Instance.objectLayers[1], OnNearbyBuildingLayerChanged);
			OnNearbyBuildingLayerChanged(null);
		}
		fetchList.Submit(OnFetchListComplete, check_storage_contents: true);
		PlaceDiggables();
		ReachabilityMonitor.Instance instance = new ReachabilityMonitor.Instance(this);
		instance.StartSM();
		Subscribe(493375141, OnRefreshUserMenuDelegate);
		Prioritizable component3 = GetComponent<Prioritizable>();
		component3.onPriorityChanged = (Action<PrioritySetting>)Delegate.Combine(component3.onPriorityChanged, new Action<PrioritySetting>(OnPriorityChanged));
		OnPriorityChanged(component3.GetMasterPriority());
	}

	private void OnPriorityChanged(PrioritySetting priority)
	{
		building.RunOnArea(delegate(int cell)
		{
			Diggable diggable = Diggable.GetDiggable(cell);
			if (diggable != null)
			{
				diggable.GetComponent<Prioritizable>().SetMasterPriority(priority);
			}
		});
	}

	private void MarkArea()
	{
		int num = Grid.PosToCell(base.transform.GetPosition());
		BuildingDef def = building.Def;
		Orientation orientation = building.Orientation;
		ObjectLayer layer = (IsReplacementTile ? def.ReplacementLayer : def.ObjectLayer);
		def.MarkArea(num, orientation, layer, base.gameObject);
		if (!def.IsTilePiece)
		{
			return;
		}
		GameObject gameObject = Grid.Objects[num, (int)def.TileLayer];
		if (gameObject == null)
		{
			def.MarkArea(num, orientation, def.TileLayer, base.gameObject);
			def.RunOnArea(num, orientation, delegate(int c)
			{
				TileVisualizer.RefreshCell(c, def.TileLayer, def.ReplacementLayer);
			});
		}
		Grid.IsTileUnderConstruction[num] = true;
	}

	private void UnmarkArea()
	{
		if (!unmarked)
		{
			unmarked = true;
			int num = Grid.PosToCell(base.transform.GetPosition());
			BuildingDef def = building.Def;
			ObjectLayer layer = (IsReplacementTile ? building.Def.ReplacementLayer : building.Def.ObjectLayer);
			def.UnmarkArea(num, building.Orientation, layer, base.gameObject);
			if (def.IsTilePiece)
			{
				Grid.IsTileUnderConstruction[num] = false;
			}
			ClearPendingUproots();
		}
	}

	private void ClearPendingUproots()
	{
		foreach (Uprootable pendingUproot in pendingUproots)
		{
			if (!pendingUproot.IsNullOrDestroyed())
			{
				pendingUproot.Unsubscribe(-216549700, OnSolidChangedOrDigDestroyed);
				pendingUproot.Unsubscribe(1198393204, OnSolidChangedOrDigDestroyed);
				pendingUproot.ForceCancelUproot();
			}
		}
		pendingUproots.Clear();
	}

	private void OnNearbyBuildingLayerChanged(object data)
	{
		hasLadderNearby = false;
		for (int i = ladderDetectionExtents.y; i < ladderDetectionExtents.y + ladderDetectionExtents.height; i++)
		{
			int num = Grid.OffsetCell(0, ladderDetectionExtents.x, i);
			if (Grid.IsValidCell(num))
			{
				GameObject value = null;
				Grid.ObjectLayers[1].TryGetValue(num, out value);
				if (value != null && value.GetComponent<Ladder>() != null)
				{
					hasLadderNearby = true;
					break;
				}
			}
		}
	}

	private bool IsWire()
	{
		return building.Def.name.Contains("Wire");
	}

	public bool IconConnectionAnimation(float delay, int connectionCount, string defName, string soundName)
	{
		int num = Grid.PosToCell(base.transform.GetPosition());
		if (this.building.Def.Name.Contains(defName))
		{
			Building building = null;
			GameObject gameObject = Grid.Objects[num, 1];
			if (gameObject != null)
			{
				building = gameObject.GetComponent<Building>();
			}
			if (building != null)
			{
				bool flag = IsWire();
				int num2 = (flag ? building.GetPowerInputCell() : building.GetUtilityInputCell());
				int num3 = (flag ? num2 : building.GetUtilityOutputCell());
				if (num == num2 || num == num3)
				{
					BuildingCellVisualizer component = building.gameObject.GetComponent<BuildingCellVisualizer>();
					if (component != null && (flag ? (component.addedPorts & (EntityCellVisualizer.Ports.PowerIn | EntityCellVisualizer.Ports.PowerOut)) : (component.addedPorts & (EntityCellVisualizer.Ports.GasIn | EntityCellVisualizer.Ports.GasOut | EntityCellVisualizer.Ports.LiquidIn | EntityCellVisualizer.Ports.LiquidOut | EntityCellVisualizer.Ports.SolidIn | EntityCellVisualizer.Ports.SolidOut))) != 0)
					{
						component.ConnectedEventWithDelay(delay, connectionCount, num, soundName);
						return true;
					}
				}
			}
		}
		return false;
	}

	protected override void OnCleanUp()
	{
		if (IsReplacementTile && building.Def.isKAnimTile)
		{
			int cell = Grid.PosToCell(base.transform.GetPosition());
			GameObject gameObject = Grid.Objects[cell, (int)building.Def.ReplacementLayer];
			if (gameObject == base.gameObject && gameObject.GetComponent<SimCellOccupier>() != null)
			{
				World.Instance.blockTileRenderer.RemoveBlock(building.Def, IsReplacementTile, SimHashes.Void, cell);
			}
		}
		GameScenePartitioner.Instance.Free(ref solidPartitionerEntry);
		GameScenePartitioner.Instance.Free(ref digPartitionerEntry);
		GameScenePartitioner.Instance.Free(ref backwallPartitionerEntry);
		GameScenePartitioner.Instance.Free(ref ladderPartitionerEntry);
		SaveLoadRoot component = GetComponent<SaveLoadRoot>();
		if (component != null)
		{
			SaveLoader.Instance.saveManager.Unregister(component);
		}
		if (fetchList != null)
		{
			fetchList.Cancel("Constructable destroyed");
		}
		UnmarkArea();
		HashSetPool<Uprootable, Constructable>.PooledHashSet pooledHashSet = HashSetPool<Uprootable, Constructable>.Allocate();
		int[] placementCells = building.PlacementCells;
		foreach (int cell2 in placementCells)
		{
			Diggable diggable = Diggable.GetDiggable(cell2);
			if (diggable != null)
			{
				diggable.gameObject.DeleteObject();
			}
			TryAddUprootable(Grid.Objects[cell2, 1], pooledHashSet);
			TryAddUprootable(Grid.Objects[cell2, 5], pooledHashSet);
		}
		foreach (Uprootable item in pooledHashSet)
		{
			item.Unsubscribe(-216549700, OnSolidChangedOrDigDestroyed);
			item.Unsubscribe(1198393204, OnSolidChangedOrDigDestroyed);
			item.ForceCancelUproot();
		}
		pooledHashSet.Recycle();
		base.OnCleanUp();
		static void TryAddUprootable(GameObject plant, HashSet<Uprootable> _uprootables)
		{
			if (!(plant == null))
			{
				Uprootable component2 = plant.GetComponent<Uprootable>();
				if (!(component2 == null))
				{
					_uprootables.Add(component2);
				}
			}
		}
	}

	private void OnDiggableReachabilityChanged(object _)
	{
		if (IsReplacementTile)
		{
			return;
		}
		int diggable_count = 0;
		int unreachable_count = 0;
		building.RunOnArea(delegate(int offset_cell)
		{
			Diggable diggable = Diggable.GetDiggable(offset_cell);
			if (diggable != null && diggable.isActiveAndEnabled)
			{
				int num = diggable_count + 1;
				diggable_count = num;
				if (!diggable.GetComponent<KPrefabID>().HasTag(GameTags.Reachable))
				{
					num = unreachable_count + 1;
					unreachable_count = num;
				}
			}
		});
		bool flag = unreachable_count > 0 && unreachable_count == diggable_count;
		if (flag != hasUnreachableDigs)
		{
			if (flag)
			{
				GetComponent<KSelectable>().AddStatusItem(Db.Get().BuildingStatusItems.ConstructableDigUnreachable);
			}
			else
			{
				GetComponent<KSelectable>().RemoveStatusItem(Db.Get().BuildingStatusItems.ConstructableDigUnreachable);
			}
			hasUnreachableDigs = flag;
		}
	}

	private bool IsCellDigRequired(int offset_cell)
	{
		if (building.Def.ObjectLayer == ObjectLayer.Backwall)
		{
			return BackwallManager.HasBackwall(offset_cell) || Diggable.IsDiggable(offset_cell);
		}
		return Diggable.IsDiggable(offset_cell);
	}

	private void PlaceDiggables()
	{
		if (waitForFetchesBeforeDigging && fetchList != null && !hasLadderNearby)
		{
			OnDiggableReachabilityChanged(null);
			return;
		}
		if (!solidPartitionerEntry.IsValid())
		{
			Extents validPlacementExtents = building.GetValidPlacementExtents();
			solidPartitionerEntry = GameScenePartitioner.Instance.Add("Constructable.PlaceDiggables", base.gameObject, validPlacementExtents, GameScenePartitioner.Instance.solidChangedLayer, OnSolidChangedOrDigDestroyed);
			digPartitionerEntry = GameScenePartitioner.Instance.Add("Constructable.PlaceDiggables", base.gameObject, validPlacementExtents, GameScenePartitioner.Instance.digDestroyedLayer, OnSolidChangedOrDigDestroyed);
			backwallPartitionerEntry = GameScenePartitioner.Instance.Add("Constructable.PlaceDiggables", base.gameObject, validPlacementExtents, GameScenePartitioner.Instance.backwallChangedLayer, OnSolidChangedOrDigDestroyed);
		}
		bool digs_complete = true;
		if (!IsReplacementTile)
		{
			PrioritySetting master_priority = GetComponent<Prioritizable>().GetMasterPriority();
			HashSetPool<Uprootable, Constructable>.PooledHashSet uprootables = HashSetPool<Uprootable, Constructable>.Allocate();
			building.RunOnArea(delegate(int offset_cell)
			{
				if (IsCellDigRequired(offset_cell))
				{
					digs_complete = false;
					Diggable diggable = Diggable.GetDiggable(offset_cell);
					if (diggable != null && !diggable.isActiveAndEnabled)
					{
						diggable.Unsubscribe(-1432940121, OnDiggableReachabilityChanged);
						diggable = null;
					}
					if (diggable == null)
					{
						diggable = GameUtil.KInstantiate(Assets.GetPrefab(new Tag("DigPlacer")), Grid.SceneLayer.Move).GetComponent<Diggable>();
						diggable.choreTypeIdHash = Db.Get().ChoreTypes.BuildDig.IdHash;
						diggable.gameObject.SetActive(value: true);
						diggable.transform.SetPosition(Grid.CellToPosCBC(offset_cell, Grid.SceneLayer.Move));
						Grid.Objects[offset_cell, 7] = diggable.gameObject;
						diggable.Subscribe(-1432940121, OnDiggableReachabilityChanged);
					}
					diggable.GetComponent<Prioritizable>().SetMasterPriority(master_priority);
					RenderUtil.EnableRenderer(diggable.transform, is_enabled: false);
					SaveLoadRoot component = diggable.GetComponent<SaveLoadRoot>();
					if (component != null)
					{
						UnityEngine.Object.Destroy(component);
					}
				}
				else if (building.Def.ObjectLayer == ObjectLayer.Building)
				{
					GameObject plant = Grid.Objects[offset_cell, 5];
					if (Uprootable.CanUproot(plant, out var uprootable))
					{
						digs_complete = false;
						uprootables.Add(uprootable);
					}
				}
			});
			ListPool<Uprootable, Constructable>.PooledList pooledList = ListPool<Uprootable, Constructable>.Allocate();
			foreach (Uprootable pendingUproot in pendingUproots)
			{
				if (pendingUproot.IsNullOrDestroyed())
				{
					pooledList.Add(pendingUproot);
				}
				else if (!uprootables.Contains(pendingUproot))
				{
					pendingUproot.Unsubscribe(-216549700, OnSolidChangedOrDigDestroyed);
					pendingUproot.Unsubscribe(1198393204, OnSolidChangedOrDigDestroyed);
					pooledList.Add(pendingUproot);
				}
			}
			foreach (Uprootable item in pooledList)
			{
				pendingUproots.Remove(item);
			}
			pooledList.Recycle();
			foreach (Uprootable item2 in uprootables)
			{
				bool flag = pendingUproots.Add(item2);
				item2.choreTypeIdHash = Db.Get().ChoreTypes.BuildUproot.IdHash;
				item2.MarkForUproot();
				if (flag)
				{
					item2.Subscribe(-216549700, OnSolidChangedOrDigDestroyed);
					item2.Subscribe(1198393204, OnSolidChangedOrDigDestroyed);
				}
			}
			uprootables.Recycle();
			OnDiggableReachabilityChanged(null);
		}
		bool flag2 = building.Def.IsValidBuildLocation(base.gameObject, base.transform.GetPosition(), building.Orientation, IsReplacementTile);
		if (flag2)
		{
			notifier.Remove(invalidLocation);
		}
		else
		{
			notifier.Add(invalidLocation);
		}
		GetComponent<KSelectable>().ToggleStatusItem(Db.Get().BuildingStatusItems.InvalidBuildingLocation, !flag2, this);
		bool flag3 = digs_complete && flag2 && fetchList == null;
		if (flag3 && buildChore == null)
		{
			buildChore = new WorkChore<Constructable>(Db.Get().ChoreTypes.Build, this, null, run_until_complete: true, UpdateBuildState, UpdateBuildState, UpdateBuildState, allow_in_red_alert: true, null, ignore_schedule_block: false, only_when_operational: true, null, is_preemptable: true);
			UpdateBuildState(buildChore);
		}
		else if (!flag3 && buildChore != null)
		{
			buildChore.Cancel("Need to dig");
			buildChore = null;
		}
	}

	private void OnFetchListComplete()
	{
		fetchList = null;
		PlaceDiggables();
		ClearMaterialNeeds();
	}

	private void ClearMaterialNeeds()
	{
		if (!materialNeedsCleared)
		{
			Recipe.Ingredient[] allIngredients = Recipe.GetAllIngredients(SelectedElementsTags);
			foreach (Recipe.Ingredient ingredient in allIngredients)
			{
				MaterialNeeds.UpdateNeed(ingredient.tag, 0f - ingredient.amount, base.gameObject.GetMyWorldId());
			}
			materialNeedsCleared = true;
		}
	}

	private void OnSolidChangedOrDigDestroyed(object data)
	{
		if (!(this == null) && !finished)
		{
			PlaceDiggables();
		}
	}

	private void UpdateBuildState(Chore chore)
	{
		KSelectable component = GetComponent<KSelectable>();
		StatusItem status_item = (chore.InProgress() ? Db.Get().BuildingStatusItems.UnderConstruction : Db.Get().BuildingStatusItems.UnderConstructionNoWorker);
		component.SetStatusItem(Db.Get().StatusItemCategories.Main, status_item);
	}

	[OnDeserialized]
	internal void OnDeserialized()
	{
		if (ids == null)
		{
			return;
		}
		selectedElements = new Element[ids.Length];
		for (int i = 0; i < ids.Length; i++)
		{
			selectedElements[i] = ElementLoader.FindElementByHash((SimHashes)ids[i]);
		}
		if (selectedElementsTags == null)
		{
			selectedElementsTags = new Tag[ids.Length];
			for (int j = 0; j < ids.Length; j++)
			{
				selectedElementsTags[j] = ElementLoader.FindElementByHash((SimHashes)ids[j]).tag;
			}
		}
		Debug.Assert(selectedElements.Length == selectedElementsTags.Length);
		for (int k = 0; k < selectedElements.Length; k++)
		{
			Debug.Assert(selectedElements[k].tag == SelectedElementsTags[k]);
		}
	}

	private void OnReachableChanged(object data)
	{
		KAnimControllerBase component = GetComponent<KAnimControllerBase>();
		if (((Boxed<bool>)data).value)
		{
			GetComponent<KSelectable>().RemoveStatusItem(Db.Get().BuildingStatusItems.ConstructionUnreachable);
			if (component != null)
			{
				component.TintColour = Game.Instance.uiColours.Build.validLocation;
			}
		}
		else
		{
			GetComponent<KSelectable>().AddStatusItem(Db.Get().BuildingStatusItems.ConstructionUnreachable, this);
			if (component != null)
			{
				component.TintColour = Game.Instance.uiColours.Build.unreachable;
			}
		}
	}

	private void OnRefreshUserMenu(object data)
	{
		Game.Instance.userMenu.AddButton(base.gameObject, new KIconButtonMenu.ButtonInfo("action_cancel", UI.USERMENUACTIONS.CANCELCONSTRUCTION.NAME, OnPressCancel, Action.NumActions, null, null, null, UI.USERMENUACTIONS.CANCELCONSTRUCTION.TOOLTIP));
	}

	private void OnPressCancel()
	{
		base.gameObject.Trigger(2127324410);
	}

	private void OnCancel(object _ = null)
	{
		DetailsScreen.Instance.Show(show: false);
		ClearMaterialNeeds();
		ClearPendingUproots();
	}
}
