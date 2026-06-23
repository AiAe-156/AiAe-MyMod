using UnityEngine;

public class IdleCellSensor : Sensor
{
	private MinionBrain brain;

	private Navigator navigator;

	private SwimMonitor.Instance swimMonitor;

	private KPrefabID prefabid;

	private int cell;

	private bool canSwim;

	public IdleCellSensor(Sensors sensors)
		: base(sensors)
	{
		navigator = GetComponent<Navigator>();
		brain = GetComponent<MinionBrain>();
		prefabid = GetComponent<KPrefabID>();
		brain.Subscribe(1589886948, OnMinionSpawned);
	}

	private void OnMinionSpawned(object obj)
	{
		swimMonitor = brain.GetSMI<SwimMonitor.Instance>();
		canSwim = swimMonitor != null && swimMonitor.CanSwim();
		brain.Unsubscribe(1589886948);
	}

	public override void Update()
	{
		if (!prefabid.HasTag(GameTags.Idle))
		{
			cell = Grid.InvalidCell;
			return;
		}
		canSwim = swimMonitor != null && swimMonitor.CanSwim();
		MinionPathFinderAbilities minionPathFinderAbilities = (MinionPathFinderAbilities)navigator.GetCurrentAbilities();
		minionPathFinderAbilities.SetIdleNavMaskEnabled(enabled: true);
		IdleCellQuery idleCellQuery = PathFinderQueries.idleCellQuery.Reset(brain, Random.Range(30, 60), canSwim);
		navigator.RunQuery(idleCellQuery);
		minionPathFinderAbilities.SetIdleNavMaskEnabled(enabled: false);
		cell = idleCellQuery.GetResultCell();
	}

	public int GetCell()
	{
		return cell;
	}
}
