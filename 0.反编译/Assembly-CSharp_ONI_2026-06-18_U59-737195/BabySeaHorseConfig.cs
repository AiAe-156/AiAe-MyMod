using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabySeaHorseConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SeaHorseBaby";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = SeaHorseConfig.CreateSeaHorse("SeaHorseBaby", CREATURES.SPECIES.SEAHORSE.BABY.NAME, CREATURES.SPECIES.SEAHORSE.BABY.DESC, "baby_seahorse_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "SeaHorse");
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
