using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class MosquitoLarvaConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "MosquitoBaby";

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
		GameObject gameObject = MosquitoConfig.CreateMosquito("MosquitoBaby", CREATURES.SPECIES.MOSQUITO.BABY.NAME, CREATURES.SPECIES.MOSQUITO.BABY.DESC, "baby_mosquito_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "Mosquito");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
