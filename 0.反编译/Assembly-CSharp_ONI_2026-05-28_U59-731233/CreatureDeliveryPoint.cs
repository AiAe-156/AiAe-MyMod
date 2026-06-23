using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using KSerialization;
using UnityEngine;

public class CreatureDeliveryPoint : StateMachineComponent<CreatureDeliveryPoint.SMInstance>
{
	public class SMInstance : GameStateMachine<States, SMInstance, CreatureDeliveryPoint, object>.GameInstance
	{
		public bool isDroppingAllCreatures = false;

		private Operational operational;

		private BuildingPointStraw straw;

		public bool IsOperational => operational != null && operational.IsOperational;

		public bool IsStrawInstalled => straw != null;

		public bool IsStrawOutsideLiquid => IsStrawInstalled && !straw.isInLiquid;

		public bool IsStrawBlocked => IsStrawInstalled && straw.currentDepth <= 0;

		public SMInstance(CreatureDeliveryPoint master)
			: base(master)
		{
			operational = GetComponent<Operational>();
			straw = GetComponent<BuildingPointStraw>();
		}
	}

	public class States : GameStateMachine<States, SMInstance, CreatureDeliveryPoint>
	{
		public class OperationalState : State
		{
			public State waiting;

			public State interact_waiting;

			public State interact_pre;

			public State interact_pst;
		}

		public class UnoperationalStates : State
		{
			public State noOperational;

			public State strawBlocked;

			public State noLiquidOnStraw;
		}

		public OperationalState operational;

		public UnoperationalStates unoperational;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = operational.waiting;
			root.Update("RefreshCreatureCount", delegate(SMInstance smi, float dt)
			{
				smi.master.critterCapacity.RefreshCreatureCount();
			}, UpdateRate.SIM_1000ms);
			root.EventHandler(GameHashes.OnStorageChange, delegate(SMInstance smi)
			{
				if (!smi.master.playAnimsOnFetch)
				{
					DropAllCreatures(smi);
				}
			});
			unoperational.PlayAnim("off", KAnim.PlayMode.Once, (SMInstance smi) => smi.master.animSuffix).EventTransition(GameHashes.LogicEvent, operational, ShouldBeOn).EventTransition(GameHashes.OperationalChanged, operational, ShouldBeOn)
				.EventTransition(GameHashes.BuildingStrawChange, operational, ShouldBeOn)
				.Enter(ClearFetches)
				.DefaultState(unoperational.noOperational);
			unoperational.noOperational.EventTransition(GameHashes.OperationalChanged, unoperational.strawBlocked, (SMInstance smi) => IsOperational(smi) && IsStrawBlocked(smi)).EventTransition(GameHashes.OperationalChanged, unoperational.noLiquidOnStraw, (SMInstance smi) => IsOperational(smi) && IsStrawOutsideLiquid(smi));
			unoperational.strawBlocked.ToggleStatusItem(Db.Get().BuildingStatusItems.OutputTileBlocked).EventTransition(GameHashes.OperationalChanged, unoperational.noOperational, GameStateMachine<States, SMInstance, CreatureDeliveryPoint, object>.Not(IsOperational)).EventTransition(GameHashes.BuildingStrawChange, unoperational.noLiquidOnStraw, (SMInstance smi) => !IsStrawBlocked(smi) && IsStrawOutsideLiquid(smi));
			unoperational.noLiquidOnStraw.ToggleStatusItem(Db.Get().BuildingStatusItems.NotSubmerged).EventTransition(GameHashes.OperationalChanged, unoperational.noOperational, GameStateMachine<States, SMInstance, CreatureDeliveryPoint, object>.Not(IsOperational)).EventTransition(GameHashes.BuildingStrawChange, unoperational.strawBlocked, IsStrawBlocked);
			operational.PlayAnim("on", KAnim.PlayMode.Once, (SMInstance smi) => smi.master.animSuffix).EventTransition(GameHashes.LogicEvent, unoperational, GameStateMachine<States, SMInstance, CreatureDeliveryPoint, object>.Not(ShouldBeOn)).EventTransition(GameHashes.BuildingStrawChange, unoperational, GameStateMachine<States, SMInstance, CreatureDeliveryPoint, object>.Not(ShouldBeOn))
				.EventTransition(GameHashes.OperationalChanged, unoperational, GameStateMachine<States, SMInstance, CreatureDeliveryPoint, object>.Not(ShouldBeOn))
				.Enter(RefreshFetches)
				.DefaultState(operational.waiting);
			operational.waiting.EnterTransition(operational.interact_waiting, (SMInstance smi) => smi.master.playAnimsOnFetch).EnterTransition(operational.interact_pre, HasItemInInventory).PlayAnim("on", KAnim.PlayMode.Once, (SMInstance smi) => smi.master.animSuffix);
			operational.interact_waiting.EnterTransition(operational.interact_pre, HasItemInInventory).WorkableStartTransition((SMInstance smi) => smi.master.GetComponent<Storage>(), operational.interact_pre);
			operational.interact_pre.PlayAnim("working_pre", KAnim.PlayMode.Once, (SMInstance smi) => smi.master.animSuffix).OnAnimQueueComplete(operational.interact_pst);
			operational.interact_pst.Enter(DropAllCreatures).PlayAnim("working_pst", KAnim.PlayMode.Once, (SMInstance smi) => smi.master.animSuffix).OnAnimQueueComplete(operational.interact_waiting);
		}

