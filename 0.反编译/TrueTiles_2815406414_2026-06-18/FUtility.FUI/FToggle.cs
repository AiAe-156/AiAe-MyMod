using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FUtility.FUI;

public class FToggle : KMonoBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler
{
	[MyCmpReq]
	public Toggle toggle;

	public event Action OnClick;

	public void OnPointerDown(PointerEventData eventData)
	{
		if (KInputManager.isFocused)
		{
			KInputManager.SetUserActive();
			KMonoBehaviour.PlaySound(UISoundHelper.Click);
			this.OnClick?.Invoke();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (KInputManager.isFocused)
		{
			KInputManager.SetUserActive();
			KMonoBehaviour.PlaySound(UISoundHelper.MouseOver);
		}
	}
}
