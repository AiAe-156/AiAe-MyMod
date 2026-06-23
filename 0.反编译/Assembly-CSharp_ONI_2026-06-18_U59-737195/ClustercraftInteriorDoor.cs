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
		int[] occupiedGridCells = GetComponent<OccupyArea>().GetOccupiedGridCells();
		foreach (int i2 in occupiedGridCells)
		{
			Grid.HasDoor[i2] = false;
		}
		base.OnCleanUp();
	}
}
