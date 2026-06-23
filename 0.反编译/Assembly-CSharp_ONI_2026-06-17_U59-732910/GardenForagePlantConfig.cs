using STRINGS;
using TUNING;
using UnityEngine;

public class GardenForagePlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "GardenForagePlant";

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("GardenForagePlant", STRINGS.ITEMS.FOOD.GARDENFORAGEPLANT.NAME, STRINGS.ITEMS.FOOD.GARDENFORAGEPLANT.DESC, 1f, unitMass: false, Assets.GetAnim("fatplantfood_kanim"), "object", Grid.SceneLayer.BuildingBack, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f, isPickupable: true), FOOD.FOOD_TYPES.GARDENFORAGEPLANT);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
