using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class FertilityShearable : GameStateMachine<FertilityShearable, FertilityShearable.Instance, IStateMachineTarget, FertilityShearable.Def>
{
	public class Def : BaseDef, IGameObjectEffectDescriptor
	{
		private const float DEFAULT_MINIMUM_FERTILITY = 50f;

		private const float DEFAULT_PERCENT_FERTILITY_CONSUMED_PER_MILKING = 1f;

		public SimHashes milkElement;

		public float dropMass;

		public float minimumFertility = 50f;

		public float percentFertilityConsumedPerMilking = 1f;

		public bool requiresHappy;

		public bool suppressedByElderly;

		public override void Configure(GameObject prefab)
		{
		}

		public List<Descriptor> GetDescriptors(GameObject obj)
		{
			string newValue = ElementLoader.FindElementByHash(milkElement).tag.ProperName();
			string formattedMass = GameUtil.GetFormattedMass(dropMass);
			string txt = GlobalStringBuilderPool.ReturnAndFree(GlobalStringBuilderPool.Alloc().Append(UI.BUILDINGEFFECTS.SCALE_GROWTH_FERTILITY).Replace("{Item}", newValue)
				.Replace("{Amount}", formattedMass)
				.Replace("{Percent}", GameUtil.GetFormattedPercent(minimumFertility)));
			string tooltip = GlobalStringBuilderPool.ReturnAndFree(GlobalStringBuilderPool.Alloc().Append(UI.BUILDINGEFFECTS.TOOLTIPS.SCALE_GROWTH_FERTILE).Replace("{Item}", newValue)
				.Replace("{Amount}", formattedMass));
			return new List<Descriptor>
			{
				new Descriptor(txt, tooltip)
			};
		}
	}

	public new class Instance : GameInstance, IMilkable
	{
		[MyCmpGet]
		private Effects effects;

		[MyCmpGet]
		public KBatchedAnimController animController;

		[MyCmpReq]
		private KPrefabID prefabId;

		[MyCmpReq]
		public KSelectable selectable;

		public AmountInstance fertility;

		public Guid milkReadyStatusGuid;

		private WildnessMonitor.Instance wildnessMonitor;

		private AgeMonitor.Instance ageMonitor;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			fertility = Db.Get().Amounts.Fertility.Lookup(base.gameObject);
		}

		public override void StartSM()
		{
			base.StartSM();
			wildnessMonitor = base.gameObject.GetSMI<WildnessMonitor.Instance>();
			ageMonitor = base.gameObject.GetSMI<AgeMonitor.Instance>();
		}

		public bool IsReadyToBeMilked()
		{
			if (fertility.value < base.def.minimumFertility)
			{
				return false;
			}
			if (wildnessMonitor != null && wildnessMonitor.IsWild())
			{
				return false;
			}
			if (base.def.requiresHappy && !prefabId.HasTag(GameTags.Creatures.Happy))
			{
				return false;
			}
			if (base.def.suppressedByElderly && ageMonitor != null && ageMonitor.IsElderly)
			{
				return false;
			}
			return true;
		}

		public SimHashes GetMilkElement()
		{
			return base.def.milkElement;
		}

		public void MilkingComplete(Storage storage)
		{
			storage.GetComponent<Storage>().AddLiquid(base.def.milkElement, base.def.dropMass, 310.15f, byte.MaxValue, 0);
			float b = fertility.value - fertility.GetMax() * base.def.percentFertilityConsumedPerMilking;
			fertility.value = Mathf.Max(0f, b);
		}
	}

	public State hasMilk;

	public State noMilk;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = noMilk;
		noMilk.Transition(hasMilk, (Instance smi) => IsFertileEnoughToHarvest(smi)).Enter(delegate(Instance smi)
		{
			ShowBellySymbol(smi, show: false);
		});
		hasMilk.Transition(noMilk, (Instance smi) => !IsFertileEnoughToHarvest(smi)).Enter(delegate(Instance smi)
		{
			ShowBellySymbol(smi, show: true);
		}).Update(delegate(Instance smi, float dt)
		{
			smi.milkReadyStatusGuid = smi.selectable.ToggleStatusItem(Db.Get().CreatureStatusItems.FishFullMilk, smi.milkReadyStatusGuid, smi.IsReadyToBeMilked(), smi);
		}, UpdateRate.SIM_1000ms)
			.Exit(delegate(Instance smi)
			{
				smi.milkReadyStatusGuid = smi.selectable.RemoveStatusItem(smi.milkReadyStatusGuid);
			});
	}

	private static bool IsFertileEnoughToHarvest(Instance smi)
	{
		return smi.fertility.value >= smi.def.minimumFertility;
	}

	private static void ShowBellySymbol(Instance smi, bool show)
	{
		if (show)
		{
			AddBellyOverride(smi);
		}
		else
		{
			RemoveBellyOverride(smi);
		}
	}

	private static void AddBellyOverride(Instance smi)
	{
		SymbolOverrideController component = smi.GetComponent<SymbolOverrideController>();
		KBatchedAnimController component2 = smi.GetComponent<KBatchedAnimController>();
		KAnim.Build.Symbol symbol = component2.AnimFiles[0].GetData().build.GetSymbol("belly_full");
		if (symbol != null)
		{
			component.AddSymbolOverride("belly", symbol, 1);
		}
	}

	private static void RemoveBellyOverride(Instance smi)
	{
		SymbolOverrideController component = smi.GetComponent<SymbolOverrideController>();
		component.TryRemoveSymbolOverride("belly", 1);
	}
}
