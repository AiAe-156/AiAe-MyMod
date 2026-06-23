using STRINGS;
using TUNING;
using UnityEngine;

public class EdamameConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Edamame";

	public const string ANIM = "edamame_kanim";

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("Edamame", STRINGS.ITEMS.FOOD.EDAMAME.NAME, STRINGS.ITEMS.FOOD.EDAMAME.DESC, 1f, unitMass: false, Assets.GetAnim("edamame_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.9f, 0.6f, isPickupable: true), FOOD.FOOD_TYPES.EDAMAME);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
