using System.Collections.Generic;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/Workable/Door")]
public class Door : Workable, ISaveLoadable, ISim200ms, INavDoor
{
	public enum DoorType
	{
		Pressure,
		ManualPressure,
		Internal,
		Sealed
	}

	public enum ControlState
	{
		Auto,
		Opened,
		Locked,
		NumStates
	}

	public class Controller : GameStateMachine<Controller, Controller.Instance, Door>
	{
		public class SealedStates : State
		{
			public class AwaitingUnlock : State
			{
				public State awaiting_arrival;

				public State unlocking;
			}

			public State closed;

			public AwaitingUnlock awaiting_unlock;

			public State chore_pst;
		}

		public new class Instance : GameInstance
		{
			[MyCmpReq]
			public Building building;

			public Instance(Door door)
				: base(door)
			{
			}

			public void RefreshIsBlocked()
			{
				bool value = false;
				int[] placementCells = building.PlacementCells;
				foreach (int cell in placementCells)
				{
					if (Grid.Objects[cell, 40] != null)
					{
						value = true;
						break;
					}
				}
				base.sm.isBlocked.Set(value, base.smi);
			}
		}

		public State open;

		public State opening;

		public State closed;

		public State closing;

		public State closedelay;

		public State closeblocked;

		public State locking;

		public State locked;

		public State unlocking;

		public SealedStates Sealed;

		public BoolParameter isOpen;

		public BoolParameter isLocked;

		public BoolParameter isBlocked;

		public BoolParameter isSealed;

		public BoolParameter sealDirectionRight;

