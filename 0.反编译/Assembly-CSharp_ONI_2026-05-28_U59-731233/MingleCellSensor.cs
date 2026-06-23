using UnityEngine;

public class MingleCellSensor : Sensor
{
	private MinionBrain brain;

	private Navigator navigator;

	private Schedulable scheduable;

	private int cell;

	public MingleCellSensor(Sensors sensors)
		: base(sensors)
	{
		navigator = GetComponent<Navigator>();
		brain = GetComponent<MinionBrain>();
		scheduable = GetComponent<Schedulable>();
	}

	public bool IsAllowed()
	{
		return ScheduleManager.Instance.IsAllowed(scheduable, Db.Get().ScheduleBlockTypes.Recreation);
	}

	public override void Update()
	{
		if (!IsAllowed())
		{
			return;
		}
		cell = Grid.InvalidCell;
		int num = int.MaxValue;
		ListPool<int, MingleCellSensor>.PooledList pooledList = ListPool<int, MingleCellSensor>.Allocate();
		int num2 = 50;
		foreach (int mingleCell in Game.Instance.mingleCellTracker.mingleCells)
		{
			if (!brain.IsCellClear(mingleCell))
			{
				continue;
			}
			int navigationCost = navigator.GetNavigationCost(mingleCell);
			if (navigationCost != -1)
			{
				if (mingleCell == Grid.InvalidCell || navigationCost < num)
				{
					cell = mingleCell;
					num = navigationCost;
				}
				if (navigationCost < num2)
				{
					pooledList.Add(mingleCell);
				}
			}
		}
		if (pooledList.Count > 0)
		{
			cell = pooledList[Random.Range(0, pooledList.Count)];
		}
		pooledList.Recycle();
	}

	public int GetCell()
	{
		return cell;
	}
}
