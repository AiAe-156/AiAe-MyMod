using STRINGS;
using UnityEngine;

public class FlopStates : GameStateMachine<FlopStates, FlopStates.Instance, IStateMachineTarget, FlopStates.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		public float currentDir = 1f;

		public Instance(Chore<Instance> chore, Def def)
			: base((IStateMachineTarget)chore, def)
		{
			chore.AddPrecondition(ChorePreconditions.instance.CheckBehaviourPrecondition, GameTags.Creatures.Flopping);
		}
	}

	private State flop_pre;

	private State flop_cycle;

	private State pst;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = flop_pre;
		State state = root;
		string text = CREATURES.STATUSITEMS.FLOPPING.NAME;
		string tooltip = CREATURES.STATUSITEMS.FLOPPING.TOOLTIP;
		StatusItemCategory main = Db.Get().StatusItemCategories.Main;
		state.ToggleStatusItem(text, tooltip, "", StatusItem.IconType.Exclamation, NotificationType.Bad, allow_multiples: false, default(HashedString), 129022, null, null, main);
		flop_pre.Enter(ChooseDirection).Transition(flop_cycle, ShouldFlop).Transition(pst, GameStateMachine<FlopStates, Instance, IStateMachineTarget, Def>.Not(ShouldFlop));
		flop_cycle.PlayAnim("flop_loop", KAnim.PlayMode.Once).Transition(pst, IsSubstantialLiquid).Update("Flop", FlopForward, UpdateRate.SIM_33ms)
			.OnAnimQueueComplete(flop_pre);
		pst.QueueAnim("flop_loop", loop: true).BehaviourComplete(GameTags.Creatures.Flopping);
	}

	public static bool ShouldFlop(Instance smi)
	{
		int cell = Grid.PosToCell(smi.transform.GetPosition());
		int num = Grid.CellBelow(cell);
		return Grid.IsValidCell(num) && Grid.Solid[num];
	}

	public static void ChooseDirection(Instance smi)
	{
		int cell = Grid.PosToCell(smi.transform.GetPosition());
		if (SearchForLiquid(cell, 1))
		{
			smi.currentDir = 1f;
		}
		else if (SearchForLiquid(cell, -1))
		{
			smi.currentDir = -1f;
		}
		else if (Random.value > 0.5f)
		{
			smi.currentDir = 1f;
		}
		else
		{
			smi.currentDir = -1f;
		}
	}

	private static bool SearchForLiquid(int cell, int delta_x)
	{
		while (true)
		{
			if (!Grid.IsValidCell(cell))
			{
				return false;
			}
			if (Grid.IsSubstantialLiquid(cell))
			{
				return true;
			}
			if (Grid.Solid[cell])
			{
				return false;
			}
			if (Grid.CritterImpassable[cell])
			{
				break;
			}
			int num = Grid.CellBelow(cell);
			cell = ((!Grid.IsValidCell(num) || !Grid.Solid[num]) ? num : (cell + delta_x));
		}
		return false;
	}

	public static void FlopForward(Instance smi, float dt)
	{
		if (smi.HasTag(GameTags.Creatures.StunnedForCapture))
		{
			return;
		}
		KBatchedAnimController component = smi.GetComponent<KBatchedAnimController>();
		int currentFrame = component.currentFrame;
		if (!component.IsVisible() || (currentFrame >= 23 && currentFrame <= 36))
		{
			Vector3 position = smi.transform.GetPosition();
			Vector3 vector = position;
			vector.x = position.x + smi.currentDir * dt * 1f;
			int new_cell = Grid.PosToCell(vector);
			if (CanFlopForward(smi, new_cell))
			{
				smi.transform.SetPosition(vector);
			}
			else
			{
				smi.currentDir = 0f - smi.currentDir;
			}
		}
	}

	private static bool CanFlopForward(Instance smi, int new_cell)
	{
		if (!Grid.IsValidCell(new_cell) || Grid.Solid[new_cell] || Grid.CritterImpassable[new_cell])
		{
			return false;
		}
		OccupyArea component = smi.GetComponent<OccupyArea>();
		CellOffset[] occupiedCellsOffsets = component.OccupiedCellsOffsets;
		for (int i = 0; i < occupiedCellsOffsets.Length; i++)
		{
			int num = Grid.OffsetCell(new_cell, occupiedCellsOffsets[i]);
			if (!Grid.IsValidCell(num) || Grid.Solid[num] || Grid.CritterImpassable[num])
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsSubstantialLiquid(Instance smi)
	{
		return Grid.IsSubstantialLiquid(Grid.PosToCell(smi.transform.GetPosition()));
	}
}
