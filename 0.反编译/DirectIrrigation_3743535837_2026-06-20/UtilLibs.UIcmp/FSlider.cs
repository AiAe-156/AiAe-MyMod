using System;
using FMOD.Studio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UtilLibs.UIcmp;

public class FSlider : KMonoBehaviour, IEventSystemHandler, IDragHandler, IPointerDownHandler
{
	public delegate float MapValue(float val);

	public Slider slider;

	public FNumberInputField inputField;

	private readonly float movePlayRate = 0.01f;

	private float lastMoveTime;

	private float lastMoveValue;

	private bool playedBoundaryBump;

	private MapValue mapValue;

	private LocText outputTarget;

	private bool wholeNumbers;

	public int TrailingOutputNumbers = 3;

	public string UnitString = string.Empty;

	public bool WholeNumbers => wholeNumbers;

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

	public event Action<float> OnChange;

	public event Action OnMaxReached;

	public override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		slider = ((Component)this).gameObject.GetComponent<Slider>();
	}

	public void AttachOutputField(LocText targetText)
	{
		outputTarget = targetText;
		UpdateSlider();
	}

	public void SetWholeNumbers(bool wholeNumbers)
	{
		if ((Object)(object)slider != (Object)null)
		{
			slider.wholeNumbers = wholeNumbers;
			this.wholeNumbers = wholeNumbers;
		}
	}

	public void SetCurrent(float current)
	{
		if ((Object)(object)slider != (Object)null)
		{
			slider.value = current;
		}
		SetOutputText();
	}

	public void SetMax(float value)
	{
		if ((Object)(object)slider != (Object)null)
		{
			if (slider.value > value)
			{
				slider.value = value;
			}
			slider.maxValue = value;
		}
	}

	public void SetMin(float value)
	{
		if ((Object)(object)slider != (Object)null)
		{
			if (slider.value < value)
			{
				slider.value = value;
			}
			slider.minValue = value;
		}
	}

	public void SetMinMaxCurrent(float min, float max, float current = -1f)
	{
		SetMin(min);
		SetMax(max);
		SetCurrent(current);
	}

	public void SetInteractable(bool interactable)
	{
		if ((Object)(object)slider != (Object)null)
		{
			((Selectable)slider).interactable = interactable;
		}
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
		this.OnChange?.Invoke(slider.value);
		if (slider.value == slider.maxValue)
		{
			this.OnMaxReached?.Invoke();
		}
		SetOutputText();
	}

	private void SetOutputText()
	{
		if ((Object)(object)outputTarget != (Object)null)
		{
			((TMP_Text)outputTarget).text = slider.value.ToString((!wholeNumbers) ? ("0." + new string('0', TrailingOutputNumbers)) : "0") + UnitString;
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
			this.OnChange?.Invoke(slider.value);
		}
	}

	public void PlayMoveSound()
	{
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		if (!KInputManager.isFocused)
		{
			return;
		}
		float num = Time.unscaledTime - lastMoveTime;
		if (num < movePlayRate)
		{
			return;
		}
		float num2 = Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value);
		string text = null;
		if (num2 == 1f && lastMoveValue == 1f)
		{
			if (!playedBoundaryBump)
			{
				text = UISoundHelper.SliderBoundaryHigh;
				playedBoundaryBump = true;
			}
		}
		else if (num2 == 0f && lastMoveValue == 0f)
		{
			if (!playedBoundaryBump)
			{
				text = UISoundHelper.SliderBoundaryLow;
				playedBoundaryBump = true;
			}
		}
		else if (num2 >= 0f && num2 <= 1f)
		{
			text = UISoundHelper.SliderMove;
			playedBoundaryBump = false;
		}
		if (text != null && text.Length > 0)
		{
			lastMoveTime = Time.unscaledTime;
			lastMoveValue = num2;
			EventInstance val = KFMOD.BeginOneShot(text, Vector3.zero, 1f);
			((EventInstance)(ref val)).setParameterByName("sliderValue", num2, false);
			((EventInstance)(ref val)).setParameterByName("timeSinceLast", num, false);
			KFMOD.EndOneShot(val);
		}
	}
}
