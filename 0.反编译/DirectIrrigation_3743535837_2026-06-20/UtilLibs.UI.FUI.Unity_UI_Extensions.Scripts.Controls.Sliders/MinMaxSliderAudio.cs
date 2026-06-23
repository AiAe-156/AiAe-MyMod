using FMOD.Studio;
using UnityEngine;
using UtilLibs.UIcmp;

namespace UtilLibs.UI.FUI.Unity_UI_Extensions.Scripts.Controls.Sliders;

internal class MinMaxSliderAudio : KMonoBehaviour
{
	private readonly float movePlayRate = 0.01f;

	private float lastMoveTime;

	private float lastMoveValue;

	private bool playedBoundaryBump;

	private MinMaxSlider slider;

	public override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
		slider = ((Component)this).GetComponent<MinMaxSlider>();
	}

	public void OnDragEnd()
	{
		if (KInputManager.isFocused)
		{
			KMonoBehaviour.PlaySound(UISoundHelper.SliderEnd);
		}
	}

	public void OnDragStart()
	{
		if (KInputManager.isFocused)
		{
			KMonoBehaviour.PlaySound(UISoundHelper.SliderStart);
		}
	}

	public void OnDrag(bool isMaxSlider)
	{
		if (KInputManager.isFocused)
		{
			PlayMoveSound(isMaxSlider);
		}
	}

	public void PlayMoveSound(bool MaxSlider)
	{
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		if (!KInputManager.isFocused)
		{
			return;
		}
		float num = Time.unscaledTime - lastMoveTime;
		if (num < movePlayRate)
		{
			return;
		}
		float num2 = Mathf.InverseLerp(slider.MinLimit, slider.MaxLimit, MaxSlider ? slider.MaxValue : slider.MinValue);
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
