using STRINGS;
using TUNING;
using UnityEngine;

public class FriedMushBarConfig : IEntityConfig
{
	public const string ID = "FriedMushBar";

	public static ComplexRecipe recipe;

	public GameObject CreatePrefab()
	{
		GameObject template = EntityTemplates.CreateLooseEntity("FriedMushBar", STRINGS.ITEMS.FOOD.FRIEDMUSHBAR.NAME, STRINGS.ITEMS.FOOD.FRIEDMUSHBAR.DESC, 1f, unitMass: false, Assets.GetAnim("mushbarfried_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.FRIEDMUSHBAR);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
