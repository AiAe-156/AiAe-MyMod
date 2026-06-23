using STRINGS;
using TUNING;
using UnityEngine;

public class SwampFruitConfig : IEntityConfig, IHasDlcRestrictions
{
	public static string ID = "SwampFruit";

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity(ID, STRINGS.ITEMS.FOOD.SWAMPFRUIT.NAME, STRINGS.ITEMS.FOOD.SWAMPFRUIT.DESC, 1f, unitMass: false, Assets.GetAnim("swampcrop_fruit_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 1f, 0.72f, isPickupable: true), FOOD.FOOD_TYPES.SWAMPFRUIT);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
