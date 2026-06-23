using TUNING;
using UnityEngine;

public class ReanimateBionicWorkable : Workable
{
	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		workAnims = new HashedString[2] { "offline_battery_change_pre", "offline_battery_change_loop" };
		workingPstComplete = new HashedString[1] { "offline_battery_change_pst" };
		workingPstFailed = new HashedString[1] { "offline_battery_change_failed" };
		SetWorkTime(30f);
		readyForSkillWorkStatusItem = Db.Get().DuplicantStatusItems.BionicRequiresSkillPerk;
		SetWorkerStatusItem(Db.Get().DuplicantStatusItems.InstallingElectrobank);
		workingStatusItem = Db.Get().DuplicantStatusItems.BionicBeingRebooted;
		overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_bionic_kanim") };
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
		skillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
		lightEfficiencyBonus = true;
		synchronizeAnims = true;
		resetProgressOnStop = false;
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		base.OnStartWork(worker);
		Vector3 position = worker.transform.GetPosition();
		position.x = base.transform.GetPosition().x;
		position.z = Grid.GetLayerZ(Grid.SceneLayer.Creatures);
		worker.transform.SetPosition(position);
	}

	protected override void OnStopWork(WorkerBase worker)
	{
		Vector3 position = worker.transform.GetPosition();
		position.z = Grid.GetLayerZ(Grid.SceneLayer.Move);
		worker.transform.SetPosition(position);
		base.OnStopWork(worker);
	}
}
