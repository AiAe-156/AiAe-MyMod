using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class BionicGunkSpillChore : Chore<BionicGunkSpillChore.StatesInstance>
{
	public class States : GameStateMachine<States, StatesInstance, BionicGunkSpillChore>
	{
		public class SuitAnimState : State
		{
			public State noSuit;

			public State suit;
		}

		public SuitAnimState enter;

		public SuitAnimState running;

		public SuitAnimState pst;

		public State complete;

		public TargetParameter worker;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = enter;
			Target(worker);
			root.ToggleAnims("anim_bionic_oil_overload_kanim").ToggleEffect("ExpellingGunk").ToggleTag(GameTags.MakingMess)
				.DoNotification((StatesInstance smi) => smi.stressfullyEmptyingGunk)
				.Enter(delegate(StatesInstance smi)
				{
					if (Sim.IsRadiationEnabled() && smi.master.gameObject.GetAmounts().Get(Db.Get().Amounts.RadiationBalance).value > 0f)
					{
						smi.master.gameObject.GetComponent<KSelectable>().AddStatusItem(Db.Get().DuplicantStatusItems.ExpellingRads);
					}
				});
			enter.DefaultState(enter.noSuit);
			enter.noSuit.EventTransition(GameHashes.EquippedItemEquipper, enter.suit, HasSuit).PlayAnim("oiloverload_pre", KAnim.PlayMode.Once).OnAnimQueueComplete(running);
			enter.suit.EventTransition(GameHashes.UnequippedItemEquipper, enter.noSuit, GameStateMachine<States, StatesInstance, BionicGunkSpillChore, object>.Not(HasSuit)).PlayAnim("oiloverload_helmet_pre", KAnim.PlayMode.Once).OnAnimQueueComplete(running);
			running.DefaultState(running.noSuit).Update(ExpellGunkUpdate);
			running.noSuit.EventTransition(GameHashes.EquippedItemEquipper, running.suit, HasSuit).PlayAnim("oiloverload_loop", KAnim.PlayMode.Loop);
			running.suit.EventTransition(GameHashes.UnequippedItemEquipper, running.noSuit, GameStateMachine<States, StatesInstance, BionicGunkSpillChore, object>.Not(HasSuit)).PlayAnim("oiloverload_helmet_loop", KAnim.PlayMode.Loop);
			pst.DefaultState(pst.noSuit);
			pst.noSuit.EventTransition(GameHashes.EquippedItemEquipper, pst.suit, HasSuit).PlayAnim("overload_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(complete);
			pst.suit.EventTransition(GameHashes.UnequippedItemEquipper, pst.noSuit, GameStateMachine<States, StatesInstance, BionicGunkSpillChore, object>.Not(HasSuit)).PlayAnim("oiloverload_helmet_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(complete);
			complete.ReturnSuccess();
		}
	}

	public class StatesInstance : GameStateMachine<States, StatesInstance, BionicGunkSpillChore, object>.GameInstance
	{
		public Notification stressfullyEmptyingGunk = new Notification(DUPLICANTS.STATUSITEMS.STRESSFULLYEMPTYINGOIL.NOTIFICATION_NAME, NotificationType.Bad, (List<Notification> notificationList, object data) => string.Concat(DUPLICANTS.STATUSITEMS.STRESSFULLYEMPTYINGOIL.NOTIFICATION_TOOLTIP, notificationList.ReduceMessages(countNames: false)));

		public GunkMonitor.Instance gunkMonitor;

		public StatesInstance(BionicGunkSpillChore master, GameObject worker)
			: base(master)
		{
			gunkMonitor = worker.GetSMI<GunkMonitor.Instance>();
			base.sm.worker.Set(worker, base.smi);
		}
	}

	public const float EVENT_DURATION = 10f;

	public const string PRE_ANIM_NAME = "oiloverload_pre";

	public const string LOOP_ANIM_NAME = "oiloverload_loop";

	public const string PST_ANIM_NAME = "overload_pst";

	public const string SUIT_PRE_ANIM_NAME = "oiloverload_helmet_pre";

	public const string SUIT_LOOP_ANIM_NAME = "oiloverload_helmet_loop";

	public const string SUIT_PST_ANIM_NAME = "oiloverload_helmet_pst";

	public static bool HasSuit(StatesInstance smi)
	{
		return smi.GetComponent<SuitEquipper>().IsWearingAirtightSuit();
	}

	public static void ExpellGunkUpdate(StatesInstance smi, float dt)
	{
		float num = GunkMonitor.GUNK_CAPACITY * (dt / 10f);
		if (num >= smi.gunkMonitor.CurrentGunkMass)
		{
			smi.GoTo(smi.sm.pst);
		}
		else
		{
			smi.gunkMonitor.ExpellGunk(num);
		}
	}

	public BionicGunkSpillChore(IStateMachineTarget target)
		: base(Db.Get().ChoreTypes.ExpellGunk, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.compulsory, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new StatesInstance(this, target.gameObject);
	}
}
