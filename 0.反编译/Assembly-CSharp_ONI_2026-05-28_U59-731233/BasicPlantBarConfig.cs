using STRINGS;
using TUNING;
using UnityEngine;

public class BasicPlantBarConfig : IEntityConfig
{
	public const string ID = "BasicPlantBar";

	public const string ANIM = "liceloaf_kanim";

	public static ComplexRecipe recipe;

	public GameObject CreatePrefab()
	{
		GameObject template = EntityTemplates.CreateLooseEntity("BasicPlantBar", STRINGS.ITEMS.FOOD.BASICPLANTBAR.NAME, STRINGS.ITEMS.FOOD.BASICPLANTBAR.DESC, 1f, unitMass: false, Assets.GetAnim("liceloaf_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.BASICPLANTBAR);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
