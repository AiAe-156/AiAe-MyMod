using Klei.AI;
using STRINGS;

public class SurfaceAirConsumerStates : GameStateMachine<SurfaceAirConsumerStates, SurfaceAirConsumerStates.Instance, IStateMachineTarget, SurfaceAirConsumerStates.Def>
{
	public class Def : BaseDef
	{
		public string effectId;

		public float consumptionRate;

		public float consumeDuration;
	}

	public new class Instance : GameInstance
	{
		[MySmiGet]
		public SurfaceAirConsumerMonitor.Instance monitor;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.WantsToConsumeAir);
		}

		public void ConsumeOxygen(float dt)
		{
			int num = Grid.CellAbove(Grid.PosToCell(this));
			if (Grid.IsValidCell(num))
			{
				SimHashes element = monitor.def.element;
				SimMessages.ConsumeMass(num, element, base.def.consumptionRate * dt, 3);
			}
		}

		public void ApplyEffect()
		{
			Effects component = GetComponent<Effects>();
			if (component != null && !string.IsNullOrEmpty(base.def.effectId))
			{
				component.Add(base.def.effectId, should_save: true);
			}
		}
	}

	public State goingToSurface;

	public State consuming;

	public State consuming_pst;

	public State behaviourComplete;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = goingToSurface;
		State state = goingToSurface.MoveTo((Instance smi) => smi.monitor.targetCell, consuming, behaviourComplete);
		string text = CREATURES.STATUSITEMS.SURFACE_AIR_CONSUMER_MOVING.NAME;
		string tooltip = CREATURES.STATUSITEMS.SURFACE_AIR_CONSUMER_MOVING.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		State state2 = consuming.PlayAnim("breathe_pre").QueueAnim("breathe_loop", loop: true);
		string text2 = CREATURES.STATUSITEMS.SURFACE_AIR_CONSUMER_CONSUMING.NAME;
		string tooltip2 = CREATURES.STATUSITEMS.SURFACE_AIR_CONSUMER_CONSUMING.TOOLTIP;
		main = Db.Get().StatusItemCategories.Main;
		state2.ToggleStatusItem(text2, tooltip2, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main).Update("ConsumeOxygen", delegate(Instance smi, float dt)
		{
			smi.ConsumeOxygen(dt);
		}, UpdateRate.SIM_1000ms).ScheduleGoTo((Instance smi) => smi.def.consumeDuration, consuming_pst);
		consuming_pst.QueueAnim("breathe_pst").OnAnimQueueComplete(behaviourComplete);
		behaviourComplete.Enter(delegate(Instance smi)
		{
			smi.ApplyEffect();
		}).BehaviourComplete(GameTags.Creatures.WantsToConsumeAir);
	}
}
