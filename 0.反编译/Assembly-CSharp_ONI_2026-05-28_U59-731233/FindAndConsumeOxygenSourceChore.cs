using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class FindAndConsumeOxygenSourceChore : Chore<FindAndConsumeOxygenSourceChore.Instance>
{
	public class States : GameStateMachine<States, Instance, FindAndConsumeOxygenSourceChore>
	{
		public class InstallState : State
		{
			public State pre;

			public State loop;

			public State pst;
		}

		public FetchSubState fetch;

		public InstallState consume;

		public State complete;

		public State oxygenSourceLost;

		public State scheduleFailure;

		public TargetParameter dupe;

		public TargetParameter oxygenSourceItem;

		public TargetParameter pickedUpItem;

		public FloatParameter actualunits;

		public FloatParameter amountRequested;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = fetch;
			Target(dupe);
			fetch.InitializeStates(dupe, oxygenSourceItem, pickedUpItem, amountRequested, actualunits, consume).OnTargetLost(oxygenSourceItem, oxygenSourceLost).ScheduleChange(scheduleFailure, IsNotAllowedByScheduleAndChoreIsNotCritical);
			consume.Target(pickedUpItem).OnTargetLost(pickedUpItem, oxygenSourceLost).Target(dupe)
				.ScheduleChange(scheduleFailure, IsNotAllowedByScheduleAndChoreIsNotCritical)
				.DefaultState(consume.pre)
				.ToggleAnims("anim_bionic_kanim")
				.ToggleTag(GameTags.RecoveringBreath)
				.Enter("Add Symbol Override", delegate(FindAndConsumeOxygenSourceChore.Instance smi)
				{
					SetOverrideAnimSymbol(smi, overriding: true);
				})
				.Exit("Revert Symbol Override", delegate(FindAndConsumeOxygenSourceChore.Instance smi)
				{
					SetOverrideAnimSymbol(smi, overriding: false);
				});
			consume.pre.PlayAnim("consume_canister_pre", KAnim.PlayMode.Once).OnAnimQueueComplete(consume.loop).ScheduleGoTo(3f, consume.loop);
			consume.loop.PlayAnim("consume_canister_loop", KAnim.PlayMode.Loop).ScheduleGoTo(GetConsumeDuration, consume.pst);
			consume.pst.PlayAnim("consume_canister_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(complete).ScheduleGoTo(3f, complete);
			complete.Enter(ExtractOxygenFromItem).ReturnSuccess();
			scheduleFailure.Target(dupe).ReturnFailure();
			oxygenSourceLost.Target(dupe).Enter(TriggerOxygenItemLostSignal).ReturnFailure();
		}
	}

	public class Instance : GameStateMachine<States, Instance, FindAndConsumeOxygenSourceChore, object>.GameInstance, BionicOxygenTankMonitor.IChore
	{
		public KBatchedAnimController canisterBodySymbolOverrideObject;

		public KBatchedAnimController canisterCapSymbolOverrideObject;

		public BionicOxygenTankMonitor.Instance oxygenTankMonitor => base.sm.dupe.Get(this).GetSMI<BionicOxygenTankMonitor.Instance>();

		public Instance(FindAndConsumeOxygenSourceChore master, GameObject duplicant)
			: base(master)
		{
		}

		public bool IsConsumingOxygen()
		{
			return !IsInsideState(base.sm.fetch);
		}

		public void ShowBottleSymbolOverrideObject(Element elementOfCanister)
		{
			if (canisterBodySymbolOverrideObject == null)
			{
				KAnimFile[] anims = elementOfCanister.substance.anims;
				GameObject gameObject = Util.NewGameObject(base.gameObject, "canister_symbol");
				gameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
				gameObject.SetActive(value: false);
				canisterBodySymbolOverrideObject = gameObject.AddComponent<KBatchedAnimController>();
				canisterBodySymbolOverrideObject.AnimFiles = anims;
				canisterBodySymbolOverrideObject.initialAnim = "idle1";
				canisterBodySymbolOverrideObject.SetSymbolVisiblity("cap", is_visible: false);
				canisterBodySymbolOverrideObject.SetSymbolVisiblity("substance_tinter_cap", is_visible: false);
				KBatchedAnimTracker kBatchedAnimTracker = gameObject.AddComponent<KBatchedAnimTracker>();
				kBatchedAnimTracker.symbol = new HashedString("canister");
				kBatchedAnimTracker.offset = Vector3.zero;
				kBatchedAnimTracker.matchParentOffset = true;
				kBatchedAnimTracker.forceAlwaysAlive = true;
				kBatchedAnimTracker.forceAlwaysVisible = true;
				gameObject.SetActive(value: true);
				Color32 colour = elementOfCanister.substance.colour;
				colour.a = byte.MaxValue;
				canisterBodySymbolOverrideObject.SetSymbolTint(new KAnimHashedString("substance_tinter"), colour);
			}
			if (canisterCapSymbolOverrideObject == null)
			{
				KAnimFile[] anims2 = elementOfCanister.substance.anims;
				GameObject gameObject2 = Util.NewGameObject(base.gameObject, "canister_cap_symbol");
				gameObject2.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
				gameObject2.SetActive(value: false);
				canisterCapSymbolOverrideObject = gameObject2.AddComponent<KBatchedAnimController>();
				canisterCapSymbolOverrideObject.AnimFiles = anims2;
				canisterCapSymbolOverrideObject.initialAnim = "cap";
				KBatchedAnimTracker kBatchedAnimTracker2 = gameObject2.AddComponent<KBatchedAnimTracker>();
				kBatchedAnimTracker2.symbol = new HashedString("cap");
				kBatchedAnimTracker2.offset = Vector3.zero;
				kBatchedAnimTracker2.matchParentOffset = true;
				kBatchedAnimTracker2.forceAlwaysAlive = true;
				kBatchedAnimTracker2.forceAlwaysVisible = true;
				gameObject2.SetActive(value: true);
				Color32 colour2 = elementOfCanister.substance.colour;
				colour2.a = byte.MaxValue;
				canisterCapSymbolOverrideObject.SetSymbolTint(new KAnimHashedString("substance_tinter_cap"), colour2);
			}
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			bool symbolVisible;
			Vector4 column = component.GetSymbolTransform("canister", out symbolVisible).GetColumn(3);
			Vector3 position = column;
			position.z = canisterBodySymbolOverrideObject.transform.parent.position.z - 0.01f;
			canisterBodySymbolOverrideObject.transform.position = position;
			bool symbolVisible2;
			Vector4 column2 = component.GetSymbolTransform("canister", out symbolVisible2).GetColumn(3);
			Vector3 position2 = column2;
			position2.z = position.z - 0.01f;
			canisterCapSymbolOverrideObject.transform.position = position2;
			component.SetSymbolVisiblity("canister", is_visible: false);
			component.SetSymbolVisiblity("cap", is_visible: false);
		}

		public void RemoveSymbolOverrideObject()
		{
			if (canisterBodySymbolOverrideObject != null)
			{
				canisterBodySymbolOverrideObject.gameObject.DeleteObject();
				canisterBodySymbolOverrideObject = null;
			}
			if (canisterCapSymbolOverrideObject != null)
			{
				canisterCapSymbolOverrideObject.gameObject.DeleteObject();
				canisterCapSymbolOverrideObject = null;
			}
		}

		protected override void OnCleanUp()
		{
			RemoveSymbolOverrideObject();
			base.OnCleanUp();
		}
	}

	public const string CANISTER_BODY_SYMBOL_NAME = "canister";

	public const string CANISTER_CAP_SYMBOL_NAME = "cap";

	public const string CANISTER_CAP_COLOR_SYMBOL_NAME = "substance_tinter_cap";

	public const string CANISTER_BODY_COLOR_SYMBOL_NAME = "substance_tinter";

	public const float MAX_LOOP_DURATION = 24f;

	public const float MIN_LOOP_DURATION = 4.333f;

	public static readonly Precondition OxygenSourceItemIsNotNull = new Precondition
	{
		id = "OxygenSourceIsNotNull",
		description = DUPLICANTS.CHORES.PRECONDITIONS.EDIBLE_IS_NOT_NULL,
		fn = delegate(ref Precondition.Context context, object data)
		{
			BionicOxygenTankMonitor.Instance sMI = context.consumerState.consumer.GetSMI<BionicOxygenTankMonitor.Instance>();
			Pickupable closestOxygenSource = sMI.GetClosestOxygenSource();
			return closestOxygenSource != null && closestOxygenSource.UnreservedAmount > 0f;
		}
	};

	public FindAndConsumeOxygenSourceChore(IStateMachineTarget target, bool critical)
		: base(critical ? Db.Get().ChoreTypes.FindOxygenSourceItem_Critical : Db.Get().ChoreTypes.FindOxygenSourceItem, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, critical ? PriorityScreen.PriorityClass.compulsory : PriorityScreen.PriorityClass.personalNeeds, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new Instance(this, target.gameObject);
		AddPrecondition(ChorePreconditions.instance.IsNotRedAlert);
		AddPrecondition(OxygenSourceItemIsNotNull);
	}

	public override void Begin(Precondition.Context context)
	{
		if (context.consumerState.consumer == null)
		{
			Debug.LogError("FindAndConsumeOxygenSourceChore null context.consumer");
			return;
		}
		BionicOxygenTankMonitor.Instance sMI = context.consumerState.consumer.GetSMI<BionicOxygenTankMonitor.Instance>();
		if (sMI == null)
		{
			Debug.LogError("FindAndConsumeOxygenSourceChore null BionicOxygenTankMonitor.Instance");
			return;
		}
		Pickupable closestOxygenSource = sMI.GetClosestOxygenSource();
		if (closestOxygenSource == null)
		{
			Debug.LogError("FindAndConsumeOxygenSourceChore null oxygenSourceItem.gameObject");
			return;
		}
		base.smi.sm.oxygenSourceItem.Set(closestOxygenSource.gameObject, base.smi);
		base.smi.sm.amountRequested.Set(Mathf.Min(sMI.SpaceAvailableInTank, closestOxygenSource.UnreservedAmount), base.smi);
		base.smi.sm.dupe.Set(context.consumerState.consumer, base.smi);
		base.Begin(context);
	}

	public static bool IsNotAllowedByScheduleAndChoreIsNotCritical(Instance smi)
	{
		return !IsCriticalChore(smi) && !IsAllowedBySchedule(smi);
	}

	public static bool IsAllowedBySchedule(Instance smi)
	{
		return BionicOxygenTankMonitor.IsAllowedToSeekOxygenBySchedule(smi.oxygenTankMonitor);
	}

	public static bool IsCriticalChore(Instance smi)
	{
		return smi.master.choreType == Db.Get().ChoreTypes.FindOxygenSourceItem_Critical;
	}

	public static void ExtractOxygenFromItem(Instance smi)
	{
		GameObject gameObject = smi.sm.pickedUpItem.Get(smi);
		PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
		if (component.Element.IsGas)
		{
			Storage[] components = smi.gameObject.GetComponents<Storage>();
			for (int i = 0; i < components.Length; i++)
			{
				if (!(components[i] != smi.oxygenTankMonitor.storage))
				{
					continue;
				}
				List<GameObject> list = new List<GameObject>();
				components[i].Find(GameTags.Breathable, list);
				foreach (GameObject item in list)
				{
					if (item != null)
					{
						components[i].ConsumeAndGetDisease(component.Element.tag, component.Mass, out var amount_consumed, out var disease_info, out var aggregate_temperature);
						smi.oxygenTankMonitor.storage.AddGasChunk(component.Element.id, amount_consumed, aggregate_temperature, disease_info.idx, disease_info.count, keep_zero_mass: false);
						break;
					}
				}
			}
		}
		else
		{
			SimHashes element = SimHashes.Oxygen;
			Element element2 = ElementLoader.GetElement(component.Element.sublimateId.CreateTag());
			if (element2.HasTag(GameTags.Breathable))
			{
				element = component.Element.sublimateId;
			}
			smi.oxygenTankMonitor.storage.AddGasChunk(element, component.Mass, component.Temperature, component.DiseaseIdx, component.DiseaseCount, keep_zero_mass: false);
			Util.KDestroyGameObject(gameObject);
		}
	}

	public static void SetOverrideAnimSymbol(Instance smi, bool overriding)
	{
		GameObject gameObject = smi.sm.pickedUpItem.Get(smi);
		if (gameObject != null)
		{
			KBatchedAnimTracker component = gameObject.GetComponent<KBatchedAnimTracker>();
			if (component != null)
			{
				component.enabled = !overriding;
			}
			Storage.MakeItemInvisible(gameObject, overriding, is_initializing: false);
		}
		if (!overriding)
		{
			smi.RemoveSymbolOverrideObject();
		}
		else if (gameObject != null)
		{
			PrimaryElement component2 = gameObject.GetComponent<PrimaryElement>();
			smi.ShowBottleSymbolOverrideObject(component2.Element);
		}
	}

	public static void TriggerOxygenItemLostSignal(Instance smi)
	{
		if (smi.oxygenTankMonitor != null)
		{
			smi.oxygenTankMonitor.sm.OxygenSourceItemLostSignal.Trigger(smi.oxygenTankMonitor);
		}
	}

	public static float GetConsumeDuration(Instance smi)
	{
		float num = smi.sm.actualunits.Get(smi);
		float num2 = num / BionicOxygenTankMonitor.OXYGEN_TANK_CAPACITY_KG;
		return Mathf.Max(24f * num2, 4.333f);
	}
}
