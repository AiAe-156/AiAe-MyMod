using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("KMonoBehaviour/scripts/ScheduleScreenEntry")]
public class ScheduleScreenEntry : KMonoBehaviour
{
	[SerializeField]
	private ScheduleBlockButton blockButtonPrefab;

	[SerializeField]
	private ScheduleMinionWidget minionWidgetPrefab;

	[SerializeField]
	private GameObject minionWidgetContainer;

	private ScheduleMinionWidget blankMinionWidget;

	[SerializeField]
	private KButton duplicateScheduleButton;

	[SerializeField]
	private KButton deleteScheduleButton;

	[SerializeField]
	private EditableTitleBar title;

	[SerializeField]
	private LocText alarmField;

	[SerializeField]
	private KButton optionsButton;

	[SerializeField]
	private LocText noteEntryLeft;

	[SerializeField]
	private LocText noteEntryRight;

	[SerializeField]
	private MultiToggle alarmButton;

	private List<GameObject> timetableRows;

	private Dictionary<GameObject, List<ScheduleBlockButton>> blockButtonsByTimetableRow;

	private List<ScheduleMinionWidget> minionWidgets;

	[SerializeField]
	private GameObject timetableRowPrefab;

	[SerializeField]
	private GameObject timetableRowContainer;

	private Dictionary<string, GameObject> paintButtons = new Dictionary<string, GameObject>();

	[SerializeField]
	private GameObject PaintButtonBathtime;

	[SerializeField]
	private GameObject PaintButtonWorktime;

	[SerializeField]
	private GameObject PaintButtonRecreation;

	[SerializeField]
	private GameObject PaintButtonSleep;

	[SerializeField]
	private TimeOfDayPositioner timeOfDayPositioner;

	private Dictionary<string, int> blockTypeCounts = new Dictionary<string, int>();

	public Schedule schedule { get; private set; }

