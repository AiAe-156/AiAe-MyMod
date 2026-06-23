using System;
using STRINGS;
using UnityEngine;

public class FixedCaptureChore : Chore<FixedCaptureChore.FixedCaptureChoreStates.Instance>
{
	public class FixedCaptureChoreStates : GameStateMachine<FixedCaptureChoreStates, FixedCaptureChoreStates.Instance>
	{
		public new class Instance : GameInstance
		{
			public FixedCapturePoint.Instance fixedCapturePoint;

			public Instance(KPrefabID capture_point)
				: base((IStateMachineTarget)capture_point)
			{
				fixedCapturePoint = capture_point.GetSMI<FixedCapturePoint.Instance>();
			}
		}

		public TargetParameter rancher;

		public TargetParameter creature;

		private State movetopoint;

		private State waitforcreature_pre;

		private State waitforcreature;

		private State precaptureanim;

		private State capturecreature;

		private State failed;

		private State success;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = movetopoint;
			Target(rancher);
			root.Exit("ResetCapturePoint", delegate(Instance smi)
			{
				smi.fixedCapturePoint.isCurrentlyCapturingCreature = false;
				smi.fixedCapturePoint.ResetCapturePoint();
			});
			movetopoint.MoveTo((Instance smi) => smi.fixedCapturePoint.GetRancherInteractCell(), waitforcreature_pre).Target(masterTarget).EventTransition(GameHashes.CreatureAbandonedCapturePoint, failed);
			waitforcreature_pre.EnterTransition(null, (Instance smi) => smi.fixedCapturePoint.IsNullOrStopped()).EnterTransition(failed, HasCreatureLeft).EnterTransition(waitforcreature, (Instance smi) => true);
			waitforcreature.ToggleAnims("anim_interacts_rancherstation_kanim").PlayAnim("calling_loop", KAnim.PlayMode.Loop).Transition(failed, HasCreatureLeft)
				.Face(creature)
				.Enter("SetRancherIsAvailableForCapturing", delegate(Instance smi)
				{
					smi.fixedCapturePoint.SetRancherIsAvailableForCapturing();
				})
				.Exit("ClearRancherIsAvailableForCapturing", delegate(Instance smi)
				{
					smi.fixedCapturePoint.ClearRancherIsAvailableForCapturing();
				})
				.Target(masterTarget)
				.EventTransition(GameHashes.CreatureArrivedAtCapturePoint, precaptureanim);
			precaptureanim.EnterTransition(capturecreature, (Instance smi) => smi.fixedCapturePoint.def.preCaptureAnimName == null).Enter("LockCaptureTarget", delegate(Instance smi)
			{
				smi.fixedCapturePoint.isCurrentlyCapturingCreature = true;
			}).Enter("StoreCreature", delegate(Instance smi)
			{
				GameObject gameObject = smi.sm.creature.Get(smi);
				if (gameObject != null)
				{
					Storage component = smi.GetComponent<Storage>();
					component.Store(gameObject, hide_popups: true, block_events: true);
				}
			})
				.Exit("DropAndWrangleCreature", delegate(Instance smi)
				{
					GameObject gameObject = smi.sm.creature.Get(smi);
					if (gameObject != null)
					{
						Storage component = smi.GetComponent<Storage>();
						if (component.items.Contains(gameObject))
						{
							component.Drop(gameObject, do_disease_transfer: false);
							CellOffset? postCaptureOffset = smi.fixedCapturePoint.def.postCaptureOffset;
							if (postCaptureOffset.HasValue)
							{
								int cell = Grid.PosToCell(smi.transform.GetPosition());
								int cell2 = Grid.OffsetCell(cell, postCaptureOffset.Value);
								gameObject.transform.SetPosition(Grid.CellToPosCCC(cell2, Grid.SceneLayer.Creatures));
							}
							Capturable component2 = gameObject.GetComponent<Capturable>();
							if (component2 != null)
							{
								component2.MarkForCapture(mark: false);
							}
							Baggable component3 = gameObject.GetComponent<Baggable>();
							if (component3 != null)
							{
								component3.SetWrangled();
							}
						}
					}
				})
				.Target(masterTarget)
				.PlayAnim(delegate(Instance smi)
				{
					string text = smi.fixedCapturePoint.def.preCaptureAnimName;
					Func<FixedCapturePoint.Instance, string> getPreCaptureAnimSuffix = smi.fixedCapturePoint.def.getPreCaptureAnimSuffix;
					if (getPreCaptureAnimSuffix != null)
					{
						text += getPreCaptureAnimSuffix(smi.fixedCapturePoint);
					}
					return text;
				})
				.OnAnimQueueComplete(success);
			capturecreature.EventTransition(GameHashes.CreatureAbandonedCapturePoint, failed).EnterTransition(failed, (Instance smi) => smi.fixedCapturePoint.targetCapturable.IsNullOrStopped()).ToggleWork<Capturable>(creature, success, failed, null);
			failed.GoTo(null);
			success.Enter("PostCaptureRelocate", delegate(Instance smi)
			{
				CellOffset? postCaptureOffset = smi.fixedCapturePoint.def.postCaptureOffset;
				if (postCaptureOffset.HasValue)
				{
					GameObject gameObject = smi.sm.creature.Get(smi);
					if (gameObject != null)
					{
						int cell = Grid.PosToCell(smi.transform.GetPosition());
						int cell2 = Grid.OffsetCell(cell, postCaptureOffset.Value);
						gameObject.transform.SetPosition(Grid.CellToPosCCC(cell2, Grid.SceneLayer.Ore));
					}
				}
			}).ReturnSuccess();
		}

