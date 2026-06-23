public class WorkingToiletTracker : WorldTracker
{
	public WorkingToiletTracker(int worldID)
		: base(worldID)
	{
	}

	public override void UpdateData()
	{
		int num = 0;
		foreach (IUsable item in Components.Toilets.WorldItemsEnumerate(base.WorldID, checkChildWorlds: true))
		{
			if (item.IsUsable())
			{
				num++;
			}
		}
		AddPoint(num);
	}

	public override string FormatValueString(float value)
	{
		return value.ToString();
	}
}
