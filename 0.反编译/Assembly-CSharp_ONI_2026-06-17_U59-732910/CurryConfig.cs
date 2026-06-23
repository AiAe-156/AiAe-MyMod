using STRINGS;
using TUNING;
using UnityEngine;

public class CurryConfig : IEntityConfig
{
	public const string ID = "Curry";

	public GameObject CreatePrefab()
	{
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("Curry", STRINGS.ITEMS.FOOD.CURRY.NAME, STRINGS.ITEMS.FOOD.CURRY.DESC, 1f, unitMass: false, Assets.GetAnim("curried_beans_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.9f, 0.5f, isPickupable: true), FOOD.FOOD_TYPES.CURRY);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
