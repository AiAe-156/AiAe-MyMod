using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabyPrehistoricPacuConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "PrehistoricPacuBaby";

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
		GameObject gameObject = PrehistoricPacuConfig.CreatePrehistoricPacu("PrehistoricPacuBaby", CREATURES.SPECIES.PREHISTORICPACU.BABY.NAME, CREATURES.SPECIES.PREHISTORICPACU.BABY.DESC, "baby_paculacanth_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "PrehistoricPacu");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
