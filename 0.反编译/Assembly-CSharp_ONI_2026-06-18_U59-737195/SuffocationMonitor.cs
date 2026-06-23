using Klei.AI;
using STRINGS;
using TUNING;

public class SuffocationMonitor : GameStateMachine<SuffocationMonitor, SuffocationMonitor.Instance, IStateMachineTarget, SuffocationMonitor.Def>
{
	public class Def : BaseDef
	{
		public float timeBeforeDeath = 120f;
	}

	public class NoOxygenState : State
	{
		public State holdingbreath;

		public State suffocating;

		public State incapacitated;
	}

	public class SatisfiedState : State
	{
		public State normal;

		public State low;
	}

	public new class Instance : GameInstance
	{
		private AmountInstance breath;

		public AttributeModifier increaseBreathModifier;

		public AttributeModifier decreaseBreathModifier;

		public OxygenBreather oxygenBreather { get; private set; }

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			breath = Db.Get().Amounts.Breath.Lookup(master.gameObject);
			Attribute deltaAttribute = Db.Get().Amounts.Breath.deltaAttribute;
			float bREATH_RATE = DUPLICANTSTATS.STANDARD.Breath.BREATH_RATE;
			increaseBreathModifier = new AttributeModifier(deltaAttribute.Id, bREATH_RATE, DUPLICANTS.MODIFIERS.BREATHING.NAME);
			decreaseBreathModifier = new AttributeModifier(deltaAttribute.Id, 0f - bREATH_RATE, DUPLICANTS.MODIFIERS.HOLDINGBREATH.NAME);
			oxygenBreather = GetComponent<OxygenBreather>();
		}

		public bool IsBreathDepletingSignificantly()
		{
			return breath.deltaAttribute.GetTotalValue() <= (0f - DUPLICANTSTATS.STANDARD.Breath.BREATH_RATE) * 0.5f;
		}

		public bool IsBreathLow()
		{
			return breath.value <= DUPLICANTSTATS.STANDARD.Breath.SUFFOCATE_AMOUNT;
		}

		public bool CanBreath()
		{
			if (!oxygenBreather.prefabID.HasTag(GameTags.RecoveringBreath) && !oxygenBreather.prefabID.HasTag(GameTags.InTransitTube))
			{
				return oxygenBreather.HasOxygen;
			}
			return true;
		}

		public bool HasSuffocated()
		{
			return breath.value <= 0f;
		}

		public bool IsSuffocating()
		{
			if (breath.deltaAttribute.GetTotalValue() <= 0f)
			{
				return breath.value <= DUPLICANTSTATS.STANDARD.Breath.SUFFOCATE_AMOUNT;
			}
			return false;
		}

		public float GetTimeUntilDeath(Instance smi)
		{
			return smi.sm.timeUntilDeath.Get(smi);
		}

		public void Kill()
		{
			base.gameObject.GetSMI<DeathMonitor.Instance>().Kill(Db.Get().Deaths.Suffocation);
		}
	}

	public FloatParameter timeUntilDeath;

	public SatisfiedState satisfied;

	public NoOxygenState noOxygen;

	public State death;

	public State dead;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = satisfied;
		root.TagTransition(GameTags.Dead, dead);
		satisfied.DefaultState(satisfied.normal).ToggleAttributeModifier("Breathing", (Instance smi) => smi.increaseBreathModifier).EventTransition(GameHashes.OxygenBreatherHasAirChanged, noOxygen, (Instance smi) => !smi.CanBreath())
			.Transition(noOxygen, (Instance smi) => !smi.CanBreath());
		satisfied.normal.Transition(satisfied.low, (Instance smi) => smi.oxygenBreather.IsLowOxygen());
		satisfied.low.Transition(satisfied.normal, (Instance smi) => !smi.oxygenBreather.IsLowOxygen()).ToggleEffect("LowOxygen");
		noOxygen.EventTransition(GameHashes.OxygenBreatherHasAirChanged, satisfied, (Instance smi) => smi.CanBreath()).TagTransition(GameTags.RecoveringBreath, satisfied).ToggleExpression(Db.Get().Expressions.Suffocate, (Instance smi) => smi.IsBreathDepletingSignificantly() || smi.IsBreathLow())
			.ToggleAttributeModifier("Holding Breath", (Instance smi) => smi.decreaseBreathModifier)
			.ToggleTag(GameTags.NoOxygen)
			.DefaultState(noOxygen.holdingbreath);
		noOxygen.holdingbreath.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Suffocation, Db.Get().DuplicantStatusItems.HoldingBreath).Transition(noOxygen.suffocating, (Instance smi) => smi.IsSuffocating());
		noOxygen.suffocating.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Suffocation, Db.Get().DuplicantStatusItems.Suffocating).Transition(noOxygen.incapacitated, (Instance smi) => smi.HasSuffocated());
		noOxygen.incapacitated.Enter(delegate(Instance smi)
		{
			smi.sm.timeUntilDeath.Set(smi.def.timeBeforeDeath, smi);
		}).ToggleRecurringChore((Instance smi) => new BeIncapacitatedSuffocatingChore(smi.master)).ToggleUrge(Db.Get().Urges.BeIncapacitated)
			.ToggleTag(GameTags.SuffocatingIncapacitated)
			.Update(delegate(Instance smi, float dt)
			{
				UpdateTimeUntilDeath(smi, dt);
			})
			.ParamTransition(timeUntilDeath, death, GameStateMachine<SuffocationMonitor, Instance, IStateMachineTarget, Def>.IsLTZero)
			.EventTransition(GameHashes.IncapacitationRecovery, satisfied);
		death.Enter("SuffocationDeath", delegate(Instance smi)
		{
			smi.Kill();
		});
		dead.DoNothing();
	}

	private void UpdateTimeUntilDeath(Instance smi, float dt)
	{
		smi.sm.timeUntilDeath.Delta(dt * -1f, smi);
	}
}
