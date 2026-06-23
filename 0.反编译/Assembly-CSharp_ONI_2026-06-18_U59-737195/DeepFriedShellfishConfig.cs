using STRINGS;
using TUNING;
using UnityEngine;

public class DeepFriedShellfishConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "DeepFriedShellfish";

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
		return EntityTemplates.ExtendEntityToFood(EntityTemplates.CreateLooseEntity("DeepFriedShellfish", STRINGS.ITEMS.FOOD.DEEPFRIEDSHELLFISH.NAME, STRINGS.ITEMS.FOOD.DEEPFRIEDSHELLFISH.DESC, 1f, unitMass: false, Assets.GetAnim("deepfried_shellfish_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.8f, 0.4f, isPickupable: true), FOOD.FOOD_TYPES.DEEP_FRIED_SHELLFISH);
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
