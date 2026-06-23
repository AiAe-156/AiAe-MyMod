using STRINGS;
using UnityEngine;

[EntityConfigOrder(4)]
public class BabyCrabWoodConfig : IEntityConfig
{
	public const string ID = "CrabWoodBaby";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = CrabWoodConfig.CreateCrabWood("CrabWoodBaby", CREATURES.SPECIES.CRAB.VARIANT_WOOD.BABY.NAME, CREATURES.SPECIES.CRAB.VARIANT_WOOD.BABY.DESC, "baby_pincher_kanim", is_baby: true, new string[2] { "CrabWoodShell", "ShellfishMeat" }, new float[2] { 50f, 1.2f });
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "CrabWood", "CrabWoodShell");
		gameObject.AddOrGetDef<BabyMonitor.Def>().onGrowDropUnits = 50f;
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
