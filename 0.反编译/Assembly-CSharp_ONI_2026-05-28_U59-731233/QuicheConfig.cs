using STRINGS;
using TUNING;
using UnityEngine;

public class QuicheConfig : IEntityConfig
{
	public const string ID = "Quiche";

	public static ComplexRecipe recipe;

	public GameObject CreatePrefab()
	{
		GameObject template = EntityTemplates.CreateLooseEntity("Quiche", STRINGS.ITEMS.FOOD.QUICHE.NAME, STRINGS.ITEMS.FOOD.QUICHE.DESC, 1f, unitMass: false, Assets.GetAnim("quiche_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.QUICHE);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
