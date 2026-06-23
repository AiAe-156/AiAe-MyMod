using System.Collections.Generic;
using Klei.AI;
using STRINGS;

public class MeterScreen_Sickness : MeterScreen_VTD_DuplicantIterator
{
	protected override void InternalRefresh()
	{
		List<MinionIdentity> worldMinionIdentities = GetWorldMinionIdentities();
		int num = CountSickDupes(worldMinionIdentities);
		Label.text = num.ToString();
	}

	protected override string OnTooltip()
	{
		List<MinionIdentity> worldMinionIdentities = GetWorldMinionIdentities();
		int num = CountSickDupes(worldMinionIdentities);
		Tooltip.ClearMultiStringTooltip();
		Tooltip.AddMultiStringTooltip(string.Format(UI.TOOLTIPS.METERSCREEN_SICK_DUPES, num.ToString()), ToolTipStyle_Header);
		for (int i = 0; i < worldMinionIdentities.Count; i++)
		{
			MinionIdentity minionIdentity = worldMinionIdentities[i];
			if (minionIdentity.IsNullOrDestroyed())
			{
				continue;
			}
			string text = minionIdentity.GetComponent<KSelectable>().GetName();
			Sicknesses sicknesses = minionIdentity.GetComponent<MinionModifiers>().sicknesses;
			if (sicknesses.IsInfected())
			{
				text += " (";
				int num2 = 0;
				foreach (SicknessInstance modifier in sicknesses.ModifierList)
				{
					text = text + ((num2 > 0) ? ", " : "") + modifier.modifier.Name;
					num2++;
				}
				text += ")";
			}
			bool selected = i == lastSelectedDuplicantIndex;
			AddToolTipLine(text, selected);
		}
		return "";
	}

	private int CountSickDupes(List<MinionIdentity> minions)
	{
		int num = 0;
		foreach (MinionIdentity minion in minions)
		{
			if (!minion.IsNullOrDestroyed() && minion.GetComponent<MinionModifiers>().sicknesses.IsInfected())
			{
				num++;
			}
		}
		return num;
	}
}
