using STRINGS;
using TUNING;
using UnityEngine;

public class FernFoodConfig : IEntityConfig, IHasDlcRestrictions
{
	public static string ID = "FernFood";

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity(ID, STRINGS.ITEMS.FOOD.FERNFOOD.NAME, STRINGS.ITEMS.FOOD.FERNFOOD.DESC, 1f, unitMass: true, Assets.GetAnim("megafrond_grain_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true), FOOD.FOOD_TYPES.FERNFOOD);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
