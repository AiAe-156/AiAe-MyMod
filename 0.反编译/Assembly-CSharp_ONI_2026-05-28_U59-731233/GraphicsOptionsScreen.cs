using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

internal class GraphicsOptionsScreen : KModalScreen
{
	private struct Settings
	{
		public FullScreenMode fullscreen;

		public Resolution resolution;

		public int lowRes;

		public int colorSetId;
	}

	[SerializeField]
	private Dropdown resolutionDropdown;

	[SerializeField]
	private MultiToggle lowResToggle;

	[SerializeField]
	private MultiToggle fullscreenToggle;

	[SerializeField]
	private KButton applyButton;

	[SerializeField]
	private KButton doneButton;

	[SerializeField]
	private KButton closeButton;

	[SerializeField]
	private ConfirmDialogScreen confirmPrefab;

	[SerializeField]
	private ConfirmDialogScreen feedbackPrefab;

	[SerializeField]
	private KSlider uiScaleSlider;

	[SerializeField]
	private LocText sliderLabel;

	[SerializeField]
	private LocText title;

	[SerializeField]
	private Dropdown colorModeDropdown;

	[SerializeField]
	private KImage colorExampleLogicOn;

	[SerializeField]
	private KImage colorExampleLogicOff;

	[SerializeField]
	private KImage colorExampleCropHalted;

	[SerializeField]
	private KImage colorExampleCropGrowing;

	[SerializeField]
	private KImage colorExampleCropGrown;

	public static readonly string ResolutionWidthKey = "ResolutionWidth";

	public static readonly string ResolutionHeightKey = "ResolutionHeight";

	public static readonly string RefreshRateKeyNumerator = "RefreshRateNumerator";

	public static readonly string RefreshRateKeyDenominator = "RefreshRateDenominator";

	public static readonly string FullScreenKey = "FullScreen";

	public static readonly string LowResKey = "LowResTextures";

	public static readonly string ColorModeKey = "ColorModeID";

	private const FullScreenMode FULLSCREEN = FullScreenMode.FullScreenWindow;

	private const FullScreenMode WINDOWED = FullScreenMode.Windowed;

	private const FullScreenMode MAXIMIZED_WINDOWED = FullScreenMode.MaximizedWindow;

	private KCanvasScaler[] CanvasScalers;

	private ConfirmDialogScreen confirmDialog;

	private ConfirmDialogScreen feedbackDialog;

	private List<Resolution> resolutions = new List<Resolution>();

	private List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

	private List<Dropdown.OptionData> colorModeOptions = new List<Dropdown.OptionData>();

	private int colorModeId;

	private bool colorModeChanged = false;

