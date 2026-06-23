using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabyWoodDeerConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "WoodDeerBaby";

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
		GameObject gameObject = WoodDeerConfig.CreateWoodDeer("WoodDeerBaby", CREATURES.SPECIES.WOODDEER.BABY.NAME, CREATURES.SPECIES.WOODDEER.BABY.DESC, "baby_ice_floof_kanim", is_baby: true);
		GameObject go = EntityTemplates.ExtendEntityToBeingABaby(gameObject, "WoodDeer");
		go.AddOrGetDef<BabyMonitor.Def>().configureAdultOnMaturation = delegate(GameObject go2)
		{
			AmountInstance amountInstance = Db.Get().Amounts.ScaleGrowth.Lookup(go2);
			amountInstance.value = amountInstance.GetMax() * WoodDeerConfig.ANTLER_STARTING_GROWTH_PCT;
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
