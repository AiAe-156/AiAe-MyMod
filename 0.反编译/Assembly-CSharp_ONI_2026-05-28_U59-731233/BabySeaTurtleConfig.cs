using Klei.AI;
using STRINGS;
using UnityEngine;

[EntityConfigOrder(3)]
public class BabySeaTurtleConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SeaTurtleBaby";

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
		GameObject gameObject = SeaTurtleConfig.CreateSeaTurtle("SeaTurtleBaby", CREATURES.SPECIES.SEATURTLE.BABY.NAME, CREATURES.SPECIES.SEATURTLE.BABY.DESC, "baby_turtle_kanim", is_baby: true);
		GameObject go = EntityTemplates.ExtendEntityToBeingABaby(gameObject, "SeaTurtle");
		go.AddOrGetDef<BabyMonitor.Def>().configureAdultOnMaturation = delegate(GameObject go2)
		{
			AmountInstance amountInstance = Db.Get().Amounts.ScaleGrowth.Lookup(go2);
			amountInstance.value = amountInstance.GetMax() * SeaTurtleTuning.SCALE_INITIAL_GROWTH_PCT;
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
