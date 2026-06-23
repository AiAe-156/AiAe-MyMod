using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabySnailConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SnailBaby";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = SnailConfig.CreateSnail("SnailBaby", CREATURES.SPECIES.SNAIL.BABY.NAME, CREATURES.SPECIES.SNAIL.BABY.DESC, "baby_snail_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "Snail", null, force_adult_nav_type: true);
		return gameObject;
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