		public override void InitializeStates(out BaseState default_state)
		{
			base.serializable = SerializeType.Both_DEPRECATED;
			default_state = closed;
			root.Update("RefreshIsBlocked", delegate(Instance smi, float dt)
			{
				smi.RefreshIsBlocked();
			}).ParamTransition(isSealed, Sealed.closed, GameStateMachine<Controller, Instance, Door, object>.IsTrue);
			closeblocked.PlayAnim("open").ParamTransition(isOpen, open, GameStateMachine<Controller, Instance, Door, object>.IsTrue).ParamTransition(isBlocked, closedelay, GameStateMachine<Controller, Instance, Door, object>.IsFalse);
			closedelay.PlayAnim("open").ScheduleGoTo(0.5f, closing).ParamTransition(isOpen, open, GameStateMachine<Controller, Instance, Door, object>.IsTrue)
				.ParamTransition(isBlocked, closeblocked, GameStateMachine<Controller, Instance, Door, object>.IsTrue);
			closing.ParamTransition(isBlocked, closeblocked, GameStateMachine<Controller, Instance, Door, object>.IsTrue).ToggleTag(GameTags.Transition).ToggleLoopingSound("Closing loop", (Instance smi) => smi.master.doorClosingSound, (Instance smi) => !string.IsNullOrEmpty(smi.master.doorClosingSound))
				.Enter("SetParams", delegate(Instance smi)
				{
					smi.master.UpdateAnimAndSoundParams(smi.master.on);
				})
				.Update(delegate(Instance smi, float dt)
				{
					if (smi.master.doorClosingSound != null)
					{
						smi.master.loopingSounds.UpdateSecondParameter(smi.master.doorClosingSound, SOUND_PROGRESS_PARAMETER, smi.Get<KBatchedAnimController>().GetPositionPercent());
					}
				}, UpdateRate.SIM_33ms)
				.Enter("SetActive", delegate(Instance smi)
				{
					smi.master.SetActive(active: true);
				})
				.Exit("SetActive", delegate(Instance smi)
				{
					smi.master.SetActive(active: false);
				})
				.PlayAnim("closing")
				.OnAnimQueueComplete(closed);
			open.PlayAnim("open").ParamTransition(isOpen, closeblocked, GameStateMachine<Controller, Instance, Door, object>.IsFalse).Enter("SetWorldStateOpen", delegate(Instance smi)
			{
				smi.master.SetWorldState(updateSim: true);
			})
				.Enter(RemoveWaterproof);
			closed.Enter(SetWaterproof).PlayAnim("closed").ParamTransition(isOpen, opening, GameStateMachine<Controller, Instance, Door, object>.IsTrue)
				.ParamTransition(isLocked, locking, GameStateMachine<Controller, Instance, Door, object>.IsTrue)
				.Enter("SetWorldStateClosed", delegate(Instance smi)
				{
					smi.master.SetWorldState(updateSim: true);
				});
			locking.PlayAnim("locked_pre").OnAnimQueueComplete(locked);
			locked.Enter(SetWaterproof).PlayAnim("locked").ParamTransition(isLocked, unlocking, GameStateMachine<Controller, Instance, Door, object>.IsFalse)
				.Enter("SetWorldStateLocked", delegate(Instance smi)
				{
					smi.master.SetWorldState(updateSim: true);
				});
			unlocking.PlayAnim("locked_pst").OnAnimQueueComplete(closed);
			opening.ToggleTag(GameTags.Transition).ToggleLoopingSound("Opening loop", (Instance smi) => smi.master.doorOpeningSound, (Instance smi) => !string.IsNullOrEmpty(smi.master.doorOpeningSound)).Enter("SetParams", delegate(Instance smi)
			{
				smi.master.UpdateAnimAndSoundParams(smi.master.on);
			})
				.Update(delegate(Instance smi, float dt)
				{
					if (smi.master.doorOpeningSound != null)
					{
						smi.master.loopingSounds.UpdateSecondParameter(smi.master.doorOpeningSound, SOUND_PROGRESS_PARAMETER, smi.Get<KBatchedAnimController>().GetPositionPercent());
					}
				}, UpdateRate.SIM_33ms)
				.Enter("SetActive", delegate(Instance smi)
				{
					smi.master.SetActive(active: true);
				})
				.Exit("SetActive", delegate(Instance smi)
				{
					smi.master.SetActive(active: false);
				})
				.PlayAnim("opening")
				.OnAnimQueueComplete(open);
			Sealed.Enter(delegate(Instance smi)
			{
				OccupyArea component = smi.master.GetComponent<OccupyArea>();
				for (int i = 0; i < component.OccupiedCellsOffsets.Length; i++)
				{
					Grid.PreventFogOfWarReveal[Grid.OffsetCell(Grid.PosToCell(smi.master.gameObject), component.OccupiedCellsOffsets[i])] = false;
				}
				smi.sm.isLocked.Set(value: true, smi);
				smi.master.controlState = ControlState.Locked;
				smi.master.RefreshControlState();
				if (smi.master.GetComponent<Unsealable>().facingRight)
				{
					smi.master.GetComponent<KBatchedAnimController>().FlipX = true;
				}
			}).Enter(SetWaterproof).Enter("SetWorldStateClosed", delegate(Instance smi)
			{
				smi.master.SetWorldState(updateSim: true);
			})
				.Exit(delegate(Instance smi)
				{
					smi.sm.isLocked.Set(value: false, smi);
					smi.master.GetComponent<AccessControl>().controlEnabled = true;
					smi.master.controlState = ControlState.Opened;
					smi.master.RefreshControlState();
					smi.sm.isOpen.Set(value: true, smi);
					smi.sm.isLocked.Set(value: false, smi);
					smi.sm.isSealed.Set(value: false, smi);
				})
				.Exit(RemoveWaterproof);
			Sealed.closed.PlayAnim("sealed", KAnim.PlayMode.Once);
			Sealed.awaiting_unlock.ToggleChore((Instance smi) => CreateUnsealChore(smi, approach_right: true), Sealed.chore_pst);
			Sealed.chore_pst.Enter(delegate(Instance smi)
			{
				smi.master.hasBeenUnsealed = true;
				if (smi.master.GetComponent<Unsealable>().unsealed)
				{
					smi.GoTo(opening);
					FogOfWarMask.ClearMask(Grid.CellRight(Grid.PosToCell(smi.master.gameObject)));
					FogOfWarMask.ClearMask(Grid.CellLeft(Grid.PosToCell(smi.master.gameObject)));
				}
				else
				{
					smi.GoTo(Sealed.closed);
				}
			});
		}

