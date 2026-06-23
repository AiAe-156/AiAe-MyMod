using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.EventSystems;

[AddComponentMenu("KMonoBehaviour/scripts/ScheduleBlockPainter")]
public class ScheduleBlockPainter : KMonoBehaviour, IPointerDownHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	private ScheduleScreenEntry entry;

	private static int paintCounter;

	private GameObject previousBlockTriedPainted = null;

	public void SetEntry(ScheduleScreenEntry entry)
	{
		this.entry = entry;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		PaintBlocksBelow(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		PaintBlocksBelow(eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		PaintBlocksBelow(eventData);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		paintCounter = 0;
		PaintBlocksBelow(eventData);
	}

	private void PaintBlocksBelow(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left || ScheduleScreen.Instance.SelectedPaint.IsNullOrWhiteSpace())
		{
			return;
		}
		List<RaycastResult> list = new List<RaycastResult>();
		UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, list);
		if (list == null || list.Count <= 0)
		{
			return;
		}
		ScheduleBlockButton component = list[0].gameObject.GetComponent<ScheduleBlockButton>();
		if (!(component != null))
		{
			return;
		}
		if (entry.PaintBlock(component))
		{
			string sound = GlobalAssets.GetSound("ScheduleMenu_Select");
			if (sound != null)
			{
				EventInstance instance = SoundEvent.BeginOneShot(sound, SoundListenerController.Instance.transform.GetPosition());
				instance.setParameterByName("Drag_Count", paintCounter);
				paintCounter++;
				SoundEvent.EndOneShot(instance);
				previousBlockTriedPainted = component.gameObject;
			}
		}
		else if (previousBlockTriedPainted != component.gameObject)
		{
			previousBlockTriedPainted = component.gameObject;
			string sound2 = GlobalAssets.GetSound("ScheduleMenu_Select_none");
			if (sound2 != null)
			{
				EventInstance instance2 = SoundEvent.BeginOneShot(sound2, SoundListenerController.Instance.transform.GetPosition());
				SoundEvent.EndOneShot(instance2);
			}
		}
	}
}
