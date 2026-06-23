using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UtilLibs.UIcmp;

public class FToggle : KMonoBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler
{
	[SerializeField]
	public Image mark;

	private bool _interactable = true;

	private bool on;

	public bool Interactable => _interactable;

	public bool On
	{
		get
		{
			return on;
		}
		set
		{
			on = value;
			if ((Object)(object)mark != (Object)null && Interactable)
			{
				((Behaviour)mark).enabled = value;
				this.OnChange?.Invoke(value);
			}
		}
	}

	public event Action<bool> OnClick;

	public event Action<bool> OnChange;

	public void SetInteractable(bool interactable)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		_interactable = interactable;
		if ((Object)(object)mark != (Object)null)
		{
			((Graphic)mark).color = (Color)(_interactable ? Color.white : new Color(1f, 1f, 1f, 0.5f));
		}
	}

	public override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		if ((Object)(object)mark == (Object)null)
		{
			mark = ((Component)this).gameObject.GetComponentInChildren<Image>();
		}
	}

	public void SetCheckmark(string path)
	{
		mark = ((Component)((KMonoBehaviour)this).transform.Find(path)).GetComponent<Image>();
	}

	public override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
	}

	public void Toggle()
	{
		On = !On;
	}

	public void SetOn(bool toggleOn)
	{
		On = toggleOn;
	}

	public void SetOnFromCode(bool setOn)
	{
		on = setOn;
		if ((Object)(object)mark != (Object)null)
		{
			((Behaviour)mark).enabled = on;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (KInputManager.isFocused && Interactable)
		{
			KInputManager.SetUserActive();
			KMonoBehaviour.PlaySound(UISoundHelper.Click);
			Toggle();
			this.OnClick?.Invoke(On);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (KInputManager.isFocused && Interactable)
		{
			KInputManager.SetUserActive();
			KMonoBehaviour.PlaySound(UISoundHelper.MouseOver);
		}
	}
}
