using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UtilLibs.UIcmp;

public class FMultiSelectDropdown : KMonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public class FDropDownEntry
	{
		public string Title;

		public string Description = "";

		public Action<bool> OnToggled;

		public bool Enabled = true;

		public FToggle Toggle;

		public FDropDownEntry(string title, Action<bool> onToggled, bool enabled = true, string tooltip = "")
		{
			Title = title;
			OnToggled = onToggled;
			Enabled = enabled;
			Description = tooltip;
		}
	}

	public class FDropDownButtonEntry : FDropDownEntry
	{
		public FButton Button;

		public FDropDownButtonEntry(string title, Action<bool> onToggled, string tooltip = "")
			: base(title, onToggled, enabled: true, tooltip)
		{
		}
	}

	public Action RefreshUI;

	private GameObject DropDownContent;

	private FToggle entryPrefab;

	private FButton buttonEntryPrefab;

	private Image backgroundImage;

	public Color Inactive = UIUtils.rgb(62f, 67f, 87f);

	public Color OnHover = UIUtils.rgb(88f, 95f, 122f);

	public List<FDropDownEntry> DropDownEntries = null;

	public override void OnPrefabInit()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		((KMonoBehaviour)this).OnPrefabInit();
		backgroundImage = ((Component)this).GetComponent<Image>();
		((Graphic)backgroundImage).color = Inactive;
		DropDownContent = ((Component)((KMonoBehaviour)this).transform.Find("DropDownContent")).gameObject;
		entryPrefab = EntityTemplateExtensions.AddOrGet<FToggle>(((Component)((KMonoBehaviour)this).transform.Find("DropDownContent/Item")).gameObject);
		((Component)entryPrefab).gameObject.SetActive(false);
		Transform obj = ((KMonoBehaviour)this).transform.Find("DropDownContent/ButtonItem");
		object obj2;
		if (obj == null)
		{
			obj2 = null;
		}
		else
		{
			GameObject gameObject = ((Component)obj).gameObject;
			obj2 = ((gameObject != null) ? EntityTemplateExtensions.AddOrGet<FButton>(gameObject) : null);
		}
		buttonEntryPrefab = (FButton)obj2;
		FButton fButton = buttonEntryPrefab;
		if (fButton != null)
		{
			GameObject gameObject2 = ((Component)fButton).gameObject;
			if (gameObject2 != null)
			{
				gameObject2.SetActive(false);
			}
		}
		InitializeDropDown();
	}

	public void InitializeDropDown()
	{
		if (DropDownEntries == null)
		{
			return;
		}
		DropDownContent.SetActive(true);
		foreach (FDropDownEntry dropDownEntry in DropDownEntries)
		{
			if (dropDownEntry is FDropDownButtonEntry entry && (Object)(object)buttonEntryPrefab != (Object)null)
			{
				InitializeButton(entry);
			}
			else if ((Object)(object)entryPrefab != (Object)null)
			{
				InitializeToggle(dropDownEntry);
			}
		}
		DropDownContent.SetActive(false);
	}

	private void InitializeButton(FDropDownButtonEntry entry)
	{
		FButton fButton = Util.KInstantiateUI<FButton>(((Component)buttonEntryPrefab).gameObject, DropDownContent, true);
		fButton.OnClick += delegate
		{
			entry.OnToggled(obj: true);
		};
		if (RefreshUI != null)
		{
			fButton.OnClick += delegate
			{
				RefreshUI();
			};
		}
		((TMP_Text)((Component)fButton).GetComponentInChildren<LocText>()).text = entry.Title;
		if (entry.Description != null && entry.Description.Length > 0)
		{
			UIUtils.AddSimpleTooltipToObject(((KMonoBehaviour)fButton).transform, entry.Description);
		}
		entry.Button = fButton;
	}

	private void InitializeToggle(FDropDownEntry entry)
	{
		FToggle fToggle = Util.KInstantiateUI<FToggle>(((Component)entryPrefab).gameObject, DropDownContent, true);
		fToggle.SetCheckmark("Background/Checkmark");
		fToggle.SetOnFromCode(entry.Enabled);
		fToggle.OnClick += entry.OnToggled;
		if (RefreshUI != null)
		{
			fToggle.OnClick += delegate
			{
				RefreshUI();
			};
		}
		((TMP_Text)((Component)fToggle).GetComponentInChildren<LocText>()).text = entry.Title;
		if (entry.Description != null && entry.Description.Length > 0)
		{
			UIUtils.AddSimpleTooltipToObject(((KMonoBehaviour)fToggle).transform, entry.Description);
		}
		entry.Toggle = fToggle;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)backgroundImage).color = OnHover;
		GameObject dropDownContent = DropDownContent;
		if (dropDownContent != null)
		{
			dropDownContent.SetActive(true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)backgroundImage).color = Inactive;
		GameObject dropDownContent = DropDownContent;
		if (dropDownContent != null)
		{
			dropDownContent.SetActive(false);
		}
	}
}
