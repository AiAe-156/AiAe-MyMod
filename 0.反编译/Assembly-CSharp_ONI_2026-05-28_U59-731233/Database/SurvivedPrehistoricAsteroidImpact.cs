using STRINGS;
using UnityEngine;

namespace Database;

public class SurvivedPrehistoricAsteroidImpact : ColonyAchievementRequirement
{
	private int requiredCyclesAfterImpact;

	public SurvivedPrehistoricAsteroidImpact(int requiredCyclesAfterImpact)
	{
		this.requiredCyclesAfterImpact = requiredCyclesAfterImpact;
	}

	public override string GetProgress(bool complete)
	{
		int num = (complete ? requiredCyclesAfterImpact : 0);
		if (!complete && SaveGame.Instance.ColonyAchievementTracker.largeImpactorLandedCycle >= 0)
		{
			num = Mathf.Clamp(GameClock.Instance.GetCycle() - SaveGame.Instance.ColonyAchievementTracker.largeImpactorLandedCycle, 0, requiredCyclesAfterImpact);
		}
		return GameUtil.SafeStringFormat(COLONY_ACHIEVEMENTS.ASTEROID_SURVIVED.REQUIREMENT_DESCRIPTION, GameUtil.GetFormattedInt(num), GameUtil.GetFormattedInt(requiredCyclesAfterImpact));
	}

	public override bool Success()
	{
		if (SaveGame.Instance.ColonyAchievementTracker.largeImpactorLandedCycle >= 0)
		{
			return GameClock.Instance.GetCycle() - SaveGame.Instance.ColonyAchievementTracker.largeImpactorLandedCycle >= requiredCyclesAfterImpact;
		}
		return false;
	}

	public override bool Fail()
	{
		return SaveGame.Instance.ColonyAchievementTracker.largeImpactorState == ColonyAchievementTracker.LargeImpactorState.Defeated;
	}
}
