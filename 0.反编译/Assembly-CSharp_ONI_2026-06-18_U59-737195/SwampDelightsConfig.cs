using STRINGS;
using TUNING;
using UnityEngine;

public class SwampDelightsConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SwampDelights";

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("SwampDelights", STRINGS.ITEMS.FOOD.SWAMPDELIGHTS.NAME, STRINGS.ITEMS.FOOD.SWAMPDELIGHTS.DESC, 1f, unitMass: false, Assets.GetAnim("swamp_delights_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.7f, isPickupable: true), FOOD.FOOD_TYPES.SWAMP_DELIGHTS);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
