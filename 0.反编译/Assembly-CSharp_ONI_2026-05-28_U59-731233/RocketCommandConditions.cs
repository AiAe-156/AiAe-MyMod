public class RocketCommandConditions : CommandConditions
{
	public ConditionHasAstronaut hasAstronaut;

	public ConditionPilotOnBoard pilotOnBoard;

	public ConditionPassengersOnBoard passengersOnBoard;

	public ConditionNoExtraPassengers noExtraPassengers;

	public ConditionHasAtmoSuit hasSuit;

	public ConditionHasControlStation hasControlStation;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		RocketModule component = GetComponent<RocketModule>();
		reachable = (ConditionDestinationReachable)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionDestinationReachable(GetComponent<RocketModule>()));
		allModulesComplete = (ConditionAllModulesComplete)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionAllModulesComplete(GetComponent<ILaunchableRocket>()));
		if (GetComponent<ILaunchableRocket>().registerType == LaunchableRocketRegisterType.Spacecraft)
		{
			destHasResources = (ConditionHasMinimumMass)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketStorage, new ConditionHasMinimumMass(GetComponent<CommandModule>()));
			hasAstronaut = (ConditionHasAstronaut)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionHasAstronaut(GetComponent<CommandModule>()));
			hasSuit = (ConditionHasAtmoSuit)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketStorage, new ConditionHasAtmoSuit(GetComponent<CommandModule>()));
			cargoEmpty = (CargoBayIsEmpty)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketStorage, new CargoBayIsEmpty(GetComponent<CommandModule>()));
		}
		else if (GetComponent<ILaunchableRocket>().registerType == LaunchableRocketRegisterType.Clustercraft)
		{
			hasEngine = (ConditionHasEngine)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionHasEngine(GetComponent<ILaunchableRocket>()));
			hasNosecone = (ConditionHasNosecone)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionHasNosecone(GetComponent<LaunchableRocketCluster>()));
			hasControlStation = (ConditionHasControlStation)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionHasControlStation(GetComponent<RocketModuleCluster>()));
			pilotOnBoard = (ConditionPilotOnBoard)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketBoard, new ConditionPilotOnBoard(GetComponent<PassengerRocketModule>()));
			passengersOnBoard = (ConditionPassengersOnBoard)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketBoard, new ConditionPassengersOnBoard(GetComponent<PassengerRocketModule>()));
			noExtraPassengers = (ConditionNoExtraPassengers)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketBoard, new ConditionNoExtraPassengers(GetComponent<PassengerRocketModule>()));
			onLaunchPad = (ConditionOnLaunchPad)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketPrep, new ConditionOnLaunchPad(GetComponent<RocketModuleCluster>().CraftInterface));
		}
		int bufferWidth = 1;
		if (DlcManager.FeatureClusterSpaceEnabled())
		{
			bufferWidth = 0;
		}
		flightPathIsClear = (ConditionFlightPathIsClear)component.AddModuleCondition(ProcessCondition.ProcessConditionType.RocketFlight, new ConditionFlightPathIsClear(base.gameObject, bufferWidth));
	}
}
