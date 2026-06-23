using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FUtility.FUI;

public class FSlider : KMonoBehaviour, IEventSystemHandler, IDragHandler, IPointerDownHandler
{
	public delegate float MapValue(float val);

	public Slider slider;

	public FNumberInputField inputField;

	private readonly float movePlayRate = 0.05f;

	private float lastMoveTime;

	private float lastMoveValue;

	private bool playedBoundaryBump;

	private MapValue mapValue;

	public float Value
	{
		get
		{
			return slider.value;
		}
		set
		{
			slider.value = value;
		}
	}

	public event Action OnChange;

	public event Action OnMaxReached;

	protected override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		slider = ((Component)this).gameObject.GetComponent<Slider>();
	}

	public void AttachInputField(FNumberInputField field, MapValue map = null)
	{
		mapValue = map;
		inputField = field;
		inputField.OnEndEdit += OnInputFieldChanged;
	}

	private void OnInputFieldChanged()
	{
		float num = inputField.GetFloat;
		if (mapValue != null)
		{
			num = mapValue(num);
		}
		slider.value = num;
		UpdateSlider();
	}

	private void UpdateSlider()
	{
		this.OnChange?.Invoke();
		if (slider.value == slider.maxValue)
		{
			this.OnMaxReached?.Invoke();
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (KInputManager.isFocused)
		{
			KInputManager.SetUserActive();
			PlayMoveSound();
			UpdateSlider();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (KInputManager.isFocused)
		{
			KInputManager.SetUserActive();
			KMonoBehaviour.PlaySound(UISoundHelper.SliderStart);
			this.OnChange?.Invoke();
		}
	}

	public void PlayMoveSound()
	{
	}
}
