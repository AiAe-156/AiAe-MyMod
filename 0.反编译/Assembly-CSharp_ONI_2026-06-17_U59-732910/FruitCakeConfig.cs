using STRINGS;
using TUNING;
using UnityEngine;

public class FruitCakeConfig : IEntityConfig
{
	public const string ID = "FruitCake";

	public const string ANIM = "fruitcake_kanim";

	public static ComplexRecipe recipe;

	public GameObject CreatePrefab()
	{
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("FruitCake", STRINGS.ITEMS.FOOD.FRUITCAKE.NAME, STRINGS.ITEMS.FOOD.FRUITCAKE.DESC, 1f, unitMass: false, Assets.GetAnim("fruitcake_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true), FOOD.FOOD_TYPES.FRUITCAKE);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
