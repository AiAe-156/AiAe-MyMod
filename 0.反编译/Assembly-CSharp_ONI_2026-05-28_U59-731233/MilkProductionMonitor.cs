using System;
using Klei.AI;
using UnityEngine;

public class MilkProductionMonitor : GameStateMachine<MilkProductionMonitor, MilkProductionMonitor.Instance, IStateMachineTarget, MilkProductionMonitor.Def>
{
	public class Def : BaseDef
	{
		public SimHashes element = SimHashes.Milk;

		public Func<CreatureCalorieMonitor.CaloriesConsumedEvent, bool> dietConditionForMilkProduction = null;

		public string effectId = null;

		public float Capacity = 200f;

		public float CaloriesPerCycle = 1000f;

		public float HappinessRequired = 0f;

		public override void Configure(GameObject prefab)
		{
			prefab.GetComponent<Modifiers>().initialAmounts.Add(Db.Get().Amounts.MilkProduction.Id);
		}
	}

	public class ProducingStates : State
	{
		public State paused;

		public State producing;

		public State full;
	}

	public new class Instance : GameInstance, IMilkable
	{
		public Action<float> OnMilkAmountChanged = null;

		public AmountInstance milkAmountInstance;

		public EffectInstance effectInstance;

		[MyCmpGet]
		private Effects effects;

		public float MilkAmount => MilkPercentage / 100f * base.def.Capacity;

		public float MilkPercentage => milkAmountInstance.value;

		public bool IsFull => MilkPercentage >= milkAmountInstance.GetMax();

		public bool IsUnderProductionEffect => milkAmountInstance.GetDelta() > 0f;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}

		public override void StartSM()
		{
			milkAmountInstance = Db.Get().Amounts.MilkProduction.Lookup(base.gameObject);
			if (base.def.effectId != null)
			{
				effectInstance = effects.Get(base.smi.def.effectId);
			}
			base.StartSM();
		}

		public void OnCaloriesConsumed(object data)
		{
			if (base.def.effectId == null)
			{
				return;
			}
			CreatureCalorieMonitor.CaloriesConsumedEvent value = ((Boxed<CreatureCalorieMonitor.CaloriesConsumedEvent>)data).value;
			if (base.def.dietConditionForMilkProduction == null || base.def.dietConditionForMilkProduction(value))
			{
				effectInstance = effects.Get(base.smi.def.effectId);
				if (effectInstance == null)
				{
					effectInstance = effects.Add(base.smi.def.effectId, should_save: true);
				}
				effectInstance.timeRemaining += value.calories / base.smi.def.CaloriesPerCycle * 600f;
			}
		}

		public bool IsReadyToBeMilked()
		{
			return GetComponent<KPrefabID>().HasTag(GameTags.Creatures.RequiresMilking);
		}

		public SimHashes GetMilkElement()
		{
			return base.def.element;
		}

		public void MilkingComplete(Storage storage)
		{
			AmountInstance amountInstance = base.gameObject.GetAmounts().Get(Db.Get().Amounts.MilkProduction.Id);
			if (amountInstance.value > 0f)
			{
				float mass = amountInstance.value * (base.def.Capacity / amountInstance.GetMax());
				storage.GetComponent<Storage>().AddLiquid(base.def.element, mass, 310.15f, byte.MaxValue, 0);
				amountInstance.SetValue(0f);
			}
			GetComponent<KPrefabID>().RemoveTag(GameTags.Creatures.RequiresMilking);
		}

		private void RemoveMilk(float amount)
		{
			if (milkAmountInstance != null)
			{
				float value = Mathf.Min(milkAmountInstance.GetMin(), MilkPercentage - amount);
				milkAmountInstance.SetValue(value);
			}
		}

		public PrimaryElement ExtractMilk(float desiredAmount)
		{
			float num = Mathf.Min(desiredAmount, MilkAmount);
			float temperature = GetComponent<PrimaryElement>().Temperature;
			if (num <= 0f)
			{
				return null;
			}
			RemoveMilk(num);
			SubstanceChunk substanceChunk = LiquidSourceManager.Instance.CreateChunk(base.def.element, num, temperature, 0, 0, base.transform.GetPosition());
			PrimaryElement component = substanceChunk.GetComponent<PrimaryElement>();
			component.KeepZeroMassObject = false;
			return component;
		}

		public PrimaryElement ExtractMilkIntoElementChunk(float desiredAmount, PrimaryElement elementChunk)
		{
			if (elementChunk == null || elementChunk.ElementID != base.def.element)
			{
				return null;
			}
			float num = Mathf.Min(desiredAmount, MilkAmount);
			float temperature = GetComponent<PrimaryElement>().Temperature;
			RemoveMilk(num);
			float mass = elementChunk.Mass;
			float finalTemperature = GameUtil.GetFinalTemperature(elementChunk.Temperature, mass, temperature, num);
			elementChunk.SetMassTemperature(mass + num, finalTemperature);
			return elementChunk;
		}

		public PrimaryElement ExtractMilkIntoStorage(float desiredAmount, Storage storage)
		{
			float num = Mathf.Min(desiredAmount, MilkAmount);
			float temperature = GetComponent<PrimaryElement>().Temperature;
			RemoveMilk(num);
			return storage.AddLiquid(base.def.element, num, temperature, 0, 0);
		}
	}

	public ProducingStates producing;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = producing;
		producing.DefaultState(producing.paused).EventHandler(GameHashes.CaloriesConsumed, delegate(Instance smi, object data)
		{
			smi.OnCaloriesConsumed(data);
		});
		producing.paused.Transition(producing.full, IsFull, UpdateRate.SIM_1000ms).Transition(producing.producing, IsProducing, UpdateRate.SIM_1000ms);
		producing.producing.Transition(producing.full, IsFull, UpdateRate.SIM_1000ms).Transition(producing.paused, GameStateMachine<MilkProductionMonitor, Instance, IStateMachineTarget, Def>.Not(IsProducing), UpdateRate.SIM_1000ms).ToggleCritterEmotion(Db.Get().CritterEmotions.WellFed);
		producing.full.ToggleStatusItem(Db.Get().CreatureStatusItems.MilkFull).Transition(producing.paused, GameStateMachine<MilkProductionMonitor, Instance, IStateMachineTarget, Def>.Not(IsFull), UpdateRate.SIM_1000ms).Enter(delegate(Instance smi)
		{
			smi.gameObject.AddTag(GameTags.Creatures.RequiresMilking);
		});
	}

	private static bool IsProducing(Instance smi)
	{
		return !smi.IsFull && smi.IsUnderProductionEffect;
	}

	private static bool IsFull(Instance smi)
	{
		return smi.IsFull;
	}

	private static bool HasCapacity(Instance smi)
	{
		return !smi.IsFull;
	}
}
