using System.Collections.Generic;
using Klei.AI;
using UnityEngine;

public class MilkFeeder : GameStateMachine<MilkFeeder, MilkFeeder.Instance, IStateMachineTarget, MilkFeeder.Def>
{
	public class Def : BaseDef, IGameObjectEffectDescriptor
	{
		public CellOffset drinkCellOffset;

		public Tag elementProducedTag;

		public float unitsProducedPerFeeding;

		public bool tintMeter;

		public List<Descriptor> GetDescriptors(GameObject go)
		{
			List<Descriptor> list = new List<Descriptor>();
			Instance sMI = go.GetSMI<Instance>();
			for (int i = 0; i < MilkFeederConfig.EffectsPerDrinkableLiquid.Length; i++)
			{
				Tag first = MilkFeederConfig.EffectsPerDrinkableLiquid[i].first;
				string second = MilkFeederConfig.EffectsPerDrinkableLiquid[i].second;
				Descriptor item = default(Descriptor);
				item.SetupDescriptor(Strings.Get("STRINGS.CREATURES.MODIFIERS." + second.ToUpper() + ".NAME"), "");
				list.Add(item);
				Effect.AddModifierDescriptions(list, second, increase_indent: true, "STRINGS.CREATURES.STATS.");
			}
			return list;
		}
	}

	public class OffState : State
	{
		public State noOperational;

		public State strawBlocked;

		public State noLiquidOnStraw;
	}

	public class OnState : State
	{
		public class WorkingState : State
		{
			public State empty;

			public State refilling;

			public State full;

			public State emptying;
		}

		public State pre;

		public WorkingState working;

		public State pst;
	}

	public new class Instance : GameInstance
	{
		public Storage milkStorage;

		public MeterController storageMeter;

		private CellOffset strawCellOffset = new CellOffset(0, 0);

		private Operational operational;

		private BuildingPointStraw straw;

		public bool IsOperational => operational != null && operational.IsOperational;

		public bool IsStrawInstalled => straw != null;

		public bool IsStrawOutsideLiquid => IsStrawInstalled && !straw.isInLiquid;

		public bool IsStrawBlocked => IsStrawInstalled && straw.currentDepth <= 0;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			milkStorage = GetComponent<Storage>();
			operational = GetComponent<Operational>();
			straw = GetComponent<BuildingPointStraw>();
			storageMeter = new MeterController(base.smi.GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer);
			Subscribe(360192579, OnStrawChanged);
		}

		private void OnStrawChanged(object o)
		{
			BuildingPointStraw buildingPointStraw = (BuildingPointStraw)o;
			CellOffset bottomCellOffset = buildingPointStraw.GetBottomCellOffset();
			strawCellOffset = bottomCellOffset;
		}

		public override void StartSM()
		{
			base.StartSM();
			Components.MilkFeeders.Add(base.smi.GetMyWorldId(), this);
			RefreshLiquidColor();
		}

		protected override void OnCleanUp()
		{
			base.OnCleanUp();
			Components.MilkFeeders.Remove(base.smi.GetMyWorldId(), this);
		}

		public CellOffset GetDrinkCellOffset()
		{
			return base.def.drinkCellOffset + strawCellOffset;
		}

		public void UpdateStorageMeter()
		{
			storageMeter.SetPositionPercent(1f - Mathf.Clamp01(milkStorage.RemainingCapacity() / milkStorage.capacityKg));
		}

		public bool IsReserved()
		{
			return HasTag(GameTags.Creatures.ReservedByCreature);
		}

		public void SetReserved(bool isReserved)
		{
			if (isReserved)
			{
				Debug.Assert(!HasTag(GameTags.Creatures.ReservedByCreature));
				GetComponent<KPrefabID>().SetTag(GameTags.Creatures.ReservedByCreature, set: true);
			}
			else if (HasTag(GameTags.Creatures.ReservedByCreature))
			{
				GetComponent<KPrefabID>().RemoveTag(GameTags.Creatures.ReservedByCreature);
			}
			else
			{
				Debug.LogWarningFormat(base.smi.gameObject, "Tried to unreserve a MilkFeeder that wasn't reserved");
			}
		}

