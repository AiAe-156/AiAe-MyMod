using UnityEngine;

public class TelescopeTargetConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "TelescopeTarget";

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
		GameObject gameObject = EntityTemplates.CreateEntity("TelescopeTarget", "TelescopeTarget");
		gameObject.AddOrGet<TelescopeTarget>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
