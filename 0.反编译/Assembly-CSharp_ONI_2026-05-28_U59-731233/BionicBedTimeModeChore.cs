using System;
using UnityEngine;

public class BionicBedTimeModeChore : Chore<BionicBedTimeModeChore.Instance>
{
	public class States : GameStateMachine<States, Instance, BionicBedTimeModeChore>
	{
		public class DefragmentingStates : State
		{
			public State pre;

			public State loop;

			public State pst;
		}

		public ApproachSubState<IApproachable> approach;

		public State defragmentingOnAssignable;

		public DefragmentingStates defragmentingWithoutAssignable;

		public State enter;

		public State end;

		public State unassigning;

		public TargetParameter bionic;

		public TargetParameter defragmentationZone;

		public Signal defragmentationZoneChangedSignal;

		public Signal defragmentationZoneUnassignined;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = enter;
			root.ToggleEffect("BionicBedTimeEffect");
			enter.Transition(null, GameStateMachine<States, BionicBedTimeModeChore.Instance, BionicBedTimeModeChore, object>.Not(IsBedTimeAllowed)).ParamTransition(defragmentationZone, approach, HasDefragmentationZoneAssignedAndReachable).GoTo(defragmentingWithoutAssignable);
			unassigning.ScheduleActionNextFrame("Frame delay on unassign", delegate(BionicBedTimeModeChore.Instance smi)
			{
				UpdateAssignedDefragmentationZone(smi);
				smi.GoTo(enter);
			});
			approach.InitializeStates(bionic, defragmentationZone, defragmentingOnAssignable).OnSignal(defragmentationZoneUnassignined, unassigning).ScheduleChange(null, GameStateMachine<States, BionicBedTimeModeChore.Instance, BionicBedTimeModeChore, object>.Not(IsBedTimeAllowed))
				.EventTransition(GameHashes.BionicOffline, null);
			defragmentingOnAssignable.OnTargetLost(defragmentationZone, defragmentingWithoutAssignable).OnSignal(defragmentationZoneChangedSignal, enter).OnSignal(defragmentationZoneUnassignined, unassigning)
				.EventTransition(GameHashes.BionicOffline, null)
				.ScheduleChange(null, GameStateMachine<States, BionicBedTimeModeChore.Instance, BionicBedTimeModeChore, object>.Not(IsBedTimeAllowed))
				.ToggleWork("Defragmenting", BeginWorkOnZone, (BionicBedTimeModeChore.Instance smi) => smi.GetAssignedDefragmentationZone() != null, end, null)
				.ToggleTag(GameTags.BionicBedTime);
			defragmentingWithoutAssignable.ParamTransition(defragmentationZone, approach, HasDefragmentationZoneAssignedAndReachable).EventTransition(GameHashes.AssignableReachabilityChanged, approach, HasDefragmentationZoneAssignedAndReachable).ToggleAnims("anim_bionic_kanim")
				.ToggleTag(GameTags.BionicBedTime)
				.DefaultState(defragmentingWithoutAssignable.pre);
			defragmentingWithoutAssignable.pre.PlayAnim("low_power_pre").OnAnimQueueComplete(defragmentingWithoutAssignable.loop).ScheduleGoTo(1.5f, defragmentingWithoutAssignable.loop);
			defragmentingWithoutAssignable.loop.ScheduleChange(defragmentingWithoutAssignable.pst, GameStateMachine<States, BionicBedTimeModeChore.Instance, BionicBedTimeModeChore, object>.Not(IsBedTimeAllowed)).EventTransition(GameHashes.BionicOffline, defragmentingWithoutAssignable.pst).PlayAnim("low_power_loop", KAnim.PlayMode.Loop);
			defragmentingWithoutAssignable.pst.PlayAnim("low_power_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(end);
			end.ReturnSuccess();
		}
	}

	public class Instance : GameStateMachine<States, Instance, BionicBedTimeModeChore, object>.GameInstance
	{
		public BionicBedTimeMonitor.Instance bedTimeMonitor;

		private DefragmentationZone lastAssignedDefragmentationZone;

		private Ownables ownables;

