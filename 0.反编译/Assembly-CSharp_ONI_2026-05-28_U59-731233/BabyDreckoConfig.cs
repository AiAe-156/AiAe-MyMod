using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabyDreckoConfig : IEntityConfig
{
	public const string ID = "DreckoBaby";

	public GameObject CreatePrefab()
	{
		GameObject gameObject = DreckoConfig.CreateDrecko("DreckoBaby", CREATURES.SPECIES.DRECKO.BABY.NAME, CREATURES.SPECIES.DRECKO.BABY.DESC, "baby_drecko_kanim", is_baby: true);
		GameObject go = EntityTemplates.ExtendEntityToBeingABaby(gameObject, "Drecko");
		go.AddOrGetDef<BabyMonitor.Def>().configureAdultOnMaturation = delegate(GameObject go2)
		{
			AmountInstance amountInstance = Db.Get().Amounts.ScaleGrowth.Lookup(go2);
			amountInstance.value = amountInstance.GetMax() * DreckoConfig.SCALE_INITIAL_GROWTH_PCT;
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
