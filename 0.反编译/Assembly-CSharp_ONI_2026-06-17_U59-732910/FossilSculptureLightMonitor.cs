public class FossilSculptureLightMonitor : GameStateMachine<FossilSculptureLightMonitor, FossilSculptureLightMonitor.Instance, IStateMachineTarget, FossilSculptureLightMonitor.Def>
{
	public class Def : BaseDef
	{
		public bool usingBloom = true;
	}

	public new class Instance : GameInstance
	{
		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			SetAnimLitState(lit: false);
		}

		public void SetAnimLitState(bool lit)
		{
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			component.SetSymbolVisiblity("statue_light_bloom", base.def.usingBloom && lit);
			component.SetSymbolVisiblity("shading_with_light", lit);
			component.SetSymbolVisiblity("shading_no_light", !lit);
		}
	}

	public const string LIT_LIGHT_BLOOM_SYMBOL_NAME = "statue_light_bloom";

	public const string LIT_SHADING_SYMBOL_NAME = "shading_with_light";

	public const string UNLIT_SHADING_SYMBOL_NAME = "shading_no_light";

	public State noLit;

	public State lit;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = noLit;
		noLit.TagTransition(GameTags.Operational, lit).EventHandler(GameHashes.WorkableCompleteWork, HideLitEffect).EventHandler(GameHashes.ArtableStateChanged, HideLitEffect)
			.Enter(HideLitEffect);
		lit.TagTransition(GameTags.Operational, noLit, on_remove: true).EventHandler(GameHashes.WorkableCompleteWork, ShowLitEffect).EventHandler(GameHashes.ArtableStateChanged, ShowLitEffect)
			.Enter(ShowLitEffect);
	}

	public static void ShowLitEffect(Instance smi)
	{
		smi.SetAnimLitState(lit: true);
	}

	public static void HideLitEffect(Instance smi)
	{
		smi.SetAnimLitState(lit: false);
	}
}
