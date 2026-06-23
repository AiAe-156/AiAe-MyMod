using System.Collections.Generic;
using STRINGS;

public class StunnedStates : GameStateMachine<StunnedStates, StunnedStates.Instance, IStateMachineTarget, StunnedStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public static readonly Chore.Precondition IsStunned = new Chore.Precondition
		{
			id = "IsStunned",
			fn = delegate(ref Chore.Precondition.Context context, object data)
			{
				return context.consumerState.prefabid.HasAnyTags(StunnedTags);
			}
		};

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(IsStunned);
		}
	}

	private static List<Tag> StunnedTags = new List<Tag>
	{
		GameTags.Creatures.StunnedForCapture,
		GameTags.Creatures.StunnedBeingEaten
	};

	public State init;

	public State stun_for_capture;

	public State stun_for_being_eaten;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = init;
		init.TagTransition(GameTags.Creatures.StunnedForCapture, stun_for_capture).TagTransition(GameTags.Creatures.StunnedBeingEaten, stun_for_being_eaten);
		State state = stun_for_capture;
		string text = CREATURES.STATUSITEMS.GETTING_WRANGLED.NAME;
		string tooltip = CREATURES.STATUSITEMS.GETTING_WRANGLED.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main).PlayAnim("idle_loop", KAnim.PlayMode.Loop).TagTransition(GameTags.Creatures.StunnedForCapture, null, on_remove: true);
		stun_for_being_eaten.PlayAnim("eaten", KAnim.PlayMode.Once).TagTransition(GameTags.Creatures.StunnedBeingEaten, null, on_remove: true);
	}
}