		public void RefreshLiquidColor()
		{
			PrimaryElement primaryElement = milkStorage.FindFirstWithMass(base.def.elementProducedTag, base.def.unitsProducedPerFeeding);
			if (primaryElement == null)
			{
				return;
			}
			Tag tag = primaryElement.PrefabID();
			Element element = ElementLoader.FindElementByTag(tag);
			if (base.def.tintMeter && milkStorage != null)
			{
				KBatchedAnimController meterController = storageMeter.meterController;
				if (meterController != null)
				{
					GameUtil.TintLiquidSymbolOnBuilding("meter_fill", meterController, element);
				}
			}
			KBatchedAnimController[] componentsInChildren = base.gameObject.GetComponentsInChildren<KBatchedAnimController>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				foreach (KBatchedAnimController kBatchedAnimController in componentsInChildren)
				{
					GameUtil.TintLiquidSymbolOnBuilding("Milk_fg", kBatchedAnimController, element);
					GameUtil.TintLiquidSymbolOnBuilding("Milk_fill_fg", kBatchedAnimController, element);
				}
			}
		}

		public bool IsReadyToStartFeeding()
		{
			return base.sm.isReadyToStartFeeding.Get(base.smi);
		}

		public void RequestToStartFeeding(DrinkMilkStates.Instance feedingCritter)
		{
			base.sm.currentFeedingCritter.Set(feedingCritter, base.smi);
		}

		public void StopFeeding()
		{
			base.sm.currentFeedingCritter.Get(base.smi)?.RequestToStopFeeding();
			base.sm.currentFeedingCritter.Set(null, base.smi);
		}

		public bool HasEnoughMilkForOneFeeding()
		{
			PrimaryElement primaryElement = milkStorage.FindFirstWithMass(base.def.elementProducedTag, base.def.unitsProducedPerFeeding);
			return primaryElement != null;
		}

		public Tag ConsumeMilkForOneFeeding()
		{
			PrimaryElement cmp = milkStorage.FindFirstWithMass(base.def.elementProducedTag, base.def.unitsProducedPerFeeding);
			Tag tag = cmp.PrefabID();
			milkStorage.ConsumeIgnoringDisease(tag, base.def.unitsProducedPerFeeding);
			return tag;
		}

		public bool IsInCreaturePenRoom()
		{
			Room roomOfGameObject = Game.Instance.roomProber.GetRoomOfGameObject(base.gameObject);
			if (roomOfGameObject == null)
			{
				return false;
			}
			return roomOfGameObject.roomType == Db.Get().RoomTypes.CreaturePen;
		}
	}

	private const string TINT_METER_SYMBOL_NAME = "meter_fill";

	private const string TINT_SYMBOL_NAME = "Milk_fg";

	private const string TINT_SYMBOL2_NAME = "Milk_fill_fg";

	private OffState off;

	private OnState on;

	public BoolParameter isReadyToStartFeeding;

	public ObjectParameter<DrinkMilkStates.Instance> currentFeedingCritter;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = off;
		root.Enter(delegate(Instance smi)
		{
			smi.UpdateStorageMeter();
		}).EventHandler(GameHashes.OnStorageChange, delegate(Instance smi)
		{
			smi.UpdateStorageMeter();
		}).EventHandler(GameHashes.OnStorageChange, RefreshLiquidColor);
		off.PlayAnim("off").EventTransition(GameHashes.OperationalChanged, on, ShouldBeOn).EventTransition(GameHashes.BuildingStrawChange, on, ShouldBeOn)
			.Enter(RefreshLiquidColor)
			.DefaultState(off.noOperational);
		off.noOperational.EventTransition(GameHashes.OperationalChanged, off.strawBlocked, (Instance smi) => IsOperational(smi) && IsStrawBlocked(smi)).EventTransition(GameHashes.OperationalChanged, off.noLiquidOnStraw, (Instance smi) => IsOperational(smi) && IsStrawOutsideLiquid(smi));
		off.strawBlocked.ToggleStatusItem(Db.Get().BuildingStatusItems.OutputTileBlocked).EventTransition(GameHashes.OperationalChanged, off.noOperational, GameStateMachine<MilkFeeder, Instance, IStateMachineTarget, Def>.Not(IsOperational)).EventTransition(GameHashes.BuildingStrawChange, off.noLiquidOnStraw, (Instance smi) => !IsStrawBlocked(smi) && IsStrawOutsideLiquid(smi));
		off.noLiquidOnStraw.ToggleStatusItem(Db.Get().BuildingStatusItems.NotSubmerged).EventTransition(GameHashes.OperationalChanged, off.noOperational, GameStateMachine<MilkFeeder, Instance, IStateMachineTarget, Def>.Not(IsOperational)).EventTransition(GameHashes.BuildingStrawChange, off.strawBlocked, IsStrawBlocked);
		on.DefaultState(on.pre).EventTransition(GameHashes.BuildingStrawChange, on.pst, (Instance smi) => !ShouldBeOn(smi) && smi.GetCurrentState() != on.pre).EventTransition(GameHashes.BuildingStrawChange, off, (Instance smi) => !ShouldBeOn(smi) && smi.GetCurrentState() == on.pre)
			.EventTransition(GameHashes.OperationalChanged, on.pst, (Instance smi) => !ShouldBeOn(smi) && smi.GetCurrentState() != on.pre)
			.EventTransition(GameHashes.OperationalChanged, off, (Instance smi) => !ShouldBeOn(smi) && smi.GetCurrentState() == on.pre)
			.Enter(RefreshLiquidColor);
		on.pre.PlayAnim("working_pre").OnAnimQueueComplete(on.working);
		on.working.PlayAnim("on").DefaultState(on.working.empty);
		on.working.empty.PlayAnim("empty").EnterTransition(on.working.refilling, (Instance smi) => smi.HasEnoughMilkForOneFeeding()).EventHandler(GameHashes.OnStorageChange, delegate(Instance smi)
		{
			if (smi.HasEnoughMilkForOneFeeding())
			{
				smi.GoTo(on.working.refilling);
			}
		});
		on.working.refilling.Enter(RefreshLiquidColor).PlayAnim("fill").OnAnimQueueComplete(on.working.full);
		on.working.full.PlayAnim("full").Enter(delegate(Instance smi)
		{
			isReadyToStartFeeding.Set(value: true, smi);
		}).Exit(delegate(Instance smi)
		{
			isReadyToStartFeeding.Set(value: false, smi);
		})
			.ParamTransition(currentFeedingCritter, on.working.emptying, (Instance smi, DrinkMilkStates.Instance val) => val != null);
		on.working.emptying.EnterTransition(on.working.full, delegate(Instance smi)
		{
			DrinkMilkMonitor.Instance sMI = currentFeedingCritter.Get(smi).GetSMI<DrinkMilkMonitor.Instance>();
			return sMI != null && !sMI.def.consumesMilk;
		}).PlayAnim("emptying").OnAnimQueueComplete(on.working.empty)
			.Exit(delegate(Instance smi)
			{
				smi.StopFeeding();
			});
		on.pst.PlayAnim("working_pst").OnAnimQueueComplete(off);
	}

	public static bool ShouldBeOn(Instance smi)
	{
		return IsOperational(smi) && !IsStrawBlocked(smi) && !IsStrawOutsideLiquid(smi);
	}

	public static bool IsOperational(Instance smi)
	{
		return smi.IsOperational;
	}

	public static bool IsStrawBlocked(Instance smi)
	{
		return smi.IsStrawBlocked;
	}

	public static bool IsStrawOutsideLiquid(Instance smi)
	{
		return smi.IsStrawOutsideLiquid;
	}

	public static void RefreshLiquidColor(Instance smi)
	{
		smi.RefreshLiquidColor();
	}
}
