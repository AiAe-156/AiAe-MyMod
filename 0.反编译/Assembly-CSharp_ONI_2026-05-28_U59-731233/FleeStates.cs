using STRINGS;

public class FleeStates : GameStateMachine<FleeStates, FleeStates.Instance, IStateMachineTarget, FleeStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.Flee);
			base.sm.mover.Set(GetComponent<Navigator>(), base.smi);
		}
	}

	private TargetParameter mover;

	public TargetParameter fleeToTarget;

	public State plan;

	public ApproachSubState<IApproachable> approach;

	public State cower;

	public State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = plan;
		State state = root.Enter("SetFleeTarget", delegate(Instance smi)
		{
			fleeToTarget.Set(CreatureHelpers.GetFleeTargetLocatorObject(smi.master.gameObject, smi.GetSMI<ThreatMonitor.Instance>().MainThreat), smi);
		});
		string text = CREATURES.STATUSITEMS.FLEEING.NAME;
		string tooltip = CREATURES.STATUSITEMS.FLEEING.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		plan.Enter(delegate(Instance smi)
		{
			ThreatMonitor.Instance sMI = smi.master.gameObject.GetSMI<ThreatMonitor.Instance>();
			fleeToTarget.Set(CreatureHelpers.GetFleeTargetLocatorObject(smi.master.gameObject, sMI.MainThreat), smi);
			if (fleeToTarget.Get(smi) != null)
			{
				smi.GoTo(approach);
			}
			else
			{
				smi.GoTo(cower);
			}
		});
		approach.InitializeStates(mover, fleeToTarget, cower, cower, null, NavigationTactics.ReduceTravelDistance).Enter(delegate(Instance smi)
		{
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, CREATURES.STATUSITEMS.FLEEING.NAME.text, smi.master.transform);
		});
		cower.Enter(delegate(Instance smi)
		{
			string text2 = "DEFAULT COWER ANIMATION";
			if (smi.Get<KBatchedAnimController>().HasAnimation("cower"))
			{
				text2 = "cower";
			}
			else if (smi.Get<KBatchedAnimController>().HasAnimation("idle"))
			{
				text2 = "idle";
			}
			else if (smi.Get<KBatchedAnimController>().HasAnimation("idle_loop"))
			{
				text2 = "idle_loop";
			}
			smi.Get<KBatchedAnimController>().Play(text2, KAnim.PlayMode.Loop);
		}).ScheduleGoTo(2f, behaviourcomplete);
		behaviourcomplete.BehaviourComplete(GameTags.Creatures.Flee);
	}
}
