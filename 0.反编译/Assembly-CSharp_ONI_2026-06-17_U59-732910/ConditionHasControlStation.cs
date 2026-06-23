using STRINGS;

public class ConditionHasControlStation : ProcessCondition
{
	private RocketModuleCluster module;

	public ConditionHasControlStation(RocketModuleCluster module)
	{
		this.module = module;
	}

	public override Status EvaluateCondition()
	{
		Status result = Status.Failure;
		if (Components.RocketControlStations.GetWorldItems(module.CraftInterface.GetComponent<WorldContainer>().id).Count > 0)
		{
			result = Status.Ready;
		}
		else if (module.CraftInterface.GetRobotPilotModule() != null)
		{
			result = Status.Warning;
		}
		return result;
	}

	public override string GetStatusMessage(Status status)
	{
		return status switch
		{
			Status.Ready => UI.STARMAP.LAUNCHCHECKLIST.HAS_CONTROLSTATION.STATUS.READY, 
			Status.Warning => UI.STARMAP.LAUNCHCHECKLIST.HAS_CONTROLSTATION.STATUS.WARNING, 
			_ => UI.STARMAP.LAUNCHCHECKLIST.HAS_CONTROLSTATION.STATUS.FAILURE, 
		};
	}

	public override string GetStatusTooltip(Status status)
	{
		return status switch
		{
			Status.Ready => UI.STARMAP.LAUNCHCHECKLIST.HAS_CONTROLSTATION.TOOLTIP.READY, 
			Status.Warning => UI.STARMAP.LAUNCHCHECKLIST.HAS_CONTROLSTATION.TOOLTIP.WARNING_ROBO_PILOT, 
			_ => UI.STARMAP.LAUNCHCHECKLIST.HAS_CONTROLSTATION.TOOLTIP.FAILURE, 
		};
	}

	public override bool ShowInUI()
	{
		return EvaluateCondition() != Status.Ready;
	}
}
