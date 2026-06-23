using UnityEngine;

public class SplashTransitionLayer : TransitionDriver.OverrideLayer
{
	private float lastSplashTime;

	private const float SPLASH_INTERVAL = 1f;

	public SplashTransitionLayer(Navigator navigator)
		: base(navigator)
	{
		lastSplashTime = Time.time;
	}

	private void RefreshSplashes(Navigator navigator, Navigator.ActiveTransition transition)
	{
		if (!(navigator == null) && !navigator.IsSwimming() && transition.end != NavType.Tube)
		{
			Vector3 position = navigator.transform.GetPosition();
			if (lastSplashTime + 1f < Time.time && Grid.Element[Grid.PosToCell(position)].IsLiquid)
			{
				lastSplashTime = Time.time;
				Game.Instance.SpawnFX(SpawnFXHashes.SplashStep, position + new Vector3(0f, 0.75f, -0.1f), 0f);
			}
		}
	}

	public override void BeginTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		base.BeginTransition(navigator, transition);
		RefreshSplashes(navigator, transition);
	}

	public override void UpdateTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		base.UpdateTransition(navigator, transition);
		RefreshSplashes(navigator, transition);
	}

	public override void EndTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		base.EndTransition(navigator, transition);
		RefreshSplashes(navigator, transition);
	}
}
