using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(4)]
public class BabyGoldBellyConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "GoldBellyBaby";

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
		GameObject gameObject = GoldBellyConfig.CreateGoldBelly("GoldBellyBaby", CREATURES.SPECIES.ICEBELLY.VARIANT_GOLD.BABY.NAME, CREATURES.SPECIES.ICEBELLY.VARIANT_GOLD.BABY.DESC, "baby_icebelly_kanim", is_baby: true);
		GameObject go = EntityTemplates.ExtendEntityToBeingABaby(gameObject, "GoldBelly");
		go.AddOrGetDef<BabyMonitor.Def>().configureAdultOnMaturation = delegate(GameObject go2)
		{
			AmountInstance amountInstance = Db.Get().Amounts.ScaleGrowth.Lookup(go2);
			amountInstance.value = amountInstance.GetMax() * GoldBellyConfig.SCALE_INITIAL_GROWTH_PCT;
		};
		return gameObject;
	}

	public void OnPrefabInit(GameObject prefab)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
