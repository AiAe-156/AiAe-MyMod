using Klei.AI;
using TUNING;
using UnityEngine;

public class WatchRoboDancerWorkable : Workable, IWorkerPrioritizable
{
	public GameObject owner;

	public int basePriority = RELAXATION.PRIORITY.TIER3;

	public static string SPECIFIC_EFFECT = "SawRoboDancer";

	public static string TRACKING_EFFECT = "RecentlySawRoboDancer";

	public KAnimFile[][] workerOverrideAnims = new KAnimFile[2][]
	{
		new KAnimFile[1] { Assets.GetAnim("anim_interacts_robotdance_kanim") },
		new KAnimFile[1] { Assets.GetAnim("anim_interacts_robotdance1_kanim") }
	};

	private WatchRoboDancerWorkable()
	{
		SetReportType(ReportManager.ReportType.PersonalTime);
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		synchronizeAnims = false;
		showProgressBar = true;
		resetProgressOnStop = true;
		workerStatusItem = Db.Get().DuplicantStatusItems.WatchRoboDancerWorkable;
		SetWorkTime(30f);
		showProgressBar = false;
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		Effects component = worker.GetComponent<Effects>();
		if (!string.IsNullOrEmpty(TRACKING_EFFECT))
		{
			component.Add(TRACKING_EFFECT, should_save: true);
		}
		if (!string.IsNullOrEmpty(SPECIFIC_EFFECT))
		{
			component.Add(SPECIFIC_EFFECT, should_save: true);
		}
	}

	public bool GetWorkerPriority(WorkerBase worker, out int priority)
	{
		priority = basePriority;
		Effects component = worker.GetComponent<Effects>();
		if (!string.IsNullOrEmpty(TRACKING_EFFECT) && component.HasEffect(TRACKING_EFFECT))
		{
			priority = 0;
			return false;
		}
		if (!string.IsNullOrEmpty(SPECIFIC_EFFECT) && component.HasEffect(SPECIFIC_EFFECT))
		{
			priority = RELAXATION.PRIORITY.RECENTLY_USED;
		}
		return true;
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		worker.GetComponent<Effects>().Add("Dancing", should_save: false);
	}

	protected override bool OnWorkTick(WorkerBase worker, float dt)
	{
		worker.GetComponent<Facing>().Face(owner.transform.position.x);
		return base.OnWorkTick(worker, dt);
	}

	protected override void OnStopWork(WorkerBase worker)
	{
		worker.GetComponent<Effects>().Remove("Dancing");
		ChoreHelpers.DestroyLocator(base.gameObject);
	}

	public override AnimInfo GetAnim(WorkerBase worker)
	{
		int num = Random.Range(0, workerOverrideAnims.Length);
		overrideAnims = workerOverrideAnims[num];
		return base.GetAnim(worker);
	}
}
