public class FakeFloorAdder : KMonoBehaviour
{
	public CellOffset[] floorOffsets;

	public bool initiallyActive = true;

	private bool isActive = false;

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (initiallyActive)
		{
			SetFloor(active: true);
		}
	}

	public void SetFloor(bool active)
	{
		if (isActive == active)
		{
			return;
		}
		int cell = Grid.PosToCell(this);
		Rotatable component = GetComponent<Rotatable>();
		CellOffset[] array = floorOffsets;
		foreach (CellOffset cellOffset in array)
		{
			CellOffset offset = ((component == null) ? cellOffset : component.GetRotatedCellOffset(cellOffset));
			int num = Grid.OffsetCell(cell, offset);
			if (active)
			{
				Grid.FakeFloor.Add(num);
			}
			else
			{
				Grid.FakeFloor.Remove(num);
			}
			Pathfinding.Instance.AddDirtyNavGridCell(num);
		}
		isActive = active;
	}

	protected override void OnCleanUp()
	{
		SetFloor(active: false);
		base.OnCleanUp();
	}
}
