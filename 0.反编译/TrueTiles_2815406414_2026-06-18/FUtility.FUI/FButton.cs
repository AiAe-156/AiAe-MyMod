using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FUtility.FUI;

public class FButton : KMonoBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
	private bool interactable;

	private Material material;

	[MyCmpReq]
	private Image image;

	[MyCmpGet]
	private Button button;

	[SerializeField]
	public Color disabledColor = new Color(0.78f, 0.78f, 0.78f);

	[SerializeField]
	public Color normalColor = new Color(0.243f, 0.263f, 0.341f);

	[SerializeField]
	public Color hoverColor = new Color(0.345f, 0.373f, 0.702f);

	public event Action OnClick;

	protected override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		material = ((Graphic)image).material;
		interactable = true;
	}

	public void SetInteractable(bool interactable)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (interactable != this.interactable)
		{
			this.interactable = interactable;
			if ((Object)(object)button == (Object)null)
			{
				((Graphic)image).color = (interactable ? normalColor : disabledColor);
			}
			else
			{
				((Selectable)button).interactable = interactable;
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (interactable && KInputManager.isFocused)
		{
			KInputManager.SetUserActive();
			KMonoBehaviour.PlaySound(UISoundHelper.ClickOpen);
			this.OnClick?.Invoke();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (interactable && KInputManager.isFocused)
		{
			if ((Object)(object)button == (Object)null)
			{
				((Graphic)image).color = hoverColor;
			}
			KInputManager.SetUserActive();
			KMonoBehaviour.PlaySound(UISoundHelper.MouseOver);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)button == (Object)null)
		{
			((Graphic)image).color = normalColor;
		}
	}
}
