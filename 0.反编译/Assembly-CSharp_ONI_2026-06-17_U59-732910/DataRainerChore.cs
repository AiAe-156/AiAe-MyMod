using System;
using TUNING;
using UnityEngine;

public class DataRainerChore : Chore<DataRainerChore.StatesInstance>, IWorkerPrioritizable
{
	public class States : GameStateMachine<States, StatesInstance, DataRainerChore>
	{
		public class RainingStates : State
		{
			public State pre;

			public State loop;

			public State pst;
		}

		public TargetParameter dataRainer;

		public FloatParameter nextBankTimer = new FloatParameter(DataRainer.databankSpawnInterval / 2f);

		public State idle;

		public State goToStand;

		public RainingStates raining;

		public override void InitializeStates(out BaseState default_state)
		{
			default_state = goToStand;
			Target(dataRainer);
			idle.EventTransition(GameHashes.ScheduleBlocksTick, goToStand, (StatesInstance smi) => !smi.IsRecTime());
			goToStand.MoveTo((StatesInstance smi) => smi.GetTargetCell(), raining, idle);
			raining.ToggleAnims("anim_bionic_joy_kanim").DefaultState(raining.loop).Update(delegate(StatesInstance smi, float dt)
			{
				nextBankTimer.Delta(dt, smi);
				if (nextBankTimer.Get(smi) >= DataRainer.databankSpawnInterval)
				{
					nextBankTimer.Delta(0f - DataRainer.databankSpawnInterval, smi);
					GameObject gameObject = Util.KInstantiate(Assets.GetPrefab("PowerStationTools"), smi.master.transform.position + Vector3.up);
					gameObject.GetComponent<PrimaryElement>().SetElement(SimHashes.Iron);
					gameObject.SetActive(value: true);
					KBatchedAnimController component = smi.master.GetComponent<KBatchedAnimController>();
					float num = (float)component.currentFrame / (float)component.GetCurrentNumFrames();
					Vector2 initial_velocity = new Vector2((num < 0.5f) ? (-2.5f) : 2.5f, 4f);
					if (GameComps.Fallers.Has(gameObject))
					{
						GameComps.Fallers.Remove(gameObject);
					}
					GameComps.Fallers.Add(gameObject, initial_velocity);
					DataRainer.Instance sMI = dataRainer.Get(smi).GetSMI<DataRainer.Instance>();
					DataRainer sm = sMI.sm;
					sm.databanksCreated.Set(sm.databanksCreated.Get(sMI) + 1, sMI);
				}
			}, UpdateRate.SIM_33ms);
			raining.loop.PlayAnim("makeitrain2", KAnim.PlayMode.Loop);
		}
	}

	public class StatesInstance : GameStateMachine<States, StatesInstance, DataRainerChore, object>.GameInstance
	{
		private GameObject dataRainer;

		public StatesInstance(DataRainerChore master, GameObject dataRainer)
			: base(master)
		{
			this.dataRainer = dataRainer;
			base.sm.dataRainer.Set(dataRainer, base.smi);
		}

		public bool IsRecTime()
		{
			return base.master.GetComponent<Schedulable>().IsAllowed(Db.Get().ScheduleBlockTypes.Recreation);
		}

		public int GetTargetCell()
		{
			Navigator component = GetComponent<Navigator>();
			float num = float.MaxValue;
			SocialGatheringPoint socialGatheringPoint = null;
			foreach (SocialGatheringPoint item in Components.SocialGatheringPoints.GetItems(Grid.WorldIdx[Grid.PosToCell(this)]))
			{
				float num2 = component.GetNavigationCost(Grid.PosToCell(item));
				if (num2 != -1f && num2 < num)
				{
					num = num2;
					socialGatheringPoint = item;
				}
			}
			if (socialGatheringPoint != null)
			{
				return Grid.PosToCell(socialGatheringPoint);
			}
			return Grid.PosToCell(base.master.gameObject);
		}
	}

	private int basePriority = RELAXATION.PRIORITY.TIER1;

	public DataRainerChore(IStateMachineTarget target)
		: base(Db.Get().ChoreTypes.JoyReaction, target, target.GetComponent<ChoreProvider>(), run_until_complete: false, (Action<Chore>)null, (Action<Chore>)null, (Action<Chore>)null, PriorityScreen.PriorityClass.high, 5, is_preemptable: false, allow_in_context_menu: true, 0, add_to_daily_report: false, ReportManager.ReportType.PersonalTime)
	{
		showAvailabilityInHoverText = false;
		base.smi = new StatesInstance(this, target.gameObject);
		AddPrecondition(ChorePreconditions.instance.IsNotRedAlert);
		AddPrecondition(ChorePreconditions.instance.IsScheduledTime, Db.Get().ScheduleBlockTypes.Recreation);
		AddPrecondition(ChorePreconditions.instance.CanDoWorkerPrioritizable, this);
	}

	public bool GetWorkerPriority(WorkerBase worker, out int priority)
	{
		priority = basePriority;
		return true;
	}
}
