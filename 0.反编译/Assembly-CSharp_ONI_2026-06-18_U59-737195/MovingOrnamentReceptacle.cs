using UnityEngine;

public class MovingOrnamentReceptacle : OrnamentReceptacle, ISim1000ms
{
	[MyCmpReq]
	private SnapOn snapOn;

	private Navigator navigator;

	private KPrefabID prefabID;

	private KBatchedAnimTracker occupyingTracker;

	private KAnimLink animLink;

	private CavityInfo lastCavity;

	protected override void OnPrefabInit()
	{
		prefabID = GetComponent<KPrefabID>();
		base.OnPrefabInit();
		Subscribe(144050788, OnRoomUpdate);
		UpdateCavity();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		GetComponent<KBatchedAnimController>().SetSymbolVisiblity("snapTo_ornament", is_visible: false);
	}

	protected override void PositionOccupyingObject()
	{
		KBatchedAnimController component = base.occupyingObject.GetComponent<KBatchedAnimController>();
		component.transform.SetLocalPosition(new Vector3(0f, 0f, -0.1f));
		occupyingTracker = base.occupyingObject.AddComponent<KBatchedAnimTracker>();
		occupyingTracker.symbol = new HashedString("snapTo_ornament");
		occupyingTracker.forceAlwaysVisible = true;
		animLink = new KAnimLink(GetComponent<KBatchedAnimController>(), component);
	}

	protected override void ClearOccupant()
	{
		if (occupyingTracker != null)
		{
			Object.Destroy(occupyingTracker);
			occupyingTracker = null;
		}
		if (animLink != null)
		{
			animLink.Unregister();
			animLink = null;
		}
		base.ClearOccupant();
	}

	public void Sim1000ms(float dt)
	{
		UpdateCavity();
	}

	private void OnRoomUpdate(object roomInfo)
	{
		if (roomInfo == null)
		{
			UpdateCavity();
		}
	}

	protected override void OnCleanUp()
	{
		base.OnCleanUp();
		Unsubscribe(144050788, OnRoomUpdate);
		UnregisterFromLastCavity();
	}

	public void UpdateCavity()
	{
		int cell = Grid.PosToCell(base.gameObject);
		CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(cell);
		if (lastCavity != cavityForCell)
		{
			UnregisterFromLastCavity();
			if (cavityForCell != null)
			{
				cavityForCell.AddEntity(prefabID);
				Game.Instance.roomProber.UpdateRoom(cavityForCell);
			}
			lastCavity = cavityForCell;
		}
	}

	private void UnregisterFromLastCavity()
	{
		if (lastCavity != null)
		{
			lastCavity.RemoveFromCavity(prefabID, lastCavity.otherEntities);
			Game.Instance.roomProber.UpdateRoom(lastCavity);
		}
		lastCavity = null;
	}
}
