using STRINGS;
using TUNING;
using UnityEngine;

public class PemmicanConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "Pemmican";

	public const string ANIM = "pemmican_kanim";

	public static ComplexRecipe recipe;

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
		GameObject template = EntityTemplates.CreateLooseEntity("Pemmican", STRINGS.ITEMS.FOOD.PEMMICAN.NAME, STRINGS.ITEMS.FOOD.PEMMICAN.DESC, 1f, unitMass: false, Assets.GetAnim("pemmican_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true);
		return EntityTemplates.ExtendEntityToFood(template, FOOD.FOOD_TYPES.PEMMICAN);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
