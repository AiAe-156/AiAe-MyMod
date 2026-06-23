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
		bool allMinnowQuestsCompleted = SaveGame.Instance.ColonyAchievementTracker.allMinnowQuestsCompleted;
		bool flag = allMinnowQuestsCompleted;
		if (!flag)
		{
			flag = GetMinnowInstance()?.WasCompletedAndAcknowledged ?? false;
		}
		return flag;
	}

	public override string Name()
	{
		int num = (int)minnowIdentity;
		MinnowImperativePOIStates.Instance minnowInstance = GetMinnowInstance();
		bool flag = (minnowInstance == null || !minnowInstance.WasCompletedAndAcknowledged) && (minnowInstance == null || !minnowInstance.HasUserEverClicked);
		float mass = minnowInstance?.def.requiredMass ?? 0f;
		string text = Strings.Get(flag ? requirementNamePATH_HIDDEN[num] : requirementNamePATH[num]);
		return text.Replace("{AMOUNT}", GameUtil.GetFormattedMass(mass));
	}
}
