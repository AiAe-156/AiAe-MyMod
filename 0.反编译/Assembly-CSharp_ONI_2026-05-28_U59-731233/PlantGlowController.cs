public class PlantGlowController : StateMachineComponent<PlantGlowController.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, PlantGlowController, object>.GameInstance
	{
		[MyCmpGet]
		public Light2D light2D;

		public StatesInstance(PlantGlowController master)
			: base(master)
		{
		}
	}

	public class States : GameStateMachine<States, StatesInstance, PlantGlowController>
	{
		public State emitting;

		public State wilted;

		public State dead;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = wilted;
			root.TagTransition(GameTags.Dead, dead);
			wilted.EventTransition(GameHashes.WiltRecover, emitting).Enter(delegate(StatesInstance smi)
			{
				smi.light2D.enabled = false;
			});
			emitting.EventTransition(GameHashes.Wilt, wilted).Enter(delegate(StatesInstance smi)
			{
				smi.light2D.enabled = true;
			});
			dead.DoNothing();
		}
	}

	protected override void OnSpawn()
	{
		base.smi.StartSM();
	}
}
