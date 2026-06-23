using UnityEngine;

namespace FoodRehydrator;

public class AccessabilityManager : KMonoBehaviour
{
	[MyCmpReq]
	private Operational operational;

	private GameObject reserver = null;

	private Workable activeWorkable = null;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Components.FoodRehydrators.Add(base.gameObject);
		Subscribe(824508782, ActiveChangedHandler);
	}

	protected override void OnCleanUp()
	{
		Components.FoodRehydrators.Remove(base.gameObject);
		base.OnCleanUp();
	}

	public void Reserve(GameObject reserver)
	{
		this.reserver = reserver;
		Debug.Assert(reserver != null && reserver.GetComponent<MinionResume>() != null);
	}

	public void Unreserve()
	{
		activeWorkable = null;
		reserver = null;
	}

	public void SetActiveWorkable(Workable work)
	{
		DebugUtil.DevAssert(activeWorkable == null || work == null, "FoodRehydrator::AccessabilityManager activating a second workable");
		activeWorkable = work;
		operational.SetActive(activeWorkable != null);
	}

	public bool CanAccess(GameObject worker)
	{
		return operational.IsOperational && (reserver == null || reserver == worker);
	}

	protected void ActiveChangedHandler(object obj)
	{
		if (!operational.IsActive)
		{
			CancelActiveWorkable();
		}
	}

	public void CancelActiveWorkable()
	{
		if (activeWorkable != null)
		{
			activeWorkable.StopWork(activeWorkable.worker, aborted: true);
		}
	}
}