	public void Setup(Schedule schedule)
	{
		this.schedule = schedule;
		base.gameObject.name = "Schedule_" + schedule.name;
		title.SetTitle(schedule.name);
		title.OnNameChanged += OnNameChanged;
		duplicateScheduleButton.onClick += DuplicateSchedule;
		deleteScheduleButton.onClick += DeleteSchedule;
		timetableRows = new List<GameObject>();
		blockButtonsByTimetableRow = new Dictionary<GameObject, List<ScheduleBlockButton>>();
		int num = Mathf.CeilToInt(schedule.GetBlocks().Count / 24);
		for (int i = 0; i < num; i++)
		{
			AddTimetableRow(i * 24);
		}
		minionWidgets = new List<ScheduleMinionWidget>();
		blankMinionWidget = Util.KInstantiateUI<ScheduleMinionWidget>(minionWidgetPrefab.gameObject, minionWidgetContainer);
		blankMinionWidget.SetupBlank(schedule);
		RebuildMinionWidgets();
		RefreshStatus();
		RefreshAlarmButton();
		MultiToggle multiToggle = alarmButton;
		multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, new System.Action(OnAlarmClicked));
		schedule.onChanged = (Action<Schedule>)Delegate.Combine(schedule.onChanged, new Action<Schedule>(OnScheduleChanged));
		ConfigPaintButton(PaintButtonBathtime, Db.Get().ScheduleGroups.Hygene, Def.GetUISprite(Assets.GetPrefab(ShowerConfig.ID)).first);
		ConfigPaintButton(PaintButtonWorktime, Db.Get().ScheduleGroups.Worktime, Def.GetUISprite(Assets.GetPrefab("ManualGenerator")).first);
		ConfigPaintButton(PaintButtonRecreation, Db.Get().ScheduleGroups.Recreation, Def.GetUISprite(Assets.GetPrefab("WaterCooler")).first);
		ConfigPaintButton(PaintButtonSleep, Db.Get().ScheduleGroups.Sleep, Def.GetUISprite(Assets.GetPrefab("Bed")).first);
		RefreshPaintButtons();
		RefreshTimeOfDayPositioner();
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		Deregister();
	}

	public void Deregister()
	{
		if (schedule != null)
		{
			Schedule obj = schedule;
			obj.onChanged = (Action<Schedule>)Delegate.Remove(obj.onChanged, new Action<Schedule>(OnScheduleChanged));
		}
	}

	private void DuplicateSchedule()
	{
		ScheduleManager.Instance.DuplicateSchedule(schedule);
	}

	private void DeleteSchedule()
	{
		ScheduleManager.Instance.DeleteSchedule(schedule);
	}

	public void RefreshTimeOfDayPositioner()
	{
		if (schedule.ProgressTimetableIdx >= timetableRows.Count || schedule.ProgressTimetableIdx < 0)
		{
			KCrashReporter.ReportDevNotification("RefreshTimeOfDayPositionerError", Environment.StackTrace, $"DevError: schedule.ProgressTimetableIdx is out of bounds. schedule.name:{schedule.name}, schedule.ProgressTimetableIdx:{schedule.ProgressTimetableIdx}, : timetableRows.Count:{timetableRows.Count}", includeSaveFile: true);
			timeOfDayPositioner.SetTargetTimetable(null);
		}
		else
		{
			GameObject targetTimetable = timetableRows[schedule.ProgressTimetableIdx];
			timeOfDayPositioner.SetTargetTimetable(targetTimetable);
		}
	}

	private void DuplicateTimetableRow(int sourceTimetableIdx)
	{
		List<ScheduleBlock> range = schedule.GetBlocks().GetRange(sourceTimetableIdx * 24, 24);
		List<ScheduleBlock> list = new List<ScheduleBlock>();
		for (int i = 0; i < range.Count; i++)
		{
			list.Add(new ScheduleBlock(range[i].name, range[i].GroupId));
		}
		int num = sourceTimetableIdx + 1;
		schedule.InsertTimetable(num, list);
		AddTimetableRow(num * 24);
	}

	private void AddTimetableRow(int startingBlockIdx)
	{
		GameObject row = Util.KInstantiateUI(timetableRowPrefab, timetableRowContainer, force_active: true);
		int num = startingBlockIdx / 24;
		timetableRows.Insert(num, row);
		row.transform.SetSiblingIndex(num);
		HierarchyReferences component = row.GetComponent<HierarchyReferences>();
		List<ScheduleBlockButton> list = new List<ScheduleBlockButton>();
		for (int i = startingBlockIdx; i < startingBlockIdx + 24; i++)
		{
			GameObject parent = component.GetReference<RectTransform>("BlockContainer").gameObject;
			ScheduleBlockButton scheduleBlockButton = Util.KInstantiateUI<ScheduleBlockButton>(blockButtonPrefab.gameObject, parent, force_active: true);
			scheduleBlockButton.Setup(i - startingBlockIdx);
			scheduleBlockButton.SetBlockTypes(schedule.GetBlock(i).allowed_types);
			list.Add(scheduleBlockButton);
		}
		blockButtonsByTimetableRow.Add(row, list);
		component.GetReference<ScheduleBlockPainter>("BlockPainter").SetEntry(this);
		component.GetReference<KButton>("DuplicateButton").onClick += delegate
		{
			DuplicateTimetableRow(timetableRows.IndexOf(row));
		};
		component.GetReference<KButton>("DeleteButton").onClick += delegate
		{
			RemoveTimetableRow(row);
		};
		component.GetReference<KButton>("RotateLeftButton").onClick += delegate
		{
			schedule.RotateBlocks(directionLeft: true, timetableRows.IndexOf(row));
		};
		component.GetReference<KButton>("RotateRightButton").onClick += delegate
		{
			schedule.RotateBlocks(directionLeft: false, timetableRows.IndexOf(row));
		};
		KButton rotateUpButton = component.GetReference<KButton>("ShiftUpButton");
		rotateUpButton.onClick += delegate
		{
			int timetableToShiftIdx = timetableRows.IndexOf(row);
			schedule.ShiftTimetable(up: true, timetableToShiftIdx);
			if (rotateUpButton.soundPlayer.button_widget_sound_events[0].OverrideAssetName == "ScheduleMenu_Shift_up")
			{
				rotateUpButton.soundPlayer.button_widget_sound_events[0].OverrideAssetName = "ScheduleMenu_Shift_up_reset";
			}
			else
			{
				rotateUpButton.soundPlayer.button_widget_sound_events[0].OverrideAssetName = "ScheduleMenu_Shift_up";
			}
		};
		KButton rotateDownButton = component.GetReference<KButton>("ShiftDownButton");
		rotateDownButton.onClick += delegate
		{
			int timetableToShiftIdx = timetableRows.IndexOf(row);
			schedule.ShiftTimetable(up: false, timetableToShiftIdx);
			if (rotateDownButton.soundPlayer.button_widget_sound_events[0].OverrideAssetName == "ScheduleMenu_Shift_down")
			{
				rotateDownButton.soundPlayer.button_widget_sound_events[0].OverrideAssetName = "ScheduleMenu_Shift_down_reset";
			}
			else
			{
				rotateDownButton.soundPlayer.button_widget_sound_events[0].OverrideAssetName = "ScheduleMenu_Shift_down";
			}
		};
	}

	private void RemoveTimetableRow(GameObject row)
	{
		if (timetableRows.Count != 1)
		{
			timeOfDayPositioner.SetTargetTimetable(null);
			int timetableToRemoveIdx = timetableRows.IndexOf(row);
			timetableRows.Remove(row);
			blockButtonsByTimetableRow.Remove(row);
			UnityEngine.Object.Destroy(row);
			schedule.RemoveTimetable(timetableToRemoveIdx);
		}
	}

	public GameObject GetNameInputField()
	{
		return title.inputField.gameObject;
	}

	private void RebuildMinionWidgets()
	{
		if (this.IsNullOrDestroyed() || !MinionWidgetsNeedRebuild())
		{
			return;
		}
		foreach (ScheduleMinionWidget minionWidget in minionWidgets)
		{
			Util.KDestroyGameObject(minionWidget);
		}
		minionWidgets.Clear();
		foreach (Ref<Schedulable> item in schedule.GetAssigned())
		{
			ScheduleMinionWidget scheduleMinionWidget = Util.KInstantiateUI<ScheduleMinionWidget>(minionWidgetPrefab.gameObject, minionWidgetContainer, force_active: true);
			scheduleMinionWidget.Setup(item.Get());
			minionWidgets.Add(scheduleMinionWidget);
		}
		if (Components.LiveMinionIdentities.Count > schedule.GetAssigned().Count)
		{
			blankMinionWidget.transform.SetAsLastSibling();
			blankMinionWidget.gameObject.SetActive(value: true);
		}
		else
		{
			blankMinionWidget.gameObject.SetActive(value: false);
		}
	}

	private bool MinionWidgetsNeedRebuild()
	{
		List<Ref<Schedulable>> assigned = schedule.GetAssigned();
		if (assigned.Count != minionWidgets.Count)
		{
			return true;
		}
		if (assigned.Count != Components.LiveMinionIdentities.Count != blankMinionWidget.gameObject.activeSelf)
		{
			return true;
		}
		for (int i = 0; i < assigned.Count; i++)
		{
			if (assigned[i].Get() != minionWidgets[i].schedulable)
			{
				return true;
			}
		}
		return false;
	}

	public void RefreshWidgetWorldData()
	{
		foreach (ScheduleMinionWidget minionWidget in minionWidgets)
		{
			if (!minionWidget.IsNullOrDestroyed())
			{
				minionWidget.RefreshWidgetWorldData();
			}
		}
	}

	private void OnNameChanged(string newName)
	{
		schedule.name = newName;
		base.gameObject.name = "Schedule_" + schedule.name;
	}

	private void OnAlarmClicked()
	{
		schedule.alarmActivated = !schedule.alarmActivated;
		RefreshAlarmButton();
	}

	private void RefreshAlarmButton()
	{
		alarmButton.ChangeState(schedule.alarmActivated ? 1 : 0);
		ToolTip component = alarmButton.GetComponent<ToolTip>();
		component.SetSimpleTooltip(schedule.alarmActivated ? UI.SCHEDULESCREEN.ALARM_BUTTON_ON_TOOLTIP : UI.SCHEDULESCREEN.ALARM_BUTTON_OFF_TOOLTIP);
		ToolTipScreen.Instance.MarkTooltipDirty(component);
		alarmField.text = (schedule.alarmActivated ? UI.SCHEDULESCREEN.ALARM_TITLE_ENABLED : UI.SCHEDULESCREEN.ALARM_TITLE_DISABLED);
	}

	private void OnResetClicked()
	{
		schedule.SetBlocksToGroupDefaults(Db.Get().ScheduleGroups.allGroups);
	}

	private void OnDeleteClicked()
	{
		ScheduleManager.Instance.DeleteSchedule(schedule);
	}

	private void OnScheduleChanged(Schedule changedSchedule)
	{
		foreach (KeyValuePair<GameObject, List<ScheduleBlockButton>> item in blockButtonsByTimetableRow)
		{
			GameObject key = item.Key;
			int num = timetableRows.IndexOf(key);
			List<ScheduleBlockButton> value = item.Value;
			for (int i = 0; i < value.Count; i++)
			{
				int idx = num * 24 + i;
				value[i].SetBlockTypes(changedSchedule.GetBlock(idx).allowed_types);
			}
		}
		RefreshStatus();
		RebuildMinionWidgets();
	}

	private void RefreshStatus()
	{
		blockTypeCounts.Clear();
		foreach (ScheduleBlockType resource in Db.Get().ScheduleBlockTypes.resources)
		{
			blockTypeCounts[resource.Id] = 0;
		}
		foreach (ScheduleBlock block in schedule.GetBlocks())
		{
			foreach (ScheduleBlockType allowed_type in block.allowed_types)
			{
				blockTypeCounts[allowed_type.Id]++;
			}
		}
		if (noteEntryRight == null)
		{
			return;
		}
		int num = 0;
		ToolTip component = noteEntryRight.GetComponent<ToolTip>();
		component.ClearMultiStringTooltip();
		foreach (KeyValuePair<string, int> blockTypeCount in blockTypeCounts)
		{
			if (blockTypeCount.Value == 0)
			{
				num++;
				component.AddMultiStringTooltip(string.Format(UI.SCHEDULEGROUPS.NOTIME, Db.Get().ScheduleBlockTypes.Get(blockTypeCount.Key).Name), null);
			}
		}
		if (num > 0)
		{
			noteEntryRight.text = string.Format(UI.SCHEDULEGROUPS.MISSINGBLOCKS, num);
		}
		else
		{
			noteEntryRight.text = "";
		}
	}

	private void ConfigPaintButton(GameObject button, ScheduleGroup group, Sprite iconSprite)
	{
		string groupID = group.Id;
		button.GetComponent<MultiToggle>().onClick = delegate
		{
			ScheduleScreen.Instance.SelectedPaint = groupID;
			ScheduleScreen.Instance.RefreshAllPaintButtons();
		};
		paintButtons.Add(group.Id, button);
		HierarchyReferences component = button.GetComponent<HierarchyReferences>();
		component.GetReference<Image>("Icon").sprite = iconSprite;
		component.GetReference<LocText>("Label").text = group.Name;
	}

	public void RefreshPaintButtons()
	{
		foreach (KeyValuePair<string, GameObject> paintButton in paintButtons)
		{
			paintButton.Value.GetComponent<MultiToggle>().ChangeState((paintButton.Key == ScheduleScreen.Instance.SelectedPaint) ? 1 : 0);
		}
	}

	public bool PaintBlock(ScheduleBlockButton blockButton)
	{
		foreach (KeyValuePair<GameObject, List<ScheduleBlockButton>> item in blockButtonsByTimetableRow)
		{
			GameObject key = item.Key;
			for (int i = 0; i < item.Value.Count; i++)
			{
				if (item.Value[i] == blockButton)
				{
					int idx = timetableRows.IndexOf(key) * 24 + i;
					ScheduleGroup scheduleGroup = Db.Get().ScheduleGroups.Get(ScheduleScreen.Instance.SelectedPaint);
					if (schedule.GetBlock(idx).GroupId != scheduleGroup.Id)
					{
						schedule.SetBlockGroup(idx, scheduleGroup);
						return true;
					}
					return false;
				}
			}
		}
		return false;
	}
}
