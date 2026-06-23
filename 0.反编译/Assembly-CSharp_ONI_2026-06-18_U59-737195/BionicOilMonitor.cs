using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;

public class BionicOilMonitor : GameStateMachine<BionicOilMonitor, BionicOilMonitor.Instance, IStateMachineTarget, BionicOilMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public class WantsOilChangeState : State
	{
		public State hasOil;

		public State noOil;
	}

	public class OnlineStates : State
	{
		public State idle;

		public WantsOilChangeState seeking;
	}

	public new class Instance : GameInstance
	{
		public float LastOilAmountMassRecorded = -1f;

		public Action<float> OnOilValueChanged;

		private BionicBatteryMonitor.Instance batterySMI;

		[MyCmpGet]
		private MinionResume resume;

		[MyCmpGet]
		public Effects effects;

		public HashedString currentNoLubricationEffectApplied;

		private AttributeModifier BaseOilDeltaModifier = new AttributeModifier(Db.Get().Amounts.BionicOil.deltaAttribute.Id, -1f / 30f, BionicMinionConfig.NAME);

		private ClosestLubricantSensor closestSolidLubricantSensor;

		public bool IsOnline
		{
			get
			{
				if (batterySMI != null)
				{
					return batterySMI.IsOnline;
				}
				return false;
			}
		}

		public bool HasOil => CurrentOilMass > 0f;

		public float CurrentOilPercentage => CurrentOilMass / oilAmount.GetMax();

		public float CurrentOilMass
		{
			get
			{
				if (oilAmount != null)
				{
					return oilAmount.value;
				}
				return 0f;
			}
		}

		public AmountInstance oilAmount { get; private set; }

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			oilAmount = Db.Get().Amounts.BionicOil.Lookup(base.gameObject);
			batterySMI = base.gameObject.GetSMI<BionicBatteryMonitor.Instance>();
		}

		public override void StartSM()
		{
			closestSolidLubricantSensor = GetComponent<Sensors>().GetSensor<ClosestLubricantSensor>();
			ClosestLubricantSensor closestLubricantSensor = closestSolidLubricantSensor;
			closestLubricantSensor.OnItemChanged = (Action<Pickupable>)Delegate.Combine(closestLubricantSensor.OnItemChanged, new Action<Pickupable>(OnClosestSolidLubricantChanged));
			LastOilAmountMassRecorded = CurrentOilMass;
			base.StartSM();
		}

		public string GetEffect()
		{
			if (!resume.HasPerk(Db.Get().SkillPerks.EfficientBionicGears))
			{
				return "NoLubricationMajor";
			}
			return "NoLubricationMinor";
		}

		private void ReportOilTankFilled()
		{
			base.sm.OilFilledSignal.Trigger(this);
		}

		public void ReportOilRanOut()
		{
			base.sm.OilRanOutSignal.Trigger(this);
		}

		public void ReportOilValueChanged(float delta)
		{
			base.sm.OilValueChanged.Trigger(this);
			OnOilValueChanged?.Invoke(delta);
		}

		public void SetOilMassValue(float value)
		{
			oilAmount.SetValue(value);
		}

		public void SetBaseDeltaModifierActiveState(bool isActive)
		{
			MinionModifiers component = GetComponent<MinionModifiers>();
			if (isActive)
			{
				bool flag = false;
				int count = component.attributes.Get(BaseOilDeltaModifier.AttributeId).Modifiers.Count;
				for (int i = 0; i < count; i++)
				{
					if (component.attributes.Get(BaseOilDeltaModifier.AttributeId).Modifiers[i] == BaseOilDeltaModifier)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					component.attributes.Add(BaseOilDeltaModifier);
				}
			}
			else
			{
				component.attributes.Remove(BaseOilDeltaModifier);
			}
		}

		public void RefillOil(float amount)
		{
			oilAmount.SetValue(CurrentOilMass + amount);
			ReportOilTankFilled();
		}

		private void OnClosestSolidLubricantChanged(Pickupable newItem)
		{
			base.sm.OnClosestSolidLubricantChangedSignal.Trigger(this);
		}

		public Pickupable GetClosestSolidLubricant()
		{
			return closestSolidLubricantSensor.GetItem();
		}

		public void SetSolidLubricationSensorActiveState(bool shouldItBeActive)
		{
			closestSolidLubricantSensor.SetActive(shouldItBeActive);
			if (shouldItBeActive)
			{
				closestSolidLubricantSensor.Update();
			}
		}

		public Reactable GetGrindingGearReactable()
		{
			SelfEmoteReactable selfEmoteReactable = new SelfEmoteReactable(base.master.gameObject, Db.Get().Emotes.Minion.GrindingGears.Id, Db.Get().ChoreTypes.EmoteHighPriority, 0f, 10f);
			Emote grindingGears = Db.Get().Emotes.Minion.GrindingGears;
			selfEmoteReactable.SetEmote(grindingGears);
			selfEmoteReactable.SetThought(Db.Get().Thoughts.RefillOilDesire);
			selfEmoteReactable.preventChoreInterruption = true;
			return selfEmoteReactable;
		}
	}

	public static Dictionary<SimHashes, Effect> LUBRICANT_TYPE_EFFECT = new Dictionary<SimHashes, Effect>
	{
		[SimHashes.Tallow] = CreateFreshOilEffectVariation(SimHashes.Tallow.ToString(), -1f / 60f, 3f),
		[SimHashes.CrudeOil] = CreateFreshOilEffectVariation(SimHashes.CrudeOil.ToString(), -1f / 60f, 3f),
		[SimHashes.PhytoOil] = CreateFreshOilEffectVariation(SimHashes.PhytoOil.ToString(), -1f / 120f, 2f)
	};

	public const float OIL_CAPACITY = 200f;

	public const float OIL_TANK_DURATION = 6000f;

	public const float OIL_REFILL_TRESHOLD = 0.2f;

	public const string NO_OIL_EFFECT_NAME_MINOR = "NoLubricationMinor";

	public const string NO_OIL_EFFECT_NAME_MAJOR = "NoLubricationMajor";

	public State offline;

	public OnlineStates online;

	public Signal OilFilledSignal;

	public Signal OilRanOutSignal;

	public Signal OilValueChanged;

	public Signal OnClosestSolidLubricantChangedSignal;

	private static Effect CreateFreshOilEffectVariation(string id, float stressBonus, float moralBonus)
	{
		Effect effect = new Effect("FreshOil_" + id, DUPLICANTS.MODIFIERS.FRESHOIL.NAME, DUPLICANTS.MODIFIERS.FRESHOIL.TOOLTIP, 4800f, show_in_ui: true, trigger_floating_text: true, is_bad: false);
		effect.Add(new AttributeModifier(Db.Get().Attributes.QualityOfLife.Id, moralBonus, DUPLICANTS.MODIFIERS.FRESHOIL.NAME));
		effect.Add(new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, stressBonus, DUPLICANTS.MODIFIERS.FRESHOIL.NAME));
		return effect;
	}

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = offline;
		root.Update(OilAmountInstanceWatcherUpdate).Exit(RemoveBaseOilDeltaModifier);
		offline.EventTransition(GameHashes.BionicOnline, online, IsBionicOnline).Enter(RemoveBaseOilDeltaModifier);
		online.EventTransition(GameHashes.BionicOffline, offline, GameStateMachine<BionicOilMonitor, Instance, IStateMachineTarget, Def>.Not(IsBionicOnline)).Enter(AddBaseOilDeltaModifier).DefaultState(online.idle)
			.Enter(EnableSolidLubricationSensor)
			.Exit(DisableSolidLubricationSensor);
		online.idle.EnterTransition(online.seeking, WantsOilChange).OnSignal(OilValueChanged, online.seeking, WantsOilChange);
		online.seeking.OnSignal(OilFilledSignal, online.idle).OnSignal(OilValueChanged, online.idle, HasDecentAmountOfOil).DefaultState(online.seeking.hasOil)
			.ToggleThought(Db.Get().Thoughts.RefillOilDesire)
			.ToggleUrge(Db.Get().Urges.OilRefill)
			.ToggleChore((Instance smi) => new UseSolidLubricantChore(smi.master), online.idle);
		online.seeking.hasOil.EnterTransition(online.seeking.noOil, GameStateMachine<BionicOilMonitor, Instance, IStateMachineTarget, Def>.Not(HasAnyAmountOfOil)).OnSignal(OilRanOutSignal, online.seeking.noOil).ToggleStatusItem(Db.Get().DuplicantStatusItems.BionicWantsOilChange);
		online.seeking.noOil.Enter(delegate(Instance smi)
		{
			smi.currentNoLubricationEffectApplied = smi.effects.Add(smi.GetEffect(), should_save: false).effect.IdHash;
		}).Exit(delegate(Instance smi)
		{
			smi.effects.Remove(smi.currentNoLubricationEffectApplied);
		}).ToggleReactable(GrindingGearsReactable)
			.EventTransition(GameHashes.AssignedRoleChanged, online.seeking.hasOil);
	}

	public static bool IsBionicOnline(Instance smi)
	{
		return smi.IsOnline;
	}

	public static bool HasAnyAmountOfOil(Instance smi)
	{
		return smi.CurrentOilMass > 0f;
	}

	public static bool HasDecentAmountOfOil(Instance smi)
	{
		return HasDecentAmountOfOil(smi, null);
	}

	public static bool HasDecentAmountOfOil(Instance smi, SignalParameter param)
	{
		return smi.CurrentOilPercentage > 0.2f;
	}

	public static bool WantsOilChange(Instance smi)
	{
		return WantsOilChange(smi, null);
	}

	public static bool WantsOilChange(Instance smi, SignalParameter param)
	{
		return smi.CurrentOilPercentage <= 0.2f;
	}

	public static void AddBaseOilDeltaModifier(Instance smi)
	{
		smi.SetBaseDeltaModifierActiveState(isActive: true);
	}

	public static void RemoveBaseOilDeltaModifier(Instance smi)
	{
		smi.SetBaseDeltaModifierActiveState(isActive: false);
	}

	public static void OilAmountInstanceWatcherUpdate(Instance smi, float dt)
	{
		float lastOilAmountMassRecorded = smi.LastOilAmountMassRecorded;
		float num = smi.CurrentOilMass - lastOilAmountMassRecorded;
		if (num != 0f)
		{
			smi.LastOilAmountMassRecorded = smi.CurrentOilMass;
			if (!smi.HasOil)
			{
				smi.ReportOilRanOut();
			}
			smi.ReportOilValueChanged(num);
		}
	}

	public static void EnableSolidLubricationSensor(Instance smi)
	{
		smi.SetSolidLubricationSensorActiveState(shouldItBeActive: true);
	}

	public static void DisableSolidLubricationSensor(Instance smi)
	{
		smi.SetSolidLubricationSensorActiveState(shouldItBeActive: false);
	}

	private static Reactable GrindingGearsReactable(Instance smi)
	{
		return smi.GetGrindingGearReactable();
	}

	public static void ApplyLubricationEffects(Effects targetBionicEffects, SimHashes lubricant)
	{
		foreach (SimHashes key in LUBRICANT_TYPE_EFFECT.Keys)
		{
			if (LUBRICANT_TYPE_EFFECT.ContainsKey(key))
			{
				Effect effect = LUBRICANT_TYPE_EFFECT[key];
				if (lubricant == key)
				{
					targetBionicEffects.Add(effect, should_save: true);
				}
				else
				{
					targetBionicEffects.Remove(effect);
				}
			}
		}
	}
}
