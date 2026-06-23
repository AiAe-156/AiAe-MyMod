using Klei;
using UnityEngine;

public class BreathingGeyser : GameStateMachine<BreathingGeyser, BreathingGeyser.Instance, IStateMachineTarget, BreathingGeyser.Def>
{
	public class Def : BaseDef
	{
		public float inhaleRate;

		public float exhaleRate;

		public byte diseaseIdx;

		public float germsPerKg;
	}

	public class ActiveStates : State
	{
		public class AnimStates : State
		{
			public State pre;

			public State loop;

			public State pst;

			public State done;
		}

		public AnimStates inhale;

		public AnimStates exhale;
	}

	public new class Instance : GameInstance
	{
		private ElementConsumer elementConsumer;

		private Submergable submergable;

		private Storage storage;

		private int exhaleCell;

		private KPrefabID prefabID;

		public bool IsSubmerged => submergable.IsSubmerged;

		public bool IsExhaling => prefabID.HasTag(GameTags.GeyserExhaling);

		public bool IsStorageFull => storage.RemainingCapacity() <= 0f;

		public bool IsStorageEmpty => storage.ExactMassStored() == 0f;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			elementConsumer = GetComponent<ElementConsumer>();
			storage = GetComponent<Storage>();
			submergable = GetComponent<Submergable>();
			exhaleCell = Grid.PosToCell(base.transform.GetPosition() + elementConsumer.sampleCellOffset);
			prefabID = GetComponent<KPrefabID>();
		}

		public void ExhaleUpdate(float dt)
		{
			if (dt != 0f)
			{
				PrimaryElement primaryElement = storage.FindFirstWithMass(GameTags.Liquid);
				if (!(primaryElement == null) && primaryElement.Mass != 0f)
				{
					float b = base.smi.def.exhaleRate * dt;
					float num = Mathf.Min(primaryElement.Mass, b);
					Pickupable pickupable = primaryElement.GetComponent<Pickupable>().Take(num);
					SimUtil.DiseaseInfo diseaseInfo = SimUtil.CalculateFinalDiseaseInfo(pickupable.PrimaryElement.DiseaseIdx, pickupable.PrimaryElement.DiseaseCount, base.smi.def.diseaseIdx, (int)(base.smi.def.germsPerKg * num));
					SimMessages.AddRemoveSubstance(exhaleCell, primaryElement.ElementID, CellEventLogger.Instance.BreathingGeyser, num, primaryElement.Temperature, diseaseInfo.idx, diseaseInfo.count);
					Util.KDestroyGameObject(pickupable);
				}
			}
		}

		public Element GetStoredElementTag()
		{
			PrimaryElement primaryElement = storage.FindFirstWithMass(GameTags.Liquid);
			if (primaryElement == null)
			{
				return null;
			}
			return primaryElement.Element;
		}

		public float GetStoredMass()
		{
			return storage.MassStored();
		}

		public string GetExpellingElementName()
		{
			PrimaryElement primaryElement = storage.FindFirstWithMass(GameTags.Liquid);
			if (primaryElement != null && primaryElement.Mass > 0f)
			{
				return primaryElement.Element.name;
			}
			return Strings.Get("STRINGS.ELEMENTS.STATE.LIQUID");
		}

		public void RestrictElementConsumerState()
		{
			elementConsumer.consumptionRate = 0f;
		}

		public void UnrestrictElementConsumerState()
		{
			elementConsumer.consumptionRate = base.def.inhaleRate;
		}

