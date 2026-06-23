using UnityEngine;

public class ExplodingClusterShipConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "ExplodingClusterShip";

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateEntity("ExplodingClusterShip", "ExplodingClusterShip", is_selectable: false);
		ClusterFXEntity clusterFXEntity = gameObject.AddOrGet<ClusterFXEntity>();
		clusterFXEntity.kAnimName = "rocket_self_destruct_kanim";
		clusterFXEntity.animName = "explode";
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
