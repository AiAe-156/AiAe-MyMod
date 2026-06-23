using UnityEngine;

public class BionicUpgrade_ExplorerBooster : GameStateMachine<BionicUpgrade_ExplorerBooster, BionicUpgrade_ExplorerBooster.Instance, IStateMachineTarget, BionicUpgrade_ExplorerBooster.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		private BionicUpgrade_ExplorerBoosterMonitor.Instance monitor;

		public bool IsBeingMonitored => monitor != null;

		public bool IsReady => Progress == 1f;

		public float Progress => base.sm.Progress.Get(this);

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}

		public void SetMonitor(BionicUpgrade_ExplorerBoosterMonitor.Instance monitor)
		{
			this.monitor = monitor;
		}

		public void AddData(float dataProgressDelta)
		{
			float dataProgress = Mathf.Clamp(Progress + dataProgressDelta, 0f, 1f);
			SetDataProgress(dataProgress);
		}

		public void SetDataProgress(float dataProgress)
		{
			float num = Mathf.Clamp(dataProgress, 0f, 1f);
			base.sm.Progress.Set(dataProgress, this);
		}
	}

	public const float DataGatheringDuration = 600f;

	private FloatParameter Progress;

	public State not_ready;

	public State ready;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = not_ready;
		not_ready.ParamTransition(Progress, ready, GameStateMachine<BionicUpgrade_ExplorerBooster, Instance, IStateMachineTarget, Def>.IsGTEOne).ToggleStatusItem(Db.Get().MiscStatusItems.BionicExplorerBooster);
		ready.ParamTransition(Progress, not_ready, GameStateMachine<BionicUpgrade_ExplorerBooster, Instance, IStateMachineTarget, Def>.IsLTOne).ToggleStatusItem(Db.Get().MiscStatusItems.BionicExplorerBoosterReady);
	}
}
