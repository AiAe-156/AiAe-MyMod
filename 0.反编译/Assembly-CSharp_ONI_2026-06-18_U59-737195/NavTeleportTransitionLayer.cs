public class NavTeleportTransitionLayer : TransitionDriver.OverrideLayer
{
	public NavTeleportTransitionLayer(Navigator navigator)
		: base(navigator)
	{
	}

	public override void BeginTransition(Navigator navigator, Navigator.ActiveTransition transition)
	{
		base.BeginTransition(navigator, transition);
		if (transition.start == NavType.Teleport)
		{
			int num = Grid.PosToCell(navigator);
			Grid.CellToXY(num, out var x, out var y);
			int x2 = x;
			int y2 = y;
			if (navigator.NavGrid.teleportTransitions.TryGetValue(num, out var value))
			{
				Grid.CellToXY(value, out x2, out y2);
			}
			transition.x = x2 - x;
			transition.y = y2 - y;
		}
	}
}
