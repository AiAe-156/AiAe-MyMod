using KSerialization;
using TUNING;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/Workable/Harvestable")]
public class Harvestable : Workable
{
	public StatusItem readyForHarvestStatusItem = Db.Get().CreatureStatusItems.ReadyForHarvest;

	public HarvestDesignatable harvestDesignatable;

	[Serialize]
	protected bool canBeHarvested = false;

	protected Chore chore;

	private static readonly EventSystem.IntraObjectHandler<Harvestable> ForceCancelHarvestDelegate = new EventSystem.IntraObjectHandler<Harvestable>(delegate(Harvestable component, object data)
	{
		component.ForceCancelHarvest(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Harvestable> OnCancelDelegate = new EventSystem.IntraObjectHandler<Harvestable>(delegate(Harvestable component, object data)
	{
		component.OnCancel(data);
	});

	public WorkerBase completed_by { get; protected set; }

	public bool CanBeHarvested => canBeHarvested;

	protected Harvestable()
	{
		SetOffsetTable(OffsetGroups.InvertedStandardTable);
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		workerStatusItem = Db.Get().DuplicantStatusItems.Harvesting;
		multitoolContext = "harvest";
		multitoolHitEffectTag = "fx_harvest_splash";
		harvestDesignatable = GetComponent<HarvestDesignatable>();
	}

	protected override void OnSpawn()
	{
		Subscribe(2127324410, ForceCancelHarvestDelegate);
		SetWorkTime(10f);
		Subscribe(2127324410, OnCancelDelegate);
		faceTargetWhenWorking = true;
		Components.Harvestables.Add(this);
		attributeConverter = Db.Get().AttributeConverters.HarvestSpeed;
		attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
		skillExperienceSkillGroup = Db.Get().SkillGroups.Farming.Id;
		skillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
	}

	public void OnUprooted(object data)
	{
		if (canBeHarvested)
		{
			Harvest();
		}
	}

	public void Harvest()
	{
		if (harvestDesignatable != null)
		{
			harvestDesignatable.MarkedForHarvest = false;
		}
		chore = null;
		Trigger(1272413801, (object)this);
		KSelectable component = GetComponent<KSelectable>();
		component.RemoveStatusItem(Db.Get().MiscStatusItems.PendingHarvest);
		component.RemoveStatusItem(Db.Get().MiscStatusItems.Operating);
		Game.Instance.userMenu.Refresh(base.gameObject);
		completed_by = null;
	}

	public virtual void OnMarkedForHarvest()
	{
		KSelectable component = GetComponent<KSelectable>();
		if (chore == null)
		{
			chore = new WorkChore<Harvestable>(Db.Get().ChoreTypes.Harvest, this, null, run_until_complete: true, null, null, null, allow_in_red_alert: true, null, ignore_schedule_block: false, only_when_operational: true, null, is_preemptable: true);
			component.AddStatusItem(Db.Get().MiscStatusItems.PendingHarvest, this);
		}
	}

	public void SetCanBeHarvested(bool state)
	{
		canBeHarvested = state;
		KSelectable component = GetComponent<KSelectable>();
		if (canBeHarvested)
		{
			component.AddStatusItem(readyForHarvestStatusItem);
			if (harvestDesignatable != null)
			{
				if (harvestDesignatable.HarvestWhenReady)
				{
					harvestDesignatable.MarkForHarvest();
				}
				else if (harvestDesignatable.InPlanterBox)
				{
					component.AddStatusItem(Db.Get().MiscStatusItems.NotMarkedForHarvest, this);
				}
			}
			else
			{
				OnMarkedForHarvest();
			}
		}
		else
		{
			component.RemoveStatusItem(readyForHarvestStatusItem);
			component.RemoveStatusItem(Db.Get().MiscStatusItems.NotMarkedForHarvest);
		}
		Game.Instance.userMenu.Refresh(base.gameObject);
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		completed_by = worker;
		Harvest();
	}

	protected virtual void OnCancel(object data)
	{
		bool flag = data == null || (data is Boxed<bool> && !((Boxed<bool>)data).value);
		if (chore != null)
		{
			chore.Cancel("Cancel harvest");
			chore = null;
			KSelectable component = GetComponent<KSelectable>();
			component.RemoveStatusItem(Db.Get().MiscStatusItems.PendingHarvest);
			if (flag && harvestDesignatable != null)
			{
				harvestDesignatable.SetHarvestWhenReady(state: false);
			}
		}
		if (flag && harvestDesignatable != null)
		{
			harvestDesignatable.MarkedForHarvest = false;
		}
	}

	public bool HasChore()
	{
		if (chore == null)
		{
			return false;
		}
		return true;
	}

	public virtual void ForceCancelHarvest(object data = null)
	{
		OnCancel(data);
		KSelectable component = GetComponent<KSelectable>();
		component.RemoveStatusItem(Db.Get().MiscStatusItems.PendingHarvest);
		Game.Instance.userMenu.Refresh(base.gameObject);
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		Components.Harvestables.Remove(this);
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		base.OnStartWork(worker);
		KSelectable component = GetComponent<KSelectable>();
		component.RemoveStatusItem(Db.Get().MiscStatusItems.PendingHarvest);
	}
}
