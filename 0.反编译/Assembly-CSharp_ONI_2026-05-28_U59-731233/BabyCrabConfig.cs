using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabyCrabConfig : IEntityConfig
{
	public const string ID = "CrabBaby";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = CrabConfig.CreateCrab("CrabBaby", CREATURES.SPECIES.CRAB.BABY.NAME, CREATURES.SPECIES.CRAB.BABY.DESC, "baby_pincher_kanim", is_baby: true, new string[2] { "CrabShell", "ShellfishMeat" }, new float[2] { 30f, 1.2f });
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "Crab", "CrabShell");
		gameObject.AddOrGetDef<BabyMonitor.Def>().onGrowDropUnits = 30f;
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
