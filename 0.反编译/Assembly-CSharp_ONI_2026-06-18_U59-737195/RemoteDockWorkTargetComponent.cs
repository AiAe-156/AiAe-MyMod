public abstract class RemoteDockWorkTargetComponent : KMonoBehaviour, IRemoteDockWorkTarget
{
	public abstract Chore RemoteDockChore { get; }

	public virtual IApproachable Approachable => base.gameObject.GetComponent<IApproachable>();

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Components.RemoteDockWorkTargets.Add(base.gameObject.GetMyWorldId(), this);
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		Components.RemoteDockWorkTargets.Remove(base.gameObject.GetMyWorldId(), this);
	}
}
