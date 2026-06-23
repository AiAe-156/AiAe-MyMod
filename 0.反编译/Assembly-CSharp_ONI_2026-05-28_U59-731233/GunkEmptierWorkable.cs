using System.Collections.Generic;
using Klei.AI;
using UnityEngine;

public class GunkEmptierWorkable : Workable
{
	private const float BATHROOM_EFFECTS_DURATION_OVERRIDE = 1800f;

	private Storage storage;

	private GunkMonitor.Instance gunkMonitor;

	private GunkEmptierWorkable()
	{
		SetReportType(ReportManager.ReportType.PersonalTime);
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		showProgressBar = true;
		resetProgressOnStop = true;
		overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_gunkdump_kanim") };
		attributeConverter = Db.Get().AttributeConverters.ToiletSpeed;
		storage = GetComponent<Storage>();
		SetWorkTime(8.5f);
	}

	protected override bool OnWorkTick(WorkerBase worker, float dt)
	{
		float mass = Mathf.Min(dt / workTime * GunkMonitor.GUNK_CAPACITY, gunkMonitor.CurrentGunkMass, storage.RemainingCapacity());
		gunkMonitor.ExpellGunk(mass, storage);
		return base.OnWorkTick(worker, dt);
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		base.OnStartWork(worker);
		gunkMonitor = worker.GetSMI<GunkMonitor.Instance>();
		if (Sim.IsRadiationEnabled() && worker.GetAmounts().Get(Db.Get().Amounts.RadiationBalance).value > 0f)
		{
			worker.gameObject.GetComponent<KSelectable>().AddStatusItem(Db.Get().DuplicantStatusItems.ExpellingRads);
		}
		TriggerRoomEffects();
	}

	private void TriggerRoomEffects()
	{
		Room roomOfGameObject = Game.Instance.roomProber.GetRoomOfGameObject(base.gameObject);
		if (roomOfGameObject == null)
		{
			return;
		}
		RoomType roomType = roomOfGameObject.roomType;
		List<EffectInstance> result = null;
		roomType.TriggerRoomEffects(GetComponent<KPrefabID>(), base.worker.GetComponent<Effects>(), out result);
		if (result == null)
		{
			return;
		}
		foreach (EffectInstance item in result)
		{
			item.timeRemaining = 1800f;
		}
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		if (gunkMonitor != null)
		{
			gunkMonitor.ExpellAllGunk(storage);
		}
		gunkMonitor = null;
		base.OnCompleteWork(worker);
	}

	protected override void OnStopWork(WorkerBase worker)
	{
		RemoveExpellingRadStatusItem();
		base.OnStopWork(worker);
	}

	protected override void OnAbortWork(WorkerBase worker)
	{
		RemoveExpellingRadStatusItem();
		base.OnAbortWork(worker);
		gunkMonitor = null;
	}

	private void RemoveExpellingRadStatusItem()
	{
		if (Sim.IsRadiationEnabled())
		{
			base.worker.gameObject.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().DuplicantStatusItems.ExpellingRads);
		}
	}
}
