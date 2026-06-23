using Klei.Actions;

namespace Klei.Input;

[Action("Immediate")]
public class ImmediateDigAction : DigAction
{
	public override void Dig(int cell, int distFromOrigin)
	{
		if (DigTool.Instance.IsActiveLayer(ToolParameterMenu.FILTERLAYERS.TILES) && Grid.Solid[cell] && !Grid.Foundation[cell])
		{
			SimMessages.Dig(cell);
		}
		else if (DigTool.Instance.IsActiveLayer(ToolParameterMenu.FILTERLAYERS.NATURALBACKWALL) && BackwallManager.HasBackwall(cell))
		{
			SimMessages.Dig(cell, -1, skipEvent: false, backwall: true);
		}
	}

	protected override void EntityDig(IDigActionEntity digActionEntity)
	{
		digActionEntity?.Dig();
	}
}
