using UnityEngine;

public class SimpleFXConfig : IEntityConfig
{
	public static readonly string ID = "SimpleFX";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateEntity(ID, ID, is_selectable: false);
		gameObject.AddOrGet<KBatchedAnimController>();
		return gameObject;
	}

	public void OnPrefabInit(GameObject go)
	{
	}

	public void OnSpawn(GameObject go)
	{
	}
}
