using System;

public class BeIncapacitatedSuffocatingChore : Chore<BeIncapacitatedSuffocatingChore.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, BeIncapacitatedSuffocatingChore, object>.GameInstance
	{
		public bool isDrowning = false;

		public StatesInstance(BeIncapacitatedSuffocatingChore master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, BeIncapacitatedSuffocatingChore>
	{
		public State incapacitated;

		public State resuscitated;

		public State fail;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = incapacitated;
			root.ToggleAnims((Func<StatesInstance, HashedString>)GetSuffocatingAnimSet).ToggleStatusItem(Db.Get().DuplicantStatusItems.SuffocatingIncapacitated, (StatesInstance smi) => smi.master.gameObject.GetSMI<SuffocationMonitor.Instance>());
			incapacitated.EventHandler(GameHashes.Died, delegate(StatesInstance smi)
			{
				smi.SetStatus(Status.Failed);
				smi.StopSM("died");
			}).PlayAnim("incapacitate_pre").QueueAnim("incapacitate_loop", loop: true)
				.ToggleChore((StatesInstance smi) => new ResuscitateSuffocatedChore(smi.master, masterTarget.Get(smi)), resuscitated, fail)
				.EventTransition(GameHashes.IncapacitationRecovery, resuscitated);
			fail.ReturnFailure();
			resuscitated.ReturnSuccess();
		}

		private HashedString GetSuffocatingAnimSet(StatesInstance smi)
		{
			if (smi.isDrowning)
			{
				return "anim_incapacitated_drowning_kanim";
			}
			return "anim_incapacitated_kanim";
		}
	}

	public BeIncapacitatedSuffocatingChore(IStateMachineTarget master)
		: base(Db.Get().ChoreTypes.BeIncapacitated, master, master.GetComponent<ChoreProvider>(), run_until_complete: true, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.compulsory, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new StatesInstance(this);
		base.smi.isDrowning = Grid.IsLiquid(Grid.PosToCell(base.smi));
	}
}
