using System.Collections.Generic;
using STRINGS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodexTemperatureTransitionPanel : CodexWidget<CodexTemperatureTransitionPanel>
{
	public enum TransitionType
	{
		HEAT,
		COOL,
		SUBLIMATE,
		OFFGASS
	}

	private Element sourceElement;

	private TransitionType transitionType;

	private static readonly Color SUBLIMATE_TEXT_COLOR = new Color(0.23137255f, 0.56078434f, 2f / 3f, 1f);

	private static readonly Color OFFGASS_TEXT_COLOR = new Color(0f, 0.2901961f, 0.38431373f, 1f);

	private GameObject materialPrefab;

	private GameObject sourceContainer;

	private GameObject temperaturePanel;

	private GameObject resultsContainer;

	private LocText headerLabel;

	public CodexTemperatureTransitionPanel(Element source, TransitionType type)
	{
		sourceElement = source;
		transitionType = type;
	}

	public override void Configure(GameObject contentGameObject, Transform displayPane, Dictionary<CodexTextStyle, TextStyleSetting> textStyles)
	{
		HierarchyReferences component = contentGameObject.GetComponent<HierarchyReferences>();
		materialPrefab = component.GetReference<RectTransform>("MaterialPrefab").gameObject;
		sourceContainer = component.GetReference<RectTransform>("SourceContainer").gameObject;
		temperaturePanel = component.GetReference<RectTransform>("TemperaturePanel").gameObject;
		resultsContainer = component.GetReference<RectTransform>("ResultsContainer").gameObject;
		headerLabel = component.GetReference<LocText>("HeaderLabel");
		ClearPanel();
		ConfigureSource(contentGameObject, displayPane, textStyles);
		ConfigureTemperature(contentGameObject, displayPane, textStyles);
		ConfigureResults(contentGameObject, displayPane, textStyles);
	}

	private void ConfigureSource(GameObject contentGameObject, Transform displayPane, Dictionary<CodexTextStyle, TextStyleSetting> textStyles)
	{
		HierarchyReferences component = Util.KInstantiateUI(materialPrefab, sourceContainer, force_active: true).GetComponent<HierarchyReferences>();
		Tuple<Sprite, Color> uISprite = Def.GetUISprite(sourceElement);
		component.GetReference<Image>("Icon").sprite = uISprite.first;
		component.GetReference<Image>("Icon").color = uISprite.second;
		component.GetReference<LocText>("Title").text = $"{GameUtil.GetFormattedMass(1f)}";
		component.GetReference<LocText>("Title").color = Color.black;
		component.GetReference<ToolTip>("ToolTip").toolTip = sourceElement.name;
		component.GetReference<KButton>("Button").onClick += delegate
		{
			ManagementMenu.Instance.codexScreen.ChangeArticle(UI.ExtractLinkID(sourceElement.tag.ProperName()));
		};
	}

	private void ConfigureTemperature(GameObject contentGameObject, Transform displayPane, Dictionary<CodexTextStyle, TextStyleSetting> textStyles)
	{
		float temp = ((transitionType == TransitionType.COOL) ? sourceElement.lowTemp : sourceElement.highTemp);
		HierarchyReferences component = temperaturePanel.GetComponent<HierarchyReferences>();
		Sprite sprite = null;
		Color color = default(Color);
		string text = GameUtil.GetFormattedTemperature(temp);
		string toolTip = "";
		switch (transitionType)
		{
		case TransitionType.HEAT:
			sprite = Assets.GetSprite("crew_state_temp_up");
			color = Color.red;
			toolTip = GameUtil.SafeStringFormat(CODEX.FORMAT_STRINGS.TEMPERATURE_OVER, GameUtil.GetFormattedTemperature(temp));
			break;
		case TransitionType.COOL:
			sprite = Assets.GetSprite("crew_state_temp_down");
			color = Color.blue;
			toolTip = GameUtil.SafeStringFormat(CODEX.FORMAT_STRINGS.TEMPERATURE_UNDER, GameUtil.GetFormattedTemperature(temp));
			break;
		case TransitionType.SUBLIMATE:
			sprite = Assets.GetSprite("codex_sublimation");
			color = SUBLIMATE_TEXT_COLOR;
			text = CODEX.FORMAT_STRINGS.SUBLIMATION_NAME;
			toolTip = GameUtil.SafeStringFormat(CODEX.FORMAT_STRINGS.SUBLIMATION_TRESHOLD, GameUtil.GetFormattedMass(1.8f));
			break;
		case TransitionType.OFFGASS:
			sprite = Assets.GetSprite("codex_offgas");
			color = OFFGASS_TEXT_COLOR;
			text = CODEX.FORMAT_STRINGS.OFFGASS_NAME;
			toolTip = GameUtil.SafeStringFormat(CODEX.FORMAT_STRINGS.OFFGASS_TRESHOLD, GameUtil.GetFormattedMass(1.8f));
			break;
		}
		component.GetReference<Image>("Icon").sprite = sprite;
		LocText reference = component.GetReference<LocText>("Label");
		reference.text = text;
		reference.textWrappingMode = TextWrappingModes.NoWrap;
		reference.gameObject.SetActive(text != null);
		component.GetReference<LocText>("Label").color = color;
		component.GetReference<ToolTip>("ToolTip").toolTip = toolTip;
	}

	private void ConfigureResults(GameObject contentGameObject, Transform displayPanel, Dictionary<CodexTextStyle, TextStyleSetting> textStyles)
	{
		Element primaryElement = null;
		Element secondaryElement = null;
		float num = 1f;
		float num2 = 0f;
		switch (transitionType)
		{
		case TransitionType.HEAT:
			primaryElement = sourceElement.highTempTransition;
			secondaryElement = ElementLoader.FindElementByHash(sourceElement.highTempTransitionOreID);
			num2 = sourceElement.highTempTransitionOreMassConversion;
			break;
		case TransitionType.COOL:
			primaryElement = sourceElement.lowTempTransition;
			secondaryElement = ElementLoader.FindElementByHash(sourceElement.lowTempTransitionOreID);
			num2 = sourceElement.lowTempTransitionOreMassConversion;
			break;
		case TransitionType.SUBLIMATE:
		{
			primaryElement = ElementLoader.FindElementByHash(sourceElement.sublimateId);
			secondaryElement = null;
			num2 = sourceElement.sublimateRate;
			num = sourceElement.sublimateEfficiency;
			if (primaryElement != null)
			{
				break;
			}
			GameObject prefab = Assets.GetPrefab(sourceElement.id.CreateTag());
			if (prefab != null)
			{
				Sublimates component = prefab.GetComponent<Sublimates>();
				if (component != null)
				{
					primaryElement = ElementLoader.FindElementByHash(component.info.sublimatedElement);
					num2 = component.info.sublimationRate;
					num = component.info.massPower;
				}
			}
			break;
		}
		case TransitionType.OFFGASS:
			primaryElement = ElementLoader.FindElementByHash(sourceElement.sublimateId);
			secondaryElement = null;
			num2 = sourceElement.offGasPercentage;
			num = sourceElement.offGasPercentage;
			break;
		}
		HierarchyReferences component2 = Util.KInstantiateUI(materialPrefab, resultsContainer, force_active: true).GetComponent<HierarchyReferences>();
		Tuple<Sprite, Color> uISprite = Def.GetUISprite(primaryElement);
		component2.GetReference<Image>("Icon").sprite = uISprite.first;
		component2.GetReference<Image>("Icon").color = uISprite.second;
		string text = $"{GameUtil.GetFormattedMass(num)}";
		if (secondaryElement != null)
		{
			text = $"{GameUtil.GetFormattedMass(num - num2)}";
		}
		component2.GetReference<LocText>("Title").text = text;
		component2.GetReference<LocText>("Title").color = Color.black;
		component2.GetReference<ToolTip>("ToolTip").toolTip = primaryElement.name;
		component2.GetReference<KButton>("Button").onClick += delegate
		{
			ManagementMenu.Instance.codexScreen.ChangeArticle(UI.ExtractLinkID(primaryElement.tag.ProperName()));
		};
		if (secondaryElement != null)
		{
			HierarchyReferences component3 = Util.KInstantiateUI(materialPrefab, resultsContainer, force_active: true).GetComponent<HierarchyReferences>();
			Tuple<Sprite, Color> uISprite2 = Def.GetUISprite(secondaryElement);
			component3.GetReference<Image>("Icon").sprite = uISprite2.first;
			component3.GetReference<Image>("Icon").color = uISprite2.second;
			component3.GetReference<LocText>("Title").text = $"{GameUtil.GetFormattedMass(num2 * num)} {secondaryElement.name}";
			component3.GetReference<LocText>("Title").color = Color.black;
			component3.GetReference<ToolTip>("ToolTip").toolTip = secondaryElement.name;
			component3.GetReference<KButton>("Button").onClick += delegate
			{
				ManagementMenu.Instance.codexScreen.ChangeArticle(UI.ExtractLinkID(secondaryElement.tag.ProperName()));
			};
		}
		headerLabel.SetText((secondaryElement == null) ? string.Format(CODEX.FORMAT_STRINGS.TRANSITION_LABEL_TO_ONE_ELEMENT, sourceElement.name, primaryElement.name) : string.Format(CODEX.FORMAT_STRINGS.TRANSITION_LABEL_TO_TWO_ELEMENTS, sourceElement.name, primaryElement.name, secondaryElement.name));
	}

	private void ClearPanel()
	{
		foreach (Transform item in sourceContainer.transform)
		{
			Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in resultsContainer.transform)
		{
			Object.Destroy(item2.gameObject);
		}
	}
}
