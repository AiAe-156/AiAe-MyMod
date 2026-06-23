using STRINGS;
using TUNING;
using UnityEngine;

public class WormBasicFruitConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "WormBasicFruit";

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("WormBasicFruit", STRINGS.ITEMS.FOOD.WORMBASICFRUIT.NAME, STRINGS.ITEMS.FOOD.WORMBASICFRUIT.DESC, 1f, unitMass: false, Assets.GetAnim("wormwood_basic_fruit_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.7f, 0.4f, isPickupable: true), FOOD.FOOD_TYPES.WORMBASICFRUIT);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
