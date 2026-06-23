using KSerialization;

[SerializationConfig(MemberSerialization.OptIn)]
public class PartialLightBlocking : KMonoBehaviour
{
	private const byte PartialLightBlockingProperties = 48;

	protected override void OnSpawn()
	{
		SetLightBlocking();
		base.OnSpawn();
	}

	protected override void OnCleanUp()
	{
		ClearLightBlocking();
		base.OnCleanUp();
	}

	public void SetLightBlocking()
	{
		Building component = GetComponent<Building>();
		int[] placementCells = component.PlacementCells;
		foreach (int gameCell in placementCells)
		{
			SimMessages.SetCellProperties(gameCell, 48);
		}
	}

	public void ClearLightBlocking()
	{
		Building component = GetComponent<Building>();
		int[] placementCells = component.PlacementCells;
		foreach (int gameCell in placementCells)
		{
			SimMessages.ClearCellProperties(gameCell, 48);
		}
	}
}
