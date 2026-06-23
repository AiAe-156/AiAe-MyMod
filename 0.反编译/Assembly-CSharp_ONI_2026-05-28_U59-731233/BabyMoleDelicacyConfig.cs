using STRINGS;
using UnityEngine;

[EntityConfigOrder(4)]
public class BabyMoleDelicacyConfig : IEntityConfig
{
	public const string ID = "MoleDelicacyBaby";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = MoleDelicacyConfig.CreateMole("MoleDelicacyBaby", CREATURES.SPECIES.MOLE.VARIANT_DELICACY.BABY.NAME, CREATURES.SPECIES.MOLE.VARIANT_DELICACY.BABY.DESC, "baby_driller_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "MoleDelicacy");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
		MoleConfig.SetSpawnNavType(inst);
	}
}
