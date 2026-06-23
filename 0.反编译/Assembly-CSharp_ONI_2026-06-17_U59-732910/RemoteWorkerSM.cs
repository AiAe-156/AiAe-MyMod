using KSerialization;
using UnityEngine;

public class RemoteWorkerSM : StateMachineComponent<RemoteWorkerSM.StatesInstance>
{
	public class StatesInstance : GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.GameInstance
	{
		public StatesInstance(RemoteWorkerSM master)
			: base(master)
		{
			base.sm.homedock.Set(base.smi.master.HomeDepot, base.smi);
		}
	}

	public class States : GameStateMachine<States, StatesInstance, RemoteWorkerSM>
	{
		public class ControlledStates : State
		{
			public class WorkingStates : State
			{
				public State find_work;

				public State do_work;
			}

			public State exit_dock;

			public WorkingStates working;

			public State no_work;
		}

		public class UncontrolledStates : State
		{
			public class WorkingDockStates : State
			{
				public State new_worker;

				public State enter;

				public State recharge;

				public State recharge_pst;

				public State drain_gunk;

				public State drain_gunk_pst;

				public State fill_oil;

				public State fill_oil_pst;
			}

			public State approach_dock;

			public WorkingDockStates working;

			public State idle;
		}

		public class IncapacitatedStates : State
		{
			public State lost;

			public State lost_recovery;

			public State die;

			public State explode;
		}

		public ControlledStates controlled;

		public UncontrolledStates uncontrolled;

		public IncapacitatedStates incapacitated;

