using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScheduleScreen : KScreen
{
	public static ScheduleScreen Instance;

	[SerializeField]
	private ScheduleScreenEntry scheduleEntryPrefab;

	[SerializeField]
	private GameObject scheduleEntryContainer;

	[SerializeField]
	private KButton addScheduleButton;

	[SerializeField]
	private KButton closeButton;

	[SerializeField]
	private GameObject bottomSpacer;

	private List<ScheduleScreenEntry> scheduleEntries;

	private int uiRefreshHandle;

	public string SelectedPaint { get; set; }

	public override float GetSortKey()
	{
		return 50f;
	}

	protected override void OnPrefabInit()
	{
		base.ConsumeMouseScroll = true;
		scheduleEntries = new List<ScheduleScreenEntry>();
		Instance = this;
	}

	protected override void OnSpawn()
	{
		foreach (Schedule schedule in ScheduleManager.Instance.GetSchedules())
		{
			AddScheduleEntry(schedule);
		}
		addScheduleButton.onClick += OnAddScheduleClick;
		closeButton.onClick += delegate
		{
			ManagementMenu.Instance.CloseAll();
		};
		ScheduleManager.Instance.onSchedulesChanged += OnSchedulesChanged;
		Game.Instance.Subscribe(1983128072, RefreshWidgetWorldData);
		uiRefreshHandle = Subscribe(1980521255, RefreshWidgetWorldData);
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		ScheduleManager.Instance.onSchedulesChanged -= OnSchedulesChanged;
		Instance = null;
		Unsubscribe(ref uiRefreshHandle);
	}

	protected override void OnShow(bool show)
	{
		base.OnShow(show);
		if (show)
		{
			Activate();
			SetScreenHeight();
		}
	}

	private void SetScreenHeight()
	{
		bool flag = ScheduleManager.Instance.GetSchedules().Count == 1;
		GetComponent<LayoutElement>().preferredHeight = (flag ? 410 : 604);
		bottomSpacer.SetActive(flag);
	}

	public void RefreshAllPaintButtons()
	{
		foreach (ScheduleScreenEntry scheduleEntry in scheduleEntries)
		{
			scheduleEntry.RefreshPaintButtons();
		}
	}

	private void OnAddScheduleClick()
	{
		ScheduleManager.Instance.AddDefaultSchedule(alarmOn: false, useDefaultName: false);
	}

	private void AddScheduleEntry(Schedule schedule)
	{
		ScheduleScreenEntry scheduleScreenEntry = Util.KInstantiateUI<ScheduleScreenEntry>(scheduleEntryPrefab.gameObject, scheduleEntryContainer, force_active: true);
		scheduleScreenEntry.Setup(schedule);
		scheduleEntries.Add(scheduleScreenEntry);
		SetScreenHeight();
	}

	private void OnSchedulesChanged(List<Schedule> schedules)
	{
		foreach (ScheduleScreenEntry scheduleEntry in scheduleEntries)
		{
			scheduleEntry.Deregister();
			Util.KDestroyGameObject(scheduleEntry.gameObject);
		}
		scheduleEntries.Clear();
		foreach (Schedule schedule in schedules)
		{
			AddScheduleEntry(schedule);
		}
		SetScreenHeight();
	}

	private void RefreshWidgetWorldData(object data = null)
	{
		foreach (ScheduleScreenEntry scheduleEntry in scheduleEntries)
		{
			scheduleEntry.RefreshWidgetWorldData();
		}
	}

	public void OnChangeCurrentTimetable()
	{
		foreach (ScheduleScreenEntry scheduleEntry in scheduleEntries)
		{
			scheduleEntry.RefreshTimeOfDayPositioner();
		}
	}

	public override void OnKeyDown(KButtonEvent e)
	{
		if (CheckBlockedInput())
		{
			if (!e.Consumed)
			{
				e.Consumed = true;
			}
		}
		else
		{
			base.OnKeyDown(e);
		}
	}

	private bool CheckBlockedInput()
	{
		bool result = false;
		if (UnityEngine.EventSystems.EventSystem.current != null)
		{
			GameObject currentSelectedGameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
			if (currentSelectedGameObject != null)
			{
				foreach (ScheduleScreenEntry scheduleEntry in scheduleEntries)
				{
					if (currentSelectedGameObject == scheduleEntry.GetNameInputField())
					{
						result = true;
						break;
					}
				}
			}
		}
		return result;
	}
}
