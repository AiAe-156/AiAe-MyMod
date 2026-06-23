using STRINGS;

public class IdleStandStillStates : GameStateMachine<IdleStandStillStates, IdleStandStillStates.Instance, IStateMachineTarget, IdleStandStillStates.Def>
{
	public class Def : BaseDef
	{
		public delegate HashedString IdleAnimCallback(Instance smi, ref HashedString pre_anim);

		public IdleAnimCallback customIdleAnim;
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
		}
	}

	private State loop;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = loop;
		State state = root;
		string text = CREATURES.STATUSITEMS.IDLE.NAME;
		string tooltip = CREATURES.STATUSITEMS.IDLE.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main).ToggleTag(GameTags.Idle);
		loop.Enter(PlayIdle);
	}

	public void PlayIdle(Instance smi)
	{
		KAnimControllerBase component = smi.GetComponent<KAnimControllerBase>();
		if (smi.def.customIdleAnim != null)
		{
			HashedString pre_anim = HashedString.Invalid;
			HashedString hashedString = smi.def.customIdleAnim(smi, ref pre_anim);
			if (hashedString != HashedString.Invalid)
			{
				if (pre_anim != HashedString.Invalid)
				{
					component.Play(pre_anim);
				}
				component.Queue(hashedString, KAnim.PlayMode.Loop);
				return;
			}
		}
		component.Play("idle", KAnim.PlayMode.Loop);
	}
}
