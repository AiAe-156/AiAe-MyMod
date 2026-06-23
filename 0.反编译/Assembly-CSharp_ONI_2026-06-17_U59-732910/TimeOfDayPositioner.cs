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
		}
		else
		{
			RectTransform rectTransform = TimetableRow.GetComponent<HierarchyReferences>().GetReference<RectTransform>("BlockContainer").rectTransform();
			targetRect = rectTransform;
			base.transform.SetParent(targetRect.transform);
		}
	}

	private void Update()
	{
		if (!(targetRect == null))
		{
			if (base.transform.parent != targetRect.transform)
			{
				base.transform.parent = targetRect.transform;
			}
			float f = GameClock.Instance.GetCurrentCycleAsPercentage() * targetRect.rect.width;
			(base.transform as RectTransform).anchoredPosition = new Vector2(Mathf.Round(f), 0f);
		}
	}
}
