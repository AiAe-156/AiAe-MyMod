using STRINGS;

public class DisabledCreatureStates : GameStateMachine<DisabledCreatureStates, DisabledCreatureStates.Instance, IStateMachineTarget, DisabledCreatureStates.Def>
{
	public class Def : BaseDef
	{
		public string disabledAnim = "off";

		public Def(string anim)
		{
			disabledAnim = anim;
		}
	}

	public new class Instance : GameInstance
	{
		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.HasTag, GameTags.Creatures.Behaviours.DisableCreature);
		}
	}

	public State disableCreature;

	public State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = disableCreature;
		State state = root;
		string text = CREATURES.STATUSITEMS.DISABLED.NAME;
		string tooltip = CREATURES.STATUSITEMS.DISABLED.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main).TagTransition(GameTags.Creatures.Behaviours.DisableCreature, behaviourcomplete, on_remove: true);
		disableCreature.PlayAnim((Instance smi) => smi.def.disabledAnim);
		behaviourcomplete.BehaviourComplete(GameTags.Creatures.Behaviours.DisableCreature);
	}
}
