using STRINGS;
using UnityEngine;

[EntityConfigOrder(4)]
public class BabyCrabFreshWaterConfig : IEntityConfig
{
	public const string ID = "CrabFreshWaterBaby";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = CrabFreshWaterConfig.CreateCrabFreshWater("CrabFreshWaterBaby", CREATURES.SPECIES.CRAB.VARIANT_FRESH_WATER.BABY.NAME, CREATURES.SPECIES.CRAB.VARIANT_FRESH_WATER.BABY.DESC, "baby_pincher_kanim", is_baby: true, "ShellfishMeat", 4);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "CrabFreshWater");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
