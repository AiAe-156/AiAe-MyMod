using UnityEngine;

public class TimeOfDayPositioner : KMonoBehaviour
{
	private RectTransform targetRect;

	public void SetTargetTimetable(GameObject TimetableRow)
	{
		if (TimetableRow == null)
		{
			targetRect = null;
			base.transform.SetParent(null);
			return;
		}
		HierarchyReferences component = TimetableRow.GetComponent<HierarchyReferences>();
		RectTransform rectTransform = component.GetReference<RectTransform>("BlockContainer").rectTransform();
		targetRect = rectTransform;
		base.transform.SetParent(targetRect.transform);
	}

	private void Update()
	{
		if (!(targetRect == null))
		{
			if (base.transform.parent != targetRect.transform)
			{
				base.transform.parent = targetRect.transform;
			}
			float currentCycleAsPercentage = GameClock.Instance.GetCurrentCycleAsPercentage();
			float f = currentCycleAsPercentage * targetRect.rect.width;
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.anchoredPosition = new Vector2(Mathf.Round(f), 0f);
		}
	}
}
