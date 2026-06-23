using UnityEngine;

[AddComponentMenu("KMonoBehaviour/scripts/Reservable")]
public class Reservable : KMonoBehaviour
{
	private GameObject reservedBy;

	public GameObject ReservedBy => reservedBy;

	public bool IsReserved => reservedBy != null;

	public bool Reserve(GameObject reserver)
	{
		if (reservedBy == null)
		{
			reservedBy = reserver;
			return true;
		}
		return false;
	}

	public void ClearReservation()
	{
		reservedBy = null;
	}

	public bool IsReservableBy(GameObject reserver)
	{
		if (!(reservedBy == null))
		{
			return reservedBy == reserver;
		}
		return true;
	}
}
