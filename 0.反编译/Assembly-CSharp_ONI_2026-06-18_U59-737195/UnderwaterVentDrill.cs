using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class UnderwaterVentDrill : GameStateMachine<UnderwaterVentDrill, UnderwaterVentDrill.Instance, IStateMachineTarget, UnderwaterVentDrill.Def>
{
	public class Def : BaseDef, IGameObjectEffectDescriptor
	{
		public Tag DiamondTag = SimHashes.Diamond.CreateTag();

		public float DiamondConsumptionRate;

		public float WorkDuration;

		public Vector3 ProgressBarOffset = Vector3.zero;

		public List<Descriptor> GetDescriptors(GameObject go)
		{
			List<Descriptor> list = new List<Descriptor>();
			string formattedMass = GameUtil.GetFormattedMass(DiamondConsumptionRate, GameUtil.TimeSlice.PerSecond);
			list.Add(new Descriptor(UI.BUILDINGEFFECTS.UNDERWATER_DRILL_DIAMOND_CONSUMPTION.Replace("{Rate}", formattedMass), UI.BUILDINGEFFECTS.TOOLTIPS.UNDERWATER_DRILL_DIAMOND_CONSUMPTION.Replace("{Rate}", formattedMass), Descriptor.DescriptorType.Requirement));
			return list;
		}
	}

	public class OperationalStates : State
	{
		public State idle;

		public State missingDiamonds;

		public PreLoopPostState working;

		public State workEnded;

		public State completed;
	}

	public new class Instance : GameInstance
	{
		private Storage storage;

		private UnderwaterVent.Instance vent;

		private Operational operational;

		private ProgressBar progressBar;

		private MeterController diamondMeter;

		public float DrillProgress => base.sm.DrillProgress.Get(this);

		public bool CanWork
		{
			get
			{
				if (HasAnyDiamond)
				{
					return IsVentBlocked;
				}
				return false;
			}
		}

		public bool HasAnyDiamond => storage.GetMassAvailable(base.def.DiamondTag) > 0f;

		public bool IsVentBlocked
		{
			get
			{
				if (vent != null)
				{
					return vent.IsBlocked;
				}
				return false;
			}
		}

		public bool IsOff => IsInsideState(base.sm.noOperational);

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			storage = GetComponent<Storage>();
			operational = GetComponent<Operational>();
			KBatchedAnimController component = GetComponent<KBatchedAnimController>();
			diamondMeter = new MeterController(component, "target_meter", "meter", Meter.Offset.Infront, Grid.SceneLayer.BuildingBack);
		}

		public override void StartSM()
		{
			int cell = Grid.PosToCell(base.gameObject);
			GameObject gameObject = Grid.Objects[cell, 1];
			vent = ((gameObject == null) ? null : gameObject.GetSMI<UnderwaterVent.Instance>());
			base.sm.Vent.Set((vent == null) ? null : vent.gameObject, this);
			base.StartSM();
			UpdateDiamondMeter();
		}

		public void SetOperationalActiveFlag(bool active)
		{
			operational.SetActive(active);
		}

		public bool DrillUpdate(float dt)
		{
			if (dt == 0f)
			{
				return false;
			}
			float num = dt * base.def.DiamondConsumptionRate;
			float massAvailable = storage.GetMassAvailable(base.def.DiamondTag);
			float num2 = Mathf.Min(num, massAvailable);
			float num3 = num2 / num;
			float num4 = dt / base.def.WorkDuration * num3;
			storage.ConsumeIgnoringDisease(base.def.DiamondTag, num2);
			float drillProgress = DrillProgress;
			drillProgress += num4;
			base.sm.DrillProgress.Set(drillProgress, this);
			bool result = !HasAnyDiamond || DrillProgress >= 1f;
			UpdateDiamondMeter();
			return result;
		}

		public void UpdateDiamondMeter()
		{
			if (diamondMeter != null)
			{
				float positionPercent = (IsOff ? 0f : (storage.MassStored() / storage.Capacity()));
				diamondMeter.SetPositionPercent(positionPercent);
			}
		}

		public void UnblockVent()
		{
			if (vent != null)
			{
				vent.Unblock();
			}
		}

		public void CreateProgressBar()
		{
			progressBar = ProgressBar.CreateProgressBar(base.gameObject, () => DrillProgress, base.def.ProgressBarOffset);
			progressBar.SetVisibility(visible: true);
		}

		public void ClearProgressBar()
		{
			if (progressBar != null)
			{
				Util.KDestroyGameObject(progressBar.gameObject);
				progressBar = null;
			}
		}
	}

	private const string OFF_ANIM_NAME = "idle";

	private const string IDLE_ANIM_NAME = "idle";

	private const string PRE_ANIM_NAME = "working_pre";

	private const string LOOP_ANIM_NAME = "working_loop";

	private const string PST_ANIM_NAME = "working_pst";

	private const string VENT_PRE_ANIM_NAME = "drill_pre";

	private const string VENT_LOOP_ANIM_NAME = "drill_loop";

	private const string VENT_PST_ANIM_NAME = "drill_pst";

	private const string METER_TARGET_NAME = "target_meter";

	private const string METER_ANIM_NAME = "meter";

	public State noOperational;

	public OperationalStates operational;

	public FloatParameter DrillProgress = new FloatParameter(0f);

	public TargetParameter Vent;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = noOperational;
		noOperational.TagTransition(GameTags.Operational, operational).Enter(UpdateDiamondMeter).PlayAnim("idle");
		operational.Enter(UpdateDiamondMeter).DefaultState(operational.idle);
		operational.idle.Target(Vent).EventTransition(GameHashes.VentBlocked, operational.working, CanWork).Target(masterTarget)
			.TagTransition(GameTags.Operational, noOperational, on_remove: true)
			.EventTransition(GameHashes.OnStorageChange, operational.missingDiamonds, GameStateMachine<UnderwaterVentDrill, Instance, IStateMachineTarget, Def>.Not(HasAnyDiamond))
			.PlayAnim("idle")
			.ToggleStatusItem(Db.Get().BuildingStatusItems.UnderwaterDrillIdle);
		operational.missingDiamonds.TagTransition(GameTags.Operational, noOperational, on_remove: true).EventTransition(GameHashes.OnStorageChange, operational.idle, HasAnyDiamond).EventHandler(GameHashes.OnStorageChange, UpdateDiamondMeter)
			.PlayAnim("idle");
		operational.working.ToggleStatusItem(Db.Get().BuildingStatusItems.UnderwaterDrillActive).Toggle("HeatProduction", EnableHeatProduction, DisableHeatProduction).DefaultState(operational.working.pre);
		operational.working.pre.Target(Vent).PlayAnim("drill_pre").Target(masterTarget)
			.PlayAnim("working_pre")
			.OnAnimQueueComplete(operational.working.loop);
		operational.working.loop.Target(Vent).PlayAnim("drill_loop", KAnim.PlayMode.Loop).ToggleStatusItem(Db.Get().MiscStatusItems.UnderwaterVentBeingDrilled)
			.Target(masterTarget)
			.TagTransition(GameTags.Operational, operational.working.pst, on_remove: true)
			.UpdateTransition(operational.working.pst, DrillUpdate)
			.Toggle("ToggleProgressBar", CreateProgressBar, ClearProgressBar)
			.PlayAnim("working_loop", KAnim.PlayMode.Loop);
		operational.working.pst.Target(Vent).PlayAnim("drill_pst").Target(masterTarget)
			.PlayAnim("working_pst")
			.OnAnimQueueComplete(operational.workEnded);
		operational.workEnded.ParamTransition(DrillProgress, operational.completed, GameStateMachine<UnderwaterVentDrill, Instance, IStateMachineTarget, Def>.IsGTEOne).GoTo(operational.missingDiamonds);
		operational.completed.Enter(ResetDrillProgress).Enter(UnblockVent).EnterGoTo(operational.idle);
	}

	private static void EnableHeatProduction(Instance smi)
	{
		smi.SetOperationalActiveFlag(active: true);
	}

	private static void DisableHeatProduction(Instance smi)
	{
		smi.SetOperationalActiveFlag(active: false);
	}

	private static void ResetDrillProgress(Instance smi)
	{
		smi.sm.DrillProgress.Set(0f, smi);
	}

	private static void UnblockVent(Instance smi)
	{
		smi.UnblockVent();
	}

	private static void CreateProgressBar(Instance smi)
	{
		smi.CreateProgressBar();
	}

	private static void ClearProgressBar(Instance smi)
	{
		smi.ClearProgressBar();
	}

	private static bool DrillUpdate(Instance smi, float dt)
	{
		return smi.DrillUpdate(dt);
	}

	private static bool CanWork(Instance smi)
	{
		return smi.CanWork;
	}

	private static bool HasAnyDiamond(Instance smi)
	{
		return smi.HasAnyDiamond;
	}

	private static void UpdateDiamondMeter(Instance smi)
	{
		smi.UpdateDiamondMeter();
	}
}
