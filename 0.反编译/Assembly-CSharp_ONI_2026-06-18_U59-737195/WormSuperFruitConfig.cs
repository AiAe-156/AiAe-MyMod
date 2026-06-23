using STRINGS;
using TUNING;
using UnityEngine;

public class WormSuperFruitConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "WormSuperFruit";

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("WormSuperFruit", STRINGS.ITEMS.FOOD.WORMSUPERFRUIT.NAME, STRINGS.ITEMS.FOOD.WORMSUPERFRUIT.DESC, 1f, unitMass: false, Assets.GetAnim("wormwood_super_fruits_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.6f, 0.4f, isPickupable: true), FOOD.FOOD_TYPES.WORMSUPERFRUIT);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
