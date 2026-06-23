using Klei.AI;

public class RobotPathFinderAbilities : PathFinderAbilities
{
	public bool canTraverseSubmered;

	private Tag prefabTag;

	public RobotPathFinderAbilities(Navigator navigator)
		: base(navigator)
	{
		KPrefabID component = navigator.GetComponent<KPrefabID>();
		prefabTag = component.PrefabTag;
	}

	protected override void Refresh(Navigator navigator)
	{
		if (PathFinder.IsSubmerged(navigator.cachedCell))
		{
			canTraverseSubmered = true;
			return;
		}
		AttributeInstance attributeInstance = Db.Get().Attributes.MaxUnderwaterTravelCost.Lookup(navigator);
		canTraverseSubmered = attributeInstance == null;
	}

	public override bool TraversePath(ref PathFinder.PotentialPath path, int from_cell, NavType from_nav_type, int cost, int transition_id, bool submerged)
	{
		if (submerged && !canTraverseSubmered)
		{
			return false;
		}
		if (!IsAccessPermitted(prefabTag, path.cell, from_cell, from_nav_type))
		{
			return false;
		}
		return true;
	}

	private static bool IsAccessPermitted(Tag prefabTag, int cell, int from_cell, NavType from_nav_type)
	{
		int tagId = GridRestrictionSerializer.Instance.GetTagId(prefabTag);
		int tagId2 = GridRestrictionSerializer.Instance.GetTagId(GameTags.Robot);
		return Grid.HasPermission(cell, tagId, tagId2, from_cell, from_nav_type);
	}

	public override PathFinderAbilities Clone()
	{
		return new RobotPathFinderAbilities(navigator)
		{
			prefabInstanceID = prefabInstanceID,
			canTraverseSubmered = canTraverseSubmered,
			prefabTag = prefabTag
		};
	}

	public override void RecycleClone()
	{
	}
}
