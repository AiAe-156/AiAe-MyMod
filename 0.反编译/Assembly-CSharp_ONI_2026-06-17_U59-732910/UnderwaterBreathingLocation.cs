using UnityEngine;

public class UnderwaterBreathingLocation : KMonoBehaviour
{
	[MyCmpGet]
	public Storage storage;

	[MyCmpAdd]
	private Reservable reservable;

	public int breathableCell { get; private set; }

	public float GetAvailableBreathableMass()
	{
		PrimaryElement primaryElement = storage.FindFirstWithMass(GameTags.Breathable);
		if (!(primaryElement != null))
		{
			return 0f;
		}
		return primaryElement.Mass;
	}

	public void MarkCells()
	{
		Components.UnderwaterBreathingLocations.Add(this);
		breathableCell = Grid.PosToCell(this);
	}

	public void UnmarkCells()
	{
		breathableCell = -1;
		Components.UnderwaterBreathingLocations.Remove(this);
	}

	public bool ReserveLocation(GameObject reserver, bool reserve)
	{
		bool result = false;
		if (reserve)
		{
			result = reservable.Reserve(reserver);
		}
		else if (reservable.IsReservableBy(reserver))
		{
			reservable.ClearReservation();
			result = true;
		}
		return result;
	}

	public bool CanReserve(GameObject reserver)
	{
		if (reservable.IsReserved)
		{
			return reservable.IsReservableBy(reserver);
		}
		return true;
	}
}
