using UnityEngine;

public class TemporalTearConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "TemporalTear";

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
		GameObject gameObject = EntityTemplates.CreateEntity("TemporalTear", "TemporalTear");
		gameObject.AddOrGet<SaveLoadRoot>();
		gameObject.AddOrGet<TemporalTear>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
