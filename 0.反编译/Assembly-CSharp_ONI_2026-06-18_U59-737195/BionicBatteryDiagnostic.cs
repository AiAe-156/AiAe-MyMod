using STRINGS;
using UnityEngine;

public class BionicBatteryDiagnostic : BionicColonyDiagnostic
{
	private float bionicJoulesPerCycle;

	private float recommendedJoulesPerBionic;

	private float multiplier = 1f;

	public BionicBatteryDiagnostic(int worldID)
		: base(worldID, UI.COLONY_DIAGNOSTICS.BIONICBATTERYDIAGNOSTIC.ALL_NAME)
	{
		tracker = TrackerTool.Instance.GetWorldTracker<ElectrobankJoulesTracker>(worldID);
		icon = "BionicPower";
		trackerSampleCountSeconds = 150f;
		presentationSetting = PresentationSetting.CurrentValue;
		AddCriterion("CheckEnoughBatteries", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.BIONICBATTERYDIAGNOSTIC.CRITERIA.CHECKENOUGHBATTERIES, CheckEnoughBatteries));
		AddCriterion("CheckPowerLevel", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.BIONICBATTERYDIAGNOSTIC.CRITERIA.CHECKPOWERLEVEL, CheckPowerLevel));
		multiplier = (BionicBatteryMonitor.GetDifficultyModifier().value + 200f) / 200f;
		recommendedJoulesPerBionic = 480000f * multiplier;
		bionicJoulesPerCycle = 120000f * multiplier;
	}

	private DiagnosticResult CheckEnoughBatteries()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
		if (tracker.GetDataTimeLength() < 10f)
		{
			result.opinion = DiagnosticResult.Opinion.Normal;
			result.Message = UI.COLONY_DIAGNOSTICS.NO_DATA;
		}
		else if (bionics.Count != 0)
		{
			if (tracker.GetAverageValue(trackerSampleCountSeconds) == 0f)
			{
				result.Message = UI.COLONY_DIAGNOSTICS.BIONICBATTERYDIAGNOSTIC.CRITERIA_BATTERIES.NO_POWERBANKS;
				result.opinion = DiagnosticResult.Opinion.Bad;
			}
			else if ((float)bionics.Count * recommendedJoulesPerBionic > tracker.GetAverageValue(trackerSampleCountSeconds))
			{
				result.opinion = DiagnosticResult.Opinion.Concern;
				float currentValue = tracker.GetCurrentValue();
				float f = bionicJoulesPerCycle * (float)bionics.Count;
				string formattedJoules = GameUtil.GetFormattedJoules(currentValue);
				string formattedJoules2 = GameUtil.GetFormattedJoules(Mathf.Abs(f));
				string text = UI.COLONY_DIAGNOSTICS.BIONICBATTERYDIAGNOSTIC.CRITERIA_BATTERIES.LOW_POWERBANKS;
				text = text.Replace("{0}", formattedJoules);
				text = text.Replace("{1}", formattedJoules2);
				result.Message = text;
			}
		}
		return result;
	}

	private DiagnosticResult CheckPowerLevel()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
		foreach (MinionIdentity bionic in bionics)
		{
			if (bionic.isNull)
			{
				continue;
			}
			BionicBatteryMonitor.Instance sMI = bionic.GetSMI<BionicBatteryMonitor.Instance>();
			if (!sMI.IsNullOrStopped())
			{
				if (sMI.IsInsideState(sMI.sm.online.critical) && result.opinion != DiagnosticResult.Opinion.Bad)
				{
					result.opinion = DiagnosticResult.Opinion.Concern;
					result.Message = UI.COLONY_DIAGNOSTICS.BIONICBATTERYDIAGNOSTIC.CRITERIA_POWERLEVEL.CRITICAL_MODE;
					result.clickThroughTarget = new Tuple<Vector3, GameObject>(sMI.gameObject.transform.position, sMI.gameObject);
				}
				if (sMI.IsInsideState(sMI.sm.offline))
				{
					result.opinion = DiagnosticResult.Opinion.Bad;
					result.Message = UI.COLONY_DIAGNOSTICS.BIONICBATTERYDIAGNOSTIC.CRITERIA_POWERLEVEL.POWERLESS;
					result.clickThroughTarget = new Tuple<Vector3, GameObject>(sMI.gameObject.transform.position, sMI.gameObject);
				}
			}
		}
		return result;
	}

	public override string GetCurrentValueString()
	{
		return GameUtil.GetFormattedJoules(tracker.GetCurrentValue());
	}

	protected override string GetDefaultResultMessage()
	{
		return UI.COLONY_DIAGNOSTICS.BIONICBATTERYDIAGNOSTIC.CRITERIA_BATTERIES.PASS;
	}
}
