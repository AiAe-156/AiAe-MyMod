using KSerialization;
using TUNING;

public class ClamHarvestable : Harvestable
{
	[Serialize]
	protected bool hasBeenOpened;

	private StandardCropPlant standardCropPlant;

	private Growing growing;

	public bool IsClosedAndReadyForHarvesting
	{
		get
		{
			if (!hasBeenOpened)
			{
				return growing.IsGrown();
			}
			return false;
		}
	}

	protected override void OnPrefabInit()
	{
		standardCropPlant = GetComponent<StandardCropPlant>();
		growing = GetComponent<Growing>();
		Components.ClamHarvestables.Add(this);
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (skillsUpdateHandle != -1)
		{
			Game.Instance.Unsubscribe(skillsUpdateHandle);
		}
		skillsUpdateHandle = Game.Instance.Subscribe(-1523247426, Workable.UpdateStatusItemDispatcher, this);
		Subscribe(-266953818, OnHarvestDesignationChanged);
		Subscribe(1272413801, OnHarvested);
		SetupWorkable();
		UpdateHarvestReadyAnimations();
	}

	private void SetupWorkable()
	{
		if (hasBeenOpened)
		{
			SetupWorkableForHarvestClam();
			growing.shouldGrowOld = true;
			growing.smi.ModifyOldAgeGrowthRate(1f);
		}
		else
		{
			SetupWorkableForOpenClam();
			growing.shouldGrowOld = false;
			growing.smi.ModifyOldAgeGrowthRate(0f);
		}
		UpdateStatusItem();
	}

	public override void OnMarkedForHarvest()
	{
		base.OnMarkedForHarvest();
		SetupWorkable();
	}

	private void OnHarvestDesignationChanged(object data)
	{
		SetupWorkable();
	}

	private void OnHarvested(object data)
	{
		if (hasBeenOpened)
		{
			hasBeenOpened = false;
			UpdateHarvestReadyAnimations(refresh: false);
			SetupWorkable();
		}
	}

	private void SetupWorkableForOpenClam()
	{
		workerStatusItem = Db.Get().DuplicantStatusItems.Harvesting;
		multitoolContext = default(HashedString);
		multitoolHitEffectTag = null;
		faceTargetWhenWorking = true;
		overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_sculpture_kanim") };
		synchronizeAnims = false;
		attributeConverter = Db.Get().AttributeConverters.HarvestSpeed;
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
		requiredSkillPerk = Db.Get().SkillPerks.CanFarmClams.Id;
		shouldShowSkillPerkStatusItem = base.CanBeHarvested && harvestDesignatable.HarvestWhenReady;
		skillExperienceSkillGroup = Db.Get().SkillGroups.Farming.Id;
		skillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
		if (offsetTracker != null)
		{
			offsetTracker.Clear();
			offsetTracker = null;
		}
		SetWorkTime(10f);
	}

	private void SetupWorkableForHarvestClam()
	{
		workerStatusItem = Db.Get().DuplicantStatusItems.Harvesting;
		multitoolContext = "harvest";
		multitoolHitEffectTag = "fx_harvest_splash";
		faceTargetWhenWorking = true;
		overrideAnims = null;
		synchronizeAnims = false;
		attributeConverter = Db.Get().AttributeConverters.HarvestSpeed;
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
		requiredSkillPerk = null;
		shouldShowSkillPerkStatusItem = false;
		skillExperienceSkillGroup = Db.Get().SkillGroups.Farming.Id;
		skillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
		SetOffsetTable(OffsetGroups.InvertedStandardTable);
		SetWorkTime(10f);
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		if (!hasBeenOpened)
		{
			hasBeenOpened = true;
			UpdateHarvestReadyAnimations();
			chore = null;
			SetupWorkable();
		}
		else
		{
			base.OnCompleteWork(worker);
		}
	}

	public void PunchOpen()
	{
		if (!hasBeenOpened)
		{
			hasBeenOpened = true;
			if (base.worker != null && chore != null)
			{
				chore.Cancel("Punched open while being worked on");
			}
			UpdateHarvestReadyAnimations();
			chore = null;
			SetupWorkable();
		}
	}

	private void UpdateHarvestReadyAnimations(bool refresh = true)
	{
		if (hasBeenOpened)
		{
			standardCropPlant.anims = ClamConfig.CROP_PLANT_DEFAULT_ANIM_SET;
		}
		else
		{
			standardCropPlant.anims = ClamConfig.CROP_PLANT_CLOSED_ANIM_SET;
		}
		if ((refresh && standardCropPlant.smi.IsInsideState(standardCropPlant.smi.sm.alive.fruiting)) || standardCropPlant.smi.IsInsideState(standardCropPlant.smi.sm.alive.pre_fruiting))
		{
			standardCropPlant.smi.GoTo(standardCropPlant.smi.sm.alive.idle);
		}
	}

	protected override void OnCleanUp()
	{
		Components.ClamHarvestables.Remove(this);
		base.OnCleanUp();
	}
}
