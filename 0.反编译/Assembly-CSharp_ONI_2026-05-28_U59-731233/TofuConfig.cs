using STRINGS;
using TUNING;
using UnityEngine;

public class TofuConfig : IEntityConfig
{
	public const string ID = "Tofu";

	public const string ANIM = "loafu_kanim";

	public static ComplexRecipe recipe;

	public GameObject CreatePrefab()
	{
		GameObject template = EntityTemplates.CreateLooseEntity("Tofu", STRINGS.ITEMS.FOOD.TOFU.NAME, STRINGS.ITEMS.FOOD.TOFU.DESC, 1f, unitMass: false, Assets.GetAnim("loafu_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.9f, 0.6f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.TOFU);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
