using System.Collections.Generic;
using Klei.AI;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MeterScreen_VTD_DuplicantIterator : MeterScreen_ValueTrackerDisplayer
{
	protected int lastSelectedDuplicantIndex = -1;

	protected virtual void UpdateDisplayInfo(BaseEventData base_ev_data, IList<MinionIdentity> minions)
	{
		if (!(base_ev_data is PointerEventData pointerEventData))
		{
			return;
		}
		List<MinionIdentity> worldMinionIdentities = GetWorldMinionIdentities();
		switch (pointerEventData.button)
		{
		case PointerEventData.InputButton.Left:
			if (worldMinionIdentities.Count < lastSelectedDuplicantIndex)
			{
				lastSelectedDuplicantIndex = -1;
			}
			if (worldMinionIdentities.Count > 0)
			{
				lastSelectedDuplicantIndex = (lastSelectedDuplicantIndex + 1) % worldMinionIdentities.Count;
				MinionIdentity minionIdentity = minions[lastSelectedDuplicantIndex];
				SelectTool.Instance.SelectAndFocus(minionIdentity.transform.GetPosition(), minionIdentity.GetComponent<KSelectable>(), Vector3.zero);
			}
			break;
		case PointerEventData.InputButton.Right:
			lastSelectedDuplicantIndex = -1;
			break;
		}
	}

	public override void OnClick(BaseEventData base_ev_data)
	{
		List<MinionIdentity> worldMinionIdentities = GetWorldMinionIdentities();
		UpdateDisplayInfo(base_ev_data, worldMinionIdentities);
		OnTooltip();
		Tooltip.forceRefresh = true;
	}

	protected void AddToolTipLine(string str, bool selected)
	{
		if (selected)
		{
			Tooltip.AddMultiStringTooltip("<color=#F0B310FF>" + str + "</color>", ToolTipStyle_Property);
		}
		else
		{
			Tooltip.AddMultiStringTooltip(str, ToolTipStyle_Property);
		}
	}

	protected void AddToolTipAmountPercentLine(AmountInstance amount, MinionIdentity id, bool selected)
	{
		string str = id.GetComponent<KSelectable>().GetName() + ":  " + Mathf.Round(amount.value) + "%";
		AddToolTipLine(str, selected);
	}
}
