public class ImmuneToPressureDamage : KMonoBehaviour
{
	public CellOffset[] Cells = new CellOffset[1]
	{
		new CellOffset(0, 0)
	};

	protected override void OnPrefabInit()
	{
		CellOffset[] cells = Cells;
		foreach (CellOffset offset in cells)
		{
			int gameCell = Grid.OffsetCell(Grid.PosToCell(this), offset);
			SimMessages.SetCellProperties(gameCell, 8);
		}
		base.OnPrefabInit();
	}

	protected override void OnCleanUp()
	{
		CellOffset[] cells = Cells;
		foreach (CellOffset offset in cells)
		{
			int gameCell = Grid.OffsetCell(Grid.PosToCell(this), offset);
			SimMessages.ClearCellProperties(gameCell, 8);
		}
	}
}
