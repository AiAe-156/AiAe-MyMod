using Klei.AI;
using TUNING;
using UnityEngine;

public class ExternalTemperatureMonitor : GameStateMachine<ExternalTemperatureMonitor, ExternalTemperatureMonitor.Instance>
{
	public class AliveStates : State
	{
		public State comfortable;

		public State transitionToTooWarm;

		public State tooWarm;

		public State transitionToTooCool;

		public State tooCool;
	}

	public new class Instance : GameInstance
	{
		public Effects effects;

		public Traits traits;

		public Attributes attributes;

		public AmountInstance internalTemperature;

		public CreatureSimTemperatureTransfer temperatureTransferer;

		public PrimaryElement primaryElement;

		public MinionResume minionResume;

		private Effect coldAirEffect = Db.Get().effects.Get("ColdAir");

		private Effect[] immunityToColdEffects = new Effect[2]
		{
			Db.Get().effects.Get("WarmTouch"),
			Db.Get().effects.Get("WarmTouchFood")
		};

		private Effect warmAirEffect = Db.Get().effects.Get("WarmAir");

		public float CurrentColdResistanceModifier
		{
			get
			{
				if (!HasColdResistance)
				{
					return 1f;
				}
				return 10f;
			}
		}

		public bool HasColdResistance => ColdResistanceDurationRemaining > 0f;

		public float ColdResistanceDurationRemaining => base.sm._ColdResistanceDurationRemaining.Get(this);

		public Instance(IStateMachineTarget master)
			: base(master)
		{
			attributes = base.gameObject.GetAttributes();
			minionResume = base.gameObject.GetComponent<MinionResume>();
			internalTemperature = Db.Get().Amounts.Temperature.Lookup(base.gameObject);
			temperatureTransferer = base.gameObject.GetComponent<CreatureSimTemperatureTransfer>();
			primaryElement = base.gameObject.GetComponent<PrimaryElement>();
			effects = base.gameObject.GetComponent<Effects>();
			traits = base.gameObject.GetComponent<Traits>();
		}

		public bool IsTooHot()
		{
			if (effects.HasEffect("RefreshingTouch"))
			{
				return false;
			}
			if (effects.HasImmunityTo(warmAirEffect))
			{
				return false;
			}
			if (!temperatureTransferer.LastTemperatureRecordIsReliable)
			{
				return false;
			}
			if (base.smi.temperatureTransferer.average_kilowatts_exchanged.GetUnweightedAverage > 0.008f)
			{
				return true;
			}
			return false;
		}

		public bool IsTooCold()
		{
			for (int i = 0; i < immunityToColdEffects.Length; i++)
			{
				if (effects.HasEffect(immunityToColdEffects[i]))
				{
					return false;
				}
			}
			if (effects.HasImmunityTo(coldAirEffect))
			{
				return false;
			}
			if (traits != null && traits.IsEffectIgnored(coldAirEffect))
			{
				return false;
			}
			if (WarmthProvider.IsWarmCell(Grid.PosToCell(this)))
			{
				return false;
			}
			if (!temperatureTransferer.LastTemperatureRecordIsReliable)
			{
				return false;
			}
			if (base.smi.temperatureTransferer.average_kilowatts_exchanged.GetUnweightedAverage < GetExternalColdThreshold(this))
			{
				return true;
			}
			return false;
		}

		public void UpdateTemperatureTresholdModifiers(float dt)
		{
			int num = Grid.PosToCell(this);
			bool flag = minionResume != null && minionResume.HasPerk(Db.Get().SkillPerks.ImprovedLiquidTemperatureTolerance.Id);
			if (Grid.IsValidCell(num) && Grid.Element[num].IsLiquid && flag)
			{
				base.sm._ColdResistanceDurationRemaining.Set(5f, this);
				return;
			}
			float b = ColdResistanceDurationRemaining - dt;
			b = Mathf.Max(0f, b);
			base.sm._ColdResistanceDurationRemaining.Set(b, this);
		}
	}

	public const float EXTERNAL_WARM_THRESHOLD = 0.008f;

	public const float EXTERNAL_COLD_THRESHOLD = -0.039f;

	public const float EXTERNAL_COLD_THRESHOLD_RESISTANCE_DURATION = 5f;

	public const float EXTERNAL_COLD_THRESHOLD_RESISTANCE_MULTIPLIER = 10f;

	public const string CHILLY_SURROUNDINGS_EFFECT_NAME = "ColdAir";

	public const string TOASTY_SURROUNDINGS_EFFECT_NAME = "WarmAir";

	public static readonly float BASE_STRESS_TOLERANCE_COLD = DUPLICANTSTATS.STANDARD.BaseStats.DUPLICANT_WARMING_KILOWATTS * 0.2f;

	public static readonly float BASE_STRESS_TOLERANCE_WARM = DUPLICANTSTATS.STANDARD.BaseStats.DUPLICANT_COOLING_KILOWATTS * 0.2f;

	private const float START_GAME_AVERAGING_DELAY = 6f;

	private const float TRANSITION_TO_DELAY = 1f;

	private const float TRANSITION_OUT_DELAY = 6f;

	public AliveStates alive;

	public State dead;

	private FloatParameter _ColdResistanceDurationRemaining;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = alive;
		base.serializable = SerializeType.ParamsOnly;
		alive.TagTransition(GameTags.Dead, dead).Update(UpdateTemperatureTresholdModifiers, UpdateRate.SIM_1000ms).DefaultState(alive.comfortable);
		alive.comfortable.Transition(alive.transitionToTooWarm, (Instance smi) => smi.IsTooHot() && smi.timeinstate > 6f).Transition(alive.transitionToTooCool, (Instance smi) => smi.IsTooCold() && smi.timeinstate > 6f);
		alive.transitionToTooWarm.Transition(alive.comfortable, (Instance smi) => !smi.IsTooHot()).Transition(alive.tooWarm, (Instance smi) => smi.IsTooHot() && smi.timeinstate > 1f);
		alive.transitionToTooCool.Transition(alive.comfortable, (Instance smi) => !smi.IsTooCold()).Transition(alive.tooCool, (Instance smi) => smi.IsTooCold() && smi.timeinstate > 1f);
		alive.tooWarm.ToggleTag(GameTags.FeelingWarm).Transition(alive.comfortable, (Instance smi) => !smi.IsTooHot() && smi.timeinstate > 6f).EventHandlerTransition(GameHashes.EffectAdded, alive.comfortable, (Instance smi, object obj) => !smi.IsTooHot())
			.Enter(delegate
			{
				Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_ThermalComfort);
			});
		alive.tooCool.ToggleTag(GameTags.FeelingCold).Transition(alive.comfortable, (Instance smi) => !smi.IsTooCold() && smi.timeinstate > 6f).EventHandlerTransition(GameHashes.EffectAdded, alive.comfortable, (Instance smi, object obj) => !smi.IsTooCold())
			.Enter(delegate
			{
				Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_ThermalComfort);
			});
		dead.DoNothing();
	}

	public static void UpdateTemperatureTresholdModifiers(Instance smi, float dt)
	{
		smi.UpdateTemperatureTresholdModifiers(dt);
	}

	public static float GetExternalColdThreshold(Instance smi)
	{
		if (smi == null)
		{
			return -0.039f;
		}
		return -0.039f * smi.CurrentColdResistanceModifier;
	}
}
