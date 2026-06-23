using System;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class Schedule : ISaveLoadable, IListableOption
{
	[Serialize]
	private List<ScheduleBlock> blocks;

	[Serialize]
	private List<Ref<Schedulable>> assigned;

	[Serialize]
	public string name;

	[Serialize]
	public bool alarmActivated = true;

	[Serialize]
	private int[] tones;

	[Serialize]
	public bool isDefaultForBionics;

	[Serialize]
	private int progressTimetableIdx;

	public Action<Schedule> onChanged;

	public int ProgressTimetableIdx
	{
		get
		{
			return progressTimetableIdx;
		}
		set
		{
			progressTimetableIdx = value;
		}
	}

	public ScheduleBlock GetCurrentScheduleBlock()
	{
		return GetBlock(GetCurrentBlockIdx());
	}

	public int GetCurrentBlockIdx()
	{
		return Math.Min((int)(GameClock.Instance.GetCurrentCycleAsPercentage() * 24f), 23) + progressTimetableIdx * 24;
	}

	public ScheduleBlock GetPreviousScheduleBlock()
	{
		return GetBlock(GetPreviousBlockIdx());
	}

	public int GetPreviousBlockIdx()
	{
		int num = GetCurrentBlockIdx() - 1;
		if (num == -1)
		{
			num = blocks.Count - 1;
		}
		return num;
	}

	public void ClearNullReferences()
	{
		assigned.RemoveAll((Ref<Schedulable> x) => x.Get() == null);
	}

	public Schedule(string name, List<ScheduleGroup> defaultGroups, bool alarmActivated)
	{
		this.name = name;
		this.alarmActivated = alarmActivated;
		blocks = new List<ScheduleBlock>(defaultGroups.Count);
		assigned = new List<Ref<Schedulable>>();
		tones = GenerateTones();
		SetBlocksToGroupDefaults(defaultGroups);
	}

	public Schedule(string name, List<ScheduleBlock> sourceBlocks, bool alarmActivated)
	{
		this.name = name;
		this.alarmActivated = alarmActivated;
		blocks = new List<ScheduleBlock>();
		for (int i = 0; i < sourceBlocks.Count; i++)
		{
			blocks.Add(new ScheduleBlock(sourceBlocks[i].name, sourceBlocks[i].GroupId));
		}
		assigned = new List<Ref<Schedulable>>();
		tones = GenerateTones();
		Changed();
	}

	public void SetBlocksToGroupDefaults(List<ScheduleGroup> defaultGroups)
	{
		blocks = GetScheduleBlocksFromGroupDefaults(defaultGroups);
		Debug.Assert(blocks.Count == 24);
		Changed();
	}

	public static List<ScheduleBlock> GetScheduleBlocksFromGroupDefaults(List<ScheduleGroup> defaultGroups)
	{
		List<ScheduleBlock> list = new List<ScheduleBlock>();
		for (int i = 0; i < defaultGroups.Count; i++)
		{
			ScheduleGroup scheduleGroup = defaultGroups[i];
			for (int j = 0; j < scheduleGroup.defaultSegments; j++)
			{
				list.Add(new ScheduleBlock(scheduleGroup.Name, scheduleGroup.Id));
			}
		}
		return list;
	}

	public void Tick()
	{
		ScheduleBlock currentScheduleBlock = GetCurrentScheduleBlock();
		ScheduleBlock block = GetBlock(GetPreviousBlockIdx());
		Debug.Assert(block != currentScheduleBlock);
		if (GetCurrentBlockIdx() % 24 == 0)
		{
			progressTimetableIdx++;
			if (progressTimetableIdx >= blocks.Count / 24)
			{
				progressTimetableIdx = 0;
			}
			if (ScheduleScreen.Instance != null)
			{
				ScheduleScreen.Instance.OnChangeCurrentTimetable();
			}
		}
		if (!AreScheduleTypesIdentical(currentScheduleBlock.allowed_types, block.allowed_types))
		{
			ScheduleGroup scheduleGroup = Db.Get().ScheduleGroups.FindGroupForScheduleTypes(currentScheduleBlock.allowed_types);
			ScheduleGroup scheduleGroup2 = Db.Get().ScheduleGroups.FindGroupForScheduleTypes(block.allowed_types);
			if (alarmActivated && scheduleGroup2.alarm != scheduleGroup.alarm)
			{
				ScheduleManager.Instance.PlayScheduleAlarm(this, currentScheduleBlock, scheduleGroup.alarm);
			}
			foreach (Ref<Schedulable> item in GetAssigned())
			{
				item.Get().OnScheduleBlocksChanged(this);
			}
		}
		foreach (Ref<Schedulable> item2 in GetAssigned())
		{
			item2.Get().OnScheduleBlocksTick(this);
		}
	}

	string IListableOption.GetProperName()
	{
		return name;
	}

	public int[] GenerateTones()
	{
		int minToneIndex = TuningData<ScheduleManager.Tuning>.Get().minToneIndex;
		int maxToneIndex = TuningData<ScheduleManager.Tuning>.Get().maxToneIndex;
		int firstLastToneSpacing = TuningData<ScheduleManager.Tuning>.Get().firstLastToneSpacing;
		int[] array = new int[4];
		array[0] = UnityEngine.Random.Range(minToneIndex, maxToneIndex - firstLastToneSpacing + 1);
		array[1] = UnityEngine.Random.Range(minToneIndex, maxToneIndex + 1);
		array[2] = UnityEngine.Random.Range(minToneIndex, maxToneIndex + 1);
		array[3] = UnityEngine.Random.Range(array[0] + firstLastToneSpacing, maxToneIndex + 1);
		return array;
	}

	public List<Ref<Schedulable>> GetAssigned()
	{
		if (assigned == null)
		{
			assigned = new List<Ref<Schedulable>>();
		}
		return assigned;
	}

	public int[] GetTones()
	{
		if (tones == null)
		{
			tones = GenerateTones();
		}
		return tones;
	}

	public void SetBlockGroup(int idx, ScheduleGroup group)
	{
		if (0 <= idx && idx < blocks.Count)
		{
			blocks[idx] = new ScheduleBlock(group.Name, group.Id);
			Changed();
		}
	}

	private void Changed()
	{
		foreach (Ref<Schedulable> item in GetAssigned())
		{
			item.Get().OnScheduleChanged(this);
		}
		if (onChanged != null)
		{
			onChanged(this);
		}
	}

	public List<ScheduleBlock> GetBlocks()
	{
		return blocks;
	}

	public ScheduleBlock GetBlock(int idx)
	{
		return blocks[idx];
	}

	public void InsertTimetable(int timetableIdx, List<ScheduleBlock> newBlocks)
	{
		blocks.InsertRange(timetableIdx * 24, newBlocks);
		if (timetableIdx <= progressTimetableIdx)
		{
			progressTimetableIdx++;
		}
	}

	public void AddTimetable(List<ScheduleBlock> newBlocks)
	{
		blocks.AddRange(newBlocks);
	}

	public void RemoveTimetable(int TimetableToRemoveIdx)
	{
		int index = TimetableToRemoveIdx * 24;
		int num = blocks.Count / 24;
		blocks.RemoveRange(index, 24);
		bool flag = TimetableToRemoveIdx == progressTimetableIdx;
		bool flag2 = progressTimetableIdx == num - 1;
		if (TimetableToRemoveIdx < progressTimetableIdx || (flag && flag2))
		{
			progressTimetableIdx--;
		}
		ScheduleScreen.Instance.OnChangeCurrentTimetable();
	}

	public void Assign(Schedulable schedulable)
	{
		if (!IsAssigned(schedulable))
		{
			GetAssigned().Add(new Ref<Schedulable>(schedulable));
		}
		Changed();
	}

	public void Unassign(Schedulable schedulable)
	{
		for (int i = 0; i < GetAssigned().Count; i++)
		{
			if (GetAssigned()[i].Get() == schedulable)
			{
				GetAssigned().RemoveAt(i);
				break;
			}
		}
		Changed();
	}

	public bool IsAssigned(Schedulable schedulable)
	{
		foreach (Ref<Schedulable> item in GetAssigned())
		{
			if (item.Get() == schedulable)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AreScheduleTypesIdentical(List<ScheduleBlockType> a, List<ScheduleBlockType> b)
	{
		if (a.Count != b.Count)
		{
			return false;
		}
		foreach (ScheduleBlockType item in a)
		{
			bool flag = false;
			foreach (ScheduleBlockType item2 in b)
			{
				if (item.IdHash == item2.IdHash)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public bool ShiftTimetable(bool up, int timetableToShiftIdx = 0)
	{
		if (timetableToShiftIdx == 0 && up)
		{
			return false;
		}
		if (timetableToShiftIdx == blocks.Count / 24 - 1 && !up)
		{
			return false;
		}
		int num = timetableToShiftIdx * 24;
		List<ScheduleBlock> list = new List<ScheduleBlock>();
		List<ScheduleBlock> list2 = new List<ScheduleBlock>();
		if (up)
		{
			list = blocks.GetRange(num, 24);
			list2 = blocks.GetRange(num - 24, 24);
			blocks.RemoveRange(num - 24, 48);
			blocks.InsertRange(num - 24, list2);
			blocks.InsertRange(num - 24, list);
		}
		else
		{
			list = blocks.GetRange(num, 24);
			list2 = blocks.GetRange(num + 24, 24);
			blocks.RemoveRange(num, 48);
			blocks.InsertRange(num, list);
			blocks.InsertRange(num, list2);
		}
		Changed();
		return true;
	}

	public void RotateBlocks(bool directionLeft, int timetableToRotateIdx = 0)
	{
		List<ScheduleBlock> list = new List<ScheduleBlock>();
		int index = timetableToRotateIdx * 24;
		list = blocks.GetRange(index, 24);
		if (!directionLeft)
		{
			ScheduleGroup scheduleGroup = Db.Get().ScheduleGroups.Get(list[list.Count - 1].GroupId);
			for (int num = list.Count - 1; num >= 1; num--)
			{
				ScheduleGroup scheduleGroup2 = Db.Get().ScheduleGroups.Get(list[num - 1].GroupId);
				list[num].GroupId = scheduleGroup2.Id;
			}
			list[0].GroupId = scheduleGroup.Id;
		}
		else
		{
			ScheduleGroup scheduleGroup3 = Db.Get().ScheduleGroups.Get(list[0].GroupId);
			for (int i = 0; i < list.Count - 1; i++)
			{
				ScheduleGroup scheduleGroup4 = Db.Get().ScheduleGroups.Get(list[i + 1].GroupId);
				list[i].GroupId = scheduleGroup4.Id;
			}
			list[list.Count - 1].GroupId = scheduleGroup3.Id;
		}
		blocks.RemoveRange(index, 24);
		blocks.InsertRange(index, list);
		Changed();
	}
}
