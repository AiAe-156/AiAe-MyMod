using KSerialization;

public class ClusterMapLargeImpactor : GameStateMachine<ClusterMapLargeImpactor, ClusterMapLargeImpactor.Instance, IStateMachineTarget, ClusterMapLargeImpactor.Def>
{
	public class Def : BaseDef
	{
		public string name;

		public string description;

		public string eventID;

		public int destinationWorldID;

		public float arrivalTime;
	}

	public class TravelingState : State
	{
		public State unidentified;

		public State identified;
	}

	public new class Instance : GameInstance
	{
		[Serialize]
		public int DestinationWorldID = -1;

		[Serialize]
		public float ArrivalTime;

		[Serialize]
		private float Speed = 0f;

		[MyCmpGet]
		private InfoDescription descriptor;

		[MyCmpGet]
		private KSelectable selectable;

		[MyCmpGet]
		private ClusterMapMeteorShowerVisualizer visualizer;

		[MyCmpGet]
		private ClusterTraveler traveler;

		[MyCmpGet]
		private ClusterDestinationSelector destinationSelector;

		public WorldContainer World_Destination => ClusterManager.Instance.GetWorld(DestinationWorldID);

		public AxialI ClusterGridPosition()
		{
			return visualizer.Location;
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			traveler.getSpeedCB = GetSpeed;
			traveler.onTravelCB = OnTravellerMoved;
		}

		private void OnTravellerMoved()
		{
			Game.Instance.Trigger(-1975776133, (object)this);
		}

		protected override void OnCleanUp()
		{
			Components.LongRangeMissileTargetables.Remove(base.gameObject.GetComponent<ClusterGridEntity>());
			visualizer.Deselect();
			base.OnCleanUp();
		}

		public override void StartSM()
		{
			base.StartSM();
			if (DestinationWorldID < 0)
			{
				Setup(base.def.destinationWorldID, base.def.arrivalTime);
			}
			Components.LongRangeMissileTargetables.Add(base.gameObject.GetComponent<ClusterGridEntity>());
			RefreshVisuals();
		}

		public void RefreshVisuals(bool playIdentifyAnimationIfVisible = false)
		{
			selectable.SetName(base.def.name);
			descriptor.description = base.def.description;
			visualizer.PlayRevealAnimation(playIdentifyAnimationIfVisible);
			Trigger(1980521255);
		}

		public void Setup(int destinationWorldID, float arrivalTime)
		{
			DestinationWorldID = destinationWorldID;
			ArrivalTime = arrivalTime;
			AxialI location = World_Destination.GetComponent<ClusterGridEntity>().Location;
			destinationSelector.SetDestination(location);
			traveler.RevalidatePath(react_to_change: false);
			ClusterFogOfWarManager.Instance sMI = SaveGame.Instance.GetSMI<ClusterFogOfWarManager.Instance>();
			foreach (AxialI item in traveler.CurrentPath)
			{
				sMI.RevealLocation(item, 0, 0);
			}
			int count = traveler.CurrentPath.Count;
			float num = arrivalTime - GameUtil.GetCurrentTimeInCycles() * 600f;
			Speed = (float)count / num * 600f;
		}

		public float GetSpeed()
		{
			return Speed;
		}
	}

	public BoolParameter IsIdentified;

	public TravelingState traveling;

	public State arrived;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = traveling;
		traveling.DefaultState(traveling.unidentified).EventTransition(GameHashes.ClusterDestinationReached, arrived);
		traveling.unidentified.ParamTransition(IsIdentified, traveling.identified, GameStateMachine<ClusterMapLargeImpactor, Instance, IStateMachineTarget, Def>.IsTrue);
		traveling.identified.ParamTransition(IsIdentified, traveling.unidentified, GameStateMachine<ClusterMapLargeImpactor, Instance, IStateMachineTarget, Def>.IsFalse).ToggleStatusItem(Db.Get().MiscStatusItems.ClusterMeteorRemainingTravelTime);
		arrived.DoNothing();
	}
}
