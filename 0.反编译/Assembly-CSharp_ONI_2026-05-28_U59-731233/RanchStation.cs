using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class RanchStation : GameStateMachine<RanchStation, RanchStation.Instance, IStateMachineTarget, RanchStation.Def>
{
	public class Def : BaseDef
	{
		public Func<GameObject, Instance, bool> IsCritterEligibleToBeRanchedCb;

		public Action<GameObject, WorkerBase> OnRanchCompleteCb;

		public Action<RanchedStates.Instance, Workable> OnRanchWorkBegins = null;

		public Action<GameObject, float, Workable> OnRanchWorkTick = null;

		public HashedString RanchedPreAnim = "idle_loop";

		public HashedString RanchedLoopAnim = "idle_loop";

		public HashedString RanchedPstAnim = "idle_loop";

		public HashedString RanchedAbortAnim = "idle_loop";

		public HashedString RancherInteractAnim = "anim_interacts_rancherstation_kanim";

		public HashedString RancherCallingAndWipeBrowAnim = "anim_interacts_rancherstation_kanim";

		public bool RancherWipesBrowAnim = false;

		public StatusItem RanchingStatusItem = Db.Get().DuplicantStatusItems.Ranching;

		public StatusItem CreatureRanchingStatusItem = Db.Get().CreatureStatusItems.GettingRanched;

		public float WorkTime = 12f;

		public bool RequiresRoom = true;

		public Func<Instance, int> GetTargetRanchCell = (Instance smi) => Grid.PosToCell(smi);
	}

	public class OperationalState : State
	{
	}

	public new class Instance : GameInstance
	{
		[MyCmpAdd]
		public ManuallySetRemoteWorkTargetComponent remoteChore;

		private const int QUEUE_SIZE = 2;

		private List<RanchableMonitor.Instance> targetRanchables = new List<RanchableMonitor.Instance>();

		private RanchedStates.Instance activeRanchable = null;

		private Room ranch = null;

		private WorkerBase rancher = null;

		private BuildingComplete station = null;

		private int onRoomUpdatedHandle = -1;

		public RanchedStates.Instance ActiveRanchable => activeRanchable;

		private bool isCritterAvailableForRanching => targetRanchables.Count > 0;

		public bool IsCritterAvailableForRanching
		{
			get
			{
				ValidateTargetRanchables();
				return isCritterAvailableForRanching;
			}
		}

		public bool HasRancher => rancher != null;

		public bool IsRancherReady => base.sm.RancherIsReady.Get(this);

		public Extents StationExtents => station.GetExtents();

		public int GetRanchNavTarget()
		{
			return base.def.GetTargetRanchCell(this);
		}

		private CavityInfo GetStationCavity()
		{
			if (!base.def.RequiresRoom)
			{
				return Game.Instance.roomProber.GetCavityForCell(GetRanchNavTarget());
			}
			if (ranch != null)
			{
				return ranch.cavity;
			}
			return null;
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			base.gameObject.AddOrGet<RancherChore.RancherWorkable>();
			station = GetComponent<BuildingComplete>();
		}

		public Chore CreateChore()
		{
			RancherChore rancherChore = new RancherChore(GetComponent<KPrefabID>());
			StateMachine<RancherChore.RancherChoreStates, RancherChore.RancherChoreStates.Instance, IStateMachineTarget, object>.TargetParameter targetParameter = rancherChore.smi.sm.rancher;
			StateMachine<RancherChore.RancherChoreStates, RancherChore.RancherChoreStates.Instance, IStateMachineTarget, object>.Parameter<GameObject>.Context context = targetParameter.GetContext(rancherChore.smi);
			context.onDirty = (Action<RancherChore.RancherChoreStates.Instance>)Delegate.Combine(context.onDirty, new Action<RancherChore.RancherChoreStates.Instance>(OnRancherChanged));
			rancher = targetParameter.Get<WorkerBase>(rancherChore.smi);
			return rancherChore;
		}

		public int GetTargetRanchCell()
		{
			return base.def.GetTargetRanchCell(this);
		}

		public override void StartSM()
		{
			base.StartSM();
			onRoomUpdatedHandle = Subscribe(144050788, OnRoomUpdated);
			CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(GetTargetRanchCell());
			if (cavityForCell != null && cavityForCell.room != null)
			{
				OnRoomUpdated(cavityForCell.room);
			}
		}

		public override void StopSM(string reason)
		{
			base.StopSM(reason);
			Unsubscribe(ref onRoomUpdatedHandle);
		}

		private void OnRoomUpdated(object data)
		{
			ranch = data as Room;
			if (ranch != null && base.def.RequiresRoom && ranch.roomType != Db.Get().RoomTypes.CreaturePen)
			{
				TriggerRanchStationNoLongerAvailable();
				ranch = null;
			}
		}

		private void OnRancherChanged(RancherChore.RancherChoreStates.Instance choreInstance)
		{
			rancher = choreInstance.sm.rancher.Get<WorkerBase>(choreInstance);
			TriggerRanchStationNoLongerAvailable();
		}

		public bool TryGetRanched(RanchedStates.Instance ranchable)
		{
			return activeRanchable == null || activeRanchable == ranchable;
		}

		public void MessageCreatureArrived(RanchedStates.Instance critter)
		{
			activeRanchable = critter;
			base.sm.RancherIsReady.Set(value: false, this);
			Trigger(-1357116271);
		}

		public void MessageRancherReady()
		{
			base.sm.RancherIsReady.Set(value: true, base.smi);
			MessageRanchables(GameHashes.RancherReadyAtRanchStation);
		}

		private bool CanRanchableBeRanchedAtRanchStation(RanchableMonitor.Instance ranchable)
		{
			bool flag = !ranchable.IsNullOrStopped();
			if (flag && ranchable.TargetRanchStation != null && ranchable.TargetRanchStation != this)
			{
				flag = !ranchable.TargetRanchStation.IsRunning() || !ranchable.TargetRanchStation.HasRancher;
			}
			flag = flag && base.def.IsCritterEligibleToBeRanchedCb(ranchable.gameObject, this) && ranchable.ChoreConsumer.IsChoreEqualOrAboveCurrentChorePriority<RanchedStates>();
			if (flag)
			{
				int cell = Grid.PosToCell(ranchable.transform.GetPosition());
				CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(cell);
				CavityInfo stationCavity = GetStationCavity();
				if (cavityForCell == null || stationCavity == null || cavityForCell != stationCavity)
				{
					flag = false;
				}
				else
				{
					int cell2 = GetRanchNavTarget();
					if (ranchable.HasTag(GameTags.Creatures.Flyer))
					{
						cell2 = Grid.CellAbove(cell2);
					}
					int navigationCost = ranchable.NavComponent.GetNavigationCost(cell2);
					flag = navigationCost != -1;
				}
			}
			return flag;
		}

		public void ValidateTargetRanchables()
		{
			if (!HasRancher)
			{
				return;
			}
			List<RanchableMonitor.Instance> list = CollectionPool<List<RanchableMonitor.Instance>, RanchableMonitor.Instance>.Get();
			list.AddRange(targetRanchables);
			foreach (RanchableMonitor.Instance item in list)
			{
				if (item.States == null || !CanRanchableBeRanchedAtRanchStation(item))
				{
					Abandon(item);
				}
			}
			CollectionPool<List<RanchableMonitor.Instance>, RanchableMonitor.Instance>.Release(list);
		}

		public void FindRanchable(object _ = null)
		{
			CavityInfo stationCavity = GetStationCavity();
			if (stationCavity == null)
			{
				return;
			}
			ValidateTargetRanchables();
			if (targetRanchables.Count == 2)
			{
				return;
			}
			List<KPrefabID> creatures = stationCavity.creatures;
			if (HasRancher && !isCritterAvailableForRanching && creatures.Count == 0)
			{
				TryNotifyEmptyRanch();
			}
			for (int i = 0; i < creatures.Count; i++)
			{
				KPrefabID kPrefabID = creatures[i];
				if (!(kPrefabID == null))
				{
					RanchableMonitor.Instance sMI = kPrefabID.GetSMI<RanchableMonitor.Instance>();
					if (!targetRanchables.Contains(sMI) && CanRanchableBeRanchedAtRanchStation(sMI) && sMI != null)
					{
						sMI.States.SetRanchStation(this);
						targetRanchables.Add(sMI);
						break;
					}
				}
			}
		}

		public Option<CavityInfo> GetCavityInfo()
		{
			CavityInfo stationCavity = GetStationCavity();
			if (stationCavity == null)
			{
				return Option.None;
			}
			return stationCavity;
		}

		public void RanchCreature()
		{
			if (!activeRanchable.IsNullOrStopped())
			{
				Debug.Assert(activeRanchable != null, "targetRanchable was null");
				Debug.Assert(activeRanchable.GetMaster() != null, "GetMaster was null");
				Debug.Assert(base.def != null, "def was null");
				Debug.Assert(base.def.OnRanchCompleteCb != null, "onRanchCompleteCb cb was null");
				base.def.OnRanchCompleteCb(activeRanchable.gameObject, rancher);
				targetRanchables.Remove(activeRanchable.Monitor);
				activeRanchable.Trigger(1827504087);
				activeRanchable = null;
				FindRanchable();
			}
		}

		public void TriggerRanchStationNoLongerAvailable()
		{
			for (int num = targetRanchables.Count - 1; num >= 0; num--)
			{
				RanchableMonitor.Instance instance = targetRanchables[num];
				if (instance.IsNullOrStopped() || instance.States.IsNullOrStopped())
				{
					instance.TargetRanchStation = null;
					targetRanchables.RemoveAt(num);
				}
				else
				{
					targetRanchables.Remove(instance);
					instance.Trigger(1689625967);
				}
			}
			Debug.Assert(targetRanchables.Count == 0, "targetRanchables is not empty");
			activeRanchable = null;
			base.sm.RancherIsReady.Set(value: false, this);
		}

		public void MessageRanchables(GameHashes hash)
		{
			for (int i = 0; i < targetRanchables.Count; i++)
			{
				RanchableMonitor.Instance instance = targetRanchables[i];
				if (!instance.IsNullOrStopped())
				{
					Game.BrainScheduler.PrioritizeBrain(instance.GetComponent<CreatureBrain>());
					if (!instance.States.IsNullOrStopped())
					{
						instance.Trigger((int)hash);
					}
				}
			}
		}

		public void Abandon(RanchableMonitor.Instance critter)
		{
			if (critter == null)
			{
				Debug.LogWarning("Null critter trying to abandon ranch station");
				targetRanchables.Remove(critter);
				return;
			}
			critter.TargetRanchStation = null;
			if (targetRanchables.Remove(critter) && critter.States != null)
			{
				bool flag = !isCritterAvailableForRanching;
				if (critter.States == activeRanchable)
				{
					flag = true;
					activeRanchable = null;
				}
				if (flag)
				{
					TryNotifyEmptyRanch();
				}
			}
		}

		private void TryNotifyEmptyRanch()
		{
			if (HasRancher)
			{
				rancher.Trigger(-364750427);
			}
		}

		public bool IsCritterInQueue(RanchableMonitor.Instance critter)
		{
			return targetRanchables.Contains(critter);
		}

		public List<RanchableMonitor.Instance> DEBUG_GetTargetRanchables()
		{
			return targetRanchables;
		}
	}

	public BoolParameter RancherIsReady;

	public State Unoperational;

	public OperationalState Operational;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = Operational;
		Unoperational.TagTransition(GameTags.Operational, Operational);
		Operational.TagTransition(GameTags.Operational, Unoperational, on_remove: true).ToggleChore((Instance smi) => smi.CreateChore(), SetRemoteChore, Unoperational, Unoperational).Update("FindRanachable", delegate(Instance smi, float dt)
		{
			smi.FindRanchable();
		});
	}

	private static void SetRemoteChore(Instance smi, Chore chore)
	{
		smi.remoteChore.SetChore(chore);
	}
}
