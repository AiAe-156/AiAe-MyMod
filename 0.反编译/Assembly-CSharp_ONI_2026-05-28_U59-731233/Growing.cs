using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TemplateClasses;
using UnityEngine;

public class Growing : StateMachineComponent<Growing.StatesInstance>, IGameObjectEffectDescriptor, IManageGrowingStates
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, Growing, object>.GameInstance
	{
		public AttributeModifier baseGrowingRate;

		public AttributeModifier wildGrowingRate;

		public AttributeModifier getOldRate;

		public Harvestable harvestable;

		public StatesInstance(Growing master)
			: base(master)
		{
			baseGrowingRate = new AttributeModifier(master.maturity.deltaAttribute.Id, master.GROWTH_RATE, CREATURES.STATS.MATURITY.GROWING);
			wildGrowingRate = new AttributeModifier(master.maturity.deltaAttribute.Id, master.WILD_GROWTH_RATE, CREATURES.STATS.MATURITY.GROWINGWILD);
			getOldRate = new AttributeModifier(master.oldAge.deltaAttribute.Id, master.shouldGrowOld ? 1f : 0f);
			harvestable = GetComponent<Harvestable>();
		}

		public bool IsGrown()
		{
			return base.master.IsGrown();
		}

		public bool ReachedNextHarvest()
		{
			return base.master.ReachedNextHarvest();
		}

		public void ClampGrowthToHarvest()
		{
			base.master.ClampGrowthToHarvest();
		}

		public bool IsWilting()
		{
			return base.master.wiltCondition != null && base.master.wiltCondition.IsWilting();
		}

		public bool IsStalledByCustomCondition()
		{
			bool result = false;
			if (base.master.CustomGrowStallCondition_IsStalled != null)
			{
				result = base.master.CustomGrowStallCondition_IsStalled(base.master.gameObject);
			}
			return result;
		}

		public bool CanExitStalled()
		{
			return !IsWilting() && (base.master.CustomGrowStallCondition_IsStalled == null || !base.master.CustomGrowStallCondition_IsStalled(base.master.gameObject));
		}
	}

	public class States : GameStateMachine<States, StatesInstance, Growing>
	{
		public class GrowingStates : State
		{
			public State wild;

			public State planted;
		}

		public class GrownStates : State
		{
			public State idle;
		}

		public GrowingStates growing;

		public State stalled;

		public GrownStates grown;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = growing;
			base.serializable = SerializeType.Both_DEPRECATED;
			growing.EventTransition(GameHashes.Wilt, stalled, (StatesInstance smi) => smi.IsWilting()).EventTransition(GameHashes.CropSleep, stalled, (StatesInstance smi) => smi.IsStalledByCustomCondition()).EventTransition(GameHashes.ReceptacleMonitorChange, growing.planted, (StatesInstance smi) => !smi.master.IsWildPlanted())
				.EventTransition(GameHashes.ReceptacleMonitorChange, growing.wild, (StatesInstance smi) => smi.master.IsWildPlanted())
				.EventTransition(GameHashes.PlanterStorage, growing.planted, (StatesInstance smi) => !smi.master.IsWildPlanted())
				.EventTransition(GameHashes.PlanterStorage, growing.wild, (StatesInstance smi) => smi.master.IsWildPlanted())
				.TriggerOnEnter(GameHashes.Grow)
				.Update("CheckGrown", delegate(StatesInstance smi, float dt)
				{
					if (smi.ReachedNextHarvest())
					{
						smi.GoTo(grown);
					}
				}, UpdateRate.SIM_4000ms)
				.ToggleStatusItem(Db.Get().CreatureStatusItems.Growing, (StatesInstance smi) => smi.master.GetComponent<IManageGrowingStates>())
				.Enter(delegate(StatesInstance smi)
				{
					State state = (smi.master.IsWildPlanted() ? growing.wild : growing.planted);
					smi.GoTo(state);
				});
			growing.wild.ToggleAttributeModifier("GrowingWild", (StatesInstance smi) => smi.wildGrowingRate);
			growing.planted.ToggleAttributeModifier("Growing", (StatesInstance smi) => smi.baseGrowingRate);
			stalled.EventTransition(GameHashes.WiltRecover, growing, (StatesInstance smi) => smi.CanExitStalled()).EventTransition(GameHashes.CropWakeUp, growing, (StatesInstance smi) => smi.CanExitStalled());
			grown.DefaultState(grown.idle).TriggerOnEnter(GameHashes.Grow).Update("CheckNotGrown", delegate(StatesInstance smi, float dt)
			{
				if (!smi.ReachedNextHarvest())
				{
					smi.GoTo(growing);
				}
			}, UpdateRate.SIM_4000ms)
				.ToggleAttributeModifier("GettingOld", (StatesInstance smi) => smi.getOldRate)
				.Enter(delegate(StatesInstance smi)
				{
					smi.ClampGrowthToHarvest();
				})
				.Exit(delegate(StatesInstance smi)
				{
					smi.master.oldAge.SetValue(0f);
				});
			grown.idle.Update("CheckNotGrown", delegate(StatesInstance smi, float dt)
			{
				if (smi.master.shouldGrowOld && smi.master.oldAge.value >= smi.master.oldAge.GetMax() && (bool)smi.harvestable && smi.harvestable.CanBeHarvested)
				{
					if (smi.harvestable.harvestDesignatable != null)
					{
						bool harvestWhenReady = smi.harvestable.harvestDesignatable.HarvestWhenReady;
						smi.harvestable.ForceCancelHarvest();
						smi.harvestable.Harvest();
						if (harvestWhenReady && smi.harvestable != null)
						{
							smi.harvestable.harvestDesignatable.SetHarvestWhenReady(state: true);
						}
					}
					else
					{
						smi.harvestable.ForceCancelHarvest();
						smi.harvestable.Harvest();
					}
					smi.master.maturity.SetValue(0f);
					smi.master.oldAge.SetValue(0f);
				}
			}, UpdateRate.SIM_4000ms);
		}
	}

	public Func<GameObject, bool> CustomGrowStallCondition_IsStalled = null;

	public float MaxMaturityValuePercentageToSpawnWith = 1f;

	public float GROWTH_RATE = 0.0016666667f;

	public float WILD_GROWTH_RATE = 0.00041666668f;

	public bool shouldGrowOld = true;

	public float maxAge = 2400f;

	private AmountInstance maturity;

	private AmountInstance oldAge;

	[MyCmpGet]
	private WiltCondition wiltCondition;

	[MyCmpReq]
	private KSelectable selectable;

	[MyCmpReq]
	private Modifiers modifiers;

	[MyCmpReq]
	private ReceptacleMonitor rm;

	private static readonly EventSystem.IntraObjectHandler<Growing> OnNewGameSpawnDelegate = new EventSystem.IntraObjectHandler<Growing>(delegate(Growing component, object data)
	{
		component.OnNewGameSpawn(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Growing> ResetGrowthDelegate = new EventSystem.IntraObjectHandler<Growing>(delegate(Growing component, object data)
	{
		component.ResetGrowth(data);
	});

	protected override void OnPrefabInit()
	{
		Amounts amounts = base.gameObject.GetAmounts();
		maturity = amounts.Get(Db.Get().Amounts.Maturity);
		oldAge = amounts.Add(new AmountInstance(Db.Get().Amounts.OldAge, base.gameObject));
		oldAge.maxAttribute.ClearModifiers();
		oldAge.maxAttribute.Add(new AttributeModifier(Db.Get().Amounts.OldAge.maxAttribute.Id, maxAge));
		base.OnPrefabInit();
		Subscribe(1119167081, OnNewGameSpawnDelegate);
		Subscribe(1272413801, ResetGrowthDelegate);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
		base.gameObject.AddTag(GameTags.GrowingPlant);
	}

	private void OnNewGameSpawn(object data)
	{
		Prefab prefab = (Prefab)data;
		if (prefab.amounts != null)
		{
			Prefab.template_amount_value[] amounts = prefab.amounts;
			foreach (Prefab.template_amount_value template_amount_value in amounts)
			{
				if (template_amount_value.id == maturity.amount.Id && template_amount_value.value == GetMaxMaturity())
				{
					return;
				}
			}
		}
		if (maturity == null)
		{
			KCrashReporter.ReportDevNotification("Maturity.OnNewGameSpawn", Environment.StackTrace);
		}
		maturity.SetValue(maturity.maxAttribute.GetTotalValue() * MaxMaturityValuePercentageToSpawnWith * UnityEngine.Random.Range(0f, 1f));
	}

	public void OverrideMaturityLevel(float percent)
	{
		float value = maturity.GetMax() * percent;
		maturity.SetValue(value);
	}

	public bool ReachedNextHarvest()
	{
		return PercentOfCurrentHarvest() >= 1f;
	}

	public bool IsGrown()
	{
		return maturity.value == maturity.GetMax();
	}

	public bool CanGrow()
	{
		return !IsGrown();
	}

	public bool IsGrowing()
	{
		return maturity.GetDelta() > 0f;
	}

	public void ClampGrowthToHarvest()
	{
		maturity.value = maturity.GetMax();
	}

	public float GetMaxMaturity()
	{
		return maturity.GetMax();
	}

	public float PercentOfCurrentHarvest()
	{
		return maturity.value / maturity.GetMax();
	}

	public float TimeUntilNextHarvest()
	{
		float num = maturity.GetMax() - maturity.value;
		return num / maturity.GetDelta();
	}

	public float DomesticGrowthTime()
	{
		return maturity.GetMax() / base.smi.baseGrowingRate.Value;
	}

	public float WildGrowthTime()
	{
		return maturity.GetMax() / base.smi.wildGrowingRate.Value;
	}

	public float PercentGrown()
	{
		return maturity.value / maturity.GetMax();
	}

	public void ResetGrowth(object data = null)
	{
		maturity.value = 0f;
	}

	public float PercentOldAge()
	{
		return shouldGrowOld ? (oldAge.value / oldAge.GetMax()) : 0f;
	}

	public List<Descriptor> GetDescriptors(GameObject go)
	{
		List<Descriptor> list = new List<Descriptor>();
		Klei.AI.Attribute maxAttribute = Db.Get().Amounts.Maturity.maxAttribute;
		list.Add(new Descriptor(go.GetComponent<Modifiers>().GetPreModifiedAttributeDescription(maxAttribute), go.GetComponent<Modifiers>().GetPreModifiedAttributeToolTip(maxAttribute), Descriptor.DescriptorType.Requirement));
		return list;
	}

	public void ConsumeMass(float mass_to_consume)
	{
		float value = maturity.value;
		mass_to_consume = Mathf.Min(mass_to_consume, value);
		maturity.value -= mass_to_consume;
		base.gameObject.Trigger(-1793167409);
	}

	public void ConsumeGrowthUnits(float units_to_consume, float unit_maturity_ratio)
	{
		float num = units_to_consume / unit_maturity_ratio;
		DebugUtil.DevAssert(num <= maturity.value, "A critter consuming a plant (" + base.gameObject.GetProperName() + ") tried to consumed more maturity units than there were available, available: " + maturity.value + ", attempted amount: " + num);
		num = Mathf.Clamp(num, 0f, maturity.value);
		maturity.value -= num;
		base.gameObject.Trigger(-1793167409);
	}

	public Crop GetCropComponent()
	{
		return GetComponent<Crop>();
	}

	public bool IsWildPlanted()
	{
		return !rm.Replanted;
	}
}
