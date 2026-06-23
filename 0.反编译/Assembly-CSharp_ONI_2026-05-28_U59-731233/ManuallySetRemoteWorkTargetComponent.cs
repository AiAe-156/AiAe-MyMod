public class ManuallySetRemoteWorkTargetComponent : RemoteDockWorkTargetComponent
{
	private Chore chore = null;

	public override Chore RemoteDockChore => chore;

	public void SetChore(Chore chore_)
	{
		chore = chore_;
	}
}
