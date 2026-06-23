using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(4)]
public class BabyGlassDeerConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "GlassDeerBaby";

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
		GameObject gameObject = GlassDeerConfig.CreateGlassDeer("GlassDeerBaby", CREATURES.SPECIES.GLASSDEER.BABY.NAME, CREATURES.SPECIES.GLASSDEER.BABY.DESC, "baby_ice_floof_kanim", is_baby: true);
		EntityTemplates.ExtendEntityToBeingABaby(gameObject, "GlassDeer").AddOrGetDef<BabyMonitor.Def>().configureAdultOnMaturation = delegate(GameObject go)
		{
			AmountInstance amountInstance = Db.Get().Amounts.ScaleGrowth.Lookup(go);
			amountInstance.value = amountInstance.GetMax() * 0.5f;
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
