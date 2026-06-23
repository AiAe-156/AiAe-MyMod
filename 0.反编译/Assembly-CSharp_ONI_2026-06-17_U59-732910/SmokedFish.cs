using STRINGS;
using TUNING;
using UnityEngine;

public class SmokedFish : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SmokedFish";

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("SmokedFish", STRINGS.ITEMS.FOOD.SMOKEDFISH.NAME, STRINGS.ITEMS.FOOD.SMOKEDFISH.DESC, 1f, unitMass: false, Assets.GetAnim("smokedfish_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true), FOOD.FOOD_TYPES.SMOKED_FISH);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
