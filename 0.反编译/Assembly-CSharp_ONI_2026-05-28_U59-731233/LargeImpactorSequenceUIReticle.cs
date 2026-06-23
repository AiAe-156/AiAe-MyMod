using System;
using System.Collections;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class LargeImpactorSequenceUIReticle : KMonoBehaviour
{
	private const float reticleEnterDuration = 0.4f;

	private const float flashDuration = 0.4f;

	private const int flashTimes = 3;

	private const float reticleZoomOutDuration = 0.8f;

	private const float labelRevealDuration = 1f;

	private const float sidePanel_TitleRevealDuration = 0.5f;

	private const float sidePanel_DescriptionRevealDuration = 1.5f;

	private const float exitToCalculationDuration = 0.5f;

	private const float expandReticleHorizontallyDuration = 0.8f;

	private const float calculateImpactZoneTextRevealDuration = 0.5f;

	private const float exitDuration = 0.8f;

	public const float RevealPOI_LandingZone_Duration = 3.5f;

	private const string Sound_LockTarget = "HUD_Imperative_analysis_start";

	private const string Sound_BracketSquareExpand = "HUD_Imperative_bracket_open_first";

	private const string Sound_BracketExpandsForCalculatingLandingZone = "HUD_Imperative_bracket_open_second";

	private const string Sound_CalculatingLandingZoneTextAppears = "HUD_Imperative_calculating_beep";

	private const string Sound_TypeHeader = "HUD_Imperative_Text_typing_header";

	private const string Sound_TypeBody = "HUD_Imperative_Text_typing_body";

	public Vector2 initialSize = new Vector2(100f, 100f);

	public Vector2 zoomedOutSize = new Vector2(180f, 180f);

	public Vector2 calculatingImpactSize = new Vector2(500f, 120f);

	[Space]
	public LocText label;

	public LocText sidePanelTitleLabel;

	public LocText sidePanelDescriptionLabel;

	public LocText calculateImpactLabel;

	public Image bg;

	public Image border;

	public Image sidePanelIcon;

	private new RectTransform transform;

	private LargeImpactorStatus.Instance largeImpactorStatus;

	private LoopingSounds loopingSounds;

	private bool isVisible;

	private Color bgOriginalColor;

	private Color calculatingImpactLabelOriginalColor;

	private System.Action onPhase1Completed;

	private System.Action onComplete;

	private Coroutine coroutine;

	protected override void OnPrefabInit()
	{
		transform = base.transform as RectTransform;
		bgOriginalColor = bg.color;
		calculatingImpactLabelOriginalColor = calculateImpactLabel.color;
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		ResetGraphics();
	}

	public void Run(System.Action onPhase1Completed = null, System.Action onComplete = null)
	{
		SetVisibility(visible: true);
		AbortCoroutine();
		ResetGraphics();
		this.onPhase1Completed = onPhase1Completed;
		this.onComplete = onComplete;
		InitializeAndRunCoroutine();
	}

	public void Hide()
	{
		AbortCoroutine();
		ResetGraphics();
		SetVisibility(visible: false);
	}

	private void SetVisibility(bool visible)
	{
		isVisible = visible;
		label.enabled = visible;
		bg.enabled = visible;
		border.enabled = visible;
	}

	private void InitializeAndRunCoroutine()
	{
		coroutine = StartCoroutine(EnterSequence());
	}

	private void AbortCoroutine()
	{
		StopAllCoroutines();
		coroutine = null;
	}

	public void SetTarget(LargeImpactorStatus.Instance largeImpactorStatus)
	{
		this.largeImpactorStatus = largeImpactorStatus;
		loopingSounds = largeImpactorStatus.GetComponent<LoopingSounds>();
	}

	private void ResetGraphics()
	{
		label.SetText("");
		border.Opacity(0f);
		bg.color = bgOriginalColor;
		bg.Opacity(0f);
		sidePanelIcon.Opacity(0f);
		sidePanelTitleLabel.SetText("");
		calculateImpactLabel.SetText("");
		sidePanelDescriptionLabel.SetText("");
		calculateImpactLabel.color = calculatingImpactLabelOriginalColor;
	}

	private void PlayLoopingSound(string soundName)
	{
		string sound = GlobalAssets.GetSound(soundName);
		loopingSounds.StartSound(sound, pause_on_game_pause: false, enable_culling: false, enable_camera_scaled_position: false);
	}

	private IEnumerator EnterSequence()
	{
		KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Imperative_analysis_start"));
		yield return this.Interpolate(delegate(float n)
		{
			transform.sizeDelta = Vector2.Lerp(initialSize * 2f, initialSize, n);
			border.Opacity(n);
		}, 0.4f);
		if (bg.color != border.color)
		{
			bg.color = border.color;
		}
		yield return this.Interpolate(delegate(float n)
		{
			bg.Opacity(Mathf.Abs(Mathf.Sin(n * MathF.PI * 3f)));
		}, 0.4f, delegate
		{
			bg.color = bgOriginalColor;
		});
		KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Imperative_bracket_open_first"));
		yield return this.Interpolate(delegate(float n)
		{
			bg.Opacity(n * bgOriginalColor.a);
			transform.sizeDelta = Vector2.Lerp(initialSize, zoomedOutSize, n);
		}, 0.8f);
		PlayLoopingSound("HUD_Imperative_Text_typing_header");
		string titleText = MISC.NOTIFICATIONS.LARGEIMPACTORREVEALSEQUENCE.RETICLE.LARGE_IMPACTOR_NAME;
		yield return this.Interpolate(delegate(float n)
		{
			SequenceTools.TextWriter(label, titleText, n);
		}, 1f);
		loopingSounds.StopSound(GlobalAssets.GetSound("HUD_Imperative_Text_typing_header"));
		yield return null;
		PlayLoopingSound("HUD_Imperative_Text_typing_header");
		sidePanelIcon.color = Color.white;
		string sidePanelTitleText = MISC.NOTIFICATIONS.LARGEIMPACTORREVEALSEQUENCE.RETICLE.SIDE_PANEL_TITLE;
		yield return this.Interpolate(delegate(float n)
		{
			sidePanelIcon.Opacity(n);
			sidePanelIcon.transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(90f, 0f, n), 0f);
			SequenceTools.TextWriter(sidePanelTitleLabel, sidePanelTitleText, n);
		}, 0.5f);
		loopingSounds.StopSound(GlobalAssets.GetSound("HUD_Imperative_Text_typing_header"));
		PlayLoopingSound("HUD_Imperative_Text_typing_body");
		string sidePanelDescriptionText = GameUtil.SafeStringFormat(MISC.NOTIFICATIONS.LARGEIMPACTORREVEALSEQUENCE.RETICLE.SIDE_PANEL_DESCRIPTION, GameUtil.GetFormattedCycles(largeImpactorStatus.TimeRemainingBeforeCollision).Split(' ')[0]);
		yield return this.Interpolate(delegate(float n)
		{
			SequenceTools.TextWriter(sidePanelDescriptionLabel, sidePanelDescriptionText, n);
		}, 1.5f);
		loopingSounds.StopSound(GlobalAssets.GetSound("HUD_Imperative_Text_typing_body"));
		yield return new WaitForSecondsRealtime(2f);
		if (onPhase1Completed != null)
		{
			onPhase1Completed();
			onPhase1Completed = null;
		}
		yield return this.Interpolate(delegate(float n)
		{
			SequenceTools.TextEraser(label, titleText, n);
			SequenceTools.TextEraser(sidePanelTitleLabel, sidePanelTitleText, n);
			SequenceTools.TextEraser(sidePanelDescriptionLabel, sidePanelDescriptionText, n);
			sidePanelIcon.color = Color.Lerp(Color.white, Color.red, n);
			sidePanelIcon.Opacity(1f - n);
		}, 0.5f);
		Color bgColor = bg.color;
		Color targetBgColor = Color.Lerp(bgOriginalColor, Color.black, 0.5f);
		targetBgColor.a = bgOriginalColor.a * 0.8f;
		KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Imperative_bracket_open_second"));
		yield return this.Interpolate(delegate(float n)
		{
			bg.color = Color.Lerp(bgColor, targetBgColor, n);
			transform.sizeDelta = Vector2.Lerp(zoomedOutSize, calculatingImpactSize, n);
		}, 0.8f);
		KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Imperative_calculating_beep"));
		Coroutine flashLabelCoroutine = null;
		this.Interpolate(delegate(float n)
		{
			calculateImpactLabel.color = Color.Lerp(Color.white, Color.red, Mathf.Abs(Mathf.Sin(n * MathF.PI * 999f)));
		}, 999f, out flashLabelCoroutine);
		yield return this.Interpolate(delegate(float n)
		{
			SequenceTools.TextWriter(calculateImpactLabel, MISC.NOTIFICATIONS.LARGEIMPACTORREVEALSEQUENCE.RETICLE.CALCULATING_IMPACT_ZONE_TEXT, n);
		}, 0.5f);
		yield return new WaitForSecondsRealtime(3.5f);
		StopCoroutine(flashLabelCoroutine);
		Color impactLabelColor = calculateImpactLabel.color;
		yield return this.Interpolate(delegate(float n)
		{
			float f = 1f - n;
			float num = Mathf.Sqrt(f);
			float t = Mathf.Sqrt(n);
			bg.Opacity(bgOriginalColor.a * num);
			border.Opacity(num);
			calculateImpactLabel.Opacity(impactLabelColor.a * num);
			transform.sizeDelta = Vector2.Lerp(calculatingImpactSize, calculatingImpactSize * 1.3f, t);
		}, 0.8f);
		if (onComplete != null)
		{
			onComplete();
			onComplete = null;
		}
	}

	protected override void OnCmpDisable()
	{
		AbortCoroutine();
	}

	protected override void OnCmpEnable()
	{
		if (isVisible)
		{
			AbortCoroutine();
			ResetGraphics();
			InitializeAndRunCoroutine();
		}
	}
}
