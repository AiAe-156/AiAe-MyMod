using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabySealConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SealBaby";

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC2;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = SealConfig.CreateSeal("SealBaby", CREATURES.SPECIES.SEAL.BABY.NAME, CREATURES.SPECIES.SEAL.BABY.DESC, "baby_seal_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "Seal");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
