using System.Collections.Generic;
using TUNING;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("KMonoBehaviour/scripts/ScheduleBlockButton")]
public class ScheduleBlockButton : KMonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private ToolTip toolTip;

	[SerializeField]
	private GameObject highlightObject;

	public void Setup(int hour)
	{
		if (hour < TRAITS.EARLYBIRD_SCHEDULEBLOCK)
		{
			HierarchyReferences component = GetComponent<HierarchyReferences>();
			component.GetReference<RectTransform>("MorningIcon").gameObject.SetActive(value: true);
		}
		else if (hour >= 21)
		{
			HierarchyReferences component2 = GetComponent<HierarchyReferences>();
			component2.GetReference<RectTransform>("NightIcon").gameObject.SetActive(value: true);
		}
		base.gameObject.name = "ScheduleBlock_" + hour;
		ToggleHighlight(on: false);
	}

	public void SetBlockTypes(List<ScheduleBlockType> blockTypes)
	{
		ScheduleGroup scheduleGroup = Db.Get().ScheduleGroups.FindGroupForScheduleTypes(blockTypes);
		if (scheduleGroup != null)
		{
			image.color = scheduleGroup.uiColor;
			toolTip.SetSimpleTooltip(scheduleGroup.Name);
		}
		else
		{
			toolTip.SetSimpleTooltip("UNKNOWN");
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ToggleHighlight(on: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ToggleHighlight(on: false);
	}

	private void ToggleHighlight(bool on)
	{
		highlightObject.SetActive(on);
	}
}
