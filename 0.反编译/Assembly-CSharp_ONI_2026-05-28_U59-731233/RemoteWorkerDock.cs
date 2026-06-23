using System;
using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/Workable/RemoteWorkDock")]
public class RemoteWorkerDock : KMonoBehaviour
{
	public class NewWorker : Workable
	{
		private static readonly HashedString[] WORK_ANIMS = new HashedString[1] { "new_worker" };

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			workAnims = WORK_ANIMS;
			workingPstComplete = null;
			workingPstFailed = null;
			workAnimPlayMode = KAnim.PlayMode.Once;
			synchronizeAnims = true;
			triggerWorkReactions = false;
			workLayer = Grid.SceneLayer.BuildingUse;
			resetProgressOnStop = true;
			KAnim.Anim anim = Assets.GetAnim(RemoteWorkerConfig.DOCK_ANIM_OVERRIDES).GetData().GetAnim("new_worker");
			SetWorkTime((float)anim.numFrames / anim.frameRate);
		}

		protected override void OnStartWork(WorkerBase worker)
		{
			base.OnStartWork(worker);
		}

		protected override void OnCompleteWork(WorkerBase worker)
		{
			base.OnCompleteWork(worker);
			worker.GetComponent<RemoteWorkerSM>().Docked = true;
		}
	}

	public class EnterableDock : Workable
	{
		private static readonly HashedString[] WORK_ANIMS = new HashedString[1] { "enter_dock" };

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			workerStatusItem = Db.Get().DuplicantStatusItems.EnteringDock;
			workAnims = WORK_ANIMS;
			workingPstComplete = null;
			workingPstFailed = null;
			workAnimPlayMode = KAnim.PlayMode.Once;
			synchronizeAnims = true;
			triggerWorkReactions = false;
			workLayer = Grid.SceneLayer.BuildingUse;
			resetProgressOnStop = true;
			KAnim.Anim anim = Assets.GetAnim(RemoteWorkerConfig.DOCK_ANIM_OVERRIDES).GetData().GetAnim("enter_dock");
			SetWorkTime((float)anim.numFrames / anim.frameRate);
		}

		protected override void OnCompleteWork(WorkerBase worker)
		{
			worker.GetComponent<RemoteWorkerSM>().Docked = true;
			base.OnCompleteWork(worker);
		}
	}

	public class ExitableDock : Workable
	{
		private static readonly HashedString[] WORK_ANIMS = new HashedString[1] { "exit_dock" };

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			workAnims = WORK_ANIMS;
			workingPstComplete = null;
			workingPstFailed = null;
			workAnimPlayMode = KAnim.PlayMode.Once;
			synchronizeAnims = true;
			triggerWorkReactions = false;
			workLayer = Grid.SceneLayer.BuildingUse;
			resetProgressOnStop = true;
			KAnim.Anim anim = Assets.GetAnim(RemoteWorkerConfig.DOCK_ANIM_OVERRIDES).GetData().GetAnim("exit_dock");
			SetWorkTime((float)anim.numFrames / anim.frameRate);
		}

		protected override void OnCompleteWork(WorkerBase worker)
		{
			base.OnCompleteWork(worker);
			worker.GetComponent<RemoteWorkerSM>().Docked = false;
		}
	}

	public class WorkerRecharger : Workable
	{
		private static readonly HashedString[] WORK_ANIMS = new HashedString[2] { "recharge_pre", "recharge_loop" };

		private static readonly HashedString[] WORK_PST_ANIM = new HashedString[1] { "recharge_pst" };

		private float progress = 0f;

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			workAnims = WORK_ANIMS;
			workingPstComplete = WORK_PST_ANIM;
			workingPstFailed = WORK_PST_ANIM;
			synchronizeAnims = true;
			triggerWorkReactions = false;
			workLayer = Grid.SceneLayer.BuildingUse;
			workerStatusItem = Db.Get().DuplicantStatusItems.RemoteWorkerRecharging;
			SetWorkTime(float.PositiveInfinity);
		}

		protected override void OnStartWork(WorkerBase worker)
		{
			base.OnStartWork(worker);
			RemoteWorkerCapacitor component = worker.GetComponent<RemoteWorkerCapacitor>();
			progress = ((component != null) ? component.ChargeRatio : 0f);
			if (progressBar != null)
			{
				progressBar.SetUpdateFunc(() => progress);
			}
		}

		protected override bool OnWorkTick(WorkerBase worker, float dt)
		{
			base.OnWorkTick(worker, dt);
			RemoteWorkerCapacitor component = worker.GetComponent<RemoteWorkerCapacitor>();
			if (component != null)
			{
				progress = component.ChargeRatio;
				return component.ApplyDeltaEnergy(7.5f * dt) == 0f;
			}
			return true;
		}
	}

	public class WorkerGunkRemover : Workable
	{
		private static readonly HashedString[] WORK_ANIMS = new HashedString[2] { "drain_gunk_pre", "drain_gunk_loop" };

		private static readonly HashedString[] WORK_PST_ANIM = new HashedString[1] { "drain_gunk_pst" };

		[MyCmpGet]
		private Storage storage;

		private float progress = 0f;

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_remote_work_dock_kanim") };
			workAnims = WORK_ANIMS;
			workingPstComplete = WORK_PST_ANIM;
			workingPstFailed = WORK_PST_ANIM;
			synchronizeAnims = true;
			triggerWorkReactions = false;
			workLayer = Grid.SceneLayer.BuildingUse;
			workerStatusItem = Db.Get().DuplicantStatusItems.RemoteWorkerDraining;
			SetWorkTime(float.PositiveInfinity);
		}

		protected override void OnStartWork(WorkerBase worker)
		{
			base.OnStartWork(worker);
			Storage component = worker.GetComponent<Storage>();
			if (component != null)
			{
				progress = 1f - component.GetMassAvailable(SimHashes.LiquidGunk) / 20.000002f;
			}
			if (progressBar != null)
			{
				progressBar.SetUpdateFunc(() => progress);
			}
		}

		protected override bool OnWorkTick(WorkerBase worker, float dt)
		{
			base.OnWorkTick(worker, dt);
			Storage component = worker.GetComponent<Storage>();
			if (component != null)
			{
				float massAvailable = component.GetMassAvailable(SimHashes.LiquidGunk);
				float num = Math.Min(massAvailable, 3.3333337f * dt);
				progress = 1f - massAvailable / 20.000002f;
				if (num > 0f)
				{
					component.TransferMass(storage, SimHashes.LiquidGunk.CreateTag(), num, flatten: false, block_events: false, hide_popups: true);
					return false;
				}
			}
			return true;
		}
	}

	public class WorkerOilRefiller : Workable
	{
		private static readonly HashedString[] WORK_ANIMS = new HashedString[2] { "oil_pre", "oil_loop" };

		private static readonly HashedString[] WORK_PST_ANIM = new HashedString[1] { "oil_pst" };

		[MyCmpGet]
		private Storage storage;

		private float progress = 0f;

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_remote_work_dock_kanim") };
			workAnims = WORK_ANIMS;
			workingPstComplete = WORK_PST_ANIM;
			workingPstFailed = WORK_PST_ANIM;
			synchronizeAnims = true;
			triggerWorkReactions = false;
			workLayer = Grid.SceneLayer.BuildingUse;
			workerStatusItem = Db.Get().DuplicantStatusItems.RemoteWorkerOiling;
			SetWorkTime(float.PositiveInfinity);
		}

		protected override void OnStartWork(WorkerBase worker)
		{
			base.OnStartWork(worker);
			Storage component = worker.GetComponent<Storage>();
			if (component != null)
			{
				float massAvailable = component.GetMassAvailable(GameTags.LubricatingOil);
				progress = massAvailable / 20.000002f;
			}
			if (progressBar != null)
			{
				progressBar.SetUpdateFunc(() => progress);
			}
		}

		protected override bool OnWorkTick(WorkerBase worker, float dt)
		{
			base.OnWorkTick(worker, dt);
			Storage component = worker.GetComponent<Storage>();
			if (component != null)
			{
				float massAvailable = component.GetMassAvailable(GameTags.LubricatingOil);
				float num = Math.Min(20.000002f - massAvailable, 2.5000002f * dt);
				progress = massAvailable / 20.000002f;
				if (num > 0f)
				{
					storage.TransferMass(component, GameTags.LubricatingOil, num, flatten: false, block_events: false, hide_popups: true);
					return false;
				}
			}
			return true;
		}
	}

	[Serialize]
	protected Ref<KSelectable> worker = null;

	protected RemoteWorkerSM remoteWorker = null;

	private int remoteWorkerDestroyedEventId = -1;

	protected RemoteWorkTerminal terminal = null;

	[MyCmpGet]
	private Storage storage;

	[MyCmpGet]
	private Operational operational;

	[MyCmpAdd]
	private UserNameable nameable;

	[MyCmpAdd]
	private NewWorker new_worker_;

	[MyCmpAdd]
	private EnterableDock enter_;

	[MyCmpAdd]
	private ExitableDock exit_;

	[MyCmpAdd]
	private WorkerRecharger recharger_;

	[MyCmpAdd]
	private WorkerGunkRemover gunk_remover_;

	[MyCmpAdd]
	private WorkerOilRefiller oil_refiller_;

	private Guid status_item_handle;

	private SchedulerHandle newRemoteWorkerHandle;

	private List<IRemoteDockWorkTarget> providers = new List<IRemoteDockWorkTarget>();

	private Action<IRemoteDockWorkTarget> add_provider_binding;

	private Action<IRemoteDockWorkTarget> remove_provider_binding;

	private bool activeFetch = false;

	public RemoteWorkerSM RemoteWorker
	{
		get
		{
			return remoteWorker;
		}
		private set
		{
			remoteWorker = value;
			worker = ((value != null) ? new Ref<KSelectable>(value.GetComponent<KSelectable>()) : null);
		}
	}

	public bool IsOperational => operational.IsOperational;

	public WorkerBase GetActiveTerminalWorker()
	{
		if (terminal == null)
		{
			return null;
		}
		return terminal.worker;
	}

	private bool canWork(IRemoteDockWorkTarget provider)
	{
		Grid.CellToXY(Grid.PosToCell(this), out var x, out var y);
		Grid.CellToXY(provider.Approachable.GetCell(), out var x2, out var y2);
		return y == y2 && Math.Abs(x - x2) <= 12;
	}

	private void considerProvider(IRemoteDockWorkTarget provider)
	{
		if (canWork(provider))
		{
			providers.Add(provider);
		}
	}

	private void forgetProvider(IRemoteDockWorkTarget provider)
	{
		providers.Remove(provider);
	}

	private static string GenerateName()
	{
		string text = "";
		for (int i = 0; i < 3; i++)
		{
			text += "011223345789"[UnityEngine.Random.Range(0, "011223345789".Length)];
		}
		return BUILDINGS.PREFABS.REMOTEWORKERDOCK.NAME_FMT.Replace("{ID}", text);
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		UserNameable component = GetComponent<UserNameable>();
		if (component.savedName == "" || component.savedName == BUILDINGS.PREFABS.REMOTEWORKERDOCK.NAME)
		{
			component.SetName(GenerateName());
		}
		Subscribe(-1697596308, OnStorageChanged);
		Components.RemoteWorkerDocks.Add(this.GetMyWorldId(), this);
		add_provider_binding = considerProvider;
		remove_provider_binding = forgetProvider;
		Components.RemoteDockWorkTargets.Register(this.GetMyWorldId(), add_provider_binding, remove_provider_binding);
		remoteWorker = worker?.Get()?.GetComponent<RemoteWorkerSM>();
		if (remoteWorker == null)
		{
			RequestNewWorker();
		}
		else
		{
			remoteWorkerDestroyedEventId = remoteWorker.Subscribe(1969584890, RequestNewWorker);
		}
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		Components.RemoteWorkerDocks.Remove(this.GetMyWorldId(), this);
		Components.RemoteDockWorkTargets.Unregister(this.GetMyWorldId(), add_provider_binding, remove_provider_binding);
		if (remoteWorker != null)
		{
			remoteWorker.Unsubscribe(remoteWorkerDestroyedEventId);
		}
		if (newRemoteWorkerHandle.IsValid)
		{
			newRemoteWorkerHandle.ClearScheduler();
		}
	}

	public void CollectChores(ChoreConsumerState duplicant_state, List<Chore.Precondition.Context> succeeded_contexts, List<Chore.Precondition.Context> incomplete_contexts, List<Chore.Precondition.Context> failed_contexts, bool is_attempting_override)
	{
		if (remoteWorker == null)
		{
			return;
		}
		ChoreConsumerState consumerState = remoteWorker.ConsumerState;
		consumerState.resume = duplicant_state.resume;
		foreach (IRemoteDockWorkTarget provider in providers)
		{
			provider.RemoteDockChore?.CollectChores(consumerState, succeeded_contexts, incomplete_contexts, failed_contexts, is_attempting_override: false);
		}
	}

	public bool AvailableForWorkBy(RemoteWorkTerminal terminal)
	{
		return this.terminal == null || this.terminal == terminal;
	}

	public bool HasWorker()
	{
		return remoteWorker != null;
	}

	public void SetNextChore(RemoteWorkTerminal terminal, Chore.Precondition.Context chore_context)
	{
		Debug.Assert(worker != null);
		Debug.Assert(this.terminal == null || this.terminal == terminal);
		this.terminal = terminal;
		if (remoteWorker != null)
		{
			remoteWorker.SetNextChore(chore_context);
		}
	}

	public bool StartWorking(RemoteWorkTerminal terminal)
	{
		if (this.terminal == null)
		{
			this.terminal = terminal;
		}
		if (this.terminal == terminal && remoteWorker != null)
		{
			remoteWorker.ActivelyControlled = true;
			return true;
		}
		return false;
	}

	public void StopWorking(RemoteWorkTerminal terminal)
	{
		if (terminal == this.terminal)
		{
			this.terminal = null;
			if (remoteWorker != null)
			{
				remoteWorker.ActivelyControlled = false;
			}
		}
	}

	public bool OnRemoteWorkTick(float dt)
	{
		if (remoteWorker == null)
		{
			return true;
		}
		return !remoteWorker.ActivelyWorking && !remoteWorker.HasChoreQueued();
	}

	private void OnStorageChanged(object _)
	{
		if (remoteWorker == null || worker.Get() == null)
		{
			RequestNewWorker();
		}
	}

	private void RequestNewWorker(object _ = null)
	{
		if (newRemoteWorkerHandle.IsValid)
		{
			return;
		}
		Tag bUILD_MATERIAL_TAG = RemoteWorkerConfig.BUILD_MATERIAL_TAG;
		if (storage.FindFirstWithMass(bUILD_MATERIAL_TAG, 200f) == null)
		{
			if (!activeFetch)
			{
				activeFetch = true;
				FetchList2 fetchList = new FetchList2(storage, Db.Get().ChoreTypes.Fetch);
				fetchList.Add(bUILD_MATERIAL_TAG, null, 200f);
				fetchList.Submit(delegate
				{
					activeFetch = false;
					RequestNewWorker();
				}, check_storage_contents: true);
			}
		}
		else
		{
			MakeNewWorker();
		}
	}

	private void MakeNewWorker(object _ = null)
	{
		if (newRemoteWorkerHandle.IsValid || storage.GetAmountAvailable(RemoteWorkerConfig.BUILD_MATERIAL_TAG) < 200f)
		{
			return;
		}
		PrimaryElement elem = storage.FindFirstWithMass(RemoteWorkerConfig.BUILD_MATERIAL_TAG, 200f);
		if (elem == null)
		{
			return;
		}
		storage.ConsumeAndGetDisease(elem.ElementID.CreateTag(), 200f, out var _, out var disease, out var temperature);
		status_item_handle = GetComponent<KSelectable>().AddStatusItem(Db.Get().BuildingStatusItems.RemoteWorkDockMakingWorker);
		newRemoteWorkerHandle = GameScheduler.Instance.Schedule("MakeRemoteWorker", 2f, delegate
		{
			GameObject gameObject = GameUtil.KInstantiate(Assets.GetPrefab(RemoteWorkerConfig.ID), base.transform.position, Grid.SceneLayer.Creatures);
			if (remoteWorkerDestroyedEventId != -1 && remoteWorker != null)
			{
				remoteWorker.Unsubscribe(remoteWorkerDestroyedEventId);
			}
			RemoteWorker = gameObject.GetComponent<RemoteWorkerSM>();
			remoteWorker.HomeDepot = this;
			remoteWorker.playNewWorker = true;
			gameObject.SetActive(value: true);
			PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
			component.ElementID = elem.ElementID;
			component.Temperature = temperature;
			if (disease.idx != byte.MaxValue)
			{
				component.AddDisease(disease.idx, disease.count, "Inherited from construction material");
			}
			remoteWorkerDestroyedEventId = gameObject.Subscribe(1969584890, RequestNewWorker);
			newRemoteWorkerHandle.ClearScheduler();
			GetComponent<KSelectable>().RemoveStatusItem(status_item_handle);
		});
	}
}