		private void SetWaterproof(Instance smi)
		{
			if (!DisplacesGas(smi.master.doorType))
			{
				return;
			}
			smi.master.animController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.WaterProof, isActive: true);
			if (!smi.master.waterProofAffectsFGLayers)
			{
				return;
			}
			KBatchedAnimController[] componentsInChildrenOnly = smi.master.gameObject.GetComponentsInChildrenOnly<KBatchedAnimController>();
			foreach (KBatchedAnimController kBatchedAnimController in componentsInChildrenOnly)
			{
				if (kBatchedAnimController.name.Contains("_fg"))
				{
					kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.WaterProof, isActive: true);
				}
			}
		}

		private void RemoveWaterproof(Instance smi)
		{
			smi.master.animController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.WaterProof, isActive: false);
			if (!smi.master.waterProofAffectsFGLayers)
			{
				return;
			}
			KBatchedAnimController[] componentsInChildrenOnly = smi.master.gameObject.GetComponentsInChildrenOnly<KBatchedAnimController>();
			foreach (KBatchedAnimController kBatchedAnimController in componentsInChildrenOnly)
			{
				if (kBatchedAnimController.name.Contains("_fg"))
				{
					kBatchedAnimController.SetBlendValue(KBatchedAnimInstanceData.BlendActiveOptions.WaterProof, isActive: false);
				}
			}
		}

		private Chore CreateUnsealChore(Instance smi, bool approach_right)
		{
			return new WorkChore<Unsealable>(Db.Get().ChoreTypes.Toggle, smi.master);
		}
	}

	[MyCmpReq]
	private Operational operational;

	[MyCmpGet]
	private Rotatable rotatable;

	[MyCmpReq]
	private KBatchedAnimController animController;

	[MyCmpReq]
	public Building building;

	[MyCmpGet]
	private EnergyConsumer consumer;

	[MyCmpAdd]
	private LoopingSounds loopingSounds;

	public Orientation verticalOrientation;

	[SerializeField]
	public bool hasComplexUserControls;

	[SerializeField]
	public float unpoweredAnimSpeed = 0.25f;

	[SerializeField]
	public float poweredAnimSpeed = 1f;

	[SerializeField]
	public DoorType doorType;

	[SerializeField]
	public bool allowAutoControl = true;

	[SerializeField]
	public string doorClosingSoundEventName;

	[SerializeField]
	public string doorOpeningSoundEventName;

	public float insulationModifier = 1f;

	public bool waterProofAffectsFGLayers;

	private string doorClosingSound;

	private string doorOpeningSound;

	private static readonly HashedString SOUND_POWERED_PARAMETER = "doorPowered";

	private static readonly HashedString SOUND_PROGRESS_PARAMETER = "doorProgress";

	[Serialize]
	private bool hasBeenUnsealed;

	[Serialize]
	private ControlState controlState;

	private bool on;

	private bool do_melt_check;

	private int openCount;

	private const ControlState INVALID_CONTROL_STATE = ControlState.NumStates;

	[Serialize]
	private ControlState requestedState = ControlState.NumStates;

	private Chore changeStateChore;

	private Controller.Instance controller;

	private LoggerFSS log;

	private const float REFRESH_HACK_DELAY = 1f;

	private bool doorOpenLiquidRefreshHack;

	private float doorOpenLiquidRefreshTime;

	private static readonly EventSystem.IntraObjectHandler<Door> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<Door>(delegate(Door component, object data)
	{
		component.OnCopySettings(data);
	});

	public static readonly HashedString OPEN_CLOSE_PORT_ID = new HashedString("DoorOpenClose");

	private static readonly KAnimFile[] OVERRIDE_ANIMS = new KAnimFile[1] { Assets.GetAnim("anim_use_remote_kanim") };

	private static readonly EventSystem.IntraObjectHandler<Door> OnOperationalChangedDelegate = new EventSystem.IntraObjectHandler<Door>(delegate(Door component, object data)
	{
		component.OnOperationalChanged(data);
	});

	private static readonly EventSystem.IntraObjectHandler<Door> OnLogicValueChangedDelegate = new EventSystem.IntraObjectHandler<Door>(delegate(Door component, object data)
	{
		component.OnLogicValueChanged(data);
	});

	private bool applyLogicChange;

	public ControlState CurrentState => controlState;

	public ControlState RequestedState => requestedState;

	public bool ShouldBlockFallingSand => rotatable.GetOrientation() != verticalOrientation;

	public bool isSealed
	{
		get
		{
			if (controller != null)
			{
				return controller.sm.isSealed.Get(controller);
			}
			return false;
		}
	}

	private void OnCopySettings(object data)
	{
		Door component = ((GameObject)data).GetComponent<Door>();
		if (component != null)
		{
			QueueStateChange(component.requestedState);
		}
	}

	public Door()
	{
		SetOffsetTable(OffsetGroups.InvertedStandardTable);
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		overrideAnims = OVERRIDE_ANIMS;
		synchronizeAnims = false;
		requestedState = ControlState.NumStates;
		SetWorkTime(3f);
		if (!string.IsNullOrEmpty(doorClosingSoundEventName))
		{
			doorClosingSound = GlobalAssets.GetSound(doorClosingSoundEventName);
		}
		if (!string.IsNullOrEmpty(doorOpeningSoundEventName))
		{
			doorOpeningSound = GlobalAssets.GetSound(doorOpeningSoundEventName);
		}
		Subscribe(-905833192, OnCopySettingsDelegate);
	}

	private ControlState GetNextState(ControlState wantedState)
	{
		return (ControlState)((int)(wantedState + 1) % 3);
	}

	private static bool DisplacesGas(DoorType type)
	{
		return type != DoorType.Internal;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (GetComponent<KPrefabID>() != null)
		{
			log = new LoggerFSS("Door");
		}
		if (!allowAutoControl && controlState == ControlState.Auto)
		{
			controlState = ControlState.Locked;
		}
		StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
		HandleVector<int>.Handle handle = structureTemperatures.GetHandle(base.gameObject);
		if (DisplacesGas(doorType))
		{
			structureTemperatures.Bypass(handle);
		}
		controller = new Controller.Instance(this);
		controller.StartSM();
		if (doorType == DoorType.Sealed && !hasBeenUnsealed)
		{
			Seal();
		}
		UpdateDoorSpeed(operational.IsOperational);
		Subscribe(-592767678, OnOperationalChangedDelegate);
		Subscribe(824508782, OnOperationalChangedDelegate);
		Subscribe(-801688580, OnLogicValueChangedDelegate);
		ApplyControlState();
		if (requestedState != ControlState.NumStates && requestedState != controlState)
		{
			ControlState nextState = requestedState;
			requestedState = controlState;
			QueueStateChange(nextState);
		}
		else
		{
			requestedState = controlState;
		}
		int num = ((rotatable.GetOrientation() == Orientation.Neutral) ? (building.Def.WidthInCells * (building.Def.HeightInCells - 1)) : 0);
		int num2 = ((rotatable.GetOrientation() == Orientation.Neutral) ? building.Def.WidthInCells : building.Def.HeightInCells);
		for (int i = 0; i != num2; i++)
		{
			int num3 = building.PlacementCells[num + i];
			Grid.FakeFloor.Add(num3);
			Pathfinding.Instance.AddDirtyNavGridCell(num3);
		}
		int[] placementCells = building.PlacementCells;
		foreach (int num4 in placementCells)
		{
			Grid.HasDoor[num4] = true;
			SimMessages.SetCellProperties(num4, 8);
			if (DisplacesGas(doorType))
			{
				Grid.RenderedByWorld[num4] = false;
			}
		}
	}

	protected override void OnCleanUp()
	{
		UpdateDoorState(cleaningUp: true);
		int[] placementCells = building.PlacementCells;
		foreach (int num in placementCells)
		{
			if (insulationModifier != 1f)
			{
				SimMessages.SetInsulation(num, 1f);
			}
			SimMessages.ClearCellProperties(num, 12);
			Grid.RenderedByWorld[num] = Grid.Element[num].substance.renderedByWorld;
			Grid.FakeFloor.Remove(num);
			if (Grid.Element[num].IsSolid)
			{
				SimMessages.ReplaceAndDisplaceElement(num, SimHashes.Vacuum, CellEventLogger.Instance.DoorOpen, 0f);
			}
			Pathfinding.Instance.AddDirtyNavGridCell(num);
		}
		placementCells = building.PlacementCells;
		foreach (int num2 in placementCells)
		{
			Grid.HasDoor[num2] = false;
			Game.Instance.SetDupePassableSolid(num2, passable: false, Grid.Solid[num2]);
			Grid.CritterImpassable[num2] = false;
			Grid.DupeImpassable[num2] = false;
			Pathfinding.Instance.AddDirtyNavGridCell(num2);
		}
		base.OnCleanUp();
	}

	public void Seal()
	{
		controller.sm.isSealed.Set(value: true, controller);
	}

	public void OrderUnseal()
	{
		controller.GoTo(controller.sm.Sealed.awaiting_unlock);
	}

	private void RefreshControlState()
	{
		switch (controlState)
		{
		case ControlState.Auto:
			controller.sm.isLocked.Set(value: false, controller);
			break;
		case ControlState.Opened:
			controller.sm.isLocked.Set(value: false, controller);
			break;
		case ControlState.Locked:
			controller.sm.isLocked.Set(value: true, controller);
			break;
		}
		BoxingTrigger(279163026, controlState);
		SetWorldState(updateSim: false);
		GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().BuildingStatusItems.CurrentDoorControlState, this);
	}

	private void OnOperationalChanged(object _)
	{
		bool isOperational = operational.IsOperational;
		if (isOperational != on)
		{
			UpdateDoorSpeed(isOperational);
			if (on && GetComponent<KPrefabID>().HasTag(GameTags.Transition))
			{
				SetActive(active: true);
			}
			else
			{
				SetActive(active: false);
			}
		}
	}

	private void UpdateDoorSpeed(bool powered)
	{
		on = powered;
		UpdateAnimAndSoundParams(powered);
		float positionPercent = animController.GetPositionPercent();
		animController.Play(animController.CurrentAnim.hash, animController.PlayMode);
		animController.SetPositionPercent(positionPercent);
	}

	private void UpdateAnimAndSoundParams(bool powered)
	{
		if (powered)
		{
			animController.PlaySpeedMultiplier = poweredAnimSpeed;
			if (doorClosingSound != null)
			{
				loopingSounds.UpdateFirstParameter(doorClosingSound, SOUND_POWERED_PARAMETER, 1f);
			}
			if (doorOpeningSound != null)
			{
				loopingSounds.UpdateFirstParameter(doorOpeningSound, SOUND_POWERED_PARAMETER, 1f);
			}
		}
		else
		{
			animController.PlaySpeedMultiplier = unpoweredAnimSpeed;
			if (doorClosingSound != null)
			{
				loopingSounds.UpdateFirstParameter(doorClosingSound, SOUND_POWERED_PARAMETER, 0f);
			}
			if (doorOpeningSound != null)
			{
				loopingSounds.UpdateFirstParameter(doorOpeningSound, SOUND_POWERED_PARAMETER, 0f);
			}
		}
	}

	private void SetActive(bool active)
	{
		if (operational.IsOperational)
		{
			operational.SetActive(active);
		}
	}

	private void SetWorldState(bool updateSim)
	{
		int[] placementCells = building.PlacementCells;
		bool is_door_open = IsOpen();
		SetPassableState(is_door_open, placementCells);
		if (updateSim)
		{
			SetSimState(is_door_open, placementCells);
		}
	}

	private void SetPassableState(bool is_door_open, IList<int> cells)
	{
		for (int i = 0; i < cells.Count; i++)
		{
			int num = cells[i];
			switch (doorType)
			{
			case DoorType.Pressure:
			case DoorType.ManualPressure:
			case DoorType.Sealed:
			{
				Grid.CritterImpassable[num] = controlState != ControlState.Opened;
				bool solid = !is_door_open;
				bool passable = controlState != ControlState.Locked;
				Game.Instance.SetDupePassableSolid(num, passable, solid);
				if (controlState == ControlState.Opened)
				{
					doorOpenLiquidRefreshHack = true;
					doorOpenLiquidRefreshTime = 1f;
				}
				break;
			}
			case DoorType.Internal:
				Grid.CritterImpassable[num] = controlState != ControlState.Opened;
				Grid.DupeImpassable[num] = controlState == ControlState.Locked;
				break;
			}
			Pathfinding.Instance.AddDirtyNavGridCell(num);
		}
	}

	private void SetSimState(bool is_door_open, IList<int> cells)
	{
		PrimaryElement component = GetComponent<PrimaryElement>();
		float mass = component.Mass / (float)cells.Count;
		DoorType doorType = this.doorType;
		if ((uint)doorType > 1u && doorType != DoorType.Sealed)
		{
			return;
		}
		if (is_door_open)
		{
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			HandleVector<int>.Handle handle = structureTemperatures.GetHandle(base.gameObject);
			if (!handle.IsValid() || !structureTemperatures.IsBypassed(handle))
			{
				return;
			}
			float num = 0f;
			int num2 = 0;
			foreach (int cell in cells)
			{
				if (Grid.Mass[cell] > 0f)
				{
					num2++;
					num += Grid.Temperature[cell];
				}
				HandleVector<Game.CallbackInfo>.Handle handle2 = Game.Instance.callbackManager.Add(new Game.CallbackInfo(OnSimDoorOpened));
				if (insulationModifier != 1f)
				{
					SimMessages.SetInsulation(cell, 1f);
				}
				SimMessages.Dig(cell, handle2.index, skipEvent: true);
				if (ShouldBlockFallingSand)
				{
					SimMessages.ClearCellProperties(cell, 4);
				}
				else
				{
					SimMessages.SetCellProperties(cell, 4);
				}
				World.Instance.groundRenderer.MarkDirty(cell);
			}
			if (num2 > 0)
			{
				num /= (float)cells.Count;
				KCrashReporter.Assert(num > 0f, "Door has calculated an invalid temperature");
				component.Temperature = num;
			}
			return;
		}
		foreach (int cell2 in cells)
		{
			HandleVector<Game.CallbackInfo>.Handle handle3 = Game.Instance.callbackManager.Add(new Game.CallbackInfo(OnSimDoorClosed));
			float temperature = component.Temperature;
			if (temperature <= 0f)
			{
				temperature = component.Temperature;
			}
			SimMessages.ReplaceAndDisplaceElement(cell2, component.ElementID, CellEventLogger.Instance.DoorClose, mass, temperature, byte.MaxValue, 0, handle3.index);
			SimMessages.SetCellProperties(cell2, 4);
			if (insulationModifier != 1f)
			{
				SimMessages.SetInsulation(cell2, insulationModifier);
			}
			World.Instance.groundRenderer.MarkDirty(cell2);
		}
		StructureTemperatureComponents structureTemperatures2 = GameComps.StructureTemperatures;
		HandleVector<int>.Handle handle4 = structureTemperatures2.GetHandle(base.gameObject);
		if (handle4.IsValid() && !structureTemperatures2.IsBypassed(handle4))
		{
			float temperature2 = structureTemperatures2.GetPayload(handle4).Temperature;
			component.Temperature = temperature2;
		}
	}

	private void UpdateDoorState(bool cleaningUp)
	{
		int[] placementCells = building.PlacementCells;
		foreach (int num in placementCells)
		{
			if (Grid.IsValidCell(num))
			{
				Grid.Foundation[num] = !cleaningUp;
			}
		}
	}

	public void QueueStateChange(ControlState nextState)
	{
		if (requestedState != nextState)
		{
			requestedState = nextState;
		}
		else
		{
			requestedState = controlState;
		}
		if (requestedState == controlState)
		{
			if (changeStateChore != null)
			{
				changeStateChore.Cancel("Change state");
				changeStateChore = null;
				GetComponent<KSelectable>().RemoveStatusItem(Db.Get().BuildingStatusItems.ChangeDoorControlState);
			}
		}
		else if (DebugHandler.InstantBuildMode)
		{
			controlState = requestedState;
			RefreshControlState();
			OnOperationalChanged(null);
			GetComponent<KSelectable>().RemoveStatusItem(Db.Get().BuildingStatusItems.ChangeDoorControlState);
			Open();
			Close();
		}
		else
		{
			if (changeStateChore != null)
			{
				changeStateChore.Cancel("Change state");
			}
			GetComponent<KSelectable>().AddStatusItem(Db.Get().BuildingStatusItems.ChangeDoorControlState, this);
			changeStateChore = new WorkChore<Door>(Db.Get().ChoreTypes.Toggle, this, null, run_until_complete: true, null, null, null, allow_in_red_alert: true, null, ignore_schedule_block: false, only_when_operational: false);
		}
	}

	private void OnSimDoorOpened()
	{
		if (!(this == null) && DisplacesGas(doorType))
		{
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			HandleVector<int>.Handle handle = structureTemperatures.GetHandle(base.gameObject);
			structureTemperatures.UnBypass(handle);
			do_melt_check = false;
		}
	}

	private void OnSimDoorClosed()
	{
		if (!(this == null) && DisplacesGas(doorType))
		{
			StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
			HandleVector<int>.Handle handle = structureTemperatures.GetHandle(base.gameObject);
			structureTemperatures.Bypass(handle);
			do_melt_check = true;
		}
	}

	protected override void OnCompleteWork(WorkerBase worker)
	{
		base.OnCompleteWork(worker);
		changeStateChore = null;
		ApplyRequestedControlState();
	}

	public void Open()
	{
		openCount++;
		ControlState controlState = this.controlState;
		if ((uint)controlState > 1u)
		{
			_ = 2;
		}
		else
		{
			controller.sm.isOpen.Set(value: true, controller);
		}
	}

	public void Close()
	{
		openCount = Mathf.Max(0, openCount - 1);
		switch (controlState)
		{
		case ControlState.Locked:
			controller.sm.isOpen.Set(value: false, controller);
			break;
		case ControlState.Auto:
			if (openCount == 0)
			{
				controller.sm.isOpen.Set(value: false, controller);
				Game.Instance.userMenu.Refresh(base.gameObject);
			}
			break;
		case ControlState.Opened:
			break;
		}
	}

	public bool IsPendingClose()
	{
		return controller.IsInsideState(controller.sm.closedelay);
	}

	public bool IsOpen()
	{
		if (!controller.IsInsideState(controller.sm.open) && !controller.IsInsideState(controller.sm.closedelay))
		{
			return controller.IsInsideState(controller.sm.closeblocked);
		}
		return true;
	}

	private void ApplyControlState(bool force = false)
	{
		RefreshControlState();
		OnOperationalChanged(null);
		GetComponent<KSelectable>().RemoveStatusItem(Db.Get().BuildingStatusItems.ChangeDoorControlState);
		Trigger(1734268753, (object)this);
		if (!force)
		{
			Open();
			Close();
		}
	}

	private void ApplyRequestedControlState(bool force = false)
	{
		if (requestedState != controlState || force)
		{
			controlState = requestedState;
			ApplyControlState(force);
		}
	}

	public void OnLogicValueChanged(object data)
	{
		LogicValueChanged logicValueChanged = (LogicValueChanged)data;
		if (!(logicValueChanged.portID != OPEN_CLOSE_PORT_ID))
		{
			int newValue = logicValueChanged.newValue;
			if (changeStateChore != null)
			{
				changeStateChore.Cancel("Change state");
				changeStateChore = null;
			}
			bool flag = LogicCircuitNetwork.IsBitActive(0, newValue);
			requestedState = (flag ? ControlState.Opened : ControlState.Locked);
			applyLogicChange = true;
		}
	}

	public void Sim200ms(float dt)
	{
		if (this == null)
		{
			return;
		}
		int[] placementCells;
		if (doorOpenLiquidRefreshHack)
		{
			doorOpenLiquidRefreshTime -= dt;
			if (doorOpenLiquidRefreshTime <= 0f)
			{
				doorOpenLiquidRefreshHack = false;
				placementCells = building.PlacementCells;
				foreach (int cell in placementCells)
				{
					Pathfinding.Instance.AddDirtyNavGridCell(cell);
				}
			}
		}
		if (applyLogicChange)
		{
			applyLogicChange = false;
			ApplyRequestedControlState();
		}
		if (!do_melt_check)
		{
			return;
		}
		StructureTemperatureComponents structureTemperatures = GameComps.StructureTemperatures;
		HandleVector<int>.Handle handle = structureTemperatures.GetHandle(base.gameObject);
		if (!handle.IsValid() || !structureTemperatures.IsBypassed(handle))
		{
			return;
		}
		placementCells = building.PlacementCells;
		foreach (int i2 in placementCells)
		{
			if (!Grid.Solid[i2])
			{
				Util.KDestroyGameObject(this);
				break;
			}
		}
	}

	bool INavDoor.get_isSpawned()
	{
		return base.isSpawned;
	}
}
