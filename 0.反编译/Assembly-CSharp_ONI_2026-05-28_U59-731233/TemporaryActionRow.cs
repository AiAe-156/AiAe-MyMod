using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TemporaryActionRow : KMonoBehaviour, IRender200ms
{
	public const float ROW_HEIGHT_ANIM_ENTRY_DURATION = 0.5f;

	public const float ROW_HEIGHT_ANIM_EXIT_DURATION = 0.3f;

	public const float SLIDE_ENTER_ANIM_DURATION = 0.4f;

	public const float SLIDE_EXIT_ANIM_DURATION = 0.4f;

	public RectTransform Content;

	public RectTransform IconSection;

	public RectTransform TimeoutBarSection;

	public KImage Image;

	public Image TimeoutImage;

	public LocText Label;

	public ToolTip Tooltip;

	public Action<TemporaryActionRow> OnRowClicked = null;

	public Action<TemporaryActionRow> OnRowHidden = null;

	private LayoutElement layoutElement;

	private Coroutine layoutCoroutine;

	private Button button;

	private bool HasBeenShown = false;

	private float lastSpecifiedLifetime = -1f;

	public float MaxHeight { get; private set; }

	public bool IsVisible { get; private set; }

	public bool ShouldProgressBarBeEnabled => ShowTimeout && Lifetime > 0f && lastSpecifiedLifetime > 0f;

	public float Lifetime { get; private set; } = -1f;

	public bool ShowTimeout { get; set; } = true;

	public bool ShowOnSpawn { get; set; } = true;

	public bool HideOnClick { get; set; } = true;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		layoutElement = GetComponent<LayoutElement>();
		button = GetComponent<Button>();
		button.onClick.AddListener(_OnRowClicked);
		MaxHeight = layoutElement.minHeight;
		HideImmediatly();
	}

	private void Update()
	{
		if (!HasBeenShown && ShowOnSpawn)
		{
			RefreshContentWidth();
			if (Content.sizeDelta.x > 0f)
			{
				Show();
			}
		}
	}

	private void _OnRowClicked()
	{
		OnRowClicked?.Invoke(this);
		if (HideOnClick)
		{
			Hide();
		}
	}

	private void _OnRowHidden()
	{
		OnRowHidden?.Invoke(this);
	}

	protected override void OnCmpEnable()
	{
		base.OnCmpEnable();
		if (base.isSpawned)
		{
			RefreshContentWidth();
		}
	}

	protected override void OnCmpDisable()
	{
		base.OnCmpDisable();
		HideImmediatly();
		_OnRowHidden();
	}

	public void SetLifetime(float lifetime)
	{
		Lifetime = lifetime;
		lastSpecifiedLifetime = lifetime;
		UpdateTimeout();
	}

	private void UpdateTimeout()
	{
		bool shouldProgressBarBeEnabled = ShouldProgressBarBeEnabled;
		if (shouldProgressBarBeEnabled != TimeoutBarSection.gameObject.activeInHierarchy)
		{
			TimeoutBarSection.gameObject.SetActive(shouldProgressBarBeEnabled);
		}
		if (shouldProgressBarBeEnabled)
		{
			TimeoutImage.fillAmount = Mathf.Clamp(Lifetime / lastSpecifiedLifetime, 0f, 1f);
		}
	}

	public void Render200ms(float dt)
	{
		if (HasBeenShown && Lifetime > 0f && IsVisible)
		{
			Lifetime -= dt;
			if (Lifetime <= 0f)
			{
				Hide();
			}
			UpdateTimeout();
		}
	}

	public void Setup(string text, string tooltip, Sprite icon = null)
	{
		Label.SetText(text);
		Tooltip.SetSimpleTooltip(tooltip);
		Image.sprite = icon;
		IconSection.gameObject.SetActive(icon != null);
	}

	public void Show()
	{
		AbortCoroutine();
		IsVisible = true;
		HasBeenShown = true;
		button.interactable = true;
		if (base.gameObject.activeInHierarchy)
		{
			SetContentToHiddenPosition();
			layoutCoroutine = RunEnterHeightAnimation(delegate
			{
				layoutCoroutine = RunEnterSlideAnimation();
			});
		}
	}

	public void HideImmediatly()
	{
		AbortCoroutine();
		IsVisible = false;
		Content.localPosition = new Vector3(0f - (base.transform as RectTransform).sizeDelta.x, Content.localPosition.y, Content.localPosition.z);
		layoutElement.minHeight = 0f;
		button.interactable = false;
	}

	public void Hide()
	{
		AbortCoroutine();
		IsVisible = false;
		button.interactable = false;
		if (base.gameObject.activeInHierarchy)
		{
			layoutCoroutine = RunExitSlideAnimation(delegate
			{
				layoutCoroutine = RunExitHeightAnimation(_OnRowHidden);
			});
		}
	}

	private void AbortCoroutine()
	{
		if (layoutCoroutine != null)
		{
			StopCoroutine(layoutCoroutine);
			layoutCoroutine = null;
		}
	}

	private void RefreshContentWidth()
	{
		RectTransform rectTransform = base.transform as RectTransform;
		if (rectTransform.sizeDelta.x != Content.sizeDelta.x)
		{
			Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.sizeDelta.x);
		}
	}

	private void SetContentToHiddenPosition()
	{
		RefreshContentWidth();
		Vector3 vector = Content.anchoredPosition;
		vector.x = 0f - (base.transform as RectTransform).sizeDelta.x;
		Content.anchoredPosition = vector;
	}

	private Coroutine RunEnterSlideAnimation(System.Action onAnimationEnds = null)
	{
		return StartCoroutine(SlideTransitionAnimation(0.4f, show: true, (float n) => Mathf.Sqrt(n), onAnimationEnds));
	}

	private Coroutine RunExitSlideAnimation(System.Action onAnimationEnds = null)
	{
		return StartCoroutine(SlideTransitionAnimation(0.4f, show: false, (float n) => Mathf.Pow(n, 2f), onAnimationEnds));
	}

	private Coroutine RunEnterHeightAnimation(System.Action onAnimationEnds = null)
	{
		return StartCoroutine(HeightTransitionAnimation(0.5f, show: true, (float n) => Mathf.Sqrt(n), onAnimationEnds));
	}

	private Coroutine RunExitHeightAnimation(System.Action onAnimationEnds = null)
	{
		return StartCoroutine(HeightTransitionAnimation(0.3f, show: false, (float n) => Mathf.Pow(n, 2f), onAnimationEnds));
	}

	private IEnumerator SlideTransitionAnimation(float duration, bool show, Func<float, float> curveModifier = null, System.Action onAnimationEnds = null)
	{
		RectTransform transform = base.transform as RectTransform;
		float hiddenPosition = 0f - transform.sizeDelta.x;
		float visiblePosition = 0f;
		float contentInitialXPosition = (show ? hiddenPosition : Content.anchoredPosition.x);
		float targetPosition = (show ? visiblePosition : hiddenPosition);
		float timePassed = 0f;
		_ = (Vector3)Content.anchoredPosition;
		Vector3 position;
		while (timePassed < duration)
		{
			RefreshContentWidth();
			float n = timePassed / duration;
			if (curveModifier != null)
			{
				n = curveModifier(n);
			}
			position = Content.anchoredPosition;
			position.x = Mathf.Lerp(contentInitialXPosition, targetPosition, n);
			Content.anchoredPosition = position;
			timePassed += Time.unscaledDeltaTime;
			yield return null;
		}
		RefreshContentWidth();
		position = Content.anchoredPosition;
		position.x = targetPosition;
		Content.anchoredPosition = position;
		yield return null;
		onAnimationEnds?.Invoke();
	}

	private IEnumerator HeightTransitionAnimation(float duration, bool show, Func<float, float> curveModifier = null, System.Action onAnimationEnds = null)
	{
		_ = base.transform is RectTransform;
		float initialHeight = layoutElement.minHeight;
		float targetHeight = (show ? MaxHeight : 0f);
		float timePassed = 0f;
		_ = layoutElement.minHeight;
		float height;
		while (timePassed < duration)
		{
			RefreshContentWidth();
			float n = timePassed / duration;
			if (curveModifier != null)
			{
				n = curveModifier(n);
			}
			height = Mathf.Lerp(initialHeight, targetHeight, n);
			layoutElement.minHeight = height;
			timePassed += Time.unscaledDeltaTime;
			yield return null;
		}
		RefreshContentWidth();
		height = targetHeight;
		layoutElement.minHeight = height;
		yield return null;
		onAnimationEnds?.Invoke();
	}
}
