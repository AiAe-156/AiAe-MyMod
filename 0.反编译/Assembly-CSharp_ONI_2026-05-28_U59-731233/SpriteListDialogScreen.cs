using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteListDialogScreen : KModalScreen
{
	private struct Button
	{
		public System.Action action;

		public GameObject gameObject;

		public string label;
	}

	public System.Action onDeactivateCB;

	[SerializeField]
	private GameObject buttonPrefab;

	[SerializeField]
	private GameObject buttonPanel;

	[SerializeField]
	private LocText titleText;

	[SerializeField]
	private LocText popupMessage;

	[SerializeField]
	private GameObject listPanel;

	[SerializeField]
	private GameObject listPrefab;

	private List<Button> buttons;

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		base.gameObject.SetActive(value: false);
		buttons = new List<Button>();
	}

	public override bool IsModal()
	{
		return true;
	}

	public override void OnKeyDown(KButtonEvent e)
	{
		if (e.TryConsume(Action.Escape))
		{
			Deactivate();
		}
		else
		{
			base.OnKeyDown(e);
		}
	}

	public void AddOption(string text, System.Action action)
	{
		GameObject gameObject = Util.KInstantiateUI(buttonPrefab, buttonPanel, force_active: true);
		buttons.Add(new Button
		{
			label = text,
			action = action,
			gameObject = gameObject
		});
	}

	public void AddListRow(Sprite sprite, string text, float width = -1f, float height = -1f)
	{
		GameObject gameObject = Util.KInstantiateUI(listPrefab, listPanel, force_active: true);
		gameObject.GetComponentInChildren<LocText>().text = text;
		Image componentInChildren = gameObject.GetComponentInChildren<Image>();
		componentInChildren.sprite = sprite;
		if (sprite == null)
		{
			Color color = componentInChildren.color;
			color.a = 0f;
			componentInChildren.color = color;
		}
		if (width >= 0f || height >= 0f)
		{
			AspectRatioFitter component = componentInChildren.GetComponent<AspectRatioFitter>();
			component.enabled = false;
			LayoutElement component2 = componentInChildren.GetComponent<LayoutElement>();
			component2.minWidth = width;
			component2.preferredWidth = width;
			component2.minHeight = height;
			component2.preferredHeight = height;
		}
		else
		{
			AspectRatioFitter component3 = componentInChildren.GetComponent<AspectRatioFitter>();
			float aspectRatio = ((sprite == null) ? 1f : (sprite.rect.width / sprite.rect.height));
			component3.aspectRatio = aspectRatio;
		}
	}

	public void PopupConfirmDialog(string text, string title_text = null)
	{
		foreach (Button button in buttons)
		{
			button.gameObject.GetComponentInChildren<LocText>().text = button.label;
			button.gameObject.GetComponent<KButton>().onClick += button.action;
		}
		if (title_text != null)
		{
			titleText.text = title_text;
		}
		popupMessage.text = text;
	}

	protected override void OnDeactivate()
	{
		if (onDeactivateCB != null)
		{
			onDeactivateCB();
		}
		base.OnDeactivate();
	}
}
