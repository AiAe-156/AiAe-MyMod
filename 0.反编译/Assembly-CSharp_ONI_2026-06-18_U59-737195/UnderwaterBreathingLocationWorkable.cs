using Klei.AI;
using TUNING;

public class UnderwaterBreathingLocationWorkable : Workable
{
	[MyCmpReq]
	private Storage storage;

	private OxygenBreather breather;

	private AmountInstance breath;

	protected override void OnPrefabInit()
	{
		workTime = 150f;
		workAnims = new HashedString[2] { "working_pre", "working_loop" };
		workingPstComplete = new HashedString[1] { "working_pst" };
		workingPstFailed = new HashedString[1] { "working_pst" };
		resetProgressOnStop = false;
		showProgressBar = false;
		faceTargetWhenWorking = true;
		workLayer = Grid.SceneLayer.BuildingUse;
		overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_underwater_breathing_station_kanim") };
		base.OnPrefabInit();
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		base.OnStartWork(worker);
		SetWorkTime(150f);
		worker.GetComponent<KPrefabID>().AddTag(GameTags.RecoveringBreath);
		worker.Trigger(961737054);
		breather = worker.GetComponent<OxygenBreather>();
		breath = Db.Get().Amounts.Breath.Lookup(worker);
	}

	protected override bool OnWorkTick(WorkerBase worker, float dt)
	{
		if (breather == null || breath == null)
		{
			return true;
		}
		float amount = breather.ConsumptionRate * dt * 50f;
		storage.ConsumeAndGetDisease(GameTags.Breathable, amount, out var amount_consumed, out var disease_info, out var aggregate_temperature, out var mostRelevantItemElement);
		if (amount_consumed > 0f)
		{
			OxygenBreather.BreathableGasConsumed(breather, mostRelevantItemElement, amount_consumed, aggregate_temperature, disease_info.idx, disease_info.count);
			breath.ApplyDelta(amount_consumed * DUPLICANTSTATS.STANDARD.BaseStats.RECOVER_BREATH_DELTA);
		}
		if (storage.FindFirstWithMass(GameTags.Breathable) == null)
		{
			return true;
		}
		return false;
	}

	protected override void OnStopWork(WorkerBase worker)
	{
		worker.GetComponent<KPrefabID>().RemoveTag(GameTags.RecoveringBreath);
		worker.Trigger(-2037519664);
		base.OnStopWork(worker);
	}
}
