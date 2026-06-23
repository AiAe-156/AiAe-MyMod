using UnityEngine;

public class ResearchDestinationConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "ResearchDestination";

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
		GameObject gameObject = EntityTemplates.CreateEntity("ResearchDestination", "ResearchDestination");
		gameObject.AddOrGet<SaveLoadRoot>();
		gameObject.AddOrGet<ResearchDestination>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
