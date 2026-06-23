using System;
using KSerialization;
using STRINGS;
using UnityEngine;

public class FixedCapturePoint : GameStateMachine<FixedCapturePoint, FixedCapturePoint.Instance, IStateMachineTarget, FixedCapturePoint.Def>
{
	public class Def : BaseDef
	{
		public Func<Instance, FixedCapturableMonitor.Instance, bool> isAmountStoredOverCapacity;

		public Func<Instance, int> getTargetCapturePoint = delegate(Instance smi)
		{
			int num = Grid.PosToCell(smi);
			Navigator navigator = smi.targetCapturable.Navigator;
			if (Grid.IsValidCell(num - 1) && navigator.CanReach(num - 1))
			{
				return num - 1;
			}
			return (Grid.IsValidCell(num + 1) && navigator.CanReach(num + 1)) ? (num + 1) : num;
		};

		public bool allowBabies;

		public CellOffset captureCellOffset = new CellOffset(0, 0);

		public CellOffset rancherInteractOffset = new CellOffset(0, 0);

		public HashedString logicPortId = "CritterPickUpInput";

		public CellOffset? postCaptureOffset;

		public string preCaptureAnimName;

		public Func<Instance, string> getPreCaptureAnimSuffix;

		public string offAnimName;

		public string onAnimName;
	}

	public class OperationalState : State
	{
		public State manual;

		public State automated;
	}

	public class UnoperationalStates : State
	{
		public State noOperational;

		public State strawBlocked;

		public State noLiquidOnStraw;
	}

	[SerializationConfig(MemberSerialization.OptIn)]
	public new class Instance : GameInstance
	{
		public bool isCurrentlyCapturingCreature;

		public BaggableCritterCapacityTracker critterCapactiy;

		private int captureCell;

		private Operational operationComp;

		private LogicPorts logicPorts;

		public bool IsOperational
		{
			get
			{
				if (operationComp != null)
				{
					return operationComp.IsOperational;
				}
				return false;
			}
		}

		public bool IsStrawInstalled => Straw != null;

		public bool IsStrawOutsideLiquid
		{
			get
			{
				if (IsStrawInstalled)
				{
					return !Straw.isInLiquid;
				}
				return false;
			}
		}

		public bool IsStrawBlocked
		{
			get
			{
				if (IsStrawInstalled)
				{
					return Straw.currentDepth <= 0;
				}
				return false;
			}
		}

		public FixedCapturableMonitor.Instance targetCapturable { get; private set; }

		public bool shouldCreatureGoGetCaptured { get; private set; }

		public BuildingPointStraw Straw { get; private set; }

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			Subscribe(-905833192, OnCopySettings);
			captureCell = Grid.OffsetCell(Grid.PosToCell(base.transform.GetPosition()), def.captureCellOffset);
			critterCapactiy = GetComponent<BaggableCritterCapacityTracker>();
			Straw = GetComponent<BuildingPointStraw>();
			operationComp = GetComponent<Operational>();
			logicPorts = GetComponent<LogicPorts>();
			if (logicPorts != null)
			{
				Subscribe(-801688580, OnLogicEvent);
				operationComp.SetFlag(enabledFlag, !logicPorts.IsPortConnected(def.logicPortId) || logicPorts.GetInputValue(def.logicPortId) > 0);
			}
			else
			{
				operationComp.SetFlag(enabledFlag, value: true);
			}
		}

		public int GetRancherInteractCell()
		{
			return Grid.OffsetCell(Grid.PosToCell(base.transform.GetPosition()), base.def.rancherInteractOffset);
		}

		private void OnLogicEvent(object data)
		{
			LogicValueChanged logicValueChanged = (LogicValueChanged)data;
			if (logicValueChanged.portID == base.def.logicPortId && logicPorts.IsPortConnected(base.def.logicPortId))
			{
				operationComp.SetFlag(enabledFlag, logicValueChanged.newValue > 0);
			}
		}

		public void PlayOnOffAnim()
		{
			string text = ((Straw != null) ? Straw.GetAnimSuffix() : "");
			string text2 = ((!ShouldBeOn(this)) ? ((base.def.offAnimName != null) ? (base.def.offAnimName + text) : null) : ((base.def.onAnimName != null) ? (base.def.onAnimName + text) : null));
			if (!string.IsNullOrEmpty(text2))
			{
				GetComponent<KBatchedAnimController>().Play(text2);
			}
		}

