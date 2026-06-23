using UnityEngine;

public class OrbitalBGConfig : IEntityConfig
{
	public static string ID = "OrbitalBG";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateEntity(ID, ID, is_selectable: false);
		gameObject.AddOrGet<LoopingSounds>();
		gameObject.AddOrGet<OrbitalObject>();
		gameObject.AddOrGet<SaveLoadRoot>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject go)
	{
	}

	public void OnSpawn(GameObject go)
	{
	}
}
