using STRINGS;
using TUNING;
using UnityEngine;

public class IceCavesForagePlantConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "IceCavesForagePlant";

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
		GameObject template = EntityTemplates.CreateLooseEntity("IceCavesForagePlant", STRINGS.ITEMS.FOOD.ICECAVESFORAGEPLANT.NAME, STRINGS.ITEMS.FOOD.ICECAVESFORAGEPLANT.DESC, 1f, unitMass: false, Assets.GetAnim("frozenberries_fruit_kanim"), "object", Grid.SceneLayer.BuildingBack, EntityTemplates.CollisionShape.CIRCLE, 0.3f, 0.3f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.ICECAVESFORAGEPLANT);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
