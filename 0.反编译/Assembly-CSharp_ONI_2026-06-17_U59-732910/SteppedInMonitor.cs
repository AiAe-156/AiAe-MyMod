using Klei.AI;
using UnityEngine;

public class SteppedInMonitor : GameStateMachine<SteppedInMonitor, SteppedInMonitor.Instance>
{
	public new class Instance : GameInstance
	{
		public Effects effects;

		public string[] effectsAllowed { get; private set; }

		public Instance(IStateMachineTarget master)
			: this(master, new string[3] { "CarpetFeet", "WetFeet", "SoakingWet" })
		{
		}

		public Instance(IStateMachineTarget master, string[] effectsAllowed)
			: base(master)
		{
			effects = GetComponent<Effects>();
			this.effectsAllowed = effectsAllowed;
		}

		public bool IsEffectAllowed(string effectName)
		{
			if (effectsAllowed == null || effectsAllowed.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < effectsAllowed.Length; i++)
			{
				if (effectsAllowed[i] == effectName)
				{
					return true;
				}
			}
			return false;
		}
	}

	public const string CARPET_EFFECT_NAME = "CarpetFeet";

	public const string WET_FEET_EFFECT_NAME = "WetFeet";

	public const string SOAK_EFFECT_NAME = "SoakingWet";

	public State satisfied;

	public State carpetedFloor;

	public State wetFloor;

	public State wetBody;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = satisfied;
		satisfied.Transition(carpetedFloor, IsOnCarpet).Transition(wetFloor, IsFloorWet).Transition(wetBody, IsSubmerged);
		carpetedFloor.Enter(GetCarpetFeet).ToggleExpression(Db.Get().Expressions.Tickled).Update(GetCarpetFeet, UpdateRate.SIM_1000ms)
			.Transition(satisfied, GameStateMachine<SteppedInMonitor, Instance, IStateMachineTarget, object>.Not(IsOnCarpet))
			.Transition(wetFloor, IsFloorWet)
			.Transition(wetBody, IsSubmerged);
		wetFloor.Enter(GetWetFeet).Update(GetWetFeet, UpdateRate.SIM_1000ms).Transition(satisfied, GameStateMachine<SteppedInMonitor, Instance, IStateMachineTarget, object>.Not(IsFloorWet))
			.Transition(wetBody, IsSubmerged);
		wetBody.Enter(GetSoaked).Update(GetSoaked, UpdateRate.SIM_1000ms).Transition(wetFloor, GameStateMachine<SteppedInMonitor, Instance, IStateMachineTarget, object>.Not(IsSubmerged));
	}

	private static void GetCarpetFeet(Instance smi, float dt)
	{
		GetCarpetFeet(smi);
	}

	private static void GetCarpetFeet(Instance smi)
	{
		if (!smi.effects.HasEffect("SoakingWet") && !smi.effects.HasEffect("WetFeet") && smi.IsEffectAllowed("CarpetFeet"))
		{
			smi.effects.Add("CarpetFeet", should_save: true);
		}
	}

	private static void GetWetFeet(Instance smi, float dt)
	{
		GetWetFeet(smi);
	}

	private static void GetWetFeet(Instance smi)
	{
		if (!smi.effects.HasEffect("SoakingWet") && smi.IsEffectAllowed("WetFeet"))
		{
			smi.effects.Add("WetFeet", should_save: true);
		}
	}

	private static void GetSoaked(Instance smi, float dt)
	{
		GetSoaked(smi);
	}

	private static void GetSoaked(Instance smi)
	{
		if (smi.effects.HasEffect("WetFeet"))
		{
			smi.effects.Remove("WetFeet");
		}
		if (smi.IsEffectAllowed("SoakingWet"))
		{
			smi.effects.Add("SoakingWet", should_save: true);
		}
	}

	private static bool IsOnCarpet(Instance smi)
	{
		int cell = Grid.CellBelow(Grid.PosToCell(smi));
		if (!Grid.IsValidCell(cell))
		{
			return false;
		}
		GameObject gameObject = Grid.Objects[cell, 9];
		if (Grid.IsValidCell(cell) && gameObject != null)
		{
			return gameObject.HasTag(GameTags.Carpeted);
		}
		return false;
	}

	private static bool IsFloorWet(Instance smi)
	{
		int num = Grid.PosToCell(smi);
		if (Grid.IsValidCell(num))
		{
			return Grid.Element[num].IsLiquid;
		}
		return false;
	}

	private static bool IsSubmerged(Instance smi)
	{
		int num = Grid.CellAbove(Grid.PosToCell(smi));
		if (Grid.IsValidCell(num))
		{
			return Grid.Element[num].IsLiquid;
		}
		return false;
	}
}
