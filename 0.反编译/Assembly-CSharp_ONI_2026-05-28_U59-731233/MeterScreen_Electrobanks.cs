using System.Collections.Generic;
using System.Linq;
using STRINGS;
using UnityEngine;

public class MeterScreen_Electrobanks : MeterScreen_ValueTrackerDisplayer
{
	private long cachedJoules = -1L;

	private Dictionary<string, float> per_electrobankType_UnitCount_Dictionary = new Dictionary<string, float>();

	private float bionicJoulesPerCycle;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		Components.LiveMinionIdentities.OnAdd += OnNewMinionAdded;
		List<MinionIdentity> allMinionsFromAllWorlds = GetAllMinionsFromAllWorlds();
		SetVisibility(allMinionsFromAllWorlds != null && allMinionsFromAllWorlds.Find((MinionIdentity m) => m.model == BionicMinionConfig.MODEL) != null);
		bionicJoulesPerCycle = (BionicBatteryMonitor.GetDifficultyModifier().value + 200f) * 600f;
	}

	protected override void OnCleanUp()
	{
		Components.LiveMinionIdentities.OnAdd -= OnNewMinionAdded;
		base.OnCleanUp();
	}

	private void OnNewMinionAdded(MinionIdentity id)
	{
		if (id.model == BionicMinionConfig.MODEL)
		{
			SetVisibility(isVisible: true);
		}
	}

	public void SetVisibility(bool isVisible)
	{
		base.gameObject.SetActive(isVisible);
	}

	protected override string OnTooltip()
	{
		per_electrobankType_UnitCount_Dictionary.Clear();
		float totalUnitsFound = 0f;
		float joules = WorldResourceAmountTracker<ElectrobankTracker>.Get().CountAmount(per_electrobankType_UnitCount_Dictionary, out totalUnitsFound, ClusterManager.Instance.activeWorld.worldInventory, excludeUnreachable: true);
		string formattedJoules = GameUtil.GetFormattedJoules(joules);
		Label.text = formattedJoules;
		Tooltip.ClearMultiStringTooltip();
		Tooltip.AddMultiStringTooltip(string.Format(UI.TOOLTIPS.METERSCREEN_ELECTROBANK_JOULES, formattedJoules, GameUtil.GetFormattedJoules(bionicJoulesPerCycle), GameUtil.GetFormattedUnits((int)totalUnitsFound)), ToolTipStyle_Header);
		Tooltip.AddMultiStringTooltip("", ToolTipStyle_Property);
		IOrderedEnumerable<KeyValuePair<string, float>> source = per_electrobankType_UnitCount_Dictionary.OrderByDescending((KeyValuePair<string, float> x) => x.Value);
		Dictionary<string, float> dictionary = source.ToDictionary((KeyValuePair<string, float> t) => t.Key, (KeyValuePair<string, float> t) => t.Value);
		foreach (KeyValuePair<string, float> item in dictionary)
		{
			GameObject prefab = Assets.GetPrefab(item.Key);
			Tooltip.AddMultiStringTooltip((prefab != null) ? $"{prefab.GetProperName()} ({GameUtil.GetFormattedUnits((int)item.Value)}): {GameUtil.GetFormattedJoules(item.Value * 120000f)}" : string.Format(UI.TOOLTIPS.METERSCREEN_INVALID_ELECTROBANK_TYPE, item.Key), ToolTipStyle_Property);
		}
		return "";
	}

	protected override void InternalRefresh()
	{
		if (!Game.IsDlcActiveForCurrentSave("DLC3_ID"))
		{
			return;
		}
		if (Label != null && WorldResourceAmountTracker<ElectrobankTracker>.Get() != null)
		{
			float totalUnitsFound;
			long num = (long)WorldResourceAmountTracker<ElectrobankTracker>.Get().CountAmount(null, out totalUnitsFound, ClusterManager.Instance.activeWorld.worldInventory, excludeUnreachable: true);
			if (cachedJoules != num)
			{
				Label.text = GameUtil.GetFormattedJoules(num);
				cachedJoules = num;
			}
		}
		diagnosticGraph.GetComponentInChildren<SparkLayer>().SetColor(((float)cachedJoules > (float)GetWorldMinionIdentities().Count * 120000f) ? Constants.NEUTRAL_COLOR : Constants.NEGATIVE_COLOR);
		WorldTracker worldTracker = TrackerTool.Instance.GetWorldTracker<ElectrobankJoulesTracker>(ClusterManager.Instance.activeWorldId);
		if (worldTracker != null)
		{
			diagnosticGraph.GetComponentInChildren<LineLayer>().RefreshLine(worldTracker.ChartableData(600f), "joules");
		}
	}
}
