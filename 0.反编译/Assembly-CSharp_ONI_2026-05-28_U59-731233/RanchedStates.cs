using STRINGS;

public class RanchedStates : GameStateMachine<RanchedStates, RanchedStates.Instance, IStateMachineTarget, RanchedStates.Def>
{
	public class Def : BaseDef
	{
		public string StartWaitingAnim = "queue_pre";

		public string WaitingAnim = "queue_loop";

		public string EndWaitingAnim = "queue_pst";

		public int WaitCellOffset = 1;
	}

	public new class Instance : GameInstance
	{
		public float OriginalSpeed;

		private int waitCell = 0;

		private KBatchedAnimController animController = null;

		private RanchableMonitor.Instance ranchMonitor = null;

		public float cheerAnimLength;

		public RanchableMonitor.Instance Monitor
		{
			get
			{
				if (ranchMonitor == null)
				{
					ranchMonitor = this.GetSMI<RanchableMonitor.Instance>();
				}
				return ranchMonitor;
			}
		}

		public KBatchedAnimController AnimController => animController;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			animController = GetComponent<KBatchedAnimController>();
			OriginalSpeed = Monitor.NavComponent.defaultSpeed;
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.WantsToGetRanched);
			KAnim.Anim anim = animController.GetAnim(new HashedString("excited_loop"));
			cheerAnimLength = ((anim != null) ? (anim.totalTime + 0.2f) : 1.2f);
		}

		public RanchStation.Instance GetRanchStation()
		{
			return (Monitor == null) ? null : Monitor.TargetRanchStation;
		}

		public void EnterQueue()
		{
			if (GetRanchStation() != null)
			{
				InitializeWaitCell();
				Monitor.NavComponent.GoTo(waitCell);
			}
		}

		public void AbandonRanchStation()
		{
			if (Monitor.TargetRanchStation != null && status != Status.Failed)
			{
				StopSM("Abandoned Ranch");
			}
		}

		public void SetRanchStation(RanchStation.Instance ranch_station)
		{
			if (Monitor.TargetRanchStation != null && Monitor.TargetRanchStation != ranch_station)
			{
				Monitor.TargetRanchStation.Abandon(base.smi.Monitor);
			}
			base.smi.sm.ranchTarget.Set(ranch_station.gameObject, base.smi);
			Monitor.TargetRanchStation = ranch_station;
		}

		public int ModifyNavTargetForCritter(int navCell)
		{
			if (base.smi.HasTag(GameTags.Creatures.Flyer))
			{
				return Grid.CellAbove(navCell);
			}
			return navCell;
		}

		private void InitializeWaitCell()
		{
			if (GetRanchStation() != null)
			{
				int cell = 0;
				Extents stationExtents = Monitor.TargetRanchStation.StationExtents;
				int cell2 = ModifyNavTargetForCritter(Grid.XYToCell(stationExtents.x, stationExtents.y));
				int num = 0;
				if (Grid.Raycast(cell2, new Vector2I(-1, 0), out var hitDistance, base.def.WaitCellOffset, ~(Grid.BuildFlags.DupePassable | Grid.BuildFlags.DupeImpassable)))
				{
					num = 1 + base.def.WaitCellOffset - hitDistance;
					cell = ModifyNavTargetForCritter(Grid.XYToCell(stationExtents.x + 1, stationExtents.y));
				}
				int num2 = 0;
				if (num != 0 && Grid.Raycast(cell, new Vector2I(1, 0), out var hitDistance2, base.def.WaitCellOffset, ~(Grid.BuildFlags.DupePassable | Grid.BuildFlags.DupeImpassable)))
				{
					num2 = base.def.WaitCellOffset - hitDistance2;
				}
				int x = (base.def.WaitCellOffset - num) * -1;
				if (num == base.def.WaitCellOffset)
				{
					x = 1 + base.def.WaitCellOffset - num2;
				}
				CellOffset offset = new CellOffset(x, 0);
				waitCell = Grid.OffsetCell(cell2, offset);
			}
		}

		public void UpdateWaitingState()
		{
			if (IsCrittersTurn(base.smi))
			{
				if (base.smi.IsInsideState(base.sm.ranch.Wait.Waiting))
				{
					base.smi.GoTo(base.smi.sm.ranch.Wait.DoneWaiting);
				}
				else
				{
					base.smi.GoTo(base.smi.sm.ranch.Cheer);
				}
			}
			else
			{
				base.smi.GoTo(base.smi.sm.ranch.Wait.WaitInLine);
			}
		}
	}

	public class RanchStates : State
	{
		public CheerStates Cheer;

		public MoveStates Move;

		public WaitStates Wait;

		public State Ranching;

		public State Wavegoodbye;

		public State Runaway;
	}

	public class CheerStates : State
	{
		public State Cheer;

		public State Pst;
	}

	public class MoveStates : State
	{
		public State MoveToRanch;
	}

	public class WaitStates : State
	{
		public State WaitInLine;

		public State Waiting;

		public State DoneWaiting;
	}

	private RanchStates ranch;

	private TargetParameter ranchTarget;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = ranch;
		root.Exit("AbandonedRanchStation", delegate(Instance smi)
		{
			if (smi.Monitor.TargetRanchStation != null)
			{
				if (smi.Monitor.TargetRanchStation.IsCritterInQueue(smi.Monitor))
				{
					Debug.LogWarning("Why are we exiting RanchedStates while in the queue?");
					smi.Monitor.TargetRanchStation.Abandon(smi.Monitor);
				}
				smi.Monitor.TargetRanchStation = null;
			}
			smi.sm.ranchTarget.Set(null, smi);
		});
		ranch.EnterTransition(ranch.Cheer, (Instance smi) => IsCrittersTurn(smi)).EventHandler(GameHashes.RanchStationNoLongerAvailable, delegate(Instance smi)
		{
			smi.GoTo((BaseState)null);
		}).BehaviourComplete(GameTags.Creatures.WantsToGetRanched, on_exit: true)
			.Update(delegate(Instance smi, float deltaSeconds)
			{
				RanchStation.Instance ranchStation = smi.GetRanchStation();
				if (ranchStation.IsNullOrDestroyed())
				{
					smi.StopSM("No more target ranch station.");
				}
				else
				{
					Option<CavityInfo> option = Option.Maybe(Game.Instance.roomProber.GetCavityForCell(Grid.PosToCell(smi)));
					Option<CavityInfo> cavityInfo = ranchStation.GetCavityInfo();
					if (option.IsNone() || cavityInfo.IsNone())
					{
						smi.StopSM("No longer in any cavity.");
					}
					else if (option.Unwrap() != cavityInfo.Unwrap())
					{
						smi.StopSM("Critter is in a different cavity");
					}
				}
			})
			.EventHandler(GameHashes.RancherReadyAtRanchStation, delegate(Instance smi)
			{
				smi.UpdateWaitingState();
			})
			.Exit(ClearLayerOverride);
		CheerStates cheer = ranch.Cheer;
		string text = CREATURES.STATUSITEMS.EXCITED_TO_GET_RANCHED.NAME;
		string tooltip = CREATURES.STATUSITEMS.EXCITED_TO_GET_RANCHED.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		cheer.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main).Enter("FaceRancher", delegate(Instance smi)
		{
			smi.GetComponent<Facing>().Face(smi.GetRanchStation().transform.GetPosition());
		}).PlayAnim("excited_loop")
			.OnAnimQueueComplete(ranch.Cheer.Pst)
			.ScheduleGoTo((Instance smi) => smi.cheerAnimLength, ranch.Move);
		ranch.Cheer.Pst.ScheduleGoTo(0.2f, ranch.Move);
		State state = ranch.Move.DefaultState(ranch.Move.MoveToRanch).Enter("Speedup", delegate(Instance smi)
		{
			smi.GetComponent<Navigator>().defaultSpeed = smi.OriginalSpeed * 1.25f;
		});
		string text2 = CREATURES.STATUSITEMS.EXCITED_TO_GET_RANCHED.NAME;
		string tooltip2 = CREATURES.STATUSITEMS.EXCITED_TO_GET_RANCHED.TOOLTIP;
		main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text2, tooltip2, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main).Exit("RestoreSpeed", delegate(Instance smi)
		{
			smi.GetComponent<Navigator>().defaultSpeed = smi.OriginalSpeed;
		});
		ranch.Move.MoveToRanch.EnterTransition(ranch.Wait.WaitInLine, GameStateMachine<RanchedStates, Instance, IStateMachineTarget, Def>.Not(IsCrittersTurn)).MoveTo(GetRanchNavTarget, ranch.Wait.WaitInLine).Target(ranchTarget)
			.EventTransition(GameHashes.CreatureArrivedAtRanchStation, ranch.Wait.WaitInLine, (Instance smi) => !IsCrittersTurn(smi));
		ranch.Wait.WaitInLine.EnterTransition(ranch.Ranching, IsCrittersTurn).Enter(delegate(Instance smi)
		{
			smi.EnterQueue();
		}).EventTransition(GameHashes.DestinationReached, ranch.Wait.Waiting);
		ranch.Wait.Waiting.Face(ranchTarget).PlayAnim((Instance smi) => smi.def.StartWaitingAnim).QueueAnim((Instance smi) => smi.def.WaitingAnim, loop: true);
		ranch.Wait.DoneWaiting.PlayAnim((Instance smi) => smi.def.EndWaitingAnim).OnAnimQueueComplete(ranch.Move.MoveToRanch);
		ranch.Ranching.Enter(GetOnTable).Enter("SetCreatureAtRanchingStation", delegate(Instance smi)
		{
			RanchStation.Instance ranchStation = smi.GetRanchStation();
			ranchStation.MessageCreatureArrived(smi);
			smi.AnimController.SetSceneLayer(Grid.SceneLayer.BuildingUse);
		}).EventTransition(GameHashes.RanchingComplete, ranch.Wavegoodbye)
			.ToggleMainStatusItem(delegate(Instance smi)
			{
				RanchStation.Instance ranchStation = GetRanchStation(smi);
				return (ranchStation != null) ? ranchStation.def.CreatureRanchingStatusItem : Db.Get().CreatureStatusItems.GettingRanched;
			});
		State state2 = ranch.Wavegoodbye.Enter(ClearLayerOverride).OnAnimQueueComplete(ranch.Runaway);
		string text3 = CREATURES.STATUSITEMS.EXCITED_TO_BE_RANCHED.NAME;
		string tooltip3 = CREATURES.STATUSITEMS.EXCITED_TO_BE_RANCHED.TOOLTIP;
		main = Db.Get().StatusItemCategories.Main;
		state2.ToggleStatusItem(text3, tooltip3, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
		State state3 = ranch.Runaway.MoveTo(GetRunawayCell);
		string text4 = CREATURES.STATUSITEMS.IDLE.NAME;
		string tooltip4 = CREATURES.STATUSITEMS.IDLE.TOOLTIP;
		main = Db.Get().StatusItemCategories.Main;
		state3.ToggleStatusItem(text4, tooltip4, "", StatusItem.IconType.Info, NotificationType.Neutral, allow_multiples: false, default(HashedString), 129022, null, null, main);
	}

	private static void ClearLayerOverride(Instance smi)
	{
		smi.AnimController.SetSceneLayer(Grid.SceneLayer.Creatures);
	}

	private static RanchStation.Instance GetRanchStation(Instance smi)
	{
		return smi.GetRanchStation();
	}

	private static void GetOnTable(Instance smi)
	{
		Navigator navigator = smi.Get<Navigator>();
		if (navigator.IsValidNavType(NavType.Floor))
		{
			navigator.SetCurrentNavType(NavType.Floor);
		}
		smi.Get<Facing>().SetFacing(mirror_x: false);
	}

	private static bool IsCrittersTurn(Instance smi)
	{
		RanchStation.Instance ranchStation = GetRanchStation(smi);
		if (ranchStation == null)
		{
			return false;
		}
		return ranchStation.IsRancherReady && ranchStation.TryGetRanched(smi);
	}

	private static int GetRanchNavTarget(Instance smi)
	{
		RanchStation.Instance ranchStation = GetRanchStation(smi);
		int num = smi.ModifyNavTargetForCritter(ranchStation.GetRanchNavTarget());
		if (smi.HasTag(GameTags.LargeCreature))
		{
			if (smi.HasTag(GameTags.Creatures.Swimmer))
			{
				num = Grid.CellLeft(num);
			}
			else
			{
				Vector2I vector2I = Grid.PosToXY(smi.gameObject.transform.position);
				Vector2I vector2I2 = Grid.CellToXY(num);
				if (vector2I.x > vector2I2.x)
				{
					num = Grid.CellLeft(num);
				}
			}
		}
		return num;
	}

	private static int GetRunawayCell(Instance smi)
	{
		int cell = Grid.PosToCell(smi.transform.GetPosition());
		int num = Grid.OffsetCell(cell, 2, 0);
		if (Grid.Solid[num])
		{
			num = Grid.OffsetCell(cell, -2, 0);
		}
		return num;
	}
}
