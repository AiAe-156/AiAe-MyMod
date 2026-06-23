using TUNING;

public class ArmTrapWorkable : Workable
{
	public bool WorkInPstAnimation = false;

	public bool CanBeArmedAtLongDistance = false;

	public CellOffset[] initialOffsets;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		if (CanBeArmedAtLongDistance)
		{
			SetOffsetTable(OffsetGroups.InvertedStandardTableWithCorners);
			faceTargetWhenWorking = true;
			multitoolContext = "build";
			multitoolHitEffectTag = EffectConfigs.BuildSplashId;
		}
		if (initialOffsets != null && initialOffsets.Length != 0)
		{
			SetOffsets(initialOffsets);
		}
		SetWorkerStatusItem(Db.Get().DuplicantStatusItems.ArmingTrap);
		requiredSkillPerk = Db.Get().SkillPerks.CanWrangleCreatures.Id;
		attributeConverter = Db.Get().AttributeConverters.CapturableSpeed;
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
		skillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
		SetWorkTime(5f);
		synchronizeAnims = true;
		resetProgressOnStop = true;
	}

	public override void OnPendingCompleteWork(WorkerBase worker)
	{
		base.OnPendingCompleteWork(worker);
		WorkInPstAnimation = true;
		base.gameObject.Trigger(-2025798095);
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		base.OnCompleteWork(worker);
		WorkInPstAnimation = false;
	}
}
