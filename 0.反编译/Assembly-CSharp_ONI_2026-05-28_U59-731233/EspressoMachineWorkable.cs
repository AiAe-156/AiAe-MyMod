using System.Collections.Generic;
using Klei.AI;
using TUNING;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/EspressoMachineWorkable")]
public class EspressoMachineWorkable : Workable, IWorkerPrioritizable
{
	public Dictionary<Tag, KAnimFile[]> workerTypeOverrideAnims = new Dictionary<Tag, KAnimFile[]>();

	[MyCmpReq]
	private Operational operational;

	public int basePriority = RELAXATION.PRIORITY.TIER5;

	private static string specificEffect = "Espresso";

	private static string trackingEffect = "RecentlyRecDrink";

	private EspressoMachineWorkable()
	{
		SetReportType(ReportManager.ReportType.PersonalTime);
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_espresso_machine_kanim") };
		showProgressBar = true;
		resetProgressOnStop = true;
		synchronizeAnims = false;
		SetWorkTime(30f);
	}

	public override AnimInfo GetAnim(WorkerBase worker)
	{
		KAnimFile[] value = null;
		if (workerTypeOverrideAnims.TryGetValue(worker.PrefabID(), out value))
		{
			overrideAnims = value;
		}
		return base.GetAnim(worker);
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		operational.SetActive(value: true);
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		Storage component = GetComponent<Storage>();
		component.ConsumeAndGetDisease(GameTags.Water, EspressoMachine.WATER_MASS_PER_USE, out var amount_consumed, out var disease_info, out var aggregate_temperature);
		component.ConsumeAndGetDisease(EspressoMachine.INGREDIENT_TAG, EspressoMachine.INGREDIENT_MASS_PER_USE, out amount_consumed, out var disease_info2, out aggregate_temperature);
		GermExposureMonitor.Instance sMI = worker.GetSMI<GermExposureMonitor.Instance>();
		if (sMI != null)
		{
			sMI.TryInjectDisease(disease_info.idx, disease_info.count, GameTags.Water, Sickness.InfectionVector.Digestion);
			sMI.TryInjectDisease(disease_info2.idx, disease_info2.count, EspressoMachine.INGREDIENT_TAG, Sickness.InfectionVector.Digestion);
		}
		Effects component2 = worker.GetComponent<Effects>();
		if (!string.IsNullOrEmpty(specificEffect))
		{
			component2.Add(specificEffect, should_save: true);
		}
		if (!string.IsNullOrEmpty(trackingEffect))
		{
			component2.Add(trackingEffect, should_save: true);
		}
	}

	protected override void OnStopWork(WorkerBase worker)
	{
		operational.SetActive(value: false);
	}

	public bool GetWorkerPriority(WorkerBase worker, out int priority)
	{
		priority = basePriority;
		Effects component = worker.GetComponent<Effects>();
		if (!string.IsNullOrEmpty(trackingEffect) && component.HasEffect(trackingEffect))
		{
			priority = 0;
			return false;
		}
		if (!string.IsNullOrEmpty(specificEffect) && component.HasEffect(specificEffect))
		{
			priority = RELAXATION.PRIORITY.RECENTLY_USED;
		}
		return true;
	}
}
