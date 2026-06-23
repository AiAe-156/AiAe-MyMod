using System.Collections.Generic;
using System.Linq;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

public class MeterScreen_Stress : MeterScreen_VTD_DuplicantIterator
{
	protected override void OnSpawn()
	{
		minionListCustomSortOperation = SortByStressLevel;
		base.OnSpawn();
	}

	private List<MinionIdentity> SortByStressLevel(List<MinionIdentity> minions)
	{
		Amount stress_amount = Db.Get().Amounts.Stress;
		return minions.OrderByDescending((MinionIdentity x) => stress_amount.Lookup(x).value).ToList();
	}

	protected override string OnTooltip()
	{
		float maxStressInActiveWorld = GameUtil.GetMaxStressInActiveWorld();
		Tooltip.ClearMultiStringTooltip();
		Tooltip.AddMultiStringTooltip(string.Format(UI.TOOLTIPS.METERSCREEN_AVGSTRESS, Mathf.Round(maxStressInActiveWorld) + "%"), ToolTipStyle_Header);
		Amount stress = Db.Get().Amounts.Stress;
		List<MinionIdentity> worldMinionIdentities = GetWorldMinionIdentities();
		bool flag = lastSelectedDuplicantIndex >= 0 && lastSelectedDuplicantIndex < worldMinionIdentities.Count;
		for (int i = 0; i < worldMinionIdentities.Count; i++)
		{
			MinionIdentity minionIdentity = worldMinionIdentities[i];
			AmountInstance amount = stress.Lookup(minionIdentity);
			AddToolTipAmountPercentLine(amount, minionIdentity, flag && worldMinionIdentities[lastSelectedDuplicantIndex] == minionIdentity);
		}
		return "";
	}

	protected override void InternalRefresh()
	{
		float maxStressInActiveWorld = GameUtil.GetMaxStressInActiveWorld();
		Label.text = Mathf.Round(maxStressInActiveWorld).ToString();
		WorldTracker worldTracker = TrackerTool.Instance.GetWorldTracker<StressTracker>(ClusterManager.Instance.activeWorldId);
		diagnosticGraph.GetComponentInChildren<SparkLayer>().SetColor((worldTracker.GetCurrentValue() >= STRESS.ACTING_OUT_RESET) ? Constants.NEGATIVE_COLOR : Constants.NEUTRAL_COLOR);
		diagnosticGraph.GetComponentInChildren<LineLayer>().RefreshLine(worldTracker.ChartableData(600f), "stressData");
	}
}
