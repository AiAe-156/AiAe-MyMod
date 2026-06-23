using Klei.AI;
using UnityEngine;

public class PollinateMonitor : GameStateMachine<PollinateMonitor, PollinateMonitor.Instance, IStateMachineTarget, PollinateMonitor.Def>
{
	public class Def : BaseDef
	{
		public int radius = 10;

		private Navigator.Scanner<KPrefabID> plantSeeker;

		public Navigator.Scanner<KPrefabID> PlantSeeker
		{
			get
			{
				if (plantSeeker == null)
				{
					plantSeeker = new Navigator.Scanner<KPrefabID>(radius, GameScenePartitioner.Instance.plants, IsHarvestablePlant);
					plantSeeker.SetEarlyOutThreshold(5);
				}
				return plantSeeker;
			}
		}

		private static bool IsHarvestablePlant(KPrefabID plant)
		{
			if (plant == null)
			{
				return false;
			}
			if (plant.HasTag(GameTags.Creatures.ReservedByCreature))
			{
				return false;
			}
			if (plant.HasTag("ButterflyPlant"))
			{
				return false;
			}
			if (!plant.HasTag(GameTags.GrowingPlant))
			{
				return false;
			}
			if (plant.HasTag(GameTags.FullyGrown))
			{
				return false;
			}
			Effects component = plant.GetComponent<Effects>();
			if (component == null)
			{
				return false;
			}
			for (int i = 0; i < PollinationMonitor.PollinationEffects.Length; i++)
			{
				HashedString effect_id = PollinationMonitor.PollinationEffects[i];
				if (component.HasEffect(effect_id))
				{
					return false;
				}
			}
			return true;
		}
	}

	public new class Instance : GameInstance, IApproachableBehaviour, ICreatureMonitor
	{
		public GameObject target;

		public int targetCell;

		public Navigator navigator;

		public Tag Id => ID;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			navigator = master.GetComponent<Navigator>();
		}

		public bool IsValidTarget()
		{
			if (!target.IsNullOrDestroyed())
			{
				return navigator.GetNavigationCost(targetCell) != -1;
			}
			return false;
		}

		public GameObject GetTarget()
		{
			return target;
		}

		public StatusItem GetApproachStatusItem()
		{
			return Db.Get().CreatureStatusItems.TravelingToPollinate;
		}

		public StatusItem GetBehaviourStatusItem()
		{
			return Db.Get().CreatureStatusItems.Pollinating;
		}

		public void OnSuccess()
		{
			Effects component = target.GetComponent<Effects>();
			if (component != null)
			{
				component.Add(Db.Get().effects.Get("ButterflyPollinated"), should_save: true);
			}
			target = null;
		}
	}

	public static Tag ID = new Tag("PollinateMonitor");

	public State lookingForPlant;

	public State satisfied;

	private FloatParameter remainingSecondsForEffect;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = lookingForPlant;
		base.serializable = SerializeType.ParamsOnly;
		lookingForPlant.PreBrainUpdate(FindPollinateTarget).ToggleBehaviour(GameTags.Creatures.WantsToPollinate, (Instance smi) => smi.IsValidTarget(), delegate(Instance smi)
		{
			smi.GoTo(satisfied);
		});
		satisfied.Enter(delegate(Instance smi)
		{
			remainingSecondsForEffect.Set(ButterflyTuning.SEARCH_COOLDOWN, smi);
		}).ScheduleGoTo((Instance smi) => remainingSecondsForEffect.Get(smi), lookingForPlant);
	}

	private static void FindPollinateTarget(Instance smi)
	{
		if (smi.IsValidTarget())
		{
			return;
		}
		KPrefabID kPrefabID = smi.def.PlantSeeker.Scan(Grid.PosToXY(smi.transform.GetPosition()), smi.navigator);
		GameObject gameObject = ((kPrefabID != null) ? kPrefabID.gameObject : null);
		if (gameObject != smi.target)
		{
			if (gameObject == null)
			{
				smi.target = null;
				smi.targetCell = -1;
			}
			else
			{
				smi.target = gameObject;
				smi.targetCell = Grid.PosToCell(smi.target);
			}
			smi.Trigger(-255880159);
		}
	}
}
