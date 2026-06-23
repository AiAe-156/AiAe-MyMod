using System.Collections.Generic;
using System.Linq;
using STRINGS;
using TUNING;

public class MeterScreen_Rations : MeterScreen_ValueTrackerDisplayer
{
	private long cachedCalories = -1L;

	private Dictionary<string, float> rationsDict = new Dictionary<string, float>();

	protected override string OnTooltip()
	{
		rationsDict.Clear();
		float calories = WorldResourceAmountTracker<RationTracker>.Get().CountAmount(rationsDict, ClusterManager.Instance.activeWorld.worldInventory);
		Label.text = GameUtil.GetFormattedCalories(calories);
		Tooltip.ClearMultiStringTooltip();
		Tooltip.AddMultiStringTooltip(string.Format(UI.TOOLTIPS.METERSCREEN_MEALHISTORY, GameUtil.GetFormattedCalories(calories), GameUtil.GetFormattedCalories((0f - MinionIdentity.GetCalorieBurnMultiplier()) * DUPLICANTSTATS.STANDARD.BaseStats.CALORIES_BURNED_PER_CYCLE)), ToolTipStyle_Header);
		Tooltip.AddMultiStringTooltip("", ToolTipStyle_Property);
		IOrderedEnumerable<KeyValuePair<string, float>> source = rationsDict.OrderByDescending(delegate(KeyValuePair<string, float> x)
		{
			EdiblesManager.FoodInfo foodInfo2 = EdiblesManager.GetFoodInfo(x.Key);
			return x.Value * (foodInfo2?.CaloriesPerUnit ?? (-1f));
		});
		Dictionary<string, float> dictionary = source.ToDictionary((KeyValuePair<string, float> t) => t.Key, (KeyValuePair<string, float> t) => t.Value);
		foreach (KeyValuePair<string, float> item in dictionary)
		{
			EdiblesManager.FoodInfo foodInfo = EdiblesManager.GetFoodInfo(item.Key);
			Tooltip.AddMultiStringTooltip((foodInfo != null) ? $"{foodInfo.Name}: {GameUtil.GetFormattedCalories(item.Value * foodInfo.CaloriesPerUnit)}" : string.Format(UI.TOOLTIPS.METERSCREEN_INVALID_FOOD_TYPE, item.Key), ToolTipStyle_Property);
		}
		return "";
	}

	protected override void InternalRefresh()
	{
		if (Label != null && WorldResourceAmountTracker<RationTracker>.Get() != null)
		{
			long num = (long)WorldResourceAmountTracker<RationTracker>.Get().CountAmount(null, ClusterManager.Instance.activeWorld.worldInventory);
			if (cachedCalories != num)
			{
				Label.text = GameUtil.GetFormattedCalories(num);
				cachedCalories = num;
			}
		}
		diagnosticGraph.GetComponentInChildren<SparkLayer>().SetColor(((float)cachedCalories > (float)GetWorldMinionIdentities().Count * FOOD.FOOD_CALORIES_PER_CYCLE) ? Constants.NEUTRAL_COLOR : Constants.NEGATIVE_COLOR);
		diagnosticGraph.GetComponentInChildren<LineLayer>().RefreshLine(TrackerTool.Instance.GetWorldTracker<KCalTracker>(ClusterManager.Instance.activeWorldId).ChartableData(600f), "kcal");
	}
}
