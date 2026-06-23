using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class CodexElementCategoryList : CodexCollapsibleHeader
{
	private List<GameObject> rows = new List<GameObject>();

	public Tag categoryTag { get; set; }

	public CodexElementCategoryList()
		: base(UI.CODEX.CATEGORYNAMES.ELEMENTS, null)
	{
	}

	public override void Configure(GameObject contentGameObject, Transform displayPane, Dictionary<CodexTextStyle, TextStyleSetting> textStyles)
	{
		HierarchyReferences component = contentGameObject.GetComponent<HierarchyReferences>();
		base.ContentsGameObject = component.GetReference<RectTransform>("ContentContainer").gameObject;
		base.Configure(contentGameObject, displayPane, textStyles);
		RectTransform reference = component.GetReference<RectTransform>("HeaderLabel");
		RectTransform reference2 = component.GetReference<RectTransform>("PrefabLabelWithIcon");
		ClearPanel(reference2.transform.parent, reference2);
		reference.GetComponent<LocText>().SetText(UI.CODEX.CATEGORYNAMES.ELEMENTS);
		foreach (GameObject item in Assets.GetPrefabsWithTag(categoryTag))
		{
			GameObject gameObject = Util.KInstantiateUI(reference2.gameObject, reference2.parent.gameObject, force_active: true);
			Image componentInChildren = gameObject.GetComponentInChildren<Image>();
			Tuple<Sprite, Color> uISprite = Def.GetUISprite(item);
			componentInChildren.sprite = uISprite.first;
			componentInChildren.color = uISprite.second;
			gameObject.GetComponentInChildren<LocText>().SetText(item.GetProperName());
			rows.Add(gameObject);
		}
	}

	private void ClearPanel(Transform containerToClear, Transform skipDestroyingPrefab)
	{
		skipDestroyingPrefab.SetAsFirstSibling();
		for (int num = containerToClear.childCount - 1; num >= 1; num--)
		{
			Object.Destroy(containerToClear.GetChild(num).gameObject);
		}
		for (int num2 = rows.Count - 1; num2 >= 0; num2--)
		{
			Object.Destroy(rows[num2].gameObject);
		}
		rows.Clear();
	}
}
