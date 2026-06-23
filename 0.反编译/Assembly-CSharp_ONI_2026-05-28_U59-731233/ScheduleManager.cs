using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using FMOD.Studio;
using KSerialization;
using STRINGS;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/ScheduleManager")]
public class ScheduleManager : KMonoBehaviour, ISim33ms
{
	public class Tuning : TuningData<Tuning>
	{
		public float toneSpacingSeconds;

		public int minToneIndex;

		public int maxToneIndex;

		public int firstLastToneSpacing;
	}

	[Serialize]
	private List<Schedule> schedules;

	[Serialize]
	private int lastHour = 0;

	[Serialize]
	private int scheduleNameIncrementor = 0;

	public static ScheduleManager Instance;

	[Serialize]
	private bool hasDeletedDefaultBionicSchedule = false;

	public event Action<List<Schedule>> onSchedulesChanged;

	public static void DestroyInstance()
	{
		Instance = null;
	}

	public Schedule GetDefaultBionicSchedule()
	{
		return schedules.Find((Schedule match) => match.isDefaultForBionics);
	}

	[OnDeserialized]
	private void OnDeserialized()
	{
		if (schedules.Count == 0)
		{
			AddDefaultSchedule(alarmOn: true);
		}
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		schedules = new List<Schedule>();
		Instance = this;
	}

	protected override void OnSpawn()
	{
		if (schedules.Count == 0)
		{
			AddDefaultSchedule(alarmOn: true);
		}
		foreach (Schedule schedule in schedules)
		{
			schedule.ClearNullReferences();
		}
		List<ScheduleBlock> scheduleBlocksFromGroupDefaults = Schedule.GetScheduleBlocksFromGroupDefaults(Db.Get().ScheduleGroups.allGroups);
		foreach (Schedule schedule2 in schedules)
		{
			List<ScheduleBlock> blocks = schedule2.GetBlocks();
			for (int i = 0; i < blocks.Count; i++)
			{
				ScheduleBlock scheduleBlock = blocks[i];
				if (Db.Get().ScheduleGroups.FindGroupForScheduleTypes(scheduleBlock.allowed_types) == null)
				{
					ScheduleGroup scheduleGroup = Db.Get().ScheduleGroups.FindGroupForScheduleTypes(scheduleBlocksFromGroupDefaults[i].allowed_types);
					schedule2.SetBlockGroup(i, scheduleGroup);
				}
			}
		}
		foreach (MinionIdentity item in Components.LiveMinionIdentities.Items)
		{
			Schedulable component = item.GetComponent<Schedulable>();
			if (GetSchedule(component) == null)
			{
				schedules[0].Assign(component);
			}
		}
		Components.LiveMinionIdentities.OnAdd += OnAddDupe;
		Components.LiveMinionIdentities.OnRemove += OnRemoveDupe;
	}

	private void OnAddDupe(MinionIdentity minion)
	{
		Schedulable component = minion.GetComponent<Schedulable>();
		if (component.GetSchedule() != null)
		{
			return;
		}
		Schedule schedule = schedules[0];
		if (minion.model == GameTags.Minions.Models.Bionic)
		{
			if (GetDefaultBionicSchedule() == null)
			{
				if (!hasDeletedDefaultBionicSchedule)
				{
					Schedule schedule2 = AddSchedule(Db.Get().ScheduleGroups.allGroups, UI.SCHEDULESCREEN.SCHEDULE_NAME_DEFAULT_BIONIC, alarmOn: true);
					schedule2.AddTimetable(Schedule.GetScheduleBlocksFromGroupDefaults(Db.Get().ScheduleGroups.allGroups));
					schedule2.AddTimetable(Schedule.GetScheduleBlocksFromGroupDefaults(Db.Get().ScheduleGroups.allGroups));
					for (int i = 0; i < schedule2.GetBlocks().Count; i++)
					{
						schedule2.SetBlockGroup(i, Db.Get().ScheduleGroups.Worktime);
					}
					for (int j = 1; j <= 6; j++)
					{
						schedule2.SetBlockGroup(schedule2.GetBlocks().Count - j, Db.Get().ScheduleGroups.Sleep);
					}
					for (int k = 7; k <= 12; k++)
					{
						schedule2.SetBlockGroup(schedule2.GetBlocks().Count - k, Db.Get().ScheduleGroups.Recreation);
					}
					schedule = schedule2;
					schedule2.isDefaultForBionics = true;
					if (this.onSchedulesChanged != null)
					{
						this.onSchedulesChanged(schedules);
					}
				}
			}
			else
			{
				schedule = GetDefaultBionicSchedule();
			}
		}
		else if (GetSchedule(component) != null)
		{
			schedule = GetSchedule(component);
		}
		schedule.Assign(component);
	}

	private void OnRemoveDupe(MinionIdentity minion)
	{
		Schedulable component = minion.GetComponent<Schedulable>();
		GetSchedule(component)?.Unassign(component);
	}

	public void OnStoredDupeDestroyed(StoredMinionIdentity dupe)
	{
		foreach (Schedule schedule in schedules)
		{
			schedule.Unassign(dupe.gameObject.GetComponent<Schedulable>());
		}
	}

