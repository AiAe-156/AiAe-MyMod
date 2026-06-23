using System;
using UnityEngine;
using UnityEngine.UI;
using UtilLibs.UIcmp;

namespace UtilLibs.UI.FUI;

public class FExpandToggle : KMonoBehaviour
{
	private Image IconOpen;

	private Image IconClose;

	private FButton button;

	public bool Expanded = true;

	public Action<bool> OnChange;

	public override void OnPrefabInit()
	{
		((KMonoBehaviour)this).OnPrefabInit();
		button = EntityTemplateExtensions.AddOrGet<FButton>(((Component)this).gameObject);
		IconOpen = ((Component)((KMonoBehaviour)this).transform.Find("IconOpen")).gameObject.GetComponent<Image>();
		IconClose = ((Component)((KMonoBehaviour)this).transform.Find("IconClose")).gameObject.GetComponent<Image>();
		button.OnClick += OnClicked;
	}

	public override void OnSpawn()
	{
		((KMonoBehaviour)this).OnSpawn();
		Refresh();
	}

	public void Refresh()
	{
		Image iconOpen = IconOpen;
		if (iconOpen != null)
		{
			((Component)iconOpen).gameObject.SetActive(!Expanded);
		}
		Image iconClose = IconClose;
		if (iconClose != null)
		{
			((Component)iconClose).gameObject.SetActive(Expanded);
		}
	}

	public void OnClicked()
	{
		Expanded = !Expanded;
		Refresh();
		OnChange?.Invoke(Expanded);
	}
}
