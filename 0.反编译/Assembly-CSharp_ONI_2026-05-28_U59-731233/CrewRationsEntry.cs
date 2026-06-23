using Klei.AI;
using UnityEngine;

public class CrewRationsEntry : CrewListEntry
{
	public KButton incRationPerDayButton;

	public KButton decRationPerDayButton;

	public LocText rationPerDayText;

	public LocText rationsEatenToday;

	public LocText currentCaloriesText;

	public LocText currentStressText;

	public LocText currentHealthText;

	public ValueTrendImageToggle stressTrendImage;

	private RationMonitor.Instance rationMonitor;

	public override void Populate(MinionIdentity _identity)
	{
		base.Populate(_identity);
		rationMonitor = _identity.GetSMI<RationMonitor.Instance>();
		Refresh();
	}

	public override void Refresh()
	{
		base.Refresh();
		rationsEatenToday.text = GameUtil.GetFormattedCalories(rationMonitor.GetRationsAteToday());
		if (identity == null)
		{
			return;
		}
		Amounts amounts = identity.GetAmounts();
		foreach (AmountInstance modifier in amounts.ModifierList)
		{
			float min = modifier.GetMin();
			float max = modifier.GetMax();
			float num = max - min;
			float num2 = (num - (max - modifier.value)) / num;
			string text = Mathf.RoundToInt(num2 * 100f).ToString();
			if (modifier.amount == Db.Get().Amounts.Stress)
			{
				currentStressText.text = modifier.GetValueString();
				currentStressText.GetComponent<ToolTip>().toolTip = modifier.GetTooltip();
				stressTrendImage.SetValue(modifier);
			}
			else if (modifier.amount == Db.Get().Amounts.Calories)
			{
				currentCaloriesText.text = text + "%";
				currentCaloriesText.GetComponent<ToolTip>().toolTip = modifier.GetTooltip();
			}
			else if (modifier.amount == Db.Get().Amounts.HitPoints)
			{
				currentHealthText.text = text + "%";
				currentHealthText.GetComponent<ToolTip>().toolTip = modifier.GetTooltip();
			}
		}
	}
}
