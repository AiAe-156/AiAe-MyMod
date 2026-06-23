using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabySnailIronConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SnailIronBaby";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = SnailIronConfig.CreateSnail("SnailIronBaby", CREATURES.SPECIES.SNAIL.VARIANT_IRON.BABY.NAME, CREATURES.SPECIES.SNAIL.VARIANT_IRON.BABY.DESC, "baby_snail_iron_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "SnailIron", null, force_adult_nav_type: true);
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
