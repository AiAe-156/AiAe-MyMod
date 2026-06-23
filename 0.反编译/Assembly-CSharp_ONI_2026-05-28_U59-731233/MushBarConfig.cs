using STRINGS;
using TUNING;
using UnityEngine;

public class MushBarConfig : IEntityConfig
{
	public const string ID = "MushBar";

	public const string ANIM = "mushbar_kanim";

	public static ComplexRecipe recipe;

	public GameObject CreatePrefab()
	{
		GameObject template = EntityTemplates.CreateLooseEntity("MushBar", STRINGS.ITEMS.FOOD.MUSHBAR.NAME, STRINGS.ITEMS.FOOD.MUSHBAR.DESC, 1f, unitMass: false, Assets.GetAnim("mushbar_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.MUSHBAR);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
