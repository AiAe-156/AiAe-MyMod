using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class FertilizationMonitor : GameStateMachine<FertilizationMonitor, FertilizationMonitor.Instance, IStateMachineTarget, FertilizationMonitor.Def>
{
	public class Def : BaseDef, IGameObjectEffectDescriptor
	{
		public PlantElementAbsorber.ConsumeInfo[] consumedElements;

		public List<Descriptor> GetDescriptors(GameObject obj)
		{
			if (consumedElements.Length == 0)
			{
				return null;
			}
			List<Descriptor> list = new List<Descriptor>();
			float preModifiedAttributeValue = obj.GetComponent<Modifiers>().GetPreModifiedAttributeValue(Db.Get().PlantAttributes.FertilizerUsageMod);
			PlantElementAbsorber.ConsumeInfo[] array = consumedElements;
			for (int i = 0; i < array.Length; i++)
			{
				PlantElementAbsorber.ConsumeInfo consumeInfo = array[i];
				float num = consumeInfo.massConsumptionRate * preModifiedAttributeValue;
				list.Add(new Descriptor(string.Format(UI.GAMEOBJECTEFFECTS.IDEAL_FERTILIZER, consumeInfo.tag.ProperName(), GameUtil.GetFormattedMass(0f - num, GameUtil.TimeSlice.PerCycle)), string.Format(UI.GAMEOBJECTEFFECTS.TOOLTIPS.IDEAL_FERTILIZER, consumeInfo.tag.ProperName(), GameUtil.GetFormattedMass(num, GameUtil.TimeSlice.PerCycle)), Descriptor.DescriptorType.Requirement));
			}
			return list;
		}

		public PlantElementAbsorber.ConsumeInfo[] ScaleConsumedElements(float scale)
		{
			PlantElementAbsorber.ConsumeInfo[] array = new PlantElementAbsorber.ConsumeInfo[consumedElements.Length];
			for (int i = 0; i < consumedElements.Length; i++)
			{
				PlantElementAbsorber.ConsumeInfo consumeInfo = consumedElements[i];
				consumeInfo.massConsumptionRate *= scale;
				array[i] = consumeInfo;
			}
			return array;
		}
	}

	public enum FertilizerStatus
	{
		Starved,
		Correct
	}

	public class FertilizedStates : State
	{
		public State absorbing;

		public State wilting;
	}

	public class ReplantedStates : State
	{
		public FertilizedStates fertilized;

		public State starved;
	}

	public new class Instance : GameInstance, IWiltCause
	{
		public AttributeModifier absorptionRate;

		protected AmountInstance fertilization;

		private Storage storage;

		private HandleVector<int>.Handle absorberHandle = HandleVector<int>.InvalidHandle;

		[MyCmpReq]
		public WiltCondition wiltCondition;

		private readonly PlantElementAbsorber.ConsumeInfo[] consumedElements;

		public float total_fertilizer_available => PlantElementAbsorber.FindLargest(storage, consumedElements);

		public WiltCondition.Condition[] Conditions => new WiltCondition.Condition[1] { WiltCondition.Condition.Fertilized };

		public string WiltStateString
		{
			get
			{
				if (!base.smi.IsInsideState(base.smi.sm.replanted.starved))
				{
					return "";
				}
				return GetStarvedStatusItem().resolveStringCallback(CREATURES.STATUSITEMS.NEEDSFERTILIZER.NAME, this);
			}
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			AddAmounts(base.gameObject);
			MakeModifiers();
			master.Subscribe(1309017699, SetStorage);
			float totalValue = base.gameObject.GetAttributes().Get(Db.Get().PlantAttributes.FertilizerUsageMod).GetTotalValue();
			consumedElements = def.ScaleConsumedElements(totalValue);
		}

		public virtual StatusItem GetStarvedStatusItem()
		{
			return Db.Get().CreatureStatusItems.NeedsFertilizer;
		}

		protected virtual void AddAmounts(GameObject gameObject)
		{
			Amounts amounts = gameObject.GetAmounts();
			fertilization = amounts.Add(new AmountInstance(Db.Get().Amounts.Fertilization, gameObject));
		}

		protected virtual void MakeModifiers()
		{
			absorptionRate = new AttributeModifier(Db.Get().Amounts.Fertilization.deltaAttribute.Id, 1.6666666f, CREATURES.STATS.FERTILIZATION.ABSORBING_MODIFIER);
		}

		public void SetStorage(object obj)
		{
			storage = (Storage)obj;
			base.sm.fertilizerStorage.Set(storage, base.smi);
			IrrigationMonitor.Instance.DumpIncorrectFertilizers(storage, base.smi.gameObject);
			ManualDeliveryKG[] components = base.smi.gameObject.GetComponents<ManualDeliveryKG>();
			foreach (ManualDeliveryKG manualDeliveryKG in components)
			{
				bool flag = false;
				PlantElementAbsorber.ConsumeInfo[] array = base.def.consumedElements;
				for (int j = 0; j < array.Length; j++)
				{
					PlantElementAbsorber.ConsumeInfo consumeInfo = array[j];
					if (manualDeliveryKG.RequestedItemTag == consumeInfo.tag)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					manualDeliveryKG.SetStorage(storage);
					manualDeliveryKG.enabled = true;
				}
			}
		}

		public virtual bool AcceptsFertilizer()
		{
			if (base.sm.fertilizerStorage.Get(this).TryGetComponent<PlantablePlot>(out var component))
			{
				return component.AcceptsFertilizer;
			}
			return false;
		}

		public void UpdateFertilization(float dt)
		{
			if (base.def.consumedElements != null && !(storage == null) && dt != 0f)
			{
				bool value = PlantElementAbsorber.PlanConsume(storage, consumedElements, dt, null);
				base.sm.isFertilized.Set(value, base.smi);
			}
		}

		public void StartAbsorbing()
		{
			if (!absorberHandle.IsValid() && base.def.consumedElements != null && base.def.consumedElements.Length != 0)
			{
				absorberHandle = Game.Instance.plantElementAbsorbers.Add(storage, consumedElements);
			}
		}

		public void StopAbsorbing()
		{
			if (absorberHandle.IsValid())
			{
				absorberHandle = Game.Instance.plantElementAbsorbers.Remove(absorberHandle);
			}
		}
	}

	public TargetParameter fertilizerStorage;

	public BoolParameter isFertilized;

	public State wild;

	public State unfertilizable;

	public ReplantedStates replanted;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = wild;
		base.serializable = SerializeType.Never;
		wild.ParamTransition(fertilizerStorage, unfertilizable, (Instance smi, GameObject p) => p != null);
		unfertilizable.EnterTransition(replanted, (Instance smi) => smi.AcceptsFertilizer());
		replanted.Enter(delegate(Instance smi)
		{
			ManualDeliveryKG[] components = smi.gameObject.GetComponents<ManualDeliveryKG>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].Pause(pause: false, "replanted");
			}
			smi.UpdateFertilization(0.2f);
		}).ParamTransition(isFertilized, replanted.fertilized, (Instance _, bool status) => status).ParamTransition(isFertilized, replanted.starved, (Instance _, bool status) => !status)
			.Target(fertilizerStorage)
			.EventHandler(GameHashes.OnStorageChange, delegate(Instance smi)
			{
				smi.UpdateFertilization(0.2f);
			})
			.Target(masterTarget);
		replanted.fertilized.DefaultState(replanted.fertilized.absorbing).TriggerOnEnter(GameHashes.Fertilized).EnterTransition(replanted.fertilized.wilting, (Instance smi) => smi.wiltCondition.IsWilting());
		replanted.fertilized.absorbing.ToggleAttributeModifier("Absorbing", (Instance smi) => smi.absorptionRate).EventTransition(GameHashes.Wilt, replanted.fertilized.wilting).Enter(delegate(Instance smi)
		{
			smi.StartAbsorbing();
		})
			.Exit(delegate(Instance smi)
			{
				smi.StopAbsorbing();
			});
		replanted.fertilized.wilting.EventTransition(GameHashes.WiltRecover, replanted.fertilized.absorbing);
		replanted.starved.TriggerOnEnter(GameHashes.Unfertilized);
	}
}
