using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class BreathabilityDiagnostic : ColonyDiagnostic
{
	public BreathabilityDiagnostic(int worldID)
		: base(worldID, UI.COLONY_DIAGNOSTICS.BREATHABILITYDIAGNOSTIC.ALL_NAME)
	{
		tracker = TrackerTool.Instance.GetWorldTracker<BreathabilityTracker>(worldID);
		trackerSampleCountSeconds = 50f;
		icon = "overlay_oxygen";
		AddCriterion("CheckSuffocation", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.BREATHABILITYDIAGNOSTIC.CRITERIA.CHECKSUFFOCATION, CheckSuffocation));
		AddCriterion("CheckLowBreathability", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.BREATHABILITYDIAGNOSTIC.CRITERIA.CHECKLOWBREATHABILITY, CheckLowBreathability));
		AddCriterion("CheckBionicOxygen", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.BREATHABILITYDIAGNOSTIC.CRITERIA.CHECKLOWBIONICOXYGEN, CheckLowBionicOxygen));
	}

	private DiagnosticResult CheckSuffocation()
	{
		List<MinionIdentity> worldItems = Components.LiveMinionIdentities.GetWorldItems(base.worldID);
		if (worldItems.Count != 0)
		{
			foreach (MinionIdentity item in worldItems)
			{
				SuffocationMonitor.Instance sMI = item.GetSMI<SuffocationMonitor.Instance>();
				if (sMI != null && sMI.IsInsideState(sMI.sm.noOxygen.suffocating))
				{
					return new DiagnosticResult(DiagnosticResult.Opinion.DuplicantThreatening, UI.COLONY_DIAGNOSTICS.BREATHABILITYDIAGNOSTIC.SUFFOCATING, new Tuple<Vector3, GameObject>(sMI.transform.position, sMI.gameObject));
				}
			}
			return new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.BREATHABILITYDIAGNOSTIC.NORMAL);
		}
		return new DiagnosticResult(DiagnosticResult.Opinion.Normal, base.NO_MINIONS);
	}

	private DiagnosticResult CheckLowBreathability()
	{
		List<MinionIdentity> worldItems = Components.LiveMinionIdentities.GetWorldItems(base.worldID);
		if (worldItems.Count != 0 && tracker.GetAverageValue(trackerSampleCountSeconds) < 60f)
		{
			return new DiagnosticResult(DiagnosticResult.Opinion.Concern, UI.COLONY_DIAGNOSTICS.BREATHABILITYDIAGNOSTIC.POOR);
		}
		return new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.BREATHABILITYDIAGNOSTIC.NORMAL);
	}

	private DiagnosticResult CheckLowBionicOxygen()
	{
		List<MinionIdentity> worldItems = Components.LiveMinionIdentities.GetWorldItems(base.worldID);
		if (worldItems.Count != 0)
		{
			foreach (MinionIdentity item in worldItems)
			{
				if (item.HasTag(GameTags.Minions.Models.Bionic))
				{
					BionicOxygenTankMonitor.Instance sMI = item.GetSMI<BionicOxygenTankMonitor.Instance>();
					if (sMI.OxygenPercentage <= 0f)
					{
						return new DiagnosticResult(DiagnosticResult.Opinion.DuplicantThreatening, UI.COLONY_DIAGNOSTICS.BREATHABILITYDIAGNOSTIC.NEAR_OR_EMPTY_BIONIC_TANKS, new Tuple<Vector3, GameObject>(item.transform.position, item.gameObject));
					}
					if (sMI.OxygenPercentage < 0.5f)
					{
						return new DiagnosticResult(DiagnosticResult.Opinion.Concern, UI.COLONY_DIAGNOSTICS.BREATHABILITYDIAGNOSTIC.POOR_BIONIC_TANKS, new Tuple<Vector3, GameObject>(item.transform.position, item.gameObject));
					}
				}
			}
		}
		return new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.BREATHABILITYDIAGNOSTIC.NORMAL);
	}

	public override DiagnosticResult Evaluate()
	{
		if (ColonyDiagnosticUtility.IgnoreRocketsWithNoCrewRequested(base.worldID, out var result))
		{
			return result;
		}
		return base.Evaluate();
	}
}
