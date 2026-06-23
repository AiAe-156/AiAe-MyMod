using System;
using UnityEngine;

public class SimpleLayeredAnimWork : GameStateMachine<SimpleLayeredAnimWork, SimpleLayeredAnimWork.Instance, WorkerBase>
{
	public new class Instance : GameInstance
	{
		public readonly bool SynchAnims;

		public KAnimFile[] anims;

		public Grid.SceneLayer sceneLayer;

		private KBatchedAnimController animController;

		public GameObject WorkProvider => base.sm.workProvider.Get(this);

		public Instance(GameObject workProvider, WorkerBase master, Grid.SceneLayer sceneLayer, bool synchAnims, KAnimFile[] overrideAnims)
			: base(master)
		{
			base.sm.workProvider.Set(workProvider, base.smi);
			base.sm.worker.Set(master, base.smi);
			this.sceneLayer = sceneLayer;
			anims = overrideAnims;
			SynchAnims = synchAnims;
			animController = GetComponent<KBatchedAnimController>();
		}

		public void SetForegroundLayer()
		{
			animController.SetFGLayer(sceneLayer);
			animController.GetLayering().HideSymbols();
		}

		public void ClearForegroundLayer()
		{
			animController.SetFGLayer(Grid.SceneLayer.NoLayer);
			animController.GetLayering().HideSymbols();
		}

		public void PlayAnimOnWorkProvider(string animName, KAnim.PlayMode playmode)
		{
			if (SynchAnims && WorkProvider != null)
			{
				WorkProvider.GetComponent<KBatchedAnimController>().Play(animName, playmode);
			}
		}
	}

	private const string ANIM_NAME_PRE = "working_pre";

	private const string ANIM_NAME_LOOP = "working_loop";

	private const string ANIM_NAME_PST = "working_pst";

	public PreLoopPostState work;

	public State complete;

	public TargetParameter workProvider;

	public TargetParameter worker;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = work;
		Target(worker);
		work.Exit(ClearForegroundLayer).ToggleAnims((Func<Instance, KAnimFile[]>)GetAnimOverrides).Enter(SetForegroundLayer)
			.DefaultState(work.pre);
		work.pre.EventTransition(GameHashes.WorkerPlayPostAnim, work.pst).PlayAnim("working_pre", KAnim.PlayMode.Once).Enter(delegate(Instance smi)
		{
			PlayAnimsOnWorkProvider(smi, "working_pre", KAnim.PlayMode.Once);
		})
			.OnAnimQueueComplete(work.loop);
		work.loop.EventTransition(GameHashes.WorkerPlayPostAnim, work.pst).PlayAnim("working_loop", KAnim.PlayMode.Loop).Enter(delegate(Instance smi)
		{
			PlayAnimsOnWorkProvider(smi, "working_loop", KAnim.PlayMode.Loop);
		});
		work.pst.PlayAnim("working_pst", KAnim.PlayMode.Once).Enter(delegate(Instance smi)
		{
			PlayAnimsOnWorkProvider(smi, "working_pst", KAnim.PlayMode.Once);
		}).OnAnimQueueComplete(complete);
		complete.GoTo(null);
	}

	private static void SetForegroundLayer(Instance smi)
	{
		smi.SetForegroundLayer();
	}

	private static void ClearForegroundLayer(Instance smi)
	{
		smi.ClearForegroundLayer();
	}

	private static void PlayAnimsOnWorkProvider(Instance smi, string animName, KAnim.PlayMode playMode)
	{
		smi.PlayAnimOnWorkProvider(animName, playMode);
	}

	private static KAnimFile[] GetAnimOverrides(Instance smi)
	{
		return smi.anims;
	}
}
