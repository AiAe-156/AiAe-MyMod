using System.Collections.Generic;
using UnityEngine;

public class ShakeHarvestMonitor : GameStateMachine<ShakeHarvestMonitor, ShakeHarvestMonitor.Instance, IStateMachineTarget, ShakeHarvestMonitor.Def>
{
	public class Def : BaseDef
	{
		public float cooldownDuration;

		public HashSet<Tag> harvestablePlants = new HashSet<Tag>();

		public int radius = 10;

		private Navigator.Scanner<KPrefabID> plantSeeker;

		public Navigator.Scanner<KPrefabID> PlantSeeker
		{
			get
			{
				if (plantSeeker == null)
				{
					plantSeeker = new Navigator.Scanner<KPrefabID>(radius, GameScenePartitioner.Instance.plants, IsHarvestablePlant);
					plantSeeker.SetDynamicOffsetsFn(delegate(KPrefabID plant, List<CellOffset> offsets)
					{
						GetApproachOffsets(plant.gameObject, offsets);
					});
				}
				return plantSeeker;
			}
		}

		private bool IsHarvestablePlant(KPrefabID plant)
		{
			if (plant == null)
			{
				return false;
			}
			if (plant.pendingDestruction)
			{
				return false;
			}
			if (plant.HasTag(Reserved))
			{
				return false;
			}
			if (!harvestablePlants.Contains(plant.PrefabID()))
			{
				return false;
			}
			Harvestable component = plant.GetComponent<Harvestable>();
			if (component == null)
			{
				return false;
			}
			if (!component.CanBeHarvested)
			{
				return false;
			}
			return true;
		}

		public static void GetApproachOffsets(GameObject plant, List<CellOffset> offsets)
		{
			OccupyArea component = plant.GetComponent<OccupyArea>();
			Extents extents = component.GetExtents();
			int x = -1;
			int width = extents.width;
			for (int i = 0; i != extents.height; i++)
			{
				int y = i;
				offsets.Add(new CellOffset(x, y));
				offsets.Add(new CellOffset(width, y));
			}
		}
	}

	public class HarvestStates : State
	{
		public State seek;

		public State execute;
	}

	public new class Instance : GameInstance
	{
		private readonly Navigator navigator;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			navigator = GetComponent<Navigator>();
		}

		public KPrefabID Seek()
		{
			return base.def.PlantSeeker.Scan(Grid.PosToXY(base.smi.transform.GetPosition()), navigator);
		}
	}

	public static readonly Tag Reserved = GameTags.Creatures.ReservedByCreature;

	public State cooldown;

	public HarvestStates harvest;

	public FloatParameter elapsedTime = new FloatParameter(float.MaxValue);

	public TargetParameter plant;

	public Signal failed;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = cooldown;
		cooldown.Update(delegate(Instance smi, float dt)
		{
			elapsedTime.Set(elapsedTime.Get(smi) + dt, smi);
		}).ParamTransition(elapsedTime, harvest, (Instance smi, float elapsedTime) => elapsedTime > smi.def.cooldownDuration);
		harvest.DefaultState(harvest.seek).ParamTransition(elapsedTime, cooldown, GameStateMachine<ShakeHarvestMonitor, Instance, IStateMachineTarget, Def>.IsLTEZero);
		harvest.seek.PreBrainUpdate(delegate(Instance smi)
		{
			plant.Set(smi.Seek(), smi);
		}).ParamTransition(plant, harvest.execute, GameStateMachine<ShakeHarvestMonitor, Instance, IStateMachineTarget, Def>.IsNotNull);
		harvest.execute.Enter(delegate(Instance smi)
		{
			plant.Get(smi).AddTag(Reserved);
		}).OnSignal(failed, harvest.seek).ToggleBehaviour(GameTags.Creatures.WantsToHarvest, (Instance smi) => plant.Get(smi) != null, delegate(Instance smi)
		{
			elapsedTime.Set(0f, smi);
		})
			.Exit(delegate(Instance smi)
			{
				GameObject gameObject = plant.Get(smi);
				if (gameObject != null)
				{
					gameObject.RemoveTag(Reserved);
					plant.Set(null, smi);
				}
			});
	}
}
