using Klei.AI;
using STRINGS;
using TUNING;

public class BionicSuffocationMonitor : GameStateMachine<BionicSuffocationMonitor, BionicSuffocationMonitor.Instance, IStateMachineTarget, BionicSuffocationMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public class NoOxygenState : State
	{
		public State holdingbreath;

		public State suffocating;
	}

	public new class Instance : GameInstance
	{
		private AmountInstance breath;

		public AttributeModifier breathing;

		public AttributeModifier holdingbreath;

		public OxygenBreather oxygenBreather { get; private set; }

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			breath = Db.Get().Amounts.Breath.Lookup(master.gameObject);
			Attribute deltaAttribute = Db.Get().Amounts.Breath.deltaAttribute;
			float bREATH_RATE = DUPLICANTSTATS.STANDARD.Breath.BREATH_RATE;
			breathing = new AttributeModifier(deltaAttribute.Id, bREATH_RATE, DUPLICANTS.MODIFIERS.BREATHING.NAME);
			holdingbreath = new AttributeModifier(deltaAttribute.Id, 0f - bREATH_RATE, DUPLICANTS.MODIFIERS.HOLDINGBREATH.NAME);
			oxygenBreather = GetComponent<OxygenBreather>();
		}

		public bool IsBreathing()
		{
			if (!oxygenBreather.HasOxygen && !base.master.GetComponent<KPrefabID>().HasTag(GameTags.RecoveringBreath))
			{
				return oxygenBreather.HasTag(GameTags.InTransitTube);
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

		public void Kill()
		{
			base.gameObject.GetSMI<DeathMonitor.Instance>().Kill(Db.Get().Deaths.Suffocation);
		}
	}

	public NoOxygenState noOxygen;

	public State normal;

	public State death;

	public State dead;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = normal;
		root.TagTransition(GameTags.Dead, dead);
		normal.ToggleAttributeModifier("Breathing", (Instance smi) => smi.breathing).EventTransition(GameHashes.OxygenBreatherHasAirChanged, noOxygen, (Instance smi) => !smi.IsBreathing());
		noOxygen.EventTransition(GameHashes.OxygenBreatherHasAirChanged, normal, (Instance smi) => smi.IsBreathing()).TagTransition(GameTags.RecoveringBreath, normal).ToggleExpression(Db.Get().Expressions.Suffocate)
			.ToggleAttributeModifier("Holding Breath", (Instance smi) => smi.holdingbreath)
			.ToggleTag(GameTags.NoOxygen)
			.DefaultState(noOxygen.holdingbreath);
		noOxygen.holdingbreath.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Suffocation, Db.Get().DuplicantStatusItems.HoldingBreath).Transition(noOxygen.suffocating, (Instance smi) => smi.IsSuffocating());
		noOxygen.suffocating.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Suffocation, Db.Get().DuplicantStatusItems.Suffocating).Transition(death, (Instance smi) => smi.HasSuffocated());
		death.Enter("SuffocationDeath", delegate(Instance smi)
		{
			smi.Kill();
		});
		dead.DoNothing();
	}
}
