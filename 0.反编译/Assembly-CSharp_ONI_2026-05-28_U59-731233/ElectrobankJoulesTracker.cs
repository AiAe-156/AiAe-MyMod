public class ElectrobankJoulesTracker : WorldTracker
{
	public ElectrobankJoulesTracker(int worldID)
		: base(worldID)
	{
	}

	public override void UpdateData()
	{
		AddPoint(WorldResourceAmountTracker<ElectrobankTracker>.Get().CountAmount(null, ClusterManager.Instance.GetWorld(base.WorldID).worldInventory));
	}

	public override string FormatValueString(float value)
	{
		return GameUtil.GetFormattedJoules(value);
	}
}
