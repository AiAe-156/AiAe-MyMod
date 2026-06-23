public class BuildingSubmergable : Submergable
{
	public static Operational.Flag notSubmergedFlag = new Operational.Flag("submerged", Operational.Flag.Type.Functional);

	[MyCmpReq]
	private Operational operational;

	private static StatusItem GetSubmergableStatusItem()
	{
		return Db.Get().BuildingStatusItems.NotSubmerged;
	}

	protected override void OnSpawn()
	{
		operational.SetFlag(notSubmergedFlag, isSubmerged);
		GetStatusItem = GetSubmergableStatusItem;
		base.OnSpawn();
	}

	protected override void OnSubmergedStateChanged()
	{
		operational.SetFlag(notSubmergedFlag, isSubmerged);
		base.OnSubmergedStateChanged();
	}
}
