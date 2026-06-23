using UnityEngine.Pool;

public class MinionPathFinderAbilities : PathFinderAbilities
{
	private int proxyID;

	private int accessControlDefaultKey;

	private bool out_of_fuel;

	private bool idleNavMaskEnabled;

	private bool hasSwimSkill;

	private static ObjectPool<MinionPathFinderAbilities> pool = new ObjectPool<MinionPathFinderAbilities>(() => new MinionPathFinderAbilities(), null, delegate(MinionPathFinderAbilities obj)
	{
		obj.navigator = null;
		obj.prefabInstanceID = -1;
	}, null, collectionCheck: false, 4, 8);

	public MinionPathFinderAbilities(Navigator navigator)
		: base(navigator)
	{
	}

	private MinionPathFinderAbilities()
		: base(null)
	{
	}

	protected override void Refresh(Navigator navigator)
	{
		MinionAssignablesProxy minionAssignablesProxy = navigator.GetComponent<MinionIdentity>().assignableProxy.Get();
		proxyID = minionAssignablesProxy.GetComponent<KPrefabID>().InstanceID;
		accessControlDefaultKey = GridRestrictionSerializer.Instance.GetTagId(minionAssignablesProxy.GetMinionModel());
		out_of_fuel = navigator.HasTag(GameTags.JetSuitOutOfFuel);
		hasSwimSkill = navigator.GetComponent<MinionResume>().HasPerk(Db.Get().SkillPerks.CanSwim);
	}

	public void SetIdleNavMaskEnabled(bool enabled)
	{
		idleNavMaskEnabled = enabled;
	}

	private static bool IsAccessPermitted(int proxyID, int proxyTag, int cell, int from_cell, NavType from_nav_type)
	{
		return Grid.HasPermission(cell, proxyID, proxyTag, from_cell, from_nav_type);
	}

	public override int GetSubmergedPathCostPenalty(PathFinder.PotentialPath path, NavGrid.Link link)
	{
		bool flag = path.HasAnyFlag(PathFinder.PotentialPath.Flags.HasAtmoSuit | PathFinder.PotentialPath.Flags.HasJetPack | PathFinder.PotentialPath.Flags.HasLeadSuit);
		bool flag2 = link.endNavType == NavType.Swim;
		if (!hasSwimSkill)
		{
			if (!flag)
			{
				return link.cost * 2;
			}
			return 0;
		}
		if (flag && flag2)
		{
			return link.cost * 50;
		}
		if (!flag && !flag2)
		{
			return link.cost * 2;
		}
		if (!flag && flag2 && PathFinder.IsSubmerged(link.link))
		{
			return link.cost / 2;
		}
		return 0;
	}

	public override bool TraversePath(ref PathFinder.PotentialPath path, int from_cell, NavType from_nav_type, int cost, int transition_id, bool submerged)
	{
		if (!IsAccessPermitted(proxyID, accessControlDefaultKey, path.cell, from_cell, from_nav_type))
		{
			return false;
		}
		CellOffset[] voidOffsets = navigator.NavGrid.transitions[transition_id].voidOffsets;
		foreach (CellOffset offset in voidOffsets)
		{
			int cell = Grid.OffsetCell(from_cell, offset);
			if (!IsAccessPermitted(proxyID, accessControlDefaultKey, cell, from_cell, from_nav_type))
			{
				return false;
			}
		}
		if (path.navType == NavType.Tube && from_nav_type == NavType.Floor && !Grid.HasUsableTubeEntrance(from_cell, prefabInstanceID))
		{
			return false;
		}
		if (!hasSwimSkill && (path.navType == NavType.Swim || from_nav_type == NavType.Swim))
		{
			return false;
		}
		if (path.navType == NavType.Hover && (out_of_fuel || !path.HasFlag(PathFinder.PotentialPath.Flags.HasJetPack)))
		{
			return false;
		}
		Grid.SuitMarker.Flags flags = (Grid.SuitMarker.Flags)0;
		PathFinder.PotentialPath.Flags pathFlags = PathFinder.PotentialPath.Flags.None;
		bool flag = path.HasFlag(PathFinder.PotentialPath.Flags.PerformSuitChecks) && Grid.TryGetSuitMarkerFlags(from_cell, out flags, out pathFlags) && (flags & Grid.SuitMarker.Flags.Operational) != 0;
		bool flag2 = SuitMarker.DoesTraversalDirectionRequireSuit(from_cell, path.cell, flags);
		bool flag3 = path.HasAnyFlag(PathFinder.PotentialPath.Flags.HasAtmoSuit | PathFinder.PotentialPath.Flags.HasJetPack | PathFinder.PotentialPath.Flags.HasOxygenMask | PathFinder.PotentialPath.Flags.HasLeadSuit);
		if (flag)
		{
			bool flag4 = path.HasFlag(pathFlags);
			if (flag2)
			{
				if (!flag3 && !Grid.HasSuit(from_cell, prefabInstanceID))
				{
					return false;
				}
			}
			else if (flag3 && (flags & Grid.SuitMarker.Flags.OnlyTraverseIfUnequipAvailable) != 0 && (!flag4 || !Grid.HasEmptyLocker(from_cell, prefabInstanceID)))
			{
				return false;
			}
		}
		if (idleNavMaskEnabled && (Grid.PreventIdleTraversal[path.cell] || Grid.PreventIdleTraversal[from_cell]))
		{
			return false;
		}
		if (flag)
		{
			if (flag2)
			{
				if (!flag3)
				{
					path.SetFlags(pathFlags);
				}
			}
			else
			{
				path.ClearFlags(PathFinder.PotentialPath.Flags.HasAtmoSuit | PathFinder.PotentialPath.Flags.HasJetPack | PathFinder.PotentialPath.Flags.HasOxygenMask | PathFinder.PotentialPath.Flags.HasLeadSuit);
			}
		}
		return true;
	}

	public override PathFinderAbilities Clone()
	{
		MinionPathFinderAbilities minionPathFinderAbilities = pool.Get();
		minionPathFinderAbilities.navigator = navigator;
		minionPathFinderAbilities.prefabInstanceID = prefabInstanceID;
		minionPathFinderAbilities.proxyID = proxyID;
		minionPathFinderAbilities.accessControlDefaultKey = accessControlDefaultKey;
		minionPathFinderAbilities.out_of_fuel = out_of_fuel;
		minionPathFinderAbilities.idleNavMaskEnabled = idleNavMaskEnabled;
		minionPathFinderAbilities.hasSwimSkill = hasSwimSkill;
		return minionPathFinderAbilities;
	}

	public override void RecycleClone()
	{
		pool.Release(this);
	}
}
