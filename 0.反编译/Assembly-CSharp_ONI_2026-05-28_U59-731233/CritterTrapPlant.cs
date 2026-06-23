using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

public class CritterTrapPlant : StateMachineComponent<CritterTrapPlant.StatesInstance>, IPlantConsumeEntities
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, CritterTrapPlant, object>.GameInstance
	{
		[Serialize]
		public string lastConsumedEntityPrefabID;

		public string LastConsumedEntityName => string.IsNullOrEmpty(lastConsumedEntityPrefabID) ? "Unknown Critter" : Assets.GetPrefab(lastConsumedEntityPrefabID).GetProperName();

		public StatesInstance(CritterTrapPlant master)
			: base(master)
		{
		}

		public void OnTrapTriggered(object data)
		{
			base.smi.sm.trapTriggered.Trigger(base.smi);
		}

		public void AddGas(float dt)
		{
			float temperature = base.smi.GetComponent<PrimaryElement>().Temperature + base.smi.master.GAS_TEMPERATURE_DELTA;
			base.smi.master.storage.AddGasChunk(base.smi.master.outputElement, base.smi.master.gasOutputRate * dt, temperature, byte.MaxValue, 0, keep_zero_mass: false);
			if (ShouldVentGas())
			{
				base.smi.sm.ventGas.Trigger(base.smi);
			}
		}

		public void VentGas()
		{
			PrimaryElement primaryElement = base.smi.master.storage.FindPrimaryElement(base.smi.master.outputElement);
			if (primaryElement != null)
			{
				SimMessages.AddRemoveSubstance(Grid.PosToCell(base.smi.transform.GetPosition()), primaryElement.ElementID, CellEventLogger.Instance.Dumpable, primaryElement.Mass, primaryElement.Temperature, primaryElement.DiseaseIdx, primaryElement.DiseaseCount);
				base.smi.master.storage.ConsumeIgnoringDisease(primaryElement.gameObject);
			}
		}

		public bool ShouldVentGas()
		{
			PrimaryElement primaryElement = base.smi.master.storage.FindPrimaryElement(base.smi.master.outputElement);
			if (primaryElement == null)
			{
				return false;
			}
			return primaryElement.Mass >= base.smi.master.gasVentThreshold;
		}
	}

	public class States : GameStateMachine<States, StatesInstance, CritterTrapPlant>
	{
		public class DigestingStates : State
		{
			public State idle;

			public State vent_pre;

			public State vent;
		}

		public class TrapStates : State
		{
			public State open;

			public State trigger;

			public DigestingStates digesting;

			public State wilting;
		}

		public class FruitingStates : State
		{
			public State enter;

			public State idle;

			public State old;

			public State wilting;
		}

		public Signal trapTriggered;

		public Signal ventGas;

		public BoolParameter hasEatenCreature;

		public State dead;

		public FruitingStates fruiting;

		public State harvest;

		public TrapStates trap;

		public override void InitializeStates(out BaseState default_state)
		{
			base.serializable = SerializeType.Both_DEPRECATED;
			default_state = trap;
			trap.DefaultState(trap.open);
			trap.open.ToggleComponent<TrapTrigger>().ToggleStatusItem(Db.Get().CreatureStatusItems.CarnivorousPlantAwaitingVictim, (StatesInstance smi) => smi.master.GetComponent<IPlantConsumeEntities>()).Enter(delegate(StatesInstance smi)
			{
				smi.VentGas();
				smi.master.storage.ConsumeAllIgnoringDisease();
			})
				.EventHandler(GameHashes.TrapTriggered, delegate(StatesInstance smi, object data)
				{
					smi.OnTrapTriggered(data);
				})
				.EventTransition(GameHashes.Wilt, trap.wilting)
				.OnSignal(trapTriggered, trap.trigger)
				.ParamTransition(hasEatenCreature, trap.digesting, GameStateMachine<States, StatesInstance, CritterTrapPlant, object>.IsTrue)
				.PlayAnim("idle_open", KAnim.PlayMode.Loop);
			trap.trigger.PlayAnim("trap", KAnim.PlayMode.Once).Enter(delegate(StatesInstance smi)
			{
				GameObject gameObject = smi.master.storage.FindFirst(GameTags.Creature);
				smi.lastConsumedEntityPrefabID = ((gameObject != null) ? gameObject.PrefabID().ToString() : null);
				smi.master.storage.ConsumeAllIgnoringDisease();
				smi.sm.hasEatenCreature.Set(value: true, smi);
			}).OnAnimQueueComplete(trap.digesting);
			trap.digesting.PlayAnim("digesting_loop", KAnim.PlayMode.Loop).ToggleComponent<Growing>().EventTransition(GameHashes.Grow, fruiting.enter, (StatesInstance smi) => smi.master.growing.ReachedNextHarvest())
				.EventTransition(GameHashes.Wilt, trap.wilting)
				.DefaultState(trap.digesting.idle);
			trap.digesting.idle.PlayAnim("digesting_loop", KAnim.PlayMode.Loop).Update(delegate(StatesInstance smi, float dt)
			{
				smi.AddGas(dt);
			}, UpdateRate.SIM_4000ms).OnSignal(ventGas, trap.digesting.vent_pre);
			trap.digesting.vent_pre.PlayAnim("vent_pre").Exit(delegate(StatesInstance smi)
			{
				smi.VentGas();
			}).OnAnimQueueComplete(trap.digesting.vent);
			trap.digesting.vent.PlayAnim("vent_loop", KAnim.PlayMode.Once).QueueAnim("vent_pst").OnAnimQueueComplete(trap.digesting.idle);
			trap.wilting.PlayAnim("wilt1", KAnim.PlayMode.Loop).EventTransition(GameHashes.WiltRecover, trap, (StatesInstance smi) => !smi.master.wiltCondition.IsWilting());
			fruiting.EventTransition(GameHashes.Wilt, fruiting.wilting).EventTransition(GameHashes.Harvest, harvest).DefaultState(fruiting.idle);
			fruiting.enter.PlayAnim("open_harvest", KAnim.PlayMode.Once).Exit(delegate(StatesInstance smi)
			{
				smi.VentGas();
				smi.master.storage.ConsumeAllIgnoringDisease();
			}).OnAnimQueueComplete(fruiting.idle);
			fruiting.idle.PlayAnim("harvestable_loop", KAnim.PlayMode.Once).Enter(delegate(StatesInstance smi)
			{
				if (smi.master.harvestable != null)
				{
					smi.master.harvestable.SetCanBeHarvested(state: true);
				}
			}).Transition(fruiting.old, IsOld, UpdateRate.SIM_4000ms);
			fruiting.old.PlayAnim("wilt1", KAnim.PlayMode.Once).Enter(delegate(StatesInstance smi)
			{
				if (smi.master.harvestable != null)
				{
					smi.master.harvestable.SetCanBeHarvested(state: true);
				}
			}).Transition(fruiting.idle, GameStateMachine<States, StatesInstance, CritterTrapPlant, object>.Not(IsOld), UpdateRate.SIM_4000ms);
			fruiting.wilting.PlayAnim("wilt1", KAnim.PlayMode.Once).EventTransition(GameHashes.WiltRecover, fruiting, (StatesInstance smi) => !smi.master.wiltCondition.IsWilting());
			harvest.PlayAnim("harvest", KAnim.PlayMode.Once).Enter(delegate(StatesInstance smi)
			{
				if (GameScheduler.Instance != null && smi.master != null)
				{
					GameScheduler.Instance.Schedule("SpawnFruit", 0.2f, smi.master.crop.SpawnConfiguredFruit);
				}
				smi.master.harvestable.SetCanBeHarvested(state: false);
			}).Exit(delegate(StatesInstance smi)
			{
				smi.sm.hasEatenCreature.Set(value: false, smi);
			})
				.OnAnimQueueComplete(trap.open);
			dead.ToggleMainStatusItem(Db.Get().CreatureStatusItems.Dead).Enter(delegate(StatesInstance smi)
			{
				if (smi.master.rm.Replanted && !smi.master.GetComponent<KPrefabID>().HasTag(GameTags.Uprooted))
				{
					Notifier notifier = smi.master.gameObject.AddOrGet<Notifier>();
					Notification notification = smi.master.CreateDeathNotification();
					notifier.Add(notification);
				}
				GameUtil.KInstantiate(Assets.GetPrefab(EffectConfigs.PlantDeathId), smi.master.transform.GetPosition(), Grid.SceneLayer.FXFront).SetActive(value: true);
				Harvestable harvestable = smi.master.harvestable;
				if (harvestable != null && harvestable.CanBeHarvested && GameScheduler.Instance != null)
				{
					GameScheduler.Instance.Schedule("SpawnFruit", 0.2f, smi.master.crop.SpawnConfiguredFruit);
				}
				smi.master.Trigger(1623392196);
				smi.master.GetComponent<KBatchedAnimController>().StopAndClear();
				Object.Destroy(smi.master.GetComponent<KBatchedAnimController>());
				smi.Schedule(0.5f, smi.master.DestroySelf);
			});
		}

		public bool IsOld(StatesInstance smi)
		{
			return smi.master.growing.PercentOldAge() > 0.5f;
		}
	}

	private const string CONSUMED_ENTITY_NAME_FALLBACK = "Unknown Critter";

	[MyCmpReq]
	private Crop crop;

	[MyCmpReq]
	private WiltCondition wiltCondition;

	[MyCmpReq]
	private ReceptacleMonitor rm;

	[MyCmpReq]
	private Growing growing;

	[MyCmpReq]
	private KAnimControllerBase animController;

	[MyCmpReq]
	private Harvestable harvestable;

	[MyCmpReq]
	private Storage storage;

	public Tag[] CONSUMABLE_TAGs = new Tag[0];

	public float gasOutputRate;

	public float gasVentThreshold;

	public SimHashes outputElement;

	private float GAS_TEMPERATURE_DELTA = 10f;

	private static readonly EventSystem.IntraObjectHandler<CritterTrapPlant> OnUprootedDelegate = new EventSystem.IntraObjectHandler<CritterTrapPlant>(delegate(CritterTrapPlant component, object data)
	{
		component.OnUprooted(data);
	});

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.master.growing.enabled = false;
		Subscribe(-216549700, OnUprootedDelegate);
		base.smi.StartSM();
	}

	public void RefreshPositionPercent()
	{
		animController.SetPositionPercent(growing.PercentOfCurrentHarvest());
	}

	private void OnUprooted(object data = null)
	{
		GameUtil.KInstantiate(Assets.GetPrefab(EffectConfigs.PlantDeathId), base.gameObject.transform.GetPosition(), Grid.SceneLayer.FXFront).SetActive(value: true);
		base.gameObject.Trigger(1623392196);
		base.gameObject.GetComponent<KBatchedAnimController>().StopAndClear();
		Object.Destroy(base.gameObject.GetComponent<KBatchedAnimController>());
		Util.KDestroyGameObject(base.gameObject);
	}

	protected void DestroySelf(object callbackParam)
	{
		CreatureHelpers.DeselectCreature(base.gameObject);
		Util.KDestroyGameObject(base.gameObject);
	}

	public Notification CreateDeathNotification()
	{
		return new Notification(CREATURES.STATUSITEMS.PLANTDEATH.NOTIFICATION, NotificationType.Bad, (List<Notification> notificationList, object data) => string.Concat(CREATURES.STATUSITEMS.PLANTDEATH.NOTIFICATION_TOOLTIP, notificationList.ReduceMessages(countNames: false)), "/t• " + base.gameObject.GetProperName());
	}

	public string GetConsumableEntitiesCategoryName()
	{
		return CREATURES.SPECIES.CRITTERTRAPPLANT.VICTIM_IDENTIFIER;
	}

	public string GetRequirementText()
	{
		return CREATURES.SPECIES.CRITTERTRAPPLANT.PLANT_HUNGER_REQUIREMENT;
	}

	public bool AreEntitiesConsumptionRequirementsSatisfied()
	{
		return base.smi != null && base.smi.sm.hasEatenCreature.Get(base.smi);
	}

	public string GetConsumedEntityName()
	{
		return (base.smi == null) ? "Unknown Critter" : base.smi.LastConsumedEntityName;
	}

	public List<KPrefabID> GetPrefabsOfPossiblePrey()
	{
		List<GameObject> prefabsWithComponent = Assets.GetPrefabsWithComponent<CreatureBrain>();
		List<KPrefabID> list = new List<KPrefabID>();
		for (int i = 0; i < prefabsWithComponent.Count; i++)
		{
			KPrefabID component = prefabsWithComponent[i].GetComponent<KPrefabID>();
			if (!list.Contains(component) && IsEntityEdible(component) && Game.IsCorrectDlcActiveForCurrentSave(component))
			{
				list.Add(component);
			}
		}
		return list;
	}

	public string[] GetFormattedPossiblePreyList()
	{
		List<string> list = new List<string>();
		List<KPrefabID> prefabsOfPossiblePrey = GetPrefabsOfPossiblePrey();
		foreach (KPrefabID item2 in prefabsOfPossiblePrey)
		{
			CreatureBrain component = item2.GetComponent<CreatureBrain>();
			if (component != null)
			{
				Tag species = component.species;
				string item = species.ProperName();
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list.ToArray();
	}

	public bool IsEntityEdible(GameObject entity)
	{
		return IsEntityEdible(entity.GetComponent<KPrefabID>());
	}

	public bool IsEntityEdible(KPrefabID entity)
	{
		if (!entity.HasAnyTags(CONSUMABLE_TAGs))
		{
			return false;
		}
		if (!(entity.GetComponent<Trappable>() != null))
		{
			return false;
		}
		if (entity.GetComponent<OccupyArea>().OccupiedCellsOffsets.Length >= 3)
		{
			return false;
		}
		return true;
	}
}
