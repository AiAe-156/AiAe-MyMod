using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UtilLibs.UIcmp;

public class FButton : KMonoBehaviour, IEventSystemHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
	public bool PlayClickSound = true;

	private bool interactable;

	private Material material;

	[MyCmpGet]
	private Image image;

	[MyCmpGet]
	private Button button;

	[SerializeField]
	public Color disabledColor = new Color(0.78f, 0.78f, 0.78f);

	[SerializeField]
	public Color normalColor = new Color(0.243f, 0.263f, 0.341f);

	[SerializeField]
	public Color hoverColor = new Color(0.345f, 0.373f, 0.702f);

	public bool allowRightClick = false;

	public event Action OnClick;

	public event Action OnRightClick;

	public event Action OnPointerEnterAction;

	public event Action OnPointerExitAction;

	public override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		if ((Object)(object)button != (Object)null && (Object)(object)((Selectable)button).image != (Object)null)
		{
			image = ((Selectable)button).image;
		}
		if ((Object)(object)image != (Object)null)
		{
			material = ((Graphic)image).material;
		}
		interactable = true;
	}

	public override void OnSpawn()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		((KMonoBehaviour)this).OnSpawn();
		if ((Object)(object)button != (Object)null)
		{
			((Selectable)button).navigation = GetNoNavigation();
		}
	}

	private Navigation GetNoNavigation()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Navigation result = default(Navigation);
		((Navigation)(ref result)).mode = (Mode)0;
		((Navigation)(ref result)).wrapAround = false;
		return result;
	}

	public void SetInteractable(bool interactable)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (Util.IsNullOrDestroyed((object)this) || ((Object)(object)button == (Object)null && (Object)(object)image == (Object)null) || interactable == this.interactable)
		{
			return;
		}
		this.interactable = interactable;
		if ((Object)(object)button == (Object)null)
		{
			if ((Object)(object)image != (Object)null)
			{
				((Graphic)image).color = (interactable ? normalColor : disabledColor);
			}
		}
		else
		{
			((Selectable)button).interactable = interactable;
		}
	}

	public void ClearOnClick()
	{
		this.OnClick = null;
		this.OnRightClick = null;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (!interactable || !KInputManager.isFocused)
		{
			return;
		}
		KInputManager.SetUserActive();
		if (!eventData.IsPointerMoving())
		{
			Button obj = button;
			if (obj != null)
			{
				((Selectable)obj).OnDeselect((BaseEventData)null);
			}
			if (this.OnRightClick != null && (int)eventData.button == 1)
			{
				this.OnRightClick();
			}
			else if (this.OnClick != null && ((int)eventData.button == 0 || allowRightClick))
			{
				this.OnClick?.Invoke();
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (this.OnPointerEnterAction != null)
		{
			this.OnPointerEnterAction();
		}
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
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (this.OnPointerExitAction != null)
		{
			this.OnPointerExitAction();
		}
		if ((Object)(object)button == (Object)null)
		{
			((Graphic)image).color = normalColor;
		}
		else
		{
			((Selectable)button).OnDeselect((BaseEventData)null);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		if (interactable && KInputManager.isFocused)
		{
			KInputManager.SetUserActive();
			if (PlayClickSound && ((this.OnClick != null && ((int)eventData.button == 0 || allowRightClick)) || (this.OnRightClick != null && (int)eventData.button == 1)))
			{
				KMonoBehaviour.PlaySound(UISoundHelper.ClickOpen);
			}
		}
	}
}
