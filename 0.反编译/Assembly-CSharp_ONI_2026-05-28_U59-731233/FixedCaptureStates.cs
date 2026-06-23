using STRINGS;

public class FixedCaptureStates : GameStateMachine<FixedCaptureStates, FixedCaptureStates.Instance, IStateMachineTarget, FixedCaptureStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public float originalSpeed;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			originalSpeed = GetComponent<Navigator>().defaultSpeed;
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.WantsToGetCaptured);
		}

		public FixedCapturePoint.Instance GetCapturePoint()
		{
			return this.GetSMI<FixedCapturableMonitor.Instance>()?.targetCapturePoint;
		}

		public void AbandonedCapturePoint()
		{
			if (GetCapturePoint() != null)
			{
				GetCapturePoint().Trigger(-1000356449);
			}
		}
	}

	public class CaptureStates : State
	{
		public class CheerStates : State
		{
			public State pre;

			public State cheer;

			public State pst;
		}

		public class MoveStates : State
		{
			public State movetoranch;

			public State waitforranchertobeready;
		}

		public CheerStates cheer;

		public MoveStates move;

		public State ranching;
	}

	private CaptureStates capture;

	private State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = capture;
		root.Exit("AbandonedCapturePoint", delegate(Instance smi)
		{
			smi.AbandonedCapturePoint();
		});
		capture.EventTransition(GameHashes.CapturePointNoLongerAvailable, null).DefaultState(capture.cheer);
		State state = capture.cheer.DefaultState(capture.cheer.pre);
		string text = CREATURES.STATUSITEMS.EXCITED_TO_BE_RANCHED.NAME;
		string tooltip = CREATURES.STATUSITEMS.EXCITED_TO_BE_RANCHED.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		capture.cheer.pre.ScheduleGoTo(0.9f, capture.cheer.cheer);
		capture.cheer.cheer.Enter("FaceRancher", delegate(Instance smi)
		{
			smi.GetComponent<Facing>().Face(smi.GetCapturePoint().transform.GetPosition());
		}).PlayAnim("excited_loop").OnAnimQueueComplete(capture.cheer.pst);
		capture.cheer.pst.ScheduleGoTo(0.2f, capture.move);
		State state2 = capture.move.DefaultState(capture.move.movetoranch);
		string text2 = CREATURES.STATUSITEMS.GETTING_WRANGLED.NAME;
		string tooltip2 = CREATURES.STATUSITEMS.GETTING_WRANGLED.TOOLTIP;
		main = Db.Get().StatusItemCategories.Main;
		state2.ToggleStatusItem(text2, tooltip2, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		capture.move.movetoranch.Enter("Speedup", delegate(Instance smi)
		{
			smi.GetComponent<Navigator>().defaultSpeed = smi.originalSpeed * 1.25f;
		}).MoveTo(GetTargetCaptureCell, capture.move.waitforranchertobeready).Exit("RestoreSpeed", delegate(Instance smi)
		{
			smi.GetComponent<Navigator>().defaultSpeed = smi.originalSpeed;
		});
		capture.move.waitforranchertobeready.Enter("SetCreatureAtRanchingStation", delegate(Instance smi)
		{
			smi.GetCapturePoint().Trigger(-1992722293);
		}).EventTransition(GameHashes.RancherReadyAtCapturePoint, capture.ranching);
		State ranching = capture.ranching;
		string text3 = CREATURES.STATUSITEMS.GETTING_WRANGLED.NAME;
		string tooltip3 = CREATURES.STATUSITEMS.GETTING_WRANGLED.TOOLTIP;
		main = Db.Get().StatusItemCategories.Main;
		ranching.ToggleStatusItem(text3, tooltip3, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		behaviourcomplete.BehaviourComplete(GameTags.Creatures.WantsToGetCaptured);
	}

	private static FixedCapturePoint.Instance GetCapturePoint(Instance smi)
	{
		return smi.GetSMI<FixedCapturableMonitor.Instance>().targetCapturePoint;
	}

	private static int GetTargetCaptureCell(Instance smi)
	{
		FixedCapturePoint.Instance capturePoint = GetCapturePoint(smi);
		return capturePoint.def.getTargetCapturePoint(capturePoint);
	}
}