	private Settings originalSettings;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		title.SetText(UI.FRONTEND.GRAPHICS_OPTIONS_SCREEN.TITLE);
		originalSettings = CaptureSettings();
		applyButton.isInteractable = false;
		applyButton.onClick += OnApply;
		applyButton.GetComponentInChildren<LocText>().SetText(UI.FRONTEND.GRAPHICS_OPTIONS_SCREEN.APPLYBUTTON);
		doneButton.onClick += OnDone;
		closeButton.onClick += OnDone;
		doneButton.GetComponentInChildren<LocText>().SetText(UI.FRONTEND.GRAPHICS_OPTIONS_SCREEN.DONE_BUTTON);
		bool flag = QualitySettings.GetQualityLevel() == 1;
		lowResToggle.ChangeState(flag ? 1 : 0);
		MultiToggle multiToggle = lowResToggle;
		multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, new System.Action(OnLowResToggle));
		lowResToggle.GetComponentInChildren<LocText>().SetText(UI.FRONTEND.GRAPHICS_OPTIONS_SCREEN.LOWRES);
		resolutionDropdown.ClearOptions();
		BuildOptions();
		ScreenResize instance = ScreenResize.Instance;
		instance.OnResize = (System.Action)Delegate.Combine(instance.OnResize, (System.Action)delegate
		{
			BuildOptions();
			resolutionDropdown.options = options;
		});
		resolutionDropdown.options = options;
		resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
		fullscreenToggle.ChangeState(Screen.fullScreen ? 1 : 0);
		MultiToggle multiToggle2 = fullscreenToggle;
		multiToggle2.onClick = (System.Action)Delegate.Combine(multiToggle2.onClick, new System.Action(OnFullscreenToggle));
		fullscreenToggle.GetComponentInChildren<LocText>().SetText(UI.FRONTEND.GRAPHICS_OPTIONS_SCREEN.FULLSCREEN);
		resolutionDropdown.transform.parent.GetComponentInChildren<LocText>().SetText(UI.FRONTEND.GRAPHICS_OPTIONS_SCREEN.RESOLUTION);
		if (fullscreenToggle.CurrentState == 1)
		{
			int resolutionIndex = GetResolutionIndex(originalSettings.resolution);
			if (resolutionIndex != -1)
			{
				resolutionDropdown.value = resolutionIndex;
			}
		}
		CanvasScalers = UnityEngine.Object.FindObjectsByType<KCanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
		UpdateSliderLabel();
		uiScaleSlider.onValueChanged.AddListener(delegate
		{
			sliderLabel.text = uiScaleSlider.value + "%";
		});
		uiScaleSlider.onReleaseHandle += delegate
		{
			UpdateUIScale(uiScaleSlider.value);
		};
		BuildColorModeOptions();
		colorModeDropdown.options = colorModeOptions;
		colorModeDropdown.onValueChanged.AddListener(OnColorModeChanged);
		int value = 0;
		if (KPlayerPrefs.HasKey(ColorModeKey))
		{
			value = KPlayerPrefs.GetInt(ColorModeKey);
		}
		colorModeDropdown.value = value;
		RefreshColorExamples(originalSettings.colorSetId);
	}

	public static void SetSettingsFromPrefs()
	{
		SetResolutionFromPrefs();
		SetLowResFromPrefs();
	}

	public static void SetLowResFromPrefs()
	{
		int num = 0;
		if (KPlayerPrefs.HasKey(LowResKey))
		{
			num = KPlayerPrefs.GetInt(LowResKey);
			QualitySettings.SetQualityLevel(num, applyExpensiveChanges: true);
		}
		else
		{
			QualitySettings.SetQualityLevel(num, applyExpensiveChanges: true);
		}
		DebugUtil.LogArgs(string.Format("Low Res Textures? {0}", (num == 1) ? "Yes" : "No"));
	}

	public static void SetResolutionFromPrefs()
	{
		int num = Screen.currentResolution.width;
		int num2 = Screen.currentResolution.height;
		RefreshRate refreshRate = Screen.currentResolution.refreshRateRatio;
		FullScreenMode fullScreenMode = Screen.fullScreenMode;
		if (KPlayerPrefs.HasKey(ResolutionWidthKey) && KPlayerPrefs.HasKey(ResolutionHeightKey))
		{
			int num3 = KPlayerPrefs.GetInt(ResolutionWidthKey);
			int num4 = KPlayerPrefs.GetInt(ResolutionHeightKey);
			uint numerator = (uint)KPlayerPrefs.GetInt(RefreshRateKeyNumerator, (int)Screen.currentResolution.refreshRateRatio.numerator);
			uint denominator = (uint)KPlayerPrefs.GetInt(RefreshRateKeyDenominator, (int)Screen.currentResolution.refreshRateRatio.denominator);
			FullScreenMode fullScreenMode2 = ((KPlayerPrefs.GetInt(FullScreenKey, Screen.fullScreen ? 1 : 0) == 1) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
			if (num4 <= 1 || num3 <= 1)
			{
				DebugUtil.LogArgs("Saved resolution was invalid, ignoring...");
			}
			else
			{
				num = num3;
				num2 = num4;
				refreshRate.numerator = numerator;
				refreshRate.denominator = denominator;
				fullScreenMode = fullScreenMode2;
			}
		}
		if (num <= 1 || num2 <= 1)
		{
			DebugUtil.LogWarningArgs("Detected a degenerate resolution, attempting to fix...");
			Resolution[] array = Screen.resolutions;
			for (int i = 0; i < array.Length; i++)
			{
				Resolution resolution = array[i];
				if (resolution.width == 1920)
				{
					num = resolution.width;
					num2 = resolution.height;
					refreshRate = default(RefreshRate);
				}
			}
			if (num <= 1 || num2 <= 1)
			{
				Resolution[] array2 = Screen.resolutions;
				for (int j = 0; j < array2.Length; j++)
				{
					Resolution resolution2 = array2[j];
					if (resolution2.width == 1280)
					{
						num = resolution2.width;
						num2 = resolution2.height;
						refreshRate = default(RefreshRate);
					}
				}
			}
			if (num <= 1 || num2 <= 1)
			{
				Resolution[] array3 = Screen.resolutions;
				for (int k = 0; k < array3.Length; k++)
				{
					Resolution resolution3 = array3[k];
					if (resolution3.width > 1 && resolution3.height > 1 && resolution3.refreshRateRatio.value > 0.0)
					{
						num = resolution3.width;
						num2 = resolution3.height;
						refreshRate = default(RefreshRate);
					}
				}
			}
			if (num <= 1 || num2 <= 1)
			{
				string text = "Could not find a suitable resolution for this screen! Reported available resolutions are:";
				Resolution[] array4 = Screen.resolutions;
				for (int l = 0; l < array4.Length; l++)
				{
					Resolution resolution4 = array4[l];
					text += $"\n{resolution4.width}x{resolution4.height} @ {resolution4.refreshRateRatio.value}hz";
				}
				Debug.LogError(text);
				num = 1280;
				num2 = 720;
				fullScreenMode = FullScreenMode.Windowed;
				refreshRate = default(RefreshRate);
			}
		}
		DebugUtil.LogArgs($"Applying resolution {num}x{num2} @{refreshRate}hz (fullscreen: {fullScreenMode})");
		Screen.SetResolution(num, num2, fullScreenMode, refreshRate);
	}

	public static void SetColorModeFromPrefs()
	{
		int num = 0;
		if (KPlayerPrefs.HasKey(ColorModeKey))
		{
			num = KPlayerPrefs.GetInt(ColorModeKey);
		}
		GlobalAssets.Instance.colorSet = GlobalAssets.Instance.colorSetOptions[num];
	}

	public static void OnResize()
	{
		Settings settings = default(Settings);
		settings.resolution = Screen.currentResolution;
		settings.resolution.width = Screen.width;
		settings.resolution.height = Screen.height;
		settings.fullscreen = Screen.fullScreenMode;
		settings.lowRes = QualitySettings.GetQualityLevel();
		settings.colorSetId = Array.IndexOf(GlobalAssets.Instance.colorSetOptions, GlobalAssets.Instance.colorSet);
		SaveSettingsToPrefs(settings);
	}

	private static void SaveSettingsToPrefs(Settings settings)
	{
		KPlayerPrefs.SetInt(LowResKey, settings.lowRes);
		Debug.LogFormat("Screen resolution updated, saving values to prefs: {0}x{1} @ {2}, fullscreen: {3}", settings.resolution.width, settings.resolution.height, settings.resolution.refreshRateRatio, settings.fullscreen);
		KPlayerPrefs.SetInt(ResolutionWidthKey, settings.resolution.width);
		KPlayerPrefs.SetInt(ResolutionHeightKey, settings.resolution.height);
		KPlayerPrefs.SetInt(RefreshRateKeyNumerator, (int)settings.resolution.refreshRateRatio.numerator);
		KPlayerPrefs.SetInt(RefreshRateKeyDenominator, (int)settings.resolution.refreshRateRatio.denominator);
		KPlayerPrefs.SetInt(FullScreenKey, (settings.fullscreen != FullScreenMode.Windowed && settings.fullscreen != FullScreenMode.MaximizedWindow) ? 1 : 0);
		KPlayerPrefs.SetInt(ColorModeKey, settings.colorSetId);
	}

	private void UpdateUIScale(float value)
	{
		CanvasScalers = UnityEngine.Object.FindObjectsByType<KCanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
		KCanvasScaler[] canvasScalers = CanvasScalers;
		foreach (KCanvasScaler kCanvasScaler in canvasScalers)
		{
			float userScale = value / 100f;
			kCanvasScaler.SetUserScale(userScale);
			KPlayerPrefs.SetFloat(KCanvasScaler.UIScalePrefKey, value);
		}
		ScreenResize.Instance.TriggerResize();
		UpdateSliderLabel();
	}

	private void UpdateSliderLabel()
	{
		if (CanvasScalers != null && CanvasScalers.Length != 0 && CanvasScalers[0] != null)
		{
			uiScaleSlider.value = CanvasScalers[0].GetUserScale() * 100f;
			sliderLabel.text = uiScaleSlider.value + "%";
		}
	}

	public override void OnKeyDown(KButtonEvent e)
	{
		if (e.TryConsume(Action.Escape) || e.TryConsume(Action.MouseRight))
		{
			resolutionDropdown.Hide();
			Deactivate();
		}
		else
		{
			base.OnKeyDown(e);
		}
	}

	private void BuildOptions()
	{
		options.Clear();
		resolutions.Clear();
		Resolution resolution = new Resolution
		{
			width = Screen.width,
			height = Screen.height,
			refreshRateRatio = Screen.currentResolution.refreshRateRatio
		};
		options.Add(new Dropdown.OptionData(ResolutionDisplayString(resolution)));
		resolutions.Add(resolution);
		Resolution[] array = Screen.resolutions;
		for (int i = 0; i < array.Length; i++)
		{
			Resolution resolution2 = array[i];
			if (resolution2.height >= 720)
			{
				options.Add(new Dropdown.OptionData(ResolutionDisplayString(resolution2)));
				resolutions.Add(resolution2);
			}
		}
	}

	private string ResolutionDisplayString(Resolution resolution)
	{
		return $"{resolution.width} x {resolution.height} @ {Mathf.Floor((float)resolution.refreshRateRatio.value)}Hz";
	}

	private void BuildColorModeOptions()
	{
		colorModeOptions.Clear();
		for (int i = 0; i < GlobalAssets.Instance.colorSetOptions.Length; i++)
		{
			colorModeOptions.Add(new Dropdown.OptionData(Strings.Get(GlobalAssets.Instance.colorSetOptions[i].settingName)));
		}
	}

	private void RefreshColorExamples(int idx)
	{
		Color32 logicOn = GlobalAssets.Instance.colorSetOptions[idx].logicOn;
		Color32 logicOff = GlobalAssets.Instance.colorSetOptions[idx].logicOff;
		Color32 cropHalted = GlobalAssets.Instance.colorSetOptions[idx].cropHalted;
		Color32 cropGrowing = GlobalAssets.Instance.colorSetOptions[idx].cropGrowing;
		Color32 cropGrown = GlobalAssets.Instance.colorSetOptions[idx].cropGrown;
		logicOn.a = byte.MaxValue;
		logicOff.a = byte.MaxValue;
		cropHalted.a = byte.MaxValue;
		cropGrowing.a = byte.MaxValue;
		cropGrown.a = byte.MaxValue;
		colorExampleLogicOn.color = logicOn;
		colorExampleLogicOff.color = logicOff;
		colorExampleCropHalted.color = cropHalted;
		colorExampleCropGrowing.color = cropGrowing;
		colorExampleCropGrown.color = cropGrown;
	}

	private int GetResolutionIndex(Resolution resolution)
	{
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < resolutions.Count; i++)
		{
			Resolution resolution2 = resolutions[i];
			if (resolution2.width == resolution.width && resolution2.height == resolution.height && resolution2.refreshRateRatio.value == 0.0)
			{
				num2 = i;
			}
			if (resolution2.width == resolution.width && resolution2.height == resolution.height && Math.Abs(resolution2.refreshRateRatio.value - resolution.refreshRateRatio.value) <= 1.0)
			{
				num = i;
				break;
			}
		}
		return (num == -1) ? num2 : num;
	}

	private Settings CaptureSettings()
	{
		return new Settings
		{
			fullscreen = Screen.fullScreenMode,
			resolution = new Resolution
			{
				width = Screen.width,
				height = Screen.height,
				refreshRateRatio = Screen.currentResolution.refreshRateRatio
			},
			lowRes = QualitySettings.GetQualityLevel(),
			colorSetId = Array.IndexOf(GlobalAssets.Instance.colorSetOptions, GlobalAssets.Instance.colorSet)
		};
	}

	private void OnApply()
	{
		try
		{
			Settings new_settings = default(Settings);
			new_settings.resolution = resolutions[resolutionDropdown.value];
			new_settings.fullscreen = ((fullscreenToggle.CurrentState != 0) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
			new_settings.lowRes = lowResToggle.CurrentState;
			new_settings.colorSetId = colorModeId;
			if (GlobalAssets.Instance.colorSetOptions[colorModeId] != GlobalAssets.Instance.colorSet)
			{
				colorModeChanged = true;
			}
			ApplyConfirmSettings(new_settings, delegate
			{
				applyButton.isInteractable = false;
				if (colorModeChanged)
				{
					feedbackDialog = Util.KInstantiateUI(confirmPrefab.gameObject, base.transform.parent.gameObject).GetComponent<ConfirmDialogScreen>();
					feedbackDialog.PopupConfirmDialog(UI.FRONTEND.GRAPHICS_OPTIONS_SCREEN.COLORBLIND_FEEDBACK.text, null, null, UI.FRONTEND.GRAPHICS_OPTIONS_SCREEN.COLORBLIND_FEEDBACK_BUTTON.text, delegate
					{
						App.OpenWebURL("https://forums.kleientertainment.com/forums/topic/117325-color-blindness-feedback/");
					});
					feedbackDialog.gameObject.SetActive(value: true);
				}
				colorModeChanged = false;
				SaveSettingsToPrefs(new_settings);
			});
		}
		catch (Exception ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Failed to apply graphics options!\nResolutions:");
			foreach (Resolution resolution in resolutions)
			{
				stringBuilder.Append("\t" + resolution.ToString() + "\n");
			}
			stringBuilder.Append("Selected Resolution Idx: " + resolutionDropdown.value);
			stringBuilder.Append("FullScreen: " + fullscreenToggle.CurrentState);
			Debug.LogError(stringBuilder.ToString());
			throw ex;
		}
	}

	public void OnDone()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void RefreshApplyButton()
	{
		Settings settings = CaptureSettings();
		if (settings.fullscreen != FullScreenMode.Windowed && fullscreenToggle.CurrentState == 0)
		{
			applyButton.isInteractable = true;
			return;
		}
		if (settings.fullscreen == FullScreenMode.Windowed && fullscreenToggle.CurrentState == 1)
		{
			applyButton.isInteractable = true;
			return;
		}
		if (settings.lowRes != lowResToggle.CurrentState)
		{
			applyButton.isInteractable = true;
			return;
		}
		if (settings.colorSetId != colorModeId)
		{
			applyButton.isInteractable = true;
			return;
		}
		int resolutionIndex = GetResolutionIndex(settings.resolution);
		applyButton.isInteractable = resolutionDropdown.value != resolutionIndex;
	}

	private void OnFullscreenToggle()
	{
		fullscreenToggle.ChangeState((fullscreenToggle.CurrentState == 0) ? 1 : 0);
		RefreshApplyButton();
	}

	private void OnResolutionChanged(int idx)
	{
		RefreshApplyButton();
	}

	private void OnColorModeChanged(int idx)
	{
		colorModeId = idx;
		RefreshApplyButton();
		RefreshColorExamples(colorModeId);
	}

	private void OnLowResToggle()
	{
		lowResToggle.ChangeState((lowResToggle.CurrentState == 0) ? 1 : 0);
		RefreshApplyButton();
	}

	private void ApplyConfirmSettings(Settings new_settings, System.Action on_confirm)
	{
		Settings current_settings = CaptureSettings();
		ApplySettings(new_settings);
		confirmDialog = Util.KInstantiateUI(confirmPrefab.gameObject, base.transform.parent.gameObject).GetComponent<ConfirmDialogScreen>();
		System.Action action = delegate
		{
			ApplySettings(current_settings);
		};
		Coroutine timer = StartCoroutine(Timer(15f, action));
		confirmDialog.onDeactivateCB = delegate
		{
			if (timer != null)
			{
				StopCoroutine(timer);
			}
		};
		confirmDialog.PopupConfirmDialog(colorModeChanged ? UI.FRONTEND.GRAPHICS_OPTIONS_SCREEN.ACCEPT_CHANGES_STRING_COLOR.text : UI.FRONTEND.GRAPHICS_OPTIONS_SCREEN.ACCEPT_CHANGES.text, on_confirm, action);
		confirmDialog.gameObject.SetActive(value: true);
	}

	private void ApplySettings(Settings new_settings)
	{
		Resolution resolution = new_settings.resolution;
		Screen.SetResolution(resolution.width, resolution.height, new_settings.fullscreen, resolution.refreshRateRatio);
		Screen.fullScreenMode = new_settings.fullscreen;
		int resolutionIndex = GetResolutionIndex(new_settings.resolution);
		if (resolutionIndex != -1)
		{
			resolutionDropdown.value = resolutionIndex;
		}
		GlobalAssets.Instance.colorSet = GlobalAssets.Instance.colorSetOptions[new_settings.colorSetId];
		Debug.Log("Applying low res settings " + new_settings.lowRes + " / existing is " + QualitySettings.GetQualityLevel());
		if (QualitySettings.GetQualityLevel() != new_settings.lowRes)
		{
			QualitySettings.SetQualityLevel(new_settings.lowRes, applyExpensiveChanges: true);
		}
	}

	private IEnumerator Timer(float time, System.Action revert)
	{
		yield return new WaitForSecondsRealtime(time);
		if (confirmDialog != null)
		{
			confirmDialog.Deactivate();
			revert();
		}
	}

	private void Update()
	{
		Debug.developerConsoleVisible = false;
	}
}
