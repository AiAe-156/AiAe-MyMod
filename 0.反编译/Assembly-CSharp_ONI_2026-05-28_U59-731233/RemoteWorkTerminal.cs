using KSerialization;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/RemoteWorkTerminal")]
public class RemoteWorkTerminal : Workable
{
	[Serialize]
	private Ref<RemoteWorkerDock> dock = null;

	private static int NUM_WORKING_INTERACTS = -1;

	[MyCmpReq]
	private KBatchedAnimController kbac;

	private static readonly HashedString[] normalWorkAnims = new HashedString[1] { "working_pre" };

	private static readonly HashedString[] hatWorkAnims = new HashedString[1] { "hat_pre" };

	private static readonly HashedString[] normalWorkPstAnim = new HashedString[1] { "working_pst" };

	private static readonly HashedString[] hatWorkPstAnim = new HashedString[1] { "working_hat_pst" };

	public RemoteWorkerDock future_dock = null;

	public RemoteWorkerDock CurrentDock
	{
		get
		{
			return dock?.Get();
		}
		set
		{
			if (dock?.Get() != null)
			{
				dock.Get().StopWorking(this);
			}
			dock = new Ref<RemoteWorkerDock>(value);
		}
	}

	public RemoteWorkerDock FutureDock
	{
		get
		{
			return future_dock ?? CurrentDock;
		}
		set
		{
			CurrentDock = value;
		}
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_remote_terminal_kanim") };
		InitializeWorkingInteracts();
		synchronizeAnims = true;
		showProgressBar = false;
		workLayer = Grid.SceneLayer.BuildingUse;
		surpressWorkerForceSync = true;
		kbac.onAnimComplete += PlayNextWorkingAnim;
	}

	private void InitializeWorkingInteracts()
	{
		if (NUM_WORKING_INTERACTS != -1)
		{
			return;
		}
		KAnimFileData data = overrideAnims[0].GetData();
		NUM_WORKING_INTERACTS = 0;
		while (true)
		{
			string anim_name = $"working_loop_{NUM_WORKING_INTERACTS + 1}";
			KAnim.Anim anim = data.GetAnim(anim_name);
			if (anim == null)
			{
				break;
			}
			NUM_WORKING_INTERACTS++;
		}
	}

	public override HashedString[] GetWorkAnims(WorkerBase worker)
	{
		MinionResume component = worker.GetComponent<MinionResume>();
		if (GetComponent<Building>() != null && component != null && component.CurrentHat != null)
		{
			return hatWorkAnims;
		}
		return normalWorkAnims;
	}

	public override HashedString[] GetWorkPstAnims(WorkerBase worker, bool successfully_completed)
	{
		MinionResume component = worker.GetComponent<MinionResume>();
		if (GetComponent<Building>() != null && component != null && component.CurrentHat != null)
		{
			return hatWorkPstAnim;
		}
		return normalWorkPstAnim;
	}

	protected override void OnStartWork(WorkerBase worker)
	{
		base.OnStartWork(worker);
		kbac.Queue(GetWorkingLoop());
		CurrentDock?.StartWorking(this);
	}

	protected override void OnStopWork(WorkerBase worker)
	{
		base.OnStopWork(worker);
		CurrentDock?.StopWorking(this);
	}

	protected override bool OnWorkTick(WorkerBase worker, float dt)
	{
		if (CurrentDock == null)
		{
			return true;
		}
		return CurrentDock.OnRemoteWorkTick(dt);
	}

	private HashedString GetWorkingLoop()
	{
		return $"working_loop_{Random.Range(1, NUM_WORKING_INTERACTS + 1)}";
	}

	private void PlayNextWorkingAnim(HashedString anim)
	{
		if (!(base.worker == null) && base.worker.GetState() == WorkerBase.State.Working)
		{
			kbac.Play(GetWorkingLoop());
		}
	}
}
