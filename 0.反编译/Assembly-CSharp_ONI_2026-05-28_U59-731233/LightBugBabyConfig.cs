using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class LightBugBabyConfig : IEntityConfig
{
	public const string ID = "LightBugBaby";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = LightBugConfig.CreateLightBug("LightBugBaby", CREATURES.SPECIES.LIGHTBUG.BABY.NAME, CREATURES.SPECIES.LIGHTBUG.BABY.DESC, "baby_lightbug_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "LightBug");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
