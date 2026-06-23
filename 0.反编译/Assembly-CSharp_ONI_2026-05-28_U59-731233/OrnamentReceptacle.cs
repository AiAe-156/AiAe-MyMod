using UnityEngine;

public class OrnamentReceptacle : SingleEntityReceptacle
{
	protected StatusItem ornamentDisabledStatusItem;

	protected StatusItem noItemDisplayedStatusItem;

	private bool refreshAnims = false;

	public bool IsHoldingOrnament => base.Occupant != null && base.Occupant.HasTag(GameTags.Ornament);

	public bool IsOperational => operational == null || operational.IsOperational;

	protected override void OnPrefabInit()
	{
		ornamentDisabledStatusItem = Db.Get().BuildingStatusItems.OrnamentDisabled;
		noItemDisplayedStatusItem = Db.Get().BuildingStatusItems.PedestalNoItemDisplayed;
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		SetReceptacleDirection(ReceptacleDirection.Any);
		AddAdditionalCriteria((GameObject obj) => obj.HasTag(GameTags.PedestalDisplayable));
		if (base.occupyingObject == null && storage.MassStored() > 0f)
		{
			OnDepositObject(storage.items[0]);
		}
		else
		{
			RefreshDecorTag();
		}
	}

	protected override void ClearOccupant()
	{
		base.ClearOccupant();
		RefreshDecorTag();
		int cell = Grid.PosToCell(base.gameObject);
		Game.Instance.roomProber.TriggerBuildingChangedEvent(cell, base.gameObject);
	}

	protected override void OnDepositObject(GameObject depositedObject)
	{
		base.OnDepositObject(depositedObject);
		RefreshDecorTag();
		int cell = Grid.PosToCell(base.gameObject);
		Game.Instance.roomProber.TriggerBuildingChangedEvent(cell, base.gameObject);
	}

	protected override void OnOperationalChanged(object data)
	{
		base.OnOperationalChanged(data);
		RefreshDecorTag();
		int cell = Grid.PosToCell(base.gameObject);
		Game.Instance.roomProber.TriggerBuildingChangedEvent(cell, base.gameObject);
		UpdateStatusItem();
	}

	protected override void PositionOccupyingObject()
	{
		base.PositionOccupyingObject();
		refreshAnims = true;
	}

	public override void Render1000ms(float dt)
	{
		base.Render1000ms(dt);
		if (refreshAnims)
		{
			if (base.Occupant != null)
			{
				KBatchedAnimController component = base.occupyingObject.GetComponent<KBatchedAnimController>();
				component.enabled = false;
				component.enabled = true;
			}
			KBatchedAnimController component2 = GetComponent<KBatchedAnimController>();
			component2.enabled = false;
			component2.enabled = true;
			refreshAnims = false;
		}
	}

	protected override void UpdateStatusItem(KSelectable selectable)
	{
		base.UpdateStatusItem(selectable);
		if (operational != null && IsHoldingOrnament && !operational.IsOperational)
		{
			selectable.AddStatusItem(ornamentDisabledStatusItem);
		}
		else
		{
			selectable.RemoveStatusItem(ornamentDisabledStatusItem);
		}
		if (base.Occupant == null && (operational == null || operational.IsOperational))
		{
			selectable.AddStatusItem(noItemDisplayedStatusItem);
		}
		else
		{
			selectable.RemoveStatusItem(noItemDisplayedStatusItem);
		}
	}

	public virtual void RefreshDecorTag()
	{
		KPrefabID component = base.gameObject.GetComponent<KPrefabID>();
		bool flag = component.HasTag(GameTags.Decoration);
		bool flag2 = base.Occupant != null && (operational == null || operational.IsOperational);
		if (flag2)
		{
			component.AddTag(GameTags.Decoration);
		}
		else
		{
			component.RemoveTag(GameTags.Decoration);
		}
		if (flag != flag2)
		{
			Game.Instance.roomProber.TriggerBuildingChangedEvent(Grid.PosToCell(base.gameObject), component);
		}
	}
}
