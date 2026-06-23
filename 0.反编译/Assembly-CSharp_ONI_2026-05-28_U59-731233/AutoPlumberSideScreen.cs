using UnityEngine;

public class AutoPlumberSideScreen : SideScreenContent
{
	public KButton activateButton;

	public KButton powerButton;

	public KButton pipesButton;

	public KButton solidsButton;

	public KButton minionButton;

	private Building building;

	protected override void OnSpawn()
	{
		activateButton.onClick += delegate
		{
			DevAutoPlumber.AutoPlumbBuilding(building);
		};
		powerButton.onClick += delegate
		{
			DevAutoPlumber.DoElectricalPlumbing(building);
		};
		pipesButton.onClick += delegate
		{
			DevAutoPlumber.DoLiquidAndGasPlumbing(building);
		};
		solidsButton.onClick += delegate
		{
			DevAutoPlumber.SetupSolidOreDelivery(building);
		};
		minionButton.onClick += delegate
		{
			SpawnMinion();
		};
	}

	private void SpawnMinion()
	{
		MinionStartingStats minionStartingStats = new MinionStartingStats(is_starter_minion: false, null, null, isDebugMinion: true);
		GameObject prefab = Assets.GetPrefab(BaseMinionConfig.GetMinionIDForModel(minionStartingStats.personality.model));
		GameObject gameObject = Util.KInstantiate(prefab);
		gameObject.name = prefab.name;
		Immigration.Instance.ApplyDefaultPersonalPriorities(gameObject);
		Vector3 position = Grid.CellToPos(Grid.PosToCell(building), CellAlignment.Bottom, Grid.SceneLayer.Move);
		gameObject.transform.SetLocalPosition(position);
		gameObject.SetActive(value: true);
		minionStartingStats.Apply(gameObject);
	}

	public override int GetSideScreenSortOrder()
	{
		return -150;
	}

	public override bool IsValidForTarget(GameObject target)
	{
		return DebugHandler.InstantBuildMode && target.GetComponent<Building>() != null;
	}

	public override void SetTarget(GameObject target)
	{
		building = target.GetComponent<Building>();
	}

	public override void ClearTarget()
	{
	}
}
