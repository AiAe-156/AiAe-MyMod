using STRINGS;

public class ConditionRobotPilotReady : ProcessCondition
{
	private LaunchableRocketRegisterType craftRegisterType;

	private RoboPilotModule module;

	private CraftModuleInterface craftInterface;

	public ConditionRobotPilotReady(RoboPilotModule module)
	{
		this.module = module;
		craftRegisterType = module.GetComponent<ILaunchableRocket>().registerType;
		if (craftRegisterType == LaunchableRocketRegisterType.Clustercraft)
		{
			craftInterface = module.GetComponent<RocketModuleCluster>().CraftInterface;
		}
	}

	public override Status EvaluateCondition()
	{
		Status result = Status.Failure;
		switch (craftRegisterType)
		{
		case LaunchableRocketRegisterType.Clustercraft:
			if (HasDestination())
			{
				Clustercraft component = craftInterface.GetComponent<Clustercraft>();
				ClusterTraveler component2 = craftInterface.GetComponent<ClusterTraveler>();
				if (component == null || component2 == null || component2.CurrentPath == null)
				{
					return Status.Failure;
				}
				int num = component2.RemainingTravelNodes();
				bool flag = module.HasResourcesToMove(num * 2);
				bool flag2 = module.HasResourcesToMove(num);
				if (flag)
				{
					result = Status.Ready;
				}
				else if (flag2 || RocketHasDupeControlStation())
				{
					result = Status.Warning;
				}
			}
			break;
		case LaunchableRocketRegisterType.Spacecraft:
		{
			int id = SpacecraftManager.instance.GetSpacecraftFromLaunchConditionManager(module.GetComponent<LaunchConditionManager>()).id;
			SpaceDestination spacecraftDestination = SpacecraftManager.instance.GetSpacecraftDestination(id);
			result = ((spacecraftDestination != null) ? (module.HasResourcesToMove(spacecraftDestination.OneBasedDistance * 2) ? Status.Ready : Status.Failure) : Status.Failure);
			break;
		}
		}
		return result;
	}

	private bool HasDestination()
	{
		if (craftRegisterType == LaunchableRocketRegisterType.Clustercraft)
		{
			CraftModuleInterface craftModuleInterface = module.GetComponent<RocketModuleCluster>().CraftInterface;
			RocketClusterDestinationSelector component = craftModuleInterface.GetComponent<RocketClusterDestinationSelector>();
			if (component.IsAtDestination())
			{
				return false;
			}
			return true;
		}
		if (craftRegisterType == LaunchableRocketRegisterType.Spacecraft)
		{
			int id = SpacecraftManager.instance.GetSpacecraftFromLaunchConditionManager(module.GetComponent<LaunchConditionManager>()).id;
			SpaceDestination spacecraftDestination = SpacecraftManager.instance.GetSpacecraftDestination(id);
			return spacecraftDestination != null;
		}
		return false;
	}

	private bool RocketHasDupeControlStation()
	{
		if (craftInterface != null)
		{
			PassengerRocketModule passengerModule = craftInterface.GetPassengerModule();
			if (passengerModule != null)
			{
				return passengerModule.CheckPilotBoarded();
			}
		}
		return false;
	}

	public override string GetStatusMessage(Status status)
	{
		switch (status)
		{
		case Status.Ready:
			return UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.STATUS.READY;
		case Status.Warning:
			if (RocketHasDupeControlStation())
			{
				return UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.STATUS.WARNING_NO_DATA_BANKS_HUMAN_PILOT;
			}
			return UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.STATUS.WARNING;
		default:
			return UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.STATUS.FAILURE;
		}
	}

	public override string GetStatusTooltip(Status status)
	{
		switch (craftRegisterType)
		{
		case LaunchableRocketRegisterType.Clustercraft:
		{
			ClusterTraveler component = craftInterface.GetComponent<ClusterTraveler>();
			switch (status)
			{
			case Status.Ready:
			{
				if (craftInterface.GetClusterDestinationSelector().IsAtDestination())
				{
					return string.Format(UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.READY_NO_DESTINATION, module.GetDataBanksStored());
				}
				int num4 = component.RemainingTravelNodes() * 2 * module.dataBankConsumption;
				return string.Format(UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.READY, module.GetDataBanksStored(), num4);
			}
			case Status.Warning:
			{
				if (RocketHasDupeControlStation())
				{
					return UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.WARNING_NO_DATA_BANKS_HUMAN_PILOT;
				}
				if (component == null || component.CurrentPath == null)
				{
					return UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.FAILURE_NO_DESTINATION;
				}
				int num5 = component.RemainingTravelNodes() * 2 * module.dataBankConsumption;
				return string.Format(UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.WARNING, module.GetDataBanksStored(), num5);
			}
			default:
			{
				if (!HasDestination() || component == null || component.CurrentPath == null)
				{
					if (module.IsFull())
					{
						return string.Format(UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.READY_NO_DESTINATION, module.GetDataBanksStored());
					}
					return UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.FAILURE_NO_DESTINATION;
				}
				int num3 = component.RemainingTravelNodes();
				return string.Format(UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.FAILURE, num3 * module.dataBankConsumption, module.GetDataBanksStored());
			}
			}
		}
		case LaunchableRocketRegisterType.Spacecraft:
		{
			int id = SpacecraftManager.instance.GetSpacecraftFromLaunchConditionManager(module.GetComponent<LaunchConditionManager>()).id;
			SpaceDestination spacecraftDestination = SpacecraftManager.instance.GetSpacecraftDestination(id);
			switch (status)
			{
			case Status.Ready:
			{
				int num = spacecraftDestination.OneBasedDistance * 2;
				return string.Format(UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.READY, module.GetDataBanksStored(), num);
			}
			case Status.Warning:
				break;
			default:
				if (spacecraftDestination == null)
				{
					if (module.IsFull())
					{
						return string.Format(UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.READY_NO_DESTINATION, module.GetDataBanksStored());
					}
					return UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.FAILURE_NO_DESTINATION;
				}
				return string.Format(UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.FAILURE, spacecraftDestination.OneBasedDistance * 2, module.GetDataBanksStored());
			}
			if (spacecraftDestination != null)
			{
				int num2 = spacecraftDestination.OneBasedDistance * 2;
				return string.Format(UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.WARNING, module.GetDataBanksStored(), num2);
			}
			break;
		}
		}
		DebugUtil.DevAssert(test: false, "Rocket type " + craftRegisterType.ToString() + " does not have a status tooltip for " + status);
		return UI.STARMAP.LAUNCHCHECKLIST.ROBOT_PILOT_DATA_REQUIREMENTS.TOOLTIP.FAILURE_NO_DESTINATION;
	}

	public override bool ShowInUI()
	{
		return true;
	}
}
