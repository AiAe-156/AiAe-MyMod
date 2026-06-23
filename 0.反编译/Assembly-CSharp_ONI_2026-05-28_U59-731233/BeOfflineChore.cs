using System;

public class BeOfflineChore : Chore<BeOfflineChore.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, BeOfflineChore, object>.GameInstance
	{
		public StatesInstance(BeOfflineChore master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, BeOfflineChore>
	{
		public override void InitializeStates(out BaseState default_state)
		{
			default_state = root;
			root.ToggleAnims("anim_bionic_kanim").ToggleStatusItem(Db.Get().DuplicantStatusItems.BionicOfflineIncapacitated, (StatesInstance smi) => smi.master.gameObject.GetSMI<BionicBatteryMonitor.Instance>()).ToggleEffect("BionicOffline")
				.PlayAnim((Func<StatesInstance, string>)GetPowerDownAnimPre, KAnim.PlayMode.Once)
				.QueueAnim(GetPowerDownAnimLoop, loop: true);
		}
	}

	public const string EFFECT_NAME = "BionicOffline";

	public static string GetPowerDownAnimPre(StatesInstance smi)
	{
		NavType currentNavType = smi.gameObject.GetComponent<Navigator>().CurrentNavType;
		NavType navType = currentNavType;
		if (navType == NavType.Ladder || navType == NavType.Pole)
		{
			return "ladder_power_down";
		}
		return "power_down";
	}

	public static string GetPowerDownAnimLoop(StatesInstance smi)
	{
		NavType currentNavType = smi.gameObject.GetComponent<Navigator>().CurrentNavType;
		NavType navType = currentNavType;
		if (navType == NavType.Ladder || navType == NavType.Pole)
		{
			return "ladder_power_down_idle";
		}
		return "power_down_idle";
	}

	public BeOfflineChore(IStateMachineTarget master)
		: base(Db.Get().ChoreTypes.BeOffline, master, master.GetComponent<ChoreProvider>(), run_until_complete: true, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.compulsory, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new StatesInstance(this);
		AddPrecondition(ChorePreconditions.instance.NotInTube);
	}
}
