using System;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;

public class ClusterMapResourceMeteor : GameStateMachine<ClusterMapResourceMeteor, ClusterMapResourceMeteor.Instance, IStateMachineTarget, ClusterMapResourceMeteor.Def>
{
	public class Def : BaseDef, IGameObjectEffectDescriptor
	{
		public string name;

		public string description;

		public string description_Hidden;

		public string name_Hidden;

		public string eventID;

		private AxialI destination;

		public float arrivalTime;

		public List<Descriptor> GetDescriptors(GameObject go)
		{
			return new List<Descriptor>();
		}
	}

	public class TravelingState : State
	{
		public State unidentified;

		public State identified;
	}

	public new class Instance : GameInstance
	{
		[Serialize]
		public AxialI Destination;

		[Serialize]
		public float ArrivalTime;

		[Serialize]
		private float Speed = 0f;

		[Serialize]
		private float identifyingProgress = 0f;

		public System.Action OnDestinationReached;

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

		public bool HasBeenIdentified => base.sm.IsIdentified.Get(this);

		public float IdentifyingProgress => identifyingProgress;

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
			visualizer.Deselect();
			base.OnCleanUp();
		}

		public void Identify()
		{
			if (!HasBeenIdentified)
			{
				identifyingProgress = 1f;
				base.sm.IsIdentified.Set(value: true, this);
				Game.Instance.Trigger(1427028915, (object)this);
				RefreshVisuals(playIdentifyAnimationIfVisible: true);
				if (ClusterMapScreen.Instance.IsActive())
				{
					KFMOD.PlayUISound(GlobalAssets.GetSound("ClusterMapMeteor_Reveal"));
				}
			}
		}

		public void ProgressIdentifiction(float points)
		{
			if (!HasBeenIdentified)
			{
				identifyingProgress += points;
				identifyingProgress = Mathf.Clamp(identifyingProgress, 0f, 1f);
				if (identifyingProgress == 1f)
				{
					Identify();
				}
			}
		}

		public override void StartSM()
		{
			base.StartSM();
			RefreshVisuals();
		}

		public void RefreshVisuals(bool playIdentifyAnimationIfVisible = false)
		{
			if (HasBeenIdentified)
			{
				selectable.SetName(base.def.name);
				descriptor.description = base.def.description;
				visualizer.PlayRevealAnimation(playIdentifyAnimationIfVisible);
			}
			else
			{
				selectable.SetName(base.def.name_Hidden);
				descriptor.description = base.def.description_Hidden;
				visualizer.PlayHideAnimation();
			}
			Trigger(1980521255);
		}

		public void Setup(AxialI destination, float arrivalTime)
		{
			Destination = destination;
			ArrivalTime = arrivalTime;
			destinationSelector.SetDestination(destination);
			traveler.RevalidatePath(react_to_change: false);
			int count = traveler.CurrentPath.Count;
			float num = arrivalTime - GameUtil.GetCurrentTimeInCycles() * 600f;
			Speed = (float)count / num * 600f;
		}

		public float GetSpeed()
		{
			return Speed;
		}

		public void DestinationReached()
		{
			OnDestinationReached?.Invoke();
		}
	}

	public BoolParameter IsIdentified;

	public TravelingState traveling;

	public State leaving;

	public State left;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = traveling;
		traveling.DefaultState(traveling.unidentified).EventTransition(GameHashes.ClusterDestinationReached, leaving);
		traveling.unidentified.ParamTransition(IsIdentified, traveling.identified, GameStateMachine<ClusterMapResourceMeteor, Instance, IStateMachineTarget, Def>.IsTrue);
		traveling.identified.ParamTransition(IsIdentified, traveling.unidentified, GameStateMachine<ClusterMapResourceMeteor, Instance, IStateMachineTarget, Def>.IsFalse).ToggleStatusItem(Db.Get().MiscStatusItems.ClusterMeteorRemainingTravelTime);
		leaving.Enter(DestinationReached);
	}

	public static void DestinationReached(Instance smi)
	{
		smi.DestinationReached();
		Util.KDestroyGameObject(smi.gameObject);
	}
}
