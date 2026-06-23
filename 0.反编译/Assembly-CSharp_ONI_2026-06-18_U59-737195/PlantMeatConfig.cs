using STRINGS;
using TUNING;
using UnityEngine;

public class PlantMeatConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "PlantMeat";

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
		GameObject gameObject = EntityTemplates.CreateLooseEntity("PlantMeat", STRINGS.ITEMS.FOOD.PLANTMEAT.NAME, STRINGS.ITEMS.FOOD.PLANTMEAT.DESC, 1f, unitMass: false, Assets.GetAnim("critter_trap_fruit_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		EntityTemplates.ExtendEntityToFood(gameObject, FOOD.FOOD_TYPES.PLANTMEAT);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
