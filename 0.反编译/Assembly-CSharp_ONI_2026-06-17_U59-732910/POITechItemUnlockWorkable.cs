public class POITechItemUnlockWorkable : Workable
{
	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		workerStatusItem = Db.Get().DuplicantStatusItems.ResearchingFromPOI;
		alwaysShowProgressBar = true;
		resetProgressOnStop = false;
		synchronizeAnims = true;
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		base.OnCompleteWork(worker);
		POITechItemUnlocks.Instance sMI = this.GetSMI<POITechItemUnlocks.Instance>();
		sMI.UnlockTechItems();
		sMI.sm.pendingChore.Set(value: false, sMI);
		base.gameObject.Trigger(1980521255);
		Prioritizable.RemoveRef(base.gameObject);
	}
}
