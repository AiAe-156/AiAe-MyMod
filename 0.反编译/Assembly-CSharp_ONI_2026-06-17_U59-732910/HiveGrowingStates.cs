using STRINGS;

public class HiveGrowingStates : GameStateMachine<HiveGrowingStates, HiveGrowingStates.Instance, IStateMachineTarget, HiveGrowingStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		[MySmiReq]
		public BeeHive.StatesInstance hive;

		[MyCmpReq]
		private KAnimControllerBase animController;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.Behaviours.GrowUpBehaviour);
		}

		public void RefreshPositionPercent()
		{
			animController.SetPositionPercent(hive.sm.hiveGrowth.Get(hive));
		}
	}

	public class GrowUpStates : State
	{
		public State loop;

		public State pst;
	}

	public GrowUpStates growing;

	public State behaviourcomplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = growing;
		State state = root;
		string text = CREATURES.STATUSITEMS.GROWINGUP.NAME;
		string tooltip = CREATURES.STATUSITEMS.GROWINGUP.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		growing.DefaultState(growing.loop);
		growing.loop.PlayAnim((Instance smi) => "grow", KAnim.PlayMode.Paused).Enter(delegate(Instance smi)
		{
			smi.RefreshPositionPercent();
		}).Update(delegate(Instance smi, float dt)
		{
			smi.RefreshPositionPercent();
			if (smi.hive.IsFullyGrown())
			{
				smi.GoTo(growing.pst);
			}
		}, UpdateRate.SIM_4000ms);
		growing.pst.PlayAnim("grow_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(behaviourcomplete);
		behaviourcomplete.BehaviourComplete(GameTags.Creatures.Behaviours.GrowUpBehaviour);
	}
}
