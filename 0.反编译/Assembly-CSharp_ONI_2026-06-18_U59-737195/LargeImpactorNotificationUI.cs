using System;
using UnityEngine;
using UnityEngine.UI;

public class LargeImpactorNotificationUI : KMonoBehaviour, ISim200ms
{
	public Image healthbar;

	public LargeImpactorNotificationUI_Clock clock;

	public KToggleSlider toggle;

	public LargeImpactorUINotificationHitEffects hitEffects;

	public LargeImpactorNotificationUI_CycleLabelEffects cyclesLabelEffects;

	public LocText numberOfCyclesLabel;

	private LargeImpactorStatus.Instance statusMonitor;

	private LargeImpactorVisualizer rangeVisualizer;

	private ParallaxBackgroundObject asteroidBackground;

	private int midSkyCell = Grid.InvalidCell;

	private const string Hit_SFX = "Notification_Imperative_hit";

	private const string Click_SFX = "HUD_Click_Open ";

	private const string Focus_SFX = "HUD_Demolior_Click_focus";

	private const string ToggleOff_SFX = "HUD_Demolior_LandingZone_toggle_off";

	private const string ToggleOn_SFX = "HUD_Demolior_LandingZone_toggle_on";

	protected override void OnSpawn()
	{
		GameplayEventInstance gameplayEventInstance = GameplayEventManager.Instance.GetGameplayEventInstance(Db.Get().GameplayEvents.LargeImpactor.Id);
		LargeImpactorEvent.StatesInstance statesInstance = (LargeImpactorEvent.StatesInstance)gameplayEventInstance.smi;
		rangeVisualizer = statesInstance.impactorInstance.GetComponent<LargeImpactorVisualizer>();
		asteroidBackground = statesInstance.impactorInstance.GetComponent<ParallaxBackgroundObject>();
		statusMonitor = statesInstance.impactorInstance.GetSMI<LargeImpactorStatus.Instance>();
		LargeImpactorStatus.Instance instance = statusMonitor;
		instance.OnDamaged = (Action<int>)Delegate.Combine(instance.OnDamaged, new Action<int>(OnAsteroidDamaged));
		Game.Instance.Subscribe(445618876, OnScreenResolutionChanged);
		Game.Instance.Subscribe(-810220474, OnScreenResolutionChanged);
		cyclesLabelEffects.InitializeCycleLabelFocusMonitor();
		toggle.onValueChanged.AddListener(ToggleVisibility);
		toggle.SetIsOnWithoutNotify(rangeVisualizer != null && rangeVisualizer.Visible);
		toggle.offEffectDuration = rangeVisualizer.FoldEffectDuration;
		LargeImpactorCrashStamp component = statesInstance.impactorInstance.GetComponent<LargeImpactorCrashStamp>();
		midSkyCell = Grid.FindMidSkyCellAlignedWithCellInWorld(Grid.XYToCell(component.stampLocation.x, component.stampLocation.y), gameplayEventInstance.worldId);
		RefreshTogglePositionInRangeVisualizer();
		RefreshValues();
	}

	private void OnScreenResolutionChanged(object data)
	{
		RefreshTogglePositionInRangeVisualizer();
	}

	private void RefreshTogglePositionInRangeVisualizer()
	{
		if (rangeVisualizer != null)
		{
			RectTransform rectTransform = toggle.rectTransform();
			Vector3 worldPoint = rectTransform.TransformPoint(rectTransform.rect.center);
			Vector2 vector = RectTransformUtility.WorldToScreenPoint(null, worldPoint);
			Vector2 screenSpaceNotificationTogglePosition = new Vector2(vector.x / (float)Screen.width, vector.y / (float)Screen.height);
			rangeVisualizer.ScreenSpaceNotificationTogglePosition = screenSpaceNotificationTogglePosition;
		}
	}

	public void Sim200ms(float dt)
	{
		RefreshValues();
	}

	public void RefreshValues()
	{
		float fillAmount = (float)statusMonitor.Health / (float)statusMonitor.def.MAX_HEALTH;
		float largeImpactorTime = statusMonitor.TimeRemainingBeforeCollision / LargeImpactorEvent.GetImpactTime();
		healthbar.fillAmount = fillAmount;
		clock.SetLargeImpactorTime(largeImpactorTime);
		string[] array = GameUtil.GetFormattedCycles(statusMonitor.TimeRemainingBeforeCollision).Split(' ');
		numberOfCyclesLabel.SetText(array[0]);
		if (rangeVisualizer != null && toggle.isOn != rangeVisualizer.Visible)
		{
			toggle.isOn = rangeVisualizer.Visible;
		}
	}

	private void OnAsteroidDamaged(int newHealth)
	{
		hitEffects.PlayHitEffect();
		KFMOD.PlayUISound(GlobalAssets.GetSound("Notification_Imperative_hit"));
		RefreshValues();
	}

	public void ToggleVisibility(bool shouldBeVisible)
	{
		if (rangeVisualizer != null)
		{
			KFMOD.PlayUISound(GlobalAssets.GetSound(shouldBeVisible ? "HUD_Demolior_LandingZone_toggle_on" : "HUD_Demolior_LandingZone_toggle_off"));
			RefreshTogglePositionInRangeVisualizer();
			rangeVisualizer.SetFoldedState(!shouldBeVisible);
		}
	}

	public void OnPlayerClickedNotification()
	{
		GameUtil.FocusCamera(midSkyCell);
		KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Click_Open "));
		KFMOD.PlayUISound(GlobalAssets.GetSound("HUD_Demolior_Click_focus"));
		if (asteroidBackground != null)
		{
			asteroidBackground.PlayPlayerClickFeedback();
		}
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		cyclesLabelEffects.AbortCycleLabelFocusMonitor();
		if (statusMonitor != null)
		{
			LargeImpactorStatus.Instance instance = statusMonitor;
			instance.OnDamaged = (Action<int>)Delegate.Remove(instance.OnDamaged, new Action<int>(OnAsteroidDamaged));
		}
		Game.Instance.Unsubscribe(445618876, OnScreenResolutionChanged);
		Game.Instance.Unsubscribe(-810220474, OnScreenResolutionChanged);
	}

	protected override void OnCmpEnable()
	{
		if (base.isSpawned)
		{
			cyclesLabelEffects.InitializeCycleLabelFocusMonitor();
		}
	}

	protected override void OnCmpDisable()
	{
		cyclesLabelEffects.AbortCycleLabelFocusMonitor();
	}
}