		public override void StartSM()
		{
			base.StartSM();
			if (GetComponent<AutoWrangleCapture>() == null)
			{
				base.sm.automated.Set(value: true, this);
			}
		}

		private void OnCopySettings(object data)
		{
			GameObject gameObject = (GameObject)data;
			if (!(gameObject == null))
			{
				Instance sMI = gameObject.GetSMI<Instance>();
				if (sMI != null)
				{
					base.sm.automated.Set(base.sm.automated.Get(sMI), this);
				}
			}
		}

		public bool GetAutomated()
		{
			return base.sm.automated.Get(this);
		}

		public void SetAutomated(bool automate)
		{
			base.sm.automated.Set(automate, this);
		}

		public Chore CreateChore()
		{
			FindFixedCapturable();
			return new FixedCaptureChore(GetComponent<KPrefabID>());
		}

		public bool IsCreatureAvailableForFixedCapture()
		{
			if (!targetCapturable.IsNullOrStopped())
			{
				CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(captureCell);
				return CanCapturableBeCapturedAtCapturePoint(targetCapturable, this, cavityForCell, captureCell);
			}
			return false;
		}

		public void SetRancherIsAvailableForCapturing()
		{
			shouldCreatureGoGetCaptured = true;
		}

		public void ClearRancherIsAvailableForCapturing()
		{
			shouldCreatureGoGetCaptured = false;
		}

		private static bool CanCapturableBeCapturedAtCapturePoint(FixedCapturableMonitor.Instance capturable, Instance capture_point, CavityInfo capture_cavity_info, int capture_cell)
		{
			if (!capturable.IsRunning())
			{
				return false;
			}
			if (capturable.targetCapturePoint != capture_point && !capturable.targetCapturePoint.IsNullOrStopped())
			{
				return false;
			}
			int cell = Grid.PosToCell(capturable.transform.GetPosition());
			CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(cell);
			if (cavityForCell == null || cavityForCell != capture_cavity_info)
			{
				return false;
			}
			if (capturable.HasTag(GameTags.Creatures.Bagged))
			{
				return false;
			}
			if (capturable.isBaby && !capture_point.def.allowBabies)
			{
				return false;
			}
			if (!capturable.ChoreConsumer.IsChoreEqualOrAboveCurrentChorePriority<FixedCaptureStates>())
			{
				return false;
			}
			if (capturable.Navigator.GetNavigationCost(capture_cell) == -1)
			{
				return false;
			}
			return capture_point.def.isAmountStoredOverCapacity(capture_point, capturable);
		}

		public void FindFixedCapturable()
		{
			CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(captureCell);
			if (cavityForCell == null)
			{
				ResetCapturePoint();
				return;
			}
			if (!targetCapturable.IsNullOrStopped() && !isCurrentlyCapturingCreature && !CanCapturableBeCapturedAtCapturePoint(targetCapturable, this, cavityForCell, captureCell))
			{
				ResetCapturePoint();
			}
			if (!targetCapturable.IsNullOrStopped())
			{
				return;
			}
			foreach (FixedCapturableMonitor.Instance fixedCapturableMonitor in Components.FixedCapturableMonitors)
			{
				if (CanCapturableBeCapturedAtCapturePoint(fixedCapturableMonitor, this, cavityForCell, captureCell))
				{
					targetCapturable = fixedCapturableMonitor;
					if (!targetCapturable.IsNullOrStopped())
					{
						targetCapturable.targetCapturePoint = this;
					}
					break;
				}
			}
		}

		public void UpdateCaptureCell(CellOffset offset)
		{
			captureCell = Grid.OffsetCell(Grid.PosToCell(base.transform.GetPosition()), offset);
		}

