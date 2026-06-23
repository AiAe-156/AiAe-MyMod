using STRINGS;
using TUNING;
using UnityEngine;

public class NigiriConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Nigiri";

	public const string ANIM = "nigiri_kanim";

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
		GameObject template = EntityTemplates.CreateLooseEntity("Nigiri", STRINGS.ITEMS.FOOD.NIGIRI.NAME, STRINGS.ITEMS.FOOD.NIGIRI.DESC, 1f, unitMass: false, Assets.GetAnim("nigiri_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.9f, 0.8f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.NIGIRI);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
