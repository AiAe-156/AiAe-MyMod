using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class WellFedShearable : GameStateMachine<WellFedShearable, WellFedShearable.Instance, IStateMachineTarget, WellFedShearable.Def>
{
	public class Def : BaseDef, IGameObjectEffectDescriptor
	{
		public string effectId;

		public float caloriesPerCycle;

		public float growthDurationCycles;

		public int levelCount;

		public Tag itemDroppedOnShear;

		public float dropMass;

		public Tag requiredDiet = null;

		public KAnimHashedString[] scaleGrowthSymbols = SCALE_SYMBOL_NAMES;

		public KAnimHashedString[] hideSymbols;

		public static KAnimHashedString[] SCALE_SYMBOL_NAMES = new KAnimHashedString[5] { "scale_0", "scale_1", "scale_2", "scale_3", "scale_4" };

		public override void Configure(GameObject prefab)
		{
			prefab.GetComponent<Modifiers>().initialAmounts.Add(Db.Get().Amounts.ScaleGrowth.Id);
		}

		public List<Descriptor> GetDescriptors(GameObject obj)
		{
			List<Descriptor> list = new List<Descriptor>();
			list.Add(new Descriptor(UI.BUILDINGEFFECTS.SCALE_GROWTH.Replace("{Item}", itemDroppedOnShear.ProperName()).Replace("{Amount}", GameUtil.GetFormattedMass(dropMass)).Replace("{Time}", GameUtil.GetFormattedCycles(growthDurationCycles * 600f)), UI.BUILDINGEFFECTS.TOOLTIPS.SCALE_GROWTH_FED.Replace("{Item}", itemDroppedOnShear.ProperName()).Replace("{Amount}", GameUtil.GetFormattedMass(dropMass)).Replace("{Time}", GameUtil.GetFormattedCycles(growthDurationCycles * 600f))));
			return list;
		}
	}

	public class GrowingState : State
	{
		public State growing;

		public State stalled;
	}

	public new class Instance : GameInstance, IShearable
	{
		[MyCmpGet]
		private Effects effects;

		[MyCmpGet]
		public KBatchedAnimController animController;

		public AmountInstance scaleGrowth;

		public int currentScaleLevel = -1;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			scaleGrowth = Db.Get().Amounts.ScaleGrowth.Lookup(base.gameObject);
			scaleGrowth.value = scaleGrowth.GetMax();
		}

		public bool IsFullyGrown()
		{
			return currentScaleLevel == base.def.levelCount;
		}

		public void OnCaloriesConsumed(object data)
		{
			CreatureCalorieMonitor.CaloriesConsumedEvent caloriesConsumedEvent = Boxed<CreatureCalorieMonitor.CaloriesConsumedEvent>.Unbox(data);
			if (!(base.def.requiredDiet != null) || !(caloriesConsumedEvent.tag != base.def.requiredDiet))
			{
				EffectInstance effectInstance = effects.Get(base.smi.def.effectId);
				if (effectInstance == null)
				{
					effectInstance = effects.Add(base.smi.def.effectId, should_save: true);
				}
				effectInstance.timeRemaining += caloriesConsumedEvent.calories / base.smi.def.caloriesPerCycle * 600f;
			}
		}

		public bool HasWellFedEffect()
		{
			return effects.Get(base.def.effectId) != null;
		}

		public void Shear()
		{
			scaleGrowth.value = 0f;
			UpdateScales(this, 0f);
		}

		public Tuple<Tag, float> GetItemDroppedOnShear()
		{
			return new Tuple<Tag, float>(base.def.itemDroppedOnShear, base.def.dropMass);
		}
	}

	public GrowingState growing;

	public State fullyGrown;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = growing;
		root.Enter(delegate(Instance smi)
		{
			UpdateScales(smi, 0f);
		}).Enter(delegate(Instance smi)
		{
			if (smi.def.hideSymbols != null)
			{
				KAnimHashedString[] hideSymbols = smi.def.hideSymbols;
				foreach (KAnimHashedString symbol in hideSymbols)
				{
					smi.animController.SetSymbolVisiblity(symbol, is_visible: false);
				}
			}
		}).Update(UpdateScales, UpdateRate.SIM_1000ms)
			.EventHandler(GameHashes.CaloriesConsumed, delegate(Instance smi, object data)
			{
				smi.OnCaloriesConsumed(data);
			});
		growing.Enter(delegate(Instance smi)
		{
			UpdateScales(smi, 0f);
		}).DefaultState(growing.stalled).Transition(fullyGrown, AreScalesFullyGrown, UpdateRate.SIM_1000ms);
		growing.stalled.ToggleCritterEmotion(Db.Get().CritterEmotions.WellFed).Transition(growing.growing, (Instance smi) => smi.HasWellFedEffect(), UpdateRate.SIM_1000ms);
		growing.growing.Transition(growing.stalled, GameStateMachine<WellFedShearable, Instance, IStateMachineTarget, Def>.Not((Instance smi) => smi.HasWellFedEffect()), UpdateRate.SIM_1000ms).ToggleCritterEmotion(Db.Get().CritterEmotions.WellFed);
		fullyGrown.Enter(delegate(Instance smi)
		{
			UpdateScales(smi, 0f);
		}).ToggleBehaviour(GameTags.Creatures.ScalesGrown, (Instance smi) => smi.HasTag(GameTags.Creatures.CanMolt)).EventTransition(GameHashes.Molt, growing, GameStateMachine<WellFedShearable, Instance, IStateMachineTarget, Def>.Not(AreScalesFullyGrown))
			.Transition(growing, GameStateMachine<WellFedShearable, Instance, IStateMachineTarget, Def>.Not(AreScalesFullyGrown), UpdateRate.SIM_1000ms);
	}

	private static bool AreScalesFullyGrown(Instance smi)
	{
		return smi.scaleGrowth.value >= smi.scaleGrowth.GetMax();
	}

	private static void UpdateScales(Instance smi, float dt)
	{
		int num = (int)((float)smi.def.levelCount * smi.scaleGrowth.value / 100f);
		if (smi.currentScaleLevel != num)
		{
			for (int i = 0; i < smi.def.scaleGrowthSymbols.Length; i++)
			{
				bool is_visible = i <= num - 1;
				smi.animController.SetSymbolVisiblity(smi.def.scaleGrowthSymbols[i], is_visible);
			}
			smi.currentScaleLevel = num;
		}
	}
}
