using System.Collections.Generic;
using UnityEngine;

public class StompMonitor : GameStateMachine<StompMonitor, StompMonitor.Instance, IStateMachineTarget, StompMonitor.Def>
{
	public class Def : BaseDef
	{
		public float Cooldown;

		public int radius = 10;

		private Navigator.Scanner<KPrefabID> plantSeeker;

		public Navigator.Scanner<KPrefabID> PlantSeeker
		{
			get
			{
				if (plantSeeker == null)
				{
					plantSeeker = new Navigator.Scanner<KPrefabID>(radius, GameScenePartitioner.Instance.plants, IsPlantTargetCandidate);
					plantSeeker.SetDynamicOffsetsFn(delegate(KPrefabID plant, List<CellOffset> offsets)
					{
						GetObjectCellsOffsetsWithExtraBottomPadding(plant.gameObject, offsets);
					});
				}
				return plantSeeker;
			}
		}

		private static bool IsPlantTargetCandidate(KPrefabID plant)
		{
			if (plant == null)
			{
				return false;
			}
			if (plant.pendingDestruction)
			{
				return false;
			}
			if (plant.HasTag(ReservedForStomp))
			{
				return false;
			}
			if (!plant.HasTag(GameTags.GrowingPlant))
			{
				return false;
			}
			return plant.HasTag(GameTags.FullyGrown);
		}

		public static void GetObjectCellsOffsetsWithExtraBottomPadding(GameObject obj, List<CellOffset> offsets)
		{
			OccupyArea component = obj.GetComponent<OccupyArea>();
			int widthInCells = component.GetWidthInCells();
			int num = int.MaxValue;
			int num2 = int.MaxValue;
			for (int i = 0; i < component.OccupiedCellsOffsets.Length; i++)
			{
				CellOffset item = component.OccupiedCellsOffsets[i];
				offsets.Add(item);
				num = Mathf.Min(num, item.x);
				num2 = Mathf.Min(num2, item.y);
			}
			for (int j = 0; j < widthInCells; j++)
			{
				CellOffset item2 = new CellOffset(num + j, num2 - 1);
				offsets.Add(item2);
			}
		}
	}

	public class StompBehaviourStates : State
	{
		public State lookingForTarget;

		public State stomping;
	}

	public new class Instance : GameInstance
	{
		public GameObject Target => base.sm.TargetPlant.Get(this);

		public float TimeSinceLastStomp => base.sm.TimeSinceLastStomp.Get(this);

		public Navigator Navigator { get; private set; }

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			Navigator = GetComponent<Navigator>();
		}

		public void LookForTarget()
		{
			KPrefabID value = base.def.PlantSeeker.Scan(Grid.PosToXY(base.transform.GetPosition()), Navigator);
			base.sm.TargetPlant.Set(value, this);
		}
	}

	public static readonly Tag ReservedForStomp = GameTags.Creatures.ReservedByCreature;

	public State cooldown;

	public StompBehaviourStates stomp;

	public FloatParameter TimeSinceLastStomp = new FloatParameter(float.MaxValue);

	public TargetParameter TargetPlant;

	public Signal StompStateFailed;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = cooldown;
		cooldown.ParamTransition(TimeSinceLastStomp, stomp, IsTimeToStomp).Update(CooldownTick);
		stomp.ParamTransition(TimeSinceLastStomp, cooldown, GameStateMachine<StompMonitor, Instance, IStateMachineTarget, Def>.IsLTEZero).DefaultState(stomp.lookingForTarget);
		stomp.lookingForTarget.ParamTransition(TargetPlant, stomp.stomping, GameStateMachine<StompMonitor, Instance, IStateMachineTarget, Def>.IsNotNull).PreBrainUpdate(LookForTarget);
		stomp.stomping.Enter(ReservePlant).OnSignal(StompStateFailed, stomp.lookingForTarget).ToggleBehaviour(GameTags.Creatures.WantsToStomp, (Instance smi) => smi.Target != null, OnStompCompleted)
			.Exit(UnreserveAndClearPlantTarget);
	}

	private static void ReservePlant(Instance smi)
	{
		smi.Target.AddTag(ReservedForStomp);
	}

	private static bool IsTimeToStomp(Instance smi, float timeSinceLastStomp)
	{
		return timeSinceLastStomp > smi.def.Cooldown;
	}

	private static void CooldownTick(Instance smi, float dt)
	{
		smi.sm.TimeSinceLastStomp.Set(smi.TimeSinceLastStomp + dt, smi);
	}

	private static void OnStompCompleted(Instance smi)
	{
		smi.sm.TimeSinceLastStomp.Set(0f, smi);
	}

	private static void LookForTarget(Instance smi)
	{
		smi.LookForTarget();
	}

	private static void UnreserveAndClearPlantTarget(Instance smi)
	{
		if (smi.Target != null)
		{
			smi.Target.RemoveTag(ReservedForStomp);
		}
		smi.sm.TargetPlant.Set(null, smi);
	}
}
