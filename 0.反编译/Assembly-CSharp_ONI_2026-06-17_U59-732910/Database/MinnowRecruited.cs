using STRINGS;

namespace Database;

public class MinnowRecruited : VictoryColonyAchievementRequirement
{
	private static string[] requirementNamePATH = new string[3] { "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_NAME_A", "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_NAME_B", "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_NAME_C" };

	private static string[] requirementDescriptionPATH = new string[3] { "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_DESCRIPTION_A", "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_DESCRIPTION_B", "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_DESCRIPTION_C" };

	private static string[] requirementNamePATH_HIDDEN = new string[3] { "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_NAME_A_HIDDEN", "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_NAME_B_HIDDEN", "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_NAME_C_HIDDEN" };

	private static string[] requirementDescriptionPATH_HIDDEN = new string[3] { "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_DESCRIPTION_A_HIDDEN", "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_DESCRIPTION_B_HIDDEN", "STRINGS.COLONY_ACHIEVEMENTS.FINDING_MINNOW.ACHIEVEMENTS.REQUIREMENT_DESCRIPTION_C_HIDDEN" };

	private MinnowImperativePOIStates.MinnowPOIIdentity minnowIdentity;

	public MinnowRecruited(MinnowImperativePOIStates.MinnowPOIIdentity minnowIdentity)
	{
		shouldUpdateNameAndDescription = true;
		this.minnowIdentity = minnowIdentity;
	}

	public override string GetProgress(bool complete)
	{
		int num = (int)minnowIdentity;
		MinnowImperativePOIStates.Instance minnowInstance = GetMinnowInstance();
		return Strings.Get((!complete && (minnowInstance == null || !minnowInstance.HasUserEverClicked)) ? requirementDescriptionPATH_HIDDEN[num] : requirementDescriptionPATH[num]);
	}

	public override string Description()
	{
		return COLONY_ACHIEVEMENTS.FINDING_MINNOW.DESCRIPTION;
	}

	private MinnowImperativePOIStates.Instance GetMinnowInstance()
	{
		foreach (MinnowImperativePOIStates.Instance item in Components.MinnowImperativePOIs.Items)
		{
			if (item.def.minnowPOIIdentity == minnowIdentity)
			{
				return item;
			}
		}
		return null;
	}

	public override bool Success()
	{
		if (SaveGame.Instance.ColonyAchievementTracker.allMinnowQuestsCompleted)
		{
			return true;
		}
		MinnowImperativePOIStates.Instance minnowInstance = GetMinnowInstance();
		if (minnowInstance == null || !minnowInstance.WasCompletedAndAcknowledged)
		{
			return false;
		}
		return !MinnowImperativePOIStates.Instance.AllPOIsCompleted();
	}

	public override string Name()
	{
		int num = (int)minnowIdentity;
		MinnowImperativePOIStates.Instance minnowInstance = GetMinnowInstance();
		bool num2 = (minnowInstance == null || !minnowInstance.WasCompletedAndAcknowledged) && (minnowInstance == null || !minnowInstance.HasUserEverClicked);
		float num3 = minnowInstance?.def.requiredMass ?? 0f;
		Tag tag = minnowInstance?.def.requestedTag ?? ((Tag)null);
		EdiblesManager.FoodInfo foodInfo = ((tag != null) ? EdiblesManager.GetFoodInfo(tag.Name) : null);
		string text = Strings.Get(num2 ? requirementNamePATH_HIDDEN[num] : requirementNamePATH[num]);
		float num4 = ((foodInfo != null) ? (foodInfo.CaloriesPerUnit * num3) : 0f);
		return text.Replace("{AMOUNT}", (num4 != 0f) ? GameUtil.GetFormattedCalories(num4) : GameUtil.GetFormattedMass(num3));
	}
}
