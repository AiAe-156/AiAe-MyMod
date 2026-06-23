using Klei.AI;
using TUNING;

public class BreathMonitor : GameStateMachine<BreathMonitor, BreathMonitor.Instance>
{
	public class LowBreathState : State
	{
		public State nowheretorecover;

		public State recoveryatcell;

		public State recoveratstation;
	}

	public class SatisfiedState : State
	{
		public State full;

		public State notfull;
	}

	public new class Instance : GameInstance
	{
		public AmountInstance breath;

		public SafetyQuery query;

		public Navigator navigator;

		public OxygenBreather breather;

		public bool canRecoverBreath = true;

		public SwimMonitor.Instance swimMonitor;

		public Instance(IStateMachineTarget master)
			: base(master)
		{
			breath = Db.Get().Amounts.Breath.Lookup(master.gameObject);
			query = new SafetyQuery(Game.Instance.safetyConditions.RecoverBreathChecker, GetComponent<KMonoBehaviour>(), int.MaxValue);
			navigator = GetComponent<Navigator>();
			breather = GetComponent<OxygenBreather>();
			swimMonitor = base.gameObject.GetSMI<SwimMonitor.Instance>();
		}

		public int GetRecoverCell()
		{
			return base.sm.recoverBreathCell.Get(base.smi);
		}

		public float GetBreath()
		{
			return breath.value / breath.GetMax();
		}
	}

	private static HashedString[] swimmingWorkAnims = new HashedString[2] { "working_pre", "working_loop" };

	private static HashedString[] swimmingWorkingPstAnims = new HashedString[1] { "working_pst" };

	private static HashedString[] landWorkAnims = new HashedString[2] { "working_land_pre", "working_land_loop" };

	private static HashedString[] landWorkingPstCompleteAnims = new HashedString[1] { "working_land_pst" };

	public SatisfiedState satisfied;

	public LowBreathState lowbreath;

	public IntParameter recoverBreathCell;

	public TargetParameter recoverBreathStation;