		public void SetElementConsumerState(bool enabled)
		{
			elementConsumer.EnableConsumption(enabled);
		}
	}

	public BoolParameter exhaleFlowDirection;

	public State inactive;

	public ActiveStates active;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = inactive;
		root.Enter(RestrictElementConsumer);
		inactive.EventTransition(GameHashes.SubmergedStateChanged, active, IsSubmerged).EventTransition(GameHashes.OnStorageChange, active.exhale, HasStoredMass).PlayAnim("inactive")
			.Enter(DisableElementConsumer)
			.Enter(RestrictElementConsumer);
		active.Transition(inactive, ShouldGoInactive, UpdateRate.SIM_1000ms).DefaultState(active.inhale);
		active.inhale.ParamTransition(exhaleFlowDirection, active.exhale, GameStateMachine<BreathingGeyser, Instance, IStateMachineTarget, Def>.IsTrue).DefaultState(active.inhale.pre);
		active.inhale.pre.PlayAnim("inhale_pre").OnAnimQueueComplete(active.inhale.loop);
		active.inhale.loop.Enter(UnRestrictElementConsumer).Enter(EnableElementConsumer).PlayAnim("inhale_loop", KAnim.PlayMode.Loop)
			.EventTransition(GameHashes.OnStorageChange, active.inhale.pst, IsStorageFull)
			.EventTransition(GameHashes.SubmergedStateChanged, active.inhale.pst, IsNotSubmergedWithStorage);
		active.inhale.pst.PlayAnim("inhale_pst").OnAnimQueueComplete(active.inhale.done);
		active.inhale.done.Enter(StopInhaling);
		active.exhale.ParamTransition(exhaleFlowDirection, active.inhale, GameStateMachine<BreathingGeyser, Instance, IStateMachineTarget, Def>.IsFalse).DefaultState(active.exhale.pre).ToggleMainStatusItem(Db.Get().BuildingStatusItems.GeyserExpelling, (Instance smi) => new Tuple<Element, float>(smi.GetStoredElementTag(), smi.def.exhaleRate))
			.ToggleTag(GameTags.GeyserExhaling)
			.Enter(DisableElementConsumer)
			.Enter(RestrictElementConsumer);
		active.exhale.pre.PlayAnim("exhale_pre").OnAnimQueueComplete(active.exhale.loop);
		active.exhale.loop.EventTransition(GameHashes.OnStorageChange, active.exhale.pst, IsStorageEmpty).PlayAnim("exhale_loop", KAnim.PlayMode.Loop).Update(ExhaleUpdate, UpdateRate.SIM_1000ms);
		active.exhale.pst.PlayAnim("exhale_pst").OnAnimQueueComplete(active.exhale.done);
		active.exhale.done.Enter(StopExhaling);
	}

	public static void StopInhaling(Instance smi)
	{
		smi.sm.exhaleFlowDirection.Set(value: true, smi);
	}

	public static void StopExhaling(Instance smi)
	{
		smi.sm.exhaleFlowDirection.Set(value: false, smi);
	}

	public static void RestrictElementConsumer(Instance smi)
	{
		smi.RestrictElementConsumerState();
	}

	public static void UnRestrictElementConsumer(Instance smi)
	{
		smi.UnrestrictElementConsumerState();
	}

	public static void EnableElementConsumer(Instance smi)
	{
		smi.SetElementConsumerState(enabled: true);
	}

	public static void DisableElementConsumer(Instance smi)
	{
		smi.SetElementConsumerState(enabled: false);
	}

	public static bool IsSubmerged(Instance smi)
	{
		return smi.IsSubmerged;
	}

	public static bool IsStorageFull(Instance smi)
	{
		return smi.IsStorageFull;
	}

	public static bool IsStorageEmpty(Instance smi)
	{
		return smi.IsStorageEmpty;
	}

	public static bool HasStoredMass(Instance smi)
	{
		return !smi.IsStorageEmpty;
	}

	public static bool ShouldGoInactive(Instance smi)
	{
		return !smi.IsSubmerged && smi.IsStorageEmpty;
	}

	public static bool IsNotSubmergedWithStorage(Instance smi)
	{
		return !smi.IsSubmerged && !smi.IsStorageEmpty;
	}

	public static void ExhaleUpdate(Instance smi, float dt)
	{
		smi.ExhaleUpdate(dt);
	}
}
