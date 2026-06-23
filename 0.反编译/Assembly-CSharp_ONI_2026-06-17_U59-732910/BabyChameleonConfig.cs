using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabyChameleonConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "ChameleonBaby";

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
		GameObject gameObject = ChameleonConfig.CreateChameleon("ChameleonBaby", CREATURES.SPECIES.CHAMELEON.BABY.NAME, CREATURES.SPECIES.CHAMELEON.BABY.DESC, "baby_chameleo_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "Chameleon");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
