using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class PrinterceptorSideScreen : SideScreenContent
{
	private HijackedHeadquarters.Instance target;

	[SerializeField]
	private KButton printButton;

	[SerializeField]
	private KButton interceptButton;

	[SerializeField]
	private LocText interceptStateLabel;

	[SerializeField]
	private GameObject[] progressIndicators;

	[SerializeField]
	private Image[] databankIcon;

	[SerializeField]
	private LocText databankCountLabel;

	[SerializeField]
	private GameObject meterSection;

	[SerializeField]
	private GameObject lockedSection;

	public override bool IsValidForTarget(GameObject target)
	{
		HijackedHeadquarters.Instance sMI = target.GetSMI<HijackedHeadquarters.Instance>();
		return sMI?.IsInsideState(sMI.sm.operational) ?? false;
	}

	public override int GetSideScreenSortOrder()
	{
		return 0;
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
	}

	public override void ScreenUpdate(bool topLevel)
	{
		base.ScreenUpdate(topLevel);
		RefreshDisplay();
	}

	private void RefreshDisplay()
	{
		HijackedHeadquarters.Instance sMI = target.GetSMI<HijackedHeadquarters.Instance>();
		interceptStateLabel.text = string.Format(UI.UISIDESCREENS.PRINTERCEPTORSIDESCREEN.INTERCEPT_METER, sMI.sm.interceptCharges.Get(sMI), 3);
		bool flag = sMI.sm.passcodeUnlocked.Get(sMI) && Immigration.Instance.ImmigrantsAvailable && sMI.sm.interceptCharges.Get(sMI) < 3;
		bool flag2 = target.IsInsideState(target.sm.operational.readyToPrint.pre) || target.IsInsideState(target.sm.operational.readyToPrint.loop);
		interceptButton.isInteractable = flag;
		printButton.isInteractable = flag2;
		interceptButton.GetComponent<ToolTip>().SetSimpleTooltip(flag ? UI.UISIDESCREENS.PRINTERCEPTORSIDESCREEN.INTERCEPT_TOOLTIP : ((sMI.sm.interceptCharges.Get(sMI) >= 3) ? UI.UISIDESCREENS.PRINTERCEPTORSIDESCREEN.INTERCEPT_TOOLTIP_DISABLED_TOO_FULL : UI.UISIDESCREENS.PRINTERCEPTORSIDESCREEN.INTERCEPT_TOOLTIP_DISABLED));
		printButton.GetComponent<ToolTip>().SetSimpleTooltip(flag2 ? UI.UISIDESCREENS.PRINTERCEPTORSIDESCREEN.PRINT_TOOLTIP : UI.UISIDESCREENS.PRINTERCEPTORSIDESCREEN.PRINT_TOOLTIP_DISABLED);
		for (int i = 0; i < progressIndicators.Length; i++)
		{
			Image componentInChildren = progressIndicators[i].GetComponentInChildren<Image>();
			componentInChildren.sprite = Def.GetUISprite("Headquarters").first;
			componentInChildren.color = ((i < sMI.sm.interceptCharges.Get(sMI)) ? Color.white : Color.gray);
		}
		databankCountLabel.SetText(GameUtil.SafeStringFormat(UI.UISIDESCREENS.PRINTERCEPTORSIDESCREEN.DATABANK_COUNT, target.GetComponent<Storage>().GetAmountAvailable(DatabankHelper.ID).ToString()));
		Image[] array = databankIcon;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].sprite = Def.GetUISprite(DatabankHelper.ID).first;
		}
		if (target.GetSMI<HijackedHeadquarters.Instance>().sm.passcodeUnlocked.Get(target))
		{
			lockedSection.SetActive(value: false);
			meterSection.SetActive(value: true);
		}
		else
		{
			lockedSection.SetActive(value: true);
			meterSection.SetActive(value: false);
		}
	}

	public override void SetTarget(GameObject new_target)
	{
		target = new_target.GetSMI<HijackedHeadquarters.Instance>();
		printButton.ClearOnClick();
		interceptButton.ClearOnClick();
		printButton.onClick += delegate
		{
			target.ActivatePrintInterface();
		};
		interceptButton.onClick += delegate
		{
			target.Intercept();
		};
		RefreshDisplay();
	}
}
