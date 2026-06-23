using STRINGS;
using TUNING;
using UnityEngine;

public class ButterflyFoodConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "ButterflyFood";

	public static ComplexRecipe recipe;

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("ButterflyFood", STRINGS.ITEMS.FOOD.BUTTERFLYFOOD.NAME, STRINGS.ITEMS.FOOD.BUTTERFLYFOOD.DESC, 1f, unitMass: false, Assets.GetAnim("fried_mimillet_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.85f, 0.75f, isPickupable: true), FOOD.FOOD_TYPES.BUTTERFLYFOOD);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
