public class VentController : GameStateMachine<VentController, VentController.Instance, IStateMachineTarget, VentController.Def>
{
	public class Def : BaseDef
	{
		public bool usingDynamicColor;

		public string outputSubstanceAnimName;

		public string outputSubstanceTintSymbolName;
	}

	public new class Instance : GameInstance
	{
		[MyCmpGet]
		private KBatchedAnimController anim;

		[MyCmpGet]
		public Exhaust exhaust;

		private MeterController outputSubstanceMeter;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			if (def.usingDynamicColor)
			{
				outputSubstanceMeter = new MeterController(anim, "meter_target", def.outputSubstanceAnimName, Meter.Offset.NoChange, Grid.SceneLayer.Building);
			}
		}

		public void PlayMeterAnim()
		{
			if (outputSubstanceMeter != null)
			{
				outputSubstanceMeter.meterController.Play(outputSubstanceMeter.meterController.initialAnim, KAnim.PlayMode.Loop);
			}
		}

		public void SetMeterOutputColor(Element element)
		{
			if (outputSubstanceMeter != null)
			{
				GameUtil.TintLiquidSymbolOnBuilding(base.def.outputSubstanceTintSymbolName, outputSubstanceMeter.meterController, element);
			}
		}
	}

	public State off;

	public State working_pre;

	public State working_loop;

	public State working_pst;

	public State closed;

	public BoolParameter isAnimating;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = off;
		root.EventHandler(GameHashes.VentAnimatingChanged, UpdateMeterColor).EventTransition(GameHashes.VentClosed, closed, (Instance smi) => smi.GetComponent<Vent>().Closed()).EventTransition(GameHashes.VentOpen, off, (Instance smi) => !smi.GetComponent<Vent>().Closed());
		off.PlayAnim("off").EventTransition(GameHashes.VentAnimatingChanged, working_pre, IsAnimating);
		working_pre.PlayAnim("working_pre").OnAnimQueueComplete(working_loop);
		working_loop.PlayAnim("working_loop", KAnim.PlayMode.Loop).Enter(PlayOutputMeterAnim).EventTransition(GameHashes.VentAnimatingChanged, working_pst, GameStateMachine<VentController, Instance, IStateMachineTarget, Def>.Not(IsAnimating));
		working_pst.PlayAnim("working_pst").OnAnimQueueComplete(off);
		closed.PlayAnim("closed").EventTransition(GameHashes.VentAnimatingChanged, working_pre, IsAnimating);
	}

	public static void PlayOutputMeterAnim(Instance smi)
	{
		smi.PlayMeterAnim();
	}

	public static bool IsAnimating(Instance smi)
	{
		return smi.exhaust.IsAnimating();
	}

	public static void UpdateMeterColor(Instance smi, object data)
	{
		if (data != null)
		{
			Element meterOutputColor = (Element)data;
			smi.SetMeterOutputColor(meterOutputColor);
		}
	}
}
