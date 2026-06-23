using Klei.Actions;
using UnityEngine;

namespace Klei.Input;

[ActionType("InterfaceTool", "Dig", true)]
public abstract class DigAction
{
	public void Uproot(int cell)
	{
		if (Grid.ObjectLayers[1].ContainsKey(cell))
		{
			GameObject gameObject = Grid.ObjectLayers[1][cell];
			if (!(gameObject == null))
			{
				IDigActionEntity component = gameObject.GetComponent<IDigActionEntity>();
				EntityDig(component);
			}
		}
		else if (Grid.ObjectLayers[5].ContainsKey(cell))
		{
			GameObject gameObject2 = Grid.ObjectLayers[5][cell];
			if (!(gameObject2 == null))
			{
				IDigActionEntity component2 = gameObject2.GetComponent<IDigActionEntity>();
				EntityDig(component2);
			}
		}
	}

	public abstract void Dig(int cell, int distFromOrigin);

	protected abstract void EntityDig(IDigActionEntity digAction);
}
