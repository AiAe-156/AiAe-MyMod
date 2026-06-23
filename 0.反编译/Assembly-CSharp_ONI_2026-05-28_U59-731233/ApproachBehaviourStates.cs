using System.Collections.Generic;
using UnityEngine;

public class ApproachBehaviourStates : GameStateMachine<ApproachBehaviourStates, ApproachBehaviourStates.Instance, IStateMachineTarget, ApproachBehaviourStates.Def>
{
	public class Def : BaseDef
	{
		public Tag monitorId;

		public Tag behaviourTag;

		public Tag reserveTag = GameTags.Creatures.ReservedByCreature;

		public string preAnim = "";

		public string loopAnim = "";

		public string pstAnim = "";

		public Def(Tag monitorId, Tag behaviourTag)
		{
			this.monitorId = monitorId;
			this.behaviourTag = behaviourTag;
		}
	}

	public class InteractState : State
	{
		public State pre;

		public State loop;

		public State pst;
	}

	public new class Instance : GameInstance
	{
		private IApproachableBehaviour monitor;

		public CellOffset[] targetOffsets;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, def.behaviourTag);
			base.sm.self.Set(base.smi.gameObject, base.smi);
		}

		public IApproachableBehaviour GetMonitor()
		{
			if (monitor.IsNullOrDestroyed())
			{
				SetMonitor();
			}
			return monitor;
		}

		private void SetMonitor()
		{
			List<ICreatureMonitor> allSMI = base.gameObject.GetAllSMI<ICreatureMonitor>();
			foreach (ICreatureMonitor item in allSMI)
			{
				if (item.Id == base.def.monitorId)
				{
					monitor = item as IApproachableBehaviour;
					break;
				}
			}
			Debug.Assert(base.smi.monitor != null, "Could not find monitor with ID");
		}
	}

	public InteractState interact;

	public State behaviourComplete;

	public ApproachSubState<IApproachable> approach;

	public State failure;

	public TargetParameter self;

	public TargetParameter target;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = approach;
		root.Enter(RefreshTarget).Enter(Reserve).Exit(Unreserve)
			.EventHandler(GameHashes.ApproachableTargetChanged, RefreshTarget);
		approach.InitializeStates(self, target, (Instance smi) => smi.targetOffsets, interact, failure).ToggleMainStatusItem((Instance smi) => smi.GetMonitor().GetApproachStatusItem());
		interact.Enter(delegate(Instance smi)
		{
			smi.GetMonitor().OnArrive();
		}).DefaultState(interact.pre).OnTargetLost(target, failure)
			.ToggleMainStatusItem((Instance smi) => smi.GetMonitor().GetBehaviourStatusItem());
		interact.pre.PlayAnim((Instance smi) => smi.def.preAnim).OnAnimQueueComplete(interact.loop);
		interact.loop.PlayAnim((Instance smi) => smi.def.loopAnim).OnAnimQueueComplete(interact.pst);
		interact.pst.PlayAnim((Instance smi) => smi.def.pstAnim).OnAnimQueueComplete(behaviourComplete);
		behaviourComplete.BehaviourComplete((Instance smi) => smi.def.behaviourTag).Exit(delegate(Instance smi)
		{
			smi.GetMonitor().OnSuccess();
		});
		failure.Enter(delegate(Instance smi)
		{
			smi.GetMonitor().OnFailure();
		}).GoTo(null);
	}

	private static void Reserve(Instance smi)
	{
		if (smi.def.reserveTag != Tag.Invalid)
		{
			smi.sm.target.Get(smi).GetComponent<KPrefabID>().SetTag(smi.def.reserveTag, set: true);
		}
	}

	private static void Unreserve(Instance smi)
	{
		if (smi.def.reserveTag != Tag.Invalid && smi.sm.target.Get(smi) != null)
		{
			smi.sm.target.Get(smi).GetComponent<KPrefabID>().RemoveTag(smi.def.reserveTag);
		}
	}

	public static void RefreshTarget(Instance smi)
	{
		GameObject gameObject = smi.GetMonitor().GetTarget();
		if (gameObject == null)
		{
			smi.GoTo(smi.sm.failure);
			return;
		}
		smi.targetOffsets = smi.GetMonitor().GetApproachOffsets();
		smi.sm.target.Set(gameObject, smi);
	}
}