		private static bool HasCreatureLeft(Instance smi)
		{
			return smi.fixedCapturePoint.targetCapturable.IsNullOrStopped() || !smi.fixedCapturePoint.targetCapturable.GetComponent<ChoreConsumer>().IsChoreEqualOrAboveCurrentChorePriority<FixedCaptureStates>();
		}
	}

	public Precondition IsCreatureAvailableForFixedCapture = new Precondition
	{
		id = "IsCreatureAvailableForFixedCapture",
		description = DUPLICANTS.CHORES.PRECONDITIONS.IS_CREATURE_AVAILABLE_FOR_FIXED_CAPTURE,
		fn = delegate(ref Precondition.Context context, object data)
		{
			FixedCapturePoint.Instance instance = data as FixedCapturePoint.Instance;
			return instance.IsCreatureAvailableForFixedCapture();
		}
	};

	public FixedCaptureChore(KPrefabID capture_point)
		: base(Db.Get().ChoreTypes.Ranch, (IStateMachineTarget)capture_point, (ChoreProvider)null, run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.basic, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		FixedCapturePoint.Instance sMI = capture_point.GetSMI<FixedCapturePoint.Instance>();
		AddPrecondition(IsCreatureAvailableForFixedCapture, sMI);
		AddPrecondition(ChorePreconditions.instance.HasSkillPerk, Db.Get().SkillPerks.CanWrangleCreatures.Id);
		AddPrecondition(ChorePreconditions.instance.IsScheduledTime, Db.Get().ScheduleBlockTypes.Work);
		AddPrecondition(ChorePreconditions.instance.CanMoveToCell, sMI.GetRancherInteractCell());
		Operational component = capture_point.GetComponent<Operational>();
		AddPrecondition(ChorePreconditions.instance.IsOperational, component);
		Deconstructable component2 = capture_point.GetComponent<Deconstructable>();
		AddPrecondition(ChorePreconditions.instance.IsNotMarkedForDeconstruction, component2);
		BuildingEnabledButton component3 = capture_point.GetComponent<BuildingEnabledButton>();
		AddPrecondition(ChorePreconditions.instance.IsNotMarkedForDisable, component3);
		base.smi = new FixedCaptureChoreStates.Instance(capture_point);
		SetPrioritizable(capture_point.GetComponent<Prioritizable>());
	}

	public override void Begin(Precondition.Context context)
	{
		base.smi.sm.rancher.Set(context.consumerState.gameObject, base.smi);
		base.smi.sm.creature.Set(base.smi.fixedCapturePoint.targetCapturable.gameObject, base.smi);
		base.Begin(context);
	}
}
