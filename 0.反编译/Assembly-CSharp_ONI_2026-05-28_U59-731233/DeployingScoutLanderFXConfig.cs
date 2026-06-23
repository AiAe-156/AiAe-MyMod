using UnityEngine;

public class DeployingScoutLanderFXConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "DeployingScoutLanderFXConfig";

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
		GameObject gameObject = EntityTemplates.CreateEntity("DeployingScoutLanderFXConfig", "DeployingScoutLanderFXConfig", is_selectable: false);
		ClusterFXEntity clusterFXEntity = gameObject.AddOrGet<ClusterFXEntity>();
		clusterFXEntity.kAnimName = "rover01_kanim";
		clusterFXEntity.animName = "landing";
		clusterFXEntity.animPlayMode = KAnim.PlayMode.Loop;
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