	public void AddDefaultSchedule(bool alarmOn, bool useDefaultName = true)
	{
		Schedule schedule = AddSchedule(Db.Get().ScheduleGroups.allGroups, useDefaultName ? UI.SCHEDULESCREEN.SCHEDULE_NAME_DEFAULT : UI.SCHEDULESCREEN.SCHEDULE_NAME_NEW, alarmOn);
		if (Game.Instance.FastWorkersModeActive)
		{
			for (int i = 0; i < 21; i++)
			{
				schedule.SetBlockGroup(i, Db.Get().ScheduleGroups.Worktime);
			}
			schedule.SetBlockGroup(21, Db.Get().ScheduleGroups.Recreation);
			schedule.SetBlockGroup(22, Db.Get().ScheduleGroups.Recreation);
			schedule.SetBlockGroup(23, Db.Get().ScheduleGroups.Sleep);
		}
	}

	public Schedule AddSchedule(List<ScheduleGroup> groups, string name = null, bool alarmOn = false)
	{
		if (name == null)
		{
			scheduleNameIncrementor++;
			name = string.Format(UI.SCHEDULESCREEN.SCHEDULE_NAME_FORMAT, scheduleNameIncrementor.ToString());
		}
		Schedule schedule = new Schedule(name, groups, alarmOn);
		schedules.Add(schedule);
		if (this.onSchedulesChanged != null)
		{
			this.onSchedulesChanged(schedules);
		}
		return schedule;
	}

	public Schedule DuplicateSchedule(Schedule source)
	{
		if (base.name == null)
		{
			scheduleNameIncrementor++;
			base.name = string.Format(UI.SCHEDULESCREEN.SCHEDULE_NAME_FORMAT, scheduleNameIncrementor.ToString());
		}
		Schedule schedule = new Schedule(string.Concat(UI.SCHEDULESCREEN.SCHEDULE_NAME_COPY_PREFIX, source.name), source.GetBlocks(), source.alarmActivated);
		schedule.ProgressTimetableIdx = source.ProgressTimetableIdx;
		schedules.Add(schedule);
		if (this.onSchedulesChanged != null)
		{
			this.onSchedulesChanged(schedules);
		}
		return schedule;
	}

	public void DeleteSchedule(Schedule schedule)
	{
		if (schedules.Count == 1)
		{
			return;
		}
		List<Ref<Schedulable>> assigned = schedule.GetAssigned();
		if (schedule.isDefaultForBionics)
		{
			hasDeletedDefaultBionicSchedule = true;
		}
		schedules.Remove(schedule);
		foreach (Ref<Schedulable> item in assigned)
		{
			schedules[0].Assign(item.Get());
		}
		if (this.onSchedulesChanged != null)
		{
			this.onSchedulesChanged(schedules);
		}
	}

	public Schedule GetSchedule(Schedulable schedulable)
	{
		foreach (Schedule schedule in schedules)
		{
			if (schedule.IsAssigned(schedulable))
			{
				return schedule;
			}
		}
		return null;
	}

	public List<Schedule> GetSchedules()
	{
		return schedules;
	}

	public bool IsAllowed(Schedulable schedulable, ScheduleBlockType schedule_block_type)
	{
		return GetSchedule(schedulable)?.GetCurrentScheduleBlock().IsAllowed(schedule_block_type) ?? false;
	}

	public static int GetCurrentHour()
	{
		float currentCycleAsPercentage = GameClock.Instance.GetCurrentCycleAsPercentage();
		int val = (int)(currentCycleAsPercentage * 24f);
		return Math.Min(val, 23);
	}

	public void Sim33ms(float dt)
	{
		int currentHour = GetCurrentHour();
		if (GetCurrentHour() == lastHour)
		{
			return;
		}
		foreach (Schedule schedule in schedules)
		{
			schedule.Tick();
		}
		lastHour = currentHour;
	}

	public void PlayScheduleAlarm(Schedule schedule, ScheduleBlock block, bool forwards)
	{
		Notification notification = new Notification(string.Format(MISC.NOTIFICATIONS.SCHEDULE_CHANGED.NAME, schedule.name, block.name), NotificationType.Good, (List<Notification> notificationList, object data) => MISC.NOTIFICATIONS.SCHEDULE_CHANGED.TOOLTIP.Replace("{0}", schedule.name).Replace("{1}", block.name).Replace("{2}", Db.Get().ScheduleGroups.Get(block.GroupId).notificationTooltip));
		GetComponent<Notifier>().Add(notification);
		StartCoroutine(PlayScheduleTone(schedule, forwards));
	}

	private IEnumerator PlayScheduleTone(Schedule schedule, bool forwards)
	{
		int[] tones = schedule.GetTones();
		for (int i = 0; i < tones.Length; i++)
		{
			int t = (forwards ? i : (tones.Length - 1 - i));
			PlayTone(tones[t], forwards);
			yield return SequenceUtil.WaitForSeconds(TuningData<Tuning>.Get().toneSpacingSeconds);
		}
	}

	private void PlayTone(int pitch, bool forwards)
	{
		EventInstance instance = KFMOD.BeginOneShot(GlobalAssets.GetSound("WorkChime_tone"), Vector3.zero);
		instance.setParameterByName("WorkChime_pitch", pitch);
		instance.setParameterByName("WorkChime_start", forwards ? 1 : 0);
		KFMOD.EndOneShot(instance);
	}
}
