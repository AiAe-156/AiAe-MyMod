using STRINGS;
using UnityEngine;

[EntityConfigOrder(4)]
public class BabyStaterpillarGasConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "StaterpillarGasBaby";

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
		GameObject gameObject = StaterpillarGasConfig.CreateStaterpillarGas("StaterpillarGasBaby", CREATURES.SPECIES.STATERPILLAR.VARIANT_GAS.BABY.NAME, CREATURES.SPECIES.STATERPILLAR.VARIANT_GAS.BABY.DESC, "baby_caterpillar_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "StaterpillarGas");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
		prefab.GetComponent<KBatchedAnimController>().SetSymbolVisiblity("electric_bolt_c_bloom", is_visible: false);
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
