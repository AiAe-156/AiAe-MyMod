using STRINGS;
using UnityEngine;

public class LimitOneRoboPilotModule : SelectModuleCondition
{
	public override bool EvaluateCondition(GameObject existingModule, BuildingDef selectedPart, SelectionContext selectionContext)
	{
		if (existingModule == null)
		{
			return true;
		}
		foreach (GameObject item in AttachableBuilding.GetAttachedNetwork(existingModule.GetComponent<AttachableBuilding>()))
		{
			if (selectionContext != SelectionContext.ReplaceModule || !(item == existingModule.gameObject))
			{
				if (item.GetComponent<RoboPilotModule>() != null)
				{
					return false;
				}
				if (item.GetComponent<BuildingUnderConstruction>() != null && item.GetComponent<BuildingUnderConstruction>().Def.BuildingComplete.GetComponent<RoboPilotModule>() != null)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override string GetStatusTooltip(bool ready, GameObject moduleBase, BuildingDef selectedPart)
	{
		if (ready)
		{
			return UI.UISIDESCREENS.SELECTMODULESIDESCREEN.CONSTRAINTS.ONE_ROBOPILOT_PER_ROCKET.COMPLETE;
		}
		return UI.UISIDESCREENS.SELECTMODULESIDESCREEN.CONSTRAINTS.ONE_ROBOPILOT_PER_ROCKET.FAILED;
	}
}
