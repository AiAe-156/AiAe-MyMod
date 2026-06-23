using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabyPufferFishConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "PufferFishBaby";

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = PufferFishConfig.CreatePufferFish("PufferFishBaby", CREATURES.SPECIES.PUFFERFISH.BABY.NAME, CREATURES.SPECIES.PUFFERFISH.BABY.DESC, "baby_blowfish_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "PufferFish");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
