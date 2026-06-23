using System;
using System.Collections.Generic;
using Klei.AI;

public class DefragmentationZone : Workable
{
	private const float BEDROOM_EFFECTS_DURATION_OVERRIDE = 1800f;

	[MyCmpGet]
	public Assignable assignable;

	public IApproachable approachable;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		SetReportType(ReportManager.ReportType.PersonalTime);
		showProgressBar = false;
		workerStatusItem = null;
		synchronizeAnims = false;
		triggerWorkReactions = false;
		lightEfficiencyBonus = false;
		approachable = GetComponent<IApproachable>();
		workAnims = new HashedString[2] { "microchip_bed_pre", "microchip_bed_loop" };
		workingPstComplete = new HashedString[1] { "microchip_bed_pst" };
		workingPstFailed = new HashedString[1] { "microchip_bed_pst" };
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		SetWorkTime(float.PositiveInfinity);
		OnWorkableEventCB = (Action<Workable, WorkableEvent>)Delegate.Combine(OnWorkableEventCB, new Action<Workable, WorkableEvent>(OnWorkableEvent));
	}

	private void OnWorkableEvent(Workable workable, WorkableEvent workable_event)
	{
		if (workable_event == WorkableEvent.WorkStarted)
		{
			AddRoomEffects();
		}
	}

	private void AddRoomEffects()
	{
		if (base.worker == null)
		{
			return;
		}
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

	public override bool InstantlyFinish(WorkerBase worker)
	{
		return false;
	}
}