		public static bool ShouldBeOn(SMInstance smi)
		{
			return IsLogicEnabled(smi) && IsOperational(smi) && !IsStrawBlocked(smi) && !IsStrawOutsideLiquid(smi);
		}

		public static bool IsLogicEnabled(SMInstance smi)
		{
			return smi.master.LogicEnabled();
		}

		public static bool IsOperational(SMInstance smi)
		{
			return smi.IsOperational;
		}

		public static bool IsStrawBlocked(SMInstance smi)
		{
			return smi.IsStrawBlocked;
		}

		public static bool IsStrawOutsideLiquid(SMInstance smi)
		{
			return smi.IsStrawOutsideLiquid;
		}

		public static void ClearFetches(SMInstance smi)
		{
			smi.master.ClearFetches();
		}

		public static void RefreshFetches(SMInstance smi)
		{
			smi.master.RebalanceFetches();
		}

		public static bool HasItemInInventory(SMInstance smi)
		{
			return !smi.master.GetComponent<Storage>().IsEmpty();
		}

		public static void DropAllCreatures(SMInstance smi)
		{
			if (smi.isDroppingAllCreatures)
			{
				return;
			}
			smi.isDroppingAllCreatures = true;
			Storage component = smi.master.GetComponent<Storage>();
			if (component.IsEmpty())
			{
				return;
			}
			List<GameObject> items = component.items;
			int count = items.Count;
			int cell = Grid.OffsetCell(Grid.PosToCell(smi.transform.GetPosition()), smi.master.spawnOffset);
			Vector3 position = Grid.CellToPosCBC(cell, Grid.SceneLayer.Creatures);
			cell = Grid.OffsetCell(Grid.PosToCell(smi.transform.GetPosition()), smi.master.largeCritterSpawnOffset);
			Vector3 position2 = Grid.CellToPosCBC(cell, Grid.SceneLayer.Creatures);
			for (int num = count - 1; num >= 0; num--)
			{
				GameObject gameObject = items[num];
				component.Drop(gameObject);
				KPrefabID component2 = gameObject.GetComponent<KPrefabID>();
				if (component2 == null || !component2.HasTag(GameTags.LargeCreature))
				{
					gameObject.transform.SetPosition(position);
				}
				else
				{
					gameObject.transform.SetPosition(position2);
				}
				KBatchedAnimController component3 = gameObject.GetComponent<KBatchedAnimController>();
				component3.SetSceneLayer(Grid.SceneLayer.Creatures);
			}
			smi.master.critterCapacity.RefreshCreatureCount();
			smi.isDroppingAllCreatures = false;
		}
	}

	[MyCmpAdd]
	private Prioritizable prioritizable;

	[MyCmpReq]
	public BaggableCritterCapacityTracker critterCapacity;

	[Obsolete]
	[Serialize]
	private int creatureLimit = 20;

	public CellOffset[] deliveryOffsets = new CellOffset[1];

	public CellOffset spawnOffset = new CellOffset(0, 0);

	public CellOffset largeCritterSpawnOffset = new CellOffset(0, 0);

	private List<FetchOrder2> fetches;

	public bool playAnimsOnFetch;

	public string animSuffix = "";

	private LogicPorts logicPorts;

	private static readonly EventSystem.IntraObjectHandler<CreatureDeliveryPoint> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<CreatureDeliveryPoint>(delegate(CreatureDeliveryPoint component, object data)
	{
		component.OnCopySettings(data);
	});

	private static readonly EventSystem.IntraObjectHandler<CreatureDeliveryPoint> RefreshCreatureCountDelegate = new EventSystem.IntraObjectHandler<CreatureDeliveryPoint>(delegate(CreatureDeliveryPoint component, object data)
	{
		component.critterCapacity.RefreshCreatureCount(data);
	});

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		fetches = new List<FetchOrder2>();
		Subscribe(360192579, OnBuildingStrawChanged);
		TreeFilterable component = GetComponent<TreeFilterable>();
		component.OnFilterChanged = (Action<HashSet<Tag>>)Delegate.Combine(component.OnFilterChanged, new Action<HashSet<Tag>>(OnFilterChanged));
		GetComponent<Storage>().SetOffsets(deliveryOffsets);
		Prioritizable.AddRef(base.gameObject);
	}

	private void OnBuildingStrawChanged(object o)
	{
		BuildingPointStraw buildingPointStraw = (BuildingPointStraw)o;
		spawnOffset = buildingPointStraw.GetBottomCellOffset();
		largeCritterSpawnOffset = new CellOffset(0, spawnOffset.y - 1);
		animSuffix = buildingPointStraw.GetAnimSuffix();
		StateMachine.BaseState currentState = base.smi.GetCurrentState();
		if (currentState != null && currentState != base.smi.sm.operational.interact_pre && currentState != base.smi.sm.operational.interact_pst)
		{
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			if (currentState == base.smi.sm.unoperational)
			{
				component.Play("off" + animSuffix);
			}
			else
			{
				component.Play("on" + animSuffix);
			}
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
		Subscribe(-905833192, OnCopySettingsDelegate);
		Subscribe(643180843, RefreshCreatureCountDelegate);
		critterCapacity = GetComponent<BaggableCritterCapacityTracker>();
		BaggableCritterCapacityTracker baggableCritterCapacityTracker = critterCapacity;
		baggableCritterCapacityTracker.onCountChanged = (System.Action)Delegate.Combine(baggableCritterCapacityTracker.onCountChanged, new System.Action(RebalanceFetches));
		critterCapacity.RefreshCreatureCount();
		logicPorts = GetComponent<LogicPorts>();
		if (logicPorts != null)
		{
			logicPorts.Subscribe(-801688580, OnLogicChanged);
		}
	}

	private void OnLogicChanged(object data)
	{
		LogicValueChanged logicValueChanged = (LogicValueChanged)data;
		if (logicValueChanged.portID == "CritterDropOffInput")
		{
			if (logicValueChanged.newValue > 0)
			{
				RebalanceFetches();
			}
			else
			{
				ClearFetches();
			}
		}
	}

	[Obsolete]
	[OnDeserialized]
	private void OnDeserialized()
	{
		if (critterCapacity != null && creatureLimit > 0)
		{
			critterCapacity.creatureLimit = creatureLimit;
			creatureLimit = -1;
		}
	}

	private void OnCopySettings(object data)
	{
		GameObject gameObject = (GameObject)data;
		if (!(gameObject == null))
		{
			CreatureDeliveryPoint component = gameObject.GetComponent<CreatureDeliveryPoint>();
			if (!(component == null))
			{
				RebalanceFetches();
			}
		}
	}

	private void OnFilterChanged(HashSet<Tag> tags)
	{
		ClearFetches();
		RebalanceFetches();
	}

	private void ClearFetches()
	{
		for (int num = fetches.Count - 1; num >= 0; num--)
		{
			fetches[num].Cancel("clearing all fetches");
		}
		fetches.Clear();
	}

	private void RebalanceFetches()
	{
		if (!LogicEnabled() || !States.ShouldBeOn(base.smi))
		{
			return;
		}
		TreeFilterable component = GetComponent<TreeFilterable>();
		HashSet<Tag> tags = component.GetTags();
		ChoreType creatureFetch = Db.Get().ChoreTypes.CreatureFetch;
		Storage component2 = GetComponent<Storage>();
		int num = critterCapacity.creatureLimit - critterCapacity.storedCreatureCount;
		int count = fetches.Count;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		for (int num6 = fetches.Count - 1; num6 >= 0; num6--)
		{
			if (fetches[num6].IsComplete())
			{
				fetches.RemoveAt(num6);
				num2++;
			}
		}
		int num7 = 0;
		for (int i = 0; i < fetches.Count; i++)
		{
			if (!fetches[i].InProgress)
			{
				num7++;
			}
		}
		if (num7 == 0 && fetches.Count < num)
		{
			float minimumFetchAmount = FetchChore.GetMinimumFetchAmount(tags);
			FetchOrder2 fetchOrder = new FetchOrder2(creatureFetch, tags, FetchChore.MatchCriteria.MatchID, GameTags.Creatures.Deliverable, null, component2, minimumFetchAmount, Operational.State.Operational);
			fetchOrder.validateRequiredTagOnTagChange = true;
			fetchOrder.Submit(OnFetchComplete, check_storage_contents: false, OnFetchBegun);
			fetches.Add(fetchOrder);
			num3++;
		}
		int num8 = fetches.Count - num;
		int num9 = fetches.Count - 1;
		while (num9 >= 0 && num8 > 0)
		{
			if (!fetches[num9].InProgress)
			{
				fetches[num9].Cancel("fewer creatures in room");
				fetches.RemoveAt(num9);
				num8--;
				num4++;
			}
			num9--;
		}
		while (num8 > 0 && fetches.Count > 0)
		{
			fetches[fetches.Count - 1].Cancel("fewer creatures in room");
			fetches.RemoveAt(fetches.Count - 1);
			num8--;
			num5++;
		}
	}

	private void OnFetchComplete(FetchOrder2 fetchOrder, Pickupable fetchedItem)
	{
		RebalanceFetches();
	}

	private void OnFetchBegun(FetchOrder2 fetchOrder, Pickupable fetchedItem)
	{
		RebalanceFetches();
	}

	protected override void OnCleanUp()
	{
		base.smi.StopSM("OnCleanUp");
		TreeFilterable component = GetComponent<TreeFilterable>();
		component.OnFilterChanged = (Action<HashSet<Tag>>)Delegate.Remove(component.OnFilterChanged, new Action<HashSet<Tag>>(OnFilterChanged));
		base.OnCleanUp();
	}

	public bool LogicEnabled()
	{
		return logicPorts == null || !logicPorts.IsPortConnected("CritterDropOffInput") || logicPorts.GetInputValue("CritterDropOffInput") == 1;
	}
}
