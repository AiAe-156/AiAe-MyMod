using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;

public class FoodDiagnostic : ColonyDiagnostic
{
	private const int CYCLES_OF_FOOD = 3;

	private const float BASE_KCAL_PER_CYCLE = 1000f;

	private float multiplier = 1f;

	private float recommendedKCalPerDuplicant;

	public FoodDiagnostic(int worldID)
		: base(worldID, UI.COLONY_DIAGNOSTICS.FOODDIAGNOSTIC.ALL_NAME)
	{
		tracker = TrackerTool.Instance.GetWorldTracker<KCalTracker>(worldID);
		icon = "icon_category_food";
		trackerSampleCountSeconds = 150f;
		presentationSetting = PresentationSetting.CurrentValue;
		AddCriterion("CheckEnoughFood", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.FOODDIAGNOSTIC.CRITERIA.CHECKENOUGHFOOD, CheckEnoughFood));
		AddCriterion("CheckStarvation", new DiagnosticCriterion(UI.COLONY_DIAGNOSTICS.FOODDIAGNOSTIC.CRITERIA.CHECKSTARVATION, CheckStarvation));
		multiplier = MinionIdentity.GetCalorieBurnMultiplier();
		recommendedKCalPerDuplicant = 3000f * multiplier;
	}

	private DiagnosticResult CheckAnyFood()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.FOODDIAGNOSTIC.CRITERIA_HAS_FOOD.PASS);
		if (Components.LiveMinionIdentities.GetWorldItems(base.worldID).Count != 0)
		{
			if (tracker.GetDataTimeLength() < 10f)
			{
				result.opinion = DiagnosticResult.Opinion.Normal;
				result.Message = UI.COLONY_DIAGNOSTICS.NO_DATA;
			}
			else if (tracker.GetAverageValue(trackerSampleCountSeconds) == 0f)
			{
				result.opinion = DiagnosticResult.Opinion.Bad;
				result.Message = UI.COLONY_DIAGNOSTICS.FOODDIAGNOSTIC.CRITERIA_HAS_FOOD.FAIL;
			}
		}
		return result;
	}

	private DiagnosticResult CheckEnoughFood()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
		List<MinionIdentity> list = Components.LiveMinionIdentities.GetWorldItems(base.worldID).FindAll((MinionIdentity MID) => Db.Get().Amounts.Calories.Lookup(MID) != null);
		if (tracker.GetDataTimeLength() < 10f)
		{
			result.opinion = DiagnosticResult.Opinion.Normal;
			result.Message = UI.COLONY_DIAGNOSTICS.NO_DATA;
		}
		else if ((float)list.Count * (1000f * recommendedKCalPerDuplicant) > tracker.GetAverageValue(trackerSampleCountSeconds))
		{
			result.opinion = DiagnosticResult.Opinion.Concern;
			float currentValue = tracker.GetCurrentValue();
			float f = (float)list.Count * DUPLICANTSTATS.STANDARD.BaseStats.CALORIES_BURNED_PER_CYCLE * multiplier;
			string formattedCalories = GameUtil.GetFormattedCalories(currentValue);
			string formattedCalories2 = GameUtil.GetFormattedCalories(Mathf.Abs(f));
			string text = MISC.NOTIFICATIONS.FOODLOW.TOOLTIP;
			text = text.Replace("{0}", formattedCalories);
			text = text.Replace("{1}", formattedCalories2);
			result.Message = text;
		}
		return result;
	}

	private DiagnosticResult CheckStarvation()
	{
		DiagnosticResult result = new DiagnosticResult(DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
		foreach (MinionIdentity worldItem in Components.LiveMinionIdentities.GetWorldItems(base.worldID))
		{
			if (!worldItem.IsNull())
			{
				CalorieMonitor.Instance sMI = worldItem.GetSMI<CalorieMonitor.Instance>();
				if (!sMI.IsNullOrStopped() && sMI.IsInsideState(sMI.sm.hungry.starving))
				{
					result.opinion = DiagnosticResult.Opinion.Bad;
					result.Message = UI.COLONY_DIAGNOSTICS.FOODDIAGNOSTIC.HUNGRY;
					result.clickThroughTarget = new Tuple<Vector3, GameObject>(sMI.gameObject.transform.position, sMI.gameObject);
				}
			}
		}
		return result;
	}

	public override string GetCurrentValueString()
	{
		return GameUtil.GetFormattedCalories(tracker.GetCurrentValue());
	}

	public override DiagnosticResult Evaluate()
	{
		if (ColonyDiagnosticUtility.IgnoreRocketsWithNoCrewRequested(base.worldID, out var result))
		{
			return result;
		}
		result = base.Evaluate();
		if (result.opinion == DiagnosticResult.Opinion.Normal)
		{
			result.Message = UI.COLONY_DIAGNOSTICS.FOODDIAGNOSTIC.NORMAL;
		}
		return result;
	}
}
