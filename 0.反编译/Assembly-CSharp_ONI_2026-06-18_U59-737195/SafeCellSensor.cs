using System.Collections.Generic;
using Klei.AI;

public class SafeCellSensor : Sensor
{
	private MinionBrain brain;

	private Navigator navigator;

	private KPrefabID prefabid;

	private Traits traits;

	private int cell = Grid.InvalidCell;

	private Dictionary<string, SafeCellQuery.SafeFlags> ignoredFlagsSets = new Dictionary<string, SafeCellQuery.SafeFlags>();

	private SafeCellQuery.SafeFlags GetIgnoredFlags()
	{
		SafeCellQuery.SafeFlags safeFlags = (SafeCellQuery.SafeFlags)0;
		foreach (string key in ignoredFlagsSets.Keys)
		{
			SafeCellQuery.SafeFlags safeFlags2 = ignoredFlagsSets[key];
			safeFlags |= safeFlags2;
		}
		return safeFlags;
	}

	public void AddIgnoredFlagsSet(string setID, SafeCellQuery.SafeFlags flagsToIgnore)
	{
		if (ignoredFlagsSets.ContainsKey(setID))
		{
			ignoredFlagsSets[setID] = flagsToIgnore;
		}
		else
		{
			ignoredFlagsSets.Add(setID, flagsToIgnore);
		}
	}

	public void RemoveIgnoredFlagsSet(string setID)
	{
		if (ignoredFlagsSets.ContainsKey(setID))
		{
			ignoredFlagsSets.Remove(setID);
		}
	}

	public SafeCellSensor(Sensors sensors, bool startEnabled = true)
		: base(sensors, startEnabled)
	{
		navigator = GetComponent<Navigator>();
		brain = GetComponent<MinionBrain>();
		prefabid = GetComponent<KPrefabID>();
		traits = GetComponent<Traits>();
	}

	public override void Update()
	{
		if (!prefabid.HasTag(GameTags.Idle))
		{
			cell = Grid.InvalidCell;
			return;
		}
		bool flag = HasSafeCell();
		RunSafeCellQuery(avoid_light: false);
		bool flag2 = HasSafeCell();
		if (flag2 != flag)
		{
			if (flag2)
			{
				sensors.Trigger(982561777);
			}
			else
			{
				sensors.Trigger(506919987);
			}
		}
	}

	public void RunSafeCellQuery(bool avoid_light)
	{
		cell = RunAndGetSafeCellQueryResult(avoid_light);
		if (cell == Grid.PosToCell(navigator))
		{
			cell = Grid.InvalidCell;
		}
	}

	public int RunAndGetSafeCellQueryResult(bool avoid_light)
	{
		MinionPathFinderAbilities obj = (MinionPathFinderAbilities)navigator.GetCurrentAbilities();
		obj.SetIdleNavMaskEnabled(enabled: true);
		PathFinderQuery pathFinderQuery = PathFinderQueries.safeCellQuery.Reset(brain, avoid_light, GetIgnoredFlags());
		navigator.RunQuery(pathFinderQuery);
		obj.SetIdleNavMaskEnabled(enabled: false);
		cell = pathFinderQuery.GetResultCell();
		return cell;
	}

	public int GetSensorCell()
	{
		return cell;
	}

	public int GetCellQuery()
	{
		if (cell == Grid.InvalidCell)
		{
			RunSafeCellQuery(avoid_light: false);
		}
		return cell;
	}

	public int GetSleepCellQuery()
	{
		if (cell == Grid.InvalidCell)
		{
			RunSafeCellQuery(!traits.HasTrait("NightLight"));
		}
		return cell;
	}

	public bool HasSafeCell()
	{
		if (cell != Grid.InvalidCell)
		{
			return cell != Grid.PosToCell(sensors);
		}
		return false;
	}
}
