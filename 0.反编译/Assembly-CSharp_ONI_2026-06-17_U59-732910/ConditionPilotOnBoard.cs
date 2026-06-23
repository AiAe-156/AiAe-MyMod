using STRINGS;

public class ConditionPilotOnBoard : ProcessCondition
{
	private PassengerRocketModule module;

	private RocketModuleCluster rocketModule;

	public ConditionPilotOnBoard(PassengerRocketModule module)
	{
		this.module = module;
		rocketModule = module.GetComponent<RocketModuleCluster>();
	}

	public override Status EvaluateCondition()
	{
		if (module.CheckPilotBoarded())
		{
			return Status.Ready;
		}
		if (rocketModule.CraftInterface.GetRobotPilotModule() != null)
		{
			return Status.Warning;
		}
		return Status.Failure;
	}

	public override string GetStatusMessage(Status status)
	{
		switch (status)
		{
		case Status.Ready:
			return UI.STARMAP.LAUNCHCHECKLIST.PILOT_BOARDED.READY;
		case Status.Warning:
			if (rocketModule.CraftInterface.GetRobotPilotModule() != null)
			{
				return UI.STARMAP.LAUNCHCHECKLIST.PILOT_BOARDED.ROBO_PILOT_WARNING;
			}
			break;
		}
		return UI.STARMAP.LAUNCHCHECKLIST.PILOT_BOARDED.FAILURE;
	}

	public override string GetStatusTooltip(Status status)
	{
		switch (status)
		{
		case Status.Ready:
			return UI.STARMAP.LAUNCHCHECKLIST.PILOT_BOARDED.TOOLTIP.READY;
		case Status.Warning:
			if (rocketModule.CraftInterface.GetRobotPilotModule() != null)
			{
				return UI.STARMAP.LAUNCHCHECKLIST.PILOT_BOARDED.TOOLTIP.ROBO_PILOT_WARNING;
			}
			break;
		}
		return UI.STARMAP.LAUNCHCHECKLIST.PILOT_BOARDED.TOOLTIP.FAILURE;
	}

	public override bool ShowInUI()
	{
		return true;
	}
}
