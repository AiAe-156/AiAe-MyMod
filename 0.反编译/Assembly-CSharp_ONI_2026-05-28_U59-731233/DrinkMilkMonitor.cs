using System.Collections.Generic;
using Klei.AI;

public class DrinkMilkMonitor : GameStateMachine<DrinkMilkMonitor, DrinkMilkMonitor.Instance, IStateMachineTarget, DrinkMilkMonitor.Def>
{
	public class Def : BaseDef
	{
		public bool consumesMilk = true;

		public DrinkMilkStates.Def.DrinkCellOffsetGetFn drinkCellOffsetGetFn;
	}

	public new class Instance : GameInstance
	{
		public bool isAquaticCreature = false;

		public MilkFeeder.Instance targetMilkFeeder;

		public bool doesTargetMilkFeederHaveSpaceForCritter;

		public Tag lastConsumedElementTag = Tag.Invalid;

		[MyCmpReq]
		public Navigator navigator;

		[MyCmpGet]
		public DrowningMonitor drowningMonitor;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			isAquaticCreature = HasTag(GameTags.Creatures.Swimmer);
		}

		public void ApplyDrinkEffect()
		{
			if (!base.def.consumesMilk)
			{
				return;
			}
			string text = null;
			for (int i = 0; i < MilkFeederConfig.EffectsPerDrinkableLiquid.Length; i++)
			{
				Tag first = MilkFeederConfig.EffectsPerDrinkableLiquid[i].first;
				string second = MilkFeederConfig.EffectsPerDrinkableLiquid[i].second;
				if (lastConsumedElementTag == first)
				{
					text = second;
					break;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				Effects component = GetComponent<Effects>();
				component.Add(text, should_save: true);
			}
		}

		public void NotifyFinishedDrinkingMilkFrom(MilkFeeder.Instance milkFeeder)
		{
			Tag tag = Tag.Invalid;
			lastConsumedElementTag = Tag.Invalid;
			if (milkFeeder != null && base.def.consumesMilk)
			{
				tag = milkFeeder.ConsumeMilkForOneFeeding();
			}
			lastConsumedElementTag = tag;
			base.sm.didFinishDrinkingMilk.Trigger(base.smi);
		}

		public int GetDrinkCellOf(MilkFeeder.Instance milkFeeder, bool isTwoByTwoCritterCramped)
		{
			return Grid.OffsetCell(Grid.PosToCell(milkFeeder), base.def.drinkCellOffsetGetFn(milkFeeder, this, isTwoByTwoCritterCramped));
		}
	}

	public State lookingToDrinkMilk;

	public State applyEffect;

	public State satisfied;

	private Signal didFinishDrinkingMilk;

	private FloatParameter cooldown;

	private static CellOffset UnderwaterCellOffset = new CellOffset(0, -1);

	private static CellOffset GroundCellOffset = new CellOffset(0, 0);

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = lookingToDrinkMilk;
		base.serializable = SerializeType.ParamsOnly;
		lookingToDrinkMilk.ParamTransition(cooldown, satisfied, GameStateMachine<DrinkMilkMonitor, Instance, IStateMachineTarget, Def>.IsGTZero).OnSignal(didFinishDrinkingMilk, applyEffect).PreBrainUpdate(FindMilkFeederTarget)
			.ToggleBehaviour(GameTags.Creatures.Behaviour_TryToDrinkMilkFromFeeder, (Instance smi) => !smi.targetMilkFeeder.IsNullOrStopped() && !smi.targetMilkFeeder.IsReserved())
			.Exit(delegate(Instance smi)
			{
				smi.targetMilkFeeder = null;
			});
		applyEffect.Enter(ApplyEffect).Enter(EnterCooldown).EnterGoTo(satisfied);
		satisfied.ParamTransition(cooldown, lookingToDrinkMilk, GameStateMachine<DrinkMilkMonitor, Instance, IStateMachineTarget, Def>.IsLTEZero).ScheduleGoTo((Instance smi) => cooldown.Get(smi), lookingToDrinkMilk).Update(CooldownUpdate, UpdateRate.SIM_1000ms);
	}

	private static void EnterCooldown(Instance smi)
	{
		smi.sm.cooldown.Set(600f, smi);
	}

	private static void ApplyEffect(Instance smi)
	{
		smi.ApplyDrinkEffect();
	}

	private static void CooldownUpdate(Instance smi, float dt)
	{
		float num = smi.sm.cooldown.Get(smi);
		float value = num - dt;
		smi.sm.cooldown.Set(value, smi);
	}

	private static void FindMilkFeederTarget(Instance smi)
	{
		int num = Grid.PosToCell(smi.gameObject);
		if (!Grid.IsValidCell(num))
		{
			return;
		}
		List<MilkFeeder.Instance> items = Components.MilkFeeders.GetItems(Grid.WorldIdx[num]);
		if (items == null || items.Count == 0)
		{
			return;
		}
		using ListPool<MilkFeeder.Instance, DrinkMilkMonitor>.PooledList pooledList = PoolsFor<DrinkMilkMonitor>.AllocateList<MilkFeeder.Instance>();
		CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(num);
		bool flag = !smi.isAquaticCreature;
		if (cavityForCell != null && cavityForCell.room != null && (!flag || cavityForCell.room.roomType == Db.Get().RoomTypes.CreaturePen))
		{
			foreach (MilkFeeder.Instance item in items)
			{
				if (!item.IsNullOrDestroyed())
				{
					bool flag2 = item.PrefabID() == "UnderwaterMilkFeeder";
					int cell = Grid.OffsetCell(Grid.PosToCell(item), flag2 ? UnderwaterCellOffset : GroundCellOffset);
					CavityInfo cavityForCell2 = Game.Instance.roomProber.GetCavityForCell(cell);
					if (smi.isAquaticCreature == flag2 && cavityForCell2 == cavityForCell && item.IsReadyToStartFeeding())
					{
						pooledList.Add(item);
					}
				}
			}
		}
		bool canDrown = smi.drowningMonitor != null && smi.drowningMonitor.canDrownToDeath && !smi.drowningMonitor.livesUnderWater;
		smi.targetMilkFeeder = null;
		smi.doesTargetMilkFeederHaveSpaceForCritter = false;
		int resultCost = -1;
		foreach (MilkFeeder.Instance item2 in pooledList)
		{
			MilkFeeder.Instance milkFeeder = item2;
			if (ConsiderCell(smi.GetDrinkCellOf(milkFeeder, isTwoByTwoCritterCramped: false)))
			{
				smi.doesTargetMilkFeederHaveSpaceForCritter = false;
			}
			else if (ConsiderCell(smi.GetDrinkCellOf(milkFeeder, isTwoByTwoCritterCramped: true)))
			{
				smi.doesTargetMilkFeederHaveSpaceForCritter = true;
			}
			bool ConsiderCell(int cell2)
			{
				if (canDrown && !smi.drowningMonitor.IsCellSafe(cell2))
				{
					return false;
				}
				int navigationCost = smi.navigator.GetNavigationCost(cell2);
				if (navigationCost == -1)
				{
					return false;
				}
				if (navigationCost < resultCost || resultCost == -1)
				{
					resultCost = navigationCost;
					smi.targetMilkFeeder = milkFeeder;
					return true;
				}
				return false;
			}
		}
	}
}
