using STRINGS;
using TUNING;
using UnityEngine;

public class SquidMeatConfig : IEntityConfig, IHasDlcRestrictions
{
	public const string ID = "SquidMeat";

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
		GameObject gameObject = EntityTemplates.CreateLooseEntity("SquidMeat", STRINGS.ITEMS.FOOD.SQUIDMEAT.NAME, STRINGS.ITEMS.FOOD.SQUIDMEAT.DESC, 1f, unitMass: false, Assets.GetAnim("squid_meat_kanim"), "object", Grid.SceneLayer.Front, EntityTemplates.CollisionShape.RECTANGLE, 0.9f, 0.8f, isPickupable: true);
		gameObject.GetComponent<KBoxCollider2D>().offset = new Vector2(0f, 0.1f);
		EntityTemplates.ExtendEntityToFood(gameObject, FOOD.FOOD_TYPES.SQUID_MEAT);
		return gameObject;
	}

	public void OnPrefabInit(GameObject inst)
	{
	}

	public void OnSpawn(GameObject inst)
	{
	}
}
