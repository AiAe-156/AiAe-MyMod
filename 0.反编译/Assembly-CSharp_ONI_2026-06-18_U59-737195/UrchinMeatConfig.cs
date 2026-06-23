using STRINGS;
using TUNING;
using UnityEngine;

public class UrchinMeatConfig : IEntityConfig, IHasDlcRestrictions
{
	public static string ID = "UrchinMeat";

	public static ComplexRecipe recipe;

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public GameObject CreatePrefab()
	{
		GameObject gameObject = EntityTemplates.CreateLooseEntity(ID, STRINGS.ITEMS.FOOD.URCHINMEAT.NAME, STRINGS.ITEMS.FOOD.URCHINMEAT.DESC, 1f, unitMass: false, Assets.GetAnim("ooohni_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		EntityTemplates.ExtendEntityToFood(gameObject, FOOD.FOOD_TYPES.URCHINMEAT);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