		public void ResetCapturePoint()
		{
			Trigger(643180843);
			if (!targetCapturable.IsNullOrStopped())
			{
				targetCapturable.targetCapturePoint = null;
				targetCapturable.Trigger(1034952693);
				targetCapturable = null;
			}
		}
	}

	public class AutoWrangleCapture : KMonoBehaviour, ICheckboxControl
	{
		private Instance fcp;

		string ICheckboxControl.CheckboxTitleKey => UI.UISIDESCREENS.CAPTURE_POINT_SIDE_SCREEN.TITLE.key.String;

		string ICheckboxControl.CheckboxLabel => UI.UISIDESCREENS.CAPTURE_POINT_SIDE_SCREEN.AUTOWRANGLE;

		string ICheckboxControl.CheckboxTooltip => UI.UISIDESCREENS.CAPTURE_POINT_SIDE_SCREEN.AUTOWRANGLE_TOOLTIP;

		protected override void OnSpawn()
		{
			base.OnSpawn();
			fcp = this.GetSMI<Instance>();
		}

		bool ICheckboxControl.GetCheckboxValue()
		{
			return fcp.GetAutomated();
		}

		void ICheckboxControl.SetCheckboxValue(bool value)
		{
			fcp.SetAutomated(value);
		}
	}

	public static readonly Operational.Flag enabledFlag = new Operational.Flag("enabled", Operational.Flag.Type.Requirement);

	private BoolParameter automated;

	public UnoperationalStates unoperational;

	public OperationalState operational;

	public override void InitializeStates(out BaseState default_state)
	{
		default_state = operational;
		base.serializable = SerializeType.Both_DEPRECATED;
		unoperational.EventTransition(GameHashes.OperationalChanged, operational, ShouldBeOn).EventTransition(GameHashes.BuildingStrawChange, operational, ShouldBeOn).EventHandler(GameHashes.BuildingStrawChange, HandleBuildingStrawChange)
			.Enter(Refresh)
			.DefaultState(unoperational.noOperational);
		unoperational.noOperational.EventTransition(GameHashes.OperationalChanged, unoperational.strawBlocked, (Instance smi) => IsOperational(smi) && IsStrawBlocked(smi)).EventTransition(GameHashes.OperationalChanged, unoperational.noLiquidOnStraw, (Instance smi) => IsOperational(smi) && IsStrawOutsideLiquid(smi));
		unoperational.strawBlocked.ToggleStatusItem(Db.Get().BuildingStatusItems.OutputTileBlocked).EventTransition(GameHashes.OperationalChanged, unoperational.noOperational, GameStateMachine<FixedCapturePoint, Instance, IStateMachineTarget, Def>.Not(IsOperational)).EventTransition(GameHashes.BuildingStrawChange, unoperational.noLiquidOnStraw, (Instance smi) => !IsStrawBlocked(smi) && IsStrawOutsideLiquid(smi));
		unoperational.noLiquidOnStraw.ToggleStatusItem(Db.Get().BuildingStatusItems.NotSubmerged).EventTransition(GameHashes.OperationalChanged, unoperational.noOperational, GameStateMachine<FixedCapturePoint, Instance, IStateMachineTarget, Def>.Not(IsOperational)).EventTransition(GameHashes.BuildingStrawChange, unoperational.strawBlocked, IsStrawBlocked);
		operational.DefaultState(operational.manual).EventTransition(GameHashes.OperationalChanged, unoperational, GameStateMachine<FixedCapturePoint, Instance, IStateMachineTarget, Def>.Not(ShouldBeOn)).EventTransition(GameHashes.BuildingStrawChange, unoperational, GameStateMachine<FixedCapturePoint, Instance, IStateMachineTarget, Def>.Not(ShouldBeOn))
			.EventHandler(GameHashes.BuildingStrawChange, HandleBuildingStrawChange)
			.Enter(Refresh);
		operational.manual.ParamTransition(automated, operational.automated, GameStateMachine<FixedCapturePoint, Instance, IStateMachineTarget, Def>.IsTrue);
		operational.automated.ParamTransition(automated, operational.manual, GameStateMachine<FixedCapturePoint, Instance, IStateMachineTarget, Def>.IsFalse).ToggleChore((Instance smi) => smi.CreateChore(), unoperational, unoperational).Update("FindFixedCapturable", delegate(Instance smi, float dt)
		{
			smi.FindFixedCapturable();
		}, UpdateRate.SIM_1000ms);
	}

	public static bool ShouldBeOn(Instance smi)
	{
		if (IsOperational(smi) && !IsStrawBlocked(smi))
		{
			return !IsStrawOutsideLiquid(smi);
		}
		return false;
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

	public static void HandleBuildingStrawChange(Instance smi, object o)
	{
		Refresh(smi);
	}

	public static void Refresh(Instance smi)
	{
		if (smi.IsStrawInstalled)
		{
			smi.UpdateCaptureCell(smi.Straw.GetBottomCellOffset());
		}
		smi.PlayOnOffAnim();
	}
}
