using TUNING;

public class FoodSmokerWorkableEmpty : Workable
{
	private static readonly HashedString[] WORK_ANIMS = new HashedString[2] { "empty_pre", "empty_loop" };

	private static readonly HashedString[] WORK_ANIMS_PST = new HashedString[1] { "empty_pst" };

	private static readonly HashedString[] WORK_ANIMS_FAIL_PST = new HashedString[1] { "" };

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		workerStatusItem = Db.Get().DuplicantStatusItems.Emptying;
		workAnims = WORK_ANIMS;
		workingPstComplete = WORK_ANIMS_PST;
		workingPstFailed = WORK_ANIMS_FAIL_PST;
		requiredSkillPerk = Db.Get().SkillPerks.CanGasRange.Id;
		attributeConverter = Db.Get().AttributeConverters.CookingSpeed;
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.FULL_EXPERIENCE;
		skillExperienceSkillGroup = Db.Get().SkillGroups.Cooking.Id;
		skillExperienceMultiplier = SKILLS.FULL_EXPERIENCE;
	}
}
