using System.Collections.Generic;
using KSerialization;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class FlytrapConsumptionMonitor : StateMachineComponent<FlytrapConsumptionMonitor.Instance>, IGameObjectEffectDescriptor, IPlantConsumeEntities
{
	public class States : GameStateMachine<States, Instance, FlytrapConsumptionMonitor>
	{
		public class HungryStates : State
		{
			public State wilt;

			public State idle;

			public State complete;
		}

		public HungryStates hungry;

		public State satisfied;

		public BoolParameter HasEaten;

		public Signal EatSignal;

		public override void InitializeStates(out BaseState default_state)
		{
			base.serializable = SerializeType.ParamsOnly;
			default_state = hungry;
			hungry.ParamTransition(HasEaten, satisfied, GameStateMachine<States, FlytrapConsumptionMonitor.Instance, FlytrapConsumptionMonitor, object>.IsTrue).Toggle("Toggle Standard Crop Plant Animations", SetCropPlantAnimationsToAwaitPrey, RestoreDefaultCropPlantAnimations).ToggleAttributeModifier("Pause Growing", (FlytrapConsumptionMonitor.Instance smi) => smi.pauseGrowing)
				.DefaultState(hungry.idle);
			hungry.idle.EventTransition(GameHashes.Wilt, hungry.wilt, IsWilted).ToggleStatusItem(Db.Get().CreatureStatusItems.CarnivorousPlantAwaitingVictim, (FlytrapConsumptionMonitor.Instance smi) => smi.master.GetComponent<IPlantConsumeEntities>()).Enter(RegisterVictimProximityMonitor)
				.TriggerOnEnter(GameHashes.CropSleep)
				.OnSignal(EatSignal, hungry.complete)
				.Exit(UnregisterVictimProximityMonitor);
			hungry.complete.Enter(SetAndPlayConsumeCropPlantAnimations).Enter(CompleteEat);
			hungry.wilt.EventTransition(GameHashes.WiltRecover, hungry.idle, GameStateMachine<States, FlytrapConsumptionMonitor.Instance, FlytrapConsumptionMonitor, object>.Not(IsWilted));
			satisfied.Enter(RetriggerGrowAnimationIfInGrowState).TriggerOnEnter(GameHashes.CropWakeUp).ParamTransition(HasEaten, hungry, GameStateMachine<States, FlytrapConsumptionMonitor.Instance, FlytrapConsumptionMonitor, object>.IsFalse)
				.EventHandler(GameHashes.Harvest, BecomeHungry);
		}
	}

	public class Instance : GameStateMachine<States, Instance, FlytrapConsumptionMonitor, object>.GameInstance
	{
		public AttributeModifier pauseGrowing;

		[Serialize]
		private string lastConsumedEntityPrefabID;

		private Growing growing;

		private WiltCondition wiltCondition;

		private AmountInstance maturity;

		private HandleVector<int>.Handle partitionerEntry = HandleVector<int>.InvalidHandle;

		public bool HasEaten => base.sm.HasEaten.Get(this);

		public bool IsWilted => wiltCondition.IsWilting();

		public string LastConsumedEntityName
		{
			get
			{
				if (!string.IsNullOrEmpty(lastConsumedEntityPrefabID))
				{
					return Assets.GetPrefab(lastConsumedEntityPrefabID).GetProperName();
				}
				return "Unknown Critter";
			}
		}

		public Instance(FlytrapConsumptionMonitor master)
			: base(master)
		{
			Amounts amounts = base.gameObject.GetAmounts();
			maturity = amounts.Get(Db.Get().Amounts.Maturity);
			pauseGrowing = new AttributeModifier(maturity.deltaAttribute.Id, -1f, CREATURES.SPECIES.FLYTRAPPLANT.HUNGRY, is_multiplier: true);
			wiltCondition = GetComponent<WiltCondition>();
			growing = GetComponent<Growing>();
			growing.CustomGrowStallCondition_IsStalled = ShouldStallGrowingComponent;
		}

		private bool ShouldStallGrowingComponent(GameObject plantGameObject)
		{
			return !HasEaten;
		}

		public void RegisterVictimProximityMonitor()
		{
			OccupyArea component = GetComponent<OccupyArea>();
			partitionerEntry = GameScenePartitioner.Instance.Add("FlytrapConsumptionMonitor.hungry.idle", base.gameObject, component.GetExtents(), GameScenePartitioner.Instance.pickupablesChangedLayer, OnPickupableLayerObjectDetected);
		}

		public void UnregisterVictimProximityMonitor()
		{
			GameScenePartitioner.Instance.Free(ref partitionerEntry);
			partitionerEntry = HandleVector<int>.InvalidHandle;
		}

		public void OnPickupableLayerObjectDetected(object obj)
		{
			Pickupable pickupable = obj as Pickupable;
			if (base.master.IsEntityEdible(pickupable.gameObject))
			{
				lastConsumedEntityPrefabID = pickupable.PrefabID().ToString();
				pickupable.gameObject.DeleteObject();
				base.sm.EatSignal.Trigger(this);
			}
		}
	}

	public const string AWAIT_PREY_ANIM_NAME = "awaiting_prey";

	public const string EAT_ANIM_NAME = "consume";

	private const string CONSUMED_ENTITY_NAME_FALLBACK = "Unknown Critter";

	private static Tag CONSUMABLE_TAG = GameTags.Creatures.Flyer;

	public static readonly StandardCropPlant.AnimSet HUNGRY_STATE_ANIM_SET = new StandardCropPlant.AnimSet(FlyTrapPlantConfig.Default_StandardCropAnimSet)
	{
		grow = "awaiting_prey",
		wilt_base = "flower_wilt",
		grow_playmode = KAnim.PlayMode.Loop
	};

	public static readonly StandardCropPlant.AnimSet EATING_STATE_ANIM_SET = new StandardCropPlant.AnimSet(FlyTrapPlantConfig.Default_StandardCropAnimSet)
	{
		pre_grow = "consume",
		grow_playmode = KAnim.PlayMode.Paused
	};

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}

	public string GetConsumableEntitiesCategoryName()
	{
		return CREATURES.SPECIES.FLYTRAPPLANT.VICTIM_IDENTIFIER;
	}

	public bool AreEntitiesConsumptionRequirementsSatisfied()
	{
		if (base.smi != null)
		{
			return base.smi.HasEaten;
		}
		return false;
	}

	public string GetRequirementText()
	{
		return CREATURES.SPECIES.FLYTRAPPLANT.PLANT_HUNGER_REQUIREMENT;
	}

	public string GetConsumedEntityName()
	{
		if (base.smi != null)
		{
			return base.smi.LastConsumedEntityName;
		}
		return "Unknown Critter";
	}

	public List<KPrefabID> GetPrefabsOfPossiblePrey()
	{
		List<GameObject> prefabsWithTag = Assets.GetPrefabsWithTag(CONSUMABLE_TAG);
		List<KPrefabID> list = new List<KPrefabID>();
		for (int i = 0; i < prefabsWithTag.Count; i++)
		{
			KPrefabID component = prefabsWithTag[i].GetComponent<KPrefabID>();
			if (IsEntityEdible(component) && !list.Contains(component) && Game.IsCorrectDlcActiveForCurrentSave(component))
			{
				list.Add(component);
			}
		}
		return list;
	}

	public string[] GetFormattedPossiblePreyList()
	{
		List<string> list = new List<string>();
		foreach (KPrefabID item2 in GetPrefabsOfPossiblePrey())
		{
			CreatureBrain component = item2.GetComponent<CreatureBrain>();
			if (component != null)
			{
				string item = component.species.ProperName();
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
		if (entity == null)
		{
			return false;
		}
		return IsEntityEdible(entity.GetComponent<KPrefabID>());
	}

	public bool IsEntityEdible(KPrefabID entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (!entity.HasTag(CONSUMABLE_TAG))
		{
			return false;
		}
		if (!(entity.GetComponent<CreatureBrain>() != null))
		{
			return false;
		}
		return entity.GetComponent<OccupyArea>().OccupiedCellsOffsets.Length <= 1;
	}

	public List<Descriptor> GetDescriptors(GameObject obj)
	{
		return new List<Descriptor>
		{
			new Descriptor(GetRequirementText(), "", Descriptor.DescriptorType.Requirement)
		};
	}

	public static bool IsWilted(Instance smi)
	{
		return smi.IsWilted;
	}

	public static void CompleteEat(Instance smi)
	{
		smi.sm.HasEaten.Set(value: true, smi);
	}

	public static void RetriggerGrowAnimationIfInGrowState(Instance smi)
	{
		StandardCropPlant component = smi.GetComponent<StandardCropPlant>();
		if (!(component == null) && component.smi != null && component.smi.IsInsideState(component.smi.sm.alive.idle))
		{
			KBatchedAnimController component2 = smi.GetComponent<KBatchedAnimController>();
			if (component2 != null)
			{
				component2.Play(component.anims.grow, component.anims.grow_playmode);
			}
		}
	}

	public static void BecomeHungry(Instance smi)
	{
		smi.sm.HasEaten.Set(value: false, smi);
	}

	public static void RegisterVictimProximityMonitor(Instance smi)
	{
		smi.RegisterVictimProximityMonitor();
	}

	public static void UnregisterVictimProximityMonitor(Instance smi)
	{
		smi.UnregisterVictimProximityMonitor();
	}

	public static void SetAndPlayConsumeCropPlantAnimations(Instance smi)
	{
		StandardCropPlant component = smi.GetComponent<StandardCropPlant>();
		if (!(component == null) && component.smi != null)
		{
			component.anims = EATING_STATE_ANIM_SET;
			component.smi.GoTo(component.smi.sm.alive.pre_idle);
		}
	}

	public static void SetCropPlantAnimationsToAwaitPrey(Instance smi)
	{
		SetCropPlantAnimationSet(smi, HUNGRY_STATE_ANIM_SET);
		RetriggerGrowAnimationIfInGrowState(smi);
		StandardCropPlant component = smi.GetComponent<StandardCropPlant>();
		if (!(component == null) && component.smi != null)
		{
			component.preventGrowPositionUpdate = true;
		}
	}

	public static void RestoreDefaultCropPlantAnimations(Instance smi)
	{
		SetCropPlantAnimationSet(smi, FlyTrapPlantConfig.Default_StandardCropAnimSet);
		StandardCropPlant component = smi.GetComponent<StandardCropPlant>();
		if (!(component == null) && component.smi != null)
		{
			component.preventGrowPositionUpdate = false;
		}
	}

	private static void SetCropPlantAnimationSet(Instance smi, StandardCropPlant.AnimSet set)
	{
		StandardCropPlant component = smi.GetComponent<StandardCropPlant>();
		if (!(component == null) && component.smi != null)
		{
			component.anims = set;
		}
	}
}
