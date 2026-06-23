public class ClustercraftInteriorDoor : KMonoBehaviour
{
	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		Components.ClusterCraftInteriorDoors.Add(this);
	}

	protected override void OnCleanUp()
	{
		Components.ClusterCraftInteriorDoors.Remove(this);
		OccupyArea component = GetComponent<OccupyArea>();
		int[] occupiedGridCells = component.GetOccupiedGridCells();
		foreach (int i2 in occupiedGridCells)
		{
			Grid.HasDoor[i2] = false;
		}
		base.OnCleanUp();
	}
}
