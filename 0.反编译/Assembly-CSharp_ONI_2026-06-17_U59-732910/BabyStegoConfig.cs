using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabyStegoConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "StegoBaby";

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC4;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = StegoConfig.CreateStego("StegoBaby", CREATURES.SPECIES.STEGO.BABY.NAME, CREATURES.SPECIES.STEGO.BABY.DESC, "baby_stego_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "Stego");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
