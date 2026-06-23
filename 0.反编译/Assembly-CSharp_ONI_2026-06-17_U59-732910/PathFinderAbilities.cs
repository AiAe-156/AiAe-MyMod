public abstract class PathFinderAbilities
{
	protected Navigator navigator;

	protected int prefabInstanceID;

	public PathFinderAbilities(Navigator navigator)
	{
		this.navigator = navigator;
	}

	public virtual string KPROFILER_getName()
	{
		return null;
	}

	public void Refresh()
	{
		prefabInstanceID = navigator.gameObject.GetComponent<KPrefabID>().InstanceID;
		navigator.cachedCell = Grid.PosToCell(navigator);
		Refresh(navigator);
	}

	protected abstract void Refresh(Navigator navigator);

	public abstract PathFinderAbilities Clone();

	public abstract void RecycleClone();

	public abstract bool TraversePath(ref PathFinder.PotentialPath path, int from_cell, NavType from_nav_type, int cost, int transition_id, bool submerged);

	public virtual int GetSubmergedPathCostPenalty(PathFinder.PotentialPath path, NavGrid.Link link)
	{
		return 0;
	}
}
