using System;
using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class BionicMassOxygenAbsorbChore : Chore<BionicMassOxygenAbsorbChore.Instance>
{
	public class States : GameStateMachine<States, Instance, BionicMassOxygenAbsorbChore>
	{
		public class MoveStates : State
		{
			public State onGoing;

			public State fail;
		}

		public class MassAbsorbStates : State
		{
			public class CriticalRecover : State
			{
				public State pre;

				public State loop;

				public State pst;
			}

			public State pre;

			public State loop;

			public State pst;

			public CriticalRecover criticalRecoverBreath;
		}

		public MoveStates move;

		public MassAbsorbStates absorb;

		public State fail;

		public State complete;

		public FloatParameter SecondsPassedWithoutOxygen;

		public TargetParameter dupe;

		public Signal TankFilledSignal;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = move;
			Target(dupe);
			root.Exit(delegate(BionicMassOxygenAbsorbChore.Instance smi)
			{
				smi.ChangeCellReservation(Grid.InvalidCell);
			});
			move.DefaultState(move.onGoing).ScheduleChange(fail, IsNotAllowedByScheduleAndChoreIsNotCritical);
			move.onGoing.Enter(RefreshTargetSafeCell).Update(UpdateTargetSafeCellOnlyInCriticalMode, UpdateRate.RENDER_1000ms).MoveTo((BionicMassOxygenAbsorbChore.Instance smi) => smi.targetCell, absorb, move.fail, update_cell: true);
			move.fail.ReturnFailure();
			absorb.ScheduleChange(fail, IsNotAllowedByScheduleAndChoreIsNotCritical).ToggleTag(GameTags.RecoveringBreath).ToggleAnims("anim_bionic_absorb_kanim")
				.Enter(ShowOxygenBar)
				.Exit(HideOxygenBar)
				.DefaultState(absorb.pre);
			absorb.pre.PlayAnim("absorb_pre", KAnim.PlayMode.Once).OnAnimQueueComplete(absorb.loop).ScheduleGoTo(3f, absorb.loop)
				.Exit(ResetOxygenTimer);
			absorb.loop.Enter(ResetOxygenTimer).ParamTransition(SecondsPassedWithoutOxygen, absorb.pst, (BionicMassOxygenAbsorbChore.Instance smi, float secondsPassed) => secondsPassed > smi.GetGiveupTimerTimeout()).OnSignal(TankFilledSignal, absorb.pst)
				.PlayAnim("absorb_loop", KAnim.PlayMode.Loop)
				.Update(AbsorbUpdate)
				.Transition(absorb.pst, ChoreIsCriticalModeAndGiveUpOxygenLevelReached);
			absorb.pst.Transition(absorb.criticalRecoverBreath.pre, IsCriticalChore).PlayAnim("absorb_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(complete)
				.ScheduleGoTo(3f, complete);
			absorb.criticalRecoverBreath.ToggleAnims("anim_emotes_default_kanim").DefaultState(absorb.criticalRecoverBreath.pre);
			absorb.criticalRecoverBreath.pre.PlayAnim("breathe_pre").QueueAnim("breathe_loop").OnAnimQueueComplete(absorb.criticalRecoverBreath.loop);
			absorb.criticalRecoverBreath.loop.PlayAnim("breathe_loop", KAnim.PlayMode.Loop).ToggleAttributeModifier("Recovering Breath", (BionicMassOxygenAbsorbChore.Instance smi) => smi.recoveringbreath).Transition(absorb.criticalRecoverBreath.pst, BreathIsFull)
				.Transition(absorb.criticalRecoverBreath.pst, (BionicMassOxygenAbsorbChore.Instance smi) => smi.UpdateTargetCell() == Grid.InvalidCell);
			absorb.criticalRecoverBreath.pst.PlayAnim("breathe_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(complete).ScheduleGoTo(3f, complete);
			fail.ReturnFailure();
			complete.ReturnSuccess();
		}
	}

	public struct AbsorbUpdateData
	{
		public Instance smi;

		public float dt;

		public AbsorbUpdateData(Instance smi, float dt)
		{
			this.smi = smi;
			this.dt = dt;
		}
	}

	public class Instance : GameStateMachine<States, Instance, BionicMassOxygenAbsorbChore, object>.GameInstance, BionicOxygenTankMonitor.IChore
	{
		public AttributeModifier recoveringbreath;

		public Queue<float> massAbsorbedHistory = new Queue<float>();

		public int targetCell = Grid.InvalidCell;

		public BionicOxygenTankMonitor.Instance oxygenTankMonitor;

		public float CRITICAL_OXYGEN_MASS_GIVE_UP_TRESHOLD => oxygenBreather.ConsumptionRate * 8f;

		public OxygenBreather oxygenBreather { get; private set; }

		public float GetGiveupTimerTimeout()
		{
			if (oxygenTankMonitor == null)
			{
				return 2f;
			}
			if (!BionicOxygenTankMonitor.AreOxygenLevelsCritical(oxygenTankMonitor))
			{
				return 4f;
			}
			return 2f;
		}

		public Instance(BionicMassOxygenAbsorbChore master, GameObject duplicant)
			: base(master)
		{
			base.sm.dupe.Set(duplicant, base.smi);
			oxygenTankMonitor = duplicant.GetSMI<BionicOxygenTankMonitor.Instance>();
			oxygenBreather = duplicant.GetComponent<OxygenBreather>();
			Klei.AI.Attribute deltaAttribute = Db.Get().Amounts.Breath.deltaAttribute;
			float rECOVER_BREATH_DELTA = DUPLICANTSTATS.STANDARD.BaseStats.RECOVER_BREATH_DELTA;
			recoveringbreath = new AttributeModifier(deltaAttribute.Id, rECOVER_BREATH_DELTA, DUPLICANTS.MODIFIERS.RECOVERINGBREATH.NAME);
		}

		public bool IsConsumingOxygen()
		{
			return !IsInsideState(base.sm.move);
		}

		public void ChangeCellReservation(int newCell)
		{
			if (targetCell != Grid.InvalidCell && Grid.Reserved[targetCell])
			{
				Grid.Reserved[targetCell] = false;
			}
			if (newCell != Grid.InvalidCell && !Grid.Reserved[newCell])
			{
				Grid.Reserved[newCell] = true;
			}
		}

		public override void StopSM(string reason)
		{
			ChangeCellReservation(Grid.InvalidCell);
			base.StopSM(reason);
		}

		public int UpdateTargetCell()
		{
			oxygenTankMonitor.UpdatePotentialCellToAbsorbOxygen(targetCell);
			int absorbOxygenCell = oxygenTankMonitor.AbsorbOxygenCell;
			ChangeCellReservation(absorbOxygenCell);
			targetCell = absorbOxygenCell;
			return absorbOxygenCell;
		}

		public void ResetMassTrackHistory()
		{
			massAbsorbedHistory.Clear();
			for (int i = 0; i < 15; i++)
			{
				massAbsorbedHistory.Enqueue(0f);
			}
		}

		public void AddMassToHistory(float mass_rate_this_tick)
		{
			if (massAbsorbedHistory.Count == 15)
			{
				massAbsorbedHistory.Dequeue();
			}
			massAbsorbedHistory.Enqueue(mass_rate_this_tick);
		}

		public float GetAverageMassConsumedPerSecond()
		{
			float num = 0f;
			int num2 = 0;
			foreach (float item in massAbsorbedHistory)
			{
				num += item;
				num2++;
			}
			if (num2 <= 0)
			{
				return 0f;
			}
			return num / (float)num2;
		}

		public void OnSimConsume(Sim.MassConsumedCallback mass_cb_info, float dt)
		{
			if (oxygenBreather == null || oxygenTankMonitor == null || oxygenBreather.prefabID.HasTag(GameTags.Dead))
			{
				return;
			}
			AddMassToHistory(mass_cb_info.mass / dt);
			GameObject go = oxygenBreather.gameObject;
			bool num = BionicOxygenTankMonitor.AreOxygenLevelsCritical(oxygenTankMonitor);
			float num2 = (num ? CRITICAL_OXYGEN_MASS_GIVE_UP_TRESHOLD : 2f);
			if (GetAverageMassConsumedPerSecond() <= num2)
			{
				base.sm.SecondsPassedWithoutOxygen.Set(base.sm.SecondsPassedWithoutOxygen.Get(base.smi) + dt, base.smi);
			}
			else
			{
				ResetOxygenTimer(base.smi);
			}
			if (num)
			{
				float num3 = DUPLICANTSTATS.STANDARD.Breath.BREATH_RATE * DUPLICANTSTATS.STANDARD.BaseStats.OXYGEN_USED_PER_SECOND;
				if (mass_cb_info.mass == 0f)
				{
					mass_cb_info.temperature = DUPLICANTSTATS.BIONICS.Temperature.Internal.IDEAL;
				}
				mass_cb_info.mass += DUPLICANTSTATS.STANDARD.BaseStats.RECOVER_BREATH_DELTA * num3 * dt + DUPLICANTSTATS.STANDARD.BaseStats.OXYGEN_USED_PER_SECOND * dt;
			}
			float num4 = oxygenTankMonitor.AddGas(mass_cb_info);
			if (num4 > Mathf.Epsilon)
			{
				SimMessages.EmitMass(Grid.PosToCell(go), mass_cb_info.elemIdx, num4, mass_cb_info.temperature, byte.MaxValue, 0);
			}
			if (!HasSpaceInOxygenTank(this))
			{
				base.sm.TankFilledSignal.Trigger(this);
			}
		}

		public float GetOxygen()
		{
			if (oxygenTankMonitor != null)
			{
				return oxygenTankMonitor.OxygenPercentage;
			}
			return 0f;
		}
	}

	public static CellOffset[] ABSORB_RANGE = new CellOffset[6]
	{
		new CellOffset(0, 0),
		new CellOffset(0, 1),
		new CellOffset(1, 1),
		new CellOffset(-1, 1),
		new CellOffset(1, 0),
		new CellOffset(-1, 0)
	};

	public const float ABSORB_RATE_IDEAL_CHORE_DURATION = 30f;

	public static readonly float ABSORB_RATE = BionicOxygenTankMonitor.OXYGEN_TANK_CAPACITY_KG / 30f;

	public const int HISTORY_ROW_COUNT = 15;

	public const float LOW_OXYGEN_TRESHOLD = 2f;

	public const float GIVE_UP_DURATION_CRTICIAL_MODE = 2f;

	public const float GIVE_UP_DURATION_LOW_OXYGEN_MODE = 4f;

	public const float CRITICAL_CHORE_GIVE_UP_OXYGEN_LEVEL_TRESHOLD = 0.25f;

	public const string ABSORB_ANIM_FILE = "anim_bionic_absorb_kanim";

	public const string ABSORB_PRE_ANIM_NAME = "absorb_pre";

	public const string ABSORB_LOOP_ANIM_NAME = "absorb_loop";

	public const string ABSORB_PST_ANIM_NAME = "absorb_pst";

	public static CellOffset MouthCellOffset = new CellOffset(0, 1);

	public BionicMassOxygenAbsorbChore(IStateMachineTarget target, bool critical)
		: base(critical ? Db.Get().ChoreTypes.BionicAbsorbOxygen_Critical : Db.Get().ChoreTypes.BionicAbsorbOxygen, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, critical ? PriorityScreen.PriorityClass.compulsory : PriorityScreen.PriorityClass.personalNeeds, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.WorkTime)
	{
		base.smi = new Instance(this, target.gameObject);
		Func<int> data = base.smi.UpdateTargetCell;
		AddPrecondition(ChorePreconditions.instance.IsNotRedAlert);
		AddPrecondition(ChorePreconditions.instance.CanMoveToDynamicCellUntilBegun, data);
	}

	public override string ResolveString(string str)
	{
		float mass = ((base.smi == null) ? 0f : base.smi.GetAverageMassConsumedPerSecond());
		return string.Format(base.ResolveString(str), GameUtil.GetFormattedMass(mass, GameUtil.TimeSlice.PerSecond));
	}

	public override void Begin(Precondition.Context context)
	{
		if (context.consumerState.consumer == null)
		{
			Debug.LogError("BionicMassAbsorbOxygenChore null context.consumer");
			return;
		}
		if (context.consumerState.consumer.GetSMI<BionicOxygenTankMonitor.Instance>() == null)
		{
			Debug.LogError("BionicMassAbsorbOxygenChore null BionicOxygenTankMonitor.Instance");
			return;
		}
		base.smi.ResetMassTrackHistory();
		base.smi.sm.dupe.Set(context.consumerState.consumer, base.smi);
		base.Begin(context);
	}

	public static bool IsNotAllowedByScheduleAndChoreIsNotCritical(Instance smi)
	{
		if (!IsCriticalChore(smi))
		{
			return !IsAllowedBySchedule(smi);
		}
		return false;
	}

	public static bool IsAllowedBySchedule(Instance smi)
	{
		return BionicOxygenTankMonitor.IsAllowedToSeekOxygenBySchedule(smi.oxygenTankMonitor);
	}

	public static bool IsCriticalChore(Instance smi)
	{
		return smi.master.choreType == Db.Get().ChoreTypes.BionicAbsorbOxygen_Critical;
	}

	public static void ResetOxygenTimer(Instance smi)
	{
		smi.sm.SecondsPassedWithoutOxygen.Set(0f, smi);
	}

	public static void RefreshTargetSafeCell(Instance smi)
	{
		smi.UpdateTargetCell();
	}

	public static void UpdateTargetSafeCell(Instance smi, float dt)
	{
		RefreshTargetSafeCell(smi);
	}

	public static bool HasSpaceInOxygenTank(Instance smi)
	{
		return smi.oxygenTankMonitor.SpaceAvailableInTank > 0f;
	}

	public static bool ChoreIsCriticalModeAndGiveUpOxygenLevelReached(Instance smi)
	{
		if (IsCriticalChore(smi))
		{
			return smi.oxygenTankMonitor.OxygenPercentage >= 0.25f;
		}
		return false;
	}

	public static bool BreathIsFull(Instance smi)
	{
		AmountInstance amountInstance = smi.gameObject.GetAmounts().Get(Db.Get().Amounts.Breath);
		return amountInstance.value >= amountInstance.GetMax();
	}

	public static void UpdateTargetSafeCellOnlyInCriticalMode(Instance smi, float dt)
	{
		if (IsCriticalChore(smi))
		{
			RefreshTargetSafeCell(smi);
		}
	}

	public static void AbsorbUpdate(Instance smi, float dt)
	{
		float mass = Mathf.Min(dt * ABSORB_RATE, smi.oxygenTankMonitor.SpaceAvailableInTank);
		AbsorbUpdateData absorbUpdateData = new AbsorbUpdateData(smi, dt);
		int elementCell;
		SimHashes nearBreathableElement = GetNearBreathableElement(elementCell = Grid.PosToCell(smi.sm.dupe.Get(smi)), ABSORB_RANGE, out elementCell);
		HandleVector<Game.ComplexCallbackInfo<Sim.MassConsumedCallback>>.Handle handle = Game.Instance.massConsumedCallbackManager.Add(OnSimConsumeCallback, absorbUpdateData, "BionicMassOxygenAbsorbChore");
		SimMessages.ConsumeMass(elementCell, nearBreathableElement, mass, 6, handle.index);
	}

	private static void OnSimConsumeCallback(Sim.MassConsumedCallback mass_cb_info, object data)
	{
		AbsorbUpdateData absorbUpdateData = (AbsorbUpdateData)data;
		absorbUpdateData.smi.OnSimConsume(mass_cb_info, absorbUpdateData.dt);
	}

	private static void ShowOxygenBar(Instance smi)
	{
		if (NameDisplayScreen.Instance != null)
		{
			NameDisplayScreen.Instance.SetBionicOxygenTankDisplay(smi.gameObject, smi.GetOxygen, bVisible: true);
		}
	}

	private static void HideOxygenBar(Instance smi)
	{
		if (NameDisplayScreen.Instance != null)
		{
			NameDisplayScreen.Instance.SetBionicOxygenTankDisplay(smi.gameObject, null, bVisible: false);
		}
	}

	public static SimHashes GetNearBreathableElement(int centralCell, CellOffset[] range, out int elementCell)
	{
		float num = 0f;
		int num2 = centralCell;
		SimHashes simHashes = SimHashes.Vacuum;
		foreach (CellOffset offset in range)
		{
			int num3 = Grid.OffsetCell(centralCell, offset);
			SimHashes elementID = SimHashes.Vacuum;
			float breathableMassInCell = GetBreathableMassInCell(num3, out elementID);
			if (breathableMassInCell > Mathf.Epsilon && (simHashes == SimHashes.Vacuum || breathableMassInCell > num))
			{
				simHashes = elementID;
				num = breathableMassInCell;
				num2 = num3;
			}
		}
		elementCell = num2;
		return simHashes;
	}

	private static float GetBreathableMassInCell(int cell, out SimHashes elementID)
	{
		if (Grid.IsValidCell(cell))
		{
			Element element = Grid.Element[cell];
			if (element.HasTag(GameTags.Breathable))
			{
				elementID = element.id;
				return Grid.Mass[cell];
			}
		}
		elementID = SimHashes.Vacuum;
		return 0f;
	}
}
