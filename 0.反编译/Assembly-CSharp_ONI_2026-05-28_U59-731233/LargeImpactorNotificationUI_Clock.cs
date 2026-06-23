using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LargeImpactorNotificationUI_Clock : KMonoBehaviour, ISim4000ms
{
	public KImage LargeNeedle;

	public RectTransform SmallNeedlePivot;

	public KImage NeedleTrailBg;

	public Image TimerOutCircleFill;

	public Image TimerOutCircleBG;

	private Color color_circleFillOriginalColor;

	private Color color_needleTrailBgOriginalColor;

	private Color color_timerOutCircleBGOriginalColor;

	private Color softRed;

	private Coroutine animationCoroutine;

	private bool hasSpawned = false;

	private float entryAnimationDuration = 1f;

	private float reminderAnimationDuration = 16f;

	private bool hasPlayedEntryAnimation = false;

	private float reminderAnimationTimer = -1f;

	private float lastLargeImpactorTime = -1f;

	private const int reminderSetting_BlinkTimes = 16;

	protected override void OnSpawn()
	{
		color_circleFillOriginalColor = TimerOutCircleFill.color;
		color_needleTrailBgOriginalColor = NeedleTrailBg.color;
		color_timerOutCircleBGOriginalColor = TimerOutCircleBG.color;
		softRed = new Color(1f, 0f, 0f, color_needleTrailBgOriginalColor.a);
		GameClock.Instance.Subscribe(631075836, OnNewCycleReached);
		UpdateSmallNeedlePosition();
		InitializeAnimationCoroutine();
		hasSpawned = true;
	}

	private void OnNewCycleReached(object data)
	{
		PlayReminderAnimation();
	}

	public void SetLargeImpactorTime(float normalizedValue)
	{
		lastLargeImpactorTime = normalizedValue;
		SetNeedleRotation(LargeNeedle.rectTransform, 1f - lastLargeImpactorTime);
		if (hasPlayedEntryAnimation)
		{
			TimerOutCircleFill.fillAmount = lastLargeImpactorTime;
		}
	}

	private void SetNeedleRotation(RectTransform needle, float normalizedTime)
	{
		needle.localRotation = Quaternion.Euler(0f, 0f, -360f * normalizedTime);
		if (needle.gameObject == LargeNeedle.gameObject)
		{
			NeedleTrailBg.fillAmount = normalizedTime;
		}
	}

	public void Sim4000ms(float dt)
	{
		UpdateSmallNeedlePosition();
	}

	private void UpdateSmallNeedlePosition()
	{
		float currentCycleAsPercentage = GameClock.Instance.GetCurrentCycleAsPercentage();
		SetNeedleRotation(SmallNeedlePivot, currentCycleAsPercentage);
	}

	private void InitializeAnimationCoroutine()
	{
		AbortCoroutine();
		animationCoroutine = StartCoroutine(AnimationCoroutineLogic());
	}

	private void AbortCoroutine()
	{
		if (animationCoroutine != null)
		{
			StopAllCoroutines();
		}
		animationCoroutine = null;
	}

	public void PlayReminderAnimation()
	{
		reminderAnimationTimer = 0f;
	}

	private IEnumerator AnimationCoroutineLogic()
	{
		if (!hasPlayedEntryAnimation)
		{
			_ = 0f - (0f + (1f - (GameClock.Instance.GetCurrentCycleAsPercentage() + entryAnimationDuration / 600f)));
			yield return this.Interpolate(delegate(float num)
			{
				TimerOutCircleFill.fillAmount = num * lastLargeImpactorTime;
			}, entryAnimationDuration);
			hasPlayedEntryAnimation = true;
		}
		while (true)
		{
			if (reminderAnimationTimer < 0f)
			{
				yield return null;
			}
			if (reminderAnimationTimer >= 0f && reminderAnimationTimer < reminderAnimationDuration)
			{
				float n = reminderAnimationTimer / reminderAnimationDuration;
				float blinkLevel = Mathf.Abs(Mathf.Sin(n * MathF.PI * 16f));
				TimerOutCircleBG.color = Color.Lerp(color_timerOutCircleBGOriginalColor, Color.red, blinkLevel);
				NeedleTrailBg.color = Color.Lerp(color_needleTrailBgOriginalColor, softRed, blinkLevel);
				reminderAnimationTimer += Time.deltaTime;
				yield return null;
			}
			if (reminderAnimationTimer >= reminderAnimationDuration)
			{
				TimerOutCircleBG.color = color_timerOutCircleBGOriginalColor;
				NeedleTrailBg.color = color_needleTrailBgOriginalColor;
				reminderAnimationTimer = -1f;
				yield return null;
			}
		}
	}

	protected override void OnCmpEnable()
	{
		base.OnCmpEnable();
		if (hasSpawned)
		{
			InitializeAnimationCoroutine();
		}
	}

	protected override void OnCmpDisable()
	{
		base.OnCmpDisable();
		AbortCoroutine();
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		GameClock.Instance.Unsubscribe(631075836, OnNewCycleReached);
	}
}
