using STRINGS;
using TUNING;
using UnityEngine;

public class SwampForagePlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SwampForagePlant";

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("SwampForagePlant", STRINGS.ITEMS.FOOD.SWAMPFORAGEPLANT.NAME, STRINGS.ITEMS.FOOD.SWAMPFORAGEPLANT.DESC, 1f, unitMass: false, Assets.GetAnim("swamptuber_vegetable_kanim"), "object", Grid.SceneLayer.BuildingBack, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f, isPickupable: true), FOOD.FOOD_TYPES.SWAMPFORAGEPLANT);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
