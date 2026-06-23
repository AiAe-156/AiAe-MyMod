using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class KleiInventoryDLCFilter : KMonoBehaviour
{
	[SerializeField]
	private Transform dlcFilterButtonContainer;

	[SerializeField]
	private GameObject dlcFilterButtonPrefab;

	[SerializeField]
	private Image selectedDLCIcon;

	[SerializeField]
	private Image selectedDLCStripe;

	[SerializeField]
	private KButton dropdownButton;

	public System.Action onDLCFilterChanged;

	[HideInInspector]
	public string SelectedDLCID { get; set; } = null;

	private void ShowDropdown(bool show)
	{
		dlcFilterButtonContainer.gameObject.SetActive(show);
	}

	public void ResetToDefault()
	{
		SetDLCFilter(null);
	}

	public void ConfigButtons()
	{
		dropdownButton.ClearOnClick();
		dropdownButton.onClick += delegate
		{
			ShowDropdown(!dlcFilterButtonContainer.gameObject.activeSelf);
		};
		MakeButton(null);
		List<string> list = new List<string>(DlcManager.GetActiveDLCIds());
		for (int num = list.Count - 1; num >= 0; num--)
		{
			MakeButton(list[num]);
		}
		SetDLCFilter(null);
	}

	private void MakeButton(string dlcID)
	{
		GameObject gameObject = Util.KInstantiateUI(dlcFilterButtonPrefab, dlcFilterButtonContainer.gameObject, force_active: true);
		HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
		component.GetReference<Image>("Logo").sprite = ((dlcID == null) ? Assets.GetSprite("ONI_mini_logo") : Assets.GetSprite(DlcManager.GetDlcSmallLogo(dlcID)));
		component.GetReference<Image>("Stripe").sprite = Assets.GetSprite(DlcManager.GetDlcBannerSprite(dlcID));
		component.GetReference<Image>("Stripe").color = ((dlcID == null) ? Color.white : DlcManager.GetDlcBannerColor(dlcID));
		component.GetReference<KButton>("Button").ClearOnClick();
		component.GetReference<KButton>("Button").onClick += delegate
		{
			SetDLCFilter(dlcID);
			ShowDropdown(show: false);
		};
		ShowDropdown(show: false);
	}

	private void SetDLCFilter(string DLCID)
	{
		SelectedDLCID = DLCID;
		onDLCFilterChanged?.Invoke();
		selectedDLCIcon.sprite = ((DLCID == null) ? Assets.GetSprite("ONI_mini_logo") : Assets.GetSprite(DlcManager.GetDlcSmallLogo(DLCID)));
		selectedDLCStripe.color = ((DLCID == null) ? Color.white : DlcManager.GetDlcBannerColor(DLCID));
		dropdownButton.GetComponent<ToolTip>().SetSimpleTooltip(GameUtil.SafeStringFormat(UI.KLEI_INVENTORY_SCREEN.TOOLTIP_DLC_FILTER, (DLCID == null) ? ((object)UI.KLEI_INVENTORY_SCREEN.TOOLTIP_DLC_FILTER_ALL) : ((object)DlcManager.GetDlcTitle(DLCID))));
	}

	public void HideDropdown()
	{
		ShowDropdown(show: false);
	}

	public bool IsDropdownVisible()
	{
		return dlcFilterButtonContainer.gameObject.activeSelf;
	}
}
