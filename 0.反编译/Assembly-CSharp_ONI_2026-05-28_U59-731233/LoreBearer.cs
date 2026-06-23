using System;
using KSerialization;
using STRINGS;
using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/LoreBearer")]
public class LoreBearer : KMonoBehaviour
{
	[Serialize]
	private bool BeenClicked = false;

	public string BeenSearched = UI.USERMENUACTIONS.READLORE.ALREADY_SEARCHED;

	private string[] collectionsToUnlockFrom;

	public Func<int> GetSidescreenSortOrder;

	[Serialize]
	public string poiOverrideLoreUnlockId;

	[Serialize]
	public string poiOverrideLoreDisplayText;

	[Serialize]
	public string poiOverrideNextCollectionId;

	[Tooltip("Controls if the lore should be active. The Inspect button will also disappear if set to true.")]
	public bool hideLore;

	public bool useDefaultLore = true;

	private LoreBearerAction displayContentAction;

	public string content => Strings.Get("STRINGS.LORE.BUILDINGS." + base.gameObject.name + ".ENTRY");

	public string SidescreenButtonText => BeenClicked ? UI.USERMENUACTIONS.READLORE.ALREADYINSPECTED : UI.USERMENUACTIONS.READLORE.NAME;

	public string SidescreenButtonTooltip => BeenClicked ? UI.USERMENUACTIONS.READLORE.TOOLTIP_ALREADYINSPECTED : UI.USERMENUACTIONS.READLORE.TOOLTIP;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (displayContentAction == null && !string.IsNullOrEmpty(poiOverrideLoreUnlockId))
		{
			if (!string.IsNullOrEmpty(poiOverrideNextCollectionId))
			{
				displayContentAction = LoreBearerUtil.UnlockSpecificEntryThenNext(poiOverrideLoreUnlockId, Strings.Get(poiOverrideLoreDisplayText), LoreBearerUtil.GetUnlockActionForCollection(poiOverrideNextCollectionId));
			}
			else
			{
				displayContentAction = LoreBearerUtil.UnlockSpecificEntry(poiOverrideLoreUnlockId, Strings.Get(poiOverrideLoreDisplayText));
			}
		}
	}

	public LoreBearer Internal_SetContent(LoreBearerAction action)
	{
		displayContentAction = action;
		return this;
	}

	public LoreBearer Internal_SetContent(LoreBearerAction action, string[] collectionsToUnlockFrom)
	{
		displayContentAction = action;
		this.collectionsToUnlockFrom = collectionsToUnlockFrom;
		return this;
	}

	public static InfoDialogScreen ShowPopupDialog()
	{
		return (InfoDialogScreen)GameScreenManager.Instance.StartScreen(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, GameScreenManager.Instance.ssOverlayCanvas.gameObject);
	}

	private void OnClickRead()
	{
		InfoDialogScreen infoDialogScreen = ShowPopupDialog().SetHeader(base.gameObject.GetComponent<KSelectable>().GetProperName()).AddDefaultOK(escapeCloses: true);
		if (BeenClicked)
		{
			infoDialogScreen.AddPlainText(BeenSearched);
			return;
		}
		BeenClicked = true;
		if (DlcManager.IsExpansion1Active())
		{
			Scenario.SpawnPrefab(Grid.PosToCell(base.gameObject), 0, 1, "OrbitalResearchDatabank", Grid.SceneLayer.Front).SetActive(value: true);
			PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, Assets.GetPrefab("OrbitalResearchDatabank".ToTag()).GetProperName(), base.gameObject.transform);
		}
		if (displayContentAction != null)
		{
			displayContentAction(infoDialogScreen);
		}
		else if (useDefaultLore)
		{
			LoreBearerUtil.UnlockNextJournalEntry(infoDialogScreen);
		}
	}

	public void OnSidescreenButtonPressed()
	{
		OnClickRead();
	}

	public bool SidescreenButtonInteractable()
	{
		return !BeenClicked;
	}

	public int GetSideScreenSortOrder()
	{
		if (GetSidescreenSortOrder != null)
		{
			return GetSidescreenSortOrder();
		}
		return -100;
	}

	public void Debug_ResetSearched()
	{
		BeenClicked = false;
	}
}
