using System;
using UnityEngine;

public class SweepStates : GameStateMachine<SweepStates, SweepStates.Instance, IStateMachineTarget, SweepStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		[MyCmpGet]
		public Navigator navigator;

		[MyCmpGet]
		public KBatchedAnimController animController;

		[MySmiGet]
		public StorageUnloadMonitor.Instance storageMonitor;

		[MySmiGet]
		public AnimInterruptMonitor.Instance animInterruptMonitor;

		[MyCmpGet]
		public SingleEntityReceptacle displayReceptacle;

		[MyCmpGet]
		public LoopingSounds loopingSounds;

		public Storage dustbinStorage;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			dustbinStorage = base.smi.master.gameObject.GetComponents<Storage>()[1];
		}

		public override void StartSM()
		{
			base.StartSM();
			GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().RobotStatusItems.Working, base.gameObject);
		}

		protected override void OnCleanUp()
		{
			base.OnCleanUp();
			GetComponent<KSelectable>().RemoveStatusItem(Db.Get().RobotStatusItems.Working);
		}
	}

	public const float TIME_UNTIL_BORED = 30f;

	public const string MOVE_LOOP_SOUND = "SweepBot_mvmt_lp";

	public BoolParameter headingRight;

	private FloatParameter timeUntilBored;

	public BoolParameter bored;

	private State beginPatrol;

	private State moving;

	private State pause;

	private State mopping;

	private State redirected;

	private State emoteRedirected;

	private State sweep;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = beginPatrol;
		beginPatrol.Enter(delegate(Instance smi)
		{
			smi.sm.timeUntilBored.Set(30f, smi);
			smi.GoTo(moving);
			Instance instance = smi;
			instance.OnStop = (Action<string, Status>)Delegate.Combine(instance.OnStop, (Action<string, Status>)delegate
			{
				StopMoveSound(smi);
			});
		});
		moving.Enter(delegate
		{
		}).MoveTo(GetNextCell, pause, redirected).Update(delegate(Instance smi, float dt)
		{
			smi.sm.timeUntilBored.Set(smi.sm.timeUntilBored.Get(smi) - dt, smi);
			if (smi.sm.timeUntilBored.Get(smi) <= 0f)
			{
				smi.sm.bored.Set(value: true, smi);
				smi.sm.timeUntilBored.Set(30f, smi);
				smi.animInterruptMonitor.PlayAnim("react_bored");
			}
			Storage storage = smi.storageMonitor.sm.sweepLocker.Get(smi.storageMonitor);
			if (storage != null && smi.sm.headingRight.Get(smi) == smi.master.transform.position.x > storage.transform.position.x)
			{
				Navigator navigator = smi.navigator;
				if (navigator.GetNavigationCost(Grid.PosToCell(storage)) >= navigator.maxProbeRadiusX - 1)
				{
					smi.GoTo(smi.sm.emoteRedirected);
				}
			}
		}, UpdateRate.SIM_1000ms);
		emoteRedirected.Enter(delegate(Instance smi)
		{
			StopMoveSound(smi);
			int cell = Grid.PosToCell(smi.master.gameObject);
			if (Grid.IsCellOffsetValid(cell, headingRight.Get(smi) ? 1 : (-1), -1) && !Grid.Solid[Grid.OffsetCell(cell, headingRight.Get(smi) ? 1 : (-1), -1)])
			{
				smi.animController.Play("gap");
			}
			else
			{
				smi.animController.Play("bump");
			}
			headingRight.Set(!headingRight.Get(smi), smi);
		}).OnAnimQueueComplete(pause);
		redirected.StopMoving().GoTo(emoteRedirected);
		sweep.PlayAnim("pickup").ToggleEffect("BotSweeping").Enter(delegate(Instance smi)
		{
			StopMoveSound(smi);
			smi.sm.bored.Set(value: false, smi);
			smi.sm.timeUntilBored.Set(30f, smi);
		})
			.OnAnimQueueComplete(moving);
		pause.Enter(delegate(Instance smi)
		{
			if (Grid.IsLiquid(Grid.PosToCell(smi)))
			{
				smi.GoTo(mopping);
			}
			else if (TrySweep(smi))
			{
				smi.GoTo(sweep);
			}
			else
			{
				smi.GoTo(moving);
			}
		});
		mopping.PlayAnim("mop_pre", KAnim.PlayMode.Once).QueueAnim("mop_loop", loop: true).ToggleEffect("BotMopping")
			.Enter(delegate(Instance smi)
			{
				smi.sm.timeUntilBored.Set(30f, smi);
				smi.sm.bored.Set(value: false, smi);
				StopMoveSound(smi);
			})
			.Update(delegate(Instance smi, float dt)
			{
				if (smi.timeinstate > 16f || !Grid.IsLiquid(Grid.PosToCell(smi)))
				{
					smi.GoTo(moving);
				}
				else
				{
					TryMop(smi, dt);
				}
			}, UpdateRate.SIM_1000ms);
	}

	public void StopMoveSound(Instance smi)
	{
		smi.loopingSounds.StopSound(GlobalAssets.GetSound("SweepBot_mvmt_lp"));
		smi.loopingSounds.StopAllSounds();
	}

	public void StartMoveSound(Instance smi)
	{
		if (!smi.loopingSounds.IsSoundPlaying(GlobalAssets.GetSound("SweepBot_mvmt_lp")))
		{
			smi.loopingSounds.StartSound(GlobalAssets.GetSound("SweepBot_mvmt_lp"));
		}
	}

	public void TryMop(Instance smi, float dt)
	{
		int cell = Grid.PosToCell(smi);
		if (!Grid.IsLiquid(cell))
		{
			return;
		}
		Moppable.MopCell(cell, Mathf.Min(Grid.Mass[cell], 10f * dt), delegate(Sim.MassConsumedCallback mass_cb_info, object data)
		{
			if (this != null && mass_cb_info.mass > 0f)
			{
				SubstanceChunk substanceChunk = LiquidSourceManager.Instance.CreateChunk(ElementLoader.elements[mass_cb_info.elemIdx], mass_cb_info.mass, mass_cb_info.temperature, mass_cb_info.diseaseIdx, mass_cb_info.diseaseCount, Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore));
				substanceChunk.transform.SetPosition(substanceChunk.transform.GetPosition() + new Vector3((UnityEngine.Random.value - 0.5f) * 0.5f, 0f, 0f));
				TryStore(substanceChunk.gameObject, smi);
			}
		});
	}

	public bool TrySweep(Instance smi)
	{
		int cell = Grid.PosToCell(smi);
		GameObject gameObject = Grid.Objects[cell, 3];
		if (gameObject != null)
		{
			for (ObjectLayerListItem nextItem = gameObject.GetComponent<Pickupable>().objectLayerListItem.nextItem; nextItem != null; nextItem = nextItem.nextItem)
			{
				if (TryStore(nextItem.gameObject, smi))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public bool TryStore(GameObject go, Instance smi)
	{
		Pickupable component = go.GetComponent<Pickupable>();
		if (component == null)
		{
			return false;
		}
		if (smi.dustbinStorage.IsFull())
		{
			return false;
		}
		if (component != null && component.absorbable)
		{
			SingleEntityReceptacle displayReceptacle = smi.displayReceptacle;
			if (component.gameObject == displayReceptacle.Occupant)
			{
				return false;
			}
			bool flag = false;
			if (component.TotalAmount > 10f)
			{
				component.GetComponent<EntitySplitter>();
				component = EntitySplitter.Split(component, Mathf.Min(10f, smi.dustbinStorage.RemainingCapacity()));
				smi.dustbinStorage.Store(component.gameObject);
				flag = true;
			}
			else
			{
				smi.dustbinStorage.Store(component.gameObject);
				flag = true;
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNextCell(Instance smi)
	{
		int i = 0;
		int num = Grid.PosToCell(smi);
		int invalidCell = Grid.InvalidCell;
		if (!Grid.Solid[Grid.CellBelow(num)])
		{
			return Grid.InvalidCell;
		}
		if (Grid.Solid[num])
		{
			return Grid.InvalidCell;
		}
		for (; i < 1; i++)
		{
			invalidCell = (smi.sm.headingRight.Get(smi) ? Grid.CellRight(num) : Grid.CellLeft(num));
			if (!Grid.IsValidCell(invalidCell) || Grid.Solid[invalidCell] || !Grid.IsValidCell(Grid.CellBelow(invalidCell)) || !Grid.Solid[Grid.CellBelow(invalidCell)])
			{
				break;
			}
			num = invalidCell;
		}
		if (num == Grid.PosToCell(smi))
		{
			return Grid.InvalidCell;
		}
		return num;
	}
}
