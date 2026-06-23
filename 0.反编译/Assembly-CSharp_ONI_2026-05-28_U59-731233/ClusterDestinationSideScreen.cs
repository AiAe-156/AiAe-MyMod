using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class ClusterDestinationSideScreen : SideScreenContent
{
	public Image hexEmptyBG;

	public Image destinationImage;

	[Header("Destination selection Section")]
	public RectTransform destinationSection;

	public LocText destinationInfoLabel;

	public KButton changeDestinationButton;

	public ToolTip changeDestinationButtonTooltip;

	public KButton clearDestinationButton;

	[Header("Landing Platform Section")]
	public RectTransform landingPlatformSection;

	public LocText landingPlatformInfoLabel;

	public DropDown launchPadDropDown;

	[Header("Round Trip Section")]
	public RectTransform roundtripSection;

	public LocText roundTripInfoLabel;

	public LocText roundTripButtonLabel;

	public KButton repeatButton;

	public ToolTip roundtripButtonTooltip;

	[Space]
	public ColorStyleSetting defaultButton;

	public ColorStyleSetting highlightButton;

	private int m_refreshHandle = -1;

	private int m_refreshOnCancelHandle = -1;

	private ClusterDestinationSelector targetSelector { get; set; }

	private RocketClusterDestinationSelector targetRocketSelector { get; set; }

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		CheckShouldShowTopTitle = () => false;
	}

	protected override void OnSpawn()
	{
		changeDestinationButton.onClick += OnClickChangeDestination;
		clearDestinationButton.onClick += OnClickClearDestination;
		launchPadDropDown.targetDropDownContainer = GameScreenManager.Instance.ssOverlayCanvas;
		launchPadDropDown.CustomizeEmptyRow(UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.FIRSTAVAILABLE, null);
		repeatButton.onClick += OnRepeatClicked;
	}

	public override int GetSideScreenSortOrder()
	{
		return 103;
	}

	protected override void OnShow(bool show)
	{
		base.OnShow(show);
		if (show)
		{
			Refresh();
			m_refreshHandle = targetSelector.Subscribe(543433792, delegate
			{
				Refresh();
			});
			m_refreshOnCancelHandle = targetSelector.Subscribe(94158097, delegate
			{
				Refresh();
			});
			return;
		}
		if (m_refreshHandle != -1)
		{
			targetSelector.Unsubscribe(m_refreshHandle);
			m_refreshHandle = -1;
			launchPadDropDown.Close();
		}
		if (m_refreshOnCancelHandle != -1)
		{
			targetSelector.Unsubscribe(m_refreshOnCancelHandle);
			m_refreshOnCancelHandle = -1;
			launchPadDropDown.Close();
		}
	}

	public override bool IsValidForTarget(GameObject target)
	{
		ClusterDestinationSelector component = target.GetComponent<ClusterDestinationSelector>();
		bool flag = component != null && component.assignable;
		bool flag2 = target.GetComponent<RocketModuleCluster>() != null && target.GetComponent<RocketModuleCluster>().GetComponent<PassengerRocketModule>() != null;
		bool flag3 = target.GetComponent<RocketModuleCluster>() != null && target.GetComponent<RocketModuleCluster>().GetComponent<RoboPilotModule>() != null;
		if (flag2 || flag3)
		{
			return true;
		}
		bool flag4 = target.GetComponent<RocketControlStation>() != null && target.GetComponent<RocketControlStation>().GetMyWorld().GetComponent<Clustercraft>()
			.Status != Clustercraft.CraftStatus.Launching;
		return flag || flag4;
	}

	public override void SetTarget(GameObject target)
	{
		targetSelector = target.GetComponent<ClusterDestinationSelector>();
		if (targetSelector == null)
		{
			if (target.GetComponent<RocketModuleCluster>() != null)
			{
				targetSelector = target.GetComponent<RocketModuleCluster>().CraftInterface.GetClusterDestinationSelector();
			}
			else if (target.GetComponent<RocketControlStation>() != null)
			{
				targetSelector = target.GetMyWorld().GetComponent<Clustercraft>().ModuleInterface.GetClusterDestinationSelector();
			}
		}
		targetRocketSelector = targetSelector as RocketClusterDestinationSelector;
		changeDestinationButtonTooltip.SetSimpleTooltip(targetSelector.changeTargetButtonTooltipString);
		clearDestinationButton.GetComponent<ToolTip>().SetSimpleTooltip(targetSelector.clearTargetButtonTooltipString);
	}

	private void Refresh(object data = null)
	{
		EntityLayer locationEntity = EntityLayer.None;
		bool flag = ClusterMapScreen.Instance.GetMode() == ClusterMapScreen.Mode.SelectDestination;
		if (!targetSelector.IsAtDestination())
		{
			ClusterGridEntity clusterEntityTarget = targetSelector.GetClusterEntityTarget();
			if (clusterEntityTarget != null)
			{
				destinationImage.sprite = clusterEntityTarget.GetUISprite();
				destinationInfoLabel.text = GameUtil.SafeStringFormat(UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.DESTINATION_LABEL, clusterEntityTarget.GetProperName());
			}
			else
			{
				ClusterGrid.Instance.GetLocationDescription(targetSelector.GetDestination(), out var sprite, out var label, out var _, out locationEntity);
				destinationImage.sprite = sprite;
				destinationInfoLabel.text = GameUtil.SafeStringFormat(UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.DESTINATION_LABEL, label);
			}
			clearDestinationButton.isInteractable = !flag;
		}
		else
		{
			bool flag2 = targetRocketSelector != null && targetRocketSelector.Repeat && targetRocketSelector.PreviousDestination != AxialI.INVALID;
			string text = "";
			if (flag2)
			{
				ClusterGridEntity visibleEntityOfLayerAtCell = ClusterGrid.Instance.GetVisibleEntityOfLayerAtCell(targetRocketSelector.PreviousDestination, targetRocketSelector.requiredEntityLayer);
				if (visibleEntityOfLayerAtCell != null)
				{
					destinationImage.sprite = visibleEntityOfLayerAtCell.GetUISprite();
					text = GameUtil.SafeStringFormat(UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.DESTINATION_ROUNTRIP_LABEL, visibleEntityOfLayerAtCell.GetProperName());
				}
				else
				{
					ClusterGrid.Instance.GetLocationDescription(targetRocketSelector.PreviousDestination, out var sprite2, out var label2, out var _, out locationEntity);
					destinationImage.sprite = sprite2;
					text = GameUtil.SafeStringFormat(UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.DESTINATION_ROUNTRIP_LABEL, label2);
				}
			}
			else
			{
				destinationImage.sprite = Assets.GetSprite("hex_unknown");
				text = GameUtil.SafeStringFormat(UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.DESTINATION_LABEL, UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.DESTINATION_LABEL_INVALID);
			}
			destinationInfoLabel.text = text;
			clearDestinationButton.isInteractable = false;
		}
		changeDestinationButtonTooltip.SetSimpleTooltip(flag ? ((string)UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.CHANGE_DESTINATION_BUTTON_SELECTING_TOOLTIP) : targetSelector.changeTargetButtonTooltipString);
		changeDestinationButton.isInteractable = !flag;
		if (flag)
		{
			destinationInfoLabel.text = UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.DESTINATION_LABEL_SELECTING;
		}
		if (targetRocketSelector != null)
		{
			List<LaunchPad> launchPadsForDestination = LaunchPad.GetLaunchPadsForDestination(targetRocketSelector.GetDestination());
			landingPlatformSection.gameObject.SetActive(value: true);
			roundtripSection.gameObject.SetActive(value: true);
			launchPadDropDown.Initialize(launchPadsForDestination, OnLaunchPadEntryClick, PadDropDownSort, PadDropDownEntryRefreshAction, displaySelectedValueWhenClosed: true, targetRocketSelector);
			if (!targetRocketSelector.IsAtDestination() && launchPadsForDestination.Count > 0)
			{
				launchPadDropDown.openButton.isInteractable = true;
				LaunchPad destinationPad = targetRocketSelector.GetDestinationPad();
				if (destinationPad != null)
				{
					launchPadDropDown.selectedLabel.text = destinationPad.GetProperName();
					landingPlatformInfoLabel.SetText(GameUtil.SafeStringFormat(UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.LANDING_PLATFORM_LABEL, destinationPad.GetProperName()));
				}
				else
				{
					launchPadDropDown.selectedLabel.text = UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.FIRSTAVAILABLE;
					landingPlatformInfoLabel.SetText(GameUtil.SafeStringFormat(UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.LANDING_PLATFORM_LABEL, UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.FIRSTAVAILABLE));
				}
			}
			else
			{
				launchPadDropDown.selectedLabel.text = UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.FIRSTAVAILABLE;
				landingPlatformInfoLabel.SetText(GameUtil.SafeStringFormat(UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.LANDING_PLATFORM_LABEL, UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.FIRSTAVAILABLE));
				launchPadDropDown.openButton.isInteractable = false;
			}
			RefreshRepeatButtonLabels();
		}
		else
		{
			landingPlatformSection.gameObject.SetActive(value: false);
			roundtripSection.gameObject.SetActive(value: false);
		}
		hexEmptyBG.gameObject.SetActive(locationEntity == EntityLayer.POI);
	}

	private void OnClickChangeDestination()
	{
		if (targetSelector.assignable)
		{
			ClusterMapScreen.Instance.ShowInSelectDestinationMode(targetSelector);
			AxialI myWorldLocation = targetSelector.GetMyWorldLocation();
			AxialI destination = targetSelector.GetDestination();
			AxialI randomVisibleAdjacentCellLocation = ClusterGrid.Instance.GetRandomVisibleAdjacentCellLocation(myWorldLocation, destination);
			if (randomVisibleAdjacentCellLocation != AxialI.INVALID)
			{
				ClusterMapScreen.Instance.OnHoverHex(ClusterMapScreen.Instance.GetClusterMapHexAtLocation(randomVisibleAdjacentCellLocation));
			}
		}
		Refresh();
		if (changeDestinationButtonTooltip.isHovering)
		{
			ToolTipScreen.Instance.ClearToolTip(changeDestinationButtonTooltip);
			ToolTipScreen.Instance.SetToolTip(changeDestinationButtonTooltip);
		}
	}

	private void OnClickClearDestination()
	{
		targetSelector.SetDestination(targetSelector.GetMyWorldLocation());
	}

	private void OnLaunchPadEntryClick(IListableOption option, object data)
	{
		LaunchPad destinationPad = (LaunchPad)option;
		targetRocketSelector.SetDestinationPad(destinationPad);
	}

	private void PadDropDownEntryRefreshAction(DropDownEntry entry, object targetData)
	{
		LaunchPad launchPad = (LaunchPad)entry.entryData;
		Clustercraft component = targetRocketSelector.GetComponent<Clustercraft>();
		if (launchPad != null)
		{
			if (component.CanLandAtPad(launchPad, out var failReason) == Clustercraft.PadLandingStatus.CanNeverLand)
			{
				entry.button.isInteractable = false;
				entry.image.sprite = Assets.GetSprite("iconWarning");
				entry.tooltip.SetSimpleTooltip(failReason);
			}
			else
			{
				entry.button.isInteractable = true;
				entry.image.sprite = launchPad.GetComponent<Building>().Def.GetUISprite();
				entry.tooltip.SetSimpleTooltip(string.Format(UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.DROPDOWN_TOOLTIP_VALID_SITE, launchPad.GetProperName()));
			}
		}
		else
		{
			entry.button.isInteractable = true;
			entry.image.sprite = Assets.GetBuildingDef("LaunchPad").GetUISprite();
			entry.tooltip.SetSimpleTooltip(UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.DROPDOWN_TOOLTIP_FIRST_AVAILABLE);
		}
	}

	private int PadDropDownSort(IListableOption a, IListableOption b, object targetData)
	{
		return 0;
	}

	private void OnRepeatClicked()
	{
		targetRocketSelector.Repeat = !targetRocketSelector.Repeat;
		Refresh();
		RefreshRepeatButtonLabels();
	}

	private void RefreshRepeatButtonLabels()
	{
		roundTripInfoLabel.SetText(targetRocketSelector.Repeat ? UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.ROUNDTRIP_LABEL_ROUNDTRIP : UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.ROUNDTRIP_LABEL_ONE_WAY);
		roundTripButtonLabel.SetText(targetRocketSelector.Repeat ? UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.ROUNDTRIP_BUTTON_ONE_WAY : UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.ROUNDTRIP_BUTTON_ROUNDTRIP);
		roundtripButtonTooltip.SetSimpleTooltip(targetRocketSelector.Repeat ? UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.ROUNDTRIP_BUTTON_TOOLTIP_ONE_WAY : UI.UISIDESCREENS.CLUSTERDESTINATIONSIDESCREEN.ROUNDTRIP_BUTTON_TOOLTIP_ROUNDTRIP);
	}
}
