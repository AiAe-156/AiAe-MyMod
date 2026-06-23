using Klei.AI;
using STRINGS;
using UnityEngine;

public class SlipperyMonitor : GameStateMachine<SlipperyMonitor, SlipperyMonitor.Instance, IStateMachineTarget, SlipperyMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public class UnsafeCellState : State
	{
		public RiskStates atRisk;

		public State immune;
	}

	public class RiskStates : State
	{
		public State idle;

		public State slip;
	}

	public new class Instance : GameInstance
	{
		private Effect effect;

		public Effects effects;

		public bool IsImmune => effects.HasEffect("RecentlySlippedTracker") || effects.HasImmunityTo(effect);

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			effects = GetComponent<Effects>();
			effect = Db.Get().effects.Get("RecentlySlippedTracker");
		}

		public SlipReactable CreateReactable()
		{
			return new SlipReactable(this);
		}
	}

	public class SlipReactable : Reactable
	{
		private Instance smi;

		private float startTime;

		private const string ANIM_FILE_NAME = "anim_slip_kanim";

		private const float DURATION = 4.3f;

		public SlipReactable(Instance _smi)
			: base(_smi.gameObject, "Slip", Db.Get().ChoreTypes.Slip, 1, 1, follow_transform: false, 0f, 0f, 8f)
		{
			smi = _smi;
		}

		public override bool InternalCanBegin(GameObject new_reactor, Navigator.ActiveTransition transition)
		{
			if (reactor != null)
			{
				return false;
			}
			if (new_reactor == null)
			{
				return false;
			}
			if (gameObject != new_reactor)
			{
				return false;
			}
			if (smi == null)
			{
				return false;
			}
			Navigator component = new_reactor.GetComponent<Navigator>();
			if (component == null)
			{
				return false;
			}
			if (component.CurrentNavType == NavType.Tube || component.CurrentNavType == NavType.Ladder || component.CurrentNavType == NavType.Pole)
			{
				return false;
			}
			return true;
		}

		protected override void InternalBegin()
		{
			startTime = Time.time;
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, DUPLICANTS.MODIFIERS.SLIPPED.NAME, gameObject.transform);
			KBatchedAnimController component = reactor.GetComponent<KBatchedAnimController>();
			component.AddAnimOverrides(Assets.GetAnim("anim_slip_kanim"), 1f);
			component.Play("slip_pre");
			component.Queue("slip_loop");
			component.Queue("slip_pst");
			reactor.GetComponent<KSelectable>().AddStatusItem(Db.Get().DuplicantStatusItems.Slippering);
		}

		public override void Update(float dt)
		{
			if (Time.time - startTime > 4.3f)
			{
				Cleanup();
				ApplyStress();
				ApplyTrackerEffect();
			}
		}

		public void ApplyTrackerEffect()
		{
			smi.effects.Add("RecentlySlippedTracker", should_save: true);
		}

		private void ApplyStress()
		{
			smi.master.gameObject.GetAmounts().Get(Db.Get().Amounts.Stress.Id).ApplyDelta(3f);
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, 3f + "% " + Db.Get().Amounts.Stress.Name, gameObject.transform);
			ReportManager.Instance.ReportValueWithGameObjectContext(ReportManager.ReportType.StressDelta, 3f, gameObject, DUPLICANTS.MODIFIERS.SLIPPED.NAME);
		}

		protected override void InternalEnd()
		{
			if (reactor != null)
			{
				KBatchedAnimController component = reactor.GetComponent<KBatchedAnimController>();
				if (component != null)
				{
					reactor.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().DuplicantStatusItems.Slippering);
					component.RemoveAnimOverrides(Assets.GetAnim("anim_slip_kanim"));
				}
			}
		}

		protected override void InternalCleanup()
		{
		}
	}

	public const string EFFECT_NAME = "RecentlySlippedTracker";

	public const float SLIP_FAIL_TIMEOUT = 8f;

	public const float PROBABILITY_OF_SLIP = 0.05f;

	public const float STRESS_DAMAGE = 3f;

	public State safe;

	public UnsafeCellState unsafeCell;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = safe;
		safe.EventTransition(GameHashes.NavigationCellChanged, unsafeCell, IsStandingOnASlipperyCell);
		unsafeCell.EventTransition(GameHashes.NavigationCellChanged, safe, GameStateMachine<SlipperyMonitor, Instance, IStateMachineTarget, Def>.Not(IsStandingOnASlipperyCell)).DefaultState(unsafeCell.atRisk);
		unsafeCell.atRisk.EventTransition(GameHashes.EquipmentChanged, unsafeCell.immune, IsImmuneToSlipperySurfaces).EventTransition(GameHashes.EffectAdded, unsafeCell.immune, IsImmuneToSlipperySurfaces).DefaultState(unsafeCell.atRisk.idle);
		unsafeCell.atRisk.idle.EventHandlerTransition(GameHashes.NavigationCellChanged, unsafeCell.atRisk.slip, RollDTwenty);
		unsafeCell.atRisk.slip.ToggleReactable(GetReactable).ScheduleGoTo(8f, unsafeCell.atRisk.idle);
		unsafeCell.immune.EventTransition(GameHashes.EquipmentChanged, unsafeCell.atRisk, GameStateMachine<SlipperyMonitor, Instance, IStateMachineTarget, Def>.Not(IsImmuneToSlipperySurfaces)).EventTransition(GameHashes.EffectRemoved, unsafeCell.atRisk, GameStateMachine<SlipperyMonitor, Instance, IStateMachineTarget, Def>.Not(IsImmuneToSlipperySurfaces));
	}

	public bool IsImmuneToSlipperySurfaces(Instance smi)
	{
		return smi.IsImmune;
	}

	public Reactable GetReactable(Instance smi)
	{
		return smi.CreateReactable();
	}

	private static bool IsStandingOnASlipperyCell(Instance smi)
	{
		int num = Grid.PosToCell(smi);
		int num2 = Grid.OffsetCell(num, 0, -1);
		if (Grid.IsValidCell(num) && Grid.Element[num].IsSlippery)
		{
			return true;
		}
		if (Grid.IsValidCell(num2) && Grid.Element[num2].IsSolid && Grid.Element[num2].IsSlippery)
		{
			return true;
		}
		return false;
	}

	private static bool RollDTwenty(Instance smi, object o)
	{
		float value = Random.value;
		return value <= 0.05f;
	}
}
