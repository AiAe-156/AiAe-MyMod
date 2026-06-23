public class FullPuftTransitionLayer : TransitionDriver.OverrideLayer
{
	private CreatureCalorieMonitor.Instance calorie_monitor;

	public FullPuftTransitionLayer(Navigator navigator)
		: base(navigator)
	{
		calorie_monitor = navigator.GetSMI<CreatureCalorieMonitor.Instance>();
	}

	public override void BeginTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		base.BeginTransition(navigator, transition);
		if (calorie_monitor != null && calorie_monitor.stomach.IsReadyToPoop())
		{
			string text = HashCache.Get().Get(transition.anim.HashValue) + "_full";
			if (navigator.animController.HasAnimation(text))
			{
				transition.anim = text;
			}
		}
	}
}
