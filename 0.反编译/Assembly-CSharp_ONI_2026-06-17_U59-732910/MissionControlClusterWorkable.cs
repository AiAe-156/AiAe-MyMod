using System;
using TUNING;
using UnityEngine;

public class MissionControlClusterWorkable : Workable
{
	private Clustercraft targetClustercraft;

	[MyCmpReq]
	private Operational operational;

	private Guid workStatusItem = Guid.Empty;

	public Clustercraft TargetClustercraft
	{
		get
		{
			return targetClustercraft;
		}
		set
		{
			base.WorkTimeRemaining = GetWorkTime();
			targetClustercraft = value;
		}
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		requiredSkillPerk = Db.Get().SkillPerks.CanMissionControl.Id;
		workerStatusItem = Db.Get().DuplicantStatusItems.MissionControlling;
		attributeConverter = Db.Get().AttributeConverters.ResearchSpeed;
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
		skillExperienceSkillGroup = Db.Get().SkillGroups.Research.Id;
		skillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
		overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_mission_control_station_kanim") };
		SetWorkTime(90f);
		showProgressBar = true;
		lightEfficiencyBonus = true;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Components.MissionControlClusterWorkables.Add(this);
	}

	protected override void OnCleanUp()
	{
		Components.MissionControlClusterWorkables.Remove(this);
		base.OnCleanUp();
	}

	public static bool IsRocketInRange(AxialI worldLocation, AxialI rocketLocation)
	{
		return AxialUtil.GetDistance(worldLocation, rocketLocation) <= 2;
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		base.OnStartWork(worker);
		workStatusItem = base.gameObject.GetComponent<KSelectable>().AddStatusItem(Db.Get().BuildingStatusItems.MissionControlAssistingRocket, TargetClustercraft);
		operational.SetActive(value: true);
	}

	public override float GetEfficiencyMultiplier(WorkerBase worker)
	{
		return base.GetEfficiencyMultiplier(worker) * Mathf.Clamp01(this.GetSMI<SkyVisibilityMonitor.Instance>().PercentClearSky);
	}

	protected override bool OnWorkTick(WorkerBase worker, float dt)
	{
		if (TargetClustercraft == null || !IsRocketInRange(base.gameObject.GetMyWorldLocation(), TargetClustercraft.Location))
		{
			worker.StopWork();
			return true;
		}
		return base.OnWorkTick(worker, dt);
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		Debug.Assert(TargetClustercraft != null);
		base.gameObject.GetSMI<MissionControlCluster.Instance>().ApplyEffect(TargetClustercraft);
		base.OnCompleteWork(worker);
	}

	protected override void OnStopWork(WorkerBase worker)
	{
		base.OnStopWork(worker);
		base.gameObject.GetComponent<KSelectable>().RemoveStatusItem(workStatusItem);
		TargetClustercraft = null;
		operational.SetActive(value: false);
	}
}
