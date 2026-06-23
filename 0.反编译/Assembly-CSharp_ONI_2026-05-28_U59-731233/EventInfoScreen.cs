using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EventInfoScreen : KModalScreen
{
	[SerializeField]
	private float baseCharacterScale = 0.0057f;

	[FormerlySerializedAs("midgroundPrefab")]
	[FormerlySerializedAs("mid")]
	[Header("Prefabs")]
	[SerializeField]
	private GameObject animPrefab;

	[SerializeField]
	private GameObject optionPrefab;

	[SerializeField]
	private GameObject optionIconPrefab;

	[SerializeField]
	private GameObject optionTextPrefab;

	[Header("Groups")]
	[SerializeField]
	private Transform artSection;

	[SerializeField]
	private Transform midgroundGroup;

	[SerializeField]
	private GameObject timeGroup;

	[SerializeField]
	private GameObject buttonsGroup;

	[SerializeField]
	private GameObject chainGroup;

	[Header("Text")]
	[SerializeField]
	private LocText eventHeader;

	[SerializeField]
	private LocText eventTimeLabel;

	[SerializeField]
	private LocText eventLocationLabel;

	[SerializeField]
	private LocText eventDescriptionLabel;

	[SerializeField]
	private bool loadMinionFromPersonalities = true;

	[SerializeField]
	private LocText chainCount;

	[Header("Button Colour Styles")]
	[SerializeField]
	private ColorStyleSetting neutralButtonSetting;

	[SerializeField]
	private ColorStyleSetting badButtonSetting;

	[SerializeField]
	private ColorStyleSetting goodButtonSetting;

	private List<KBatchedAnimController> createdAnimations = new List<KBatchedAnimController>();

	public override bool IsModal()
	{
		return true;
	}

	public void SetEventData(EventInfoData data)
	{
		data.FinalizeText();
		eventHeader.text = data.title;
		eventDescriptionLabel.text = data.description;
		eventLocationLabel.text = data.location;
		eventTimeLabel.text = data.whenDescription;
		if (data.location.IsNullOrWhiteSpace() && data.location.IsNullOrWhiteSpace())
		{
			timeGroup.gameObject.SetActive(value: false);
		}
		if (data.options.Count == 0)
		{
			data.AddDefaultOption();
		}
		artSection.gameObject.SetActive(data.animFileName != HashedString.Invalid);
		SetEventDataOptions(data);
		SetEventDataVisuals(data);
	}

	private void SetEventDataOptions(EventInfoData data)
	{
		foreach (EventInfoData.Option option in data.options)
		{
			GameObject gameObject = Util.KInstantiateUI(optionPrefab, buttonsGroup);
			gameObject.name = "Option: " + option.mainText;
			KButton component = gameObject.GetComponent<KButton>();
			component.isInteractable = option.allowed;
			component.onClick += delegate
			{
				if (option.callback != null)
				{
					option.callback();
				}
				Deactivate();
			};
			if (!option.tooltip.IsNullOrWhiteSpace())
			{
				gameObject.GetComponent<ToolTip>().SetSimpleTooltip(option.tooltip);
			}
			else
			{
				gameObject.GetComponent<ToolTip>().enabled = false;
			}
			foreach (EventInfoData.OptionIcon informationIcon in option.informationIcons)
			{
				CreateOptionIcon(gameObject, informationIcon);
			}
			GameObject gameObject2 = Util.KInstantiateUI(optionTextPrefab, gameObject);
			gameObject2.GetComponent<LocText>().text = ((option.description == null) ? ("<b>" + option.mainText + "</b>") : ("<b>" + option.mainText + "</b>\n<i>(" + option.description + ")</i>"));
			foreach (EventInfoData.OptionIcon consequenceIcon in option.consequenceIcons)
			{
				CreateOptionIcon(gameObject, consequenceIcon);
			}
			gameObject.SetActive(value: true);
		}
	}

	public override void Deactivate()
	{
		AudioMixer.instance.Stop(AudioMixerSnapshots.Get().EventPopupSnapshot);
		base.Deactivate();
	}

	private void CreateOptionIcon(GameObject option, EventInfoData.OptionIcon optionIcon)
	{
		GameObject gameObject = Util.KInstantiateUI(optionIconPrefab, option);
		gameObject.GetComponent<ToolTip>().SetSimpleTooltip(optionIcon.tooltip);
		HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
		Image reference = component.GetReference<Image>("Mask");
		Image reference2 = component.GetReference<Image>("Border");
		Image reference3 = component.GetReference<Image>("Icon");
		if (optionIcon.sprite != null)
		{
			reference3.transform.localScale *= optionIcon.scale;
		}
		Color32 color = Color.white;
		switch (optionIcon.containerType)
		{
		case EventInfoData.OptionIcon.ContainerType.Neutral:
			reference.sprite = Assets.GetSprite("container_fill_neutral");
			reference2.sprite = Assets.GetSprite("container_border_neutral");
			if (optionIcon.sprite == null)
			{
				optionIcon.sprite = Assets.GetSprite("knob");
			}
			color = GlobalAssets.Instance.colorSet.eventNeutral;
			break;
		case EventInfoData.OptionIcon.ContainerType.Positive:
			reference.sprite = Assets.GetSprite("container_fill_positive");
			reference2.sprite = Assets.GetSprite("container_border_positive");
			reference3.rectTransform.localPosition += Vector3.down * 1f;
			if (optionIcon.sprite == null)
			{
				optionIcon.sprite = Assets.GetSprite("icon_positive");
			}
			color = GlobalAssets.Instance.colorSet.eventPositive;
			break;
		case EventInfoData.OptionIcon.ContainerType.Negative:
			reference.sprite = Assets.GetSprite("container_fill_negative");
			reference2.sprite = Assets.GetSprite("container_border_negative");
			reference3.rectTransform.localPosition += Vector3.up * 1f;
			color = GlobalAssets.Instance.colorSet.eventNegative;
			if (optionIcon.sprite == null)
			{
				optionIcon.sprite = Assets.GetSprite("cancel");
			}
			break;
		case EventInfoData.OptionIcon.ContainerType.Information:
			reference.sprite = Assets.GetSprite("requirements");
			reference2.enabled = false;
			break;
		}
		reference.color = color;
		reference3.sprite = optionIcon.sprite;
		if (optionIcon.sprite == null)
		{
			reference3.gameObject.SetActive(value: false);
		}
	}

	private void SetEventDataVisuals(EventInfoData data)
	{
		createdAnimations.ForEach(delegate(KBatchedAnimController x)
		{
			Object.Destroy(x);
		});
		createdAnimations.Clear();
		KAnimFile anim = Assets.GetAnim(data.animFileName);
		if (anim == null)
		{
			Debug.LogWarning("Event " + data.title + " has no anim data");
			return;
		}
		Transform transform = CreateAnimLayer(midgroundGroup, anim, data.mainAnim).transform;
		KBatchedAnimController component = transform.GetComponent<KBatchedAnimController>();
		if (data.minions != null)
		{
			for (int num = 0; num < data.minions.Length; num++)
			{
				if (data.minions[num] == null)
				{
					DebugUtil.LogWarningArgs($"EventInfoScreen unable to display minion {num}");
				}
				string text = $"dupe{num + 1:D2}";
				if (component.HasAnimation(text))
				{
					CreateAnimLayer(midgroundGroup, anim, text, data.minions[num]);
				}
			}
		}
		if (data.artifact != null)
		{
			string text2 = "artifact";
			if (component.HasAnimation(text2))
			{
				CreateAnimLayer(midgroundGroup, anim, text2, null, data.artifact);
			}
		}
	}

	private GameObject CreateAnimLayer(Transform parent, KAnimFile animFile, HashedString animName, GameObject minion = null, GameObject artifact = null, string targetSymbol = null)
	{
		GameObject gameObject = Object.Instantiate(animPrefab, parent);
		KBatchedAnimController component = gameObject.GetComponent<KBatchedAnimController>();
		createdAnimations.Add(component);
		if (minion != null)
		{
			component.AnimFiles = new KAnimFile[4]
			{
				Assets.GetAnim("body_comp_default_kanim"),
				Assets.GetAnim("head_swap_kanim"),
				Assets.GetAnim("body_swap_kanim"),
				animFile
			};
		}
		else
		{
			component.AnimFiles = new KAnimFile[1] { animFile };
		}
		gameObject.SetActive(value: true);
		if (minion != null)
		{
			if (loadMinionFromPersonalities)
			{
				UIDupeSymbolOverride component2 = component.GetComponent<UIDupeSymbolOverride>();
				component2.Apply(minion.GetComponent<MinionIdentity>());
			}
			else
			{
				SymbolOverrideController component3 = component.GetComponent<SymbolOverrideController>();
				SymbolOverrideController.SymbolEntry[] getSymbolOverrides = minion.GetComponent<SymbolOverrideController>().GetSymbolOverrides;
				for (int i = 0; i < getSymbolOverrides.Length; i++)
				{
					SymbolOverrideController.SymbolEntry symbolEntry = getSymbolOverrides[i];
					component3.AddSymbolOverride(symbolEntry.targetSymbol, symbolEntry.sourceSymbol, symbolEntry.priority);
				}
			}
			BaseMinionConfig.CopyVisibleSymbols(gameObject, minion);
		}
		if (artifact != null)
		{
			SymbolOverrideController component4 = component.GetComponent<SymbolOverrideController>();
			KBatchedAnimController component5 = artifact.GetComponent<KBatchedAnimController>();
			string initialAnim = component5.initialAnim;
			initialAnim = initialAnim.Replace("idle_", "artifact_");
			initialAnim = initialAnim.Replace("_loop", "");
			KAnim.Build.Symbol symbol = component5.AnimFiles[0].GetData().build.GetSymbol(initialAnim);
			if (symbol != null)
			{
				component4.AddSymbolOverride("snapTo_artifact", symbol);
			}
		}
		if (targetSymbol != null)
		{
			KBatchedAnimTracker kBatchedAnimTracker = gameObject.AddOrGet<KBatchedAnimTracker>();
			kBatchedAnimTracker.symbol = targetSymbol;
		}
		component.Play(animName, KAnim.PlayMode.Loop);
		component.animScale = baseCharacterScale;
		return gameObject;
	}

	public static EventInfoScreen ShowPopup(EventInfoData eventInfoData)
	{
		EventInfoScreen eventInfoScreen = (EventInfoScreen)KScreenManager.Instance.StartScreen(ScreenPrefabs.Instance.eventInfoScreen.gameObject, GameScreenManager.Instance.ssOverlayCanvas.gameObject);
		eventInfoScreen.SetEventData(eventInfoData);
		AudioMixer.instance.Start(AudioMixerSnapshots.Get().EventPopupSnapshot);
		KFMOD.PlayUISound(GlobalAssets.GetSound("StoryTrait_Activation_Popup_short"));
		if (eventInfoData.showCallback != null)
		{
			eventInfoData.showCallback();
		}
		if (eventInfoData.clickFocus != null)
		{
			WorldContainer myWorld = eventInfoData.clickFocus.gameObject.GetMyWorld();
			if (myWorld != null && myWorld.IsDiscovered)
			{
				GameUtil.FocusCameraOnWorld(myWorld.id, eventInfoData.clickFocus.position);
			}
		}
		return eventInfoScreen;
	}

	public static Notification CreateNotification(EventInfoData eventInfoData, Notification.ClickCallback clickCallback = null)
	{
		if (eventInfoData == null)
		{
			DebugUtil.LogWarningArgs("eventPopup is null in CreateStandardEventNotification");
			return null;
		}
		eventInfoData.FinalizeText();
		Notification notification = new Notification(eventInfoData.title, NotificationType.Event, null, null, expires: false, 0f, null, null, eventInfoData.clickFocus);
		if (clickCallback == null)
		{
			notification.customClickCallback = delegate
			{
				ShowPopup(eventInfoData);
			};
		}
		else
		{
			notification.customClickCallback = clickCallback;
		}
		return notification;
	}
}
