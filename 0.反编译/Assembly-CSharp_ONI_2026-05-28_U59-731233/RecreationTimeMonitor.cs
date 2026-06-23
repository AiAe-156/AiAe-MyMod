using System;
using System.Collections.Generic;
using KSerialization;
using Klei.AI;
using STRINGS;
using UnityEngine;

public class RecreationTimeMonitor : GameStateMachine<RecreationTimeMonitor, RecreationTimeMonitor.Instance, IStateMachineTarget, RecreationTimeMonitor.Def>
{
	public class Def : BaseDef
	{
	}

	public new class Instance : GameInstance
	{
		[Serialize]
		public List<float> moraleAddedTimes = new List<float>();

		public Effect moraleEffect = new Effect("RecTimeEffect", "Rec Time Effect", "Rec Time Effect Description", 0f, show_in_ui: false, trigger_floating_text: false, is_bad: false);

		private Schedulable schedulable;

		private AttributeModifier moraleModifier;

		private int shiftValue;

		private float bonus_duration;

		public Instance(IStateMachineTarget master, Def def)
			: base(master, def)
		{
			bool flag = base.gameObject.PrefabID() == BionicMinionConfig.ID;
			bonus_duration = (flag ? 1800f : 600f);
			schedulable = master.GetComponent<Schedulable>();
			moraleModifier = new AttributeModifier(Db.Get().Attributes.QualityOfLife.Id, 0f, delegate
			{
				int num = Mathf.Clamp(moraleAddedTimes.Count - 1, 0, 5);
				return (num == 5) ? ((string)DUPLICANTS.MODIFIERS.BREAK_BONUS.MAX_NAME) : ((string)DUPLICANTS.MODIFIERS.BREAK_BONUS.NAME);
			});
			moraleEffect.Add(moraleModifier);
			if ((SaveLoader.Instance.GameInfo.saveMajorVersion != 0 || SaveLoader.Instance.GameInfo.saveMinorVersion != 0) && SaveLoader.Instance.GameInfo.IsVersionOlderThan(7, 35))
			{
				RestoreFromSchedule();
			}
		}

		public override void StartSM()
		{
			base.StartSM();
			RefreshTimes();
		}

		public void RefreshTimes()
		{
			for (int num = moraleAddedTimes.Count - 1; num >= 0; num--)
			{
				if (GameClock.Instance.GetTime() - moraleAddedTimes[num] > bonus_duration)
				{
					moraleAddedTimes.RemoveAt(num);
				}
			}
			int num2 = Math.Clamp(moraleAddedTimes.Count - 1, 0, 5);
			moraleModifier.SetValue(num2);
			if (num2 > 0)
			{
				if (base.smi.GetCurrentState() != base.smi.sm.bonusActive)
				{
					base.smi.GoTo(base.smi.sm.bonusActive);
				}
			}
			else if (base.smi.GetCurrentState() != base.smi.sm.idle)
			{
				base.smi.GoTo(base.smi.sm.idle);
			}
		}

		public void OnScheduleBlocksTick()
		{
			Schedule schedule = ScheduleManager.Instance.GetSchedule(schedulable);
			ScheduleBlock previousScheduleBlock = schedule.GetPreviousScheduleBlock();
			if (previousScheduleBlock.GroupId == Db.Get().ScheduleGroups.Recreation.Id)
			{
				moraleAddedTimes.Add(GameClock.Instance.GetTime());
			}
		}

		private void RestoreFromSchedule()
		{
			Effects component = GetComponent<Effects>();
			string[] array = new string[5] { "Break1", "Break2", "Break3", "Break4", "Break5" };
			string[] array2 = array;
			foreach (string effect_id in array2)
			{
				if (component.HasEffect(effect_id))
				{
					component.Remove(effect_id);
				}
			}
			Schedule schedule = ScheduleManager.Instance.GetSchedule(schedulable);
			List<ScheduleBlock> blocks = schedule.GetBlocks();
			int currentBlockIdx = schedule.GetCurrentBlockIdx();
			int num = 24;
			if (GameClock.Instance.GetTime() <= bonus_duration)
			{
				num = Math.Min(currentBlockIdx, Mathf.FloorToInt(GameClock.Instance.GetTime() / 25f));
			}
			for (int j = currentBlockIdx - num; j < currentBlockIdx; j++)
			{
				int k = j;
				Debug.Assert(blocks.Count > 0);
				for (; k < 0; k += blocks.Count)
				{
				}
				if (blocks[k].GroupId == Db.Get().ScheduleGroups.Recreation.Id)
				{
					int num2 = 0;
					num2 = ((k <= currentBlockIdx) ? (currentBlockIdx - k - 1) : (blocks.Count - k + currentBlockIdx - 1));
					float num3 = (float)num2 * 25f;
					float num4 = GameClock.Instance.GetTime() - num3;
					Debug.Assert(num4 > 0f);
					moraleAddedTimes.Add(num4);
				}
			}
		}
	}

	public const int MAX_BONUS = 5;

	public const float BONUS_DURATION_STANDARD = 600f;

	public const float BONUS_DURATION_BIONICS = 1800f;

	public State idle;

	public State bonusActive;

	public override void InitializeStates(out BaseState default_state)
	{
		base.serializable = SerializeType.ParamsOnly;
		default_state = idle;
		idle.EventHandler(GameHashes.ScheduleBlocksTick, delegate(Instance smi)
		{
			smi.OnScheduleBlocksTick();
		}).Update(delegate(Instance smi, float dt)
		{
			smi.RefreshTimes();
		});
		bonusActive.ToggleEffect((Instance smi) => smi.moraleEffect).EventHandler(GameHashes.ScheduleBlocksTick, delegate(Instance smi)
		{
			smi.OnScheduleBlocksTick();
		}).Update(delegate(Instance smi, float dt)
		{
			smi.RefreshTimes();
		});
	}
}
