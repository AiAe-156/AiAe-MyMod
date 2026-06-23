using STRINGS;
using TUNING;
using UnityEngine;

public class MusselTongueConfig : IEntityConfig, IHasDlcRestrictions
{
	public static string ID = "MusselTongue";

	public GameObject CreatePrefab()
	{
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity(ID, STRINGS.ITEMS.FOOD.MUSSELTONGUE.NAME, STRINGS.ITEMS.FOOD.MUSSELTONGUE.DESC, 1f, unitMass: false, Assets.GetAnim("musseltongue_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.3f, isPickupable: true), FOOD.FOOD_TYPES.MUSSELTONGUE);
	}

	public string[] GetForbiddenDlcIds()
	{
		return null;
	}

	public string[] GetRequiredDlcIds()
	{
		return DlcManager.DLC5;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
