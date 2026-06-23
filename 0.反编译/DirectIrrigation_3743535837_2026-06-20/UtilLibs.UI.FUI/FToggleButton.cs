using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UtilLibs.UIcmp;

namespace UtilLibs.UI.FUI;

public class FToggleButton : KMonoBehaviour, IEventSystemHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	private bool interactable;

	private Material material;

	[MyCmpGet]
	private Image image;

	[MyCmpGet]
	private Button button;

	[SerializeField]
	private bool IsHighlighted = false;

	[SerializeField]
	public Color disabledColor = new Color(0.78f, 0.78f, 0.78f);

	[SerializeField]
	public Color normalColor = new Color(0.243f, 0.263f, 0.341f);

	[SerializeField]
	public Color hoverColor = new Color(0.345f, 0.373f, 0.702f);

	[SerializeField]
	public Color highlightedColor = new Color(0.345f, 0.373f, 0.702f);

	private bool isHovered = false;

	public event Action OnClick;

	public event Action OnDoubleClick = null;

	public event Action OnPointerEnterAction;

	public event Action OnPointerExitAction;

	public override void OnPrefabInit()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		((KMonoBehaviour)this).OnPrefabInit();
		if ((Object)(object)image == (Object)null && (Object)(object)button != (Object)null)
		{
			image = ((Selectable)button).image;
		}
		ColorBlock colors = ((Selectable)button).colors;
		disabledColor = ((ColorBlock)(ref colors)).disabledColor;
		colors = ((Selectable)button).colors;
		normalColor = ((ColorBlock)(ref colors)).normalColor;
		colors = ((Selectable)button).colors;
		hoverColor = ((ColorBlock)(ref colors)).highlightedColor;
		colors = ((Selectable)button).colors;
		highlightedColor = ((ColorBlock)(ref colors)).selectedColor;
		((Behaviour)button).enabled = false;
		material = ((Graphic)image).material;
		interactable = true;
		SetColorState();
	}

	public void SetInteractable(bool interactable)
	{
		if (interactable != this.interactable)
		{
			this.interactable = interactable;
			SetColorState();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		if (interactable && (int)eventData.button <= 0)
		{
		}
	}

	public void ToggleSelection()
	{
		IsHighlighted = !IsHighlighted;
		SetIsSelected(IsHighlighted);
	}

	public void SetIsSelected(bool _isHighlighted = false)
	{
		IsHighlighted = _isHighlighted;
		SetColorState();
	}

	public void Refresh()
	{
		SetColorState();
	}

	private void SetColorState()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)image == (Object)null))
		{
			if (!interactable)
			{
				((Graphic)image).color = disabledColor;
			}
			else if (IsHighlighted)
			{
				((Graphic)image).color = highlightedColor;
			}
			else if (isHovered)
			{
				((Graphic)image).color = hoverColor;
			}
			else
			{
				((Graphic)image).color = normalColor;
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (this.OnPointerEnterAction != null)
		{
			this.OnPointerEnterAction();
		}
		if (interactable)
		{
			if (KInputManager.isFocused)
			{
				KInputManager.SetUserActive();
				KMonoBehaviour.PlaySound(UISoundHelper.MouseOver);
				isHovered = true;
			}
			SetColorState();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (this.OnPointerExitAction != null)
		{
			this.OnPointerExitAction();
		}
		isHovered = false;
		SetColorState();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (interactable && KInputManager.isFocused)
		{
			KInputManager.SetUserActive();
			KMonoBehaviour.PlaySound(UISoundHelper.ClickOpen);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (interactable)
		{
			if (this.OnDoubleClick != null && eventData.clickCount == 2)
			{
				this.OnDoubleClick();
				return;
			}
			ToggleSelection();
			this.OnClick?.Invoke();
		}
	}
}
