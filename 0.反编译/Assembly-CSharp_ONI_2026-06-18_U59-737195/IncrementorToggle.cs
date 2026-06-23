using UnityEngine;
using UnityEngine.EventSystems;

public class IncrementorToggle : MultiToggle
{
	private float timeBetweenIncrementsMin = 0.033f;

	private float timeBetweenIncrementsMax = 0.25f;

	private const float incrementAccelerationScale = 2.5f;

	private float timeToNextIncrement;

	protected override void Update()
	{
		if (clickHeldDown)
		{
			totalHeldTime += Time.unscaledDeltaTime;
			if (timeToNextIncrement <= 0f)
			{
				PlayClickSound();
				onClick();
				timeToNextIncrement = Mathf.Lerp(timeBetweenIncrementsMax, timeBetweenIncrementsMin, totalHeldTime / 2.5f);
			}
			else
			{
				timeToNextIncrement -= Time.unscaledDeltaTime;
			}
		}
	}

	private void PlayClickSound()
	{
		if (play_sound_on_click)
		{
			string text = states[state].on_click_override_sound_path;
			if (text == "")
			{
				text = "HUD_Click";
			}
			KFMOD.PlayUISound(GlobalAssets.GetSound(text));
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		if (AllowClick(eventData))
		{
			timeToNextIncrement = timeBetweenIncrementsMax;
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (!AllowClick(eventData))
		{
			return;
		}
		if (!clickHeldDown)
		{
			clickHeldDown = true;
			PlayClickSound();
			if (onClick != null)
			{
				onClick();
			}
		}
		if (states.Length - 1 < state)
		{
			Debug.LogWarning("Multi toggle has too few / no states");
		}
		RefreshHoverColor();
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		RefreshHoverColor();
	}
}
