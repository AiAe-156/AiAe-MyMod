using STRINGS;

public class RocketFuelDiagnostic : ColonyDiagnostic
{
	public RocketFuelDiagnostic(int worldID)
		: base(worldID, UI.COLONY_DIAGNOSTICS.ROCKETFUELDIAGNOSTIC.ALL_NAME)
	{
		tracker = TrackerTool.Instance.GetWorldTracker<RocketFuelTracker>(worldID);
		icon = "rocket_fuel";
	}

	public override string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public override DiagnosticResult Evaluate()
	{
		WorldContainer world = ClusterManager.Instance.GetWorld(base.worldID);
		Clustercraft component = world.gameObject.GetComponent<Clustercraft>();
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, base.NO_MINIONS);
		if (ColonyDiagnosticUtility.IgnoreRocketsWithNoCrewRequested(base.worldID, out result))
		{
			return result;
		}
		result.opinion = DiagnosticResult.Opinion.Normal;
		result.Message = UI.COLONY_DIAGNOSTICS.ROCKETFUELDIAGNOSTIC.NORMAL;
		if (component.ModuleInterface.FuelRemaining == 0f)
		{
			result.opinion = DiagnosticResult.Opinion.Concern;
			result.Message = UI.COLONY_DIAGNOSTICS.ROCKETFUELDIAGNOSTIC.WARNING;
		}
		return result;
	}
}
