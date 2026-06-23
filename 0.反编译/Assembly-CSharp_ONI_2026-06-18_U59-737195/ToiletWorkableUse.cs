using System;
using System.Collections.Generic;
using KSerialization;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/ToiletWorkableUse")]
public class ToiletWorkableUse : Workable
{
	public Dictionary<Tag, KAnimFile[]> workerTypeOverrideAnims = new Dictionary<Tag, KAnimFile[]>();

	public Dictionary<Tag, HashedString[]> workerTypePstAnims = new Dictionary<Tag, HashedString[]>();

	[Serialize]
	public int timesUsed;

	[Serialize]
	public Tag last_user_id;

	[Serialize]
	public SimHashes lastElementRemovedFromDupe = SimHashes.DirtyWater;

	[Serialize]
	public float lastAmountOfWasteMassRemovedFromDupe;

	private ToiletWorkableUse()
	{
		SetReportType(ReportManager.ReportType.PersonalTime);
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		showProgressBar = true;
		resetProgressOnStop = true;
		attributeConverter = Db.Get().AttributeConverters.ToiletSpeed;
		SetWorkTime(8.5f);
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		base.OnStartWork(worker);
		if (Sim.IsRadiationEnabled() && worker.GetAmounts().Get(Db.Get().Amounts.RadiationBalance).value > 0f)
		{
			worker.gameObject.GetComponent<KSelectable>().AddStatusItem(Db.Get().DuplicantStatusItems.ExpellingRads);
		}
		Game.Instance.roomProber.GetRoomOfGameObject(base.gameObject)?.roomType.TriggerRoomEffects(GetComponent<KPrefabID>(), worker.GetComponent<Effects>());
		if (worker != null)
		{
			last_user_id = worker.gameObject.PrefabID();
		}
	}

	public override HashedString[] GetWorkPstAnims(WorkerBase worker, bool successfully_completed)
	{
		HashedString[] value = null;
		if (workerTypePstAnims.TryGetValue(worker.PrefabID(), out value))
		{
			workingPstComplete = value;
			workingPstFailed = value;
		}
		return base.GetWorkPstAnims(worker, successfully_completed);
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

	protected override void OnStopWork(WorkerBase worker)
	{
		if (Sim.IsRadiationEnabled())
		{
			worker.gameObject.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().DuplicantStatusItems.ExpellingRads);
		}
		base.OnStopWork(worker);
	}

	protected override void OnAbortWork(WorkerBase worker)
	{
		if (Sim.IsRadiationEnabled())
		{
			worker.gameObject.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().DuplicantStatusItems.ExpellingRads);
		}
		base.OnAbortWork(worker);
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		AmountInstance amountInstance = Db.Get().Amounts.Bladder.Lookup(worker);
		if (amountInstance != null)
		{
			lastAmountOfWasteMassRemovedFromDupe = DUPLICANTSTATS.STANDARD.Secretions.PEE_PER_TOILET_PEE;
			lastElementRemovedFromDupe = SimHashes.DirtyWater;
			amountInstance.SetValue(0f);
		}
		else
		{
			GunkMonitor.Instance sMI = worker.GetSMI<GunkMonitor.Instance>();
			if (sMI != null)
			{
				lastAmountOfWasteMassRemovedFromDupe = sMI.CurrentGunkMass;
				lastElementRemovedFromDupe = GunkMonitor.GunkElement;
				sMI.SetGunkMassValue(0f);
				Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_GunkedToilet);
			}
		}
		if (Sim.IsRadiationEnabled())
		{
			worker.gameObject.GetComponent<KSelectable>().RemoveStatusItem(Db.Get().DuplicantStatusItems.ExpellingRads);
			AmountInstance amountInstance2 = Db.Get().Amounts.RadiationBalance.Lookup(worker);
			float num = Math.Min(val2: 100f * worker.GetSMI<RadiationMonitor.Instance>().difficultySettingMod, val1: amountInstance2.value);
			if (num >= 1f)
			{
				PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Negative, Math.Floor(num).ToString() + UI.UNITSUFFIXES.RADIATION.RADS, worker.transform, Vector3.up * 2f);
			}
			amountInstance2.ApplyDelta(0f - num);
		}
		timesUsed++;
		if (amountInstance != null)
		{
			Trigger(-350347868, (object)worker);
		}
		else
		{
			Trigger(1234642927, (object)worker);
		}
		base.OnCompleteWork(worker);
	}

	public override StatusItem GetWorkerStatusItem()
	{
		if (base.worker != null && base.worker.gameObject.HasTag(GameTags.Minions.Models.Bionic))
		{
			return Db.Get().DuplicantStatusItems.CloggingToilet;
		}
		return base.GetWorkerStatusItem();
	}
}