		public TargetParameter homedock;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = uncontrolled;
			root.Update(delegate(StatesInstance smi, float dt)
			{
				smi.GetComponent<Navigator>().UpdateProbe();
			}, UpdateRate.SIM_4000ms);
			controlled.Enter(delegate(StatesInstance smi)
			{
				smi.master.Available = false;
			}).EnterTransition(controlled.exit_dock, IsInsideDock).EnterTransition(controlled.working, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.Not(IsInsideDock))
				.Transition(uncontrolled, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.Not(HasRemoteOperator))
				.Transition(incapacitated.lost, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.Not(CanReachDepot))
				.Transition(incapacitated.die, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.Not(HasHomeDepot))
				.Update(TickResources);
			controlled.exit_dock.ToggleWork<RemoteWorkerDock.ExitableDock>(homedock, controlled.working, controlled.working, (StatesInstance _) => true);
			controlled.working.Enter(delegate(StatesInstance smi)
			{
				smi.master.ActivelyWorking = true;
			}).Exit(delegate(StatesInstance smi)
			{
				smi.master.ActivelyWorking = false;
			}).DefaultState(controlled.working.find_work);
			controlled.working.find_work.Enter(delegate(StatesInstance smi)
			{
				if (HasChore(smi))
				{
					smi.GoTo(controlled.working.do_work);
				}
				else
				{
					SetNextChore(smi);
					smi.GoTo(HasChore(smi) ? controlled.working.do_work : controlled.no_work);
				}
			});
			controlled.working.do_work.Exit(ClearChore).Transition(controlled.working.find_work, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.Not(HasChore));
			controlled.no_work.Transition(controlled.working.do_work, HasChore).Transition(controlled.working.find_work, HasChoreQueued);
			uncontrolled.EnterTransition(uncontrolled.working.new_worker, IsNewWorker).EnterTransition(uncontrolled.idle, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.And(IsInsideDock, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.Not(IsNewWorker))).EnterTransition(uncontrolled.approach_dock, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.And(GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.Not(IsInsideDock), GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.Not(IsNewWorker)))
				.Transition(controlled.working.find_work, HasRemoteOperator)
				.Transition(incapacitated.lost, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.Not(CanReachDepot))
				.Transition(incapacitated.die, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.Not(HasHomeDepot));
			uncontrolled.approach_dock.Enter(delegate(StatesInstance smi)
			{
				smi.master.Available = true;
			}).MoveTo<IApproachable>(homedock, uncontrolled.working.enter, incapacitated.lost);
			uncontrolled.working.Enter(delegate(StatesInstance smi)
			{
				smi.master.Available = false;
			});
			uncontrolled.working.new_worker.Enter(delegate(StatesInstance smi)
			{
				smi.master.playNewWorker = false;
			}).ToggleWork<RemoteWorkerDock.NewWorker>(homedock, uncontrolled.working.recharge, uncontrolled.working.recharge, (StatesInstance _) => true);
			uncontrolled.working.enter.ToggleWork<RemoteWorkerDock.EnterableDock>(homedock, uncontrolled.working.recharge, uncontrolled.idle, (StatesInstance _) => true);
			uncontrolled.working.recharge.ToggleWork<RemoteWorkerDock.WorkerRecharger>(homedock, uncontrolled.working.recharge_pst, uncontrolled.idle, (StatesInstance _) => true);
			uncontrolled.working.recharge_pst.OnAnimQueueComplete(uncontrolled.working.drain_gunk).ScheduleGoTo(1f, uncontrolled.working.drain_gunk);
			uncontrolled.working.drain_gunk.ToggleWork<RemoteWorkerDock.WorkerGunkRemover>(homedock, uncontrolled.working.drain_gunk_pst, uncontrolled.idle, (StatesInstance _) => true);
			uncontrolled.working.drain_gunk_pst.OnAnimQueueComplete(uncontrolled.working.fill_oil).ScheduleGoTo(1f, uncontrolled.working.fill_oil);
			uncontrolled.working.fill_oil.ToggleWork<RemoteWorkerDock.WorkerOilRefiller>(homedock, uncontrolled.working.fill_oil_pst, uncontrolled.idle, (StatesInstance _) => true);
			uncontrolled.working.fill_oil_pst.OnAnimQueueComplete(uncontrolled.idle).ScheduleGoTo(1f, uncontrolled.idle);
			uncontrolled.idle.Enter(delegate(StatesInstance smi)
			{
				smi.master.Available = true;
			}).PlayAnim(RemoteWorkerConfig.IDLE_IN_DOCK_ANIM, KAnim.PlayMode.Loop).Transition(uncontrolled.working.recharge, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.And(RequiresMaintnence, DockIsOperational), UpdateRate.SIM_1000ms);
			incapacitated.lost.Enter(delegate(StatesInstance smi)
			{
				smi.Play("sos_pre");
				smi.Queue("sos_loop", KAnim.PlayMode.Loop);
				ClearChore(smi);
			}).ToggleStatusItem(Db.Get().DuplicantStatusItems.UnreachableDock).Transition(incapacitated.lost_recovery, CanReachDepot)
				.Transition(incapacitated.die, GameStateMachine<States, StatesInstance, RemoteWorkerSM, object>.Not(HasHomeDepot));
			incapacitated.lost_recovery.PlayAnim("sos_pst").OnAnimQueueComplete(controlled);
			incapacitated.die.Enter(ClearChore).PlayAnim("explode").OnAnimQueueComplete(incapacitated.explode)
				.ToggleStatusItem(Db.Get().DuplicantStatusItems.NoHomeDock);
			incapacitated.explode.Enter(Explode);
		}

		public static bool IsNewWorker(StatesInstance smi)
		{
			return smi.master.playNewWorker;
		}

		public static void SetNextChore(StatesInstance smi)
		{
			smi.master.StartNextChore();
		}

		public static void ClearChore(StatesInstance smi)
		{
			smi.master.driver.StopChore();
		}

		public static bool HasChore(StatesInstance smi)
		{
			return smi.master.driver.HasChore();
		}

		public static bool HasChoreQueued(StatesInstance smi)
		{
			return smi.master.HasChoreQueued();
		}

		public static bool CanReachDepot(StatesInstance smi)
		{
			int depotCell = GetDepotCell(smi);
			if (depotCell == Grid.InvalidCell)
			{
				return false;
			}
			return smi.master.GetComponent<Navigator>().CanReach(depotCell);
		}

		public static int GetDepotCell(StatesInstance smi)
		{
			RemoteWorkerDock homeDepot = smi.master.HomeDepot;
			if (homeDepot == null)
			{
				return Grid.InvalidCell;
			}
			return Grid.PosToCell(homeDepot);
		}

		public static bool HasRemoteOperator(StatesInstance smi)
		{
			return smi.master.ActivelyControlled;
		}

