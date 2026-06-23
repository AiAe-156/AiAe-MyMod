using UnityEngine;

public class LoreBearerSideScreen : SideScreenContent
{
	public const int DefaultButtonMenuSideScreenSortOrder = 20;

	public KButton button;

	private LoreBearer target;

	public override bool IsValidForTarget(GameObject target)
	{
		LoreBearer component = target.GetComponent<LoreBearer>();
		if (component != null && !component.hideLore)
		{
			if (!component.useDefaultLore)
			{
				return !component.poiOverrideLoreUnlockId.IsNullOrWhiteSpace();
			}
			return true;
		}
		return false;
	}

	public override int GetSideScreenSortOrder()
	{
		return target.GetSideScreenSortOrder();
	}

	public override void SetTarget(GameObject new_target)
	{
		if (new_target == null)
		{
			Debug.LogError("Invalid gameObject received");
			return;
		}
		target = new_target.GetComponent<LoreBearer>();
		Refresh();
	}

	private void Refresh()
	{
		button.isInteractable = target.SidescreenButtonInteractable();
		button.ClearOnClick();
		button.onClick += target.OnSidescreenButtonPressed;
		button.onClick += Refresh;
		button.GetComponentInChildren<LocText>().SetText(target.SidescreenButtonText);
		button.GetComponent<ToolTip>().SetSimpleTooltip(target.SidescreenButtonTooltip);
	}
}
