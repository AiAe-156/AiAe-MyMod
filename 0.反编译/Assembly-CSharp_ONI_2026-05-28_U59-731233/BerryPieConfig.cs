using STRINGS;
using TUNING;
using UnityEngine;

public class BerryPieConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "BerryPie";

	public static ComplexRecipe recipe;

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
		GameObject template = EntityTemplates.CreateLooseEntity("BerryPie", STRINGS.ITEMS.FOOD.BERRYPIE.NAME, STRINGS.ITEMS.FOOD.BERRYPIE.DESC, 1f, unitMass: false, Assets.GetAnim("wormwood_berry_pie_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.55f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.BERRY_PIE);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
