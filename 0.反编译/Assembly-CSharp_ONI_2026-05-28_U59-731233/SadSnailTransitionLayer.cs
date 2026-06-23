public class SadSnailTransitionLayer : TransitionDriver.OverrideLayer
{
	private DesiccationMonitor.Instance desiccationMonitor;

	public SadSnailTransitionLayer(Navigator navigator)
		: base(navigator)
	{
		desiccationMonitor = navigator.GetSMI<DesiccationMonitor.Instance>();
	}

	public override void BeginTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		base.BeginTransition(navigator, transition);
		if (desiccationMonitor != null && desiccationMonitor.IsDesiccating())
		{
			string text = HashCache.Get().Get(transition.anim.HashValue) + "_sad";
			if (navigator.animController.HasAnimation(text))
			{
				transition.anim = text;
			}
		}
	}
}
