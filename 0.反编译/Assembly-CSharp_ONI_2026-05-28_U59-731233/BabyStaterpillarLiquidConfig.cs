using STRINGS;
using UnityEngine;

[EntityConfigOrder(4)]
public class BabyStaterpillarLiquidConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "StaterpillarLiquidBaby";

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
		GameObject gameObject = StaterpillarLiquidConfig.CreateStaterpillarLiquid("StaterpillarLiquidBaby", CREATURES.SPECIES.STATERPILLAR.VARIANT_LIQUID.BABY.NAME, CREATURES.SPECIES.STATERPILLAR.VARIANT_LIQUID.BABY.DESC, "baby_caterpillar_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "StaterpillarLiquid");
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
		KBatchedAnimController component = prefab.GetComponent<KBatchedAnimController>();
		component.SetSymbolVisiblity("electric_bolt_c_bloom", is_visible: false);
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
