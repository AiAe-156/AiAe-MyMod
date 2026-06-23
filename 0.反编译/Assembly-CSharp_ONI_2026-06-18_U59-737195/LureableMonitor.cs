using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class LureableMonitor : GameStateMachine<LureableMonitor, LureableMonitor.Instance, IStateMachineTarget, LureableMonitor.Def>
{
	public class Def : BaseDef, IGameObjectEffectDescriptor
	{
		public float cooldown = 20f;

		public Tag[] lures;

		public List<Descriptor> GetDescriptors(GameObject go)
		{
			List<Descriptor> list = new List<Descriptor>();
			Tag[] array = lures;
			foreach (Tag tag in array)
			{
				if (tag == GameTags.Creatures.FlyersLure)
				{
					list.Add(new Descriptor(UI.BUILDINGEFFECTS.CAPTURE_METHOD_FLYING_TRAP, UI.BUILDINGEFFECTS.TOOLTIPS.CAPTURE_METHOD_FLYING_TRAP));
				}
				else if (tag == GameTags.Creatures.FishTrapLure)
				{
					list.Add(new Descriptor(UI.BUILDINGEFFECTS.CAPTURE_METHOD_FISH_TRAP, UI.BUILDINGEFFECTS.TOOLTIPS.CAPTURE_METHOD_FISH_TRAP));
				}
			}
			return list;
		}
	}

	public new class Instance : GameInstance
	{
		private struct FindLureCounterContext
		{
			public Instance inst;

			public int cost;

			public GameObject result;
		}

		[MyCmpReq]
		private Navigator navigator;

		private static GameScenePartitioner.VisitorRef<FindLureCounterContext> _findLureCounterVisitor = FindLureCounter;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
		}

		private static Util.IterationInstruction FindLureCounter(object obj, ref FindLureCounterContext context)
		{
			if (!(obj is Lure.Instance instance) || !instance.IsActive() || !instance.HasAnyLure(context.inst.def.lures))
			{
				return Util.IterationInstruction.Continue;
			}
			int navigationCost = context.inst.navigator.GetNavigationCost(Grid.PosToCell(instance.transform.GetPosition()), instance.LurePoints);
			if (navigationCost != -1 && (context.cost == -1 || navigationCost < context.cost))
			{
				context.cost = navigationCost;
				context.result = instance.gameObject;
			}
			return Util.IterationInstruction.Continue;
		}

		public void FindLure()
		{
			FindLureCounterContext context = new FindLureCounterContext
			{
				inst = this,
				cost = -1,
				result = null
			};
			Grid.CellToXY(Grid.PosToCell(base.smi.transform.GetPosition()), out var x, out var y);
			GameScenePartitioner.Instance.ReadonlyVisitEntries(x - 1, y - 1, 2, 2, GameScenePartitioner.Instance.lure, _findLureCounterVisitor, ref context);
			base.sm.targetLure.Set(context.result, this);
		}

		public bool HasLure()
		{
			return base.sm.targetLure.Get(this) != null;
		}

		public GameObject GetTargetLure()
		{
			return base.sm.targetLure.Get(this);
		}
	}

	public TargetParameter targetLure;

	public State nolure;

	public State haslure;

	public State cooldown;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = cooldown;
		cooldown.ScheduleGoTo((Instance smi) => smi.def.cooldown, nolure);
		nolure.PreBrainUpdate(delegate(Instance smi)
		{
			smi.FindLure();
		}).ParamTransition(targetLure, haslure, (Instance smi, GameObject p) => p != null);
		haslure.ParamTransition(targetLure, nolure, (Instance smi, GameObject p) => p == null).PreBrainUpdate(delegate(Instance smi)
		{
			smi.FindLure();
		}).ToggleBehaviour(GameTags.Creatures.MoveToLure, (Instance smi) => smi.HasLure(), delegate(Instance smi)
		{
			smi.GoTo(cooldown);
		});
	}
}
