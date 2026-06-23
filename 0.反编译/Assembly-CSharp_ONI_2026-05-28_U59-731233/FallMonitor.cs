using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FallMonitor : GameStateMachine<FallMonitor, FallMonitor.Instance>
{
	public class EntombedStates : State
	{
		public State recovering;

		public State stuck;
	}

	public new class Instance : GameInstance
	{
		private CellOffset[] entombedEscapeOffsets = new CellOffset[7]
		{
			new CellOffset(0, 1),
			new CellOffset(1, 0),
			new CellOffset(-1, 0),
			new CellOffset(1, 1),
			new CellOffset(-1, 1),
			new CellOffset(1, -1),
			new CellOffset(-1, -1)
		};

		private Navigator navigator;

		private bool shouldPlayEmotes;

		public string entombedAnimOverride;

		private List<int> safeCells = new List<int>();

		private int MAX_CELLS_TRACKED = 3;

		private static Action<object, object> OnDestinationReachedDispatcher = delegate(object context, object data)
		{
			Unsafe.As<Instance>(context).OnDestinationReached(data);
		};

		private static Action<object, object> OnMovementStateChangedDispatcher = delegate(object context, object data)
		{
			Unsafe.As<Instance>(context).OnMovementStateChanged(data);
		};

		private static Action<object, object> OnCellChangedDispatcher = delegate(object context, object data)
		{
			Unsafe.As<Instance>(context).OnCellChanged(data);
		};

		private bool flipRecoverEmote = false;

		public Instance(IStateMachineTarget master, bool shouldPlayEmotes, string entombedAnimOverride = null)
			: base(master)
		{
			navigator = GetComponent<Navigator>();
			this.shouldPlayEmotes = shouldPlayEmotes;
			this.entombedAnimOverride = entombedAnimOverride;
			Pathfinding.Instance.FlushNavGridsOnLoad();
			Subscribe(915392638, OnCellChangedDispatcher, this);
			Subscribe(1027377649, OnMovementStateChangedDispatcher, this);
			Subscribe(387220196, OnDestinationReachedDispatcher, this);
		}

		private void OnDestinationReached(object data)
		{
			int item = Grid.PosToCell(base.transform.GetPosition());
			if (!safeCells.Contains(item))
			{
				safeCells.Add(item);
				if (safeCells.Count > MAX_CELLS_TRACKED)
				{
					safeCells.RemoveAt(0);
				}
			}
		}

		private void OnMovementStateChanged(object data)
		{
			GameHashes value = ((Boxed<GameHashes>)data).value;
			if (value != GameHashes.ObjectMovementWakeUp)
			{
				return;
			}
			int item = Grid.PosToCell(base.transform.GetPosition());
			if (!safeCells.Contains(item))
			{
				safeCells.Add(item);
				if (safeCells.Count > MAX_CELLS_TRACKED)
				{
					safeCells.RemoveAt(0);
				}
			}
		}

		private void OnCellChanged(object data)
		{
			int value = ((Boxed<int>)data).value;
			if (!safeCells.Contains(value))
			{
				safeCells.Add(value);
				if (safeCells.Count > MAX_CELLS_TRACKED)
				{
					safeCells.RemoveAt(0);
				}
			}
		}

		public void Recover()
		{
			int cell = Grid.PosToCell(navigator);
			NavGrid.Transition[] transitions = navigator.NavGrid.transitions;
			for (int i = 0; i < transitions.Length; i++)
			{
				NavGrid.Transition transition = transitions[i];
				if (transition.isEscape && navigator.CurrentNavType == transition.start)
				{
					int num = transition.IsValid(cell, navigator.NavGrid.NavTable);
					if (Grid.InvalidCell != num)
					{
						Vector2I vector2I = Grid.CellToXY(cell);
						flipRecoverEmote = Grid.CellToXY(num).x < vector2I.x;
						navigator.BeginTransition(transition);
						break;
					}
				}
			}
		}

		public void RecoverEmote()
		{
			if (shouldPlayEmotes)
			{
				int num = UnityEngine.Random.Range(0, 9);
				if (num == 8)
				{
					ChoreProvider component = base.master.GetComponent<ChoreProvider>();
					new EmoteChore(component, Db.Get().ChoreTypes.EmoteHighPriority, Db.Get().Emotes.Minion.CloseCall_Fall, KAnim.PlayMode.Once, 1, flipRecoverEmote);
				}
			}
		}

		public void LandFloor()
		{
			navigator.SetCurrentNavType(NavType.Floor);
			GetComponent<Transform>().SetPosition(Grid.CellToPosCBC(Grid.PosToCell(GetComponent<Transform>().GetPosition()), Grid.SceneLayer.Move));
		}

		public void AttemptInitialRecovery()
		{
			if (IsIncapacitated())
			{
				return;
			}
			int cell = Grid.PosToCell(navigator);
			NavGrid.Transition[] transitions = navigator.NavGrid.transitions;
			for (int i = 0; i < transitions.Length; i++)
			{
				NavGrid.Transition transition = transitions[i];
				if (transition.isEscape && navigator.CurrentNavType == transition.start)
				{
					int num = transition.IsValid(cell, navigator.NavGrid.NavTable);
					if (Grid.InvalidCell != num)
					{
						base.smi.GoTo(base.smi.sm.recoverinitialfall);
						break;
					}
				}
			}
		}

		public bool CanRecoverToLadder()
		{
			int cell = Grid.PosToCell(base.master.transform.GetPosition());
			return navigator.NavGrid.NavTable.IsValid(cell, NavType.Ladder) && !IsIncapacitated();
		}

		public void MountLadder()
		{
			navigator.SetCurrentNavType(NavType.Ladder);
			GetComponent<Transform>().SetPosition(Grid.CellToPosCBC(Grid.PosToCell(GetComponent<Transform>().GetPosition()), Grid.SceneLayer.Move));
		}

		public bool CanRecoverToPole()
		{
			int cell = Grid.PosToCell(base.master.transform.GetPosition());
			return navigator.NavGrid.NavTable.IsValid(cell, NavType.Pole) && !IsIncapacitated();
		}

		public void MountPole()
		{
			navigator.SetCurrentNavType(NavType.Pole);
			GetComponent<Transform>().SetPosition(Grid.CellToPosCBC(Grid.PosToCell(GetComponent<Transform>().GetPosition()), Grid.SceneLayer.Move));
		}

		public bool CanRecoverToSwim()
		{
			int cell = Grid.PosToCell(base.master.transform.GetPosition());
			SwimMonitor.Instance sMI = navigator.GetSMI<SwimMonitor.Instance>();
			if (sMI != null)
			{
				return sMI.CanSwim() && !IsIncapacitated() && navigator.NavGrid.NavTable.IsValid(cell, NavType.Swim);
			}
			return false;
		}

		public void Swim()
		{
			navigator.SetCurrentNavType(NavType.Swim);
			GetComponent<Transform>().SetPosition(Grid.CellToPosCBC(Grid.PosToCell(GetComponent<Transform>().GetPosition()), Grid.SceneLayer.Move));
		}

		private bool IsIncapacitated()
		{
			return base.gameObject.HasTag(GameTags.Incapacitated) || base.gameObject.HasTag(GameTags.SuffocatingIncapacitated);
		}

		public void UpdateFalling()
		{
			bool value = false;
			bool flag = false;
			if (!navigator.IsMoving() && navigator.CurrentNavType != NavType.Tube)
			{
				int num = Grid.PosToCell(base.transform.GetPosition());
				int num2 = Grid.CellAbove(num);
				bool flag2 = Grid.IsValidCell(num);
				bool flag3 = Grid.IsValidCell(num2);
				bool flag4 = IsValidNavCell(num) && (!IsIncapacitated() || (navigator.CurrentNavType != NavType.Ladder && navigator.CurrentNavType != NavType.Pole && navigator.CurrentNavType != NavType.Swim));
				flag = (!flag4 && flag2 && Grid.Solid[num] && !Grid.DupePassable[num]) || (flag3 && Grid.Solid[num2] && !Grid.DupePassable[num2]) || (flag2 && Grid.DupeImpassable[num]) || (flag3 && Grid.DupeImpassable[num2]);
				value = !flag4 && !flag;
				if ((!flag2 && flag3) || (flag3 && Grid.WorldIdx[num] != Grid.WorldIdx[num2] && Grid.IsWorldValidCell(num2)))
				{
					TeleportInWorld(num);
				}
			}
			base.sm.isFalling.Set(value, base.smi);
			base.sm.isEntombed.Set(flag, base.smi);
		}

		private void TeleportInWorld(int cell)
		{
			int num = Grid.CellAbove(cell);
			WorldContainer world = ClusterManager.Instance.GetWorld(Grid.WorldIdx[num]);
			if (world != null)
			{
				int safeCell = world.GetSafeCell();
				Debug.Log($"Teleporting {navigator.name} to {safeCell}");
				MoveToCell(safeCell);
			}
			else
			{
				Debug.LogError($"Unable to teleport {navigator.name} stuck on {cell}");
			}
		}

		private bool IsValidNavCell(int cell)
		{
			return navigator.NavGrid.NavTable.IsValid(cell, navigator.CurrentNavType) && !Grid.DupeImpassable[cell];
		}

		public void TryEntombedEscape()
		{
			int num = Grid.PosToCell(base.transform.GetPosition());
			int backCell = GetComponent<Facing>().GetBackCell();
			int num2 = Grid.CellAbove(backCell);
			int num3 = Grid.CellBelow(backCell);
			int[] array = new int[3] { backCell, num2, num3 };
			foreach (int num4 in array)
			{
				if (IsValidNavCell(num4) && !Grid.HasDoor[num4])
				{
					MoveToCell(num4);
					return;
				}
			}
			int cell = Grid.PosToCell(base.transform.GetPosition());
			CellOffset[] array2 = entombedEscapeOffsets;
			foreach (CellOffset offset in array2)
			{
				if (Grid.IsCellOffsetValid(cell, offset))
				{
					int num5 = Grid.OffsetCell(cell, offset);
					if (IsValidNavCell(num5) && !Grid.HasDoor[num5])
					{
						MoveToCell(num5);
						return;
					}
				}
			}
			for (int num6 = safeCells.Count - 1; num6 >= 0; num6--)
			{
				int num7 = safeCells[num6];
				if (num7 != num && IsValidNavCell(num7) && !Grid.HasDoor[num7])
				{
					MoveToCell(num7);
					return;
				}
			}
			CellOffset[] array3 = entombedEscapeOffsets;
			foreach (CellOffset offset2 in array3)
			{
				if (Grid.IsCellOffsetValid(cell, offset2))
				{
					int num8 = Grid.OffsetCell(cell, offset2);
					int num9 = Grid.CellAbove(num8);
					if (Grid.IsValidCell(num9) && !Grid.Solid[num8] && !Grid.Solid[num9] && !Grid.DupeImpassable[num8] && !Grid.DupeImpassable[num9] && !Grid.HasDoor[num8] && !Grid.HasDoor[num9])
					{
						MoveToCell(num8, forceFloorNav: true);
						return;
					}
				}
			}
			GoTo(base.sm.entombed.stuck);
		}

		private void MoveToCell(int cell, bool forceFloorNav = false)
		{
			base.transform.SetPosition(Grid.CellToPosCBC(cell, Grid.SceneLayer.Move));
			base.transform.GetComponent<Navigator>().Stop();
			if (IsIncapacitated() || forceFloorNav)
			{
				base.transform.GetComponent<Navigator>().SetCurrentNavType(NavType.Floor);
			}
			UpdateFalling();
			if (base.sm.isEntombed.Get(base.smi))
			{
				GoTo(base.sm.entombed.stuck);
			}
			else
			{
				GoTo(base.sm.standing);
			}
		}
	}

	public State standing;

	public State falling_pre;

	public State falling;

	public EntombedStates entombed;

	public State recoverladder;

	public State recoverpole;

	public State recoverswim;

	public State recoverinitialfall;

	public State landfloor;

	public State instorage;

	public BoolParameter isEntombed;

	public BoolParameter isFalling;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = standing;
		root.TagTransition(GameTags.Stored, instorage).Update("CheckLanded", delegate(Instance smi, float dt)
		{
			smi.UpdateFalling();
		}, UpdateRate.SIM_33ms, load_balance: true);
		standing.ParamTransition(isEntombed, entombed, GameStateMachine<FallMonitor, Instance, IStateMachineTarget, object>.IsTrue).ParamTransition(isFalling, falling_pre, GameStateMachine<FallMonitor, Instance, IStateMachineTarget, object>.IsTrue);
		falling_pre.Enter("StopNavigator", delegate(Instance smi)
		{
			smi.GetComponent<Navigator>().Stop();
		}).Enter("AttemptInitialRecovery", delegate(Instance smi)
		{
			smi.AttemptInitialRecovery();
		}).GoTo(falling)
			.ToggleBrain("falling_pre");
		falling.ToggleBrain("falling").PlayAnim("fall_pre").QueueAnim("fall_loop", loop: true)
			.ParamTransition(isEntombed, entombed, GameStateMachine<FallMonitor, Instance, IStateMachineTarget, object>.IsTrue)
			.Transition(recoverladder, (Instance smi) => smi.CanRecoverToLadder(), UpdateRate.SIM_33ms)
			.Transition(recoverpole, (Instance smi) => smi.CanRecoverToPole(), UpdateRate.SIM_33ms)
			.Transition(recoverswim, (Instance smi) => smi.CanRecoverToSwim(), UpdateRate.SIM_33ms)
			.ToggleGravity(landfloor);
		recoverinitialfall.ToggleBrain("recoverinitialfall").Enter("Recover", delegate(Instance smi)
		{
			smi.Recover();
		}).EventTransition(GameHashes.DestinationReached, standing)
			.EventTransition(GameHashes.NavigationFailed, standing)
			.Exit(delegate(Instance smi)
			{
				smi.RecoverEmote();
			});
		landfloor.Enter("Land", delegate(Instance smi)
		{
			smi.LandFloor();
		}).GoTo(standing);
		recoverladder.ToggleBrain("recoverladder").PlayAnim("floor_ladder_0_0").Enter("MountLadder", delegate(Instance smi)
		{
			smi.MountLadder();
		})
			.OnAnimQueueComplete(standing);
		recoverpole.ToggleBrain("recoverpole").PlayAnim("floor_pole_0_0").Enter("MountPole", delegate(Instance smi)
		{
			smi.MountPole();
		})
			.OnAnimQueueComplete(standing);
		recoverswim.ToggleBrain("recoverswim").PlayAnim("treading_loop").Enter("Swim", delegate(Instance smi)
		{
			smi.Swim();
		})
			.OnAnimQueueComplete(standing);
		instorage.TagTransition(GameTags.Stored, standing, on_remove: true);
		entombed.DefaultState(entombed.recovering);
		entombed.recovering.Enter("TryEntombedEscape", delegate(Instance smi)
		{
			smi.TryEntombedEscape();
		});
		entombed.stuck.Enter("StopNavigator", delegate(Instance smi)
		{
			smi.GetComponent<Navigator>().Stop();
		}).ToggleChore((Instance smi) => new EntombedChore(smi.master, smi.entombedAnimOverride), standing).ParamTransition(isEntombed, standing, GameStateMachine<FallMonitor, Instance, IStateMachineTarget, object>.IsFalse);
	}
}
