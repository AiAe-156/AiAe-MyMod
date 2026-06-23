using System.Collections.Generic;
using STRINGS;

public class ToiletDiagnostic : ColonyDiagnostic
{
	private const bool INCLUDE_CHILD_WORLDS = true;

	private List<MinionIdentity> minionsWithBladders;

	private List<IUsable> toilets;

	private readonly string NO_MINIONS_WITH_BLADDER;

	public ToiletDiagnostic(int worldID)
		: base(worldID, UI.COLONY_DIAGNOSTICS.TOILETDIAGNOSTIC.ALL_NAME)
	{
		icon = "icon_action_region_toilet";
		tracker = TrackerTool.Instance.GetWorldTracker<WorkingToiletTracker>(worldID);
		NO_MINIONS_WITH_BLADDER = (base.IsWorldModuleInterior ? UI.COLONY_DIAGNOSTICS.TOILETDIAGNOSTIC.NO_MINIONS_ROCKET : UI.COLONY_DIAGNOSTICS.TOILETDIAGNOSTIC.NO_MINIONS_PLANETOID);
		AddCriterion("CheckHasAnyToilets", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.TOILETDIAGNOSTIC.CRITERIA.CHECKHASANYTOILETS, CheckHasAnyToilets));
		AddCriterion("CheckEnoughToilets", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.TOILETDIAGNOSTIC.CRITERIA.CHECKENOUGHTOILETS, CheckEnoughToilets));
		AddCriterion("CheckBladders", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.TOILETDIAGNOSTIC.CRITERIA.CHECKBLADDERS, CheckBladders));
	}

	private DiagnosticResult CheckHasAnyToilets()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
		if (minionsWithBladders.Count == 0)
		{
			result.opinion = DiagnosticResult.Opinion.Normal;
			result.Message = NO_MINIONS_WITH_BLADDER;
		}
		else if (toilets.Count == 0)
		{
			result.opinion = DiagnosticResult.Opinion.Concern;
			result.Message = UI.COLONY_DIAGNOSTICS.TOILETDIAGNOSTIC.NO_TOILETS;
		}
		return result;
	}

	private DiagnosticResult CheckEnoughToilets()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
		if (minionsWithBladders.Count == 0)
		{
			result.opinion = DiagnosticResult.Opinion.Normal;
			result.Message = NO_MINIONS_WITH_BLADDER;
		}
		else
		{
			result.opinion = DiagnosticResult.Opinion.Normal;
			result.Message = UI.COLONY_DIAGNOSTICS.TOILETDIAGNOSTIC.NORMAL;
			if (tracker.GetDataTimeLength() > 10f && tracker.GetAverageValue(trackerSampleCountSeconds) <= 0f)
			{
				result.opinion = DiagnosticResult.Opinion.Concern;
				result.Message = UI.COLONY_DIAGNOSTICS.TOILETDIAGNOSTIC.NO_WORKING_TOILETS;
			}
		}
		return result;
	}

	private DiagnosticResult CheckBladders()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
		if (minionsWithBladders.Count == 0)
		{
			result.opinion = DiagnosticResult.Opinion.Normal;
			result.Message = NO_MINIONS_WITH_BLADDER;
		}
		else
		{
			result.opinion = DiagnosticResult.Opinion.Normal;
			result.Message = UI.COLONY_DIAGNOSTICS.TOILETDIAGNOSTIC.NORMAL;
			WorldContainer world = ClusterManager.Instance.GetWorld(base.worldID);
			foreach (PeeChoreMonitor.Instance item in Components.CriticalBladders.Items)
			{
				int myWorldId = item.master.gameObject.GetMyWorldId();
				if (myWorldId == base.worldID || world.GetChildWorldIds().Contains(myWorldId))
				{
					result.opinion = DiagnosticResult.Opinion.Warning;
					result.Message = UI.COLONY_DIAGNOSTICS.TOILETDIAGNOSTIC.TOILET_URGENT;
					break;
				}
			}
		}
		return result;
	}

	private bool MinionFilter(MinionIdentity minion)
	{
		return minion.modifiers.amounts.Has(Db.Get().Amounts.Bladder);
	}

	public override DiagnosticResult Evaluate()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, NO_MINIONS_WITH_BLADDER);
		if (ColonyDiagnosticUtility.IgnoreRocketsWithNoCrewRequested(base.worldID, out result))
		{
			return result;
		}
		RefreshData();
		return base.Evaluate();
	}

	private void RefreshData()
	{
		minionsWithBladders = Components.LiveMinionIdentities.GetWorldItems(base.worldID, checkChildWorlds: true, MinionFilter);
		toilets = Components.Toilets.GetWorldItems(base.worldID, checkChildWorlds: true);
	}

	public override string GetAverageValueString()
	{
		if (minionsWithBladders == null || minionsWithBladders.Count == 0)
		{
			RefreshData();
		}
		int num = toilets.Count;
		for (int i = 0; i < toilets.Count; i++)
		{
			if (!toilets[i].IsNullOrDestroyed() && !toilets[i].IsUsable())
			{
				num--;
			}
		}
		return num + ":" + minionsWithBladders.Count;
	}
}
