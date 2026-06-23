using STRINGS;
using TUNING;
using UnityEngine;

public class DeepFriedMeatConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "DeepFriedMeat";

	public static ComplexRecipe recipe;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC2;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject template = EntityTemplates.CreateLooseEntity("DeepFriedMeat", STRINGS.ITEMS.FOOD.DEEPFRIEDMEAT.NAME, STRINGS.ITEMS.FOOD.DEEPFRIEDMEAT.DESC, 1f, unitMass: false, Assets.GetAnim("deepfried_meat_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.DEEP_FRIED_MEAT);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