	private static int breathableStationPreferenceCost = 15;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = satisfied;
		satisfied.DefaultState(satisfied.full).Transition(lowbreath, IsLowBreath);
		satisfied.full.Transition(satisfied.notfull, IsNotFullBreath).Enter(HideBreathBar);
		satisfied.notfull.Transition(satisfied.full, IsFullBreath).Enter(ShowBreathBar);
		lowbreath.DefaultState(lowbreath.nowheretorecover).Transition(satisfied, IsFullBreath).ToggleExpression(Db.Get().Expressions.RecoverBreath, IsOutOfOxygen)
			.ToggleUrge(Db.Get().Urges.RecoverBreath)
			.ToggleThought(Db.Get().Thoughts.Suffocating)
			.ToggleTag(GameTags.HoldingBreath)
			.Enter(ShowBreathBar)
			.Enter(UpdateRecoverBreathCell)
			.Update(UpdateRecoverBreathCell, UpdateRate.RENDER_1000ms, load_balance: true);
		lowbreath.nowheretorecover.ParamTransition(recoverBreathCell, lowbreath.recoveryatcell, IsValidRecoverCell).ParamTransition(recoverBreathStation, lowbreath.recoveratstation, GameStateMachine<BreathMonitor, Instance, IStateMachineTarget, object>.IsNotNull);
		lowbreath.recoveryatcell.ParamTransition(recoverBreathCell, lowbreath.nowheretorecover, IsNotValidRecoverCell).ToggleChore(CreateRecoverBreathChore, lowbreath.nowheretorecover);
		lowbreath.recoveratstation.ParamTransition(recoverBreathStation, lowbreath.nowheretorecover, GameStateMachine<BreathMonitor, Instance, IStateMachineTarget, object>.IsNull).ToggleChore(CreateBreathingStationChore, lowbreath.nowheretorecover);
	}

	private static bool IsLowBreath(Instance smi)
	{
		WorldContainer myWorld = smi.master.gameObject.GetMyWorld();
		if (!(myWorld == null) && myWorld.AlertManager.IsRedAlert())
		{
			return smi.breath.value < DUPLICANTSTATS.STANDARD.Breath.SUFFOCATE_AMOUNT;
		}
		return smi.breath.value < DUPLICANTSTATS.STANDARD.Breath.RETREAT_AMOUNT;
	}

	private static Chore CreateRecoverBreathChore(Instance smi)
	{
		return new RecoverBreathChore(smi.master);
	}

	private static Chore CreateBreathingStationChore(Instance smi)
	{
		UnderwaterBreathingLocation underwaterBreathingLocation = smi.sm.recoverBreathStation.Get<UnderwaterBreathingLocation>(smi);
		if (underwaterBreathingLocation != null)
		{
			UnderwaterBreathingLocationWorkable component = underwaterBreathingLocation.GetComponent<UnderwaterBreathingLocationWorkable>();
			if (component != null)
			{
				if (underwaterBreathingLocation.allowLandUse)
				{
					if (smi.swimMonitor.CanSwim() && Grid.IsLiquid(underwaterBreathingLocation.breathableCell))
					{
						component.workAnims = swimmingWorkAnims;
						component.workingPstComplete = swimmingWorkingPstAnims;
						component.workingPstFailed = swimmingWorkingPstAnims;
					}
					else
					{
						component.workAnims = landWorkAnims;
						component.workingPstComplete = landWorkingPstCompleteAnims;
						component.workingPstFailed = landWorkingPstCompleteAnims;
					}
				}
				return new WorkChore<UnderwaterBreathingLocationWorkable>(Db.Get().ChoreTypes.RecoverBreath, component, null, run_until_complete: true, null, ReserveBreathLocation, UnReserveBreathLocation, allow_in_red_alert: true, null, ignore_schedule_block: true, only_when_operational: true, null, is_preemptable: false, allow_in_context_menu: true, allow_prioritization: false, PriorityScreen.PriorityClass.compulsory);
			}
		}
		return null;
	}

	private static void ReserveBreathLocation(Chore chore)
	{
		if (chore.gameObject.TryGetComponent<UnderwaterBreathingLocation>(out var component))
		{
			component.ReserveLocation(chore.driver.gameObject, reserve: true);
		}
	}

	private static void UnReserveBreathLocation(Chore chore)
	{
		if (chore.gameObject.TryGetComponent<UnderwaterBreathingLocation>(out var component))
		{
			component.ReserveLocation(chore.lastDriver.gameObject, reserve: false);
		}
	}

	private static bool IsNotFullBreath(Instance smi)
	{
		return !IsFullBreath(smi);
	}

	private static bool IsFullBreath(Instance smi)
	{
		return smi.breath.value >= smi.breath.GetMax();
	}

	private static bool IsOutOfOxygen(Instance smi)
	{
		return smi.breather.IsOutOfOxygen;
	}

	private static void ShowBreathBar(Instance smi)
	{
		if (NameDisplayScreen.Instance != null)
		{
			NameDisplayScreen.Instance.SetBreathDisplay(smi.gameObject, smi.GetBreath, bVisible: true);
		}
	}

	private static void HideBreathBar(Instance smi)
	{
		if (NameDisplayScreen.Instance != null)
		{
			NameDisplayScreen.Instance.SetBreathDisplay(smi.gameObject, null, bVisible: false);
		}
	}

	private static bool IsValidRecoverCell(Instance smi, int cell)
	{
		return cell != Grid.InvalidCell;
	}

	private static bool IsNotValidRecoverCell(Instance smi, int cell)
	{
		return !IsValidRecoverCell(smi, cell);
	}

	private static void UpdateRecoverBreathCell(Instance smi, float dt)
	{
		UpdateRecoverBreathCell(smi);
	}

	private static void UpdateRecoverBreathCell(Instance smi)
	{
		if (!smi.canRecoverBreath)
		{
			return;
		}
		smi.query.Reset();
		smi.navigator.RunQuery(smi.query);
		int num = smi.query.GetResultCell();
		if (!GasBreatherFromWorldProvider.GetBestBreathableCellAroundSpecificCell(num, GasBreatherFromWorldProvider.DEFAULT_BREATHABLE_OFFSETS, smi.breather).IsBreathable)
		{
			num = PathFinder.InvalidCell;
		}
		bool flag = false;
		UnderwaterBreathingLocation underwaterBreathingLocation = FindNearestReachableStation(smi.navigator);
		if (underwaterBreathingLocation != null)
		{
			int num2 = smi.navigator.GetNavigationCost(num);
			if (num2 != -1 && Grid.IsSubstantialLiquid(smi.navigator.cachedCell))
			{
				num2 += breathableStationPreferenceCost;
			}
			int navigationCost = smi.navigator.GetNavigationCost(underwaterBreathingLocation.breathableCell);
			if ((num2 == -1 && navigationCost != -1) || num2 > navigationCost)
			{
				flag = true;
				smi.sm.recoverBreathStation.Set(underwaterBreathingLocation, smi);
				smi.sm.recoverBreathCell.Set(Grid.InvalidCell, smi);
			}
		}
		if (!flag)
		{
			smi.sm.recoverBreathStation.Set(null, smi);
			smi.sm.recoverBreathCell.Set(num, smi);
		}
	}

	public static UnderwaterBreathingLocation FindNearestReachableStation(Navigator navigator)
	{
		UnderwaterBreathingLocation result = null;
		int num = int.MaxValue;
		for (int i = 0; i < Components.UnderwaterBreathingLocations.Count; i++)
		{
			UnderwaterBreathingLocation underwaterBreathingLocation = Components.UnderwaterBreathingLocations[i];
			if (!(underwaterBreathingLocation.GetAvailableBreathableMass() <= 0f) && underwaterBreathingLocation.CanReserve(navigator.gameObject))
			{
				int navigationCost = navigator.GetNavigationCost(underwaterBreathingLocation.breathableCell);
				if (navigationCost != -1 && navigationCost < num)
				{
					num = navigationCost;
					result = underwaterBreathingLocation;
				}
			}
		}
		return result;
	}
}
