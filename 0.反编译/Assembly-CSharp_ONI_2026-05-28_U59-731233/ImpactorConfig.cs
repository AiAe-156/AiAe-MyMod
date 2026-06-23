using UnityEngine;

public class ImpactorConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Impactor";

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC4;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateEntity("Impactor", "ImpactorInstance", is_selectable: false);
		gameObject.AddOrGet<ParallaxBackgroundObject>();
		gameObject.AddOrGet<SaveLoadRoot>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
