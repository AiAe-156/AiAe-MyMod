using System;
using STRINGS;
using TUNING;
using UnityEngine;

public class GunkEmptier : GameStateMachine<GunkEmptier, GunkEmptier.Instance, IStateMachineTarget, GunkEmptier.Def>
{
	public class Def : BaseDef
	{
	}

	public class OperationalStates : State
	{
		public State noStorageSpace;

		public State ready;
	}

	public new class Instance : GameInstance
	{
		private Operational operational;

		private Storage storage;

		public float RemainingStorageCapacity => storage.RemainingCapacity();

		public bool IsOperational => operational.IsOperational;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			GunkEmptierWorkable component = GetComponent<GunkEmptierWorkable>();
			component.OnWorkableEventCB = (Action<Workable, Workable.WorkableEvent>)Delegate.Combine(component.OnWorkableEventCB, new Action<Workable, Workable.WorkableEvent>(OnGunkEmptierUsed));
			Components.GunkExtractors.Add(component);
			storage = GetComponent<Storage>();
			operational = GetComponent<Operational>();
			base.gameObject.AddOrGet<Ownable>().AddAssignPrecondition(AssignablePrecondition_OnlyOnBionics);
		}

		protected override void OnCleanUp()
		{
			GunkEmptierWorkable component = GetComponent<GunkEmptierWorkable>();
			component.OnWorkableEventCB = (Action<Workable, Workable.WorkableEvent>)Delegate.Remove(component.OnWorkableEventCB, new Action<Workable, Workable.WorkableEvent>(OnGunkEmptierUsed));
			Components.GunkExtractors.Remove(component);
			base.OnCleanUp();
		}

		private bool AssignablePrecondition_OnlyOnBionics(MinionAssignablesProxy worker)
		{
			return worker.GetMinionModel() == BionicMinionConfig.MODEL;
		}

		public void OnGunkEmptierUsed(Workable workable, Workable.WorkableEvent ev)
		{
			if (ev == Workable.WorkableEvent.WorkCompleted)
			{
				AddDisseaseToWorker(workable.worker);
			}
		}

		public void AddDisseaseToWorker(WorkerBase worker)
		{
			if (worker != null)
			{
				byte index = Db.Get().Diseases.GetIndex(DISEASE_ID);
				worker.GetComponent<PrimaryElement>().AddDisease(index, DISEASE_ON_DUPE_COUNT_PER_USE, "GunkEmptier.Flush");
				PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Resource, string.Format(DUPLICANTS.DISEASES.ADDED_POPFX, Db.Get().Diseases[index].Name, DISEASE_ON_DUPE_COUNT_PER_USE), base.transform, Vector3.up);
				Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_LotsOfGerms);
			}
			else
			{
				DebugUtil.LogWarningArgs("Tried to add disease on gunk emptier use but worker was null");
			}
		}
	}

	private static string DISEASE_ID = DUPLICANTSTATS.BIONICS.Secretions.PEE_DISEASE;

	private static int DISEASE_ON_DUPE_COUNT_PER_USE = DUPLICANTSTATS.BIONICS.Secretions.DISEASE_PER_PEE / 20;

	public State noOperational;

	public OperationalStates operational;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = noOperational;
		noOperational.EventTransition(GameHashes.OperationalChanged, operational, IsOperational);
		operational.EventTransition(GameHashes.OperationalChanged, noOperational, GameStateMachine<GunkEmptier, Instance, IStateMachineTarget, Def>.Not(IsOperational)).DefaultState(operational.noStorageSpace);
		operational.noStorageSpace.ToggleStatusItem(Db.Get().BuildingStatusItems.GunkEmptierFull).EventTransition(GameHashes.OnStorageChange, operational.ready, HasSpaceToEmptyABionicGunkTank);
		operational.ready.EventTransition(GameHashes.OnStorageChange, operational.noStorageSpace, GameStateMachine<GunkEmptier, Instance, IStateMachineTarget, Def>.Not(HasSpaceToEmptyABionicGunkTank)).ToggleRecurringChore(CreateChore);
	}

	public static bool HasSpaceToEmptyABionicGunkTank(Instance smi)
	{
		return smi.RemainingStorageCapacity >= GunkMonitor.GUNK_CAPACITY;
	}

	public static bool IsOperational(Instance smi)
	{
		return smi.IsOperational;
	}

	private static WorkChore<GunkEmptierWorkable> CreateChore(Instance smi)
	{
		WorkChore<GunkEmptierWorkable> workChore = new WorkChore<GunkEmptierWorkable>(Db.Get().ChoreTypes.ExpellGunk, smi.master, null, run_until_complete: true, null, null, null, allow_in_red_alert: false, null, ignore_schedule_block: true, only_when_operational: true, null, is_preemptable: false, allow_in_context_menu: true, allow_prioritization: false, PriorityScreen.PriorityClass.personalNeeds, 5, ignore_building_assignment: false, add_to_daily_report: false);
		workChore.AddPrecondition(ChorePreconditions.instance.IsPreferredAssignableOrUrgentBladder, smi.master.GetComponent<Assignable>());
		return workChore;
	}
}
