using STRINGS;
using TUNING;
using UnityEngine;

public class VineFruitConfig : IEntityConfig, IHasDlcRestrictions
{
	public static string ID = "VineFruit";

	public const float KCalPerUnit = 325000f;

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
		GameObject gameObject = EntityTemplates.CreateLooseEntity(ID, STRINGS.ITEMS.FOOD.VINEFRUIT.NAME, STRINGS.ITEMS.FOOD.VINEFRUIT.DESC, 1f, unitMass: false, Assets.GetAnim("ova_melon_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		EntityTemplates.ExtendEntityToFood(gameObject, FOOD.FOOD_TYPES.VINEFRUIT);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
