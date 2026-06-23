using UnityEngine;

public class LitterBox : GameStateMachine<LitterBox, LitterBox.Instance, IStateMachineTarget, LitterBox.Def>
{
	public class Def : BaseDef
	{
	}

	public class OperationalStates : State
	{
		public State critterReady;

		public State requiresEmptying;

		public State empty;
	}

	public new class Instance : GameInstance, IPoopStation
	{
		private KBatchedAnimController animController;

		private EmptyLitterboxWorkable workable;

		private Operational operationalCmp;

		private Storage storage;

		private Chore chore;

		private GameObject poopUser;

		public bool IsFull => storage.RemainingCapacity() <= 0f;

		public bool IsCritterOperational
		{
			get
			{
				if (operationalCmp.IsOperational)
				{
					return IsInsideState(base.sm.operational.critterReady);
				}
				return false;
			}
		}

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			animController = GetComponent<KBatchedAnimController>();
			operationalCmp = GetComponent<Operational>();
			storage = GetComponent<Storage>();
			workable = GetComponent<EmptyLitterboxWorkable>();
		}

		public override void StartSM()
		{
			RegisterPoopStation();
			base.StartSM();
		}

		protected override void OnCleanUp()
		{
			UnregisterPoopStation();
		}

		public void DropStorage()
		{
			storage.DropAll();
		}

		public Workable GetWorkable()
		{
			return workable;
		}

		public void CreateWorkableChore()
		{
			if (chore == null)
			{
				chore = new WorkChore<EmptyLitterboxWorkable>(Db.Get().ChoreTypes.CleanLitterBox, workable);
			}
		}

		public void CancelWorkChore()
		{
			if (chore != null)
			{
				chore.Cancel("LitterBox.CancelChore");
				chore = null;
			}
		}

		public float GetAvailablePoopCapacity()
		{
			return storage.RemainingCapacity() / storage.capacityKg;
		}

		public bool IsUserCompatibleWithPoopStation(KPrefabID userPrefabID)
		{
			return userPrefabID.HasTag(GameTags.Creatures.Walker);
		}

		public GameObject GetPoopStationObject()
		{
			return base.gameObject;
		}

		public GameObject GetCurrentPoopStationUser()
		{
			return poopUser;
		}

		public bool IsPoopStationOperational()
		{
			return IsCritterOperational;
		}

		public string[] GetPoopingAnimNames()
		{
			return POOP_INTERACT_ANIM_NAMES;
		}

		public void RegisterPoopStation()
		{
			Components.PoopStations.Add(base.gameObject.GetMyWorldId(), this);
		}

		public void UnregisterPoopStation()
		{
			Components.PoopStations.Remove(base.gameObject.GetMyWorldId(), this);
		}

		public PoopData GetPoopData()
		{
			return new PoopData(skipSpawningPoop: false, storage);
		}

		public void PlayPoopStationAnim(string animName, KAnim.PlayMode playMode)
		{
			animController.Play(animName, playMode);
		}

		public void ClearPoopStationUser(GameObject userRequestingClearing)
		{
			if (poopUser == userRequestingClearing)
			{
				poopUser = null;
				Trigger(-984476291);
			}
		}

		public bool AttemptToReservePoopStation(GameObject userRequestingReserve)
		{
			if (poopUser != null && poopUser != userRequestingReserve)
			{
				return false;
			}
			poopUser = userRequestingReserve;
			return true;
		}
	}

	private static string[] POOP_INTERACT_ANIM_NAMES = new string[3] { "working_pre", "working_loop", "working_pst" };

	public State noOperational;

	public OperationalStates operational;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = noOperational;
		noOperational.TagTransition(GameTags.Operational, operational);
		operational.TagTransition(GameTags.Operational, noOperational, on_remove: true).DefaultState(operational.critterReady);
		operational.critterReady.EventTransition(GameHashes.OnStorageChange, operational.requiresEmptying, RequiresEmptying);
		operational.requiresEmptying.Enter(CreateEmptyLitterBoxChore).WorkableCompleteTransition(GetWorkable, operational.empty).WorkableStopTransition(GetWorkable, noOperational)
			.Exit(CancelEmptyLitterBoxChore);
		operational.empty.Enter(DropStorage).EnterGoTo(operational.critterReady);
	}

	private static Workable GetWorkable(Instance smi)
	{
		return smi.GetWorkable();
	}

	private static bool RequiresEmptying(Instance smi)
	{
		return smi.IsFull;
	}

	private static void DropStorage(Instance smi)
	{
		smi.DropStorage();
	}

	private static void CreateEmptyLitterBoxChore(Instance smi)
	{
		smi.CreateWorkableChore();
	}

	private static void CancelEmptyLitterBoxChore(Instance smi)
	{
		smi.CancelWorkChore();
	}
}
