using Klei.AI;
using UnityEngine;

public class RemoteWorker : StandardWorker
{
	[MyCmpGet]
	private RemoteWorkerSM remoteWorkerSM;

	public override Attributes GetAttributes()
	{
		WorkerBase workerBase = remoteWorkerSM.HomeDepot?.GetActiveTerminalWorker() ?? null;
		if (workerBase != null)
		{
			return workerBase.GetAttributes();
		}
		return null;
	}

	public override AttributeConverterInstance GetAttributeConverter(string id)
	{
		WorkerBase workerBase = remoteWorkerSM.HomeDepot?.GetActiveTerminalWorker() ?? null;
		if (workerBase != null)
		{
			return workerBase.GetAttributeConverter(id);
		}
		return null;
	}

	protected override void TryPlayingIdle()
	{
		if (remoteWorkerSM.Docked)
		{
			GetComponent<KAnimControllerBase>().Play("in_dock_idle");
		}
		else
		{
			base.TryPlayingIdle();
		}
	}

	protected override void InternalStopWork(Workable target_workable, bool is_aborted)
	{
		base.InternalStopWork(target_workable, is_aborted);
		Vector3 position = base.transform.GetPosition();
		RemoteWorkerSM obj = remoteWorkerSM;
		position.z = Grid.GetLayerZ(((object)obj != null && obj.Docked) ? Grid.SceneLayer.BuildingUse : Grid.SceneLayer.Move);
		base.transform.SetPosition(position);
	}
}
