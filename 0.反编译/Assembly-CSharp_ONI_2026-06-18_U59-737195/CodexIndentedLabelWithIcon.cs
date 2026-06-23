using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodexIndentedLabelWithIcon : CodexWidget<CodexIndentedLabelWithIcon>
{
	public CodexImage icon { get; set; }

	public CodexText label { get; set; }

	public string stringKey { get; set; } = "";

	public string batchedAnimPrefabSourceID { get; set; } = "";

	public string spriteName { get; set; } = "";

	public CodexIndentedLabelWithIcon()
	{
		icon = new CodexImage();
		label = new CodexText();
	}

	public CodexIndentedLabelWithIcon(string text, CodexTextStyle style, Tuple<Sprite, Color> coloredSprite)
	{
		icon = new CodexImage(coloredSprite);
		label = new CodexText(text, style);
	}

	public CodexIndentedLabelWithIcon(string text, CodexTextStyle style, Tuple<Sprite, Color> coloredSprite, int iconWidth, int iconHeight)
	{
		icon = new CodexImage(iconWidth, iconHeight, coloredSprite);
		label = new CodexText(text, style);
	}

	public override void Configure(GameObject contentGameObject, Transform displayPane, Dictionary<CodexTextStyle, TextStyleSetting> textStyles)
	{
		if (!string.IsNullOrEmpty(stringKey))
		{
			label.stringKey = stringKey;
		}
		if (!string.IsNullOrEmpty(batchedAnimPrefabSourceID))
		{
			GameObject gameObject = Assets.TryGetPrefab(batchedAnimPrefabSourceID);
			KBatchedAnimController kBatchedAnimController = ((gameObject != null) ? gameObject.GetComponent<KBatchedAnimController>() : null);
			KAnimFile kAnimFile = ((kBatchedAnimController != null) ? kBatchedAnimController.AnimFiles[0] : null);
			icon.sprite = ((kAnimFile != null) ? Def.GetUISpriteFromMultiObjectAnim(kAnimFile) : null);
		}
		if (!string.IsNullOrEmpty(spriteName))
		{
			icon.sprite = Assets.GetSprite(spriteName);
		}
		Image componentInChildren = contentGameObject.GetComponentInChildren<Image>();
		icon.ConfigureImage(componentInChildren);
		label.ConfigureLabel(contentGameObject.GetComponentInChildren<LocText>(), textStyles);
		if (icon.preferredWidth != -1 && icon.preferredHeight != -1)
		{
			LayoutElement component = componentInChildren.GetComponent<LayoutElement>();
			component.minWidth = icon.preferredHeight;
			component.minHeight = icon.preferredWidth;
			component.preferredHeight = icon.preferredHeight;
			component.preferredWidth = icon.preferredWidth;
		}
	}
}
