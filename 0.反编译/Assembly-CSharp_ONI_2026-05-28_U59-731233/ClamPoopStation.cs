using STRINGS;
using UnityEngine;

public class ClamPoopStation : KMonoBehaviour, IPoopStation
{
	private static Tag[] ALLOWED_USERS_IDS = new Tag[3] { "CrabFreshWater", "Crab", "CrabWood" };

	private ReceptacleMonitor receptacleMonitor;

	private Harvestable harvestable;

	private GameObject poopUser;

	public bool IsWild => !receptacleMonitor.Replanted;

	public bool IsOnPlanterBox => !IsWild && receptacleMonitor.smi.ReceptacleObject != null && receptacleMonitor.smi.ReceptacleObject is PlantablePlot && (receptacleMonitor.smi.ReceptacleObject as PlantablePlot).IsOffGround;

	protected override void OnPrefabInit()
	{
		receptacleMonitor = GetComponent<ReceptacleMonitor>();
		harvestable = GetComponent<Harvestable>();
		base.OnPrefabInit();
	}

	protected override void OnSpawn()
	{
		RegisterPoopStation();
		base.OnSpawn();
	}

	protected override void OnCleanUp()
	{
		UnregisterPoopStation();
		base.OnCleanUp();
	}

	public bool IsUserCompatibleWithPoopStation(KPrefabID userPrefabID)
	{
		return userPrefabID.HasAnyTags(ALLOWED_USERS_IDS);
	}

	public GameObject GetPoopStationObject()
	{
		return base.gameObject;
	}

	public GameObject GetCurrentPoopStationUser()
	{
		return poopUser;
	}

	public bool IsPoopStationOperational()
	{
		return harvestable == null || !harvestable.CanBeHarvested;
	}

	public string[] GetPoopingAnimNames()
	{
		return null;
	}

	public void RegisterPoopStation()
	{
		Components.PoopStations.Add(base.gameObject.GetMyWorldId(), this);
	}

	public void UnregisterPoopStation()
	{
		Components.PoopStations.Remove(base.gameObject.GetMyWorldId(), this);
	}

	public PoopData GetPoopData()
	{
		return IsWild ? new PoopData(skipSpawningPoop: true, null, CREATURES.POOP.PLANT_POOP_STATION_WILD, Def.GetUISprite(base.gameObject).first) : new PoopData(skipSpawningPoop: false, receptacleMonitor.smi.ReceptacleObject.GetComponent<Storage>(), CREATURES.POOP.PLANT_POOP_STATION_WILD, Def.GetUISprite(base.gameObject).first);
	}

	public void PlayPoopStationAnim(string animName, KAnim.PlayMode playMode)
	{
	}

	public void ClearPoopStationUser(GameObject userRequestingClearing)
	{
		if (poopUser == userRequestingClearing)
		{
			poopUser = null;
			Trigger(-984476291);
		}
	}

	public bool AttemptToReservePoopStation(GameObject userRequestingReserve)
	{
		if (poopUser != null && poopUser != userRequestingReserve)
		{
			return false;
		}
		poopUser = userRequestingReserve;
		return true;
	}
}
