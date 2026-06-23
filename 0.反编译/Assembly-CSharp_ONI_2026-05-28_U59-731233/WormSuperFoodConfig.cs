using STRINGS;
using TUNING;
using UnityEngine;

public class WormSuperFoodConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "WormSuperFood";

	public static ComplexRecipe recipe;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.EXPANSION1;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject template = EntityTemplates.CreateLooseEntity("WormSuperFood", STRINGS.ITEMS.FOOD.WORMSUPERFOOD.NAME, STRINGS.ITEMS.FOOD.WORMSUPERFOOD.DESC, 1f, unitMass: false, Assets.GetAnim("wormwood_preserved_berries_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.7f, 0.6f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.WORMSUPERFOOD);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
