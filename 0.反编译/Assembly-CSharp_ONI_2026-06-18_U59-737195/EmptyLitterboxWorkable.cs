using TUNING;

public class EmptyLitterboxWorkable : Workable
{
	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		workingStatusItem = Db.Get().BuildingStatusItems.LitterBoxBeingEmptied;
		SetWorkerStatusItem(Db.Get().DuplicantStatusItems.EmptyingLitterBox);
		overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_compost_kanim") };
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.BARELY_EVER_EXPERIENCE;
		skillExperienceSkillGroup = Db.Get().SkillGroups.Basekeeping.Id;
		skillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
		lightEfficiencyBonus = true;
		synchronizeAnims = true;
		shouldShowSkillPerkStatusItem = false;
		SetWorkTime(15f);
	}
}
