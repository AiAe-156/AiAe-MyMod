using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class PollinationMonitor : GameStateMachine<PollinationMonitor, PollinationMonitor.StatesInstance, IStateMachineTarget, PollinationMonitor.Def>
{
	public class Def : BaseDef, IGameObjectEffectDescriptor
	{
		public List<Descriptor> GetDescriptors(GameObject go)
		{
			return new List<Descriptor>
			{
				new Descriptor(UI.GAMEOBJECTEFFECTS.REQUIRES_POLLINATION, UI.GAMEOBJECTEFFECTS.TOOLTIPS.REQUIRES_POLLINATION, Descriptor.DescriptorType.Requirement)
			};
		}
	}

	public class StatesInstance : GameInstance, IWiltCause
	{
		public Effects effects;

		public WiltCondition.Condition[] Conditions => new WiltCondition.Condition[1] { WiltCondition.Condition.Pollination };

		public string WiltStateString
		{
			get
			{
				if (!IsInsideState(base.sm.not_pollinated))
				{
					return "";
				}
				return Db.Get().CreatureStatusItems.NotPollinated.GetName(this);
			}
		}

		public StatesInstance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			effects = GetComponent<Effects>();
			Subscribe(1119167081, delegate
			{
				base.sm.spawn_pollinated.Set(value: true, this);
			});
		}

		public override void StartSM()
		{
			base.StartSM();
			if (base.sm.spawn_pollinated.Get(this))
			{
				base.sm.spawn_pollinated.Set(value: false, this);
				if (effects != null)
				{
					effects.Add(INITIALLY_POLLINATED_EFFECT, should_save: true).timeRemaining *= UnityEngine.Random.Range(0.75f, 1f);
				}
			}
		}
	}

	public static readonly string INITIALLY_POLLINATED_EFFECT = "InitiallyPollinated";

	public static readonly HashedString[] PollinationEffects = new HashedString[4] { INITIALLY_POLLINATED_EFFECT, "DivergentCropTended", "DivergentCropTendedWorm", "ButterflyPollinated" };

	public State initialize;

	public State not_pollinated;

	public State pollinated;

	private readonly BoolParameter spawn_pollinated = new BoolParameter(default_value: false);

	public static bool IsPollinationEffect(Effect effect)
	{
		return Array.IndexOf(PollinationEffects, effect.IdHash) != -1;
	}

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = initialize;
		initialize.Enter(delegate(StatesInstance smi)
		{
			if (smi.effects == null)
			{
				smi.GoTo(not_pollinated);
			}
			else
			{
				bool flag = false;
				HashedString[] pollinationEffects = PollinationEffects;
				foreach (HashedString effect_id in pollinationEffects)
				{
					if (smi.effects.HasEffect(effect_id))
					{
						flag = true;
						break;
					}
				}
				smi.GoTo(flag ? pollinated : not_pollinated);
			}
		});
		not_pollinated.Enter(delegate(StatesInstance smi)
		{
			smi.BoxingTrigger(-200207042, data: false);
		}).EventHandler(GameHashes.EffectAdded, delegate(StatesInstance smi, object data)
		{
			if (IsPollinationEffect(data as Effect))
			{
				smi.GoTo(pollinated);
			}
		});
		pollinated.Enter(delegate(StatesInstance smi)
		{
			smi.BoxingTrigger(-200207042, data: true);
		}).EventHandler(GameHashes.EffectRemoved, delegate(StatesInstance smi, object data)
		{
			if (IsPollinationEffect(data as Effect))
			{
				if (smi.effects == null)
				{
					smi.GoTo(not_pollinated);
				}
				else
				{
					bool flag = false;
					HashedString[] pollinationEffects = PollinationEffects;
					foreach (HashedString effect_id in pollinationEffects)
					{
						if (smi.effects.HasEffect(effect_id))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						smi.GoTo(not_pollinated);
					}
				}
			}
		});
	}
}