		public DefragmentationZone GetAssignedDefragmentationZone()
		{
			return lastAssignedDefragmentationZone;
		}

		public Instance(BionicBedTimeModeChore master, GameObject duplicant)
			: base(master)
		{
			bedTimeMonitor = duplicant.GetSMI<BionicBedTimeMonitor.Instance>();
			base.sm.bionic.Set(duplicant, this);
			ownables = GetComponent<MinionIdentity>().GetSoleOwner();
			base.gameObject.Subscribe(-1585839766, UpdateAssignedDefragmentationZone);
			UpdateAssignedDefragmentationZone(null);
		}

		protected override void OnCleanUp()
		{
			base.gameObject.Unsubscribe(-1585839766, UpdateAssignedDefragmentationZone);
			base.OnCleanUp();
		}

		public override void StartSM()
		{
			UpdateAssignedDefragmentationZone(null);
			base.StartSM();
		}

		public bool IsDefragmentationZoneReachable()
		{
			AssignableReachabilitySensor sensor = GetComponent<Sensors>().GetSensor<AssignableReachabilitySensor>();
			return sensor.IsReachable(Db.Get().AssignableSlots.Bed);
		}

		public void UpdateAssignedDefragmentationZone(object slotInstanceObject)
		{
			DefragmentationZone defragmentationZone = null;
			AssignableSlotInstance assignableSlotInstance = ((slotInstanceObject == null) ? null : ((AssignableSlotInstance)slotInstanceObject));
			Assignable assignable = ownables.GetAssignable(Db.Get().AssignableSlots.Bed);
			if (assignableSlotInstance != null && assignableSlotInstance.IsUnassigning())
			{
				base.sm.defragmentationZoneUnassignined.Trigger(this);
				return;
			}
			if (assignable == null)
			{
				assignable = ownables.AutoAssignSlot(Db.Get().AssignableSlots.Bed);
			}
			if (assignable != null)
			{
				defragmentationZone = assignable.GetComponent<DefragmentationZone>();
			}
			if (lastAssignedDefragmentationZone != defragmentationZone)
			{
				AssignableReachabilitySensor sensor = GetComponent<Sensors>().GetSensor<AssignableReachabilitySensor>();
				if (sensor.IsEnabled)
				{
					sensor.Update();
				}
				lastAssignedDefragmentationZone = defragmentationZone;
				base.sm.defragmentationZone.Set(lastAssignedDefragmentationZone, this);
				base.sm.defragmentationZoneChangedSignal.Trigger(this);
			}
		}
	}

	public const string EFFECT_NAME = "BionicBedTimeEffect";

	public BionicBedTimeModeChore(IStateMachineTarget master)
		: base(Db.Get().ChoreTypes.BionicBedtimeMode, master, master.GetComponent<ChoreProvider>(), run_until_complete: true, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.personalNeeds, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new Instance(this, master.gameObject);
		AddPrecondition(ChorePreconditions.instance.IsNotRedAlert);
	}

	public static void BeginWorkOnZone(Instance smi)
	{
		WorkerBase workerBase = smi.sm.bionic.Get<WorkerBase>(smi);
		DefragmentationZone assignedDefragmentationZone = smi.GetAssignedDefragmentationZone();
		workerBase.StartWork(new WorkerBase.StartWorkInfo(assignedDefragmentationZone));
	}

	public static bool HasDefragmentationZoneAssignedAndReachable(Instance smi, GameObject defragmentationZone)
	{
		return defragmentationZone != null && smi.IsDefragmentationZoneReachable();
	}

	public static bool HasDefragmentationZoneAssignedAndReachable(Instance smi)
	{
		return smi.sm.defragmentationZone.Get(smi) != null && smi.IsDefragmentationZoneReachable();
	}

	public static bool IsBedTimeAllowed(Instance smi)
	{
		return BionicBedTimeMonitor.CanGoToBedTime(smi.bedTimeMonitor);
	}

	public static void UpdateAssignedDefragmentationZone(Instance smi)
	{
		smi.UpdateAssignedDefragmentationZone(null);
	}
}
