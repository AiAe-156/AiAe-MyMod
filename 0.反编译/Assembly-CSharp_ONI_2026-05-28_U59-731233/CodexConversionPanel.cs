using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class CodexConversionPanel : CodexWidget<CodexConversionPanel>
{
	public class IconSettings
	{
		public string spriteName;

		public string tooltip;

		public System.Action onClickActions;
	}

	private LocText label;

	private GameObject materialPrefab;

	private GameObject fabricatorPrefab;

	private GameObject ingredientsContainer;

	private GameObject resultsContainer;

	private GameObject fabricatorContainer;

	private GameObject arrow1;

	private GameObject arrow2;

	private string title;

	private ElementUsage[] ins;

	private ElementUsage[] outs;

	private GameObject Converter;

	public IconSettings aidIcon;

	public CodexConversionPanel(string title, Tag ctag, float inputAmount, bool inputContinuous, Tag ptag, float outputAmount, bool outputContinuous, GameObject converter)
		: this(title, ctag, inputAmount, inputContinuous, null, ptag, outputAmount, outputContinuous, null, converter)
	{
	}

	public CodexConversionPanel(string title, Tag ctag, float inputAmount, bool inputContinuous, Func<Tag, float, bool, string> input_customFormating, Tag ptag, float outputAmount, bool outputContinuous, Func<Tag, float, bool, string> output_customFormating, GameObject converter)
	{
		this.title = title;
		ins = new ElementUsage[1]
		{
			new ElementUsage(ctag, inputAmount, inputContinuous, input_customFormating)
		};
		outs = new ElementUsage[1]
		{
			new ElementUsage(ptag, outputAmount, outputContinuous, output_customFormating)
		};
		Converter = converter;
	}

	public CodexConversionPanel(string title, ElementUsage[] ins, ElementUsage[] outs, GameObject converter)
		: this(title, ins, outs, converter, null)
	{
	}

	public CodexConversionPanel(string title, ElementUsage[] ins, ElementUsage[] outs, GameObject converter, IconSettings aidIcon)
	{
		this.title = title;
		this.ins = ((ins != null) ? ins : new ElementUsage[0]);
		this.outs = ((outs != null) ? outs : new ElementUsage[0]);
		Converter = converter;
		this.aidIcon = aidIcon;
	}

	public override void Configure(GameObject contentGameObject, Transform displayPane, Dictionary<CodexTextStyle, TextStyleSetting> textStyles)
	{
		HierarchyReferences component = contentGameObject.GetComponent<HierarchyReferences>();
		label = component.GetReference<LocText>("Title");
		materialPrefab = component.GetReference<RectTransform>("MaterialPrefab").gameObject;
		fabricatorPrefab = component.GetReference<RectTransform>("FabricatorPrefab").gameObject;
		ingredientsContainer = component.GetReference<RectTransform>("IngredientsContainer").gameObject;
		resultsContainer = component.GetReference<RectTransform>("ResultsContainer").gameObject;
		fabricatorContainer = component.GetReference<RectTransform>("FabricatorContainer").gameObject;
		arrow1 = component.GetReference<RectTransform>("Arrow1").gameObject;
		arrow2 = component.GetReference<RectTransform>("Arrow2").gameObject;
		ClearPanel();
		ConfigureConversion();
	}

	private Tuple<Sprite, Color> GetUISprite(Tag tag)
	{
		if (ElementLoader.GetElement(tag) != null)
		{
			return Def.GetUISprite(ElementLoader.GetElement(tag));
		}
		if (Assets.GetPrefab(tag) != null)
		{
			return Def.GetUISprite(Assets.GetPrefab(tag));
		}
		if (Assets.GetSprite(tag.Name) != null)
		{
			return new Tuple<Sprite, Color>(Assets.GetSprite(tag.Name), Color.white);
		}
		return Def.GetUISprite(tag);
	}

	private void ConfigureConversion()
	{
		label.text = title;
		bool active = false;
		ElementUsage[] array = ins;
		foreach (ElementUsage elementUsage in array)
		{
			Tag tag = elementUsage.tag;
			if (!(tag == Tag.Invalid))
			{
				float amount = elementUsage.amount;
				active = true;
				GameObject gameObject = Util.KInstantiateUI(materialPrefab, ingredientsContainer, force_active: true);
				HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
				Tuple<Sprite, Color> uISprite = GetUISprite(tag);
				if (uISprite != null)
				{
					component.GetReference<Image>("Icon").sprite = uISprite.first;
					component.GetReference<Image>("Icon").color = uISprite.second;
				}
				GameUtil.TimeSlice timeSlice = (elementUsage.continuous ? GameUtil.TimeSlice.PerCycle : GameUtil.TimeSlice.None);
				component.GetReference<LocText>("Amount").text = ((elementUsage.customFormating == null) ? GameUtil.GetFormattedByTag(tag, amount, timeSlice) : elementUsage.customFormating(tag, amount, elementUsage.continuous));
				component.GetReference<LocText>("Amount").color = Color.black;
				string text = tag.ProperName();
				GameObject prefab = Assets.GetPrefab(tag);
				if ((bool)prefab && prefab.GetComponent<Edible>() != null)
				{
					text = text + "\n    • " + string.Format(UI.GAMEOBJECTEFFECTS.FOOD_QUALITY, GameUtil.GetFormattedFoodQuality(prefab.GetComponent<Edible>().GetQuality()));
				}
				component.GetReference<ToolTip>("Tooltip").toolTip = text;
				component.GetReference<KButton>("Button").onClick += delegate
				{
					ManagementMenu.Instance.codexScreen.ChangeArticle(UI.ExtractLinkID(tag.ProperName()));
				};
			}
		}
		arrow1.SetActive(active);
		string name = Converter.PrefabID().Name;
		GameObject gameObject2 = Util.KInstantiateUI(fabricatorPrefab, fabricatorContainer, force_active: true);
		HierarchyReferences component2 = gameObject2.GetComponent<HierarchyReferences>();
		Tuple<Sprite, Color> uISprite2 = Def.GetUISprite(name);
		component2.GetReference<Image>("Icon").sprite = uISprite2.first;
		component2.GetReference<Image>("Icon").color = uISprite2.second;
		component2.GetReference<ToolTip>("Tooltip").toolTip = Converter.GetProperName();
		component2.GetReference<KButton>("Button").onClick += delegate
		{
			ManagementMenu.Instance.codexScreen.ChangeArticle(UI.ExtractLinkID(Converter.GetProperName()));
		};
		Image reference = component2.GetReference<Image>("AidIconIcon");
		reference.gameObject.SetActive(aidIcon != null);
		if (aidIcon != null)
		{
			Tuple<Sprite, Color> uISprite3 = Def.GetUISprite(aidIcon.spriteName);
			reference.sprite = uISprite3.first;
			reference.color = uISprite3.second;
			component2.GetReference<ToolTip>("AidIconTooltip").toolTip = aidIcon.tooltip;
			component2.GetReference<KButton>("AidIconButton").onClick += aidIcon.onClickActions;
		}
		bool active2 = false;
		ElementUsage[] array2 = outs;
		foreach (ElementUsage elementUsage2 in array2)
		{
			Tag tag2 = elementUsage2.tag;
			if (!(tag2 == Tag.Invalid))
			{
				float amount2 = elementUsage2.amount;
				active2 = true;
				GameObject gameObject3 = Util.KInstantiateUI(materialPrefab, resultsContainer, force_active: true);
				HierarchyReferences component3 = gameObject3.GetComponent<HierarchyReferences>();
				Tuple<Sprite, Color> uISprite4 = GetUISprite(tag2);
				if (uISprite4 != null)
				{
					component3.GetReference<Image>("Icon").sprite = uISprite4.first;
					component3.GetReference<Image>("Icon").color = uISprite4.second;
				}
				GameUtil.TimeSlice timeSlice2 = (elementUsage2.continuous ? GameUtil.TimeSlice.PerCycle : GameUtil.TimeSlice.None);
				component3.GetReference<LocText>("Amount").text = ((elementUsage2.customFormating == null) ? GameUtil.GetFormattedByTag(tag2, amount2, timeSlice2) : elementUsage2.customFormating(tag2, amount2, elementUsage2.continuous));
				component3.GetReference<LocText>("Amount").color = Color.black;
				string text2 = tag2.ProperName();
				GameObject prefab2 = Assets.GetPrefab(tag2);
				if ((bool)prefab2 && prefab2.GetComponent<Edible>() != null)
				{
					text2 = text2 + "\n    • " + string.Format(UI.GAMEOBJECTEFFECTS.FOOD_QUALITY, GameUtil.GetFormattedFoodQuality(prefab2.GetComponent<Edible>().GetQuality()));
				}
				component3.GetReference<ToolTip>("Tooltip").toolTip = text2;
				component3.GetReference<KButton>("Button").onClick += delegate
				{
					ManagementMenu.Instance.codexScreen.ChangeArticle(UI.ExtractLinkID(tag2.ProperName()));
				};
			}
		}
		arrow2.SetActive(active2);
	}

	private void ClearPanel()
	{
		foreach (Transform item in ingredientsContainer.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in resultsContainer.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		foreach (Transform item3 in fabricatorContainer.transform)
		{
			UnityEngine.Object.Destroy(item3.gameObject);
		}
	}
}
