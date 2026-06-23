using System;
using System.Collections.Generic;
using UnityEngine;

public class OilChanger : GameStateMachine<OilChanger, OilChanger.Instance, IStateMachineTarget, OilChanger.Def>
{
	public class Def : BaseDef
	{
		public float MIN_LUBRICANT_MASS_TO_WORK = 200f;
	}

	public class OperationalStates : State
	{
		public State oilNeeded;

		public State ready;
	}

	public new class Instance : GameInstance, IFetchList
	{
		private Storage storage;

		private Operational operational;

		private MeterController oilStorageMeter;

		private MeterController readyLightMeter;

		private Dictionary<Tag, float> remainingLubricationMass = new Dictionary<Tag, float> { [GameTags.LubricatingOil] = 0f };

		public bool IsOperational => operational.IsOperational;

		public float OilAmount => storage.GetMassAvailable(GameTags.LubricatingOil);

		public Storage Destination => storage;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			storage = GetComponent<Storage>();
			operational = GetComponent<Operational>();
			oilStorageMeter = new MeterController(component, "meter_target", "meter", Meter.Offset.UserSpecified, Grid.SceneLayer.BuildingFront);
			readyLightMeter = new MeterController(component, "light_target", "light_off", Meter.Offset.UserSpecified, Grid.SceneLayer.BuildingFront);
		}

		public void SetLEDState(bool isOn)
		{
			string text = (isOn ? "light_on" : "light_off");
			readyLightMeter.meterController.Play(text);
		}

		public void UpdateStorageMeter()
		{
			float positionPercent = OilAmount / storage.capacityKg;
			oilStorageMeter.SetPositionPercent(positionPercent);
		}

		public float GetMinimumAmount(Tag tag)
		{
			return base.def.MIN_LUBRICANT_MASS_TO_WORK;
		}

		public Dictionary<Tag, float> GetRemaining()
		{
			remainingLubricationMass[GameTags.LubricatingOil] = Mathf.Clamp(base.def.MIN_LUBRICANT_MASS_TO_WORK - OilAmount, 0f, base.def.MIN_LUBRICANT_MASS_TO_WORK);
			return remainingLubricationMass;
		}

		public Dictionary<Tag, float> GetRemainingMinimum()
		{
			throw new NotImplementedException();
		}
	}

	public const string STORAGE_METER_TARGET_NAME = "meter_target";

	public const string STORAGE_METER_ANIM_NAME = "meter";

	public const string LED_METER_TARGET_NAME = "light_target";

	public const string LED_METER_ANIM_ON_NAME = "light_on";

	public const string LED_METER_ANIM_OFF_NAME = "light_off";

	public State inoperational;

	public OperationalStates operational;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = inoperational;
		root.EventHandler(GameHashes.OnStorageChange, UpdateStorageMeter);
		inoperational.PlayAnim("off").Enter(LED_Off).Enter(UpdateStorageMeter)
			.TagTransition(GameTags.Operational, operational);
		operational.PlayAnim("on").Enter(UpdateStorageMeter).TagTransition(GameTags.Operational, inoperational, on_remove: true)
			.DefaultState(operational.oilNeeded);
		operational.oilNeeded.Enter(LED_Off).ToggleStatusItem(Db.Get().BuildingStatusItems.WaitingForMaterials).EventTransition(GameHashes.OnStorageChange, operational.ready, HasEnoughLubricant);
		operational.ready.Enter(LED_On).ToggleChore(CreateChore, operational.oilNeeded);
	}

	public static bool HasEnoughLubricant(Instance smi)
	{
		return smi.OilAmount >= smi.def.MIN_LUBRICANT_MASS_TO_WORK;
	}

	private static bool IsOperational(Instance smi)
	{
		return smi.IsOperational;
	}

	public static void UpdateStorageMeter(Instance smi)
	{
		smi.UpdateStorageMeter();
	}

	public static void LED_On(Instance smi)
	{
		smi.SetLEDState(isOn: true);
	}

	public static void LED_Off(Instance smi)
	{
		smi.SetLEDState(isOn: false);
	}

	private static WorkChore<OilChangerWorkableUse> CreateChore(Instance smi)
	{
		return new WorkChore<OilChangerWorkableUse>(Db.Get().ChoreTypes.OilChange, smi.master, null, run_until_complete: true, null, null, null, allow_in_red_alert: true, null, ignore_schedule_block: false, only_when_operational: true, null, is_preemptable: false, allow_in_context_menu: true, allow_prioritization: true, PriorityScreen.PriorityClass.personalNeeds);
	}
}