		public static bool RequiresMaintnence(StatesInstance smi)
		{
			return smi.master.RequiresMaintnence;
		}

		public static bool DockIsOperational(StatesInstance smi)
		{
			if (smi.master.HomeDepot != null)
			{
				return smi.master.HomeDepot.IsOperational;
			}
			return false;
		}

		public static bool HasHomeDepot(StatesInstance smi)
		{
			return GetDepotCell(smi) != Grid.InvalidCell;
		}

		public static void StopWork(StatesInstance smi)
		{
			if (smi.master.driver.HasChore())
			{
				smi.master.driver.StopChore();
			}
		}

		public static bool IsInsideDock(StatesInstance smi)
		{
			return smi.master.Docked;
		}

		public static void Explode(StatesInstance smi)
		{
			Game.Instance.SpawnFX(SpawnFXHashes.MeteorImpactDust, smi.master.transform.position, 0f);
			PrimaryElement component = smi.master.GetComponent<PrimaryElement>();
			component.Element.substance.SpawnResource(Grid.CellToPosCCC(Grid.PosToCell(smi.master.gameObject), Grid.SceneLayer.Ore), 42f, component.Temperature, component.DiseaseIdx, component.DiseaseCount);
			Util.KDestroyGameObject(smi.master.gameObject);
		}

		public static void TickResources(StatesInstance smi, float dt)
		{
			if (dt > 0f)
			{
				smi.master.TickResources(dt);
			}
		}
	}

	[MyCmpAdd]
	private RemoteWorkerCapacitor power;

	[MyCmpAdd]
	private RemoteWorkerGunkMonitor gunk;

	[MyCmpAdd]
	private RemoteWorkerOilMonitor oil;

	[MyCmpAdd]
	private ChoreDriver driver;

	[MyCmpGet]
	private ChoreConsumer consumer;

	[MyCmpGet]
	private Storage storage;

	public bool playNewWorker;

	[Serialize]
	private bool docked = true;

	private Chore.Precondition.Context? nextChore;

	private const string LostAnim_pre = "sos_pre";

	private const string LostAnim_loop = "sos_loop";

	private const string LostAnim_pst = "sos_pst";

	private const string DeathAnim = "explode";

	[Serialize]
	private Ref<RemoteWorkerDock> homeDepot;

	public bool Docked
	{
		get
		{
			return docked;
		}
		set
		{
			docked = value;
		}
	}

	public RemoteWorkerDock HomeDepot
	{
		get
		{
			return homeDepot?.Get();
		}
		set
		{
			homeDepot = new Ref<RemoteWorkerDock>(value);
		}
	}

	public ChoreConsumerState ConsumerState => consumer.consumerState;

	public bool ActivelyControlled { get; set; }

	public bool ActivelyWorking { get; set; }

	public bool Available { get; set; }

	public bool RequiresMaintnence => power.IsLowPower;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		base.smi.StartSM();
	}

	public void SetNextChore(Chore.Precondition.Context next)
	{
		if (nextChore.HasValue)
		{
			nextChore.Value.chore.Reserve(null);
		}
		nextChore = next;
		next.chore.Reserve(driver);
	}

	public void StartNextChore()
	{
		if (nextChore.HasValue)
		{
			driver.SetChore(nextChore.Value);
			nextChore = null;
		}
	}

	public bool HasChoreQueued()
	{
		return nextChore.HasValue;
	}

	public void TickResources(float dt)
	{
		power.ApplyDeltaEnergy(-0.1f * dt);
		storage.ConsumeAndGetDisease(GameTags.LubricatingOil, 1f / 30f * dt, out var amount_consumed, out var disease_info, out var aggregate_temperature);
		if (amount_consumed > 0f)
		{
			storage.AddElement(SimHashes.LiquidGunk, amount_consumed, aggregate_temperature, disease_info.idx, disease_info.count, keep_zero_mass: true);
		}
	}

	public GameObject FindStation()
	{
		if (Components.ComplexFabricators.Count == 0)
		{
			return null;
		}
		return Components.ComplexFabricators[0].gameObject;
	}

	public bool HasHomeDepot()
	{
		return !HomeDepot.IsNullOrDestroyed();
	}
}
