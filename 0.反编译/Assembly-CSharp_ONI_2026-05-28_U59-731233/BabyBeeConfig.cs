using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabyBeeConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "BeeBaby";

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = BeeConfig.CreateBee("BeeBaby", CREATURES.SPECIES.BEE.BABY.NAME, CREATURES.SPECIES.BEE.BABY.DESC, "baby_blarva_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "Bee", null, force_adult_nav_type: true, 2f);
		KPrefabID component = gameObject.GetComponent<KPrefabID>();
		component.AddTag(GameTags.Creatures.Walker);
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
		BaseBeeConfig.SetupLoopingSounds(inst);
	}
}
