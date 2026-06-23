using Klei.AI;
using TUNING;

public class ExternalTemperatureMonitor : GameStateMachine<ExternalTemperatureMonitor, ExternalTemperatureMonitor.Instance>
{
	public new class Instance : GameInstance
	{
		public float HotThreshold = 306.15f;

		public Effects effects;

		public Traits traits;

		public Attributes attributes;

		public AmountInstance internalTemperature;

		private TemperatureMonitor.Instance internalTemperatureMonitor;

		public CreatureSimTemperatureTransfer temperatureTransferer;

		public PrimaryElement primaryElement;

		public MinionResume minionResume;

		private Effect warmAirEffect = Db.Get().effects.Get("WarmAir");

		private Effect coldAirEffect = Db.Get().effects.Get("ColdAir");

		private Effect[] immunityToColdEffects = new Effect[2]
		{
			Db.Get().effects.Get("WarmTouch"),
			Db.Get().effects.Get("WarmTouchFood")
		};

		public float GetCurrentColdThreshold
		{
			get
			{
				if (internalTemperatureMonitor.IdealTemperatureDelta() > 0.5f)
				{
					return 0f;
				}
				return CreatureSimTemperatureTransfer.PotentialEnergyFlowToCreature(Grid.PosToCell(base.gameObject), primaryElement, temperatureTransferer);
			}
		}

		public float GetCurrentHotThreshold => HotThreshold;

		public Instance(IStateMachineTarget master)
			: base(master)
		{
			attributes = base.gameObject.GetAttributes();
			minionResume = base.gameObject.GetComponent<MinionResume>();
			internalTemperatureMonitor = base.gameObject.GetSMI<TemperatureMonitor.Instance>();
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
			if (base.smi.temperatureTransferer.average_kilowatts_exchanged.GetUnweightedAverage > GetExternalWarmThreshold(base.smi.attributes))
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
			if (base.smi.temperatureTransferer.average_kilowatts_exchanged.GetUnweightedAverage < GetExternalColdThreshold(base.smi.minionResume))
			{
				return true;
			}
			return false;
		}
	}

	public State comfortable;

	public State transitionToTooWarm;

	public State tooWarm;

	public State transitionToTooCool;

	public State tooCool;

	private const float BODY_TEMPERATURE_AFFECT_EXTERNAL_FEEL_THRESHOLD = 0.5f;

	public const string CHILLY_SURROUNDINGS_EFFECT_NAME = "ColdAir";

	public const string TOASTY_SURROUNDINGS_EFFECT_NAME = "WarmAir";

	public static readonly float BASE_STRESS_TOLERANCE_COLD = DUPLICANTSTATS.STANDARD.BaseStats.DUPLICANT_WARMING_KILOWATTS * 0.2f;

	public static readonly float BASE_STRESS_TOLERANCE_WARM = DUPLICANTSTATS.STANDARD.BaseStats.DUPLICANT_COOLING_KILOWATTS * 0.2f;

	private const float START_GAME_AVERAGING_DELAY = 6f;

	private const float TRANSITION_TO_DELAY = 1f;

	private const float TRANSITION_OUT_DELAY = 6f;

	public static float GetExternalColdThreshold(MinionResume minionResume)
	{
		if (minionResume == null)
		{
			return -0.039f;
		}
		int num = Grid.PosToCell(minionResume.gameObject);
		if (minionResume.HasPerk(Db.Get().SkillPerks.ImprovedLiquidTemperatureTolerance.Id) && Grid.IsValidCell(num) && Grid.Element[num].IsLiquid)
		{
			return -0.39000002f;
		}
		return -0.039f;
	}

	public static float GetExternalWarmThreshold(Attributes affected_attributes)
	{
		return 0.008f;
	}

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = comfortable;
		comfortable.Transition(transitionToTooWarm, (Instance smi) => smi.IsTooHot() && smi.timeinstate > 6f).Transition(transitionToTooCool, (Instance smi) => smi.IsTooCold() && smi.timeinstate > 6f);
		transitionToTooWarm.Transition(comfortable, (Instance smi) => !smi.IsTooHot()).Transition(tooWarm, (Instance smi) => smi.IsTooHot() && smi.timeinstate > 1f);
		transitionToTooCool.Transition(comfortable, (Instance smi) => !smi.IsTooCold()).Transition(tooCool, (Instance smi) => smi.IsTooCold() && smi.timeinstate > 1f);
		tooWarm.ToggleTag(GameTags.FeelingWarm).Transition(comfortable, (Instance smi) => !smi.IsTooHot() && smi.timeinstate > 6f).EventHandlerTransition(GameHashes.EffectAdded, comfortable, (Instance smi, object obj) => !smi.IsTooHot())
			.Enter(delegate
			{
				Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_ThermalComfort);
			});
		tooCool.ToggleTag(GameTags.FeelingCold).Transition(comfortable, (Instance smi) => !smi.IsTooCold() && smi.timeinstate > 6f).EventHandlerTransition(GameHashes.EffectAdded, comfortable, (Instance smi, object obj) => !smi.IsTooCold())
			.Enter(delegate
			{
				Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_ThermalComfort);
			});
	}
}
