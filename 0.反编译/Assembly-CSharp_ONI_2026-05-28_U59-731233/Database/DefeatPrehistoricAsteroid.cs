using STRINGS;

namespace Database;

public class DefeatPrehistoricAsteroid : VictoryColonyAchievementRequirement
{
	public override string GetProgress(bool complete)
	{
		int num = 1000;
		int num2 = (complete ? num : 0);
		GameplayEventInstance gameplayEventInstance = GameplayEventManager.Instance.GetGameplayEventInstance(Db.Get().GameplayEvents.LargeImpactor.Id);
		if (gameplayEventInstance != null)
		{
			LargeImpactorEvent.StatesInstance statesInstance = (LargeImpactorEvent.StatesInstance)gameplayEventInstance.smi;
			if (statesInstance != null && statesInstance.impactorInstance != null)
			{
				LargeImpactorStatus.Instance sMI = statesInstance.impactorInstance.GetSMI<LargeImpactorStatus.Instance>();
				num = sMI.def.MAX_HEALTH;
				num2 = num - sMI.Health;
			}
		}
		return GameUtil.SafeStringFormat(COLONY_ACHIEVEMENTS.ASTEROID_DESTROYED.REQUIREMENT_DESCRIPTION, GameUtil.GetFormattedInt(num2), GameUtil.GetFormattedInt(num));
	}

	public override string Description()
	{
		return COLONY_ACHIEVEMENTS.ASTEROID_DESTROYED.DESCRIPTION;
	}

	public override bool Success()
	{
		return SaveGame.Instance.ColonyAchievementTracker.largeImpactorState == ColonyAchievementTracker.LargeImpactorState.Defeated;
	}

	public override bool Fail()
	{
		return SaveGame.Instance.ColonyAchievementTracker.largeImpactorState == ColonyAchievementTracker.LargeImpactorState.Landed;
	}

	public override string Name()
	{
		return COLONY_ACHIEVEMENTS.ASTEROID_DESTROYED.REQUIREMENT_NAME;
	}
}
