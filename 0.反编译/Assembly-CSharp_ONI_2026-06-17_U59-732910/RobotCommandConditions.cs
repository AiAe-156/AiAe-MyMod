public class RobotCommandConditions : CommandConditions
{
	public ConditionRobotPilotReady robotPilotReady;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		RocketModule component = GetComponent<RocketModule>();
		reachable = (ConditionDestinationReachable)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionDestinationReachable(GetComponent<RocketModule>()));
		allModulesComplete = (ConditionAllModulesComplete)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionAllModulesComplete(GetComponent<ILaunchableRocket>()));
		if (GetComponent<ILaunchableRocket>().registerType == LaunchableRocketRegisterType.Spacecraft)
		{
			destHasResources = (ConditionHasMinimumMass)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketStorage, new ConditionHasMinimumMass(GetComponent<CommandModule>()));
			cargoEmpty = (CargoBayIsEmpty)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketStorage, new CargoBayIsEmpty(GetComponent<CommandModule>()));
		}
		else if (GetComponent<ILaunchableRocket>().registerType == LaunchableRocketRegisterType.Clustercraft)
		{
			hasEngine = (ConditionHasEngine)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionHasEngine(GetComponent<ILaunchableRocket>()));
			hasNosecone = (ConditionHasNosecone)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionHasNosecone(GetComponent<LaunchableRocketCluster>()));
			onLaunchPad = (ConditionOnLaunchPad)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionOnLaunchPad(GetComponent<RocketModuleCluster>().CraftInterface));
		}
		int bufferWidth = 1;
		if (DlcManager.FeatureClusterSpaceEnabled())
		{
			bufferWidth = 0;
		}
		flightPathIsClear = (ConditionFlightPathIsClear)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketFlight, new ConditionFlightPathIsClear(base.gameObject, bufferWidth));
		robotPilotReady = (ConditionRobotPilotReady)component.AddModuleCondition((GetComponent<ILaunchableRocket>().registerType == LaunchableRocketRegisterType.Spacecraft) ? ProcessCondition.ProcessConditionType.RocketPrep : ProcessCondition.ProcessConditionType.RocketFlight, new ConditionRobotPilotReady(GetComponent<RoboPilotModule>()));
	}
}
