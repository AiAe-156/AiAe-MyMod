using UnityEngine;
using UnityEngine.UI;

public class LargeImpactorUINotificationHitEffects : KMonoBehaviour, IRenderEveryTick
{
	public Image hitBackgorund;

	public Image heartIcon;

	public Image healthBarFill;

	public UIShake shake;

	public Color HighlightedColor = Color.yellow;

	private Color heartIconOriginalColor;

	private Color healthBarOriginalColor;

	private float duration = 0.4f;

	private float lastTimerValue;

	private float timer;

	private float Intensity => timer / duration;

	public void PlayHitEffect()
	{
		timer = duration;
	}

	public void RenderEveryTick(float dt)
	{
		if (lastTimerValue != timer)
		{
			lastTimerValue = timer;
			hitBackgorund.Opacity(Intensity);
			heartIcon.color = Color.Lerp(heartIconOriginalColor, HighlightedColor, Intensity);
			healthBarFill.color = Color.Lerp(healthBarOriginalColor, HighlightedColor, Intensity);
			shake.SetIntensity(Intensity);
		}
		timer = Mathf.Clamp(timer - dt, 0f, duration);
	}

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		heartIconOriginalColor = heartIcon.color;
		healthBarOriginalColor = healthBarFill.color;
	}
}
