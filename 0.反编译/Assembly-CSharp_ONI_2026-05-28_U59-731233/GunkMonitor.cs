using System;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class GunkMonitor : GameStateMachine<GunkMonitor, GunkMonitor.Instance, IStateMachineTarget, GunkMonitor.Def>
{
	public class Def : BaseDef
	{
		public float SeekForGunkToiletTreshold_InSchedule = 0.6f;

		public float DesperetlySeekForGunkToiletTreshold = 0.9f;
	}

	public class MildUrgeStates : State
	{
		public State allowed;

		public State prevented;
	}

	public new class Instance : GameInstance
	{
		private float LastAmountOfGunkObserved = 0f;

		private BionicOilMonitor.Instance oilMonitor;

		private AmountInstance gunkAmount;

		private AmountInstance bodyTemperature;

		private Schedulable schedulable;

		public bool HasGunk => CurrentGunkMass > 0f;

		public bool IsGunkBuildupAtMax => CurrentGunkPercentage >= 1f;

		public float CurrentGunkMass => (gunkAmount == null) ? 0f : gunkAmount.value;

		public float CurrentGunkPercentage => CurrentGunkMass / gunkAmount.GetMax();

		public bool DoesCurrentScheduleAllowsGunkToilet => schedulable.IsAllowed(Db.Get().ScheduleBlockTypes.Eat) || schedulable.IsAllowed(Db.Get().ScheduleBlockTypes.Hygiene);

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			bodyTemperature = Db.Get().Amounts.Temperature.Lookup(base.gameObject);
			gunkAmount = Db.Get().Amounts.BionicGunk.Lookup(base.gameObject);
			schedulable = GetComponent<Schedulable>();
		}

		public override void StartSM()
		{
			oilMonitor = base.gameObject.GetSMI<BionicOilMonitor.Instance>();
			BionicOilMonitor.Instance instance = oilMonitor;
			instance.OnOilValueChanged = (Action<float>)Delegate.Combine(instance.OnOilValueChanged, new Action<float>(OnOilValueChanged));
			LastAmountOfGunkObserved = CurrentGunkMass;
			base.StartSM();
		}

		public void GunkAmountWatcherUpdate(float dt)
		{
			if (LastAmountOfGunkObserved != CurrentGunkMass)
			{
				LastAmountOfGunkObserved = CurrentGunkMass;
				base.sm.gunkValueChangedSignal.Trigger(this);
			}
		}

		protected override void OnCleanUp()
		{
			if (oilMonitor != null)
			{
				BionicOilMonitor.Instance instance = oilMonitor;
				instance.OnOilValueChanged = (Action<float>)Delegate.Remove(instance.OnOilValueChanged, new Action<float>(OnOilValueChanged));
			}
			base.OnCleanUp();
		}

		private void OnOilValueChanged(float delta)
		{
			float num = ((delta < 0f) ? Mathf.Abs(delta) : 0f);
			float gunkMassValue = Mathf.Clamp(CurrentGunkMass + num, 0f, gunkAmount.GetMax());
			SetGunkMassValue(gunkMassValue);
		}

		public void SetGunkMassValue(float value)
		{
			bool flag = CurrentGunkMass != value;
			gunkAmount.SetValue(value);
			LastAmountOfGunkObserved = CurrentGunkMass;
			base.sm.gunkValueChangedSignal.Trigger(this);
		}

		public void ExpellGunk(float mass, Storage targetStorage = null)
		{
			if (!HasGunk)
			{
				return;
			}
			float currentGunkMass = CurrentGunkMass;
			float a = Mathf.Min(mass, CurrentGunkMass);
			a = Mathf.Max(a, Mathf.Epsilon);
			int gameCell = Grid.PosToCell(base.transform.position);
			byte index = Db.Get().Diseases.GetIndex(DUPLICANTSTATS.BIONICS.Secretions.PEE_DISEASE);
			float num = a / GUNK_CAPACITY;
			if (targetStorage != null)
			{
				targetStorage.AddLiquid(GunkElement, a, bodyTemperature.value, index, (int)((float)DUPLICANTSTATS.BIONICS.Secretions.DISEASE_PER_PEE * num));
			}
			else
			{
				Equippable equippable = GetComponent<SuitEquipper>().IsWearingAirtightSuit();
				if (equippable != null)
				{
					equippable.GetComponent<Storage>().AddLiquid(GunkElement, a, bodyTemperature.value, index, (int)((float)DUPLICANTSTATS.BIONICS.Secretions.DISEASE_PER_PEE * num));
				}
				else
				{
					SimMessages.AddRemoveSubstance(gameCell, GunkElement, CellEventLogger.Instance.Vomit, a, bodyTemperature.value, index, (int)((float)DUPLICANTSTATS.BIONICS.Secretions.DISEASE_PER_PEE * num));
				}
			}
			if (Sim.IsRadiationEnabled())
			{
				MinionIdentity component = base.transform.GetComponent<MinionIdentity>();
				AmountInstance amountInstance = Db.Get().Amounts.RadiationBalance.Lookup(component);
				RadiationMonitor.Instance sMI = component.GetSMI<RadiationMonitor.Instance>();
				float num2 = DUPLICANTSTATS.STANDARD.BaseStats.BLADDER_INCREASE_PER_SECOND / DUPLICANTSTATS.BIONICS.BaseStats.BLADDER_INCREASE_PER_SECOND;
				float num3 = Math.Min(amountInstance.value, 300f * num2 * sMI.difficultySettingMod * num);
				if (num3 >= 1f)
				{
					PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, Math.Floor(num3).ToString() + UI.UNITSUFFIXES.RADIATION.RADS, component.transform, Vector3.up * 2f);
				}
				amountInstance.ApplyDelta(0f - num3);
			}
			SetGunkMassValue(Mathf.Clamp(CurrentGunkMass - a, 0f, gunkAmount.GetMax()));
		}

		public void ExpellAllGunk(Storage targetStorage = null)
		{
			ExpellGunk(CurrentGunkMass, targetStorage);
		}
	}

	public const float BIONIC_RADS_REMOVED_WHEN_PEE = 300f;

	public static readonly float GUNK_CAPACITY = 80f;

	public const string GUNK_FULL_EFFECT_NAME = "GunkSick";

	public const string GUNK_HUNGOVER_EFFECT_NAME = "GunkHungover";

	public static SimHashes GunkElement = SimHashes.LiquidGunk;

	public State idle;

	public MildUrgeStates mildUrge;

	public State criticalUrge;

	public State cantHold;

	public State emptyRemaining;

	public Signal gunkValueChangedSignal;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = idle;
		root.Update(GunkAmountWatcherUpdate);
		idle.OnSignal(gunkValueChangedSignal, mildUrge, IsGunkLevelsOverMildUrgeThreshold);
		mildUrge.OnSignal(gunkValueChangedSignal, criticalUrge, IsGunkLevelsOverCriticalUrgeThreshold).OnSignal(gunkValueChangedSignal, idle, DoesNotWantToExpellGunk).DefaultState(mildUrge.prevented);
		mildUrge.prevented.ScheduleChange(mildUrge.allowed, ScheduleAllowsExpelling);
		mildUrge.allowed.ScheduleChange(mildUrge.prevented, GameStateMachine<GunkMonitor, Instance, IStateMachineTarget, Def>.Not(ScheduleAllowsExpelling)).ToggleUrge(Db.Get().Urges.Pee).ToggleUrge(Db.Get().Urges.GunkPee);
		criticalUrge.OnSignal(gunkValueChangedSignal, idle, DoesNotWantToExpellGunk).OnSignal(gunkValueChangedSignal, mildUrge, (Instance smi, SignalParameter param) => !IsGunkLevelsOverCriticalUrgeThreshold(smi)).OnSignal(gunkValueChangedSignal, cantHold, CanNotHoldGunkAnymore)
			.ToggleUrge(Db.Get().Urges.GunkPee)
			.ToggleUrge(Db.Get().Urges.Pee)
			.ToggleEffect("GunkSick")
			.ToggleExpression(Db.Get().Expressions.FullBladder)
			.ToggleThought(Db.Get().Thoughts.ExpellGunkDesire)
			.ToggleAnims("anim_loco_walk_slouch_kanim")
			.ToggleAnims("anim_idle_slouch_kanim");
		cantHold.ToggleUrge(Db.Get().Urges.GunkPee).ToggleThought(Db.Get().Thoughts.ExpellingGunk).ToggleChore((Instance smi) => new BionicGunkSpillChore(smi.master), emptyRemaining);
		emptyRemaining.Enter(ExpellAllGunk).Enter(ApplyGunkHungoverEffect).GoTo(idle);
	}

	public static bool IsGunkLevelsOverCriticalUrgeThreshold(Instance smi, SignalParameter param)
	{
		return IsGunkLevelsOverCriticalUrgeThreshold(smi);
	}

	public static bool IsGunkLevelsOverCriticalUrgeThreshold(Instance smi)
	{
		return smi.CurrentGunkPercentage >= smi.def.DesperetlySeekForGunkToiletTreshold;
	}

	public static bool IsGunkLevelsOverMildUrgeThreshold(Instance smi, SignalParameter param)
	{
		return IsGunkLevelsOverMildUrgeThreshold(smi);
	}

	public static bool IsGunkLevelsOverMildUrgeThreshold(Instance smi)
	{
		return smi.CurrentGunkPercentage >= smi.def.SeekForGunkToiletTreshold_InSchedule;
	}

	public static bool ScheduleAllowsExpelling(Instance smi)
	{
		return smi.DoesCurrentScheduleAllowsGunkToilet;
	}

	public static bool DoesNotWantToExpellGunk(Instance smi, SignalParameter p)
	{
		return !IsGunkLevelsOverMildUrgeThreshold(smi);
	}

	public static bool CanNotHoldGunkAnymore(Instance smi, SignalParameter p)
	{
		return CanNotHoldGunkAnymore(smi);
	}

	public static bool CanNotHoldGunkAnymore(Instance smi)
	{
		return smi.IsGunkBuildupAtMax;
	}

	public static void ExpellAllGunk(Instance smi)
	{
		smi.ExpellAllGunk();
	}

	public static void ApplyGunkHungoverEffect(Instance smi)
	{
		smi.GetComponent<Effects>().Add("GunkHungover", should_save: true);
	}

	public static void GunkAmountWatcherUpdate(Instance smi, float dt)
	{
		smi.GunkAmountWatcherUpdate(dt);
	}
}
