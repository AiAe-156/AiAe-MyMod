using STRINGS;
using TUNING;
using UnityEngine;

public class DinosaurMeatConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "DinosaurMeat";

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
		GameObject gameObject = EntityTemplates.CreateLooseEntity("DinosaurMeat", STRINGS.ITEMS.FOOD.DINOSAURMEAT.NAME, STRINGS.ITEMS.FOOD.DINOSAURMEAT.DESC, 1f, unitMass: false, Assets.GetAnim("dinomeat_raw_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.6f, isPickupable: true);
		EntityTemplates.ExtendEntityToFood(gameObject, FOOD.FOOD_TYPES.DINOSAURMEAT);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
