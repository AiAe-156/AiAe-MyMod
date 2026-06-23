using STRINGS;
using UnityEngine;

[EntityConfigOrder(2)]
public class BabyDivergentBeetleConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "DivergentBeetleBaby";

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
		GameObject gameObject = DivergentBeetleConfig.CreateDivergentBeetle("DivergentBeetleBaby", CREATURES.SPECIES.DIVERGENT.VARIANT_BEETLE.BABY.NAME, CREATURES.SPECIES.DIVERGENT.VARIANT_BEETLE.BABY.DESC, "baby_critter_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "DivergentBeetle");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
