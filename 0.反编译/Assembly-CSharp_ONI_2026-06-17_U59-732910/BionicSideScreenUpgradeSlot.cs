using System;
using STRINGS;
using UnityEngine;

public class BionicSideScreenUpgradeSlot : KMonoBehaviour
{
	public enum State
	{
		Locked,
		Empty,
		Assigned,
		Installed
	}

	public static string TEXT_BLOCKED_SLOT = UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.UPGRADE_SLOT_LOCKED;

	public static string TEXT_NO_UPGRADE_INSTALLED = UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.UPGRADE_SLOT_EMPTY;

	public static string TEXT_UPGRADE_ASSIGNED_NOT_INSTALLED = UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.UPGRADE_SLOT_ASSIGNED;

	public static string TEXT_UPGRADE_INSTALLED = UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.UPGRADE_SLOT_INSTALLED;

	public static string TEXT_TOOLTIP_BLOCKED = UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.TOOLTIP.SLOT_LOCKED;

	public static string TEXT_TOOLTIP_EMPTY = UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.TOOLTIP.SLOT_EMPTY;

	public static string TEXT_TOOLTIP_ASSIGNED = UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.TOOLTIP.SLOT_ASSIGNED;

	public static string TEXT_TOOLTIP_INSTALLED = UI.UISIDESCREENS.BIONIC_SIDE_SCREEN.TOOLTIP.SLOT_INSTALLED;

	public MultiToggle toggle;

	public KImage icon;

	public LocText label;

	public ToolTip tooltip;

	[Header("Effects settings")]
	public float inUseAnimationDuration = 0.5f;

	public Color standardColor = Color.black;

	public Color activeColor = Color.blue;

	public Color activeColorTooltip = Color.blue;

	public Action<BionicSideScreenUpgradeSlot> OnClick;

	private bool _isSelected;

	public BionicUpgradesMonitor.UpgradeComponentSlot upgradeSlot { get; private set; }

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		MultiToggle multiToggle = toggle;
		multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, new System.Action(OnSlotClicked));
	}

	public void Setup(BionicUpgradesMonitor.UpgradeComponentSlot upgradeSlot)
	{
		if (this.upgradeSlot != null)
		{
			BionicUpgradesMonitor.UpgradeComponentSlot upgradeComponentSlot = this.upgradeSlot;
			upgradeComponentSlot.OnAssignedUpgradeChanged = (Action<BionicUpgradesMonitor.UpgradeComponentSlot>)Delegate.Remove(upgradeComponentSlot.OnAssignedUpgradeChanged, new Action<BionicUpgradesMonitor.UpgradeComponentSlot>(OnAssignedUpgradeChanged));
		}
		this.upgradeSlot = upgradeSlot;
		if (upgradeSlot != null)
		{
			upgradeSlot.OnAssignedUpgradeChanged = (Action<BionicUpgradesMonitor.UpgradeComponentSlot>)Delegate.Combine(upgradeSlot.OnAssignedUpgradeChanged, new Action<BionicUpgradesMonitor.UpgradeComponentSlot>(OnAssignedUpgradeChanged));
		}
		Refresh();
	}

	private void OnAssignedUpgradeChanged(BionicUpgradesMonitor.UpgradeComponentSlot slot)
	{
		Refresh();
	}

	public void Refresh()
	{
		label.color = standardColor;
		State state = ((!upgradeSlot.IsLocked) ? State.Empty : State.Locked);
		if (state == State.Empty && upgradeSlot.HasUpgradeInstalled)
		{
			state = State.Installed;
		}
		else if (state == State.Empty && upgradeSlot.HasUpgradeComponentAssigned && !upgradeSlot.GetAssignableSlotInstance().IsUnassigning())
		{
			state = State.Assigned;
		}
		switch (state)
		{
		case State.Locked:
			tooltip.SizingSetting = ToolTip.ToolTipSizeSetting.DynamicWidthNoWrap;
			tooltip.SetSimpleTooltip(TEXT_TOOLTIP_BLOCKED);
			label.SetText(TEXT_BLOCKED_SLOT);
			label.Opacity(0.5f);
			icon.gameObject.SetActive(value: false);
			break;
		case State.Empty:
			tooltip.SizingSetting = ToolTip.ToolTipSizeSetting.DynamicWidthNoWrap;
			tooltip.SetSimpleTooltip(TEXT_TOOLTIP_EMPTY);
			label.SetText(TEXT_NO_UPGRADE_INSTALLED);
			label.Opacity(1f);
			icon.gameObject.SetActive(value: false);
			break;
		case State.Assigned:
			icon.sprite = Def.GetUISprite(upgradeSlot.assignedUpgradeComponent.gameObject).first;
			icon.Opacity(0.5f);
			icon.gameObject.SetActive(value: true);
			label.SetText(TEXT_UPGRADE_ASSIGNED_NOT_INSTALLED);
			label.Opacity(1f);
			tooltip.SizingSetting = ToolTip.ToolTipSizeSetting.MaxWidthWrapContent;
			tooltip.SetSimpleTooltip(string.Format(TEXT_TOOLTIP_ASSIGNED, upgradeSlot.assignedUpgradeComponent.GetProperName()));
			break;
		case State.Installed:
			icon.sprite = Def.GetUISprite(upgradeSlot.installedUpgradeComponent.gameObject).first;
			icon.Opacity(1f);
			icon.gameObject.SetActive(value: true);
			label.SetText(TEXT_UPGRADE_INSTALLED);
			label.Opacity(1f);
			tooltip.SizingSetting = ToolTip.ToolTipSizeSetting.MaxWidthWrapContent;
			tooltip.SetSimpleTooltip(string.Format(TEXT_TOOLTIP_INSTALLED, BionicUpgradeComponentConfig.GenerateTooltipForBooster(upgradeSlot.installedUpgradeComponent)));
			break;
		}
		SetSelected(_isSelected);
	}

	private void OnSlotClicked()
	{
		OnClick?.Invoke(this);
	}

	public void SetSelected(bool isSelected)
	{
		_isSelected = isSelected;
		bool flag = upgradeSlot == null || upgradeSlot.IsLocked;
		bool flag2 = upgradeSlot != null && upgradeSlot.HasUpgradeComponentAssigned && !upgradeSlot.GetAssignableSlotInstance().IsUnassigning();
		bool flag3 = flag2 && upgradeSlot.assignedUpgradeComponent.Booster == BionicUpgradeComponentConfig.BoosterType.Basic;
		toggle.ChangeState(((!flag) ? 2 : 0) + (flag2 ? 2 : 0) + ((flag2 && flag3) ? 2 : 0) + (isSelected ? 1 : 0));
	}
}
