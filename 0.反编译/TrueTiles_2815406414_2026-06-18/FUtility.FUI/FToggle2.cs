using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FUtility.FUI;

public class FToggle2 : KMonoBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler
{
	[SerializeField]
	public Image mark;

	private bool on;

	public bool On
	{
		get
		{
			return on;
		}
		set
		{
			on = value;
			if ((Object)(object)mark != (Object)null)
			{
				((Behaviour)mark).enabled = value;
			}
			this.OnChange?.Invoke(value);
		}
	}

	public event Action OnClick;

	public event Action<bool> OnChange;

	public void SetOnWithoutTrigger(bool value)
	{
		on = value;
		if ((Object)(object)mark != (Object)null)
		{
			((Behaviour)mark).enabled = value;
		}
	}

	protected override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
	}

	public void SetCheckmark(string path)
	{
		mark = ((Component)((KMonoBehaviour)this).transform.Find(path)).GetComponent<Image>();
	}

	protected override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
	}

	public void Toggle()
	{
		On = !On;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (KInputManager.isFocused)
		{
			KInputManager.SetUserActive();
			KMonoBehaviour.PlaySound(UISoundHelper.Click);
			Toggle();
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
