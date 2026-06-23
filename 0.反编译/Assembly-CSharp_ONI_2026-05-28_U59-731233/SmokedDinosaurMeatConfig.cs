using STRINGS;
using TUNING;
using UnityEngine;

public class SmokedDinosaurMeatConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SmokedDinosaurMeat";

	public static ComplexRecipe recipe;

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
		GameObject template = EntityTemplates.CreateLooseEntity("SmokedDinosaurMeat", STRINGS.ITEMS.FOOD.SMOKEDDINOSAURMEAT.NAME, STRINGS.ITEMS.FOOD.SMOKEDDINOSAURMEAT.DESC, 1f, unitMass: false, Assets.GetAnim("dinobrisket_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.SMOKED_DINOSAURMEAT);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
