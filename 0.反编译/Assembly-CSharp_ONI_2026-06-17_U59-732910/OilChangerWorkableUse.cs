using Klei.AI;
using UnityEngine;

public class OilChangerWorkableUse : Workable
{
	private Operational operational;

	private OilChangerWorkableUse()
	{
		SetReportType(ReportManager.ReportType.PersonalTime);
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		operational = GetComponent<Operational>();
		showProgressBar = true;
		resetProgressOnStop = true;
		attributeConverter = Db.Get().AttributeConverters.ToiletSpeed;
		SetWorkTime(8.5f);
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		base.OnStartWork(worker);
		if (worker != null)
		{
			Vector3 position = worker.transform.GetPosition();
			position.z = Grid.GetLayerZ(Grid.SceneLayer.BuildingUse);
			worker.transform.SetPosition(position);
		}
		Game.Instance.roomProber.GetRoomOfGameObject(base.gameObject)?.roomType.TriggerRoomEffects(GetComponent<KPrefabID>(), worker.GetComponent<Effects>());
		operational.SetActive(value: true);
	}

	protected override void OnStopWork(WorkerBase worker)
	{
		if (worker != null)
		{
			Vector3 position = worker.transform.GetPosition();
			position.z = Grid.GetLayerZ(Grid.SceneLayer.Move);
			worker.transform.SetPosition(position);
		}
		operational.SetActive(value: false);
		base.OnStopWork(worker);
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		Storage component = GetComponent<Storage>();
		BionicOilMonitor.Instance sMI = worker.GetSMI<BionicOilMonitor.Instance>();
		if (sMI != null)
		{
			float b = 200f - sMI.CurrentOilMass;
			float num = Mathf.Min(component.GetMassAvailable(GameTags.LubricatingOil), b);
			float num2 = num;
			float num3 = 0f;
			Storage component2 = GetComponent<Storage>();
			SimHashes lubricant = SimHashes.CrudeOil;
			foreach (SimHashes key in BionicOilMonitor.LUBRICANT_TYPE_EFFECT.Keys)
			{
				component2.ConsumeAndGetDisease(key.CreateTag(), num2, out var amount_consumed, out var _, out var _);
				if (amount_consumed > num3)
				{
					lubricant = key;
					num3 = amount_consumed;
				}
				num2 -= amount_consumed;
			}
			GetComponent<Storage>().ConsumeIgnoringDisease(GameTags.LubricatingOil, num2);
			sMI.RefillOil(num);
			BionicOilMonitor.ApplyLubricationEffects(worker.GetComponent<Effects>(), lubricant);
		}
		base.OnCompleteWork(worker);
	}
}
