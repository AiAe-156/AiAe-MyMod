using STRINGS;
using TUNING;
using UnityEngine;

public class DeepFriedFishConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "DeepFriedFish";

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("DeepFriedFish", STRINGS.ITEMS.FOOD.DEEPFRIEDFISH.NAME, STRINGS.ITEMS.FOOD.DEEPFRIEDFISH.DESC, 1f, unitMass: false, Assets.GetAnim("deepfried_fish_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true), FOOD.FOOD_TYPES.DEEP_FRIED_FISH);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
